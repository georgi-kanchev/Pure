using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
using Pure.Engine.Utility;
using SizeI = (int width, int height);

namespace Pure.Tools.UserInterface;

internal class Container
{
    public string Name { get; set; } = "";
    public string? Parent { get; set; } = null;

    public Area Area
    {
        get => area ?? (0, 0, Input.Bounds.width, Input.Bounds.height);
        set => area = value;
    }
    public Pivot Pivot { get; set; } = Pivot.Center;
    public SizeI Gap { get; set; } = (1, 1);
    public Wrap Wrap { get; set; } = Wrap.SingleRow;
    public Dictionary<string, (string type, Block block)> Blocks { get; set; } = [];

    public void Align()
    {
        if (Blocks.Count == 0)
            return;

        var first = Blocks.First().Value.block;
        first.AlignInside(GetBoundingBox(), Pivot.TopLeft);
        var targetArea = first.Area;

        foreach (var (_, b) in Blocks.Skip(1))
            if (Wrap == Wrap.SingleRow)
            {
                b.block.AlignX((Pivot.Left, Pivot.Right), targetArea, 1);
                b.block.AlignY((Pivot.Left, Pivot.Right), targetArea);
                // b.block.Fit(Area);
                targetArea = b.block.Area;
            }
    }

#region Backend
    private Area? area;

    private Area GetBoundingBox()
    {
        if (Blocks.Count == 0)
            return default;

        var size = (0, int.MinValue);
        var (x, y, w, h) = Area.ToBundle();

        foreach (var (_, b) in Blocks)
        {
            size.Item1 += b.block.Width + Gap.width;
            size.Item2 = b.block.Height > size.Item2 ? b.block.Height : size.Item2;
        }

        size.Item1 -= Gap.width;

        if (Pivot is Pivot.Top or Pivot.Center or Pivot.Bottom)
            x = x + w / 2 - size.Item1 / 2;
        else if (Pivot is Pivot.TopRight or Pivot.Right or Pivot.BottomRight)
            x = x + w - size.Item1;

        return (x, y, size.Item1, size.Item2);
    }
#endregion
}