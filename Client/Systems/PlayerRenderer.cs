
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Components;
using Shared.Entities;

namespace Client.Systems
{
  public class PlayerRenderer : Shared.Systems.System
  {

    private GraphicsDeviceManager graphics;
    private Vector2 screenCenter;
    private Vector2 center = new Vector2(-2500, -2500);

    public PlayerRenderer(GraphicsDeviceManager graphics) :
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
      if (player != null)
      {
        center = new Vector2(
         player.get<Position>().position.X - screenCenter.X,
         player.get<Position>().position.Y - screenCenter.Y);
      }

      spriteBatch.Begin();

      foreach (Entity entity in m_entities.Values)
      {
        var position = entity.get<Shared.Components.Position>().position - center;
        var size = entity.get<Shared.Components.Size>().size;
        if (position.X - size.X > -100 && position.X < graphics.PreferredBackBufferWidth + 100 && position.Y > -100 && position.Y < graphics.PreferredBackBufferHeight + 100)
        {
          var orientation = entity.get<Shared.Components.Position>().orientation;
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

        if (entity.contains<Connected>())
        {
          drawChildren(entity, spriteBatch);
        }
      }

      spriteBatch.End();
    }

    private void drawChildren(Entity head, SpriteBatch spriteBatch)
    {
      Entity entity = head.get<Connected>().leads;

      var texture = head.get<Components.Sprite>().texture;
      var texCenter = head.get<Components.Sprite>().center;
      while (entity != null)
      {
        var position = entity.get<Shared.Components.Position>().position - center;
        var size = entity.get<Shared.Components.Size>().size;
        if (position.X - size.X / 2 > -100 && position.X + size.X < graphics.PreferredBackBufferWidth + 100 && position.Y > -100 && position.Y + size.Y < graphics.PreferredBackBufferHeight + 100)
        {
          var orientation = entity.get<Shared.Components.Position>().orientation;

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
        entity = entity.get<Connected>().leads;
      }
    }
    public void clearSystem()
    {
      m_entities = new System.Collections.Generic.Dictionary<uint, Shared.Entities.Entity>();
    }
  }
}
