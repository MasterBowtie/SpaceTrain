using CS5410;
using Microsoft.Xna.Framework;
using Shared.Components;

namespace Shared.Entities
{
  public class E_Food
  {
    public static Entity create(string texture, float size, ushort value, Vector2? setPos = null)
    {

      Entity entity = new Entity();

      MyRandom random = new MyRandom();
      Vector2 position = new Vector2(random.nextRange(100, 4900 - size), random.nextRange(100, 4900 - size));
      if (setPos != null)
      {
        position = setPos.Value;
      }
      int[] spriteTime = { 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100 };

      entity.add(new A_Appearance(texture, 10, spriteTime));
      entity.add(new Position(position));
      entity.add(new Size(new Vector2(size, size)));
      entity.add(new Collision());
      entity.add(new C_Food(value));

      return entity;
    }

    public static Entity create(string texture, float size, ushort value, uint id, Vector2? setPos = null)
    {

      Entity entity = new Entity(id);

      MyRandom random = new MyRandom();
      Vector2 position = new Vector2(random.nextRange(100, 4900 - size), random.nextRange(100, 4900 - size));
      if (setPos != null)
      {
        position = setPos.Value;
      }
      int[] spriteTime = { 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100, 100 };

      entity.add(new A_Appearance(texture, 10, spriteTime));
      entity.add(new Position(position));
      entity.add(new Size(new Vector2(size, size)));
      entity.add(new Collision());
      entity.add(new C_Food(value));

      return entity;
    }

  }
}
