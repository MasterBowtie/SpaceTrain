
using Shared.Entities;

namespace Shared.Components
{
  public class Head : Component
  {
    public Head(int id)
    {
      this.id = id;
      this.score = 0;
    }
    public int id { get; private set; }
    public uint score { get; set; }

    public bool updated { get; set; } = false;
  }
}
