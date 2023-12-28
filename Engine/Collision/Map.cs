namespace Pure.Engine.Collision;

using System.IO.Compression;
using System.Runtime.InteropServices;

public class Map
{
    public int SolidsCount { get; private set; }
    public int IgnoredCellsCount
    {
        get => ignoredCells.Count;
    }

    public Map()
    {
    }
    public Map(byte[] bytes)
    {
        var b = Decompress(bytes);
        var offset = 0;

        var sectorCount = BitConverter.ToInt32(Get<int>());

        for (var i = 0; i < sectorCount; i++)
        {
            var tile = BitConverter.ToInt32(Get<int>());
            var rectAmount = BitConverter.ToInt32(Get<int>());

            for (var j = 0; j < rectAmount; j++)
            {
                var x = BitConverter.ToSingle(Get<float>());
                var y = BitConverter.ToSingle(Get<float>());
                var w = BitConverter.ToSingle(Get<float>());
                var h = BitConverter.ToSingle(Get<float>());
                var color = BitConverter.ToUInt32(Get<uint>());

                SolidsAdd(tile, new Rectangle((w, h), (x, y), color));
            }
        }

        return;

        byte[] Get<T>()
        {
            return GetBytesFrom(b, Marshal.SizeOf(typeof(T)), ref offset);
        }
    }

    public byte[] ToBytes()
    {
        var result = new List<byte>();
        result.AddRange(BitConverter.GetBytes(cellRects.Count));

        foreach (var kvp in cellRects)
        {
            result.AddRange(BitConverter.GetBytes(kvp.Key));
            result.AddRange(BitConverter.GetBytes(kvp.Value.Count));

            foreach (var r in kvp.Value)
            {
                result.AddRange(BitConverter.GetBytes(r.Position.x));
                result.AddRange(BitConverter.GetBytes(r.Position.y));
                result.AddRange(BitConverter.GetBytes(r.Size.width));
                result.AddRange(BitConverter.GetBytes(r.Size.height));
                result.AddRange(BitConverter.GetBytes(r.Color));
            }
        }

        return Compress(result.ToArray());
    }

    public void SolidsAdd(int tileId, params Rectangle[]? solids)
    {
        if (solids == null || solids.Length == 0)
            return;

        if (cellRects.ContainsKey(tileId) == false)
            cellRects[tileId] = new List<Rectangle>();

        cellRects[tileId].AddRange(solids);
        SolidsCount += solids.Length;
    }
    public void SolidsRemove(int tileId, params Rectangle[]? solids)
    {
        if (solids == null || solids.Length == 0 || cellRects.ContainsKey(tileId) == false)
            return;

        var rects = cellRects[tileId];
        foreach (var solid in solids)
            if (rects.Remove(solid))
                SolidsCount--;
    }
    public Rectangle[] SolidsGet((int x, int y) cell)
    {
        if (tileIndices.ContainsKey(cell) == false || ignoredCells.Contains(cell))
            return Array.Empty<Rectangle>();

        var id = tileIndices[cell];
        var rects = cellRects[id];
        var result = new List<Rectangle>();

        foreach (var r in rects)
            result.Add(r);

        return result.ToArray();
    }
    public Rectangle[] SolidsGet(int tileId)
    {
        return cellRects.ContainsKey(tileId) == false ? Array.Empty<Rectangle>() : cellRects[tileId].ToArray();
    }
    public Rectangle[] SolidsGet()
    {
        var result = new List<Rectangle>();
        foreach (var kvp in tileIndices)
        {
            if (ignoredCells.Contains(kvp.Key))
                continue;

            var rects = cellRects[kvp.Value];
            var (cellX, cellY) = kvp.Key;
            foreach (var rect in rects)
            {
                var rectangle = rect;
                var (x, y) = rectangle.Position;
                rectangle.Position = (cellX + x, cellY + y);
                result.Add(rectangle);
            }
        }

        return result.ToArray();
    }
    public void SolidsClear()
    {
        SolidsCount = 0;
        cellRects.Clear();
    }
    public void SolidsClear(int tileId)
    {
        if (cellRects.ContainsKey(tileId) == false)
            return;

        SolidsCount -= cellRects[tileId].Count;
        cellRects.Remove(tileId);
    }

