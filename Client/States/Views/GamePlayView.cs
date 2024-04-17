
using System;
using System.Collections.Generic;
using System.Timers;
using Client;
using Client.Components;
using Client.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shared.Components;
using Shared.Entities;
using Shared.Messages;

namespace apedaile
{
  public class GamePlayView : GameStateView
  {

    private SpriteFont mainFont;

    private ContentManager contentManager;
    private Dictionary<uint, Entity> entities;
    
    
    private Client.Systems.KeyboardInput systemKeyboardInput;
    private Client.Systems.Network systemNetwork;
    private Client.Systems.Interpolation systemInterpolation;
    private Shared.Systems.Movement moveSystem;


    private Client.Systems.PlayerRenderer playerRenderer;
    private Client.Systems.TileRenderer tileRenderer;
    private Client.Systems.FoodRenderer foodRenderer;
    private HashSet<Keys> previouslyDown = new HashSet<Keys>();
    private int score = 0;
    private float timer = 1000;
    private bool joined = false;
    private uint playerId;
    private float moveRate;


    private GameStateEnum nextState = GameStateEnum.GamePlay;

    public override void setupInput(KeyboardInput keyboard)
    {
      keyboard.registerCommand(Keys.Escape, true, new IInputDevice.CommandDelegate(exit), GameStateEnum.GamePlay, Actions.exit);
      
      systemKeyboardInput = new Client.Systems.KeyboardInput(new List<Tuple<Shared.Components.Input.Type, Keys>>
      {
        Tuple.Create(Shared.Components.Input.Type.Up, Keys.W),
        Tuple.Create(Shared.Components.Input.Type.Left, Keys.A),
        Tuple.Create(Shared.Components.Input.Type.Right, Keys.D),
        Tuple.Create(Shared.Components.Input.Type.Down, Keys.S),
      });
    }

    public void setupNetwork(Client.Systems.Network network, Client.Systems.Interpolation interpolation, Shared.Systems.Movement move)
    {
      systemNetwork = network;
      systemInterpolation = interpolation;
      moveSystem = move;

      entities = new Dictionary<uint, Entity>();
      systemNetwork.registerHandler(Shared.Messages.Type.NewEntity, (TimeSpan elapsedTime, Message message) =>
      {
        handleNewEntity((NewEntity)message);
      });

      systemNetwork.registerHandler(Shared.Messages.Type.RemoveEntity, (TimeSpan elapsedTime, Message message) =>
      {
        handleRemoveEntity((RemoveEntity)message);
      });
    }

    public override void loadContent(ContentManager contentManager)
    {
      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime32");
      playerRenderer = new Client.Systems.PlayerRenderer(graphics);
      foodRenderer = new Client.Systems.FoodRenderer(graphics);

      tileRenderer = new Client.Systems.TileRenderer(graphics);
      Texture2D wallTexture = contentManager.Load<Texture2D>("Textures/wall");
      Texture2D tileTexture = contentManager.Load<Texture2D>("Textures/tile");

      int size = 50;
      for (int x = 0; x < size; x++)
      {
        for (int y = 0; y < size; y++)
        {
          Entity tile = Tile.create(new Vector2(x * 100, y * 100), 100);
          if (x == 0 || y == 0 || x == size-1 || y == size-1)
          {
            tile.add(new Sprite(wallTexture));
          }
          else
          {
            tile.add(new Sprite(tileTexture));
          }
          tileRenderer.add(tile);
        }
      }

      this.contentManager = contentManager;
    }

    public override GameStateEnum processInput(GameTime gameTime)
    {
      foreach (var key in previouslyDown)
      {
        if (Keyboard.GetState().IsKeyUp(key))
        {
          signalKeyReleased(key);
          previouslyDown.Remove(key);
        }
      }

      foreach (var key in Keyboard.GetState().GetPressedKeys())
      {
        if (!previouslyDown.Contains(key))
        {
          signalKeyPressed(key);
          previouslyDown.Add(key);
        }
      }

      if (nextState != GameStateEnum.GamePlay)
      {
        GameStateEnum newState = nextState;
        nextState = GameStateEnum.GamePlay;
        return newState;
      }
      return GameStateEnum.GamePlay;
    }

    public override void render(GameTime gameTime)
    {
      Entity? player = null;
      if (entities.ContainsKey(playerId))
      {
       player = entities[playerId];
      }

      tileRenderer.update(gameTime, spriteBatch, player);
      foodRenderer.update(gameTime, spriteBatch, player);
      playerRenderer.update(gameTime, spriteBatch, player);
    }

    public override void update(GameTime gameTime)
    {
      timer -= gameTime.ElapsedGameTime.Milliseconds;
      if (timer < 0 && !joined)
      {
        MessageQueueClient.instance.sendMessage(new Join());
        joined = true;
      }

      Entity? player = null;
      if (entities.ContainsKey(playerId))
      {
        player = entities[playerId];
      }

      systemKeyboardInput.update(gameTime.ElapsedGameTime);

      if (player != null && !entities.ContainsKey(player.id))
      {
        //nextState = GameStateEnum.Lose;
      }
    }

    public void exit(GameTime gameTime, float value)
    {
      score = 0;
      timer = 1000;
      joined = false;
      MessageQueueClient.instance.sendMessage(new Leave());
      nextState = GameStateEnum.MainMenu;
    }  

    public void signalKeyPressed(Keys key)
    {
      systemKeyboardInput.keyPressed(key);
    }

    public void signalKeyReleased(Keys key)
    {
      systemKeyboardInput.keyReleased(key);
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
        Texture2D texture = contentManager.Load<Texture2D>(message.texture);
        int subImageWidth = message.subImageWidth;
        int[] spriteTime = message.spriteTime;
        entity.add(new A_Sprite(texture, subImageWidth, spriteTime));
      }

      if (message.hasAppearance)
      {
        Texture2D texture = contentManager.Load<Texture2D>(message.texture);
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
        // The client seems to be moving slower than the server
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
        playerId = entity.id;
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

public interface GamePlayState
{
  public void render(GameTime gameTime);
  public void update(GameTime gameTime);
}