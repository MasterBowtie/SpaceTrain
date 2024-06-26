﻿using System.Text;
using Microsoft.Xna.Framework;
using Shared.Components;
using Shared.Entities;

namespace Shared.Messages
{
  public class NewEntity : Message
  {
    public NewEntity(Entity entity) : base(Type.NewEntity)
    {
      this.id = entity.id;

      if (entity.contains<Head>())
      {
        this.hasHead = true;
        this.head = entity.get<Head>().id;
        this.headName = entity.get<Head>().name;
      }

      if (entity.contains<Shared.Components.TurnPoint>())
      {
        this.hasTurnPoint = true;
      }

      if (entity.contains<Appearance>())
      {
        this.hasAppearance = true;
        this.texture = entity.get<Appearance>().texture;
      }
      else if (entity.contains<A_Appearance>())
      {
        this.hasAApperance = true;
        this.texture = entity.get<A_Appearance>().texture;
        this.subImageWidth = entity.get<A_Appearance>().subImageWidth;
        this.spriteTime = entity.get<A_Appearance>().spriteTime;
      }
      else
      {
        this.texture = "";
      }

      if (entity.contains<Position>())
      {
        this.hasPosition = true;
        this.position = entity.get<Position>().position;
        this.orientation = entity.get<Position>().orientation;
      }

      if (entity.contains<Size>())
      {
        this.hasSize = true;
        this.size = entity.get<Size>().size;
      }

      if (entity.contains<Movement>())
      {
        this.hasMovement = true;
        this.moveRate = entity.get<Movement>().moveRate;
        this.rotateRate = entity.get<Movement>().rotateRate;
      }

      if (entity.contains<Components.Input>())
      {
        this.hasInput = true;
        this.inputs = entity.get<Components.Input>().inputs;
      }
      else
      {
        this.inputs = new List<Components.Input.Type>();
      }

      if (entity.contains<Connected>())
      {
        this.hasConnected = true;
        if (entity.get<Connected>().leads != null)
        {
          this.hasLead = true;
          this.leads = entity.get<Connected>().leads.id;
        }
        if (entity.get<Connected>().follows != null)
        {
          this.hasFollow = true;
          this.follows = entity.get<Connected>().follows.id;
        }
      }

    }
    public NewEntity() : base(Type.NewEntity)
    {
      this.texture = "";
      this.inputs = new List<Components.Input.Type>();
    }

    public uint id { get; private set; }

    // Head
    public bool hasHead { get; private set; } = false;
    public int head { get; private set; }
    public string headName { get; private set; }

    //Turn Point
    public bool hasTurnPoint { get; private set; } = false;

    // Animated Apperance
    public bool hasAApperance { get; private set; } = false;
    public int[] spriteTime { get; private set; }
    public int subImageWidth { get; private set; }

    // Appearance
    public bool hasAppearance { get; private set; } = false;
    public string texture { get; private set; }

    // Position
    public bool hasPosition { get; private set; } = false;
    public Vector2 position { get; private set; }
    public float orientation { get; private set; }

    // size
    public bool hasSize { get; private set; } = false;
    public Vector2 size { get; private set; }

    // Movement
    public bool hasMovement { get; private set; } = false;
    public float moveRate { get; private set; }
    public float rotateRate { get; private set; }

    // Input
    public bool hasInput { get; private set; } = false;
    public List<Components.Input.Type> inputs { get; private set; }

    public bool hasConnected { get; private set; } = false;

    public bool hasLead { get; private set;} = false;
    public uint leads { get; private set; }
    public bool hasFollow { get; private set; } = false;
    public uint follows { get; private set; }

    public override byte[] serialize()
    {
      List<byte> data = new List<byte>();

      data.AddRange(base.serialize());
      data.AddRange(BitConverter.GetBytes(id));

      data.AddRange(BitConverter.GetBytes(hasHead));
      if (hasHead)
      {
        data.AddRange(BitConverter.GetBytes(head));
        data.AddRange(BitConverter.GetBytes(headName.Length));
        data.AddRange(Encoding.UTF8.GetBytes(headName));
      }

      data.AddRange(BitConverter.GetBytes(hasTurnPoint));
      // Nothing else to do for a  turn point, because marking it
      // as a turn point is all the info we need.  The position
      // and direction of the turn point are contained in the Position component
      // on the entity.


      data.AddRange(BitConverter.GetBytes(hasAApperance));
      if (hasAApperance)
      {
        data.AddRange(BitConverter.GetBytes(texture.Length));
        data.AddRange(Encoding.UTF8.GetBytes(texture));

        for (int i = 0; i < spriteTime.Length; i++)
        {
          data.AddRange(BitConverter.GetBytes(spriteTime[i]));
        }
        data.AddRange(BitConverter.GetBytes(-1));

        data.AddRange(BitConverter.GetBytes(subImageWidth));
      }

      data.AddRange(BitConverter.GetBytes(hasAppearance));
      if (hasAppearance)
      {
        data.AddRange(BitConverter.GetBytes(texture.Length));
        data.AddRange(Encoding.UTF8.GetBytes(texture));
      }

      data.AddRange(BitConverter.GetBytes(hasPosition));
      if (hasPosition)
      {
        data.AddRange(BitConverter.GetBytes(position.X));
        data.AddRange(BitConverter.GetBytes(position.Y));
        data.AddRange(BitConverter.GetBytes(orientation));
      }

      data.AddRange(BitConverter.GetBytes(hasSize));
      if (hasSize)
      {
        data.AddRange(BitConverter.GetBytes(size.X));
        data.AddRange(BitConverter.GetBytes(size.Y));
      }

      data.AddRange(BitConverter.GetBytes(hasMovement));
      if (hasMovement)
      {
        data.AddRange(BitConverter.GetBytes(moveRate));
        data.AddRange(BitConverter.GetBytes(rotateRate));
      }

      data.AddRange(BitConverter.GetBytes(hasInput));
      if (hasInput)
      {
        data.AddRange(BitConverter.GetBytes(inputs.Count));
        foreach (var input in inputs)
        {
          data.AddRange(BitConverter.GetBytes((UInt16)input));
        }
      }


      data.AddRange(BitConverter.GetBytes(hasConnected));
      if (hasConnected)
      {
        data.AddRange(BitConverter.GetBytes(hasLead));
        if (hasLead)
        {
          data.AddRange(BitConverter.GetBytes((UInt32)leads));
        }
        data.AddRange(BitConverter.GetBytes(hasFollow));
        if (hasFollow)
        {
          data.AddRange(BitConverter.GetBytes((UInt32)follows));
        }
      }


      return data.ToArray();
    }

