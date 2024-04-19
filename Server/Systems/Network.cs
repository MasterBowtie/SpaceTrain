using System.Runtime;
using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Entities;
using Shared.Messages;

namespace Server.Systems
{
  public class Network : Shared.Systems.System
  {
    public delegate void Handler(int clientId, TimeSpan elapsedTime, Shared.Messages.Message message);
    public delegate void InputHandler(Entity entity, Shared.Components.Input.Type type, TimeSpan elapsedTime);

    private Dictionary<Shared.Messages.Type, Handler> m_commandMap = new Dictionary<Shared.Messages.Type, Handler>();

    private HashSet<uint> m_reportThese = new HashSet<uint>();

    /// <summary>
    /// Primary activity in the constructor is to setup the command map
    /// that maps from message types to their handlers.
    /// </summary>
    public Network() :
        base(
            typeof(Shared.Components.Movement),
            typeof(Shared.Components.Position)
        )
    {

      // Register our own input handler
      registerHandler(Shared.Messages.Type.Input, (int clientId, TimeSpan elapsedTime, Shared.Messages.Message message) =>
      {
        handleInput((Shared.Messages.Input)message);
      });
    }

    // Have to implement this because it is abstract in the base class
    public override void update(TimeSpan elapsedTime) { }

    /// <summary>
    /// Have our own version of update, because we need a list of messages to work with, and
    /// messages aren't entities.
    /// </summary>
    public void update(TimeSpan elapsedTime, Queue<Tuple<int, Message>> messages)
    {
      if (messages != null)
      {
        while (messages.Count > 0)
        {
          var message = messages.Dequeue();
          if (m_commandMap.ContainsKey(message.Item2.type))
          {
            m_commandMap[message.Item2.type](message.Item1, elapsedTime, message.Item2);
          }
        }
      }

      // Send updated game state updates back out to connected clients
      updateInputs(elapsedTime);
    }

    public void registerHandler(Shared.Messages.Type type, Handler handler)
    {
      m_commandMap[type] = handler;
    }

    /// <summary>
    /// Handler for the Input message.  This simply passes the responsibility
    /// to the registered input handler.
    /// </summary>
    /// <param name="message"></param>
    private void handleInput(Shared.Messages.Input message)
    {
      if (m_entities.ContainsKey(message.entityId))
      {

      
      var entity = m_entities[message.entityId];

        // add all entities that move
        foreach (var input in message.inputs)
        {
          switch (input)
          {
            case Shared.Components.Input.Type.Up:
              Utility.up(entity, message.elapsedTime);
              m_reportThese.Add(message.entityId);
              break;
            case Shared.Components.Input.Type.Left:
              Utility.left(entity, message.elapsedTime);
              m_reportThese.Add(message.entityId);
              break;
            case Shared.Components.Input.Type.Right:
              Utility.right(entity, message.elapsedTime);
              m_reportThese.Add(message.entityId);
              break;
            case Shared.Components.Input.Type.Down:
              Utility.down(entity, message.elapsedTime);
              m_reportThese.Add(message.entityId);
              break;
            case Shared.Components.Input.Type.NE:
              Utility.ne(entity, message.elapsedTime);
              m_reportThese.Add(message.entityId);
              break;
            case Shared.Components.Input.Type.NW:
              Utility.nw(entity, message.elapsedTime);
              m_reportThese.Add(message.entityId);
              break;
            case Shared.Components.Input.Type.SE:
              Utility.se(entity, message.elapsedTime);
              m_reportThese.Add(message.entityId);
              break;
            case Shared.Components.Input.Type.SW:
              Utility.sw(entity, message.elapsedTime);
              m_reportThese.Add(message.entityId);
              break;
          }
        }
      }

    }

    /// <summary>
    /// For the entities that have updates, send those updates to all
    /// connected clients.
    /// </summary>
    private void updateInputs(TimeSpan elapsedTime)
    {
      foreach (var entityId in m_reportThese)
      {
        var entity = m_entities[entityId];
        var message = new UpdateEntity(entity, elapsedTime);
        MessageQueueServer.instance.broadcastMessageWithLastId(message);
      }

      m_reportThese.Clear();
    }


  }
}
