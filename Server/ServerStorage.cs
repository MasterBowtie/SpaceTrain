using System.Data.SqlTypes;
using System.Runtime.Serialization;
using Shared.Messages;


namespace Server
{
  [DataContract(Name = "Storage")]
  public class ServerStorage
  {
    [DataMember()]
    public List<(string, uint)> HighScores = new List<(string, uint)>();

    public ServerStorage()
    {
    }

    public bool submitScore(uint score, string name)
    {
      if (HighScores.Count < 5)
      {
        HighScores.Add((name,score));
        HighScores.Sort(compare);
        Message updateScores = new Shared.Messages.HighScores(HighScores);
        MessageQueueServer.instance.broadcastMessage(updateScores);
        return true;
      }
      foreach ((string, uint) item in HighScores)
      {
        if (compare(item, (name,score)) > 0)
        {
          HighScores.Add((name, score));

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

    public int compare((string, uint) item1, (string, uint) item2)
    {
      if (item1.Item2 > item2.Item2)
      {
        return -1;
      }
      return 1;
    }
  }
}
