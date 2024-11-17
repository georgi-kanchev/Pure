namespace Pure.Engine.Collision;

public class SolidMap
{
    public List<(int x, int y)> IgnoredCells { get; } = [];

    public (int x, int y) Offset { get; set; }

    public int TileCount
    {
        get => cellRects.Count;
    }
    public int Count
    {
        get => arrayCache?.Length ?? 0;
    }

    public (float x, float y, float width, float height, uint color)[] ToBundle()
    {
        var solids = ToArray();
        var result = new (float x, float y, float width, float height, uint color)[solids.Length];
        for (var i = 0; i < solids.Length; i++)
            result[i] = solids[i];
        return result;
    }
    public Solid[] ToArray()
    {
        return arrayCache?.ToArray() ?? [];
    }

    public void AddSolids(int tileId, params Solid[]? solids)
    {
        if (solids == null || solids.Length == 0)
            return;

        if (cellRects.ContainsKey(tileId) == false)
            cellRects[tileId] = [];

        cellRects[tileId].AddRange(solids);
    }
    public void RemoveSolids(int tileId, params Solid[]? solids)
    {
        if (solids == null || solids.Length == 0 || cellRects.ContainsKey(tileId) == false)
            return;

        var rects = cellRects[tileId];
        foreach (var solid in solids)
            rects.Remove(solid);
    }
    public Solid[] SolidsAt((int x, int y) cell)
    {
        if (tileIndices.ContainsKey(cell) == false || IgnoredCells.Contains(cell))
            return [];

        var id = tileIndices[cell];
        var rects = cellRects[id];
        var result = new List<Solid>();

        foreach (var r in rects)
            result.Add(r);

        return result.ToArray();
    }
    public Solid[] SolidsIn(int tileId)
    {
        return cellRects.ContainsKey(tileId) == false ?
            [] :
            cellRects[tileId].ToArray();
    }

    public void ClearSolids()
    {
        cellRects.Clear();
    }
    public void ClearSolids(int tileId)
    {
        if (cellRects.ContainsKey(tileId))
            cellRects.Remove(tileId);
    }

    public void AddIgnoredCellsIn(params Solid[]? cellRegions)
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

                    if (IgnoredCells.Contains((x, y)) == false)
                        IgnoredCells.Add((x, y));

