using System.IO.Compression;
using System.Xml;
using Pure.Engine.Tilemap;

namespace Pure.Tools.Tilemap;

public static class TiledLoader
{
    public static (string[] layers, TilemapPack maps) Load(string tmxPath)
    {
        var layerList = LoadLayers(tmxPath);
        var result = new TilemapPack();
        var names = new List<string>();

        foreach (XmlElement element in layerList)
        {
            var name = element.Attributes["name"]?.Value;
            var (layer, data) = GetLayer(layerList, tmxPath, name);

            result.Tilemaps.Add(ParseData(layer, data));
            names.Add(name ?? string.Empty);
        }

        return (names.ToArray(), result);
    }
    public static Pure.Engine.Tilemap.Tilemap Load(string tmxPath, string layerName)
    {
        var layers = LoadLayers(tmxPath);
        var (layer, data) = GetLayer(layers, tmxPath, layerName);

        return ParseData(layer, data);
    }

#region Backend
    private static (int, int) IndexToCoords(Pure.Engine.Tilemap.Tilemap tilemap, int index)
    {
        var (w, h) = tilemap.Size;
        index = index < 0 ? 0 : index;
        index = index > w * h - 1 ? w * h - 1 : index;

        return (index % w, index / w);
    }

    private static XmlNodeList LoadLayers(string tmxPath)
    {
        ArgumentNullException.ThrowIfNull(tmxPath);

        if (File.Exists(tmxPath) == false)
            throw new ArgumentException($"No tmx file was found at '{tmxPath}'.");

        var xml = new XmlDocument();
        xml.Load(tmxPath);

        return xml.GetElementsByTagName("layer");
    }
    private static (XmlElement layer, XmlNode data) GetLayer(XmlNodeList layers, string tmxPath, string? layerName)
    {
        var layer = default(XmlElement);
        var data = default(XmlNode);
        var layerFound = false;

        foreach (var element in layers)
        {
            layer = (XmlElement)element;
            data = layer.FirstChild;

            if (data?.Attributes == null)
                continue;

            var name = layer.Attributes["name"]?.Value;
            if (name != layerName)
                continue;

            layerFound = true;
            break;
        }

        if (layerFound == false)
            throw new ArgumentException(
                $"File at '{tmxPath}' does not contain the layer '{layerName}'.");

        if (layer == null || data == null)
            throw new ArgumentException($"Could not parse file at '{tmxPath}', layer '{layerName}'.");

        return (layer, data);
    }
    private static Pure.Engine.Tilemap.Tilemap ParseData(XmlElement layer, XmlNode data)
    {
        var dataStr = data.InnerText.Trim();
        var attributes = data.Attributes;
        var encoding = attributes?["encoding"]?.Value;
        var compression = attributes?["compression"]?.Value;
        _ = int.TryParse(layer.Attributes["width"]?.InnerText, out var mapWidth);
        _ = int.TryParse(layer.Attributes["height"]?.InnerText, out var mapHeight);

        var result = new Pure.Engine.Tilemap.Tilemap((mapWidth, mapHeight));

        if (encoding == "csv")
            LoadFromCSV(result, dataStr);
        else if (encoding == "base64")
        {
            if (compression == null)
                LoadFromBase64Uncompressed(result, dataStr);
            else if (compression == "gzip")
                LoadFromBase64<GZipStream>(result, dataStr);
            else if (compression == "zlib")
                LoadFromBase64<ZLibStream>(result, dataStr);
            else
                throw new ArgumentException($"Tile Layer Format encoding 'Base64' " +
                                            $"with compression '{compression}' is not supported.");
        }
        else
            throw new ArgumentException($"Tile Layer Format encoding '{encoding}' is not supported.");

        return result;
    }

    private static void LoadFromCSV(Pure.Engine.Tilemap.Tilemap tilemap, string dataStr)
    {
        var values = dataStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < values.Length; i++)
        {
            var value = uint.Parse(values[i].Trim());
            SetTile(tilemap, i, value);
        }
    }
    private static void LoadFromBase64Uncompressed(Pure.Engine.Tilemap.Tilemap tilemap, string dataStr)
    {
        var bytes = Convert.FromBase64String(dataStr);
        LoadFromByteArray(tilemap, bytes);
    }
    private static void LoadFromBase64<T>(Pure.Engine.Tilemap.Tilemap tilemap, string dataStr) where T : Stream
    {
        var buffer = Convert.FromBase64String(dataStr);
        using var msi = new MemoryStream(buffer);
        using var mso = new MemoryStream();

        using var compStream = Activator.CreateInstance(
            typeof(T), msi, CompressionMode.Decompress) as T;

        if (compStream == null)
            return;

        CopyTo(compStream, mso);
        var bytes = mso.ToArray();
        LoadFromByteArray(tilemap, bytes);
    }
    private static void LoadFromByteArray(Pure.Engine.Tilemap.Tilemap tilemap, byte[] bytes)
    {
        var size = bytes.Length / sizeof(uint);
        for (var i = 0; i < size; i++)
        {
            var value = BitConverter.ToUInt32(bytes, i * sizeof(uint));
            SetTile(tilemap, i, value);
        }
    }
    private static void CopyTo(Stream src, Stream dest)
    {
        var bytes = new byte[4096];

        int i;
        while ((i = src.Read(bytes, 0, bytes.Length)) != 0)
            dest.Write(bytes, 0, i);
    }

    private static void SetTile(Pure.Engine.Tilemap.Tilemap tilemap, int index, uint value)
    {
        value--;

        if (value == uint.MaxValue)
            value = 0;

        var (x, y) = IndexToCoords(tilemap, index);
        var bits = (IsBitSet(value, 31), IsBitSet(value, 30), IsBitSet(value, 29));
        var pose = Pose.Default;

        if (bits == (true, false, true))
            pose = Pose.Right;
        else if (bits == (true, true, false))
            pose = Pose.Down;
        else if (bits == (false, true, true))
            pose = Pose.Left;
        else if (bits == (true, false, false))
            pose = Pose.Flip;
        else if (bits == (true, true, true))
            pose = Pose.FlipRight;
        else if (bits == (false, true, false))
            pose = Pose.FlipDown;
        else if (bits == (false, false, true))
            pose = Pose.FlipLeft;

        var tile = new Tile
        {
            Pose = pose,
            Tint = uint.MaxValue
        };

        value = ClearBit(value, 31);
        value = ClearBit(value, 30);
        value = ClearBit(value, 29);
        value = ClearBit(value, 28);

        tile.Id = (ushort)value;
        tilemap.SetTile((x, y), tile);
    }
    private static bool IsBitSet(uint value, int bitPosition)
    {
        return (value & (1u << bitPosition)) != 0;
    }
    private static uint ClearBit(uint number, int bitPosition)
    {
        return number & ~(1u << bitPosition);
    }
#endregion
}