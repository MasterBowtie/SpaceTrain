using System;
using System.Collections.Generic;
using Client.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Components;
using Shared.Entities;

namespace Client.Systems
{
  public class ParticleSystemRenderer : Shared.Systems.System
  {

    private GraphicsDeviceManager graphics;
    private Vector2 screenCenter;
    private Vector2 center = new Vector2(-2500, -2500);


    public ParticleSystemRenderer(GraphicsDeviceManager graphics) :
      base(typeof(Sprite))

    {
      this.graphics = graphics;

      screenCenter = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);
    }

    public override void update(TimeSpan elapsedTime) { }
    public void update(GameTime gameTime, SpriteBatch spriteBatch, Entity player)
    {
      if (player != null)
      {
        center = new Vector2(
         player.get<Position>().position.X - screenCenter.X,
         player.get<Position>().position.Y - screenCenter.Y);
      }
      else
      {
        center = new Vector2(2500, 2500);
      }

      spriteBatch.Begin();

      foreach (Entity entity in m_entities.Values)
      {
        var position = entity.get<Shared.Components.Position>().position - center;
        var size = entity.get<Shared.Components.Size>().size;
        if (position.X - size.X > -100 && position.X < graphics.PreferredBackBufferWidth + 100 && position.Y > -100 && position.Y < graphics.PreferredBackBufferHeight + 100)
        {
          var orientation = entity.get<Shared.Components.Position>().orientation;
          var texture = entity.get<Sprite>().texture;
          var texCenter = entity.get<Sprite>().center;

          // Build a rectangle centered at position, with width/height of size

          Rectangle rectangle = new Rectangle(
              (int)(position.X),
              (int)(position.Y),
              (int)size.X,
              (int)size.Y);


          spriteBatch.Draw(
              texture,
              rectangle,
              null,
              Color.White,
              orientation,
              texCenter,
              SpriteEffects.None,
              0);
        }

      }
      spriteBatch.End();
    }

  }
}
