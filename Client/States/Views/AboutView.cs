using System;
using Client.States.Views;
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

    private Song music;
    private DrawText draw;

    public override void loadContent(ContentManager contentManager)
    {
      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime32");
      titleFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime64");
      
      draw = new DrawText(spriteBatch, graphics);
      draw.loadContent(contentManager);
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
      Vector2 biggest = mainFont.MeasureString("Game Development");
      int buffer = 50;
      float x = graphics.PreferredBackBufferWidth / 2 - biggest.X / 2 - buffer / 2;
      spriteBatch.Begin();

      draw.drawMenuItem(titleFont, "About", graphics.PreferredBackBufferHeight * .1f, graphics.PreferredBackBufferWidth / 2 - titleFont.MeasureString("About").X / 2, titleFont.MeasureString("About").X, false);

      float bottom = draw.drawMenuItem(mainFont, "Game Development", graphics.PreferredBackBufferHeight * .4f, x, biggest.X + buffer, false);

      bottom = draw.drawMenuItem(mainFont, "  Cody Apedaile", bottom, x, biggest.X + buffer, false);
      bottom = draw.drawMenuItem(mainFont, "  Dean Mathias", bottom, x, biggest.X + buffer, false);
      bottom = draw.drawMenuItem(mainFont, "Game Design:", bottom, x, biggest.X + buffer, false);
      bottom = draw.drawMenuItem(mainFont, "  Cody Apedaile", bottom, x, biggest.X + buffer, false);
      bottom = draw.drawMenuItem(mainFont, "Images:", bottom, x, biggest.X + buffer, false);
      bottom = draw.drawMenuItem(mainFont, "  NASA", bottom, x, biggest.X + buffer, false);

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