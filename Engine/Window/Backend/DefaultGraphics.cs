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