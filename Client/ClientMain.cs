using System;
using System.Collections.Generic;
using apedaile;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Client
{
  public class ClientMain : Game
  {
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private IGameView currentState;
    private Dictionary<GameViewEnum, IGameView> states;
    GameModel gameModel;


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
      systemKeyboardInput = new Client.Systems.KeyboardInput(new List<Tuple<Shared.Components.Input.Type, Keys>>
      {
        Tuple.Create(Shared.Components.Input.Type.Up, Keys.W),
        Tuple.Create(Shared.Components.Input.Type.Left, Keys.A),
        Tuple.Create(Shared.Components.Input.Type.Right, Keys.D),
        Tuple.Create(Shared.Components.Input.Type.Down, Keys.S),
      });

      foreach (var state in states.Values)
      {
        state.initialize(this.GraphicsDevice, graphics);
        state.setupInput(keyboard);
      }
      gameModel.setupInput(keyboard, systemKeyboardInput);


      GamePlayView gpv = (GamePlayView)states[GameViewEnum.GamePlay];
      HighScoresView hsv = (HighScoresView)states[GameViewEnum.HighScores];

      gpv.setupInput(keyboard, systemKeyboardInput);
      gpv.setupNetwork(systemNetwork);
      gpv.attachModel(gameModel);
      hsv.setupNetwork(systemNetwork);

      currentState = states[GameViewEnum.MainMenu];

      MessageQueueClient.shutdown();
      MessageQueueClient.instance.initialize("localhost", 3000);
      base.Initialize();
    }

    protected override void LoadContent()
    {
      spriteBatch = new SpriteBatch(GraphicsDevice);
      gameModel.loadContent(this.Content);

      foreach (var state in states.Values)
      {
        state.loadContent(this.Content);
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
      GraphicsDevice.Clear(Color.CornflowerBlue);

      gameModel.render(gameTime);
      currentState.render(gameTime);

      base.Draw(gameTime);
    }


  }
}
