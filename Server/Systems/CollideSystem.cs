using Shared.Components;
using Shared.Entities;
using Shared.Messages;
using static Server.Systems.CollideSystem;
using static Server.Systems.Network;

namespace Server.Systems
{
  internal class CollideSystem : Shared.Systems.System
  {

    public delegate void Handler(int clientId, TimeSpan elapsedTime, Shared.Messages.Message message);
    public delegate void JoinHandler(int clientId);
    public delegate void DisconnectHandler(int clientId);
    public delegate void RemoveHandler(uint entityId);

    private Dictionary<Shared.Messages.Type, Handler> m_commandMap = new Dictionary<Shared.Messages.Type, Handler>();
    private JoinHandler m_joinHandler;
    private DisconnectHandler m_disconnectHandler;
    private RemoveHandler m_removeHandler;

    private List<Entity> removeList = new List<Entity>();
    private List<Entity> killed = new List<Entity>();

    public CollideSystem() : base(
                typeof(Shared.Components.Collision)
                )
    {            
      // Register our own join handler
      registerHandler(Shared.Messages.Type.Join, (int clientId, TimeSpan elapsedTime, Shared.Messages.Message message) =>
      {
        if (m_joinHandler != null)
        {
          m_joinHandler(clientId);
        }
      });

      // Register our own disconnect handler
      registerHandler(Shared.Messages.Type.Disconnect, (int clientId, TimeSpan elapsedTime, Shared.Messages.Message message) =>
      {
        if (m_disconnectHandler != null)
        {
          m_disconnectHandler(clientId);
        }
      });
    }
    public override void update(TimeSpan elapsedTime)
    {
      List<Entity> entities1 = m_entities.Values.ToList();
      List<Entity> entities2 = m_entities.Values.ToList();

      for (int i = 0; i < entities1.Count; i++)
      {
        for (int j = 0; (j < entities2.Count); j++)
        {
          var pos1 = entities1[i].get<Position>().position;
          var size1 = entities1[i].get<Size>().size;
          var pos2 = entities2[j].get<Position>().position;
          var size2 = entities2[j].get<Size>().size;
          Entity entity1 = entities1[i];
          Entity entity2 = entities2[i];

          if (pos1.X > pos2.X && pos1.X < pos2.X + size2.X)
          {
            if (pos1.Y > pos2.Y && pos1.Y < pos2.Y + size2.Y)
            {
              if (entity1.contains<Head>() && entity2.contains<C_Food>())
              {
                removeList.Add(entity2);
              }
              else if (entity2.contains<Head>() && entity1.contains<C_Food>())
              {
                removeList.Add(entity1);
              }
              else if (entity1.contains<Head>() && entity2.contains<Head>())
              {
                killed.Add(entity1);
                killed.Add(entity2);
              } 
              else if (entity1.contains<Head>())
              {
                killed.Add(entity1);
              } else
              {
                killed.Add(entity2);
              }
            }
          }
        }
      } 
    }
    public void registerJoinHandler(JoinHandler handler)
    {
      m_joinHandler = handler;
    }

    public void registerDisconnectHandler(DisconnectHandler handler)
    {
      m_disconnectHandler = handler;
    }

    public void registerRemoveHandler(RemoveHandler handler)
    {
      m_removeHandler = handler;
    }

    private void registerHandler(Shared.Messages.Type type, Handler handler)
    {
      m_commandMap[type] = handler;
    }

    public void removeItems()
    {
      foreach (Entity entity in removeList)
      {
        m_removeHandler(entity.id);
      }
    }

    public void killEntity()
    {

    }
  }
}

