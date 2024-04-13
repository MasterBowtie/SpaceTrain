
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Entities;
using Shared.Components;

namespace Client.Systems
{
  public class TileRenderer : Shared.Systems.System
  {

    private GraphicsDeviceManager graphics;
    private Vector2 screenCenter;

    public TileRenderer(GraphicsDeviceManager graphics) :
        base(
            typeof(Client.Components.Sprite),
            typeof(Shared.Components.Position),
            typeof(Shared.Components.Size)
            )
    {
      this.graphics = graphics;

      screenCenter = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 2);

    }

    public override void update(TimeSpan elapsedTime) { }

    public void update(GameTime gameTime, SpriteBatch spriteBatch, Entity player)
    {
      Vector2 center = new Vector2(2500, 2500);

      if (player != null)
      {
        center = new Vector2(
          screenCenter.X - player.get<Position>().position.X + player.get<Size>().size.X / 2,
          screenCenter.Y - player.get<Position>().position.Y + player.get<Size>().size.Y / 2);
      }

      spriteBatch.Begin();

      foreach (Entity entity in m_entities.Values)
      {
        var position = entity.get<Shared.Components.Position>().position + center;
        if (position.X > -100 && position.X < graphics.PreferredBackBufferWidth + 100 && position.Y > -100 && position.Y < graphics.PreferredBackBufferHeight + 100) {
          var orientation = entity.get<Shared.Components.Position>().orientation;
          var size = entity.get<Shared.Components.Size>().size;
          var texture = entity.get<Components.Sprite>().texture;
          var texCenter = entity.get<Components.Sprite>().center;

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
