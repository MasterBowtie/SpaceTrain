
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Shared.Components;

namespace Shared.Entities
{
  public class Segment
  {
    public static Entity create(float size, float moveRate, Position position, Entity follows, Entity? leads = null)
    {
      Entity entity = new Entity();
      
      entity.add(new Position(new Vector2(position.position.X + 0.0f, position.position.Y + 0.0f), position.orientation + 0.0f));
      entity.add(new Shared.Components.Path(position, moveRate));
      entity.add(new Connected(leads, follows));
      entity.add(new Size(new Vector2(size, size)));
      entity.add(new Collision());

      return entity;
    }
  }
}
