namespace Pure.Engine.Collision;

using System.IO.Compression;

public abstract class Pack<T>
{
    public (float x, float y) Offset { get; set; }
    public (float width, float height) Scale { get; set; }
    public int Count
    {
        get => data.Count;
    }

    public T this[int index]
    {
        get => LocalToGlobal(data[index]);
        set => data[index] = value;
    }

    public Pack((float x, float y) offset = default, (float width, float height) scale = default)
    {
        scale = scale == default ? (1f, 1f) : scale;

        Offset = offset;
        Scale = scale;
    }
    public Pack(
        (float x, float y) offset = default,
        (float width, float height) scale = default,
        params T[] data)
        : this(offset, scale)
    {
        Add(data);
    }

    public string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public abstract byte[] ToBytes();
    public T[] ToArray()
    {
        return data.ToArray();
    }

    public void Add(params T[]? lines)
    {
        if (lines == null || lines.Length == 0)
            return;

        data.AddRange(lines);
    }
    public void Remove(params T[]? lines)
    {
        if (lines == null || lines.Length == 0)
            return;

        foreach (var line in lines)
            data.Remove(line);
    }

#region Backend
    protected readonly List<T> data = new();

    protected abstract T LocalToGlobal(T local);

    protected static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
            stream.Write(data, 0, data.Length);

        return output.ToArray();
    }
    protected static byte[] Decompress(byte[] data)
    {
        var input = new MemoryStream(data);
        var output = new MemoryStream();
        using (var stream = new DeflateStream(input, CompressionMode.Decompress)) stream.CopyTo(output);

        return output.ToArray();
    }
    protected static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}