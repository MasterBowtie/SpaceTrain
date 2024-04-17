
using System.Reflection.Metadata.Ecma335;
using Microsoft.Xna.Framework;
using Server.Systems;
using Shared.Components;
using Shared.Entities;
using Shared.Messages;

namespace Server
{
  public class GameModel
  {
    private HashSet<int> m_clients = new HashSet<int>();
    private int nextPlayerId = 0;
    //Make greater than tile count on client
    private uint entityCounter = 2500;
    private Dictionary<uint, Entity> m_entities = new Dictionary<uint, Entity>();
    private Dictionary<int, uint> m_clientToEntityId = new Dictionary<int, uint>();
    private Dictionary<int, int> m_scores = new Dictionary<int, int>();
    private Dictionary<uint, Entity> m_food = new Dictionary<uint, Entity>();
    private float timer = 100;
    private float moveRate = 0.15f;

    Systems.Network m_systemNetwork = new Server.Systems.Network();
    Systems.CollideSystem m_collideSystem = new CollideSystem();
    Shared.Systems.Movement m_moveSystem = new Shared.Systems.Movement();

    /// <summary>
    /// This is where the server-side simulation takes place.  Messages
    /// from the network are processed and then any necessary client
    /// updates are sent out.
    /// </summary>
    public void update(TimeSpan elapsedTime)
    {
      m_systemNetwork.update(elapsedTime, MessageQueueServer.instance.getMessages());
      m_collideSystem.update(elapsedTime);
      m_moveSystem.update(elapsedTime);

      timer -= elapsedTime.Milliseconds;
      if (m_food.Count < 500 && timer < 0)
      {
        Entity food = E_Food.create("Textures/food", 25.0f, 1);
        m_food.Add(food.id, food);
        addEntity(food);
        Message message = new NewEntity(food);
        MessageQueueServer.instance.broadcastMessage(message);
        timer += 100;
      } 
    }

    /// <summary>
    /// Setup notifications for when new clients connect.
    /// </summary>
    public bool initialize()
    {
      m_systemNetwork.registerHandler(Shared.Messages.Type.Join, handleJoin);
      m_systemNetwork.registerHandler(Shared.Messages.Type.Disconnect, handleDisconnect);
      m_systemNetwork.registerHandler(Shared.Messages.Type.Leave, handleLeave);

      m_collideSystem.registerHandler(Shared.Messages.Type.Join, handleJoin);
      m_collideSystem.registerHandler(Shared.Messages.Type.Disconnect, handleDisconnect);
      m_collideSystem.registerHandler(Shared.Messages.Type.Leave, handleLeave);
      m_collideSystem.registerRemoveHandler(handleRemove);
      m_collideSystem.registerEatHandler(handleEat);


      MessageQueueServer.instance.registerConnectHandler(handleConnect);


      Entity food = E_Food.create("Textures/food", 25.0f, 1, entityCounter + 1);
      for (int i = 0; i < 100; i++)
      {
        m_food.Add(food.id, food);
        addEntity(food);
        food = E_Food.create("Textures/food", 25.0f, 1);
      }

      return true;
    }

    /// <summary>
    /// Give everything a chance to gracefully shutdown.
    /// </summary>
    public void shutdown()
    {

    }

    /// <summary>
    /// Upon connection of a new client, create a player entity and
    /// send that info back to the client, along with adding it to
    /// the server simulation.
    /// </summary>
    private void handleConnect(int clientId)
    {
      m_clients.Add(clientId);

      MessageQueueServer.instance.sendMessage(clientId, new Shared.Messages.ConnectAck());
    }

    /// <summary>
    /// When a client disconnects, need to tell all the other clients
    /// of the disconnect.
    /// </summary>
    /// <param name="clientId"></param>
    private void handleDisconnect(int clientId, TimeSpan elapsedTime, Shared.Messages.Message message)
    {
      m_clients.Remove(clientId);

      if (m_clientToEntityId.ContainsKey(clientId))
      {
        handleRemove(m_clientToEntityId[clientId]);
      }

      m_clientToEntityId.Remove(clientId);
    }

