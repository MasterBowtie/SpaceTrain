using Microsoft.Xna.Framework;
using Shared.Components;
using Shared;

namespace Shared.Entities
{
  public class Player
  {
    public static Entity create(int playerId, string texture, float size, float moveRate, float rotateRate, string playerName)
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
      entity.add(new Head(playerId, playerName));

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

    private const float DIR_RIGHT = 0;
    private const float DIR_SE = (float)(2 * Math.PI * .125f);
    private const float DIR_DOWN = (float)(2 * Math.PI * 0.25);
    private const float DIR_SW = (float)(2 * Math.PI * .375f);
    private const float DIR_UP = (float)(2 * Math.PI * 0.75);
    private const float DIR_NW = (float)(2 * Math.PI * .625f);
    private const float DIR_LEFT = (float)(2 * Math.PI * 0.50);
    private const float DIR_NE = (float)(2 * Math.PI * .875f);


    public static void move(Entity entity, TimeSpan elapsedTime)
    {
      var position = entity.get<Position>();
      var movement = entity.get<Movement>();

      var vectorX = Math.Cos(position.orientation);
      var vectorY = Math.Sin(position.orientation);

      position.position = new Vector2(
          (float)(position.position.X + vectorX * movement.moveRate * elapsedTime.Milliseconds),
          (float)(position.position.Y + vectorY * movement.moveRate * elapsedTime.Milliseconds));
    }
    public static Entity? up(Entity entity, TimeSpan elapsedTime)
    {
      var position = entity.get<Position>();

      // Can't turn if already moving up or moving down, because can't move backward over itself
      if (position.orientation != DIR_UP && position.orientation != DIR_DOWN)
      {
        position.orientation = DIR_UP;
        // Because we accepted a turn, this is a new turn point right here and in this direction
        int snakeId = entity.get<Head>().id;
        //return Shared.Entities.TurnPoint.create(snakeId, position.position, position.orientation);
      }

      return null;
    }

    public static Entity? left(Entity entity, TimeSpan elapsedTime)
    {
      var position = entity.get<Position>();

      // Can't turn if moving already moving left or right, because can't move backward over itself
      if (position.orientation != DIR_LEFT && position.orientation != DIR_RIGHT)
      {
        position.orientation = DIR_LEFT;
        // Because we accepted a turn, this is a new turn point right here and in this direction
        int snakeId = entity.get<Head>().id;
        //return Shared.Entities.TurnPoint.create(snakeId, position.position, position.orientation);
      }

      return null;
    }

    public static Entity? right(Entity entity, TimeSpan elapsedTime)
    {
      var position = entity.get<Position>();

      // Can't turn if moving already moving right or left, because can't move backward over itself
      if (position.orientation != DIR_RIGHT && position.orientation != DIR_LEFT)
      {
        position.orientation = DIR_RIGHT;
        // Because we accepted a turn, this is a new turn point right here and in this direction
        int snakeId = entity.get<Head>().id;
        //return Shared.Entities.TurnPoint.create(snakeId, position.position, position.orientation);
      }

      return null;
    }

    public static Entity? down(Entity entity, TimeSpan elapsedTime)
    {
      var position = entity.get<Position>();

      // Can't turn if already moving down or moving up, because can't move backward over itself
      if (position.orientation != DIR_DOWN && position.orientation != DIR_UP)
      {
        position.orientation = DIR_DOWN;
        // Because we accepted a turn, this is a new turn point right here and in this direction
        int snakeId = entity.get<Head>().id;
        //return Shared.Entities.TurnPoint.create(snakeId, position.position, position.orientation);
      }

      return null;
    }

    public static Entity? se(Entity entity, TimeSpan elapsedTime)
    {
      var position = entity.get<Position>();

      // Can't turn if already moving down or moving up, because can't move backward over itself
      if (position.orientation != DIR_SE && position.orientation != DIR_NW)
      {
        position.orientation = DIR_SE;
        // Because we accepted a turn, this is a new turn point right here and in this direction
        int snakeId = entity.get<Head>().id;
        //return Shared.Entities.TurnPoint.create(snakeId, position.position, position.orientation);
      }

      return null;
    }

    public static Entity? sw(Entity entity, TimeSpan elapsedTime)
    {
      var position = entity.get<Position>();

      // Can't turn if already moving down or moving up, because can't move backward over itself
      if (position.orientation != DIR_SW && position.orientation != DIR_NE)
      {
        position.orientation = DIR_SW;
        // Because we accepted a turn, this is a new turn point right here and in this direction
        int snakeId = entity.get<Head>().id;
        //return Shared.Entities.TurnPoint.create(snakeId, position.position, position.orientation);
      }

      return null;
    }

    public static Entity? ne(Entity entity, TimeSpan elapsedTime)
    {
      var position = entity.get<Position>();

      // Can't turn if already moving down or moving up, because can't move backward over itself
      if (position.orientation != DIR_NE && position.orientation != DIR_SW)
      {
        position.orientation = DIR_NE;
        // Because we accepted a turn, this is a new turn point right here and in this direction
        int snakeId = entity.get<Head>().id;
        //return Shared.Entities.TurnPoint.create(snakeId, position.position, position.orientation);
      }

      return null;
    }

    public static Entity? nw(Entity entity, TimeSpan elapsedTime)
    {
      var position = entity.get<Position>();  

      // Can't turn if already moving down or moving up, because can't move backward over itself
      if (position.orientation != DIR_NW && position.orientation != DIR_SE)
      {
        position.orientation = DIR_NW;
        // Because we accepted a turn, this is a new turn point right here and in this direction
        int snakeId = entity.get<Head>().id;
        //return Shared.Entities.TurnPoint.create(snakeId, position.position, position.orientation);
      }

      return null;
    }
  }
}
