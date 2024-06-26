using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Shared.Entities;
using Shared.Messages;
using System.Collections.Generic;
using Client.States.Views;

namespace apedaile
{
  public class HighScoresView : GameView
  {

    private GameViewEnum nextState = GameViewEnum.HighScores;
    private SpriteFont mainFont;
    private SpriteFont titleFont;

    private List<(string,uint)> scores;
    private DrawText draw;

    public override void loadContent(ContentManager contentManager)
    {
      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime16");
      titleFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime64");
    }

    public override void setupDraw(DrawText draw)
    {
      this.draw = draw;
    }

    public void setupNetwork(Client.Systems.Network network) {
      network.registerHandler(Shared.Messages.Type.HighScores, (TimeSpan elapsedTime, Message message) =>
      {
        handleHighScores((HighScores)message);
      });
    }

    public override GameViewEnum processInput(GameTime gameTime) {
      if (nextState != GameViewEnum.HighScores)
      {
        GameViewEnum nextState = this.nextState;
        this.nextState = GameViewEnum.HighScores;
        return nextState;
      }
      return GameViewEnum.HighScores;
    }

    public override void render(GameTime gameTime) {
      Vector2 biggest = mainFont.MeasureString("High Scores");
      int buffer = 50;
      float x = graphics.PreferredBackBufferWidth / 2 - biggest.X / 2 - buffer / 2;
      String message = "";
      
      spriteBatch.Begin();


      float bottom = draw.drawCentered(titleFont, "High Scores", graphics.PreferredBackBufferHeight * .1f, graphics.PreferredBackBufferWidth / 2 - titleFont.MeasureString("High Scores").X / 2, titleFont.MeasureString("High Scores").X, false);
      bottom += buffer;


      if (scores != null)
      {
        foreach (var item in scores)
        {
          message = String.Format("Player: {0}", item.Item1);
          bottom = draw.drawLeft(mainFont, message, bottom, x, biggest.X, false);
          message = String.Format("   Score: {0}", item.Item2);
          bottom = draw.drawLeft(mainFont, message, bottom, x, biggest.X, false);
        }
      }
      else
      {
        message = "Loading";
        bottom = draw.drawCentered(titleFont, message, bottom, x, biggest.X, false);
      }
      spriteBatch.End();
    }

    public override void setupInput(KeyboardInput keyboard) {
      keyboard.registerCommand(Keys.Escape, true, exitState,GameViewEnum.HighScores, Shared.Components.Input.Type.Exit);
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

    public void handleHighScores(Shared.Messages.HighScores message)
    {
      this.scores = message.scores;
    }
  }

  public delegate void SaveScore();
}