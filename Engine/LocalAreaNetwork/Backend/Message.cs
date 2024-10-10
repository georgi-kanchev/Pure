using System.Runtime.InteropServices;

namespace Pure.Engine.LocalAreaNetwork;

using System.IO.Compression;
using System.Text;

internal class Message
{
    public byte FromId { get; }
    public byte ToId { get; }
    public byte Tag { get; }
    public byte TagSystem { get; }
    public string Value { get; }
    public byte[] Data { get; }
    public byte[] Total { get; }

    public Message(byte fromId, byte toId, byte sysTag, byte userTag, string value, byte[] raw)
    {
        TagSystem = sysTag;
        Tag = userTag;
        FromId = fromId;
        ToId = toId;
        Value = value;

        // [size of compressed data] | [compressed data]
        // this is because some messages may arrive back to back in the same byte[]

        var bytes = new List<byte>();
        var message = Encoding.UTF8.GetBytes(value);
        bytes.AddRange([sysTag, userTag, fromId, toId]);
        bytes.AddRange(BitConverter.GetBytes(message.Length));
        bytes.AddRange(message);
        bytes.AddRange(BitConverter.GetBytes(raw.Length));
        bytes.AddRange(raw);

        var result = new List<byte>();
        var compressed = Compress(bytes.ToArray());
        result.AddRange(BitConverter.GetBytes(compressed.Length));
        result.AddRange(compressed);

        Total = result.ToArray();
    }
    public Message(byte[] bytes, out byte[] remaining)
    {
        if (bytes == null)
        {
            remaining = null;
            return;
        }

        var offset = 0;
        // take a chunk of the bytes just for this message
        // since maybe there are multiple messages back to back
        var byteAmount = BitConverter.ToInt32(Get<int>());
        var myBytes = GetBytesFrom(bytes, byteAmount, ref offset);

        // send the remaining bytes back
        remaining = bytes[offset..];

        // and keep parsing the message...
        var offsetDecoded = 0;
        var decoded = Decompress(myBytes);
        var sysTag = GetByte();
        var userTag = GetByte();
        var fromId = GetByte();
        var toId = GetByte();
        var strByteLength = BitConverter.ToInt32(Get<int>());
        var value = Encoding.UTF8.GetString(GetBytesFrom(decoded, strByteLength, ref offsetDecoded));
        var rawByteLength = BitConverter.ToInt32(Get<int>());
        var raw = GetBytesFrom(decoded, rawByteLength, ref offsetDecoded);

        // then store
        TagSystem = sysTag;
        Tag = userTag;
        FromId = fromId;
        ToId = toId;
        Value = value;
        Data = raw;
        Total = bytes[..(4 + byteAmount)];

        byte GetByte()
        {
            return GetBytesFrom(decoded, 1, ref offsetDecoded)[0];
        }
        byte[] Get<T>()
        {
            return GetBytesFrom(bytes, Marshal.SizeOf(typeof(T)), ref offset);
        }
    }

#region Backend
    private static byte[] Compress(byte[] bytes)
    {
        using var uncompressedStream = new MemoryStream(bytes);
        using var compressedStream = new MemoryStream();
        using (var compressorStream =
               new DeflateStream(compressedStream, CompressionLevel.Fastest, true))
        {
            uncompressedStream.CopyTo(compressorStream);
        }

        var compressedBytes = compressedStream.ToArray();

        return compressedBytes;
    }
    private static byte[] Decompress(byte[] compressedStringBytes)
    {
        var compressedStream = new MemoryStream(compressedStringBytes);

        using var decompressorStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
        using var decompressedStream = new MemoryStream();
        decompressorStream.CopyTo(decompressedStream);

        var decompressedBytes = decompressedStream.ToArray();

        return decompressedBytes;
    }
    private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}