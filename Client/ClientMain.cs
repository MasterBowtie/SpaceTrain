using System;
using System.Collections.Generic;
using apedaile;
using Client.Components;
using Client.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shared.Components;
using Shared.Entities;
using Shared.Messages;

namespace Client
{
  public class ClientMain : Game
  {
    private GraphicsDeviceManager graphics;
    private SpriteBatch spriteBatch;
    private IGameState currentState;
    private Dictionary<GameStateEnum, IGameState> states;
    private apedaile.KeyboardInput keyboard;
    private Client.Systems.KeyboardInput systemKeyboardInput;

    private Client.Systems.Network systemNetwork;
    private Client.Systems.Interpolation systemInterpolation = new Client.Systems.Interpolation();
    private Shared.Systems.Movement moveSystem = new Shared.Systems.Movement();
    private Client.Systems.ParticleSystem particleSystem;
    private Dictionary<uint, Entity> entities;
    private float moveRate;

    private Client.Systems.PlayerRenderer playerRenderer;
    private Client.Systems.TileRenderer tileRenderer;
    private Client.Systems.FoodRenderer foodRenderer;
    private Client.Systems.ParticleSystemRenderer particleRenderer;

    public Entity player;


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


      entities = new Dictionary<uint, Entity>();
      systemNetwork = new Client.Systems.Network();

      systemNetwork.registerHandler(Shared.Messages.Type.NewEntity, (TimeSpan elapsedTime, Message message) =>
      {
        handleNewEntity((NewEntity)message);
      });

      systemNetwork.registerHandler(Shared.Messages.Type.RemoveEntity, (TimeSpan elapsedTime, Message message) =>
      {
        handleRemoveEntity((RemoveEntity)message);
      });

      states = new Dictionary<GameStateEnum, IGameState> {
                {GameStateEnum.MainMenu, new MainMenuView()},
                {GameStateEnum.GamePlay, new GamePlayView()},
                {GameStateEnum.Settings, new SettingsView()},
                {GameStateEnum.HighScores, new HighScoresView()},
                {GameStateEnum.About, new AboutView()}
            };

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

      GamePlayView gpv = (GamePlayView)states[GameStateEnum.GamePlay];
      HighScoresView hsv = (HighScoresView)states[GameStateEnum.HighScores];
      
      gpv.setupInput(keyboard, systemKeyboardInput);
      gpv.setupNetwork(systemNetwork);
      hsv.setupNetwork(systemNetwork);

      currentState = states[GameStateEnum.MainMenu];

      MessageQueueClient.shutdown();
      MessageQueueClient.instance.initialize("localhost", 3000);
      base.Initialize();
    }

    protected override void LoadContent()
    {
      spriteBatch = new SpriteBatch(GraphicsDevice);

      playerRenderer = new Client.Systems.PlayerRenderer(graphics);
      foodRenderer = new Client.Systems.FoodRenderer(graphics);
      particleRenderer = new ParticleSystemRenderer(graphics);
      particleSystem = new ParticleSystem();
      particleSystem.loadContent(this.Content, particleRenderer);

      tileRenderer = new Client.Systems.TileRenderer(graphics);
      tileRenderer.loadContent(Content);

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
        MessageQueueClient.shutdown();
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

      particleSystem.update(gameTime.ElapsedGameTime);

      base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
      GraphicsDevice.Clear(Color.CornflowerBlue);

      if (player != null)
      {

      }

      tileRenderer.update(gameTime, spriteBatch, player);
      foodRenderer.update(gameTime, spriteBatch, player);
      playerRenderer.update(gameTime, spriteBatch, player);
      particleRenderer.update(gameTime, spriteBatch, player);

      currentState.render(gameTime);

      base.Draw(gameTime);
    }

    /// <summary>
    /// Based upon an Entity received from the server, create the
    /// entity at the client.
    /// </summary>
    private Entity createEntity(Shared.Messages.NewEntity message)
    {
      Entity entity = new Entity(message.id);

      if (message.hasHead)
      {
        entity.add(new Head(message.head));
      }

      if (message.hasTurnPoint)
      {
        entity.add(new Shared.Components.TurnPoint());
      }

      if (message.hasAApperance)
      {
        Texture2D texture = this.Content.Load<Texture2D>(message.texture);
        int subImageWidth = message.subImageWidth;
        int[] spriteTime = message.spriteTime;
        entity.add(new A_Sprite(texture, subImageWidth, spriteTime));
      }

      if (message.hasAppearance)
      {
        Texture2D texture = this.Content.Load<Texture2D>(message.texture);
        entity.add(new Sprite(texture));
      }

      if (message.hasPosition)
      {
        entity.add(new Position(message.position, message.orientation));
      }

      if (message.hasSize)
      {
        entity.add(new Size(message.size));
      }

      if (message.hasMovement)
      {
        moveRate = message.moveRate;
        entity.add(new Movement(message.moveRate, message.rotateRate));
      }

      if (message.hasInput)
      {
        entity.add(new Shared.Components.Input(message.inputs));
      }

      if (message.hasConnected)
      {
        Entity? leads = null;
        Entity? follows = null;

        if (message.hasLead && entities.ContainsKey(message.leads))
        {
          leads = entities[message.leads];
          if (leads.get<Connected>().follows == null)
          {
            var connected = leads.get<Connected>();
            connected.follows = entity;
          }
        }

        if (message.hasFollow && entities.ContainsKey(message.follows))
        {
          follows = entities[message.follows];
          if (follows.get<Connected>().leads == null)
          {
            var connected = follows.get<Connected>();
            connected.leads = entity;
          }
          entity.add(new Shared.Components.Path(follows.get<Position>(), moveRate));
        }
        entity.add(new Connected(leads, follows));
      }

      return entity;
    }

    /// <summary>
    /// As entities are added to the game model, they are run by the systems
    /// to see if they are interested in knowing about them during their
    /// updates.
    /// </summary>
    private void addEntity(Entity entity)
    {
      if (entity == null)
      {
        return;
      }

      if (entity.contains<Shared.Components.Input>())
      {
        player = entity;
      }
      entities[entity.id] = entity;
      systemKeyboardInput.add(entity);
      playerRenderer.add(entity);
      foodRenderer.add(entity);
      systemNetwork.add(entity);
      systemInterpolation.add(entity);
      moveSystem.add(entity);
    }

    /// <summary>
    /// All entity lists for the systems must be given a chance to remove
    /// the entity.
    /// </summary>
    private void removeEntity(uint id)
    {
      if (entities.ContainsKey(id))
      {
        Entity entity = entities[id];
        particleSystem.explode(entity);
      }

      entities.Remove(id);
      systemKeyboardInput.remove(id);
      systemNetwork.remove(id);
      playerRenderer.remove(id);
      foodRenderer.remove(id);
      systemInterpolation.remove(id);
      moveSystem.remove(id);
    }

    private void handleNewEntity(NewEntity message)
    {
      Entity entity = createEntity(message);
      addEntity(entity);
    }

    /// <summary>
    /// Handler for the RemoveEntity message.  It removes the entity from
    /// the client game model (that's us!).
    /// </summary>
    private void handleRemoveEntity(RemoveEntity message)
    {
      removeEntity(message.id);
    }
  }
}
