
using Shared.Entities;

namespace Shared.Components
{
  public class Head : Component
  {
    public Head(int id, string name)
    {
      this.id = id;
      this.score = 0;
      this.name = name;
    }
    public int id { get; private set; }
    public uint score { get; set; }
    public string name { get; private set; }
  }
}
