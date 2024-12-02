namespace Pure.Engine.UserInterface;

public class Layout : Block
{
    [DoNotSave]
    public Action<(int x, int y, int width, int height), int>? OnDisplaySegment { get; set; }

    public int Count
    {
        get => segments.Count;
    }

    public Layout() : this((0, 0))
    {
    }
    public Layout((int x, int y) position) : base(position)
    {
        OnUpdate += OnRefresh;
        Size = (12, 12);
        Restore();
    }

    public void Cut(int index, Side side, float rate)
    {
        if (index >= 0 && index < segments.Count)
            segments.Add(new(rate, side, segments[index]));
    }
    public void Cut(int index, Side side, int size)
    {
        Cut(index, side, (float)size / (side is Side.Top or Side.Bottom ? Height : Width));
    }
    public void Restore()
    {
        segments.Clear();
        segments.Add(new(0, Side.Top, null));
    }

#region Backend
    private sealed class Segment(float rate, Side side, Segment? parent)
    {
        public readonly float rate = Math.Clamp(rate, 0, 1);
        public readonly Side side = side;
        public readonly Segment? parent = parent;

        public (int x, int y) position;
        public (int w, int h) size;
    }

    [DoNotSave]
    private readonly List<Segment> segments = [];

    internal void OnRefresh()
    {
        // updates should be first since it's a hierarchy
        // and then callbacks (after everything is done)

        for (var i = 0; i < segments.Count; i++)
        {
            var seg = segments[i];

            if (i == 0)
            {
                seg.position = Position;
                seg.size = Size;
                continue;
            }

            UpdateSegment(seg);
        }

        for (var i = 0; i < segments.Count; i++)
        {
            var seg = segments[i];
            OnDisplaySegment?.Invoke((seg.position.x, seg.position.y, seg.size.w, seg.size.h), i);
        }
    }
    private void UpdateSegment(Segment seg)
    {
        var (px, py) = Position;
        var (pw, ph) = Size;

        if (seg.parent != null)
        {
            (px, py) = seg.parent.position;
            (pw, ph) = seg.parent.size;
        }

        // update segment itself
        if (seg.side is Side.Left or Side.Top)
            seg.position = (px, py);
        else if (seg.side == Side.Right)
            seg.position = ((int)Map(seg.rate, (0, 1), (px + pw, px)), py);
        else if (seg.side == Side.Bottom)
            seg.position = (px, (int)Map(seg.rate, (0, 1), (py + ph, py)));

        if (seg.side == Side.Left)
            seg.size = ((int)Map(seg.rate, (0, 1), (0, pw)), ph);
        else if (seg.side == Side.Right)
            seg.size = ((int)MathF.Ceiling(Map(seg.rate, (0, 1), (0, pw))), ph);
        else if (seg.side == Side.Top)
            seg.size = (pw, (int)Map(seg.rate, (0, 1), (0, ph)));
        else if (seg.side == Side.Bottom)
            seg.size = (pw, (int)MathF.Ceiling(Map(seg.rate, (0, 1), (0, ph))));

        // update its parent
        if (seg.parent == null)
            return;

        var (x, y) = seg.position;
        var (w, h) = seg.size;

        if (seg.side is Side.Left or Side.Right)
            seg.parent.size = (pw - w, h);
        if (seg.side is Side.Top or Side.Bottom)
            seg.parent.size = (w, ph - h);

        if (seg.side == Side.Left)
            seg.parent.position = (x + w, y);
        if (seg.side == Side.Top)
            seg.parent.position = (x, y + h);
        // bottom and right cut position stays the same
    }

    private static float Map(float number, (float a, float b) range, (float a, float b) targetRange)
    {
        var value = (number - range.a) / (range.b - range.a) * (targetRange.b - targetRange.a) +
                    targetRange.a;
        return float.IsNaN(value) || float.IsInfinity(value) ? targetRange.a : value;
    }
#endregion
}