    public void IgnoredCellsAdd(params (int x, int y)[]? cells)
    {
        if (cells == null || cells.Length == 0)
            return;

        foreach (var cell in cells)
            if (ignoredCells.Contains(cell) == false)
                ignoredCells.Add(cell);
    }
    public void IgnoredCellsAdd(params Rectangle[]? cellRegions)
    {
        if (cellRegions == null || cellRegions.Length == 0)
            return;

        foreach (var region in cellRegions)
        {
            var (rx, ry) = ((int)region.Position.x, (int)region.Position.y);
            var (rw, rh) = ((int)region.Size.width, (int)region.Size.height);
            var xStep = rw < 0 ? -1 : 1;
            var yStep = rh < 0 ? -1 : 1;
            var i = 0;
            for (var x = rx; x != rx + rw; x += xStep)
                for (var y = ry; y != ry + rh; y += yStep)
                {
                    if (i > Math.Abs(rw * rh))
                        return;

                    IgnoredCellsAdd((x, y));
                    i++;
                }
        }
    }
    public void IgnoredCellsRemove(params (int x, int y)[]? cells)
    {
        if (cells == null || cells.Length == 0)
            return;

        foreach (var t in cells)
            ignoredCells.Remove(t);
    }
    public void IgnoredCellsRemove(params Rectangle[]? cellRegions)
    {
        if (cellRegions == null || cellRegions.Length == 0)
            return;

        foreach (var region in cellRegions)
        {
            var (rx, ry) = ((int)region.Position.x, (int)region.Position.y);
            var (rw, rh) = ((int)region.Size.width, (int)region.Size.height);
            var xStep = rw < 0 ? -1 : 1;
            var yStep = rh < 0 ? -1 : 1;
            var i = 0;
            for (var x = rx; x != rx + rw; x += xStep)
                for (var y = ry; y != ry + rh; y += yStep)
                {
                    if (i > Math.Abs(rw * rh))
                        return;

                    IgnoredCellsRemove((x, y));
                    i++;
                }
        }
    }
    public (int x, int y)[] IgnoredCellsGet()
    {
        return ignoredCells.ToArray();
    }
    public (int x, int y)[] IgnoredCellsGet(params Rectangle[]? cellRegions)
    {
        if (cellRegions == null || cellRegions.Length == 0)
            return Array.Empty<(int x, int y)>();

        var result = new List<(int x, int y)>();
        foreach (var region in cellRegions)
        {
            var (rx, ry) = ((int)region.Position.x, (int)region.Position.y);
            var (rw, rh) = ((int)region.Size.width, (int)region.Size.height);
            var xStep = rw < 0 ? -1 : 1;
            var yStep = rh < 0 ? -1 : 1;
            var i = 0;
            for (var x = rx; x != rx + rw; x += xStep)
                for (var y = ry; y != ry + rh; y += yStep)
                {
                    if (i > Math.Abs(rw * rh))
                        return result.ToArray();

                    var index = ignoredCells.IndexOf((x, y));
                    if (index >= 0)
                        result.Add(ignoredCells[index]);

                    i++;
                }
        }

        return result.ToArray();
    }
    public void IgnoredCellsClear()
    {
        ignoredCells.Clear();
    }

    public void Update(int[,]? tileIds)
    {
        if (tileIds == null)
            return;

        tileIndices.Clear();

        for (var y = 0; y < tileIds.GetLength(1); y++)
            for (var x = 0; x < tileIds.GetLength(0); x++)
            {
                var tile = tileIds[x, y];
                if (cellRects.ContainsKey(tile) == false)
                    continue;

                tileIndices[(x, y)] = tile;
            }
    }

