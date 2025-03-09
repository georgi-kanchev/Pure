using static Pure.Engine.Tiles.Tile;
using System.Text.RegularExpressions;

namespace Pure.Engine.Tiles;

public static class TileMapper
{
    public static int GetSeed(this TileMap tileMap)
    {
        return seeds.GetValueOrDefault(tileMap.GetHashCode()).z;
    }
    public static VecI GetSeedOffset(this TileMap tileMap)
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
        var (x, y, z) = seeds.GetValueOrDefault(tileMap.GetHashCode());
        seeds[tileMap.GetHashCode()] = (x, y, seed);
    }
    public static void ApplySeedOffset(this TileMap tileMap, VecI offset)
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
    public static void Flood(this TileMap tileMap, VecI cell, bool exactTile, params Tile[]? tiles)
    {
        if (tiles == null || tiles.Length == 0)
            return;

        var mask = GetMask(tileMap);
        var stack = new Stack<VecI>();
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

    public static void SetTiles(this TileMap tileMap, VecI cell, Tile[,]? tiles)
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
    public static void SetEllipse(this TileMap tileMap, VecI cell, SizeI radius, bool fill, params Tile[]? tiles)
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
    public static void SetCircle(this TileMap tileMap, VecI cell, int radius, bool fill, params Tile[]? tiles)
    {
        SetEllipse(tileMap, cell, (radius, radius), fill, tiles);
    }
    public static void SetLine(this TileMap tileMap, VecI cellA, VecI cellB, params Tile[]? tiles)
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
    public static void SetBlob(this TileMap tileMap, VecI cell, int radius, int warp = 2, int sides = 20, params Tile[]? tiles)
    {
        var boundaryPoints = new List<VecI>();
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

    public static void SetBar(this TileMap tileMap, VecI cell, Tile edge1, Tile fill, Tile edge2, int size = 5, bool vertical = false)
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
    public static void SetText(this TileMap tileMap, VecI cell, string? text, uint tint = uint.MaxValue, char tintBrush = '#')
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        TryInitText(tileMap);

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
            if (index != default)
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

    public static void ConfigureText(this TileMap tileMap, ushort lowercase = LOWERCASE_A, ushort uppercase = UPPERCASE_A, ushort numbers = NUMBER_0)
    {
        ConfigureText(tileMap, lowercase, "abcdefghijklmnopqrstuvwxyz");
        ConfigureText(tileMap, uppercase, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        ConfigureText(tileMap, numbers, "0123456789");
    }
    public static void ConfigureText(this TileMap tileMap, ushort firstTileId, string symbols)
    {
        TryInitText(tileMap);

        var hash = tileMap.GetHashCode();
        for (var i = 0; i < symbols.Length; i++)
            symbolMaps[hash][symbols[i]] = (ushort)(firstTileId + i);
    }

    public static ushort TileIdFrom(this TileMap tileMap, char symbol)
    {
        var id = default(ushort);
        if (symbolMaps[tileMap.GetHashCode()].TryGetValue(symbol, out var value))
            id = value;

        return id;
    }
    public static ushort[] TileIdsFrom(this TileMap tileMap, string? text)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        var result = new ushort[text.Length];
        for (var i = 0; i < text.Length; i++)
            result[i] = TileIdFrom(tileMap, text[i]);

        return result;
    }

    public static TileMap ToTrimmed(this TileMap tileMap)
    {
        var (xMin, xMax, yMin, yMax) = (tileMap.Size.width, 0, tileMap.Size.height, 0);

        for (var i = 0; i < tileMap.Size.height; i++)
            for (var j = 0; j < tileMap.Size.width; j++)
            {
                if (tileMap.TileAt((j, i)).Id == 0)
                    continue;

                xMin = Math.Min(xMin, j);
                xMax = Math.Max(xMax, j);
                yMin = Math.Min(yMin, i);
                yMax = Math.Max(yMax, i);
            }

        if (xMax < xMin || yMax < yMin)
            return new((0, 0));

        var result = new TileMap((xMax - xMin + 1, yMax - yMin + 1));
        result.SetTiles((0, 0), tileMap.TilesIn((xMin, yMin, result.Size.width, result.Size.height)));
        return result;
    }

#region Backend
    private static readonly Dictionary<int, Area?> masks = [];
    private static readonly Dictionary<int, (int x, int y, int z)> seeds = [];
    private static readonly Dictionary<int, Dictionary<char, ushort>> symbolMaps = [];

    private static void TryInitText(TileMap tileMap)
    {
        var hash = tileMap.GetHashCode();
        if (symbolMaps.ContainsKey(hash))
            return;

        symbolMaps[hash] = new()
        {
            { '░', 2 }, { '▒', 5 }, { '▓', 7 }, { '█', 10 },

            { '⅛', 140 }, { '⅐', 141 }, { '⅙', 142 }, { '⅕', 143 }, { '¼', 144 },
            { '⅓', 145 }, { '⅜', 146 }, { '⅖', 147 }, { '½', 148 }, { '⅗', 149 },
            { '⅝', 150 }, { '⅔', 151 }, { '¾', 152 }, { '⅘', 153 }, { '⅚', 154 }, { '⅞', 155 },

            { '₀', 156 }, { '₁', 157 }, { '₂', 158 }, { '₃', 159 }, { '₄', 160 },
            { '₅', 161 }, { '₆', 162 }, { '₇', 163 }, { '₈', 164 }, { '₉', 165 },

            { '⁰', 169 }, { '¹', 170 }, { '²', 171 }, { '³', 172 }, { '⁴', 173 },
            { '⁵', 174 }, { '⁶', 175 }, { '⁷', 176 }, { '⁸', 177 }, { '⁹', 178 },

            { '+', 182 }, { '-', 183 }, { '×', 184 }, { '―', 185 }, { '÷', 186 }, { '%', 187 },
            { '=', 188 }, { '≠', 189 }, { '≈', 190 }, { '√', 191 }, { '∫', 193 }, { 'Σ', 194 },
            { 'ε', 195 }, { 'γ', 196 }, { 'ϕ', 197 }, { 'π', 198 }, { 'δ', 199 }, { '∞', 200 },
            { '≪', 204 }, { '≫', 205 }, { '≤', 206 }, { '≥', 207 }, { '<', 208 }, { '>', 209 },
            { '(', 210 }, { ')', 211 }, { '[', 212 }, { ']', 213 }, { '{', 214 }, { '}', 215 },
            { '⊥', 216 }, { '∥', 217 }, { '∠', 218 }, { '∟', 219 }, { '~', 220 }, { '°', 221 },
            { '℃', 222 }, { '℉', 223 }, { '*', 224 }, { '^', 225 }, { '#', 226 }, { '№', 227 },
            { '$', 228 }, { '€', 229 }, { '£', 230 }, { '¥', 231 }, { '¢', 232 }, { '¤', 233 },

            { '!', 234 }, { '?', 235 }, { '.', 236 }, { ',', 237 }, { '…', 238 },
            { ':', 239 }, { ';', 240 }, { '"', 241 }, { '\'', 242 }, { '`', 243 }, { '–', 244 },
            { '_', 245 }, { '|', 246 }, { '/', 247 }, { '\\', 248 }, { '@', 249 }, { '&', 250 },
            { '®', 251 }, { '℗', 252 }, { '©', 253 }, { '™', 254 },

            // tiles can now be rotated, only a single arrow is stored as a tile, rather than 4
            { '→', 520 }, { '⇨', 522 }, { '➡', 523 },

            { '─', 260 }, { '┌', 261 }, { '├', 262 }, { '┼', 263 },
            { '═', 272 }, { '╔', 273 }, { '╠', 274 }, { '╬', 275 },

            { '♩', 428 }, { '♪', 429 }, { '♫', 430 }, { '♬', 431 }, { '♭', 432 }, { '♮', 433 },
            { '♯', 434 },

            { '★', 360 }, { '☆', 361 }, { '✓', 395 }, { '⏎', 399 },

            { '●', 604 }, { '○', 607 }, { '■', 598 }, { '□', 601 }, { '▲', 610 }, { '△', 612 },

            { '♟', 664 }, { '♜', 665 }, { '♞', 666 }, { '♝', 667 }, { '♛', 668 }, { '♚', 669 },
            { '♙', 670 }, { '♖', 671 }, { '♘', 672 }, { '♗', 673 }, { '♕', 674 }, { '♔', 675 },
            { '♠', 656 }, { '♥', 657 }, { '♣', 658 }, { '♦', 659 },
            { '♤', 660 }, { '♡', 661 }, { '♧', 662 }, { '♢', 663 },

            { '▕', 614 }
        };

        ConfigureText(tileMap);
    }

    private static float Random(this RangeF range, float seed = float.NaN)
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
    private static int Random(this RangeI range, float seed = float.NaN)
    {
        return (int)Math.Round(Random(((float)range.a, range.b), seed));
    }
    private static int ToSeed(TileMap map, (int a, int b, int c) parameters)
    {
        var (a, b, c) = parameters;
        var (x, y, z) = (0, 0, Random((-100_000_000, 100_000_000)));

        if (seeds.TryGetValue(map.GetHashCode(), out var seed))
            (x, y, z) = seed;

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