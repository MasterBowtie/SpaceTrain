using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Media;
using Microsoft.Win32.SafeHandles;
using Shared.Entities;
using Shared.Messages;
using System.Collections.Generic;
using Client.States.Views;

namespace apedaile
{
  public class HighScoresView : GameStateView
  {

    private GameStateEnum nextState = GameStateEnum.HighScores;
    private SpriteFont mainFont;
    private SpriteFont titleFont;

    private List<uint> scores;
    private DrawText draw;

    public override void loadContent(ContentManager contentManager)
    {
      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime32");
      titleFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime64");

      draw = new DrawText(spriteBatch, graphics);
      draw.loadContent(contentManager);
    }

    public void setupNetwork(Client.Systems.Network network) {
      network.registerHandler(Shared.Messages.Type.HighScores, (TimeSpan elapsedTime, Message message) =>
      {
        handleHighScores((HighScores)message);
      });
    }

    public override GameStateEnum processInput(GameTime gameTime) {
      if (nextState != GameStateEnum.HighScores)
      {
        GameStateEnum nextState = this.nextState;
        this.nextState = GameStateEnum.HighScores;
        return nextState;
      }
      return GameStateEnum.HighScores;
    }

    public override void render(GameTime gameTime) {
      Vector2 biggest = mainFont.MeasureString("High Scores");
      int buffer = 50;
      float x = graphics.PreferredBackBufferWidth / 2 - biggest.X / 2 - buffer / 2;
      String message = "";
      
      spriteBatch.Begin();


      float bottom = draw.drawMenuItem(titleFont, "High Scores", graphics.PreferredBackBufferHeight * .1f, graphics.PreferredBackBufferWidth / 2 - titleFont.MeasureString("High Scores").X / 2, titleFont.MeasureString("High Scores").X, false);
      bottom += buffer;


      if (scores != null)
      {
        foreach (var item in scores)
        {
          message = String.Format("Score: {0}", item);
          bottom = draw.drawMenuItem(mainFont, message, bottom, x, biggest.X, false);
        }
      }
      else
      {
        message = "Loading";
        bottom = draw.drawMenuItem(mainFont, message, bottom, x, biggest.X, false);
      }
      spriteBatch.End();
    }

    public override void setupInput(KeyboardInput keyboard) {
      keyboard.registerCommand(Keys.Escape, true, exitState,GameStateEnum.HighScores, Actions.exit);
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

    public void handleHighScores(Shared.Messages.HighScores message)
    {
      this.scores = message.scores;
    }
  }

  public delegate void SaveScore();
}