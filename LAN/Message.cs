using System.IO.Compression;
using System.Text;

namespace Pure.LAN;

internal class Message
{
	public byte FromID { get; }
	public byte ToID { get; }
	public byte Tag { get; }
	public byte TagSystem { get; }
	public string Value { get; }
	public byte[] Data { get; }

	public Message(byte fromID, byte toID, byte sysTag, byte userTag, string value)
	{
		this.TagSystem = sysTag;
		this.Tag = userTag;
		this.FromID = fromID;
		this.ToID = toID;
		this.Value = value;

		var message = Encoding.UTF8.GetBytes(value);
		var bytes = new byte[4 + message.Length];
		bytes[0] = sysTag;
		bytes[1] = userTag;
		bytes[2] = fromID;
		bytes[3] = toID;
		Array.Copy(message, 0, bytes, 4, message.Length);

		var msg = Compress(bytes);
		var result = new byte[4 + msg.Length];
		var msgSize = BitConverter.GetBytes(msg.Length);
		result[0] = msgSize[0];
		result[1] = msgSize[1];
		result[2] = msgSize[2];
		result[3] = msgSize[3];
		Array.Copy(msg, 0, result, 4, msg.Length);

		Data = result;
	}
	public Message(byte[] bytes, out byte[] remaining)
	{
		// take a chunk of the bytes just for this message, maybe there are multiple messages
		var byteAmount = BitConverter.ToInt32(bytes[..4]);
		var myBytes = bytes[4..(4 + byteAmount)];

		remaining = bytes[(4 + myBytes.Length)..];

		// decompress
		var decoded = Decompress(myBytes);
		var sysTag = decoded[0];
		var userTag = decoded[1];
		var fromID = decoded[2];
		var toID = decoded[3];
		var value = Encoding.UTF8.GetString(decoded[4..]);

		// and store
		this.TagSystem = sysTag;
		this.Tag = userTag;
		this.FromID = fromID;
		this.ToID = toID;
		this.Value = value;
		this.Data = bytes[..(4 + byteAmount)];
	}

	#region Backend
	// format
	// [amount of bytes]		- data
	// --------------------------------
	// [4]						- total amount of bytes for this message (after compression)
	// --------------------------------
	// compressed:
	// [1]						- system tag
	// [1]						- user tag
	// [1]						- fromID
	// [1]						- toID
	// [rest]					- string message

	private static byte[] Compress(byte[] bytes)
	{
		using var uncompressedStream = new MemoryStream(bytes);
		using var compressedStream = new MemoryStream();
		using (var compressorStream = new DeflateStream(compressedStream, CompressionLevel.Fastest, true))
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
	#endregion
}
