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
    private const string DEFAULT_GRAPHICS_BASE64 =
        "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";
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