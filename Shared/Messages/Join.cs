
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;
using System.Text;

namespace Shared.Messages
{
  public class Join : Message
  {
    public Join() : base(Type.Join)
    {

    }

    public Join(string playerName) : base(Type.Join)
    {
      this.playerName = playerName;
    }

    public string playerName;


    /// <summary>
    /// Take the players name
    /// </summary>
    public override byte[] serialize()
    {
      List<byte> data = new List<byte>();

      data.AddRange(base.serialize());

      data.AddRange(BitConverter.GetBytes(playerName.Length));
      data.AddRange(Encoding.UTF8.GetBytes(playerName));

      return data.ToArray();
    }

    /// <summary>
    /// Parse the name to make available to save in HighScores
    /// </summary>
    public override int parse(byte[] data)
    {
      int offset = base.parse(data);

      int stringSize = BitConverter.ToInt32(data, offset);
      offset += sizeof(Int32);
      this.playerName = Encoding.UTF8.GetString(data, offset, stringSize);
      offset += stringSize;

      return offset;
    }
  }
}
