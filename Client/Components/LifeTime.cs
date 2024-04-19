
namespace Client.Components
{
  public class LifeTime : Shared.Components.Component
  {
    public LifeTime(float time) {
      this.time = time;
    }
    public float time {  get; set; }

  }
}
