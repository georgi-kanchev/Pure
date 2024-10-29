using System.IO.Compression;
using System.Text;

namespace Pure.Engine.Storage;

public class StoragePack
{
    public byte[]? this[string key]
    {
        get => data.GetValueOrDefault(key);
        set => Set(key, value);
    }

    public byte[]? Get(string key)
    {
        return data.GetValueOrDefault(key);
    }
    public void Set(string key, byte[]? value)
    {
        if (value == default)
        {
            Remove(key);
            return;
        }

        data[key] = value;
    }
    public void Remove(string key)
    {
        data.Remove(key);
    }
    public bool IsContaining(string key)
    {
        return data.ContainsKey(key);
    }

    public StoragePack()
    {
    }
    public StoragePack(byte[] bytes)
    {
        var b = Decompress(bytes);
        var offset = 0;
        var sectorCount = BitConverter.ToInt32(GetBytesFrom(b, 4, ref offset));

        for (var i = 0; i < sectorCount; i++)
        {
            var keyLength = BitConverter.ToInt32(GetBytesFrom(b, 4, ref offset));
            var key = BytesToText(GetBytesFrom(b, keyLength, ref offset));
            var dataLength = BitConverter.ToInt32(GetBytesFrom(b, 4, ref offset));
            var dataBytes = GetBytesFrom(b, dataLength, ref offset);

            data[key] = dataBytes;
        }
    }
    public StoragePack(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public byte[] ToBytes()
    {
        var result = new List<byte>();
        result.AddRange(BitConverter.GetBytes(data.Count));

        foreach (var kvp in data)
        {
            var bKey = BytesFromText(kvp.Key);
            result.AddRange(BitConverter.GetBytes(bKey.Length));
            result.AddRange(bKey);
            result.AddRange(BitConverter.GetBytes(kvp.Value.Length));
            result.AddRange(kvp.Value);
        }

        return Compress(result.ToArray());
    }

    public StoragePack Duplicate()
    {
        return new(ToBytes());
    }

    public static implicit operator StoragePack(byte[] bytes)
    {
        return new(bytes);
    }
    public static implicit operator byte[](StoragePack storage)
    {
        return storage.ToBytes();
    }

#region Backend
    // save format in sectors
    // [amount of bytes]		- data
    // --------------------------------
    // [4]						- amount of sectors
    // = = = = = = (sector 1)
    // [4]						- key length
    // [key length]				- string key
    // [4]						- data length
    // [data length]			- data
    // = = = = = = (sector 2)
    // [4]						- key length
    // [key length]				- string key
    // [4]						- data length
    // [data length]			- data
    // = = = = = = (sector 3)
    // ...

    private readonly Dictionary<string, byte[]> data = new();

    private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }

    internal static byte[] Compress(byte[] data)
    {
        using var compressedStream = new MemoryStream();
        using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
        {
            gzipStream.Write(data, 0, data.Length);
        }

        return compressedStream.ToArray();
    }
    internal static byte[] Decompress(byte[] compressedData)
    {
        using var compressedStream = new MemoryStream(compressedData);
        using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        gzipStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }

    private static byte[] BytesFromText(string text)
    {
        return Convert.FromBase64String(Convert.ToBase64String(Encoding.UTF8.GetBytes(text)));
    }
    private static string BytesToText(byte[] bytes)
    {
        return Encoding.UTF8.GetString(Convert.FromBase64String(Convert.ToBase64String(bytes)));
    }
#endregion
}