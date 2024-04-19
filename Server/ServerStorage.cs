using System.Data.SqlTypes;
using System.Runtime.Serialization;
using Shared.Messages;


namespace Server
{
  [DataContract(Name = "Storage")]
  public class ServerStorage
  {
    [DataMember()]
    public List<uint> HighScores = new List<uint>();

    public ServerStorage()
    {
    }

    public bool submitScore(uint score)
    {
      if (HighScores.Count < 5)
      {
        HighScores.Add(score);
        HighScores.Sort(compare);
        Message updateScores = new Shared.Messages.HighScores(HighScores);
        MessageQueueServer.instance.broadcastMessage(updateScores);
        return true;
      }
      foreach (uint item in HighScores)
      {
        if (compare(item, score) > 0)
        {
          HighScores.Add(score);

          HighScores.Sort(compare);
          if (HighScores.Count > 5)
          {
            HighScores.RemoveAt(5);
          }
          Message updateScores = new Shared.Messages.HighScores(HighScores);
          MessageQueueServer.instance.broadcastMessage(updateScores);
          return true;
        }
      }
      return false;
    }

    public int compare(uint item1, uint item2)
    {
      if (item1 > item2)
      {
        return -1;
      }
      return 1;
    }
  }
}
