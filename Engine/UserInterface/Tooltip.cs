namespace Pure.Engine.UserInterface;

public class Tooltip : Block
{
    public Side Side { get; set; } = Side.Top;
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

        var opposites = new[] { Side.Right, Side.Left, Side.Bottom, Side.Top };
        Size = (width + 2, lines.Length);
        AlignOutside(Side, aroundArea, Alignment, 1);
        Fit();

        if (IsOverlapping(aroundArea) == false)
            return;

        AlignOutside(opposites[(int)Side], aroundArea, Alignment, 1);
        Fit();
    }
}