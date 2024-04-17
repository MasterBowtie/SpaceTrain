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
      //IsFixedTimeStep = false;

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

      currentState = states[GameStateEnum.MainMenu];
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
      if (nextStateEnum == GameStateEnum.GamePlay && currentState != states[GameStateEnum.GamePlay])
      {
        GamePlayView gs = (GamePlayView)states[GameStateEnum.GamePlay];
        gs.beginConnection();
      }
      if (nextStateEnum != GameStateEnum.GamePlay && currentState == states[GameStateEnum.GamePlay])
      {
        GamePlayView gs = (GamePlayView)states[GameStateEnum.GamePlay];
        gs.endConnection();
      }
      if (nextStateEnum == GameStateEnum.Exit)
      {
        Exit();
      }
      else
      {

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
