namespace Pure.Engine.UserInterface;

using System.Diagnostics;

public class Tooltip : Block
{
    public Side Side { get; set; } = Side.Top;
    public float Alignment { get; set; } = 0.5f;

    public Tooltip()
    {
        Size = (10, 1);
    }
    public Tooltip(byte[] bytes) : base(bytes)
    {
        var b = Decompress(bytes);
        Side = (Side)GrabByte(b);
        Alignment = GrabFloat(b);
    }
    public Tooltip(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public override string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public override byte[] ToBytes()
    {
        var result = Decompress(base.ToBytes()).ToList();
        Put(result, (byte)Side);
        Put(result, Alignment);
        return Compress(result.ToArray());
    }

    public void Show((int x, int y, int width, int height) aroundArea)
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

    public Tooltip Duplicate()
    {
        return new(ToBytes());
    }

    public static implicit operator byte[](Tooltip tooltip)
    {
        return tooltip.ToBytes();
    }
    public static implicit operator Tooltip(byte[] bytes)
    {
        return new(bytes);
    }

    #region Backend
    private readonly Stopwatch hold = new();
    #endregion
}