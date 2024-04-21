
using System;
using System.Collections.Generic;
using Client;
using Client.States.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shared.Entities;
using Shared.Messages;

namespace apedaile
{
  public class GamePlayView : GameView
  {

    private SpriteFont mainFont;
    private Texture2D textBack;

    private ContentManager contentManager;
    private Dictionary<uint, Entity> entities;

    private Client.Systems.KeyboardInput systemKeyboardInput;
    private KeyboardInput keyboard;
    private GameModel model;
    private DrawText draw;

    private GameState currentState;
    private GameState playState;
    private GameState lostState;


    private HashSet<Keys> previouslyDown = new HashSet<Keys>();
    private uint score = 0;

    private GameViewEnum nextState = GameViewEnum.GamePlay;


    public override void setupInput(KeyboardInput keyboard)
    { }

    public void setupInput(KeyboardInput keyboard, Client.Systems.KeyboardInput systemKeyboardInput)
    {
      keyboard.registerCommand(Keys.Escape, true, new IInputDevice.CommandDelegate(exit), GameViewEnum.GamePlay, Shared.Components.Input.Type.Exit);
      keyboard.registerCommand(Keys.Enter, true, new IInputDevice.CommandDelegate(proceed), GameViewEnum.GamePlay, Shared.Components.Input.Type.Select);
      keyboard.registerCommand(Keys.Up, false, new IInputDevice.CommandDelegate(up), GameViewEnum.GamePlay, Shared.Components.Input.Type.Up);
      keyboard.registerCommand(Keys.Down, false, new IInputDevice.CommandDelegate(down), GameViewEnum.GamePlay, Shared.Components.Input.Type.Down);
      keyboard.registerCommand(Keys.Left, false, new IInputDevice.CommandDelegate(left), GameViewEnum.GamePlay, Shared.Components.Input.Type.Left);
      keyboard.registerCommand(Keys.Right, false, new IInputDevice.CommandDelegate(right), GameViewEnum.GamePlay, Shared.Components.Input.Type.Right);
      // Attempt to add to storage
      this.systemKeyboardInput = systemKeyboardInput;
    }

    public void attachModel(GameModel gameModel)
    {
      this.model = gameModel;
    }

    public void setupNetwork(Client.Systems.Network network)
    {
      network.registerHandler(Shared.Messages.Type.Score, (TimeSpan elapsedTime, Message message) =>
      {
        handleGetScore((Score)message);
      });
    }

    public override void loadContent(ContentManager contentManager)
    {
      playState = new PlayState(this);
      lostState = new LostState(this);
      currentState = playState;

      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime32");
      textBack = contentManager.Load<Texture2D>("Textures/menu");

      this.contentManager = contentManager;
    }

    public override void setupDraw(DrawText draw)
    {
      this.draw = draw;
    }

    public override GameViewEnum processInput(GameTime gameTime)
    {
      foreach (var key in previouslyDown)
      {
        if (Keyboard.GetState().IsKeyUp(key))
        {

          signalKeyReleased(key);
          previouslyDown.Remove(key);
        }
      }

      foreach (var key in Keyboard.GetState().GetPressedKeys())
      {
        if (!previouslyDown.Contains(key))
        {
          signalKeyPressed(key);
          previouslyDown.Add(key);
        }
      }

      if (nextState != GameViewEnum.GamePlay)
      {
        GameViewEnum newState = nextState;
        nextState = GameViewEnum.GamePlay;
        return newState;
      }
      return GameViewEnum.GamePlay;
    }

    public override void render(GameTime gameTime)
    {
      currentState.render(gameTime);
    }

    public override void update(GameTime gameTime)
    {
      systemKeyboardInput.update(gameTime.ElapsedGameTime);
      if (currentState == playState && model.player != null && !model.checkPlayer())
      {
        currentState = lostState;
      }
      currentState.update(gameTime);
    }

    public void exit(GameTime gameTime, float value)
    {
      model.player = null;
      MessageQueueClient.instance.sendMessage(new Leave());
      currentState = playState;
      nextState = GameViewEnum.MainMenu;
      score = 0;
    }

    public void proceed(GameTime gameTime, float value)
    {
      if (currentState == lostState)
      {
        exit(gameTime, value);
      }
    }

    /// <summary>
    /// Inputs are for pairing with the Network Inputs
    /// This helps with storage
    /// </summary>
    /// <param name="gametime"></param>
    /// <param name="value"></param>
    public void up(GameTime gametime, float value) { }
    public void down(GameTime gametime, float value) { }
    public void left(GameTime gametime, float value) { }
    public void right(GameTime gametime, float value) { }

    public void signalKeyPressed(Keys key)
    {
      systemKeyboardInput.keyPressed(key);
    }

    public void signalKeyReleased(Keys key)
    {
      systemKeyboardInput.keyReleased(key);
    }

    private void handleGetScore(Score message)
    {
      this.score = message.score;
    }


    private class PlayState : GameState
    {
      private GamePlayView parent;

      public PlayState(GamePlayView parent)
      {
        this.parent = parent;
      }

      public void render(GameTime gametime)
      {
        parent.spriteBatch.Begin();

        Vector2 stringSize = parent.mainFont.MeasureString(" Score: 0000");

        parent.draw.drawLeft(parent.mainFont, String.Format(" Score: {0}", parent.score), 5, 5, stringSize.X, true);

        parent.spriteBatch.End();
      }

      public void update(GameTime gameTime)
      {
      }
    }
    private class LostState : GameState
    {
      private GamePlayView parent;

      public LostState(GamePlayView parent)
      {
        this.parent = parent;
      }

      public void render(GameTime gametime)
      {
        parent.spriteBatch.Begin();

        float buffer = 50;
        Vector2 measure = parent.mainFont.MeasureString("You Died!");
        Vector2 measure2 = parent.mainFont.MeasureString("Scores: 0000");

        parent.spriteBatch.Draw(
          parent.textBack,
          new Rectangle(
            (int)(parent.graphics.PreferredBackBufferWidth / 2 - measure2.X / 2 - buffer),
            (int)(parent.graphics.PreferredBackBufferHeight / 2 - measure2.Y - buffer),
            (int)(measure2.X + buffer * 2),
            (int)(measure2.Y * 2 + buffer * 2)),
          Color.White);
        parent.spriteBatch.End();


        float bottom = parent.draw.drawCentered(parent.mainFont,
          "You Died!",
          parent.graphics.PreferredBackBufferHeight / 2 - measure.Y,
          parent.graphics.PreferredBackBufferWidth / 2 - measure.X - buffer / 2,
          measure.X,
          false
          );

        parent.draw.drawCentered(
          parent.mainFont,
          String.Format("Scores: {0}", parent.score),
          bottom,
          parent.graphics.PreferredBackBufferWidth / 2 - measure2.X - buffer / 2,
          measure2.X,
          false);

      }

      public void update(GameTime gameTime)
      {

      }
    }
  }
}