using Microsoft.Xna.Framework;
using Shared.Components;
using Shared;
using CS5410;

namespace Shared.Entities
{
  public class E_Player
  {
    public static Entity create(string texture, float size, float moveRate, float rotateRate)
    {
      Entity entity = new Entity();

      entity.add(new Appearance(texture));
      MyRandom random = new MyRandom();
      Vector2 position = new Vector2(random.nextRange(100, 4900 - size), random.nextRange(100, 4900-size));
      float orientation = 0.0f;
      if (position.X < 2500)
      {
        orientation = (float)(Math.PI / 4.0f);
      }
      else {
        orientation = (float)(3 * Math.PI / 4.0f);
      }
      if (position.Y > 2500) { 
        orientation = orientation * -1; 
      }

      entity.add(new Position(position, orientation));
      entity.add(new Size(new Vector2(size, size)));
      entity.add(new Movement(moveRate, rotateRate));
      entity.add(new Collision());
      entity.add(new Head());

      List<Input.Type> inputs = new List<Input.Type>();
      inputs.Add(Input.Type.Up);
      inputs.Add(Input.Type.Left);
      inputs.Add(Input.Type.Right);
      inputs.Add(Input.Type.Down);
      entity.add(new Input(inputs));

      return entity;
    }
  }

  public class Utility
  {
    public static void up(Entity entity, TimeSpan elapsedTime)
    {
      var position = entity.get<Position>();
      var movement = entity.get<Movement>();

      if (position.orientation < (Math.PI / 2) && position.orientation > -Math.PI / 2)
      {
        position.orientation = position.orientation - movement.rotateRate * elapsedTime.Milliseconds;
      }
      else
      {
        position.orientation = position.orientation + movement.rotateRate * elapsedTime.Milliseconds;
      }

      rotationWrap(entity);
    }

    public static void left(Entity entity, TimeSpan elapsedTime)
    {
      var position = entity.get<Position>();
      var movement = entity.get<Movement>();

      if (position.orientation < 0 && position.orientation > -Math.PI)
      {
        position.orientation = position.orientation - movement.rotateRate * elapsedTime.Milliseconds;
      }
      else
      {
        position.orientation = position.orientation + movement.rotateRate * elapsedTime.Milliseconds;
      }

      rotationWrap(entity);
    }

    public static void right(Entity entity, TimeSpan elapsedTime)
    {
      var position = entity.get<Position>();
      var movement = entity.get<Movement>();

      if (position.orientation < Math.PI && position.orientation > 0)
      {
        position.orientation = position.orientation - movement.rotateRate * elapsedTime.Milliseconds;
      }
      else
      {
        position.orientation = position.orientation + movement.rotateRate * elapsedTime.Milliseconds;
      }

      rotationWrap(entity);
    }

    public static void down(Entity entity, TimeSpan elapsedTime)
    {
      var position = entity.get<Position>();
      var movement = entity.get<Movement>();

      if (position.orientation < Math.PI / 2 && position.orientation > -Math.PI / 2)
      {
        position.orientation = position.orientation + movement.rotateRate * elapsedTime.Milliseconds;
      }
      else
      {
        position.orientation = position.orientation - movement.rotateRate * elapsedTime.Milliseconds;
      }

      rotationWrap(entity);
    }

    public static void rotationWrap(Entity entity)
    {
      var position = entity.get<Position>();
      if (position.orientation < -Math.PI)
      {
        position.orientation = (float)(position.orientation + 2 * Math.PI);
      }
      if (position.orientation > Math.PI)
      {
        position.orientation = (float)(position.orientation - 2 * Math.PI);
      }
    }
  }
}
