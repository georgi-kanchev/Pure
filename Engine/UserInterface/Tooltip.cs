namespace Pure.Engine.UserInterface;

public class Tooltip : Block
{
    public Pivot Pivot { get; set; } = Pivot.Top;
    public float Alignment { get; set; } = 0.5f;

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

        var opposites = new[] { Pivot.Right, Pivot.Left, Pivot.Bottom, Pivot.Top };
        Size = (width + 2, lines.Length);
        AlignOutside(Pivot, aroundArea, Alignment, 1);
        Fit();

        if (IsOverlapping(aroundArea) == false)
            return;

        AlignOutside(opposites[(int)Pivot], aroundArea, Alignment, 1);
        Fit();
    }
}