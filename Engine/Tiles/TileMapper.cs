using System.Text.RegularExpressions;

namespace Pure.Engine.Tiles;

public static class TileMapper
{
    public static int GetSeed(this TileMap tileMap)
    {
        return seeds.GetValueOrDefault(tileMap.GetHashCode()).z;
    }
    public static (int x, int y) GetSeedOffset(this TileMap tileMap)
    {
        var (x, y, z) = seeds.GetValueOrDefault(tileMap.GetHashCode());
        return (x, y);
    }
    public static Area? GetMask(this TileMap tileMap)
    {
        return masks.GetValueOrDefault(tileMap.GetHashCode());
    }

    public static void ApplySeed(this TileMap tileMap, int seed)
    {
        var (x, y, z) = seeds[tileMap.GetHashCode()];
        seeds[tileMap.GetHashCode()] = (x, y, seed);
    }
    public static void ApplySeedOffset(this TileMap tileMap, (int x, int y) offset)
    {
        var (x, y, z) = seeds[tileMap.GetHashCode()];
        seeds[tileMap.GetHashCode()] = (offset.x, offset.y, z);
    }
    public static void ApplyMask(this TileMap tileMap, Area? mask)
    {
        masks[tileMap.GetHashCode()] = mask;
    }

    public static void Fill(this TileMap tileMap, params Tile[]? tiles)
    {
        if (tiles == null || tiles.Length == 0)
        {
            tileMap.Flush();
            return;
        }

        var mask = GetMask(tileMap);
        for (var y = 0; y < tileMap.Size.height; y++)
            for (var x = 0; x < tileMap.Size.width; x++)
            {
                var tile = tiles.Length == 1 ? tiles[0] : ChooseOne(tiles, ToSeed(tileMap, (x, y, 0)));
                tileMap.SetTile((x, y), tile, mask);
            }
    }
    public static void Flood(this TileMap tileMap, (int x, int y) cell, bool exactTile, params Tile[]? tiles)
    {
        if (tiles == null || tiles.Length == 0)
            return;

        var mask = GetMask(tileMap);
        var stack = new Stack<(int x, int y)>();
        var initialTile = tileMap.TileAt(cell);
        stack.Push(cell);

        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();
            var curTile = tileMap.TileAt((x, y));
            var tile = tiles.Length == 1 ? tiles[0] : ChooseOne(tiles, ToSeed(tileMap, (x, y, 0)));
            var exact = curTile == tile || curTile != initialTile;
            var onlyId = curTile.Id == tile.Id || curTile.Id != initialTile.Id;

            if ((exactTile && exact) ||
                (exactTile == false && onlyId))
                continue;

            tileMap.SetTile((x, y), tile, mask);

            if (tileMap.IsContaining((x - 1, y)))
                stack.Push((x - 1, y));
            if (tileMap.IsContaining((x + 1, y)))
                stack.Push((x + 1, y));
            if (tileMap.IsContaining((x, y - 1)))
                stack.Push((x, y - 1));
            if (tileMap.IsContaining((x, y + 1)))
                stack.Push((x, y + 1));
        }
    }
    public static void Replace(this TileMap tileMap, Area area, Tile targetTile, params Tile[] tiles)
    {
        if (tiles.Length == 0)
            return;

        var mask = GetMask(tileMap);
        for (var i = 0; i < Math.Abs(area.Width * area.Height); i++)
        {
            var x = area.X + i % Math.Abs(area.Width) * (area.Width < 0 ? -1 : 1);
            var y = area.Y + i / Math.Abs(area.Width) * (area.Height < 0 ? -1 : 1);

            if (tileMap.TileAt((x, y)).Id != targetTile.Id)
                continue;

            var tile = tiles.Length == 1 ? tiles[0] : ChooseOne(tiles, ToSeed(tileMap, (x, y, 0)));
            tileMap.SetTile((x, y), tile, mask);
        }
    }
    public static void Replace(this TileMap tileMap, Tile targetTile, params Tile[] tiles)
    {
        tileMap.Replace((0, 0, tileMap.Size.width, tileMap.Size.height), targetTile, tiles);
    }

    public static void SetTiles(this TileMap tileMap, (int x, int y) cell, Tile[,]? tiles)
    {
        if (tiles == null || tiles.Length == 0)
            return;

        var mask = GetMask(tileMap);
        for (var i = 0; i < tiles.GetLength(1); i++)
            for (var j = 0; j < tiles.GetLength(0); j++)
                tileMap.SetTile((cell.x + j, cell.y + i), tiles[j, i], mask);
    }
    public static void SetArea(this TileMap tileMap, Area area, params Tile[]? tiles)
    {
        if (tiles == null || tiles.Length == 0)
            return;

        var xStep = area.Width < 0 ? -1 : 1;
        var yStep = area.Height < 0 ? -1 : 1;
        var i = 0;
        var mask = GetMask(tileMap);
        for (var y = area.Y; y != area.Y + area.Height; y += yStep)
            for (var x = area.X; x != area.X + area.Width; x += xStep)
            {
                if (i > Math.Abs(area.Width * area.Height))
                    return;

                var tile = tiles.Length == 1 ? tiles[0] : ChooseOne(tiles, ToSeed(tileMap, (x, y, 0)));
                tileMap.SetTile((x, y), tile, mask);
                i++;
            }
    }
    public static void SetEllipse(this TileMap tileMap, (int x, int y) cell, (int width, int height) radius, bool fill, params Tile[]? tiles)
    {
        if (tiles == null || tiles.Length == 0)
            return;

        var rxSq = radius.width * radius.width;
        var rySq = radius.height * radius.height;
        var x = 0;
        var y = radius.height;
        var px = 0;
        var py = rxSq * 2 * y;
        var mask = GetMask(tileMap);

        // Region 1
        var p = (int)(rySq - rxSq * radius.height + 0.25f * rxSq);
        while (px < py)
        {
            Set();

            x++;
            px += rySq * 2;

            if (p < 0)
                p += rySq + px;
            else
            {
                y--;
                py -= rxSq * 2;
                p += rySq + px - py;
            }
        }

        // Region 2
        p = (int)(rySq * (x + 0.5f) * (x + 0.5f) + rxSq * (y - 1) * (y - 1) - rxSq * rySq);
        while (y >= 0)
        {
            Set();

            y--;
            py -= rxSq * 2;

            if (p > 0)
                p += rxSq - py;
            else
            {
                x++;
                px += rySq * 2;
                p += rxSq - py + px;
            }
        }

        void Set()
        {
            var c = cell;
            var o = tiles.Length == 1;
            if (fill == false)
            {
                tileMap.SetTile((c.x + x, c.y - y), o ? tiles[0] : ChooseOne(tiles, ToSeed(tileMap, (c.x + x, c.y - y, 0))), mask);
                tileMap.SetTile((c.x - x, c.y - y), o ? tiles[0] : ChooseOne(tiles, ToSeed(tileMap, (c.x - x, c.y - y, 0))), mask);
                tileMap.SetTile((c.x - x, c.y + y), o ? tiles[0] : ChooseOne(tiles, ToSeed(tileMap, (c.x - x, c.y + y, 0))), mask);
                tileMap.SetTile((c.x + x, c.y + y), o ? tiles[0] : ChooseOne(tiles, ToSeed(tileMap, (c.x + x, c.y + y, 0))), mask);
                return;
            }

            for (var i = c.x - x; i <= c.x + x; i++)
            {
                tileMap.SetTile((i, c.y - y), o ? tiles[0] : ChooseOne(tiles, ToSeed(tileMap, (i, c.y - y, 0))), mask);
                tileMap.SetTile((i, c.y + y), o ? tiles[0] : ChooseOne(tiles, ToSeed(tileMap, (i, c.y + y, 0))), mask);
            }
        }
    }
    public static void SetLine(this TileMap tileMap, (int x, int y) cellA, (int x, int y) cellB, params Tile[]? tiles)
    {
        if (tiles == null || tiles.Length == 0)
            return;

        var (x0, y0) = cellA;
        var (x1, y1) = cellB;
        var dx = Math.Abs(x1 - x0);
        var dy = Math.Abs(y1 - y0);
        var sx = x0 < x1 ? 1 : -1;
        var sy = y0 < y1 ? 1 : -1;
        var err = dx - dy;
        var mask = GetMask(tileMap);

        while (true)
        {
            var tile = tiles.Length == 1 ? tiles[0] : ChooseOne(tiles, ToSeed(tileMap, (x0, y0, 0)));
            tileMap.SetTile((x0, y0), tile, mask);

            if (x0 == x1 && y0 == y1)
                break;

            var e2 = 2 * err;

            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
    public static void SetBox(this TileMap tileMap, Area area, Tile fill, Tile corner, Tile edge)
    {
        var (x, y, w, h, _) = area.ToBundle();

        if (w <= 0 || h <= 0)
            return;

        if (w == 1 || h == 1)
        {
            tileMap.SetArea(area, fill);
            return;
        }

        var mask = GetMask(tileMap);

        tileMap.SetTile((x, y), new(corner.Id, corner.Tint), mask);
        tileMap.SetArea((x + 1, y, w - 2, 1), new Tile(edge.Id, edge.Tint));
        tileMap.SetTile((x + w - 1, y), new(corner.Id, corner.Tint, Pose.Right), mask);

        if (h != 2)
        {
            tileMap.SetArea((x, y + 1, 1, h - 2), new Tile(edge.Id, edge.Tint, Pose.Left));
            tileMap.SetArea((x + 1, y + 1, w - 2, h - 2), fill);
            tileMap.SetArea((x + w - 1, y + 1, 1, h - 2), new Tile(edge.Id, edge.Tint, Pose.Right));
        }

        tileMap.SetTile((x, y + h - 1), new(corner.Id, corner.Tint, Pose.Left), mask);
        tileMap.SetArea((x + 1, y + h - 1, w - 2, 1), new Tile(edge.Id, edge.Tint, Pose.Down));
        tileMap.SetTile((x + w - 1, y + h - 1), new(corner.Id, corner.Tint, Pose.Down), mask);
    }
    public static void SetPatch(this TileMap tileMap, Area area, Tile[,]? tiles3X3)
    {
        if (tiles3X3 == null || tiles3X3.GetLength(0) != 3 || tiles3X3.GetLength(1) != 3)
            return;

        var (x, y, w, h, _) = area.ToBundle();
        var mask = GetMask(tileMap);

        if (w <= 0 || h <= 0)
            return;

        if (w == 1 && h == 1)
        {
            tileMap.SetTile((x, y), tiles3X3[1, 1], mask);
            return;
        }

        if (h == 1)
        {
            tileMap.SetTile((x, y), tiles3X3[1, 0], mask);

            if (w > 2)
                tileMap.SetArea((x + 1, y, w - 2, 1), tiles3X3[1, 1]);

            tileMap.SetTile((x + w - 1, y), tiles3X3[1, 2], mask);
            return;
        }

        if (w == 1)
        {
            tileMap.SetTile((x, y), tiles3X3[0, 1], mask);

            if (h > 2)
                tileMap.SetArea((x, y + 1, 1, h - 2), tiles3X3[1, 1]);

            tileMap.SetTile((x, y + h - 1), tiles3X3[2, 1], mask);
            return;
        }

        if (w == 2)
        {
            tileMap.SetArea((x, y, 1, h), tiles3X3[1, 0]);
            tileMap.SetArea((x + 1, y, 1, h), tiles3X3[1, 2]);
            return;
        }

        if (h == 2)
        {
            tileMap.SetArea((x, y, w, 1), tiles3X3[0, 1]);
            tileMap.SetArea((x, y + 1, w, 1), tiles3X3[2, 1]);
            return;
        }

        tileMap.SetTile((x, y), tiles3X3[0, 0], mask);
        tileMap.SetArea((x + 1, y, w - 2, 1), tiles3X3[0, 1]);
        tileMap.SetTile((x + w - 1, y), tiles3X3[0, 2], mask);

        tileMap.SetArea((x, y + 1, 1, h - 2), tiles3X3[1, 0]);
        tileMap.SetArea((x + 1, y + 1, w - 2, h - 2), tiles3X3[1, 1]);
        tileMap.SetArea((x + w - 1, y + 1, 1, h - 2), tiles3X3[1, 2]);

        tileMap.SetTile((x, y + h - 1), tiles3X3[2, 0], mask);
        tileMap.SetArea((x + 1, y + h - 1, w - 2, 1), tiles3X3[2, 1]);
        tileMap.SetTile((x + w - 1, y + h - 1), tiles3X3[2, 2], mask);
    }
    public static void SetBlob(this TileMap tileMap, (int x, int y) cell, int radius, int warp = 2, int sides = 20, params Tile[]? tiles)
    {
        var boundaryPoints = new List<(int x, int y)>();
        const float CIRCLE = MathF.PI * 2f;

        for (var angle = 0f; angle < CIRCLE; angle += CIRCLE / sides)
        {
            var r = radius + Random((-warp, warp), ToSeed(tileMap, (cell.x, cell.y, (int)(angle * 314f))));
            var x = cell.x + (int)(r * MathF.Cos(angle));
            var y = cell.y + (int)(r * MathF.Sin(angle));

            boundaryPoints.Add((x, y));
        }

        for (var i = 0; i < boundaryPoints.Count; i++)
        {
            var (x1, y1) = boundaryPoints[i];
            var (x2, y2) = boundaryPoints[(i + 1) % boundaryPoints.Count];
            SetLine(tileMap, (x1, y1), (x2, y2), tiles);
        }

        Flood(tileMap, (cell.x, cell.y), false, tiles);
    }

    public static void SetBar(this TileMap tileMap, (int x, int y) cell, Tile edge1, Tile fill, Tile edge2, int size = 5, bool vertical = false)
    {
        var (x, y) = cell;
        var off = size == 1 ? 0 : 1;
        var mask = GetMask(tileMap);

        if (vertical)
        {
            if (size > 1)
            {
                tileMap.SetTile(cell, edge1, mask);
                tileMap.SetTile((x, y + size - 1), edge2, mask);
            }

            if (size != 2)
                tileMap.SetArea((x, y + off, 1, size - 2), fill);

            return;
        }

        if (size > 1)
        {
            tileMap.SetTile(cell, edge1, mask);
            tileMap.SetTile((x + size - 1, y), edge2, mask);
        }

        if (size != 2)
            tileMap.SetArea((x + off, y, size - 2, 1), fill);
    }
    public static void SetText(this TileMap tileMap, (int x, int y) cell, string? text, uint tint = uint.MaxValue, char tintBrush = '#')
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var colors = GetColors(text, tintBrush);
        var (x, y) = cell;
        var mask = GetMask(tileMap);

        for (var j = 0; j < text.Length; j++)
        {
            if (text[j] == '\r' || text[j] == '\b')
                continue;

            if (text[j] == '\t')
            {
                x += 4;
                continue;
            }

            if (IsTag(j))
            {
                var (color, tag, i) = colors[0];

                text = text.Remove(i, tag.Length);
                tint = color;
                colors.RemoveAt(0);
                for (var k = 0; k < colors.Count; k++)
                    colors[k] = (colors[k].color, colors[k].tag, colors[k].index - tag.Length);

                if (IsTag(j)) // tag after tag? backtrack one index to handle it above yet again
                {
                    j--;
                    continue;
                }
            }

            if (text[j] == '\n')
            {
                x = cell.x;
                y++;
                continue;
            }

            var index = tileMap.TileIdFrom(text[j]);
            if (index != default && text[j] != ' ')
                tileMap.SetTile((x, y), new(index, tint), mask);

            x++;
        }

        bool IsTag(int index)
        {
            if (text[index] != tintBrush || colors.Count <= 0)
                return false;

            var (_, tag, i) = colors[0];
            var tagEnd = i + tag.Length;
            return tagEnd < text.Length && text[i..tagEnd] == tag;
        }
    }

#region Backend
    private static readonly Dictionary<int, Area?> masks = [];
    private static readonly Dictionary<int, (int x, int y, int z)> seeds = [];

    private static float Random(this (float a, float b) range, float seed = float.NaN)
    {
        var (a, b) = range;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (a == b)
            return a;

        if (a > b)
            (a, b) = (b, a);

        var r = b - a;

        long intSeed = float.IsNaN(seed) ? Guid.NewGuid().GetHashCode() : BitConverter.SingleToInt32Bits(seed);
        intSeed = (1103515245 * intSeed + 12345) % 2147483648;
        var normalized = (intSeed & 0x7FFFFFFF) / (float)2147483648;
        return a + normalized * r;
    }
    private static T? ChooseOne<T>(this IList<T> collection, float seed = float.NaN)
    {
        return collection.Count == 0 ? default : collection[(0, collection.Count - 1).Random(seed)];
    }
    private static int Random(this (int a, int b) range, float seed = float.NaN)
    {
        return (int)Math.Round(Random(((float)range.a, range.b), seed));
    }
    private static int ToSeed(TileMap map, (int a, int b, int c) parameters)
    {
        var (a, b, c) = parameters;
        var (x, y, z) = seeds[map.GetHashCode()];

        return Calculate(z, a + x, b + y, c);

        int Calculate(int number, params int[] more)
        {
            var seed = 2654435769L;
            Seed(number);
            foreach (var p in more)
                seed = Seed(p);

            return (int)seed;

            long Seed(int s)
            {
                seed ^= s;
                seed = (seed ^ (seed >> 16)) * 2246822519L;
                seed = (seed ^ (seed >> 13)) * 3266489917L;
                seed ^= seed >> 16;
                return (int)seed;
            }
        }
    }
    private static List<(uint color, string tag, int index)> GetColors(string input, char brush)
    {
        var colors = new List<(uint color, string tag, int index)>();
        var matches = Regex.Matches(input, $"{brush}([0-9a-fA-F]+){brush}");

        foreach (Match match in matches)
        {
            var colorValue = Convert.ToUInt32(match.Groups[1].Value, 16);
            var startIndex = match.Index;
            colors.Add((colorValue, match.Value, startIndex));
        }

        return colors;
    }
#endregion
}