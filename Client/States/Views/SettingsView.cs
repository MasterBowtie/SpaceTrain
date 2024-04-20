using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace apedaile {
  public class SettingsView: GameView {

    private SpriteFont mainFont;
    private SpriteFont titleFont;
    private Storage storage;
    private KeyboardInput keyboard;
    private SaveBinding save;

    private Actions currentSelection = Actions.up;
    private GameViewEnum nextState = GameViewEnum.Settings;
    private SettingState currentState;
    private SettingState select;
    private SettingState rebind;
    private bool waitforKeyRelease = true;
    private float delay = 1000;


    public override void setupInput(KeyboardInput keyboard)
    {
      this.keyboard = keyboard;
      select = new Select(this);
      rebind = new Rebind(this);
      currentState = select;

      keyboard.registerCommand(Keys.Up, waitforKeyRelease, new IInputDevice.CommandDelegate(moveUp), GameViewEnum.Settings, Actions.up);
      keyboard.registerCommand(Keys.Down, waitforKeyRelease, new IInputDevice.CommandDelegate(moveDown), GameViewEnum.Settings, Actions.down);
      keyboard.registerCommand(Keys.Enter, waitforKeyRelease, new IInputDevice.CommandDelegate(selectItem), GameViewEnum.Settings, Actions.select);
      keyboard.registerCommand(Keys.Escape, waitforKeyRelease, new IInputDevice.CommandDelegate(exitState), GameViewEnum.Settings, Actions.exit);
    }

    public void setupExtras(SaveBinding save, Storage storage) {
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
    
    public override void render(GameTime gameTime)
    {
      spriteBatch.Begin();
      // spriteBatch.Draw(background, backRect, Color.White);
      spriteBatch.End();

      currentState.render(gameTime);
    }

    public override void update(GameTime gameTime)
    {
      currentState.update(gameTime);
      if (MediaPlayer.State == MediaState.Stopped) {
        // MediaPlayer.Play(music);
      }
    }

    private float drawMenuItem(SpriteFont font, string text, float y, bool selected) {
      Vector2 stringSize = font.MeasureString(text);
      
      spriteBatch.DrawString(
        font, text, new Vector2(graphics.PreferredBackBufferWidth/2 - stringSize.X/2, y), selected? Color.Yellow : Color.White);

      return y + stringSize.Y;
    }

    public void moveUp(GameTime gameTime, float value) {
      if (currentSelection != Actions.up) {
        currentSelection = currentSelection - 1;
      }
    }

    public void moveDown(GameTime gameTime, float value) {
      if (currentSelection != Actions.exit) {
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

    public void saveBinding(Actions action, Keys key) {
      var commands = keyboard.getStateCommands();
      foreach (GameViewEnum state in commands.Keys) {
        foreach (Actions g_action in commands[state].Keys) {
          if (g_action == action) {
            if (state != GameViewEnum.GamePlay || action == Actions.select || action == Actions.exit) {
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

        parent.drawMenuItem(parent.mainFont, string.Format("Rebinding: {0}", parent.currentSelection), parent.graphics.PreferredBackBufferHeight * .1f, false);
        parent.drawMenuItem(parent.mainFont, "Press Any Key", parent.graphics.PreferredBackBufferHeight/ 2, true);
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
        var attack = parent.keyboard.getStateCommands()[GameViewEnum.GamePlay];
        Vector2 biggest = parent.mainFont.MeasureString(string.Format("Rotate Right: {0}", bindings[Actions.select]));
        int buffer = 30;
        float x = parent.graphics.PreferredBackBufferWidth/2 - biggest.X/2 - buffer/2;
      
        parent.spriteBatch.Begin();

        Vector2 title = parent.titleFont.MeasureString("Settings");

        float bottom = parent.drawMenuItem(parent.titleFont, "Settings", parent.graphics.PreferredBackBufferHeight * .1f, false);

        bottom = parent.drawMenuItem(parent.mainFont, string.Format("Up: {0}", bindings[Actions.up].key), bottom, parent.currentSelection == Actions.up);

        bottom = parent.drawMenuItem(parent.mainFont, string.Format("Down: {0}", bindings[Actions.down].key), bottom, parent.currentSelection == Actions.down);

        // bottom = parent.drawMenuItem(parent.mainFont, string.Format("Attack: {0}", parent.keyboard.getStateCommands()[GameStateEnum.GamePlay][Actions.attack].key), bottom, parent.currentSelection == Actions.attack);

        bottom = parent.drawMenuItem(parent.mainFont, string.Format("Select: {0}", bindings[Actions.select].key), bottom, parent.currentSelection == Actions.select);

        bottom = parent.drawMenuItem(parent.mainFont, string.Format("Exit: {0}", bindings[Actions.exit].key), bottom, parent.currentSelection == Actions.exit);

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