    public override int parse(byte[] data)
    {
      int offset = base.parse(data);

      this.id = BitConverter.ToUInt32(data, offset);
      offset += sizeof(uint);

      this.hasHead = BitConverter.ToBoolean(data, offset);
      offset += sizeof(bool);
      if (hasHead)
      {
        this.head = BitConverter.ToInt32(data, offset);
        offset += sizeof(Int32);
        int stringSize = BitConverter.ToInt32(data, offset);
        offset += sizeof(Int32);
        this.headName = Encoding.UTF8.GetString(data, offset, stringSize);
        offset += stringSize;
      }

      this.hasTurnPoint = BitConverter.ToBoolean(data, offset);
      offset += sizeof(bool);

      this.hasAApperance = BitConverter.ToBoolean(data, offset);
      offset += sizeof(bool);

      if (hasAApperance)
      {
        int textureSize = BitConverter.ToInt32(data, offset);
        offset += sizeof(Int32);
        this.texture = Encoding.UTF8.GetString(data, offset, textureSize);
        offset += textureSize;

        List<int> times = new List<int>();
        int time;
        do
        {
          time = BitConverter.ToInt32(data, offset);
          offset += sizeof(Int32);
          times.Add(time);
        }
        while (time != -1);
        spriteTime = times.Take(times.Count - 1).ToArray();

        subImageWidth = BitConverter.ToInt32(data, offset);
        offset += sizeof(Int32);
      }

      this.hasAppearance = BitConverter.ToBoolean(data, offset);
      offset += sizeof(bool);
      if (hasAppearance)
      {
        int textureSize = BitConverter.ToInt32(data, offset);
        offset += sizeof(Int32);
        this.texture = Encoding.UTF8.GetString(data, offset, textureSize);
        offset += textureSize;
      }

      this.hasPosition = BitConverter.ToBoolean(data, offset);
      offset += sizeof(bool);
      if (hasPosition)
      {
        float positionX = BitConverter.ToSingle(data, offset);
        offset += sizeof(Single);
        float positionY = BitConverter.ToSingle(data, offset);
        offset += sizeof(Single);
        this.position = new Vector2(positionX, positionY);
        this.orientation = BitConverter.ToSingle(data, offset);
        offset += sizeof(Single);
      }


      this.hasSize = BitConverter.ToBoolean(data, offset);
      offset += sizeof(bool);
      if (hasSize)
      {
        float sizeX = BitConverter.ToSingle(data, offset);
        offset += sizeof(Single);
        float sizeY = BitConverter.ToSingle(data, offset);
        offset += sizeof(Single);
        this.size = new Vector2(sizeX, sizeY);
      }

      this.hasMovement = BitConverter.ToBoolean(data, offset);
      offset += sizeof(bool);
      if (hasMovement)
      {
        this.moveRate = BitConverter.ToSingle(data, offset);
        offset += sizeof(Single);
        this.rotateRate = BitConverter.ToSingle(data, offset);
        offset += sizeof(Single);
      }

      this.hasInput = BitConverter.ToBoolean(data, offset);
      offset += sizeof(bool);
      if (hasInput)
      {
        int howMany = BitConverter.ToInt32(data, offset);
        offset += sizeof(int);
        for (int i = 0; i < howMany; i++)
        {
          inputs.Add((Components.Input.Type)BitConverter.ToUInt16(data, offset));
          offset += sizeof(UInt16);
        }
      }
      this.hasConnected = BitConverter.ToBoolean(data, offset);
      offset += sizeof(bool);

      if (hasConnected)
      {
        this.hasLead = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        if (this.hasLead) { 
          this.leads = BitConverter.ToUInt32(data, offset);
          offset += sizeof(uint);
        }
        this.hasFollow = BitConverter.ToBoolean(data, offset);
        offset += sizeof(bool);
        if (this.hasFollow)
        {
          this.follows = BitConverter.ToUInt32(data, offset);
          offset += sizeof(uint);
        }
      }


      return offset;
    }
  }
}
