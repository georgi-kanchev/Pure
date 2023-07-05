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

    public Layout((int x, int y) position) : base(position) { }
    public Layout(byte[] bytes) : base(bytes)
    {
        var offset = 0;
        var count = GetInt();

        for (var i = 0; i < count; i++)
        {
            var seg = new Segment(GetInt(), GetFloat(), (CutSide)GetByte(), GetInt(), this);
            segments.Add(seg);
        }

        int GetInt() => BitConverter.ToInt32(GetBytes(bytes, 4, ref offset));
        float GetFloat() => BitConverter.ToSingle(GetBytes(bytes, 4, ref offset));
        byte GetByte() => GetBytes(bytes, 1, ref offset)[0];
    }

    public Element? this[int index]
    {
        get => ui?[segments[index].elementIndex];
        set
        {
            if (ui == null)
                return;

            segments[index].elementIndex = ui.IndexOf(value);
        }
    }

    public void Cut(int index, CutSide side, float percent)
    {
        var seg = new Segment(-1, percent, side, index, this);
        segments.Add(seg);
    }

    public void Restore() => segments.Clear();

    public override byte[] ToBytes()
    {
        var bytes = new List<byte>();

        if (ui == null)
            return bytes.ToArray();

        bytes.AddRange(BitConverter.GetBytes(segments.Count));
        foreach (var seg in segments)
        {
            bytes.AddRange(BitConverter.GetBytes(seg.elementIndex));
            bytes.AddRange(BitConverter.GetBytes(seg.percent));
            bytes.AddRange(BitConverter.GetBytes((byte)seg.cutSide));
            bytes.AddRange(BitConverter.GetBytes(seg.parentSegIndex));
        }

        return bytes.ToArray();
    }

#region Backend
    private class Segment
    {
        public Layout owner;

        public int elementIndex;
        public readonly float percent;
        public readonly CutSide cutSide;
        public readonly int parentSegIndex;

        public (int x, int y) Position => default;
        public (int w, int h) Size => default;

        public Segment(int elementIndex, float percent, CutSide cutSide, int parentSegIndex,
            Layout owner)
        {
            this.elementIndex = elementIndex;
            this.percent = percent;
            this.cutSide = cutSide;
            this.parentSegIndex = parentSegIndex;
            this.owner = owner;
        }
    }

    private readonly List<Segment> segments = new();
    internal UserInterface? ui;

    private static byte[] GetBytes(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}