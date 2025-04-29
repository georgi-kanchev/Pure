namespace Pure.Engine.UserInterface;

public class Tooltip : Block
{
    public Pivot Pivot { get; set; } = Pivot.Top;

    public Tooltip()
    {
        Size = (10, 1);
    }
    public void Show(Area aroundArea)
    {
        var lines = Text.Replace("\r", "").Split("\n");
        var width = 0;
        foreach (var line in lines)
            if (line.Length > width)
                width = line.Length;

        Size = (width + 2, lines.Length);
        AlignOutside(aroundArea, Pivot, offsets[Pivot]);
        Fit();

        if (IsOverlapping(aroundArea) == false)
            return;

        var opposite = (Pivot)(8 - (int)Pivot);
        AlignOutside(aroundArea, opposite, offsets[opposite]);
        Fit();
    }

#region Backend
    [DoNotSave]
    private static readonly Dictionary<Pivot, PointI> offsets = new()
    {
        { Pivot.TopLeft, (-1, -1) }, { Pivot.Top, (0, -1) }, { Pivot.TopRight, (1, -1) },
        { Pivot.Left, (-1, 0) }, { Pivot.Center, (0, 0) }, { Pivot.Right, (1, 0) },
        { Pivot.BottomLeft, (-1, 1) }, { Pivot.Bottom, (0, 1) }, { Pivot.BottomRight, (1, 1) }
    };
#endregion
}