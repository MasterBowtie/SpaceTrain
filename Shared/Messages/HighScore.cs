using System.Text;
using Shared.Components;
using static System.Formats.Asn1.AsnWriter;

namespace Shared.Messages
{
  public class HighScores : Message
  {
    public List<(string, uint)> scores = new List<(string, uint)>();

    public HighScores() : base(Type.HighScores)
    {
    }

    public HighScores(List<(string, uint)> scores) : base(Type.HighScores)
    {
      this.scores = scores;
    }

    public override byte[] serialize()
    {
      List<byte> data = new List<byte>();
      data.AddRange(base.serialize());

      data.AddRange(BitConverter.GetBytes(scores.Count));
      foreach (var item in scores)
      {
        data.AddRange(BitConverter.GetBytes(item.Item1.Length));
        data.AddRange(Encoding.UTF8.GetBytes(item.Item1));

        data.AddRange(BitConverter.GetBytes((UInt32)item.Item2));
      }

      return data.ToArray();
    }

    public override int parse(byte[] data)
    {
      int offset = base.parse(data);

      int howMany = BitConverter.ToInt32(data, offset);
      offset += sizeof(UInt32);

      for (int i = 0; i < howMany; i++)
      {
        int stringSize = BitConverter.ToInt32(data, offset);
        offset += sizeof(Int32);
        String playerName = Encoding.UTF8.GetString(data, offset, stringSize);
        offset += stringSize;

        var score = BitConverter.ToUInt32(data, offset);
        offset += sizeof(UInt32);
        scores.Add((playerName,score));
      }
      return offset;
    }
  }
}
