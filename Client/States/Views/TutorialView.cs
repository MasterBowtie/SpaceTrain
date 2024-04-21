using System;
using System.Collections.Generic;
using Client;
using Client.States.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shared;
using Shared.Messages;

namespace apedaile
{
  public class TutorialView : GameView
  {

    private SpriteFont titleFont;
    private SpriteFont mainFont;
    private Texture2D textBack;
    private KeyboardInput keyboard;
    private DrawText draw;

    private GameViewEnum nextView = GameViewEnum.Tutorial;
    private GameState startState;
    private GameState inputState;
    private GameState currentState;
    public TutorialView() { }

    public override void loadContent(ContentManager contentManager)
    {
      titleFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime32");
      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime16");
      textBack = contentManager.Load<Texture2D>("Textures/menu");

      startState = new StartState(this);
      inputState = new InputState(this);

      currentState = startState;
    }

    public override void setupDraw(DrawText draw)
    {
      this.draw = draw;
    }

    public override void setupInput(KeyboardInput keyboard)
    {
      this.keyboard = keyboard;

      keyboard.registerCommand(Keys.Escape, true, new IInputDevice.CommandDelegate(exit), GameViewEnum.Tutorial, Shared.Components.Input.Type.Exit);
      keyboard.registerCommand(Keys.Enter, true, new IInputDevice.CommandDelegate(proceed), GameViewEnum.Tutorial, Shared.Components.Input.Type.Select);
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
      Dictionary<Shared.Components.Input.Type, KeyboardInput.CommandEntry> controls;
      public StartState(TutorialView parent)
      {
        this.parent = parent;
        controls = parent.keyboard.getStateCommands()[GameViewEnum.GamePlay];
      }

      public void render(GameTime gameTime)
      {
        parent.spriteBatch.Begin();
        Vector2 measure = parent.titleFont.MeasureString("Welcome to Space Train");

        parent.spriteBatch.Draw(parent.textBack, new Rectangle(
          (int)(parent.graphics.PreferredBackBufferWidth / 2 - measure.X / 2 - 10),
          (int)(parent.graphics.PreferredBackBufferHeight * 0.2f - 10),
          (int)measure.X + 20,
          (int)measure.Y * 5 + 20),
          Color.White
          );
        parent.spriteBatch.End();

        String message = "Welcome to Space Train";

        float bottom = parent.draw.drawCentered(parent.titleFont,
          message,
          parent.graphics.PreferredBackBufferHeight * 0.2f,
          parent.graphics.PreferredBackBufferWidth / 2 - measure.X / 2,
          measure.X,
          false
        );
        bottom += parent.graphics.PreferredBackBufferHeight * 0.1f;
        message = "Collect the green orbs to earn points";
        bottom = parent.draw.drawCentered(parent.mainFont,
          message,
          bottom,
          parent.graphics.PreferredBackBufferWidth / 2 - measure.X / 2,
          measure.X,
          false
          );

        message = "5 pts will add a container to your train";
        bottom = parent.draw.drawCentered(parent.mainFont,
          message,
          bottom,
          parent.graphics.PreferredBackBufferWidth / 2 - measure.X / 2,
          measure.X,
          false
         );

        message = "Avoid hitting other trains";
        bottom = parent.draw.drawCentered(parent.mainFont,
          message,
          bottom,
          parent.graphics.PreferredBackBufferWidth / 2 - measure.X / 2,
          measure.X,
          false
        );

        message = String.Format("Move using: {0}, {1}, {2}, {3}",
          controls[Shared.Components.Input.Type.Up].key,
          controls[Shared.Components.Input.Type.Down].key,
          controls[Shared.Components.Input.Type.Right].key,
          controls[Shared.Components.Input.Type.Left].key);
        bottom = parent.draw.drawCentered(parent.mainFont,
          message,
          bottom,
          parent.graphics.PreferredBackBufferWidth / 2 - measure.X / 2,
          measure.X,
          false
        );

        
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
        input = string.Format("Player {0}", (int)(new MyRandom().nextRange(0, 9999)));
      }

      public void render(GameTime gameTime)
      {
        Vector2 measure = parent.titleFont.MeasureString("000000000000000000000|");

        //input = " ";
        parent.spriteBatch.Begin();

        String message = " Player Name:";
        float bottom = parent.draw.drawLeft(parent.titleFont,
          message,
          parent.graphics.PreferredBackBufferHeight / 2 - 3 * measure.Y / 2,
          parent.graphics.PreferredBackBufferWidth / 2 - measure.X / 2,
          measure.X,
          false
          );
        message = " " + input + "";
        if (visible)
        {
          message += "|";
        }
        bottom = parent.draw.drawLeft(parent.titleFont,
          message,
          parent.graphics.PreferredBackBufferHeight / 2 - measure.Y / 2,
          parent.graphics.PreferredBackBufferWidth / 2 - measure.X / 2,
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
              else if (key == Keys.Back && input.Length > 0)
              {
                input = this.input.Remove(input.Length - 1);
              }
              else if (key == Keys.Enter)
              {
                MessageQueueClient.instance.sendMessage(new Join(input));
                parent.startGame();
                parent.currentState = parent.startState;
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
