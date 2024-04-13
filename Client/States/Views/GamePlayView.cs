
using System;
using System.Collections.Generic;
using System.Timers;
using Client;
using Client.Components;
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
    private Client.Systems.Network systemNetwork;
    private Client.Systems.KeyboardInput systemKeyboardInput;
    private Client.Systems.Interpolation systemInterpolation;
    private Client.Systems.PlayerRenderer playerRenderer;
    private Client.Systems.TileRenderer tileRenderer;
    private Client.Systems.FoodRenderer foodRenderer;
    private HashSet<Keys> previouslyDown = new HashSet<Keys>();
    private int score = 0;
    private Entity player;


    private GameStateEnum nextState = GameStateEnum.GamePlay;

    public override void setupInput(KeyboardInput keyboard)
    {
      keyboard.registerCommand(Keys.Escape, true, new IInputDevice.CommandDelegate(exit), GameStateEnum.GamePlay, Actions.exit);
    }

    public override void loadContent(ContentManager contentManager)
    {
      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime32");

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
      // long start = DateTime.Now.Ticks;
      tileRenderer.update(gameTime, spriteBatch, player);
      foodRenderer.update(gameTime, spriteBatch, player);
      playerRenderer.update(gameTime, spriteBatch, player);
      // long end = DateTime.Now.Ticks;
      // System.Console.WriteLine("{0}", (end - start)/10000);
    }

    public override void update(GameTime gameTime)
    {
      systemNetwork.update(gameTime.ElapsedGameTime, MessageQueueClient.instance.getMessages());
      systemKeyboardInput.update(gameTime.ElapsedGameTime);
      systemInterpolation.update(gameTime.ElapsedGameTime);
      if (player != null && entities.ContainsKey(player.id)) {
        player = entities[player.id];
      }
      if (player != null && !entities.ContainsKey(player.id))
      {
        //nextState = GameStateEnum.Lose;
      }
    }

    public void exit(GameTime gameTime, float value)
    {
      score = 0;
      nextState = GameStateEnum.MainMenu;
    }  
    public void beginConnection()
    {
      systemNetwork = new Client.Systems.Network();
      entities = new Dictionary<uint, Entity>();
      systemInterpolation = new Client.Systems.Interpolation();
      playerRenderer = new Client.Systems.PlayerRenderer(graphics);
      foodRenderer = new Client.Systems.FoodRenderer(graphics);

      systemNetwork.registerHandler(Shared.Messages.Type.NewEntity, (TimeSpan elapsedTime, Message message) =>
        {
          handleNewEntity((NewEntity)message);
        });

      systemNetwork.registerHandler(Shared.Messages.Type.RemoveEntity, (TimeSpan elapsedTime, Message message) =>
      {
        handleRemoveEntity((RemoveEntity)message);
      });

      systemKeyboardInput = new Client.Systems.KeyboardInput(new List<Tuple<Shared.Components.Input.Type, Keys>>
      {
        Tuple.Create(Shared.Components.Input.Type.Up, Keys.W),
        Tuple.Create(Shared.Components.Input.Type.Left, Keys.A),
        Tuple.Create(Shared.Components.Input.Type.Right, Keys.D),
        Tuple.Create(Shared.Components.Input.Type.Down, Keys.S),
      });
      MessageQueueClient.instance.initialize("localhost", 3000);
    }
    public void endConnection()
    {
      MessageQueueClient.instance.sendMessage(new Shared.Messages.Disconnect());
      MessageQueueClient.shutdown();
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
        entity.add(new Movement(message.moveRate, message.rotateRate));
      }

      if (message.hasInput)
      {
        entity.add(new Shared.Components.Input(message.inputs));
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