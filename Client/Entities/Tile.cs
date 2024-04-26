using Microsoft.Xna.Framework;
using Shared.Components;

namespace Shared.Entities
{
  public class Tile
  {
    public static Entity create(uint id, Vector2 position, float size)
    {

      Entity entity = new Entity(id);

      entity.add(new Position(position));
      entity.add(new Size(new Vector2(size, size)));

      return entity;
    }
  }
}
