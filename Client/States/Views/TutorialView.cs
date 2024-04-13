using Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace apedaile{
  public class TutorialView: GameStateView {

    private SpriteFont mainFont;
    private GameStateEnum nextState = GameStateEnum.Tutorial;

    public override void loadContent(ContentManager contentManager) {
      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime32");
    }
    
    public override void setupInput(KeyboardInput keyboard) {
      keyboard.registerCommand(Keys.Enter, true, new IInputDevice.CommandDelegate(proceed), GameStateEnum.Tutorial, Actions.select);
    }

    public override GameStateEnum processInput(GameTime gameTime) {
    if (nextState != GameStateEnum.Tutorial) {
        GameStateEnum newState = nextState;
        nextState = GameStateEnum.Tutorial;
        return newState;
      }
      return GameStateEnum.Tutorial;
    }
    
    public override void render(GameTime gameTime) {
      spriteBatch.Begin();
      Vector2 measure = mainFont.MeasureString("Welcome to");
      spriteBatch.DrawString(mainFont, "Welcome to", new Vector2(graphics.PreferredBackBufferWidth/2 - measure.X/2, graphics.PreferredBackBufferHeight/2 - measure.Y*2), Color.White);
      measure = mainFont.MeasureString("Competitive Snake");
      spriteBatch.DrawString(mainFont, "Competitive Snake", new Vector2(graphics.PreferredBackBufferWidth/2 - measure.X/2, graphics.PreferredBackBufferHeight/2 - measure.Y), Color.White);

      spriteBatch.End();
    }
    
    public override void update(GameTime gameTime) {
    }

    public void proceed(GameTime gameTime, float value) {
      nextState = GameStateEnum.GamePlay;
    }
  }
}