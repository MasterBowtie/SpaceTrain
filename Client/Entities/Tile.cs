using Microsoft.Xna.Framework;
using Shared.Components;

namespace Shared.Entities
{
  public class Tile
  {
    public static Entity create(Vector2 position, float size)
    {

      Entity entity = new Entity();

      entity.add(new Position(position));
      entity.add(new Size(new Vector2(size, size)));

      return entity;
    }
  }
}
