using Pure.Engine.Collision;
using Pure.Engine.Utilities;

namespace Supremacy1257;

public class NavPath
{
    public uint Color { get; set; }
    public Line Line { get; }
    public NavPoint A { get; }
    public NavPoint B { get; }

    public NavPath(NavPoint a, NavPoint b)
    {
        Color = uint.MaxValue;
        A = a;
        B = b;
        Line = new(A.Position, B.Position, Color);
    }
}