
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using Shared.Entities;

namespace Client.Systems
{
  public class KeyboardInput : Shared.Systems.System
  {
    private class KeyToType
    {
      public Dictionary<Keys, Shared.Components.Input.Type> m_keyToType = new Dictionary<Keys, Shared.Components.Input.Type>();
    }

    private Dictionary<Shared.Components.Input.Type, Keys> m_typeToKey = new Dictionary<Shared.Components.Input.Type, Keys>();
    private Dictionary<uint, KeyToType> m_keyToFunction = new Dictionary<uint, KeyToType>();

    private HashSet<Keys> m_keysPressed = new HashSet<Keys>();
    private List<Shared.Components.Input.Type> m_inputEvents = new List<Shared.Components.Input.Type>();

    public KeyboardInput() : base(typeof(Shared.Components.Input))
    {}

    public Dictionary<Shared.Components.Input.Type, Keys> getMappings()
    {
      return m_typeToKey;
    }



    public void addMapping(Shared.Components.Input.Type action, Keys key)
    {
      m_typeToKey[action] = key;
    }

    public override void update(TimeSpan elapsedTime)
    {
      foreach (var item in m_entities)
      {
        List<Shared.Components.Input.Type> inputs = new List<Shared.Components.Input.Type>();

        if (m_keysPressed.Count == 2)
        {
          var keys = m_keysPressed.ToArray();
          var key1 = keys[0];
          var key2 = keys[1];
          if (m_keyToFunction[item.Key].m_keyToType.ContainsKey(key1) && m_keyToFunction[item.Key].m_keyToType.ContainsKey(key2))
          {
            var type1 = m_keyToFunction[item.Key].m_keyToType[key1];
            var type2 = m_keyToFunction[item.Key].m_keyToType[key2];

            switch (type1)
            {
              case Shared.Components.Input.Type.Up:
                switch (type2)
                {
                  case Shared.Components.Input.Type.Left:
                    Shared.Entities.Utility.nw(item.Value, elapsedTime);
                    inputs.Add(Shared.Components.Input.Type.NW);
                    break;
                  case Shared.Components.Input.Type.Right:
                    Shared.Entities.Utility.ne(item.Value, elapsedTime);
                    inputs.Add(Shared.Components.Input.Type.NE);
                    break;
                }
                break;
              case Shared.Components.Input.Type.Left:
                switch (type2)
                {
                  case Shared.Components.Input.Type.Up:
                    Shared.Entities.Utility.nw(item.Value, elapsedTime);
                    inputs.Add(Shared.Components.Input.Type.NW);
                    break;
                  case Shared.Components.Input.Type.Down:
                    Shared.Entities.Utility.sw(item.Value, elapsedTime);
                    inputs.Add(Shared.Components.Input.Type.SW);
                    break;
                }
                break;
              case Shared.Components.Input.Type.Right:
                switch (type2)
                {
                  case Shared.Components.Input.Type.Up:
                    Shared.Entities.Utility.ne(item.Value, elapsedTime);
                    inputs.Add(Shared.Components.Input.Type.NE);
                    break;
                  case Shared.Components.Input.Type.Down:
                    Shared.Entities.Utility.se(item.Value, elapsedTime);
                    inputs.Add(Shared.Components.Input.Type.SE);
                    break;
                }
                break;
              case Shared.Components.Input.Type.Down:
                switch (type2)
                {
                  case Shared.Components.Input.Type.Left:
                    Shared.Entities.Utility.sw(item.Value, elapsedTime);
                    inputs.Add(Shared.Components.Input.Type.SW);
                    break;
                  case Shared.Components.Input.Type.Right:
                    Shared.Entities.Utility.se(item.Value, elapsedTime);
                    inputs.Add(Shared.Components.Input.Type.SE);
                    break;
                }
                break;
            }

          }
        }

        if (m_keysPressed.Count == 1)
        {
          var key = m_keysPressed.ToArray()[0];
          if (m_keyToFunction[item.Key].m_keyToType.ContainsKey(key))
          {
            var type = m_keyToFunction[item.Key].m_keyToType[key];
            inputs.Add(type);

            // Client-side prediction of the input
            switch (type)
            {
              case Shared.Components.Input.Type.Up:
                Shared.Entities.Utility.up(item.Value, elapsedTime);
                break;
              case Shared.Components.Input.Type.Left:
                Shared.Entities.Utility.left(item.Value, elapsedTime);
                break;
              case Shared.Components.Input.Type.Right:
                Shared.Entities.Utility.right(item.Value, elapsedTime);
                break;
              case Shared.Components.Input.Type.Down:
                Shared.Entities.Utility.down(item.Value, elapsedTime);
                break;
            }
          }
        }

        if (inputs.Count > 0)
        {
          MessageQueueClient.instance.sendMessageWithId(new Shared.Messages.Input(item.Key, inputs, elapsedTime));
        }
      }
    }

    public override bool add(Entity entity)
    {
      if (!base.add(entity))
      {
        return false;
      }

      KeyToType map = new KeyToType();
      foreach (var input in entity.get<Shared.Components.Input>().inputs)
      {
        map.m_keyToType[m_typeToKey[input]] = input;
      }
      m_keyToFunction[entity.id] = map;

      return true;
    }

    public override void remove(uint id)
    {
      base.remove(id);

      m_keyToFunction.Remove(id);
    }

    public void keyPressed(Keys key)
    {
      m_keysPressed.Add(key);
    }

    public void keyReleased(Keys key)
    {
      m_keysPressed.Remove(key);
    }
  }
}
