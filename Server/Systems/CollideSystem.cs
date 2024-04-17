
using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Entities;

namespace Server.Systems
{
  internal class CollideSystem : Shared.Systems.System
  {

    public delegate void Handler(int clientId, TimeSpan elapsedTime, Shared.Messages.Message message);
    public delegate void JoinHandler(int clientId);
    public delegate void DisconnectHandler(int clientId);
    public delegate void RemoveHandler(uint entityId);
    public delegate void EatHandler(uint entityId);

    private Dictionary<Shared.Messages.Type, Handler> m_commandMap = new Dictionary<Shared.Messages.Type, Handler>();
    private RemoveHandler m_removeHandler;
    private EatHandler m_eatHandler;
    private Dictionary<uint, Entity> m_players = new Dictionary<uint, Entity>();

    private List<Entity> removeList = new List<Entity>();
    private List<Entity> killed = new List<Entity>();
    private List<Entity> ateFood = new List<Entity>();

    public CollideSystem() : base(
                typeof(Shared.Components.Collision)
                )
    {
    }
    public override void update(TimeSpan elapsedTime)
    {
      foreach (Entity player in m_players.Values)
      {
        var pos1 = player.get<Position>().position;
        var size1 = player.get<Size>().size;


        if (pos1.X < 0 || pos1.X + size1.X > 5000 || pos1.Y < 0 || pos1.Y + size1.Y > 5000)
        {
          killed.Add(player);
          continue;
        }

        foreach (Entity other in m_entities.Values)
        {
          if (other.id == player.id)
          {
            continue;
          }
          var pos2 = other.get<Position>().position;
          var size2 = other.get<Size>().size;
          float distance = (float)Math.Sqrt((pos1.X - pos2.X)*(pos1.X - pos2.X) + (pos1.Y - pos2.Y)*(pos1.Y - pos2.Y));
          float radius = (size1.X + size2.X)/2;

          if (distance < radius)
          {
            if (other.contains<C_Food>())
            {
              removeList.Add(other);
              ateFood.Add(player);
              var head = player.get<Head>();
              head.score += 1;
            }
            else if (other.contains<Head>() && other.id != player.id)
            {
              killed.Add(player);
              killed.Add(other);
            }
            else if (other.contains<Connected>())
            {
              Entity follows = other.get<Connected>().follows;
              if (follows != null)
              {

                Entity? next = null;
                do
                {
                  next = follows.get<Connected>().follows;
                  if (next != null)
                  {
                    follows = next;
                  }
                }
                while (next != null);

                if (follows.id != player.id)
                {
                  killed.Add(player);
                }
              }
            }

          }
        }
      }
      playerAte();
      removeItems();
      killEntity();
    }

    public override bool add(Entity entity)
    {
      if (entity.contains<Head>())
      {
        m_players.Add(entity.id, entity);
      }

      return base.add(entity);
    }

    public override void remove(uint id)
    {
      if (m_players.ContainsKey(id))
      {
        m_players.Remove(id);
      }

      base.remove(id);
    }

    public void registerRemoveHandler(RemoveHandler handler)
    {
      m_removeHandler = handler;
    }

    public void registerEatHandler(EatHandler handler)
    {
      m_eatHandler = handler;
    }

    public void registerHandler(Shared.Messages.Type type, Handler handler)
    {
      m_commandMap[type] = handler;
    }

    public void removeItems()
    {
      foreach (Entity entity in removeList)
      {
        m_removeHandler(entity.id);
      }
      removeList.Clear();
    }

    public void killEntity()
    {
      foreach (Entity entity in killed)
      {
        m_removeHandler(entity.id);
      }
      killed.Clear();
    }

    public void playerAte()
    {
      foreach (Entity entity in ateFood)
      {
        m_eatHandler(entity.id);
      }
      ateFood.Clear();
    }
  }
}

