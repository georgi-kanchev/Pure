namespace Pure.UserInterface;

public class Layout : Element
{
    public enum CutSide
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public int Count
    {
        get => segments.Count;
    }

    public Layout((int x, int y) position = default)
        : base(position)
    {
        Size = (12, 12);
        Restore();
    }
    public Layout(byte[] bytes)
        : base(bytes)
    {
        Restore();

        var offset = 0;
        var count = GetInt();

        // all segments need to be added before parent linking
        var parentIndexes = new List<int>();

        for (var i = 0; i < count; i++)
        {
            var rate = GetFloat();
            var cutSide = (CutSide)GetByte();

            parentIndexes.Add(GetInt());
            var seg = new Segment(rate, cutSide, null);
            segments.Add(seg);
        }

        for (var i = 0; i < segments.Count; i++)
            segments[i].parent = segments[parentIndexes[i]];
        return;

        int GetInt()
        {
            return BitConverter.ToInt32(GetBytes(bytes, 4, ref offset));
        }
        float GetFloat()
        {
            return BitConverter.ToSingle(GetBytes(bytes, 4, ref offset));
        }
        byte GetByte()
        {
            return GetBytes(bytes, 1, ref offset)[0];
        }
    }

    public void Cut(int index, CutSide side, float rate)
    {
        if (index < 0 || index >= segments.Count)
            return;

        var seg = new Segment(rate, side, segments[index]);
        segments.Add(seg);
    }
    public void Restore()
    {
        segments.Clear();
        segments.Add(new(0, CutSide.Top, null));
    }

    protected virtual void OnSegmentUpdate((int x, int y, int width, int height) segment, int index)
    {
    }

    public override byte[] ToBytes()
    {
        var bytes = base.ToBytes().ToList();

        if (ui == null)
            return bytes.ToArray();

        bytes.AddRange(BitConverter.GetBytes(segments.Count));
        foreach (var seg in segments)
        {
            var parentIndex = seg.parent == null ? -1 : segments.IndexOf(seg.parent);
            bytes.AddRange(BitConverter.GetBytes(seg.rate));
            bytes.AddRange(BitConverter.GetBytes((byte)seg.cutSide));
            bytes.AddRange(BitConverter.GetBytes(parentIndex));
        }

        return bytes.ToArray();
    }

    public void OnDisplaySegment(Action<(int x, int y, int width, int height), int> method)
    {
        displaySegment += method;
    }

#region Backend
    private class Segment
    {
        public readonly float rate;
        public readonly CutSide cutSide;
        public Segment? parent;

        public (int x, int y) position;
        public (int w, int h) size;

        public Segment(float rate, CutSide cutSide, Segment? parent)
        {
            this.rate = Math.Clamp(rate, 0, 1);
            this.cutSide = cutSide;
            this.parent = parent;
        }
    }

    private readonly List<Segment> segments = new();
    internal UserInterface? ui;

    // used in the UI class to receive callbacks
    internal Action<(int x, int y, int width, int height), int>? displaySegment;

    internal override void OnUpdate()
    {
        if (ui == null)
            return;

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
            OnSegmentUpdate((seg.position.x, seg.position.y, seg.size.w, seg.size.h), i);
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
        if (seg.cutSide is CutSide.Left or CutSide.Top)
            seg.position = (px, py);
        else if (seg.cutSide == CutSide.Right)
            seg.position = ((int)Map(seg.rate, (0, 1), (px + pw, px)), py);
        else if (seg.cutSide == CutSide.Bottom)
            seg.position = (px, (int)Map(seg.rate, (0, 1), (py + ph, py)));

        if (seg.cutSide == CutSide.Left)
            seg.size = ((int)Map(seg.rate, (0, 1), (0, pw)), ph);
        else if (seg.cutSide == CutSide.Right)
            seg.size = ((int)MathF.Ceiling(Map(seg.rate, (0, 1), (0, pw))), ph);
        else if (seg.cutSide == CutSide.Top)
            seg.size = (pw, (int)Map(seg.rate, (0, 1), (0, ph)));
        else if (seg.cutSide == CutSide.Bottom)
            seg.size = (pw, (int)MathF.Ceiling(Map(seg.rate, (0, 1), (0, ph))));

        // update its parent
        if (seg.parent == null)
            return;

        var (x, y) = seg.position;
        var (w, h) = seg.size;

        if (seg.cutSide is CutSide.Left or CutSide.Right)
            seg.parent.size = (pw - w, h);
        if (seg.cutSide is CutSide.Top or CutSide.Bottom)
            seg.parent.size = (w, ph - h);

        if (seg.cutSide == CutSide.Left)
            seg.parent.position = (x + w, y);
        if (seg.cutSide == CutSide.Top)
            seg.parent.position = (x, y + h);
        // bottom and right cut position stays the same
    }

    private static float Map(float number, (float a, float b) range, (float a, float b) targetRange)
    {
        var value = (number - range.a) / (range.b - range.a) * (targetRange.b - targetRange.a) +
                    targetRange.a;
        return float.IsNaN(value) || float.IsInfinity(value) ? targetRange.a : value;
    }
    private static byte[] GetBytes(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}