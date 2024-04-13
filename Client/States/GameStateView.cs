using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace apedaile {
  public abstract class GameStateView: IGameState {
    protected GraphicsDeviceManager graphics;
    protected BasicEffect effect;
    protected SpriteBatch spriteBatch;
    protected KeyboardInput keyboard;

    public void initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics) {
      this.graphics = graphics;
      this.spriteBatch = new SpriteBatch(graphicsDevice);
    }

    public abstract void loadContent(ContentManager contentManager);
    
    public abstract void setupInput(KeyboardInput keyboard);

    public abstract GameStateEnum processInput(GameTime gameTime);

    public abstract void render(GameTime gameTime);

    public abstract void update(GameTime gameTime);
  }
}