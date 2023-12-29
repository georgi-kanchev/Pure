using System.IO.Compression;

namespace Pure.Engine.Pathfinding;

public class Grid
{
    public (int width, int height) Size
    {
        get => pathfind.Size;
        set => pathfind.Size = value;
    }

    public int CountObstacles
    {
        get => pathfind.CountObstacles;
    }
    public int CountSolids
    {
        get => pathfind.CountSolids;
    }
    public int CountEmpty
    {
        get => pathfind.CountEmpty;
    }

    public Grid((int width, int height) size)
    {
        Size = size;
    }
    public Grid(byte[] bytes)
    {
    }

    public byte[] ToBytes()
    {
        var result = new List<byte>();
        var (w, h) = Size;

        result.AddRange(BitConverter.GetBytes(w));
        result.AddRange(BitConverter.GetBytes(h));

        for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var node = pathfind.grid[x, y];
                if (node.penalty == 0) // skip saving non-solid & non-obstacle (fully empty) cells
                    continue;

                //xs.Add(x);
                //ys.Add(y);
                //weights.Add(node.weight);
                //solids.Add(node.isSolid);
            }
        //
        //var bC = BitConverter.GetBytes(xs.Count);
        //var bW = BitConverter.GetBytes(w);
        //var bH = BitConverter.GetBytes(h);
        //var bXs = ToBytes(xs.ToArray());
        //var bYs = ToBytes(ys.ToArray());
        //var bWs = ToBytes(weights.ToArray());
        //var bSs = BoolsToBytes(solids);
        //
        //var result = new byte[bC.Length + bW.Length + bH.Length +
        //	bXs.Length + bYs.Length + bWs.Length + bSs.Length];
        //
        //Array.Copy(bC, 0, result, 0,
        //	bC.Length);
        //Array.Copy(bW, 0, result,
        //	bC.Length, bW.Length);
        //Array.Copy(bH, 0, result,
        //	bC.Length + bW.Length, bH.Length);
        //Array.Copy(bXs, 0, result,
        //	bC.Length + bW.Length + bH.Length, bXs.Length);
        //Array.Copy(bYs, 0, result,
        //	bC.Length + bW.Length + bH.Length + bXs.Length, bYs.Length);
        //Array.Copy(bWs, 0, result,
        //	bC.Length + bW.Length + bH.Length + bXs.Length + bYs.Length, bWs.Length);
        //Array.Copy(bSs, 0, result,
        //	bC.Length + bW.Length + bH.Length + bXs.Length + bYs.Length + bWs.Length, bSs.Length);
        //
        //File.WriteAllBytes(path, Compress(result));

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

    public (float x, float y)[] FindPath((float x, float y) start, (float x, float y) goal)
    {
        if (Size.width < 1 || Size.height < 1)
            return Array.Empty<(float x, float y)>();

        return pathfind.FindPath(start, goal, false, out _);
    }
    public (float x, float y, uint color)[] FindPath((float x, float y) start, (float x, float y) goal, uint color)
    {
        if (Size.width < 1 || Size.height < 1)
            return Array.Empty<(float x, float y, uint color)>();

        pathfind.FindPath(start, goal, true, out var withColors, color);
        return withColors;
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

    internal static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using var stream = new DeflateStream(output, CompressionLevel.Optimal);
        stream.Write(data, 0, data.Length);

        return output.ToArray();
    }
    internal static byte[] Decompress(byte[] data)
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