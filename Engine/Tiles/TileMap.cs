using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Pure.Engine.Tiles;

public class TileMap
{
    public List<(int[] matchIds, Tile replacedCenter)> AutoTiles { get; } = [];
    public (int x, int y, int z) SeedOffset { get; set; }

    public (int width, int height) Size { get; }
    public Area View { get; set; }

    public TileMap((int width, int height) size)
    {
        var (w, h) = (Math.Max(size.width, 1), Math.Max(size.height, 1));

        Size = (w, h);
        data = new Tile[w, h];
        bundleCache = new (ushort id, uint tint, byte pose)[w, h];
        ids = new ushort[w, h];
        View = (0, 0, w, h);
        ConfigureText();
    }
    public TileMap(Tile[,]? tileData)
    {
        tileData ??= new Tile[0, 0];

        var w = tileData.GetLength(0);
        var h = tileData.GetLength(1);
        Size = (w, h);
        data = Duplicate(tileData);
        bundleCache = new (ushort id, uint tint, byte pose)[w, h];
        ids = new ushort[w, h];
        View = (0, 0, w, h);

        for (var i = 0; i < h; i++)
            for (var j = 0; j < w; j++)
            {
                bundleCache[j, i] = tileData[j, i];
                ids[j, i] = tileData[j, i].Id;
            }

        ConfigureText();
    }

    public (ushort id, uint tint, byte pose)[,] ToBundle()
    {
        TryRestoreCache();
        return bundleCache;
    }

    public void Flush()
    {
        Array.Clear(data);
        TryRestoreCache();
        Array.Clear(ids);
        Array.Clear(bundleCache);
    }
    public void Fill(Area? mask = null, params Tile[]? tiles)
    {
        if (tiles == null || tiles.Length == 0)
        {
            Flush();
            return;
        }

        for (var y = 0; y < Size.height; y++)
            for (var x = 0; x < Size.width; x++)
            {
                var tile = tiles.Length == 1 ? tiles[0] : ChooseOne(tiles, ToSeed((x, y)));
                SetTile((x, y), tile, mask);
            }
    }
    public void Flood((int x, int y) cell, bool exactTile, Area? mask = null, params Tile[]? tiles)
    {
        if (tiles == null || tiles.Length == 0)
            return;

        var stack = new Stack<(int x, int y)>();
        var initialTile = TileAt(cell);
        stack.Push(cell);

        while (stack.Count > 0)
        {
            var (x, y) = stack.Pop();
            var curTile = TileAt((x, y));
            var tile = tiles.Length == 1 ? tiles[0] : ChooseOne(tiles, ToSeed((x, y)));
            var exact = curTile == tile || curTile != initialTile;
            var onlyId = curTile.Id == tile.Id || curTile.Id != initialTile.Id;

            if ((exactTile && exact) ||
                (exactTile == false && onlyId))
                continue;

            SetTile((x, y), tile, mask);

            if (IsOverlapping((x - 1, y)))
                stack.Push((x - 1, y));
            if (IsOverlapping((x + 1, y)))
                stack.Push((x + 1, y));
            if (IsOverlapping((x, y - 1)))
                stack.Push((x, y - 1));
            if (IsOverlapping((x, y + 1)))
                stack.Push((x, y + 1));
        }
    }
    public void Replace(Area area, Tile targetTile, Area? mask = null, params Tile[] tiles)
    {
        if (tiles.Length == 0)
            return;

        for (var i = 0; i < Math.Abs(area.Width * area.Height); i++)
        {
            var x = area.X + i % Math.Abs(area.Width) * (area.Width < 0 ? -1 : 1);
            var y = area.Y + i / Math.Abs(area.Width) * (area.Height < 0 ? -1 : 1);

            if (TileAt((x, y)).Id != targetTile.Id)
                continue;

            var tile = tiles.Length == 1 ? tiles[0] : ChooseOne(tiles, ToSeed((x, y)));
            SetTile((x, y), tile, mask);
        }
    }

