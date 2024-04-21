using Client.States.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace apedaile {
  public interface IGameView {
    void initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics);
    void loadContent(ContentManager contentManager);
    void setupDraw(DrawText draw);
    void setupInput(KeyboardInput keyboard);
    GameViewEnum processInput(GameTime gameTime);
    void update(GameTime gameTime);
    void render(GameTime gameTime);
  }
}