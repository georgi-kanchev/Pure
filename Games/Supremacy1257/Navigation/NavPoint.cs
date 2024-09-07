using Pure.Engine.Utilities;

namespace Supremacy1257;

public class NavPoint
{
    public Point Position { get; set; }
    public List<NavPath> Connections { get; } = new();

    public NavPoint(float x, float y)
    {
        Position = new(x, y);
    }
    public NavPoint(Point point)
    {
        Position = point;
    }

    public override string ToString()
    {
        return $"{Position} Connections: {Connections.Count}";
    }
}