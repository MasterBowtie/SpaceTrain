using Client.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Components;
using Shared.Entities;

namespace Client.Entities
{
  public class Particle
  {
    public static Entity create(Texture2D texture, Vector2 position, float orientation, float size, float moveRate, float time)
    {
      Entity particle = new Entity();

      particle.add(new Sprite(texture));
      particle.add(new Movement(moveRate, 0.0f));
      particle.add(new Position(position, orientation));
      particle.add(new Size(new Vector2(size, size)));
      particle.add(new LifeTime(time));

      return particle;
     }
  }
}