                    i++;
                }
        }
    }
    public void RemoveIgnoredCellsIn(params Solid[]? cellRegions)
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

                    IgnoredCells.Remove((x, y));
                    i++;
                }
        }
    }
    public (int x, int y)[] IgnoredCellsIn(params Solid[]? cellRegions)
    {
        if (cellRegions == null || cellRegions.Length == 0)
            return [];

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

                    var index = IgnoredCells.IndexOf((x, y));
                    if (index >= 0)
                        result.Add(IgnoredCells[index]);

                    i++;
                }
        }

        return result.ToArray();
    }

    public void Update(ushort[,]? tileIds, bool mergeAdjacentSolids = true)
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

                var cell = (x + Offset.x, y + Offset.y);
                tileIndices[cell] = tile;
            }

        // caching the resulting solid array to not create it on each get
        var result = new List<Solid>();
        foreach (var kvp in tileIndices)
        {
            if (IgnoredCells.Contains(kvp.Key))
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

        arrayCache = result.ToArray();

        if (mergeAdjacentSolids == false)
            return;

        var solidPack = new SolidPack(arrayCache);
        solidPack.Merge();
        arrayCache = solidPack;
    }

    public bool IsOverlapping(LinePack linePack)
    {
        for (var i = 0; i < linePack.Count; i++)
            if (IsOverlapping(linePack[i]))
                return true;

        return false;
    }
    public bool IsOverlapping(SolidMap solidMap)
    {
        return IsOverlapping(solidMap.ToArray());
    }
    public bool IsOverlapping(SolidPack solidPack)
    {
        for (var i = 0; i < solidPack.Count; i++)
            if (IsOverlapping(solidPack[i]))
                return true;

        return false;
    }
    public bool IsOverlapping(Line line)
    {
        return line.IsOverlapping(GetNeighborRects(line).ToArray());
    }
    public bool IsOverlapping(Solid solid)
    {
        var neighborRects = GetNeighborRects(solid);

        for (var i = 0; i < neighborRects.Count; i++)
            if (neighborRects[i].IsOverlapping(solid))
                return true;

        return false;
    }
    public bool IsOverlapping((float x, float y) point)
    {
        var neighborRects = GetNeighborRects(new Solid(point, (1, 1)));
        for (var i = 0; i < neighborRects.Count; i++)
            if (neighborRects[i].IsOverlapping(point))
                return true;

        return false;
    }
    public bool IsOverlapping((float x, float y, uint color) point)
    {
        return IsOverlapping((point.x, point.y));
    }

    public bool IsContaining(LinePack linePack)
    {
        for (var i = 0; i < linePack.Count; i++)
            if (IsContaining(linePack[i]) == false)
                return false;

        return true;
    }
    public bool IsContaining(SolidMap solidMap)
    {
        return IsContaining(solidMap.ToArray());
    }
    public bool IsContaining(SolidPack solidPack)
    {
        for (var i = 0; i < solidPack.Count; i++)
            if (IsContaining(solidPack[i]) == false)
                return false;

        return true;
    }
    public bool IsContaining(Solid solid)
    {
        var (x, y, w, h, _) = solid.ToBundle();
        return IsContaining((x, y)) && IsContaining((x + w, y + h));
    }
    public bool IsContaining(Line line)
    {
        return IsContaining(line.A) && IsContaining(line.B) && line.CrossPoints(this).Length == 0;
    }
    public bool IsContaining((float x, float y) point)
    {
        return IsOverlapping(point);
    }
    public bool IsContaining((float x, float y, uint color) point)
    {
        return IsOverlapping((point.x, point.y));
    }

    public static implicit operator Solid[](SolidMap solidMap)
    {
        return solidMap.ToArray();
    }
    public static implicit operator (float x, float y, float width, float height, uint color)[](SolidMap solidMap)
    {
        return solidMap.ToBundle();
    }

#region Backend
    private readonly Dictionary<(int x, int y), int> tileIndices = new();
    private readonly Dictionary<int, List<Solid>> cellRects = new();
    private Solid[]? arrayCache;

    private List<Solid> GetNeighborRects(Solid rect)
    {
        var result = new List<Solid>();
        var (x, y) = rect.Position;
        var (chW, chH) = GetChunkSizeForRect(rect);

        for (var j = -chW; j < chW; j++)
            for (var i = -chH; i < chH; i++)
            {
                var cell = ((int)x + i, (int)y + j);

                if (tileIndices.ContainsKey(cell) == false || IgnoredCells.Contains(cell))
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
    internal List<Solid> GetNeighborRects(Line line)
    {
        var (x0, y0) = ((int)line.A.x, (int)line.A.y);
        var (x1, y1) = ((int)line.B.x, (int)line.B.y);
        var dx = (int)MathF.Abs(x1 - x0);
        var dy = (int)-MathF.Abs(y1 - y0);
        var sx = x0 < x1 ? 1 : -1;
        var sy = y0 < y1 ? 1 : -1;
        var err = dx + dy;
        var rects = new List<Solid>();

        for (var i = 0; i < 1000; i++)
        {
            var ix = x0;
            var iy = y0;

            rects.AddRange(GetNeighborRects((Solid)new(ix, iy, 1, 1)));

            if (x0 == x1 && y0 == y1)
                break;

            var e2 = 2 * err;

            if (e2 > dy)
            {
                err += dy;
                x0 += sx;
            }

            if (e2 >= dx)
                continue;

            err += dx;
            y0 += sy;
        }

        return rects;
    }

    private static (int, int) GetChunkSizeForRect(Solid globalRect)
    {
        var (w, h) = globalRect.Size;
        var resultW = Math.Max((int)MathF.Ceiling(w * 2f), 1);
        var resultH = Math.Max((int)MathF.Ceiling(h * 2f), 1);
        return (resultW, resultH);
    }
#endregion
}