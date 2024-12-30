using System.Text.RegularExpressions;

namespace Pure.Engine.Tiles;

public static class TileMapper
{
    public static (int x, int y, int z) SeedOffset { get; set; }
    public static Area? Mask { get; set; }

    public static void Fill(this TileMap tileMap, params Tile[]? tiles)
    {
        if (tiles == null || tiles.Length == 0)
        {
            tileMap.Flush();
            return;
        }

        for (var y = 0; y < tileMap.Size.height; y++)
            for (var x = 0; x < tileMap.Size.width; x++)
            {
                var tile = tiles.Length == 1 ? tiles[0] : ChooseOne(tiles, ToSeed((x, y)));
                tileMap.SetTile((x, y), tile, Mask);
            }
    }
    public static void Flood(this TileMap tileMap, (int x, int y) cell, bool exactTile, params Tile[]? tiles)
    {
        if (tiles == null || tiles.Length == 0)
            return;

        var stack = new Stack<(int x, int y)>();
        var initialTile = tileMap.TileAt(cell);
        stack.Push(cell);

        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();
            var curTile = tileMap.TileAt((x, y));
            var tile = tiles.Length == 1 ? tiles[0] : ChooseOne(tiles, ToSeed((x, y)));
            var exact = curTile == tile || curTile != initialTile;
            var onlyId = curTile.Id == tile.Id || curTile.Id != initialTile.Id;

            if ((exactTile && exact) ||
                (exactTile == false && onlyId))
                continue;

            tileMap.SetTile((x, y), tile, Mask);

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

        for (var i = 0; i < Math.Abs(area.Width * area.Height); i++)
        {
            var x = area.X + i % Math.Abs(area.Width) * (area.Width < 0 ? -1 : 1);
            var y = area.Y + i / Math.Abs(area.Width) * (area.Height < 0 ? -1 : 1);

            if (tileMap.TileAt((x, y)).Id != targetTile.Id)
                continue;

            var tile = tiles.Length == 1 ? tiles[0] : ChooseOne(tiles, ToSeed((x, y)));
            tileMap.SetTile((x, y), tile, Mask);
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

        for (var i = 0; i < tiles.GetLength(1); i++)
            for (var j = 0; j < tiles.GetLength(0); j++)
                tileMap.SetTile((cell.x + j, cell.y + i), tiles[j, i], Mask);
    }
    public static void SetArea(this TileMap tileMap, Area area, params Tile[]? tiles)
    {
        if (tiles == null || tiles.Length == 0)
            return;

        var xStep = area.Width < 0 ? -1 : 1;
        var yStep = area.Height < 0 ? -1 : 1;
        var i = 0;
        for (var y = area.Y; y != area.Y + area.Height; y += yStep)
            for (var x = area.X; x != area.X + area.Width; x += xStep)
            {
                if (i > Math.Abs(area.Width * area.Height))
                    return;

                var tile = tiles.Length == 1 ? tiles[0] : ChooseOne(tiles, ToSeed((x, y)));
                tileMap.SetTile((x, y), tile, Mask);
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
                tileMap.SetTile((c.x + x, c.y - y), o ? tiles[0] : ChooseOne(tiles, ToSeed((c.x + x, c.y - y))), Mask);
                tileMap.SetTile((c.x - x, c.y - y), o ? tiles[0] : ChooseOne(tiles, ToSeed((c.x - x, c.y - y))), Mask);
                tileMap.SetTile((c.x - x, c.y + y), o ? tiles[0] : ChooseOne(tiles, ToSeed((c.x - x, c.y + y))), Mask);
                tileMap.SetTile((c.x + x, c.y + y), o ? tiles[0] : ChooseOne(tiles, ToSeed((c.x + x, c.y + y))), Mask);
                return;
            }

            for (var i = c.x - x; i <= c.x + x; i++)
            {
                tileMap.SetTile((i, c.y - y), o ? tiles[0] : ChooseOne(tiles, ToSeed((i, c.y - y))), Mask);
                tileMap.SetTile((i, c.y + y), o ? tiles[0] : ChooseOne(tiles, ToSeed((i, c.y + y))), Mask);
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

        while (true)
        {
            var tile = tiles.Length == 1 ? tiles[0] : ChooseOne(tiles, ToSeed((x0, y0)));
            tileMap.SetTile((x0, y0), tile, Mask);

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

        tileMap.SetTile((x, y), new(corner.Id, corner.Tint), Mask);
        tileMap.SetArea((x + 1, y, w - 2, 1), new Tile(edge.Id, edge.Tint));
        tileMap.SetTile((x + w - 1, y), new(corner.Id, corner.Tint, Pose.Right), Mask);

        if (h != 2)
        {
            tileMap.SetArea((x, y + 1, 1, h - 2), new Tile(edge.Id, edge.Tint, Pose.Left));
            tileMap.SetArea((x + 1, y + 1, w - 2, h - 2), fill);
            tileMap.SetArea((x + w - 1, y + 1, 1, h - 2), new Tile(edge.Id, edge.Tint, Pose.Right));
        }

        tileMap.SetTile((x, y + h - 1), new(corner.Id, corner.Tint, Pose.Left), Mask);
        tileMap.SetArea((x + 1, y + h - 1, w - 2, 1), new Tile(edge.Id, edge.Tint, Pose.Down));
        tileMap.SetTile((x + w - 1, y + h - 1), new(corner.Id, corner.Tint, Pose.Down), Mask);
    }
    public static void SetPatch(this TileMap tileMap, Area area, Tile[,]? tiles3X3)
    {
        if (tiles3X3 == null || tiles3X3.GetLength(0) != 3 || tiles3X3.GetLength(1) != 3)
            return;

        var (x, y, w, h, _) = area.ToBundle();

        if (w <= 0 || h <= 0)
            return;

        if (w == 1 || h == 1)
        {
            tileMap.SetTile((x, y), tiles3X3[1, 1]);
            return;
        }

        tileMap.SetTile((x, y), tiles3X3[0, 0], Mask);
        tileMap.SetArea((x + 1, y, w - 2, 1), tiles3X3[0, 1]);
        tileMap.SetTile((x + w - 1, y), tiles3X3[0, 2], Mask);

        if (h != 2)
        {
            tileMap.SetArea((x, y + 1, 1, h - 2), tiles3X3[1, 0]);
            tileMap.SetArea((x + 1, y + 1, w - 2, h - 2), tiles3X3[1, 1]);
            tileMap.SetArea((x + w - 1, y + 1, 1, h - 2), tiles3X3[1, 2]);
        }

        tileMap.SetTile((x, y + h - 1), tiles3X3[2, 0], Mask);
        tileMap.SetArea((x + 1, y + h - 1, w - 2, 1), tiles3X3[2, 1]);
        tileMap.SetTile((x + w - 1, y + h - 1), tiles3X3[2, 2], Mask);
    }
    public static void SetBar(this TileMap tileMap, (int x, int y) cell, Tile edge, Tile fill, int size = 5, bool vertical = false)
    {
        var (x, y) = cell;
        var off = size == 1 ? 0 : 1;

        if (vertical)
        {
            if (size > 1)
            {
                tileMap.SetTile(cell, new(edge.Id, edge.Tint, Pose.Right), Mask);
                tileMap.SetTile((x, y + size - 1), new(edge.Id, edge.Tint, Pose.Left), Mask);
            }

            if (size != 2)
                tileMap.SetArea((x, y + off, 1, size - 2), new Tile(fill.Id, fill.Tint, Pose.Right));

            return;
        }

        if (size > 1)
        {
            tileMap.SetTile(cell, new(edge.Id, edge.Tint), Mask);
            tileMap.SetTile((x + size - 1, y), new(edge.Id, edge.Tint, Pose.Down), Mask);
        }

        if (size != 2)
            tileMap.SetArea((x + off, y, size - 2, 1), new Tile(fill.Id, fill.Tint));
    }
    public static void SetText(this TileMap tileMap, (int x, int y) cell, string? text, uint tint = uint.MaxValue, char tintBrush = '#')
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var colors = GetColors(text, tintBrush);
        var (x, y) = cell;

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
                tileMap.SetTile((x, y), new(index, tint), Mask);

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
    private static float Limit(float number, float rangeA, float rangeB, bool isOverflowing = false)
    {
        if (rangeA > rangeB)
            (rangeA, rangeB) = (rangeB, rangeA);

        if (isOverflowing)
        {
            var d = rangeB - rangeA;
            return ((number - rangeA) % d + d) % d + rangeA;
        }

        if (number < rangeA)
            return rangeA;
        if (number > rangeB)
            return rangeB;
        return number;
    }
    private static int Limit(int number, int rangeA, int rangeB, bool isOverflowing = false)
    {
        return (int)Limit((float)number, rangeA, rangeB, isOverflowing);
    }
    private static float Random(float rangeA, float rangeB, float precision = 0, float seed = float.NaN)
    {
        if (rangeA > rangeB)
            (rangeA, rangeB) = (rangeB, rangeA);

        precision = (int)Limit(precision, 0, 5);
        precision = MathF.Pow(10, precision);

        rangeA *= precision;
        rangeB *= precision;

        var s = float.IsNaN(seed) ? Guid.NewGuid().GetHashCode() : (int)seed;
        var random = new Random(s);
        var randInt = random.Next((int)rangeA, Limit((int)rangeB, (int)rangeA, (int)rangeB) + 1);

        return randInt / precision;
    }
    private static T ChooseOne<T>(IList<T> collection, float seed)
    {
        return collection[(int)Random(0, collection.Count - 1, 0, seed)];
    }
    private static int ToSeed((int a, int b) parameters)
    {
        var (a, b) = parameters;
        var (x, y, z) = SeedOffset;

        return ToSeed(z, a + x, b + y);
    }
    private static int ToSeed(int number, params int[] parameters)
    {
        var seed = 2654435769L;
        Seed(number);
        foreach (var p in parameters)
            seed = Seed(p);

        return (int)seed;

        long Seed(int a)
        {
            seed ^= a;
            seed = (seed ^ (seed >> 16)) * 2246822519L;
            seed = (seed ^ (seed >> 13)) * 3266489917L;
            seed ^= seed >> 16;
            return (int)seed;
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