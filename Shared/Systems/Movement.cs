using Shared.Components;
using Shared.Entities;

namespace Shared.Systems
{
  public class Movement : System
  {
    public Movement()
        : base(
              typeof(Shared.Components.Movement),
              typeof(Shared.Components.Position))
    {
    }

    public override void update(TimeSpan elapsedTime)
    {
      foreach (var entity in m_entities.Values)
      {
        Shared.Entities.Utility.move(entity, elapsedTime);
        if (entity.contains<Connected>())
        {
          updateChildren(entity);
        }
      }
    }

    public void updateChildren(Entity entity)
    {
      var lead = entity;
      var following = entity.get<Connected>().leads;
      while (following != null)
      {
        var path = following.get<Shared.Components.Path>();
        var next = path.dequeue();
        var position = following.get<Position>();
        position.position = new Microsoft.Xna.Framework.Vector2 (next.x, next.y);
        position.orientation = next.orientation;
        path.enqueue(lead.get<Position>());

        lead = following;
        following = lead.get<Connected>().leads;
      }
    }

    public void clearSystem()
    {
      m_entities.Clear();
    }

  }
}
