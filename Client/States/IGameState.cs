using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace apedaile
{
  public interface GameState
  {
    public void render(GameTime gameTime);
    public void update(GameTime gameTime);
  }
}
