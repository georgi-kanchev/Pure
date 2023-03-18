using System.IO.Compression;
using System.Net;
using System.Text;

namespace Pure.LAN;

public class Message
{
	/*
	public string? FromIP { get; }
	public string? ToIP { get; }
	public string? Value { get; }
	public string? FromNickname { get; internal set; }
	public string? ToNickname { get; internal set; }

	internal byte Tag { get; }
	internal byte[]? Data { get; }

	private Message() { }
	internal Message(string? fromIP, string? toIP, byte tag, string value)
	{
		this.FromIP = fromIP;
		this.ToIP = toIP;
		this.Value = value;
		this.Tag = tag;

		var bFromIP = fromIP == null ? new byte[4] : IPAddress.Parse(fromIP).GetAddressBytes();
		var bToIP = toIP == null ? new byte[4] : IPAddress.Parse(toIP).GetAddressBytes();
		var message = Encoding.UTF8.GetBytes(value);
		var bytes = new byte[1 + bFromIP.Length + bToIP.Length + message.Length];
		bytes[0] = tag;

		Array.Copy(bFromIP, 0, bytes, 1, bFromIP.Length);
		Array.Copy(bToIP, 0, bytes, 1 + bFromIP.Length, bToIP.Length);
		Array.Copy(message, 0, bytes, 1 + bFromIP.Length + bToIP.Length, message.Length);

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
		var tag = b[0];
		var fromID = new IPAddress(new byte[] { b[1], b[2], b[3], b[4] });
		var toID = new IPAddress(new byte[] { b[5], b[6], b[7], b[8] });
		var message = new byte[b.Length - 9];

		Array.Copy(b, 9, message, 0, message.Length);
		var value = Encoding.UTF8.GetString(message);

		this.FromIP = fromID.ToString();
		this.ToIP = toID.ToString();
		this.Value = value;
		this.Tag = tag;
	}

	public override string ToString()
	{
		return $"{FromIP}: {Value}";
	}

	#region Backend
	// format
	// [amount of bytes]		- data
	// --------------------------------
	// [4]						- total amount of compressed bytes
	// --------------------------------
	// compressed:
	// [1]						- tag
	// [4]						- fromIP bytes
	// [4]						- toIP bytes
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
	*/
}
