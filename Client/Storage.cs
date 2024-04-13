using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Input;

namespace apedaile {
  [DataContract(Name = "Storage")]
  public class Storage {

    private KeyboardInput keyboard;

    public Storage() {
    }

    [DataMember()]
    private Dictionary<string, Dictionary<string, CommandString>> bindings = new Dictionary<string, Dictionary<string, CommandString>>();

    private SaveBinding save;

    public void attachSave(SaveBinding save) {
      this.save = save;
    }

    public struct CommandString {
      public string key;
      public bool keyPressOnly;

      public CommandString(Keys key, bool keyPressOnly) {
        this.key = key.ToString();
        this.keyPressOnly = keyPressOnly;
      }
    }

    public Dictionary<string, Dictionary<string, CommandString>> getBindings() {
      return bindings;
    }

    public void registerCommand(Keys key, bool keyPressOnly, IInputDevice.CommandDelegate callback, GameStateEnum state, Actions action) {
      keyboard.registerCommand(key, keyPressOnly, callback, state, action);
      if (bindings.ContainsKey(state.ToString())) {
        if (bindings[state.ToString()].ContainsKey(action.ToString())) {
          bindings[state.ToString()][action.ToString()] = new CommandString(key, keyPressOnly);
        }
        else {
          bindings[state.ToString()].Add(action.ToString(), new CommandString(key, keyPressOnly));
        }
      } 
      else {
        bindings.Add(state.ToString(), new Dictionary<string, CommandString>());
        bindings[state.ToString()].Add(action.ToString(), new CommandString(key, keyPressOnly));
      }
      save();
    }

    public void attachKeyboard(KeyboardInput keyboard) {
      this.keyboard = keyboard;
    }

    public delegate void SaveBinding();

    // This is overly complicated...
    public void loadCommands() {
      var stateCommands = keyboard.getStateCommands();
      if (bindings.Count == 0) {
        foreach (GameStateEnum state in stateCommands.Keys) {
            foreach (Actions action in stateCommands[state].Keys) {
              KeyboardInput.CommandEntry entry = stateCommands[state][action];
              registerCommand(
                entry.key,
                entry.keyPressOnly,
                entry.callback,
                state,
                action
              );
            }
        }
      } else {
      foreach (GameStateEnum state in stateCommands.Keys) {
        if (bindings.ContainsKey(state.ToString())) {
          var stateBindings = bindings[state.ToString()];
          foreach (Actions action in stateCommands[state].Keys) {
            if (stateBindings.ContainsKey(action.ToString())) {
              foreach (Keys key in Enum.GetValues(typeof(Keys))) {
                if (stateBindings[action.ToString()].key == key.ToString()) {
                  KeyboardInput.CommandEntry commandEntry = stateCommands[state][action];
                  registerCommand(key, commandEntry.keyPressOnly, commandEntry.callback, state, action);
                }
              }
            } else {
              KeyboardInput.CommandEntry commandEntry = stateCommands[state][action];
              registerCommand(commandEntry.key, commandEntry.keyPressOnly, commandEntry.callback, state, action);
            }
          }
        } else {
          foreach (Actions action in stateCommands[state].Keys) {
            KeyboardInput.CommandEntry commandEntry = stateCommands[state][action];
            registerCommand(commandEntry.key, commandEntry.keyPressOnly, commandEntry.callback, state, action);
          }
        }
      }
    }
    }
  }
}
