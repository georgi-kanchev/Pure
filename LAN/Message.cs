using System.IO.Compression;
using System.Text;

namespace Pure.LAN;

public class Message
{
	public byte FromID { get; }
	public byte ToID { get; }
	public string? Value { get; }
	public string? FromNickname { get; internal set; }
	public string? ToNickname { get; internal set; }

	internal byte Tag { get; }
	internal byte[]? Data { get; }

	private Message() { }
	internal Message(byte fromID, byte toID, byte tag, string value)
	{
		this.FromID = fromID;
		this.ToID = toID;
		this.Value = value;
		this.Tag = tag;

		var message = Encoding.UTF8.GetBytes(value);
		var bytes = new byte[3 + message.Length];
		bytes[0] = fromID;
		bytes[1] = toID;
		bytes[2] = tag;
		Array.Copy(message, 0, bytes, 3, message.Length);

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
	internal Message(byte[] bytes)
	{
		var b = Decompress(bytes);
		var fromID = b[0];
		var toID = b[1];
		var tag = b[2];
		var message = new byte[b.Length - 3];

		Array.Copy(b, 3, message, 0, message.Length);
		var value = Encoding.UTF8.GetString(message);

		this.FromID = fromID;
		this.ToID = toID;
		this.Value = value;
		this.Data = b;
	}

	#region Backend
	// format
	// [amount of bytes]		- data
	// --------------------------------
	// [4]						- message amount of bytes
	// [1]						- fromID / 0 for server
	// [1]						- toID / 0 for server
	// [1]						- tag
	// [rest]					- string message

	private static byte[] Compress(byte[] bytes)
	{
		byte[] compressedBytes;

		using (var uncompressedStream = new MemoryStream(bytes))
		{
			using var compressedStream = new MemoryStream();
			using (var compressorStream = new DeflateStream(compressedStream, CompressionLevel.Fastest, true))
			{
				uncompressedStream.CopyTo(compressorStream);
			}

			compressedBytes = compressedStream.ToArray();
		}

		return compressedBytes;
	}
	private static byte[] Decompress(byte[] compressedStringBytes)
	{
		byte[] decompressedBytes;

		var compressedStream = new MemoryStream(compressedStringBytes);

		using (var decompressorStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
		{
			using var decompressedStream = new MemoryStream();
			decompressorStream.CopyTo(decompressedStream);

			decompressedBytes = decompressedStream.ToArray();
		}

		return decompressedBytes;
	}
	#endregion
}
