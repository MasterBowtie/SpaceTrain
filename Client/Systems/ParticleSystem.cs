using Client.Components;
using System;
using Shared.Entities;
using Client.Entities;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Shared.Components;

namespace Client.Systems
{
  public class ParticleSystem : Shared.Systems.System
  {

    private Texture2D particleTexture;
    private ParticleSystemRenderer renderer;

    public ParticleSystem() :
      base(typeof(LifeTime))
    { }

    public void loadContent(ContentManager contentManager, ParticleSystemRenderer renderer)
    {
      this.renderer = renderer;
      particleTexture = contentManager.Load<Texture2D>("Textures/particle");
    }
    public override void update(TimeSpan elapsedTime)
    {
      foreach (var particle in m_entities.Values)
      {
        LifeTime time = particle.get<LifeTime>();
        time.time -= (float)elapsedTime.TotalMilliseconds;
        
        if (time.time < 0)
        {
          renderer.remove(particle.id);
          base.remove(particle.id);
        }
        Shared.Entities.Utility.move(particle, elapsedTime);

      }
    }

    public void explode(Entity source)
    {
      Position position = source.get<Position>();

      int amount = 15;
      float rotate = (float) (2 * Math.PI )/amount;
      for (int i = 0; i < amount; i++ )
      {
        Entity particle = Particle.create(
          particleTexture, //need to make Texture
          new Vector2(position.position.X, position.position.Y), //need to get position 
          rotate * i,
          10, 
          0.1f,
          400);

        renderer.add(particle);
        base.add(particle);
      }
    }
  }
}