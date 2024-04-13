
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Client.Components
{
  public class A_Sprite : Shared.Components.Component
  {
    public A_Sprite(Texture2D texture, int subImageWidth, int[] spriteTime)
    {
      this.texture = texture;
      this.subImageWidth = subImageWidth;
      this.spriteTime = spriteTime;
      this.animationTime = TimeSpan.FromMilliseconds(spriteTime[0]);
      center = new Vector2(subImageWidth / 2, texture.Height / 2);
      this.subImageIndex = 0;
    }

    public Texture2D texture { get; private set; }
    public Vector2 center { get; private set; }

    public TimeSpan animationTime { get; set; }

    public int[] spriteTime { get; private set; }
    public int subImageWidth { get; private set; }
    public int subImageIndex { get; set; }
  }
}
