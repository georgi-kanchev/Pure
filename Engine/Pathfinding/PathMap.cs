using System.IO.Compression;

namespace Pure.Engine.Pathfinding;

public class PathMap
{
    public (int width, int height) Size
    {
        get => pathfind.Size;
    }

    public PathMap((int width, int height) size)
    {
        pathfind.Size = size;
        Init();
    }
    public PathMap(byte[] bytes)
    {
        var b = Decompress(bytes);
        var offset = 0;

        pathfind.Size = (BitConverter.ToInt32(Get()), BitConverter.ToInt32(Get()));
        var nodeCount = BitConverter.ToInt32(Get());
        for (var i = 0; i < nodeCount; i++)
        {
            var pos = (BitConverter.ToInt32(Get()), BitConverter.ToInt32(Get()));
            var penalty = BitConverter.ToSingle(Get());
            SetObstacle(penalty, pos);
        }

        Init();

        byte[] Get()
        {
            return GetBytesFrom(b, 4, ref offset);
        }
    }
    public PathMap(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public byte[] ToBytes()
    {
        var result = new List<byte>();
        var (w, h) = Size;

        result.AddRange(BitConverter.GetBytes(w));
        result.AddRange(BitConverter.GetBytes(h));
        result.AddRange(BitConverter.GetBytes(pathfind.NodeCount));

        var nodes = pathfind.grid;
        foreach (var kvp in nodes)
        {
            var node = kvp.Value;
            result.AddRange(BitConverter.GetBytes(node.position.x));
            result.AddRange(BitConverter.GetBytes(node.position.y));
            result.AddRange(BitConverter.GetBytes(node.penalty));
        }

        return Compress(result.ToArray());
    }

    public void SetObstacle(float penalty, params (int x, int y)[]? cells)
    {
        if (cells == null || cells.Length == 0)
            return;

        foreach (var cell in cells)
            pathfind.SetNode(cell, penalty);
    }
    public void SetObstacle(float penalty, int tileId, int[,]? tileIds)
    {
        if (tileIds == null || tileIds.Length == 0)
            return;

        for (var i = 0; i < tileIds.GetLength(1); i++)
            for (var j = 0; j < tileIds.GetLength(0); j++)
                if (tileIds[j, i] == tileId)
                    pathfind.SetNode((j, i), penalty);
    }

    public bool IsSolid((int x, int y) cell)
    {
        return float.IsNaN(PenaltyAt(cell)) || float.IsInfinity(PenaltyAt(cell));
    }
    public bool IsObstacle((int x, int y) cell)
    {
        return float.IsFinite(PenaltyAt(cell));
    }
    public bool IsEmpty((int x, int y) cell)
    {
        return PenaltyAt(cell) == 0;
    }

    public float PenaltyAt((int x, int y) cell)
    {
        return pathfind.GetNode(cell)?.penalty ?? float.NaN;
    }

    public (float x, float y)[] FindPath((float x, float y) start, (float x, float y) goal, int slopeFactor = 1)
    {
        if (Size.width < 1 || Size.height < 1)
            return [];

        return pathfind.FindPath(start, goal, false, out _, slopeFactor, uint.MaxValue);
    }
    public (float x, float y, uint color)[] FindPath((float x, float y) start, (float x, float y) goal, uint color, int slopeFactor = 1)
    {
        if (Size.width < 1 || Size.height < 1)
            return [];

        pathfind.FindPath(start, goal, true, out var withColors, slopeFactor, color);
        return withColors;
    }

    public PathMap Duplicate()
    {
        return new(ToBytes());
    }

    public static implicit operator byte[](PathMap pathMap)
    {
        return pathMap.ToBytes();
    }
    public static implicit operator PathMap(byte[] bytes)
    {
        return new(bytes);
    }

#region Backend
    // save format
    // [amount of bytes]		- data
    // --------------------------------
    // [4]						- width
    // [4]						- height
    // [4]						- non-default cells count
    // [width * height * 4]		- xs
    // [width * height * 4]		- ys
    // [width * height * 4]		- weights
    // [remaining]				- is walkable bools (1 bit per bool)

    private readonly Astar pathfind = new();

    private void Init()
    {
        SetObstacle(0, 0, new int[Size.width, Size.height]);
    }

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
    private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}