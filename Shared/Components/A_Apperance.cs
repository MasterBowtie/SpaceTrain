
namespace Shared.Components
{
  public class A_Appearance : Component
  {
    public A_Appearance(string texture, int subImageWidth, int[] spriteTime)
    {
      this.texture = texture;
      this.subImageWidth = subImageWidth;
      this.spriteTime = spriteTime;
    }

    public string texture { get; private set; }
    public int subImageWidth { get; private set; }
    public int[] spriteTime { get; private set; } 
  }
}
