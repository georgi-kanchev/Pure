using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
using SizeI = (int width, int height);

namespace Pure.Tools.UserInterface;

internal class Container
{
    public string Name { get; set; } = "";
    public string? Parent { get; set; } = null;

    public Area? Area { get; set; } = null;
    public Pivot Pivot { get; set; } = Pivot.Center;
    public SizeI Gap { get; set; } = (1, 1);
    public Wrap Wrap { get; set; } = Wrap.SingleRow;
    public Dictionary<string, (string type, Block block)> Blocks { get; set; } = [];

    public void Align()
    {
        foreach (var (_, b) in Blocks)
            b.block.AlignInside(Area ?? (0, 0, Input.Bounds.width, Input.Bounds.height), Pivot);
    }
}