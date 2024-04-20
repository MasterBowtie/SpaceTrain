using System;
using System.Collections.Generic;
using apedaile;
using Client.Components;
using Client.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Shared.Components;
using Shared.Entities;
using Shared.Messages;

namespace Client
{
  public class GameModel : GameView
  {
    private Client.Systems.Network systemNetwork;
    private ContentManager contentManager;

    private Client.Systems.Interpolation systemInterpolation = new Client.Systems.Interpolation();
    private Shared.Systems.Movement moveSystem = new Shared.Systems.Movement();
    private Client.Systems.ParticleSystem particleSystem;
    private Client.Systems.KeyboardInput keyboardInput;
    private Dictionary<uint, Entity> entities;
    private float moveRate;

    private Client.Systems.PlayerRenderer playerRenderer;
    private Client.Systems.TileRenderer tileRenderer;
    private Client.Systems.FoodRenderer foodRenderer;
    private Client.Systems.ParticleSystemRenderer particleRenderer;

    public Entity player { set; get; }

    public void setupNetwork(Client.Systems.Network network)
    {
      entities = new Dictionary<uint, Entity>();
      this.systemNetwork = network;

      systemNetwork.registerHandler(Shared.Messages.Type.NewEntity, (TimeSpan elapsedTime, Message message) =>
      {
        handleNewEntity((NewEntity)message);
      });

      systemNetwork.registerHandler(Shared.Messages.Type.RemoveEntity, (TimeSpan elapsedTime, Message message) =>
      {
        handleRemoveEntity((RemoveEntity)message);
      });
    }

    public override void setupInput(apedaile.KeyboardInput keyboard)
    {
      //Do nothing here
    }

    public void setupInput(apedaile.KeyboardInput keyboard, Client.Systems.KeyboardInput systemKeyboard)
    {
      this.keyboardInput = systemKeyboard;
    }

    public override void loadContent(ContentManager contentManager)
    {
      this.contentManager = contentManager;

      playerRenderer = new Client.Systems.PlayerRenderer(graphics);
      foodRenderer = new Client.Systems.FoodRenderer(graphics);
      particleRenderer = new ParticleSystemRenderer(graphics);
      particleSystem = new ParticleSystem();
      particleSystem.loadContent(contentManager, particleRenderer);

      tileRenderer = new Client.Systems.TileRenderer(graphics);
      tileRenderer.loadContent(contentManager);
    }

    public override void update(GameTime gameTime)
    {
      particleSystem.update(gameTime.ElapsedGameTime);
      systemInterpolation.update(gameTime.ElapsedGameTime);
      moveSystem.update(gameTime.ElapsedGameTime);
    }

    public override void render(GameTime gameTime)
    {
      tileRenderer.update(gameTime, spriteBatch, player);
      foodRenderer.update(gameTime, spriteBatch, player);
      playerRenderer.update(gameTime, spriteBatch, player);
      particleRenderer.update(gameTime, spriteBatch, player);
    }

    public override GameViewEnum processInput(GameTime gameTime)
    {
      //Not going to do anything with this;
      return GameViewEnum.Model;
    }

    public bool checkPlayer()
    {
      return entities.ContainsKey(player.id);
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
        entity.add(new Head(message.head, message.headName));
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
      keyboardInput.add(entity);
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
      keyboardInput.remove(id);
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
