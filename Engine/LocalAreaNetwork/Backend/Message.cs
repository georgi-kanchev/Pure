using System.Text;

namespace Pure.Engine.LocalAreaNetwork;

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
        var arr = bytes.ToArray();
        result.AddRange(BitConverter.GetBytes(arr.Length));
        result.AddRange(arr);

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
        var byteAmount = BitConverter.ToInt32(GetBytesFrom(bytes, 4, ref offset));
        var myBytes = GetBytesFrom(bytes, byteAmount, ref offset);

        // send the remaining bytes back
        remaining = bytes[offset..];

        // and keep parsing the message...
        var offsetDecoded = 0;
        var sysTag = GetBytesFrom(myBytes, 1, ref offsetDecoded)[0];
        var userTag = GetBytesFrom(myBytes, 1, ref offsetDecoded)[0];
        var fromId = GetBytesFrom(myBytes, 1, ref offsetDecoded)[0];
        var toId = GetBytesFrom(myBytes, 1, ref offsetDecoded)[0];
        var strByteLength = BitConverter.ToInt32(GetBytesFrom(bytes, 4, ref offset));
        var value = Encoding.UTF8.GetString(GetBytesFrom(myBytes, strByteLength, ref offsetDecoded));
        var rawByteLength = BitConverter.ToInt32(GetBytesFrom(bytes, 4, ref offset));
        var raw = GetBytesFrom(myBytes, rawByteLength, ref offsetDecoded);

        // then store
        TagSystem = sysTag;
        Tag = userTag;
        FromId = fromId;
        ToId = toId;
        Value = value;
        Data = raw;
        Total = bytes[..(4 + byteAmount)];
    }

#region Backend
    private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}