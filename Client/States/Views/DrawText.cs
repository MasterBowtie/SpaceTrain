using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Client.States.Views
{
  public class DrawText
  {

    SpriteBatch spriteBatch;
    GraphicsDeviceManager graphics;
    Texture2D selector;
    float buffer = 5;

    public DrawText(SpriteBatch spriteBatch, GraphicsDeviceManager graphics)
    {
      this.spriteBatch = spriteBatch;
      this.graphics = graphics;
    }

    public void loadContent(ContentManager contentManager)
    {
      selector = contentManager.Load<Texture2D>("Textures/MenuSelector");
    }

    public float drawMenuItem(SpriteFont font, string text, float y, float x, float xSize, bool selected)
    {
      Vector2 stringSize = font.MeasureString(text);

      if (selected) {
        if (selected)
        {
          spriteBatch.Draw(selector, new Rectangle((int)x, (int)y, (int)xSize, (int)stringSize.Y), Color.White);
        }
      }

      spriteBatch.DrawString(
        font, text, new Vector2(graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2 - 2, y), Color.Black);
      spriteBatch.DrawString(
        font, text, new Vector2(graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2 + 2, y), Color.Black);
      spriteBatch.DrawString(
        font, text, new Vector2(graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, y - 2), Color.Black);
      spriteBatch.DrawString(
        font, text, new Vector2(graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, y + 2), Color.Black);
      spriteBatch.DrawString(
        font, text, new Vector2(graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, y), Color.White);

      return y + stringSize.Y;
    }
  }
}
