
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared.Components;
using Shared.Entities;

namespace Client.Systems
{
  public class FoodRenderer : Shared.Systems.System
  {

    private GraphicsDeviceManager graphics;
    private Vector2 screenCenter;
    private Vector2 center = new Vector2(-2500, -2500);

    public FoodRenderer(GraphicsDeviceManager graphics) :
        base(
            typeof(Client.Components.A_Sprite),
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
        if (position.X > -100 && position.X  < graphics.PreferredBackBufferWidth + 100 && position.Y > -100 && position.Y < graphics.PreferredBackBufferHeight + 100)
        {
          var orientation = entity.get<Shared.Components.Position>().orientation;
          var sprite = entity.get<Components.A_Sprite>();
          var texture = sprite.texture;
          var texCenter = sprite.center;
          sprite.animationTime += gameTime.ElapsedGameTime;
          if (sprite.animationTime.TotalMilliseconds >= sprite.spriteTime[sprite.subImageIndex])
          {
            sprite.animationTime -= TimeSpan.FromMilliseconds(sprite.spriteTime[sprite.subImageIndex]);
            sprite.subImageIndex++;
            sprite.subImageIndex = sprite.subImageIndex % sprite.spriteTime.Length;
          }
          var subImageIndex = sprite.subImageIndex;
          var subImageWidth = sprite.subImageWidth;


          // Build a rectangle centered at position, with width/height of size
          Rectangle rectangle = new Rectangle(
              (int)(position.X),
              (int)(position.Y),
              (int)size.X,
              (int)size.Y);
          // Build a rectangle for specific image
          Rectangle image = new Rectangle(
            subImageIndex * subImageWidth,
            0, 
            subImageWidth,
            texture.Height
            );


          spriteBatch.Draw(
              texture,
              rectangle,
              image,
              Color.White,
              orientation,
              texCenter,
              SpriteEffects.None,
              0);
        }
      }

      spriteBatch.End();
    }
    public void clearSystem()
    {
      m_entities = new System.Collections.Generic.Dictionary<uint, Shared.Entities.Entity>();
    }
  }
}
