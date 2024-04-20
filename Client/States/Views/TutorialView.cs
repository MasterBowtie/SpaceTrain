using System;
using Client;
using Client.States.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shared;
using Shared.Entities;
using Shared.Messages;

namespace apedaile
{
  public class TutorialView : GameView
  {

    private SpriteFont mainFont;
    private KeyboardInput keyboard;
    private DrawText draw;

    private GameViewEnum nextView = GameViewEnum.Tutorial;
    private GameState startState;
    private GameState inputState;
    private GameState currentState;
    public TutorialView() { }

    public override void loadContent(ContentManager contentManager)
    {
      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime32");
      draw = new DrawText(spriteBatch, graphics);
      draw.loadContent(contentManager);

      startState = new StartState(this);
      inputState = new InputState(this);

      currentState = startState;
    }

    public override void setupInput(KeyboardInput keyboard)
    {
      this.keyboard = keyboard;

      keyboard.registerCommand(Keys.Escape, true, new IInputDevice.CommandDelegate(exit), GameViewEnum.Tutorial, Actions.exit);
      keyboard.registerCommand(Keys.Enter, true, new IInputDevice.CommandDelegate(proceed), GameViewEnum.Tutorial, Actions.select);
    }

    public override GameViewEnum processInput(GameTime gameTime)
    {
      if (nextView != GameViewEnum.Tutorial)
      {
        GameViewEnum newState = nextView;
        nextView = GameViewEnum.Tutorial;
        return newState;
      }
      return GameViewEnum.Tutorial;
    }

    public override void render(GameTime gameTime)
    {
      currentState.render(gameTime);
    }

    public override void update(GameTime gameTime)
    {
      currentState.update(gameTime);
    }

    public void exit(GameTime gameTime, float value)
    {
      currentState = startState;
      nextView = GameViewEnum.MainMenu;
    }

    public void proceed(GameTime gameTime, float value)
    {
      if (currentState == startState)
      {
        currentState = inputState;
      }
    }

    public void startGame()
    {
      nextView = GameViewEnum.GamePlay;
    }



    public class StartState : GameState
    {
      TutorialView parent;
      public StartState(TutorialView parent)
      {
        this.parent = parent;
      }

      public void render(GameTime gameTime)
      {
        parent.spriteBatch.Begin();

        Vector2 measure = parent.mainFont.MeasureString("Welcome to Space Train");
        String message = "Welcome to Space Train";

        float bottom = parent.draw.drawCentered(parent.mainFont,
          message,
          parent.graphics.PreferredBackBufferWidth / 2 - measure.X / 2,
          parent.graphics.PreferredBackBufferHeight / 2 - measure.Y / 2,
          measure.X,
          false
          );

        parent.spriteBatch.End();
      }
      public void update(GameTime gameTime)
      {

      }
    }

    public class InputState : GameState
    {
      TutorialView parent;
      string input;
      float timer = 500;
      bool visible = false;
      bool ready = false;
      private KeyboardState previousState;
      public InputState(TutorialView parent)
      {
        this.parent = parent;
        input = string.Format(" Player {0}", (int)(new MyRandom().nextRange(0, 9999)));
      }

      public void render(GameTime gameTime)
      {
        Vector2 measure = parent.mainFont.MeasureString("000000000000000000000|");
        String message = input + "";
        if (visible)
        {
          message += "|";
        }
        //input = " ";
        parent.spriteBatch.Begin();

        float bottom = parent.draw.drawLeft(parent.mainFont,
          message,
          parent.graphics.PreferredBackBufferHeight / 2 - measure.Y/2,
          parent.graphics.PreferredBackBufferWidth / 2 - measure.X/2,
          measure.X,
          true
          );

        parent.spriteBatch.End();
      }
      public void update(GameTime gameTime)
      {
        timer -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
        if (timer < 0)
        {
          ready = true;
          visible = !visible;
          timer += 500;
        }
        var keyState = Keyboard.GetState();
        var keys = keyState.GetPressedKeys();

        bool shift = keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift);

        if (ready)
        {
          foreach (Keys key in keys)
          {
            if (keyPressed(key))
            {
              if ((int)key > 64 && (int)key < 91 && input.Length <= 20)
              {
                var keyValue = key.ToString();
                if (!shift)
                {
                  keyValue = keyValue.ToLower();
                }
                this.input += keyValue;
              }
              else if ((int)key > 47 && (int)key < 58 && input.Length <= 20)
              {
                var keyValue = key.ToString();
                keyValue = keyValue.TrimStart('D');

                this.input += keyValue;
              }
              else if (key == Keys.Space)
              {
                input += " ";
              }
              else if (key == Keys.Back && input.Length > 1)
              {
                input = this.input.Remove(input.Length - 1);
              }
              else if (key == Keys.Enter)
              {
                MessageQueueClient.instance.sendMessage(new Join(input.Remove(0, 1)));
                parent.startGame();
              }
            }
          }
        }
        previousState = keyState;
      }

      private bool keyPressed(Keys key)
      {
        return (Keyboard.GetState().IsKeyDown(key) && !previousState.IsKeyDown(key));
      }
    }

  }

}
