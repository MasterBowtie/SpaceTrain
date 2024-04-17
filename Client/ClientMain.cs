using System;
using System.Collections.Generic;
using apedaile;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client
{
  public class ClientMain : Game
  {
    private GraphicsDeviceManager m_graphics;
    private SpriteBatch m_spriteBatch;
    private IGameState currentState;
    private Dictionary<GameStateEnum, IGameState> states;
    private KeyboardInput keyboard;
    private Client.Systems.Network systemNetwork;
    private Client.Systems.Interpolation systemInterpolation = new Client.Systems.Interpolation();
    private Shared.Systems.Movement moveSystem = new Shared.Systems.Movement();


    public ClientMain()
    {
      m_graphics = new GraphicsDeviceManager(this);
      Content.RootDirectory = "Content";
      IsMouseVisible = true;
    }

    protected override void Initialize()
    {
      // m_graphics.PreferredBackBufferWidth = 1920;
      // m_graphics.PreferredBackBufferHeight = 1080;
      // m_graphics.ApplyChanges();


      systemNetwork = new Client.Systems.Network();
      states = new Dictionary<GameStateEnum, IGameState> {
                {GameStateEnum.MainMenu, new MainMenuView()},
                {GameStateEnum.GamePlay, new GamePlayView()},
                {GameStateEnum.Settings, new SettingsView()},
                {GameStateEnum.HighScores, new HighScoresView()},
                {GameStateEnum.About, new AboutView()},
                {GameStateEnum.Tutorial, new TutorialView()}
            };

      keyboard = new KeyboardInput();

      foreach (var state in states.Values)
      {
        state.initialize(this.GraphicsDevice, m_graphics);
        state.setupInput(keyboard);
      }

      GamePlayView gs = (GamePlayView)states[GameStateEnum.GamePlay];
      gs.setupNetwork(systemNetwork, systemInterpolation, moveSystem);

      currentState = states[GameStateEnum.MainMenu];

      MessageQueueClient.shutdown();
      MessageQueueClient.instance.initialize("localhost", 3000);
      base.Initialize();
    }

    protected override void LoadContent()
    {
      m_spriteBatch = new SpriteBatch(GraphicsDevice);

      foreach (var state in states.Values)
      {
        state.loadContent(this.Content);
      }
    }

    protected override void Update(GameTime gameTime)
    {

      GameStateEnum nextStateEnum = currentState.processInput(gameTime);
      keyboard.Update(gameTime, nextStateEnum);
      if (nextStateEnum == GameStateEnum.Exit)
      {
        MessageQueueClient.instance.sendMessage(new Shared.Messages.Disconnect());
        Exit();
      }
      else
      {
        systemNetwork.update(gameTime.ElapsedGameTime, MessageQueueClient.instance.getMessages());
        systemInterpolation.update(gameTime.ElapsedGameTime);
        moveSystem.update(gameTime.ElapsedGameTime);
        currentState.update(gameTime);
        currentState = states[nextStateEnum];
      }

      base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(Color.CornflowerBlue);
      currentState.render(gameTime);

      base.Draw(gameTime);

    }
  }
}
