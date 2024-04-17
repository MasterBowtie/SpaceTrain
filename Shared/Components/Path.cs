using Shared.Entities;

namespace Shared.Components
{
  public class Path : Component
  {
    public Path(Position position, float moveRate)
    {
      for (int i = 0; i < 3 / moveRate; i++)
      {
        PathPoint pathPT = new PathPoint();
        pathPT.x = position.position.X;
        pathPT.y = position.position.Y;
        pathPT.orientation = position.orientation;
        
        path.Enqueue(pathPT);
      }
    }

    public void enqueue(Position position)
    {
      PathPoint pathPT = new PathPoint();
      pathPT.x = position.position.X;
      pathPT.y = position.position.Y;
      pathPT.orientation = position.orientation;

      path.Enqueue(pathPT);
    }

    public PathPoint dequeue()
    {
      return path.Dequeue();
    }

    private Queue<PathPoint> path = new Queue<PathPoint>();

  public struct PathPoint
  {
    public float x;
    public float y;
    public float orientation;
  }
  }

}
