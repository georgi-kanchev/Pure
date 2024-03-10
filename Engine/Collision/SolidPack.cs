namespace Pure.Engine.Collision;

using System.Runtime.InteropServices;

public class SolidPack : Pack<Solid>
{
    public SolidPack((float x, float y) offset = default, (float width, float height) scale = default)
        : base(offset, scale)
    {
    }
    public SolidPack(
        (float x, float y) offset = default,
        (float width, float height) scale = default,
        params Solid[] solids)
        : base(offset, scale, solids)
    {
    }
    public SolidPack(byte[] bytes)
    {
        var b = Decompress(bytes);
        var offset = 0;

        var count = BitConverter.ToInt32(Get<int>());

        Offset = (BitConverter.ToSingle(Get<float>()), BitConverter.ToSingle(Get<float>()));
        Scale = (BitConverter.ToSingle(Get<float>()), BitConverter.ToSingle(Get<float>()));

        for (var i = 0; i < count; i++)
        {
            var x = BitConverter.ToSingle(Get<float>());
            var y = BitConverter.ToSingle(Get<float>());
            var w = BitConverter.ToSingle(Get<float>());
            var h = BitConverter.ToSingle(Get<float>());
            var color = BitConverter.ToUInt32(Get<uint>());

            Add(new Solid((w, h), (x, y), color));
        }

        byte[] Get<T>()
        {
            return GetBytesFrom(b, Marshal.SizeOf(typeof(T)), ref offset);
        }
    }
    public SolidPack(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public override byte[] ToBytes()
    {
        var result = new List<byte>();
        result.AddRange(BitConverter.GetBytes(data.Count));
        result.AddRange(BitConverter.GetBytes(Offset.x));
        result.AddRange(BitConverter.GetBytes(Offset.y));
        result.AddRange(BitConverter.GetBytes(Scale.width));
        result.AddRange(BitConverter.GetBytes(Scale.height));

        foreach (var r in data)
        {
            result.AddRange(BitConverter.GetBytes(r.Position.x));
            result.AddRange(BitConverter.GetBytes(r.Position.y));
            result.AddRange(BitConverter.GetBytes(r.Size.width));
            result.AddRange(BitConverter.GetBytes(r.Size.height));
            result.AddRange(BitConverter.GetBytes(r.Color));
        }

        return Compress(result.ToArray());
    }
    public (float x, float y, float width, float height, uint color)[] ToBundle()
    {
        var result = new (float x, float y, float width, float height, uint color)[data.Count];
        for (var i = 0; i < result.Length; i++)
            result[i] = this[i];
        return result;
    }

    public bool IsOverlapping(LinePack linePack)
    {
        return linePack.IsOverlapping(this);
    }
    public bool IsOverlapping(SolidPack solidPack)
    {
        for (var i = 0; i < Count; i++)
            if (solidPack.IsOverlapping(this[i]))
                return true;

        return false;
    }
    public bool IsOverlapping(Solid solid)
    {
        for (var i = 0; i < Count; i++)
            if (this[i].IsOverlapping(solid))
                return true;

        return false;
    }
    public bool IsOverlapping(Line line)
    {
        for (var i = 0; i < data.Count; i++)
            if (this[i].IsOverlapping(line))
                return true;

        return false;
    }
    public bool IsOverlapping((float x, float y) point)
    {
        for (var i = 0; i < Count; i++)
            if (this[i].IsOverlapping(point))
                return true;

        return false;
    }

    public SolidPack Duplicate()
    {
        return new(ToBytes());
    }

    public static implicit operator SolidPack(Solid[] solids)
    {
        return new(default, default, solids);
    }
    public static implicit operator Solid[](SolidPack solidPack)
    {
        return solidPack.ToArray();
    }
    public static implicit operator (float x, float y, float width, float height, uint color)[](
        SolidPack solidPack)
    {
        return solidPack.ToBundle();
    }
    public static implicit operator SolidPack(
        (float x, float y, float width, float height, uint color)[] solids)
    {
        var result = new Solid[solids.Length];
        for (var i = 0; i < result.Length; i++)
            result[i] = solids[i];
        return result;
    }
    public static implicit operator byte[](SolidPack solidPack)
    {
        return solidPack.ToBytes();
    }
    public static implicit operator SolidPack(byte[] bytes)
    {
        return new(bytes);
    }

#region Backend
    protected override Solid LocalToGlobal(Solid localRect)
    {
        var (x, y) = localRect.Position;
        var (w, h) = localRect.Size;
        localRect.Position = (Offset.x + x * Scale.width, Offset.y + y * Scale.height);
        localRect.Size = (w * Scale.width, h * Scale.height);
        return localRect;
    }
#endregion
}