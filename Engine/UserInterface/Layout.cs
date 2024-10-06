namespace Pure.Engine.UserInterface;

public enum Side
{
    Left, Right, Top, Bottom
}

public class Layout : Block
{
    public int Count
    {
        get => segments.Count;
    }

    public Layout((int x, int y) position = default) : base(position)
    {
        Init();
        Size = (12, 12);
        Restore();
    }
    public Layout(byte[] bytes) : base(bytes)
    {
        Init();
        var b = Decompress(bytes);
        var count = GrabByte(b);

        // all segments need to be added before parent linking
        var parentIndexes = new List<int>();

        for (var i = 0; i < count; i++)
        {
            var rate = GrabFloat(b);
            var cutSide = (Side)GrabByte(b);
            var parentIndex = GrabInt(b);

            parentIndexes.Add(parentIndex);
            var seg = new Segment(rate, cutSide, null);
            segments.Add(seg);
        }

        for (var i = 0; i < segments.Count; i++)
        {
            var parentIndex = parentIndexes[i];
            segments[i].parent = parentIndex == -1 ? null : segments[parentIndex];
        }
    }
    public Layout(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public override string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public override byte[] ToBytes()
    {
        var bytes = Decompress(base.ToBytes()).ToList();
        PutByte(bytes, (byte)segments.Count);

        foreach (var seg in segments)
        {
            var parentIndex = seg.parent == null ? -1 : segments.IndexOf(seg.parent);
            PutFloat(bytes, seg.rate);
            PutByte(bytes, (byte)seg.side);
            PutInt(bytes, parentIndex);
        }

        return Compress(bytes.ToArray());
    }

    public void Cut(int index, Side side, float rate)
    {
        if (index < 0 || index >= segments.Count)
            return;

        var seg = new Segment(rate, side, segments[index]);
        segments.Add(seg);
    }
    public void Cut(int index, Side side, int size)
    {
        var rate = (float)size / (side is Side.Top or Side.Bottom ? Height : Width);
        Cut(index, side, rate);
    }
    public void Restore()
    {
        segments.Clear();
        segments.Add(new(0, Side.Top, null));
    }

    public void OnDisplaySegment(Action<(int x, int y, int width, int height), int> method)
    {
        displaySegment += method;
    }

    public Layout Duplicate()
    {
        return new(ToBytes());
    }

    public static implicit operator byte[](Layout layout)
    {
        return layout.ToBytes();
    }
    public static implicit operator Layout(byte[] bytes)
    {
        return new(bytes);
    }

#region Backend
    private class Segment
    {
        public readonly float rate;
        public readonly Side side;
        public Segment? parent;

        public (int x, int y) position;
        public (int w, int h) size;

        public Segment(float rate, Side side, Segment? parent)
        {
            this.rate = Math.Clamp(rate, 0, 1);
            this.side = side;
            this.parent = parent;
        }
    }

    private readonly List<Segment> segments = new();

    internal Action<(int x, int y, int width, int height), int>? displaySegment;

    private void Init()
    {
        OnUpdate(OnUpdate);
    }
    internal void OnUpdate()
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
            }
            else
                UpdateSegment(seg);
        }

        for (var i = 0; i < segments.Count; i++)
        {
            var seg = segments[i];
            displaySegment?.Invoke((seg.position.x, seg.position.y, seg.size.w, seg.size.h), i);
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