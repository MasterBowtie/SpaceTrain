using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace apedaile {
  public interface IGameState {
    void initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics);

    void loadContent(ContentManager contentManager);

    void setupInput(KeyboardInput keyboard);

    GameStateEnum processInput(GameTime gameTime);

    void update(GameTime gameTime);

    void render(GameTime gameTime);
  }
}