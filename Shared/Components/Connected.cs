using Shared.Entities;

namespace Shared.Components
{
  public class Connected: Shared.Components.Component
  {
    public Connected(Entity? leads, Entity? follows) {
      this.leads = leads;
      this.follows = follows;
    }
    public Entity? leads {  get; set; }
    public Entity? follows { get; set; }
  }
}
