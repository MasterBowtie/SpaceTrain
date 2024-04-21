
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Client.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Shared.Components;
using Shared.Entities;

namespace Client.Systems
{
  public class TileRenderer : Shared.Systems.System
  {

    private GraphicsDeviceManager graphics;
    private Vector2 screenCenter;
    private Vector2 center = new Vector2(2500, 2500);
    private List<Texture2D> textureList;

    public TileRenderer(GraphicsDeviceManager graphics) :
        base(
            typeof(Sprite),
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
          player.get<Position>().position.X - 3 * player.get<Size>().size.X / 2 - screenCenter.X,
         player.get<Position>().position.Y - 3 * player.get<Size>().size.Y / 2 - screenCenter.Y);
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
        if (position.X > -100 && position.X < graphics.PreferredBackBufferWidth + 100 && position.Y > -100 && position.Y < graphics.PreferredBackBufferHeight + 100)
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

    public async void loadContent(ContentManager contentManager)
    {
      Texture2D image = contentManager.Load<Texture2D>("Textures/tile");
      List<Task> tasks = new List<Task>();
      int size = 25;
      for (int y = 0; y < size; y++)
      {
        for (int x = 0; x < size; x++)
        {
          Entity tile = Tile.create(new Vector2(x * 200, y * 200), 200);

          //int total = y * size + x;
          //String value = total.ToString("0000");
          //Texture2D image = contentManager.Load<Texture2D>(String.Format("Background/tiles{0}", value));

          tile.add(new Sprite(image));
          base.add(tile);
        }
      }
    }
  }
}
