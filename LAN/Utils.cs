using System.IO.Compression;
using System.Text;

namespace Pure.LAN;

internal static class Utils
{
	public static byte[] Compress(string text)
	{
		byte[] compressedBytes;

		using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
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
	public static string Decompress(byte[] compressedStringBytes)
	{
		byte[] decompressedBytes;

		var compressedStream = new MemoryStream(compressedStringBytes);

		using (var decompressorStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
		{
			using var decompressedStream = new MemoryStream();
			decompressorStream.CopyTo(decompressedStream);

			decompressedBytes = decompressedStream.ToArray();
		}

		return Encoding.UTF8.GetString(decompressedBytes);
	}
}