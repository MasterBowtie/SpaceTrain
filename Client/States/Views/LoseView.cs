
using System;
using System.Collections.Generic;
using Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shared.Entities;
using Shared.Messages;

namespace apedaile
{
  public class Lose : GameStateView
  {

    private SpriteFont mainFont;

    private ContentManager contentManager;
    private Dictionary<uint, Entity> entities = new Dictionary<uint, Entity>();
    private Client.Systems.Network systemNetwork;
    private Client.Systems.KeyboardInput systemKeyboardInput;
    private Client.Systems.Interpolation systemInterpolation;
    private Client.Systems.PlayerRenderer systemRenderer;
    private HashSet<Keys> previouslyDown = new HashSet<Keys>();
    private Entity player;


    private GameStateEnum nextState = GameStateEnum.GamePlay;

    public override void setupInput(KeyboardInput keyboard)
    {
      keyboard.registerCommand(Keys.Escape, true, new IInputDevice.CommandDelegate(exit), GameStateEnum.Lose, Actions.exit);
    }

    public override void loadContent(ContentManager contentManager)
    {
      mainFont = contentManager.Load<SpriteFont>("Fonts/CourierPrime32");
    }

    public override GameStateEnum processInput(GameTime gameTime)
    {

      if (nextState != GameStateEnum.Lose)
      {
        GameStateEnum newState = nextState;
        nextState = GameStateEnum.Lose;
        return newState;
      }
      return GameStateEnum.Lose;
    }

    public override void render(GameTime gameTime)
    {
      systemRenderer.update(gameTime, spriteBatch, player);
    }

    public override void update(GameTime gameTime)
    {
      systemNetwork.update(gameTime.ElapsedGameTime, MessageQueueClient.instance.getMessages());
      systemKeyboardInput.update(gameTime.ElapsedGameTime);
      systemInterpolation.update(gameTime.ElapsedGameTime);
      player = entities[player.id];
      if (player != null && !entities.ContainsKey(player.id))
      {
        //nextState = GameStateEnum.Lose;
      }
    }

    public void exit(GameTime gameTime, float value)
    {
      nextState = GameStateEnum.MainMenu;
      MessageQueueClient.instance.sendMessage(new Shared.Messages.Disconnect());
    }

    /// <summary>
    /// Based upon an Entity received from the server, create the
    /// entity at the client.
    /// </summary>
    private Entity createEntity(Shared.Messages.NewEntity message)
    {
      Entity entity = new Entity(message.id);

      if (message.hasAppearance)
      {
        Texture2D texture = contentManager.Load<Texture2D>(message.texture);
        entity.add(new Client.Components.Sprite(texture));
      }

      if (message.hasPosition)
      {
        entity.add(new Shared.Components.Position(message.position, message.orientation));
      }

      if (message.hasSize)
      {
        entity.add(new Shared.Components.Size(message.size));
      }

      if (message.hasMovement)
      {
        entity.add(new Shared.Components.Movement(message.moveRate, message.rotateRate));
      }

      if (message.hasInput)
      {
        entity.add(new Shared.Components.Input(message.inputs));
      }

      return entity;
    }
  }
  public interface GamePlayState
  {
    public void render(GameTime gameTime);
    public void update(GameTime gameTime);
  }
}