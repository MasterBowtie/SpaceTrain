
using System.Formats.Asn1;
using System.Runtime.CompilerServices;

namespace Shared.Messages
{
  public class Score : Message
  {
    public uint score;

    public Score() : base(Type.Score)
    {

    }

    public Score(uint score) : base(Type.Score)
    {
      this.score = score;
    }

    public override byte[] serialize()
    {
      List<byte> data = new List<byte>();

      data.AddRange(base.serialize());
      data.AddRange(BitConverter.GetBytes(score));

      return data.ToArray();
    }

    public override int parse(byte[] data)
    {
      int offset = base.parse(data);

      this.score = BitConverter.ToUInt32(data, offset);
      offset += sizeof(UInt32);

      return offset;
    }
  }
}