    public bool IsOverlapping(Hitbox hitbox)
    {
        for (var i = 0; i < hitbox.SolidsCount; i++)
            if (IsOverlapping(hitbox[i]))
                return true;

        return false;
    }
    public bool IsOverlapping(Line line)
    {
        return line.CrossPoints(this).Length > 0;
    }
    public bool IsOverlapping(Rectangle rectangle)
    {
        var neighborRects = GetNeighborRects(rectangle);

        for (var i = 0; i < neighborRects.Count; i++)
            if (neighborRects[i].IsOverlapping(rectangle))
                return true;

        return false;
    }
    public bool IsOverlapping((float x, float y) point)
    {
        var neighborRects = GetNeighborRects(new(point, (1, 1)));
        for (var i = 0; i < neighborRects.Count; i++)
            if (neighborRects[i].IsOverlapping(point))
                return true;

        return false;
    }

    public (float x, float y, float width, float height, uint color)[] ToBundle()
    {
        return this;
    }

    public static implicit operator Rectangle[](Map map)
    {
        return map.SolidsGet();
    }
    public static implicit operator (float x, float y, float width, float height, uint color)[](Map map)
    {
        var solids = map.SolidsGet();
        var result = new (float x, float y, float width, float height, uint color)[solids.Length];
        for (var i = 0; i < solids.Length; i++)
            result[i] = solids[i];
        return result;
    }

    #region Backend

    // save format in sectors
    // [amount of bytes]		- data
    // --------------------------------
    // [4]						- amount of sectors
    // = = = = = = (sector 1)
    // [4]						- tile
    // [4]						- rect amount
    // [rect amount * 4]		- xs
    // [rect amount * 4]		- ys
    // [rect amount * 4]		- widths
    // [rect amount * 4]		- heights
    // [rect amount * 4]		- colors
    // = = = = = = (sector 2)
    // [4]						- tile
    // [4]						- rect amount
    // [rect amount * 4]		- xs
    // [rect amount * 4]		- ys
    // [rect amount * 4]		- widths
    // [rect amount * 4]		- heights
    // [rect amount * 4]		- colors
    // = = = = = = (sector 3)
    // ...

    // to not repeat rectangles for each tile
    // saving map of tiles [(x, y), tile]
    // and rectangles for each tile [tile, list of rectangles]

    private readonly Dictionary<(int x, int y), int> tileIndices = new();
    private readonly Dictionary<int, List<Rectangle>> cellRects = new();
    private readonly List<(int x, int y)> ignoredCells = new();

    internal List<Rectangle> GetNeighborRects(Rectangle rect)
    {
        var result = new List<Rectangle>();
        var (x, y) = rect.Position;
        var (chW, chH) = GetChunkSizeForRect(rect);

        for (var j = -chW; j < chW; j++)
            for (var i = -chH; i < chH; i++)
            {
                var cell = ((int)x + i, (int)y + j);

                if (tileIndices.ContainsKey(cell) == false || ignoredCells.Contains(cell))
                    continue;

                var id = tileIndices[cell];
                var rects = cellRects[id];
                foreach (var curRect in rects)
                {
                    var rectangle = curRect;
                    var (rx, ry) = rectangle.Position;
                    rectangle.Position = (cell.Item1 + rx, cell.Item2 + ry);
                    result.Add(rectangle);
                }
            }

        return result;
    }
    private static (int, int) GetChunkSizeForRect(Rectangle globalRect)
    {
        var (w, h) = globalRect.Size;
        var resultW = Math.Max((int)MathF.Ceiling(w * 2f), 1);
        var resultH = Math.Max((int)MathF.Ceiling(h * 2f), 1);
        return (resultW, resultH);
    }

    private static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
        {
            stream.Write(data, 0, data.Length);
        }

        return output.ToArray();
    }
    private static byte[] Decompress(byte[] data)
    {
        var input = new MemoryStream(data);
        var output = new MemoryStream();
        using (var stream = new DeflateStream(input, CompressionMode.Decompress))
        {
            stream.CopyTo(output);
        }

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