
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace apedaile {
  public class KeyboardInput: IInputDevice {
    private KeyboardState previousState;

    private Dictionary<GameViewEnum, Dictionary<Actions, CommandEntry>> stateCommands = new Dictionary<GameViewEnum, Dictionary<Actions, CommandEntry>>(); 

    public struct CommandEntry {
      public Keys key;
      public bool keyPressOnly;
      public IInputDevice.CommandDelegate callback;
      public Actions action;

      public CommandEntry(Keys key, bool keyPressOnly, IInputDevice.CommandDelegate callback, Actions action) {
        this.key = key;
        this.keyPressOnly = keyPressOnly;
        this.callback = callback;
        this.action = action;
      }
    }
    
    public Dictionary<GameViewEnum, Dictionary<Actions, CommandEntry>> getStateCommands() {
      return stateCommands;
    }

    public void registerCommand(Keys key, bool keyPressOnly, IInputDevice.CommandDelegate callback, GameViewEnum state, Actions action) {
      
      // This will check for duplicate keys and switch with the incoming key otherwise just update incoming values
      if (stateCommands.ContainsKey(state) && stateCommands[state].ContainsKey(action)) {
        var commandEntries = stateCommands[state];
        var currentEntry = stateCommands[state][action];
        foreach (var item in commandEntries) {
          if (item.Value.key == key) {
            Keys switchKey = item.Value.key;
            CommandEntry current = (CommandEntry) item.Value;
            current.key = currentEntry.key;
            stateCommands[state][item.Key] = current;
          }
        }
        currentEntry.key = key;
        currentEntry.keyPressOnly = keyPressOnly;
        currentEntry.callback = callback;
        stateCommands[state][action] = currentEntry;
      }

      // This is check for duplicate keys and none the old key, creates new CommandEntry
      else if (stateCommands.ContainsKey(state)) {
        var commandEntries = stateCommands[state];
        foreach (var item in commandEntries) {
          if (item.Value.key == key) {
            CommandEntry current = (CommandEntry) item.Value;
            current.key = Keys.None;
            stateCommands[state][item.Key] = current;
          }
        }
        stateCommands[state].Add(action, new CommandEntry(key, keyPressOnly, callback, action));
      }

      // Create new state Dict and CommandEntry
      else {
        stateCommands.Add(state, new Dictionary<Actions, CommandEntry>());
        stateCommands[state].Add(action, new CommandEntry(key, keyPressOnly, callback, action));
      }
    }

    public void Update(GameTime gameTime, GameViewEnum state) {
      KeyboardState keyState = Keyboard.GetState();
      if (state == GameViewEnum.Exit) {
        return;
      }
      Dictionary<Actions,CommandEntry> commandEntries = stateCommands[state];
        foreach (CommandEntry entry in commandEntries.Values){
          if (entry.keyPressOnly && keyPressed(entry.key)) {
            entry.callback(gameTime, 1.0f);
          }
          else if (!entry.keyPressOnly && keyState.IsKeyDown(entry.key)) {
            entry.callback(gameTime, 1.0f);
          }
        }
        previousState = keyState;
    }

    private bool keyPressed(Keys key) {
      return (Keyboard.GetState().IsKeyDown(key) && !previousState.IsKeyDown(key));
    }
  }
}