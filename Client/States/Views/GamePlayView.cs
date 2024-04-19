
using System;
using System.Collections.Generic;
using System.Timers;
using Client;
using Client.Components;
using Client.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shared.Components;
using Shared.Entities;
using Shared.Messages;

namespace apedaile
{
  public class GamePlayView : GameStateView
  {

    private SpriteFont mainFont;

    private ContentManager contentManager;
    private Dictionary<uint, Entity> entities;
    
    
    private Client.Systems.KeyboardInput systemKeyboardInput;
    private KeyboardInput keyboard;



    private GamePlayState currentState;
    private GamePlayState tutorialState;
    private GamePlayState playState;
    private GamePlayState lostState;


    private HashSet<Keys> previouslyDown = new HashSet<Keys>();
    private uint score = 0;
    private float timer = 1000;
    private bool joined = false;
    private Entity player;
    private float moveRate;


    private GameStateEnum nextState = GameStateEnum.GamePlay;


    public override void setupInput(KeyboardInput keyboard)
    {}

    public void setupInput(KeyboardInput keyboard, Client.Systems.KeyboardInput systemKeyboardInput)
    {
      keyboard.registerCommand(Keys.Escape, true, new IInputDevice.CommandDelegate(exit), GameStateEnum.GamePlay, Actions.exit);
      keyboard.registerCommand(Keys.Enter, true, new IInputDevice.CommandDelegate(proceed), GameStateEnum.GamePlay, Actions.select);

      // Attempt to add to storage
      this.systemKeyboardInput = systemKeyboardInput;
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
      tutorialState = new TutorialState(this);
      playState = new PlayState(this);
      lostState = new LostState(this);
      currentState = tutorialState;

      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime32");

      this.contentManager = contentManager;
    }

    public override GameStateEnum processInput(GameTime gameTime)
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

      if (nextState != GameStateEnum.GamePlay)
      {
        GameStateEnum newState = nextState;
        nextState = GameStateEnum.GamePlay;
        return newState;
      }
      return GameStateEnum.GamePlay;
    }

    public override void render(GameTime gameTime)
    {
      currentState.render(gameTime);
    }

    public override void update(GameTime gameTime)
    {
      systemKeyboardInput.update(gameTime.ElapsedGameTime);
      if (currentState == playState && player != null && !entities.ContainsKey(player.id))
      {
        currentState = lostState;
      }
      currentState.update(gameTime);
    }

    public void exit(GameTime gameTime, float value)
    {
      score = 0;
      timer = 1000;
      joined = false;
      player = null;
      MessageQueueClient.instance.sendMessage(new Leave());
      currentState = tutorialState;
      nextState = GameStateEnum.MainMenu;
    }

    public void proceed(GameTime gameTime, float value)
    {
      if (currentState == tutorialState)
      {
        currentState = playState;
        MessageQueueClient.instance.sendMessage(new Join());
      }
      if (currentState == lostState)
      {
        exit(gameTime, value);
      }
    }

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
    private class TutorialState : GamePlayState
    {
      private GamePlayView parent;

      public TutorialState(GamePlayView parent) {
        this.parent = parent;
      }

      public void render(GameTime gametime)
      {
        parent.spriteBatch.Begin();
        Vector2 measure = parent.mainFont.MeasureString("Welcome to");

        parent.spriteBatch.DrawString(
          parent.mainFont, 
          "Welcome to", 
          new Vector2(
            parent.graphics.PreferredBackBufferWidth / 2 - measure.X / 2, 
            parent.graphics.PreferredBackBufferHeight / 2 - measure.Y * 2), 
          Color.Black);

        measure = parent.mainFont.MeasureString("Competitive Snake");
        
        parent.spriteBatch.DrawString(
          parent.mainFont, 
          "Competitive Snake", 
          new Vector2(
            parent.graphics.PreferredBackBufferWidth / 2 - measure.X / 2, 
            parent.graphics.PreferredBackBufferHeight / 2 - measure.Y), 
          Color.Black);

        parent.spriteBatch.End();
      }

      public void update(GameTime gameTime)
      {
      }
    }
    private class PlayState : GamePlayState
    {
      private GamePlayView parent;

      public PlayState(GamePlayView parent)
      {
        this.parent = parent;
      }

      public void render(GameTime gametime)
      {

      }

      public void update(GameTime gameTime)
      {
      }
    }
    private class LostState : GamePlayState
    {
      private GamePlayView parent;

      public LostState(GamePlayView parent)
      {
        this.parent = parent;
      }

      public void render(GameTime gametime)
      {
        parent.spriteBatch.Begin();
        Vector2 measure = parent.mainFont.MeasureString("You Died!");

        parent.spriteBatch.DrawString(
          parent.mainFont,
          "You Died!",
          new Vector2(
            parent.graphics.PreferredBackBufferWidth / 2 - measure.X / 2,
            parent.graphics.PreferredBackBufferHeight / 2 - measure.Y * 2),
          Color.Red);

        measure = parent.mainFont.MeasureString(String.Format("HighScores: {0}", parent.score));

        parent.spriteBatch.DrawString(
          parent.mainFont,
          String.Format("HighScores: {0}", parent.score),
          new Vector2(
            parent.graphics.PreferredBackBufferWidth / 2 - measure.X / 2,
            parent.graphics.PreferredBackBufferHeight / 2 - measure.Y),
          Color.Black);

        parent.spriteBatch.End();
      }

      public void update(GameTime gameTime)
      {
       
      }
    }
  }
}

public interface GamePlayState
{
  public void render(GameTime gameTime);
  public void update(GameTime gameTime);
}