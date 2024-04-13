using Microsoft.Xna.Framework;
using Shared.Components;

namespace Shared.Entities
{
  internal class E_Food
  {
    public Entity create(string texture, Vector2 position, float size)
    {

      Entity entity = new Entity();

      entity.add(new Appearance(texture));
      entity.add(new Position(position));
      entity.add(new Size(new Vector2(size, size)));
      entity.add(new Collision());

      return entity;
    }

  }
}
