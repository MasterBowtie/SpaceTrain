using System;
using System.Collections.Generic;
using Client.States.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace apedaile {
  public class SettingsView: GameView {

    private SpriteFont mainFont;
    private SpriteFont titleFont;
    private ClientStorage storage;
    private KeyboardInput keyboard;
    private SaveBinding save;

    private Shared.Components.Input.Type currentSelection = Shared.Components.Input.Type.Up;
    private GameViewEnum nextState = GameViewEnum.Settings;
    private SettingState currentState;
    private SettingState select;
    private SettingState rebind;
    private bool waitforKeyRelease = true;
    private float delay = 1000;
    private DrawText draw;


    public override void setupInput(KeyboardInput keyboard)
    {
      this.keyboard = keyboard;
      select = new Select(this);
      rebind = new Rebind(this);
      currentState = select;

      keyboard.registerCommand(Keys.Up, waitforKeyRelease, new IInputDevice.CommandDelegate(moveUp), GameViewEnum.Settings, Shared.Components.Input.Type.Up);
      keyboard.registerCommand(Keys.Down, waitforKeyRelease, new IInputDevice.CommandDelegate(moveDown), GameViewEnum.Settings, Shared.Components.Input.Type.Down);
      keyboard.registerCommand(Keys.Enter, waitforKeyRelease, new IInputDevice.CommandDelegate(selectItem), GameViewEnum.Settings, Shared.Components.Input.Type.Select);
      keyboard.registerCommand(Keys.Escape, waitforKeyRelease, new IInputDevice.CommandDelegate(exitState), GameViewEnum.Settings, Shared.Components.Input.Type.Exit);
    }

    public void setupExtras(SaveBinding save, ClientStorage storage) {
      // this.player = player;
      this.storage = storage;
      this.save = save;
    }

    public override GameViewEnum processInput(GameTime gameTime)
    {
      delay -= gameTime.ElapsedGameTime.Milliseconds;
      if (delay <= 0) {
        currentState.processInput(gameTime);
      }
      if (nextState != GameViewEnum.Settings) {
        nextState = GameViewEnum.Settings;
        return GameViewEnum.MainMenu;
      }
      return GameViewEnum.Settings;
    }

    public override void loadContent(ContentManager contentManager)
    {
      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime32");
      titleFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime64");
    }

    public override void setupDraw(DrawText draw)
    {
      this.draw = draw;
    }
    public override void render(GameTime gameTime)
    {
      currentState.render(gameTime);
    }

    public override void update(GameTime gameTime)
    {
      currentState.update(gameTime);
      if (MediaPlayer.State == MediaState.Stopped) {
        // MediaPlayer.Play(music);
      }
    }

    public void moveUp(GameTime gameTime, float value) {
      if (currentSelection != Shared.Components.Input.Type.Up) {
        currentSelection = currentSelection - 1;
      }
    }

    public void moveDown(GameTime gameTime, float value) {
      if (currentSelection != Shared.Components.Input.Type.Exit) {
        currentSelection = currentSelection + 1;
      }
    }

    public void exitState(GameTime gameTime, float value){
      if (currentState == select) {
        nextState = GameViewEnum.MainMenu;
        delay = 1000;
      } else {
        currentState = rebind;
      }
    }

    public void selectItem(GameTime gameTime, float value) {
      if (currentState == select) {
        currentState = rebind;
        delay = 100;
      } else {
        Keys[] keys = Keyboard.GetState().GetPressedKeys();
        if (keys.Length == 1) {
          saveBinding(
            currentSelection,
            keys[0]);
          currentState = select;
        }
      }
    }

    public void saveBinding(Shared.Components.Input.Type action, Keys key) {
      var commands = keyboard.getStateCommands();
      foreach (GameViewEnum state in commands.Keys) {
        foreach (Shared.Components.Input.Type g_action in commands[state].Keys) {
          if (g_action == action) {
            if (state != GameViewEnum.GamePlay || action == Shared.Components.Input.Type.Select || action == Shared.Components.Input.Type.Exit) {
              storage.registerCommand(key, true, commands[state][action].callback, state, action);
            }
            else {
              storage.registerCommand(key, false, commands[state][action].callback, state, action);
            }
          }
        }
      }
      
      select = new Select(this);
      rebind = new Rebind(this);
    }

    // This is the different states and these could have been a lot cleaner but this is how it goes for now

    protected class Rebind: SettingState {
      private SettingsView parent;

      public Rebind(SettingsView parent) {
        this.parent = parent;
      }

      public void render(GameTime gameTime) {
        // parent.select.render(gameTime);
        parent.spriteBatch.Begin();
        Vector2 biggest = parent.mainFont.MeasureString("Press any key");
        int buffer = 50;
        float x = parent.graphics.PreferredBackBufferWidth/2 - biggest.X/2 - buffer/2;

        String message = string.Format("Rebinding: {0}", parent.currentSelection);
        Vector2 stringSize = parent.mainFont.MeasureString(message);
        float bottom = parent.draw.drawCentered(
          parent.mainFont, 
          message, 
          parent.graphics.PreferredBackBufferHeight * .1f, 
          parent.graphics.PreferredBackBufferWidth/2 + stringSize.X/2,
          stringSize.X,
          false);

        message = " Press Any Key ";
        stringSize = parent.mainFont.MeasureString(message);
        parent.draw.drawCentered(
         parent.mainFont,
         message,
         parent.graphics.PreferredBackBufferHeight/2 - stringSize.Y/2,
         parent.graphics.PreferredBackBufferWidth / 2 - stringSize.X / 2,
         stringSize.X,
         true);
        parent.spriteBatch.End();
      }
      
      public void update(GameTime gameTime) {

      }

      public void processInput(GameTime gameTime) {
        Keys[] keys = Keyboard.GetState().GetPressedKeys();
        if (keys.Length == 1) {
          parent.saveBinding(
            parent.currentSelection,
            keys[0]);
          parent.currentState = parent.select;
        }
      }
    } 

    protected class Select: SettingState {
      private SettingsView parent;

      public Select(SettingsView parent) {
        this.parent = parent;
      }

      public void render(GameTime gameTime) {
        var bindings = parent.keyboard.getStateCommands()[GameViewEnum.Settings];
        var game = parent.keyboard.getStateCommands()[GameViewEnum.GamePlay];
        Vector2 biggest = parent.mainFont.MeasureString(string.Format(" Select: {0} ", bindings[Shared.Components.Input.Type.Select].key));
        int buffer = 30;
        float x = parent.graphics.PreferredBackBufferWidth/2 - biggest.X/2 - buffer/2;
      
        parent.spriteBatch.Begin();

        Vector2 title = parent.titleFont.MeasureString("Settings");

        float bottom = parent.draw.drawCentered(parent.titleFont, "Settings", parent.graphics.PreferredBackBufferHeight * .1f, parent.graphics.PreferredBackBufferWidth/2 - biggest.X/2, biggest.X,  false);

        bottom = parent.draw.drawCentered(parent.mainFont, string.Format("Up: {0}", bindings[Shared.Components.Input.Type.Up].key), bottom, parent.graphics.PreferredBackBufferWidth / 2 - biggest.X / 2, biggest.X, parent.currentSelection == Shared.Components.Input.Type.Up);
        bottom = parent.draw.drawCentered(parent.mainFont, string.Format("Down: {0}", bindings[Shared.Components.Input.Type.Down].key), bottom, parent.graphics.PreferredBackBufferWidth / 2 - biggest.X / 2, biggest.X, parent.currentSelection == Shared.Components.Input.Type.Down);
        bottom = parent.draw.drawCentered(parent.mainFont, string.Format("Left: {0}", game[Shared.Components.Input.Type.Left].key), bottom, parent.graphics.PreferredBackBufferWidth / 2 - biggest.X / 2, biggest.X, parent.currentSelection == Shared.Components.Input.Type.Left);
        bottom = parent.draw.drawCentered(parent.mainFont, string.Format("Right: {0}", game[Shared.Components.Input.Type.Right].key), bottom, parent.graphics.PreferredBackBufferWidth / 2 - biggest.X / 2, biggest.X, parent.currentSelection == Shared.Components.Input.Type.Right);
        bottom = parent.draw.drawCentered(parent.mainFont, string.Format("Select: {0}", bindings[Shared.Components.Input.Type.Select].key), bottom, parent.graphics.PreferredBackBufferWidth / 2 - biggest.X / 2, biggest.X, parent.currentSelection == Shared.Components.Input.Type.Select);
        bottom = parent.draw.drawCentered(parent.mainFont, string.Format("Exit: {0}", bindings[Shared.Components.Input.Type.Exit].key), bottom, parent.graphics.PreferredBackBufferWidth / 2 - biggest.X / 2, biggest.X, parent.currentSelection == Shared.Components.Input.Type.Exit);


        parent.spriteBatch.End();
      }

      public void update(GameTime gameTime) {
        // no updates here
      }

      public void processInput(GameTime gameTime) {
      }
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

  public interface SettingState {
      public void render(GameTime gameTime);
      public void update(GameTime gameTime);
      public void processInput(GameTime gameTime);
  }

  public delegate void SaveBinding();
}