using System;
using Client.States.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace apedaile
{
  public class AboutView : GameView
  {

    private GameViewEnum nextState = GameViewEnum.About;
    private SpriteFont mainFont;
    private SpriteFont titleFont;

    private Song music;
    private DrawText draw;

    public override void loadContent(ContentManager contentManager)
    {
      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime16");
      titleFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime64");
      
      draw = new DrawText(spriteBatch, graphics);
      draw.loadContent(contentManager);
    }

    public override GameViewEnum processInput(GameTime gameTime) {
      if (nextState != GameViewEnum.About)
      {
        GameViewEnum nextState = this.nextState;
        this.nextState = GameViewEnum.About;
        return nextState;
      }
      return GameViewEnum.About;
    }

    public override void render(GameTime gameTime) {
      Vector2 biggest = mainFont.MeasureString("Game Development");
      int buffer = 50;
      float x = graphics.PreferredBackBufferWidth / 2 - biggest.X / 2 - buffer / 2;
      spriteBatch.Begin();

      draw.drawCentered(titleFont, "About", graphics.PreferredBackBufferHeight * .1f, graphics.PreferredBackBufferWidth / 2 - titleFont.MeasureString("About").X / 2, titleFont.MeasureString("About").X, false);

      float bottom = draw.drawCentered(mainFont, "Game Development", graphics.PreferredBackBufferHeight * .4f, x, biggest.X + buffer, false);

      bottom = draw.drawCentered(mainFont, "  Cody Apedaile", bottom, x, biggest.X + buffer, false);
      bottom = draw.drawCentered(mainFont, "  Dean Mathias", bottom, x, biggest.X + buffer, false);
      bottom = draw.drawCentered(mainFont, "Game Design:", bottom, x, biggest.X + buffer, false);
      bottom = draw.drawCentered(mainFont, "  Cody Apedaile", bottom, x, biggest.X + buffer, false);
      bottom = draw.drawCentered(mainFont, "Images:", bottom, x, biggest.X + buffer, false);
      bottom = draw.drawCentered(mainFont, "  NASA", bottom, x, biggest.X + buffer, false);

      spriteBatch.End();
    }

    public override void setupInput(KeyboardInput keyboard) {
      keyboard.registerCommand(Keys.Escape, true, exitState, GameViewEnum.About,  Actions.exit);
    }

    public override void update(GameTime gameTime) {
      if (MediaPlayer.State == MediaState.Stopped) {
        // MediaPlayer.Play(music);
      }
    }

    private void exitState(GameTime gameTime, float value) {
      nextState = GameViewEnum.MainMenu;
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