    public void SetTile((int x, int y) cell, Tile tile, Area? mask = null)
    {
        if (IndicesAreValid(cell, mask) == false)
            return;

        data[cell.x, cell.y] = tile;
        TryRestoreCache();
        ids[cell.x, cell.y] = tile.Id;
        bundleCache[cell.x, cell.y] = tile;
    }
    public void SetArea(Area area, Area? mask = null, params Tile[]? tiles)
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
                SetTile((x, y), tile, mask);
                i++;
            }
    }
    public void SetGroup((int x, int y) cell, Tile[,] tiles, Area? mask = null)
    {
        if (tiles.Length == 0)
            return;

        for (var i = 0; i < tiles.GetLength(1); i++)
            for (var j = 0; j < tiles.GetLength(0); j++)
                SetTile((cell.x + j, cell.y + i), tiles[j, i], mask);
    }
    public void SetText((int x, int y) cell, string? text, uint tint = uint.MaxValue, char tintBrush = '#', Area? mask = null)
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

            var index = TileIdFrom(text[j]);
            if (index != default && text[j] != ' ')
                SetTile((x, y), new(index, tint), mask);

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
    public void SetEllipse((int x, int y) cell, (int width, int height) radius, bool fill, Area? mask = null, params Tile[]? tiles)
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
                SetTile((c.x + x, c.y - y), o ? tiles[0] : ChooseOne(tiles, ToSeed((c.x + x, c.y - y))),
                    mask);
                SetTile((c.x - x, c.y - y), o ? tiles[0] : ChooseOne(tiles, ToSeed((c.x - x, c.y - y))),
                    mask);
                SetTile((c.x - x, c.y + y), o ? tiles[0] : ChooseOne(tiles, ToSeed((c.x - x, c.y + y))),
                    mask);
                SetTile((c.x + x, c.y + y), o ? tiles[0] : ChooseOne(tiles, ToSeed((c.x + x, c.y + y))),
                    mask);
                return;
            }

            for (var i = c.x - x; i <= c.x + x; i++)
            {
                SetTile((i, c.y - y), o ? tiles[0] : ChooseOne(tiles, ToSeed((i, c.y - y))), mask);
                SetTile((i, c.y + y), o ? tiles[0] : ChooseOne(tiles, ToSeed((i, c.y + y))), mask);
            }
        }
    }

    public void SetLine((int x, int y) cellA, (int x, int y) cellB, Area? mask = null, params Tile[]? tiles)
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
            SetTile((x0, y0), tile, mask);

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
    public void SetBox(Area area, Tile fill, Tile corner, Tile edge, Area? mask = null)
    {
        var (x, y) = (area.X, area.Y);
        var (w, h) = (area.Width, area.Height);

        if (w <= 0 || h <= 0)
            return;

        if (w == 1 || h == 1)
        {
            SetArea(area, mask, fill);
            return;
        }

        SetTile((x, y), new(corner.Id, corner.Tint), mask);
        SetArea((x + 1, y, w - 2, 1), mask, new Tile(edge.Id, edge.Tint));
        SetTile((x + w - 1, y), new(corner.Id, corner.Tint, Pose.Right), mask);

        if (h != 2)
        {
            SetArea((x, y + 1, 1, h - 2), mask, new Tile(edge.Id, edge.Tint, Pose.Left));

            if (fill.Id != Tile.EMPTY)
                SetArea((x + 1, y + 1, w - 2, h - 2), mask, fill);

            SetArea((x + w - 1, y + 1, 1, h - 2), mask, new Tile(edge.Id, edge.Tint, Pose.Right));
        }

        SetTile((x, y + h - 1), new(corner.Id, corner.Tint, Pose.Left), mask);
        SetTile((x + w - 1, y + h - 1), new(corner.Id, corner.Tint, Pose.Down), mask);
        SetArea((x + 1, y + h - 1, w - 2, 1), mask, new Tile(edge.Id, edge.Tint, Pose.Down));
    }
    public void SetBar((int x, int y) cell, Tile edge, Tile fill, int size = 5, bool vertical = false, Area? mask = null)
    {
        var (x, y) = cell;
        var off = size == 1 ? 0 : 1;

        if (vertical)
        {
            if (size > 1)
            {
                SetTile(cell, new(edge.Id, edge.Tint, Pose.Right), mask);
                SetTile((x, y + size - 1), new(edge.Id, edge.Tint, Pose.Left), mask);
            }

            if (size != 2)
                SetArea((x, y + off, 1, size - 2), mask, new Tile(fill.Id, fill.Tint, Pose.Right));

            return;
        }

        if (size > 1)
        {
            SetTile(cell, new(edge.Id, edge.Tint), mask);
            SetTile((x + size - 1, y), new(edge.Id, edge.Tint, Pose.Down), mask);
        }

        if (size != 2)
            SetArea((x + off, y, size - 2, 1), mask, new Tile(fill.Id, fill.Tint));
    }

    public void SetAutoTiles(Area area, Area? mask = null)
    {
        var result = new Dictionary<(int x, int y), Tile>();

        for (var i = area.X; i < area.Height; i++)
            for (var j = area.Y; j < area.Width; j++)
                foreach (var (rule, replacement) in AutoTiles)
                {
                    if (rule is not { Length: 9 })
                        continue;

                    var isMatch = true;

                    for (var k = 0; k < rule.Length; k++)
                    {
                        var (ox, oy) = (k % 3 - 1, k / 3 - 1);
                        var offTile = TileAt((j + ox, i + oy));

                        if (offTile.Id == rule[k] || rule[k] < 0)
                            continue;

                        isMatch = false;
                        break;
                    }

                    if (isMatch)
                        result[(j, i)] = replacement;
                }

        foreach (var kvp in result)
            SetTile(kvp.Key, kvp.Value, mask);
    }

    public void ConfigureText(ushort lowercase = Tile.LOWERCASE_A, ushort uppercase = Tile.UPPERCASE_A, ushort numbers = Tile.NUMBER_0)
    {
        ConfigureText(lowercase, "abcdefghijklmnopqrstuvwxyz");
        ConfigureText(uppercase, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
        ConfigureText(numbers, "0123456789");
    }
    public void ConfigureText(ushort firstTileId, string symbols)
    {
        for (var i = 0; i < symbols.Length; i++)
            symbolMap[symbols[i]] = (ushort)(firstTileId + i);
    }

    public bool IsOverlapping((int x, int y) cell)
    {
        return cell is { x: >= 0, y: >= 0 } &&
               cell.x <= Size.width - 1 &&
               cell.y <= Size.height - 1;
    }

    public ushort TileIdFrom(char symbol)
    {
        var id = default(ushort);
        if (symbolMap.TryGetValue(symbol, out var value))
            id = value;

        return id;
    }
    public ushort[] TileIdsFrom(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return [];

        var result = new ushort[text.Length];
        for (var i = 0; i < text.Length; i++)
            result[i] = TileIdFrom(text[i]);

        return result;
    }
    public Tile TileAt((int x, int y) cell)
    {
        return IndicesAreValid(cell, null) ? data[cell.x, cell.y] : default;
    }
    public Tile[,] TilesIn(Area area)
    {
        var (rx, ry) = (area.X, area.Y);
        var (rw, rh) = (area.Width, area.Height);
        var xStep = rw < 0 ? -1 : 1;
        var yStep = rh < 0 ? -1 : 1;
        var result = new Tile[Math.Abs(rw), Math.Abs(rh)]; // Fixed array dimensions

        for (var x = 0; x < Math.Abs(rw); x++)
            for (var y = 0; y < Math.Abs(rh); y++)
            {
                var currentX = rx + x * xStep - (rw < 0 ? 1 : 0);
                var currentY = ry + y * yStep - (rh < 0 ? 1 : 0);

                result[x, y] = TileAt((currentX, currentY));
            }

        return result;
    }

    public TileMap UpdateView()
    {
        var (vx, vy, vw, vh, _) = View.ToBundle();
        var result = new TileMap((View.Width, View.Height));
        var i = 0;
        for (var x = vx; x != vx + vw; x++)
        {
            var j = 0;
            for (var y = vy; y != vy + vh; y++)
            {
                result.SetTile((i, j), TileAt((x, y)));
                j++;
            }

            i++;
        }

        return result;
    }

    public static implicit operator TileMap?(Tile[,]? data)
    {
        return data == null ? null : new(data);
    }
    public static implicit operator Tile[,]?(TileMap? tileMap)
    {
        return tileMap == null ? null : Duplicate(tileMap.data);
    }
    public static implicit operator (ushort id, uint tint, byte pose)[,]?(TileMap? tileMap)
    {
        return tileMap?.ToBundle();
    }
    public static implicit operator Area(TileMap tileMap)
    {
        return (0, 0, tileMap.Size.width, tileMap.Size.height);
    }
    public static implicit operator Area?(TileMap? tileMap)
    {
        return tileMap == null ? null : (0, 0, tileMap.Size.width, tileMap.Size.height);
    }
    public static implicit operator ushort[,]?(TileMap? tileMap)
    {
        return tileMap?.ids;
    }

#region Backend
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
    internal class DoNotSave : Attribute;

    private readonly Dictionary<char, ushort> symbolMap = new()
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

        //{ '→', 282 }, { '↓', 283 }, { '←', 284 }, { '↑', 285 },
        //{ '⇨', 330 }, { '⇩', 331 }, { '⇦', 332 }, { '⇧', 333 },
        //{ '➡', 334 }, { '⬇', 335 }, { '⬅', 336 }, { '⬆', 337 },

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

    private readonly Tile[,] data;
    [DoNotSave]
    private (ushort id, uint tint, byte pose)[,]? bundleCache;
    [DoNotSave]
    private ushort[,]? ids;

    public static (int, int) FromIndex(int index, (int width, int height) size)
    {
        index = index < 0 ? 0 : index;
        index = index > size.width * size.height - 1 ? size.width * size.height - 1 : index;

        return (index % size.width, index / size.width);
    }
    private bool IndicesAreValid((int x, int y) indices, Area? mask)
    {
        var (x, y) = indices;
        var (w, h) = Size;
        mask ??= new(0, 0, Size.width, Size.height);
        var (mx, my, mw, mh, _) = mask.Value.ToBundle();
        var isInMask = x >= mx && y >= my && x < mx + mw && y < my + mh;

        return isInMask && x >= 0 && y >= 0 && x < w && y < h;
    }
    private static T[,] Duplicate<T>(T[,] array)
    {
        var copy = new T[array.GetLength(0), array.GetLength(1)];
        Array.Copy(array, copy, array.Length);
        return copy;
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
    private static float Limit(float number, float rangeA, float rangeB, bool isOverflowing = false)
    {
        if (rangeA > rangeB)
            (rangeA, rangeB) = (rangeB, rangeA);

        if (isOverflowing)
        {
            var d = rangeB - rangeA;
            return ((number - rangeA) % d + d) % d + rangeA;
        }
        else
        {
            if (number < rangeA)
                return rangeA;
            else if (number > rangeB)
                return rangeB;
            return number;
        }
    }
    private static int Limit(int number, int rangeA, int rangeB, bool isOverflowing = false)
    {
        return (int)Limit((float)number, rangeA, rangeB, isOverflowing);
    }
    private static T ChooseOne<T>(IList<T> collection, float seed)
    {
        return collection[(int)Random(0, collection.Count - 1, 0, seed)];
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

    [MemberNotNull(nameof(bundleCache), nameof(ids))]
    private void TryRestoreCache()
    {
        if (ids != null && bundleCache != null)
            return;

        var w = data.GetLength(0);
        var h = data.GetLength(1);
        bundleCache = new (ushort id, uint tint, byte pose)[w, h];
        ids = new ushort[w, h];

        for (var i = 0; i < h; i++)
            for (var j = 0; j < w; j++)
            {
                bundleCache[j, i] = data[j, i];
                ids[j, i] = data[j, i].Id;
            }
    }
    private int ToSeed((int a, int b) parameters)
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
#endregion
}