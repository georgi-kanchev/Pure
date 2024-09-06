using Pure.Engine.Collision;

namespace Supremacy1257;

public class NavPath
{
    public Line Line { get; set; }
    public NavPoint A { get; }
    public NavPoint B { get; }

    public NavPath(NavPoint a, NavPoint b, Line line)
    {
        A = a;
        B = b;

        Line = line;
    }
}