    // Sends out message to remove entity
    // Snake components are turned into food. Doesn't turn head into food, prevents inaccessible food at border.
    private void handleRemove(uint entityId)
    {
      if (m_entities.ContainsKey(entityId) && m_entities[entityId].contains<Connected>()) {
        List<Entity> all = new List<Entity>();
        Entity? next = m_entities[entityId].get<Connected>().leads;
        while (next != null)
        {
          all.Add(m_entities[next.id]);
          next = m_entities[next.id].get<Connected>().leads;
        }

        foreach (Entity entity in all)
        {
          Message childMessage = new Shared.Messages.RemoveEntity(entity.id);
          MessageQueueServer.instance.broadcastMessage(childMessage);

          removeEntity(entity.id);

          Entity food = E_Food.create("Textures/food", 25.0f, 1, entity.id, entity.get<Position>().position);
          childMessage = new Shared.Messages.NewEntity(food);
          MessageQueueServer.instance.broadcastMessage(childMessage);

          addEntity(food);
        }

      }
      Message message = new Shared.Messages.RemoveEntity(entityId);
      MessageQueueServer.instance.broadcastMessage(message);

      removeEntity(entityId);
      
    }

    private void handleEat(uint entityId)
    {
      Entity tail = insertSegment(entityId);
      Message message = new Shared.Messages.NewEntity(tail);
      MessageQueueServer.instance.broadcastMessage(message);
    }

    /// <summary>
    /// As entities are added to the game model, they are run by the systems
    /// to see if they are interested in knowing about them during their
    /// updates.
    /// </summary>
    private void addEntity(Entity entity)
    {
      if (entity == null)
      {
        return;
      }

      m_entities[entity.id] = entity;
      m_systemNetwork.add(entity);
      m_moveSystem.add(entity);
      m_collideSystem.add(entity);
    }

    /// <summary>
    /// All entity lists for the systems must be given a chance to remove
    /// the entity.
    /// </summary>
    private void removeEntity(uint id)
    {
      m_entities.Remove(id);
      m_systemNetwork.remove(id);
      m_moveSystem.remove(id);
      m_collideSystem.remove(id);
      m_food.Remove(id);
    }

    private Entity insertSegment(uint entityId)
    {
      Entity head = m_entities[entityId];
      Entity? part = head.get<Connected>().leads;
      while (part != null && part.get<Connected>().leads != null) 
      {
        part = part.get<Connected>().leads;
      }
      Entity tail = part;
      var connection = tail.get<Connected>();
      part = Segment.create(50, moveRate, tail.get<Position>(), tail);
      connection.leads = part;
      addEntity(part);
      return part;
    }


    /// <summary>
    /// For the indicated client, sends messages for all other entities
    /// currently in the game simulation.
    /// </summary>
    private void reportAllEntities(int clientId)
    {
      foreach (var item in m_entities)
      {
        MessageQueueServer.instance.sendMessage(clientId, new Shared.Messages.NewEntity(item.Value));
      }
    }

    /// <summary>
    /// </summary>
    private void handleLeave(int clientId, TimeSpan elapsedTime, Shared.Messages.Message message)
    {
      if (m_clientToEntityId.ContainsKey(clientId))
      {
        handleRemove(m_clientToEntityId[clientId]);
      }

      m_clientToEntityId.Remove(clientId);
    }


    /// <summary>
    /// Handler for the Join message.  It gets a player entity created,
    /// added to the server game model, and notifies the requesting client
    /// of the player.
    /// </summary>
    private void handleJoin(int clientId, TimeSpan elapsedTime, Shared.Messages.Message message)
    {
      // Step 1: Tell the newly connected player about all other entities
      reportAllEntities(clientId);

      // Step 2: Create an entity for the newly joined player and sent it
      //         to the newly joined client

      Entity player = Shared.Entities.Player.create(nextPlayerId, "Textures/playerShip1_blue", 50, moveRate, (float)Math.PI / 1000);
      Entity tail = Shared.Entities.Segment.create(50, moveRate, player.get<Position>(), player);
      player.add(new Connected(tail, null));
      addEntity(player);
      addEntity(tail);

      m_clientToEntityId[clientId] = player.id;

      // Step 3: Send the new player entity to the newly joined client
      MessageQueueServer.instance.sendMessage(clientId, new NewEntity(player));
      MessageQueueServer.instance.sendMessage(clientId, new NewEntity(tail));

      // Step 4: Let all other clients know about this new player entity

      // We change the appearance for a player ship entity for all other clients to a different texture
      player.remove<Appearance>();
      player.add(new Appearance("Textures/playerShip1_red"));

      // Remove components not needed for "other" players
      player.remove<Shared.Components.Input>();

      Message messageNewEntity = new NewEntity(player);
      foreach (int otherId in m_clients)
      {
        if (otherId != clientId)
        {
          MessageQueueServer.instance.sendMessage(otherId, messageNewEntity);
        }
      }
      messageNewEntity = new NewEntity(tail);
      foreach (int otherId in m_clients)
      {
        if (otherId != clientId)
        {
          MessageQueueServer.instance.sendMessage(otherId, messageNewEntity);
        }
      }
    }
  }
}
