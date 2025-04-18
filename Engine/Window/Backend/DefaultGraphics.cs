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