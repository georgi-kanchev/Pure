using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
using Pure.Engine.Utility;

namespace Pure.Tools.UserInterface;

internal class Container
{
    public Dictionary<string, (string type, Block block)> Blocks
    {
        get => blocks;
        set
        {
            blocks = value;
            Align();
        }
    }
    public Area? Area
    {
        get => area;
        set
        {
            area = value;
            Align();
        }
    }
    public Pivot Pivot
    {
        get => pivot;
        set
        {
            pivot = value;
            Align();
        }
    }
    public bool IsAlwaysAligning { get; set; }

    public Container()
    {
        Align();
    }

    public void Align()
    {
        foreach (var (_, b) in blocks)
            b.block.AlignInside(Area ?? (0, 0, Input.Bounds.width, Input.Bounds.height), pivot);
    }

#region Backend
    private Pivot pivot;
    private Area? area;
    private Dictionary<string, (string type, Block block)> blocks = [];
#endregion
}