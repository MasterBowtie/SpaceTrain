using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace apedaile
{
  public class AboutView : GameStateView
  {

    private GameStateEnum nextState = GameStateEnum.About;
    private SpriteFont mainFont;
    private SpriteFont titleFont;
    private Texture2D background;
    private Rectangle backRect;
    private Song music;

    public override void loadContent(ContentManager contentManager)
    {
      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime32");
      titleFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime64");
      // background = contentManager.Load<Texture2D>("Images/earth_image");
      // backRect = new Rectangle(graphics.PreferredBackBufferWidth - background.Width/4, 0, background.Width/4, background.Height/4);
    }

    public override GameStateEnum processInput(GameTime gameTime) {
      if (nextState != GameStateEnum.About)
      {
        GameStateEnum nextState = this.nextState;
        this.nextState = GameStateEnum.About;
        return nextState;
      }
      return GameStateEnum.About;
    }

    public override void render(GameTime gameTime) {
      String message;
      Vector2 stringSize;
      spriteBatch.Begin();
      // spriteBatch.Draw(background, backRect, Color.White);

      message = "About";
      stringSize = titleFont.MeasureString(message);
      float bottom = stringSize.Y + graphics.PreferredBackBufferHeight * .1f;
      spriteBatch.DrawString(
        titleFont, message, new Vector2(graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, graphics.PreferredBackBufferHeight * .1f), Color.White);
      
      message = "Game Development\n  Cody Apedaile\nGame Design:\n  Cody Apedaile\nStressed Out: \n  Cody Apedaile\n  Berklie Apedaile";
      stringSize = mainFont.MeasureString("Game Development:");
      spriteBatch.DrawString(mainFont, message, new Vector2 (graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2, bottom), Color.White);

      spriteBatch.End();
    }

    public override void setupInput(KeyboardInput keyboard) {
      keyboard.registerCommand(Keys.Escape, true, exitState, GameStateEnum.About,  Actions.exit);
    }

    public override void update(GameTime gameTime) {
      if (MediaPlayer.State == MediaState.Stopped) {
        // MediaPlayer.Play(music);
      }
    }

    private void exitState(GameTime gameTime, float value) {
      nextState = GameStateEnum.MainMenu;
    }

    private void pauseMusic(GameTime gameTime, float value) {
      MediaPlayer.Pause();
      System.Console.WriteLine("Music Paused");
    }

    private void resumeMusic(GameTime gameTime, float value) {
      MediaPlayer.Resume();
      System.Console.WriteLine("Music Resume");
    }
  }
}