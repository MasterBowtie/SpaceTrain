using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.IO.IsolatedStorage;
using System.Runtime.Serialization.Json;
using apedaile;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Client.States.Views;
using Shared.Messages;
using System;

namespace Client
{
  public class ClientMain : Game
  {
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private IGameView currentState;
    private Dictionary<GameViewEnum, IGameView> states;
    GameModel gameModel;

    private bool loading = false;
    private bool saving = false;
    private ClientStorage storage = null;


    private apedaile.KeyboardInput keyboard;
    private Client.Systems.KeyboardInput systemKeyboardInput;

    private Client.Systems.Network systemNetwork;

    public ClientMain()
    {
      graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "Content";
      IsMouseVisible = true;
    }

    protected override void Initialize()
    {
      //m_graphics.PreferredBackBufferWidth = 1920;
      //m_graphics.PreferredBackBufferHeight = 1080;
      //m_graphics.ApplyChanges();



      systemNetwork = new Client.Systems.Network();

      states = new Dictionary<GameViewEnum, IGameView> {
                {GameViewEnum.MainMenu, new MainMenuView()},
                {GameViewEnum.GamePlay, new GamePlayView()},
                {GameViewEnum.Settings, new SettingsView()},
                {GameViewEnum.HighScores, new HighScoresView()},
                {GameViewEnum.About, new AboutView()},
                {GameViewEnum.Tutorial, new TutorialView()}
            };

      gameModel = new GameModel();
      gameModel.initialize(this.GraphicsDevice, graphics);
      gameModel.setupNetwork(systemNetwork);

      keyboard = new apedaile.KeyboardInput();
      systemKeyboardInput = new Client.Systems.KeyboardInput();
      systemKeyboardInput.addMapping(Shared.Components.Input.Type.Up, Keys.Up);
      systemKeyboardInput.addMapping(Shared.Components.Input.Type.Left, Keys.A);
      systemKeyboardInput.addMapping(Shared.Components.Input.Type.Right, Keys.D);
      systemKeyboardInput.addMapping(Shared.Components.Input.Type.Down, Keys.S);


      loadState();
      if (storage == null) {
        storage = new ClientStorage();
      }
      storage.attachKeyboard(keyboard, systemKeyboardInput);
      storage.attachSave(saveState);

      foreach (var state in states.Values)
      {
        state.initialize(this.GraphicsDevice, graphics);
        state.setupInput(keyboard);
      }
      gameModel.setupInput(keyboard, systemKeyboardInput);


      GamePlayView gpv = (GamePlayView)states[GameViewEnum.GamePlay];
      HighScoresView hsv = (HighScoresView)states[GameViewEnum.HighScores];
      SettingsView sv = (SettingsView)states[GameViewEnum.Settings];

      gpv.setupInput(keyboard, systemKeyboardInput);
      gpv.setupNetwork(systemNetwork);
      gpv.attachModel(gameModel);
      hsv.setupNetwork(systemNetwork);
      sv.setupExtras(saveState, storage);

      currentState = states[GameViewEnum.MainMenu];

      storage.loadCommands();
      base.Initialize();
    }

    protected override void LoadContent()
    {
      spriteBatch = new SpriteBatch(GraphicsDevice);
      gameModel.loadContent(this.Content);

      DrawText drawText = new DrawText(spriteBatch, graphics);
      drawText.loadContent(this.Content);

      foreach (var state in states.Values)
      {
        state.loadContent(this.Content);
        state.setupDraw(drawText);
      }
    }

    protected override void Update(GameTime gameTime)
    {

      GameViewEnum nextStateEnum = currentState.processInput(gameTime);
      keyboard.Update(gameTime, nextStateEnum);

      if (nextStateEnum == GameViewEnum.Exit)
      {
        MessageQueueClient.instance.sendMessage(new Shared.Messages.Disconnect());
        MessageQueueClient.shutdown();
        Exit();
      }
      else
      {
        systemNetwork.update(gameTime.ElapsedGameTime, MessageQueueClient.instance.getMessages());
        gameModel.update(gameTime);
        currentState.update(gameTime);
        currentState = states[nextStateEnum];
      }

      base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(Color.Black);

      gameModel.render(gameTime);
      currentState.render(gameTime);

      base.Draw(gameTime);
    }



    private void saveState()
    {
      lock (this)
      {
        if (!this.saving)
        {
          this.saving = true;
          finalizeSaveAsync(storage);
        }
      }
    }

    private async Task finalizeSaveAsync(ClientStorage state)
    {
      await Task.Run(() =>
      {
        using (IsolatedStorageFile storageFile = IsolatedStorageFile.GetUserStoreForApplication())
        {
          try
          {
            using (IsolatedStorageFileStream fs = storageFile.OpenFile("ClientSnake.json", FileMode.Create))
            {
              if (fs != null)
              {
                DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(ClientStorage));
                mySerializer.WriteObject(fs, state);
              }
            }
          }
          catch (IsolatedStorageException err)
          {
            System.Console.WriteLine("There was an error writing to storage\n{0}", err);
          }
        }

        this.saving = false;
      });
    }

    private void loadState()
    {
      lock (this)
      {
        if (!this.loading)
        {
          this.loading = true;
          var result = finalizeLoadAsync();
          result.Wait();
        }
      }
    }

    private async Task finalizeLoadAsync()
    {
      await Task.Run(() =>
      {
        using (IsolatedStorageFile storageFile = IsolatedStorageFile.GetUserStoreForApplication())
        {
          try
          {
            if (storageFile.FileExists("ClientSnake.json"))
            {
              using (IsolatedStorageFileStream fs = storageFile.OpenFile("ClientSnake.json", FileMode.Open))
              {
                if (fs != null)
                {
                  DataContractJsonSerializer mySerializer = new DataContractJsonSerializer(typeof(ClientStorage));
                  storage = (ClientStorage)mySerializer.ReadObject(fs);
                }
              }
            }
            else
            {
              System.Console.WriteLine("File doesn't exist yet!");
              storage = new ClientStorage();
            }
          }
          catch (IsolatedStorageException err)
          {
            System.Console.WriteLine("Something broke: {0}", err);
          }
        }
        this.loading = false;
      });
    }

  }
}
