namespace Pure.Engine.Pathfinding;

using System.IO.Compression;
using System.Runtime.InteropServices;

public class PathMap
{
    public (int width, int height) Size
    {
        get => pathfind.Size;
        set => pathfind.Size = value;
    }

    public PathMap((int width, int height) size)
    {
        Size = size;
    }
    public PathMap(byte[] bytes)
    {
        var b = Decompress(bytes);
        var offset = 0;

        Size = (BitConverter.ToInt32(Get<int>()), BitConverter.ToInt32(Get<int>()));
        var nodeCount = BitConverter.ToInt32(Get<int>());
        for (var i = 0; i < nodeCount; i++)
        {
            var pos = (BitConverter.ToInt32(Get<int>()), BitConverter.ToInt32(Get<int>()));
            var penalty = BitConverter.ToSingle(Get<float>());
            SetObstacle(penalty, pos);
        }

        byte[] Get<T>()
        {
            return GetBytesFrom(b, Marshal.SizeOf(typeof(T)), ref offset);
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
        if (tileIds == null)
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

    public (float x, float y)[] FindPath(
        (float x, float y) start,
        (float x, float y) goal,
        int slopeFactor = 1)
    {
        if (Size.width < 1 || Size.height < 1)
            return Array.Empty<(float x, float y)>();

        return pathfind.FindPath(start, goal, false, out _, slopeFactor, uint.MaxValue);
    }
    public (float x, float y, uint color)[] FindPath(
        (float x, float y) start,
        (float x, float y) goal,
        uint color,
        int slopeFactor = 1)
    {
        if (Size.width < 1 || Size.height < 1)
            return Array.Empty<(float x, float y, uint color)>();

        pathfind.FindPath(start, goal, true, out var withColors, slopeFactor, color);
        return withColors;
    }

    public static implicit operator string(PathMap pathMap)
    {
        return pathMap.ToBase64();
    }
    public static implicit operator PathMap(string base64)
    {
        return new(base64);
    }
    public static implicit operator byte[](PathMap pathMap)
    {
        return pathMap.ToBytes();
    }
    public static implicit operator PathMap(byte[] base64)
    {
        return new(base64);
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

    private static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using var stream = new DeflateStream(output, CompressionLevel.Optimal);
        stream.Write(data, 0, data.Length);

        return output.ToArray();
    }
    private static byte[] Decompress(byte[] data)
    {
        var input = new MemoryStream(data);
        var output = new MemoryStream();
        using var stream = new DeflateStream(input, CompressionMode.Decompress);
        stream.CopyTo(output);
        return output.ToArray();
    }
    private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}