using System.IO.Compression;
using Pure.Engine.Tilemap;
using Pure.Engine.Utilities;

namespace Pure.Tools.Tilemap;

public class MapGenerator
{
    public Noise Noise { get; set; } = Noise.ValueCubic;
    public float Scale { get; set; } = 10f;
    public int Seed { get; set; }
    public (int x, int y) Offset { get; set; }
    public int TargetTileId { get; set; }

    public SortedDictionary<byte, Tile> Elevations { get; } = new();

    public MapGenerator()
    {
    }
    public MapGenerator(byte[] bytes)
    {
        var b = Decompress(bytes);
        var offset = 0;

        Noise = (Noise)GetByte();
        Scale = GetFloat();
        Seed = GetInt();
        Offset = (GetInt(), GetInt());
        TargetTileId = GetInt();

        var count = GetByte();
        for (var i = 0; i < count; i++)
        {
            var depth = GetByte();
            var tile = new Tile(GetBytesFrom(b, 7, ref offset));
            Elevations[depth] = tile;
        }

        byte GetByte()
        {
            return GetBytesFrom(b, 1, ref offset)[0];
        }

        int GetInt()
        {
            return BitConverter.ToInt32(GetBytesFrom(b, 4, ref offset));
        }

        float GetFloat()
        {
            return BitConverter.ToSingle(GetBytesFrom(b, 4, ref offset));
        }
    }
    public MapGenerator(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public byte[] ToBytes()
    {
        var result = new List<byte> { (byte)Noise };
        result.AddRange(BitConverter.GetBytes(Scale));
        result.AddRange(BitConverter.GetBytes(Seed));
        result.AddRange(BitConverter.GetBytes(Offset.x));
        result.AddRange(BitConverter.GetBytes(Offset.y));
        result.AddRange(BitConverter.GetBytes(TargetTileId));

        result.Add((byte)Elevations.Count);
        foreach (var (depth, tile) in Elevations)
        {
            result.Add(depth);
            var bTiles = tile.ToBytes();
            result.AddRange(bTiles);
        }

        return Compress(result.ToArray());
    }

    public void Apply(Engine.Tilemap.Tilemap? tilemap)
    {
        if (tilemap == null || Elevations.Count == 0)
            return;

        var (w, h) = tilemap.Size;

        for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var currTileId = tilemap.TileAt((x, y)).Id;

                if (TargetTileId != currTileId)
                    continue;

                var value = new Point(x + Offset.x, y + Offset.y).ToNoise(Noise, Scale, Seed);
                var currBottom = 0f;
                foreach (var (rule, tile) in Elevations)
                {
                    var currTop = rule / 255f;

                    if (value.IsBetween((currBottom, currTop), (true, true)))
                    {
                        tilemap.SetTile((x, y), tile);
                        break;
                    }

                    currBottom = currTop;
                }
            }
    }

    public MapGenerator Duplicate()
    {
        return new(ToBytes());
    }

    public static implicit operator byte[](MapGenerator mapGenerator)
    {
        return mapGenerator.ToBytes();
    }
    public static implicit operator MapGenerator(byte[] bytes)
    {
        return new(bytes);
    }

#region Backend
    internal static byte[] Compress(byte[] data)
    {
        using var compressedStream = new MemoryStream();
        using (var gzipStream = new GZipStream(compressedStream, CompressionMode.Compress))
        {
            gzipStream.Write(data, 0, data.Length);
        }

        return compressedStream.ToArray();
    }
    internal static byte[] Decompress(byte[] compressedData)
    {
        using var compressedStream = new MemoryStream(compressedData);
        using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
        using var resultStream = new MemoryStream();
        gzipStream.CopyTo(resultStream);
        return resultStream.ToArray();
    }
    internal static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}