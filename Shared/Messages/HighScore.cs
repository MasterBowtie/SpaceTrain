using Shared.Components;

namespace Shared.Messages
{
  public class HighScores : Message
  {
    public List<uint> scores = new List<uint>();

    public HighScores() : base(Type.HighScores)
    {

    }

    public HighScores(List<uint> scores) : base(Type.HighScores)
    {
      this.scores = scores;
    }

    public override byte[] serialize()
    {
      List<byte> data = new List<byte>();
      data.AddRange(base.serialize());

      data.AddRange(BitConverter.GetBytes(scores.Count));
      foreach (uint score in scores)
      {
        data.AddRange(BitConverter.GetBytes((UInt32)score));
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
        var score = BitConverter.ToUInt32(data, offset);
        offset += sizeof(UInt32);
        scores.Add(score);
      }

      return offset;
    }
  }
}
