﻿
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
    private Dictionary<uint, Entity> m_entities = new Dictionary<uint, Entity>();
    private Dictionary<int, uint> m_clientToEntityId = new Dictionary<int, uint>();
    private Dictionary<uint, Entity> m_food = new Dictionary<uint, Entity>();
    private float timer = 100;

    Systems.Network m_systemNetwork = new Server.Systems.Network();
    Systems.CollideSystem collideSystem = new CollideSystem();

    /// <summary>
    /// This is where the server-side simulation takes place.  Messages
    /// from the network are processed and then any necessary client
    /// updates are sent out.
    /// </summary>
    public void update(TimeSpan elapsedTime)
    {
      m_systemNetwork.update(elapsedTime, MessageQueueServer.instance.getMessages());
      timer -= elapsedTime.Milliseconds;
      if (m_food.Count < 2500 && timer < 0)
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
      m_systemNetwork.registerJoinHandler(handleJoin);
      m_systemNetwork.registerDisconnectHandler(handleDisconnect);
      m_systemNetwork.registerRemoveHandler(handleRemove);
      collideSystem.registerJoinHandler(handleJoin);
      collideSystem.registerDisconnectHandler(handleDisconnect);
      collideSystem.registerRemoveHandler(handleRemove);

      MessageQueueServer.instance.registerConnectHandler(handleConnect);

      for (int i = 0; i < 1000; i++)
      {
        Entity food = E_Food.create("Textures/food", 25.0f, 1);
        m_food.Add(food.id, food);
        addEntity(food);
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
    private void handleDisconnect(int clientId)
    {
      m_clients.Remove(clientId);

      Message message = new Shared.Messages.RemoveEntity(m_clientToEntityId[clientId]);
      MessageQueueServer.instance.broadcastMessage(message);

      removeEntity(m_clientToEntityId[clientId]);

      m_clientToEntityId.Remove(clientId);
    }

    private void handleRemove(uint entityId)
    {
      Message message = new Shared.Messages.RemoveEntity(entityId);
      MessageQueueServer.instance.broadcastMessage(message);

      removeEntity(entityId);
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
      collideSystem.add(entity);
    }

    /// <summary>
    /// All entity lists for the systems must be given a chance to remove
    /// the entity.
    /// </summary>
    private void removeEntity(uint id)
    {
      m_entities.Remove(id);
      m_systemNetwork.remove(id);
      collideSystem.remove(id);
      m_food.Remove(id);
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
    /// Handler for the Join message.  It gets a player entity created,
    /// added to the server game model, and notifies the requesting client
    /// of the player.
    /// </summary>
    private void handleJoin(int clientId)
    {
      // Step 1: Tell the newly connected player about all other entities
      reportAllEntities(clientId);

      // Step 2: Create an entity for the newly joined player and sent it
      //         to the newly joined client
      Entity player = Shared.Entities.E_Player.create("Textures/playerShip1_blue", 50, 0.1f, (float)Math.PI / 1000);
      addEntity(player);
      m_clientToEntityId[clientId] = player.id;

      // Remove Server only Components
      player.remove<Collision>();
      player.remove<Head>();

      // Step 3: Send the new player entity to the newly joined client
      MessageQueueServer.instance.sendMessage(clientId, new NewEntity(player));


      // Step 4: Let all other clients know about this new player entity

      // We change the appearance for a player ship entity for all other clients to a different texture
      player.remove<Appearance>();
      player.add(new Appearance("Textures/playerShip1_red"));

      // Remove components not needed for "other" players
      player.remove<Shared.Components.Input>();

      Message message = new NewEntity(player);
      foreach (int otherId in m_clients)
      {
        if (otherId != clientId)
        {
          MessageQueueServer.instance.sendMessage(otherId, message);
        }
      }
    }
  }
}
