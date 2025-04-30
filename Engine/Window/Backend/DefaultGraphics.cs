using System.IO.Compression;

namespace Pure.Engine.Window;

internal static class DefaultGraphics
{
	public static string PngToBase64String(string pngPath)
	{
		var img = new Image(pngPath);
		var bytes = Compress(img.Pixels);
		var base64 = Convert.ToBase64String(bytes);
		img.Dispose();
		return base64;
	}
	public static Texture CreateTexture()
	{
		var base64 = Convert.FromBase64String(DEFAULT_GRAPHICS_BASE64);
		var bytes = Decompress(base64);
		var img = new Image(208, 208, bytes);
		var result = new Texture(img) { Repeated = true };
		img.Dispose();
		return result;
	}

#region Backend
	private const string DEFAULT_GRAPHICS_BASE64 = "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";
	private static byte[] Compress(this byte[]? data)
	{
		try
		{
			if (data == null || data.Length == 0)
				return [];

			using var memoryStream = new MemoryStream();
			using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal, true))
			{
				gzipStream.Write(data, 0, data.Length);
			}

			return memoryStream.ToArray();
		}
		catch (Exception)
		{
			return data ?? [];
		}
	}
	private static byte[] Decompress(this byte[]? compressedData)
	{
		try
		{
			if (compressedData == null || compressedData.Length == 0)
				return [];

			using var compressedStream = new MemoryStream(compressedData);
			using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
			using var resultStream = new MemoryStream();
			gzipStream.CopyTo(resultStream);
			return resultStream.ToArray();
		}
		catch (Exception)
		{
			return compressedData ?? [];
		}
	}
#endregion
}