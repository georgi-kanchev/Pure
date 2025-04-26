using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
using SizeI = (int width, int height);

namespace Pure.Tools.UserInterface;

internal class Container
{
    public Dictionary<string, (string type, Block block)> Blocks { get; set; } = [];
    public Area? Area { get; set; }
    public Pivot Pivot { get; set; }
    public SizeI Gap { get; set; } = (1, 1);

    public void Align()
    {
        foreach (var (_, b) in Blocks)
            b.block.AlignInside(Area ?? (0, 0, Input.Bounds.width, Input.Bounds.height), Pivot);
    }
}