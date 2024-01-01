namespace Pure.Engine.Collision;

using System.IO.Compression;
using System.Runtime.InteropServices;

public class SolidPack
{
    public (float x, float y) Position { get; set; }
    public (float width, float height) Scale { get; set; }
    public int SolidsCount
    {
        get => data.Count;
    }

    public Solid this[int index]
    {
        get => LocalToGlobalRectangle(data[index]);
        set => data[index] = value;
    }

    public SolidPack(byte[] bytes) : this((0, 0), (0, 0))
    {
        var b = Decompress(bytes);
        var offset = 0;

        var count = BitConverter.ToInt32(Get<int>());

        Position = (BitConverter.ToSingle(Get<float>()), BitConverter.ToSingle(Get<float>()));
        Scale = (BitConverter.ToSingle(Get<float>()), BitConverter.ToSingle(Get<float>()));

        for (var i = 0; i < count; i++)
        {
            var x = BitConverter.ToSingle(Get<float>());
            var y = BitConverter.ToSingle(Get<float>());
            var w = BitConverter.ToSingle(Get<float>());
            var h = BitConverter.ToSingle(Get<float>());
            var color = BitConverter.ToUInt32(Get<uint>());

            SolidsAdd(new Solid((w, h), (x, y), color));
        }

        return;

        byte[] Get<T>()
        {
            return GetBytesFrom(b, Marshal.SizeOf(typeof(T)), ref offset);
        }
    }
    public SolidPack((float x, float y) position, (float width, float height) scale = default)
    {
        scale = scale == default ? (1f, 1f) : scale;

        Position = position;
        Scale = scale;
    }
    public SolidPack(
        (float x, float y) position,
        (float width, float height) scale = default,
        params Solid[] solids)
        : this(position, scale)
    {
        SolidsAdd(solids);
    }

    public byte[] ToBytes()
    {
        var c = data.Count;
        var bX = BitConverter.GetBytes(Position.x);
        var bY = BitConverter.GetBytes(Position.y);
        var bScW = BitConverter.GetBytes(Scale.width);
        var bScH = BitConverter.GetBytes(Scale.height);
        var bCount = BitConverter.GetBytes(c);
        var result = new List<byte>();

        result.AddRange(bX);
        result.AddRange(bY);
        result.AddRange(bScW);
        result.AddRange(bScH);
        result.AddRange(bCount);
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

    public void SolidsAdd(params Solid[]? solids)
    {
        if (solids == null || solids.Length == 0)
            return;

        data.AddRange(solids);
    }

    public bool IsOverlapping(SolidPack solidPack)
    {
        for (var i = 0; i < SolidsCount; i++)
            if (solidPack.IsOverlapping(this[i]))
                return true;

        return false;
    }
    public bool IsOverlapping(Solid solid)
    {
        for (var i = 0; i < SolidsCount; i++)
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
        for (var i = 0; i < SolidsCount; i++)
            if (this[i].IsOverlapping(point))
                return true;

        return false;
    }

    public Solid[] ToArray()
    {
        return this;
    }
    public (float x, float y, float width, float height, uint color)[] ToBundle()
    {
        return this;
    }

    public static implicit operator SolidPack(Solid[] solids)
    {
        return new(default, default, solids);
    }
    public static implicit operator Solid[](SolidPack solidPack)
    {
        return solidPack.data.ToArray();
    }
    public static implicit operator (float x, float y, float width, float height, uint color)[]
        (SolidPack solidPack)
    {
        var result =
            new (float x, float y, float width, float height, uint color)[solidPack.data.Count];
        for (var i = 0; i < result.Length; i++)
            result[i] = solidPack[i];
        return result;
    }
    public static implicit operator SolidPack(
        (float x, float y, float width, float height, uint color)[] solids)
    {
        var result = new Solid[solids.Length];
        for (var i = 0; i < result.Length; i++)
            result[i] = solids[i];
        return result;
    }

    #region Backend
    // save format in sectors
    // [amount of bytes]	- data
    // --------------------------------
    // [4]					- x
    // [4]					- y
    // [4]					- scale width
    // [4]					- scale height
    // [4]					- count
    // = = = = = = (sector 1)
    // [4]					- x
    // [4]					- y
    // [4]					- width
    // [4]					- height
    // [4]					- color
    // = = = = = = (sector 2)
    // [4]					- x
    // [4]					- y
    // [4]					- width
    // [4]					- height
    // [4]					- color
    // = = = = = = (sector 3)
    // ... up to sector [count]

    private readonly List<Solid> data = new();

    private static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var stream = new DeflateStream(output, CompressionLevel.Optimal)) stream.Write(data, 0, data.Length);

        return output.ToArray();
    }
    private static byte[] Decompress(byte[] data)
    {
        var input = new MemoryStream(data);
        var output = new MemoryStream();
        using (var stream = new DeflateStream(input, CompressionMode.Decompress)) stream.CopyTo(output);

        return output.ToArray();
    }
    private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }

    private Solid LocalToGlobalRectangle(Solid localRect)
    {
        var (x, y) = localRect.Position;
        var (w, h) = localRect.Size;
        localRect.Position = (Position.x + x * Scale.width, Position.y + y * Scale.height);
        localRect.Size = (w * Scale.width, h * Scale.height);
        return localRect;
    }
    #endregion
}