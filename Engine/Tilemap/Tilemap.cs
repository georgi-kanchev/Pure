namespace Pure.Engine.Tilemap;

using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

/// <summary>
/// Represents a tilemap consisting of a grid of tiles.
/// </summary>
public class Tilemap
{
    public (int x, int y, int z) SeedOffset { get; set; }

    /// <summary>
    /// Gets the size of the tilemap in tiles.
    /// </summary>
    public (int width, int height) Size { get; }
    public Area View { get; set; }

    /// <summary>
    /// Initializes a new tilemap instance with the specified size.
    /// </summary>
    /// <param name="size">The size of the tilemap in tiles.</param>
    public Tilemap((int width, int height) size)
    {
        var (w, h) = size;
        w = Math.Max(w, 1);
        h = Math.Max(h, 1);

        Size = (w, h);
        data = new Tile[w, h];
        bundleCache = new (int, uint, sbyte, bool, bool)[w, h];
        ids = new int[w, h];
        View = (0, 0, size.width, size.height);
    }
    /// <summary>
    /// Initializes a new tilemap instance with the specified tileData.
    /// </summary>
    /// <param name="tileData">The tile data to use for the tilemap.</param>
    /// <exception cref="ArgumentNullException">Thrown if tileData is null.</exception>
    public Tilemap(Tile[,] tileData)
    {
        if (tileData == null)
            throw new ArgumentNullException(nameof(tileData));

        var w = tileData.GetLength(0);
        var h = tileData.GetLength(1);
        Size = (w, h);
        data = Duplicate(tileData);
        bundleCache = new (int, uint, sbyte, bool, bool)[w, h];
        ids = new int[w, h];
        View = (0, 0, w, h);

        for (var i = 0; i < h; i++)
            for (var j = 0; j < w; j++)
            {
                bundleCache[j, i] = tileData[j, i];
                ids[j, i] = tileData[j, i].Id;
            }
    }
    public Tilemap(byte[] bytes)
    {
        var b = Decompress(bytes);
        var offset = 0;
        var w = BitConverter.ToInt32(Get<int>());
        var h = BitConverter.ToInt32(Get<int>());

        data = new Tile[w, h];
        bundleCache = new (int, uint, sbyte, bool, bool)[w, h];
        ids = new int[w, h];
        Size = (w, h);
        View = (BitConverter.ToInt32(Get<int>()), BitConverter.ToInt32(Get<int>()),
            BitConverter.ToInt32(Get<int>()), BitConverter.ToInt32(Get<int>()));

        for (var i = 0; i < h; i++)
            for (var j = 0; j < w; j++)
            {
                var bTile = GetBytesFrom(b, Tile.BYTE_SIZE, ref offset);
                SetTile((j, i), new(bTile));
            }

        return;

        byte[] Get<T>()
        {
            return GetBytesFrom(b, Marshal.SizeOf(typeof(T)), ref offset);
        }
    }
    public Tilemap(string base64) : this(Convert.FromBase64String(base64))
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
        result.AddRange(BitConverter.GetBytes(View.X));
        result.AddRange(BitConverter.GetBytes(View.Y));
        result.AddRange(BitConverter.GetBytes(View.Width));
        result.AddRange(BitConverter.GetBytes(View.Height));

        for (var i = 0; i < h; i++)
            for (var j = 0; j < w; j++)
                result.AddRange(TileAt((j, i)).ToBytes());

        return Compress(result.ToArray());
    }
    /// <returns>
    /// A 2D array of the bundle tuples of the tiles in the tilemap.</returns>
    public (int id, uint tint, sbyte turns, bool mirror, bool flip)[,] ToBundle()
    {
        return bundleCache;
    }

    public void Flush()
    {
        Array.Clear(data);
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

    /// <summary>
    /// Sets the tile at the specified cell to the specified tile.
    /// </summary>
    /// <param name="cell">The cell to set the tile at.</param>
    /// <param name="tile">The tile to set.</param>
    /// <param name="mask">An optional mask that skips any tile outside of it.</param>
    public void SetTile((int x, int y) cell, Tile tile, Area? mask = null)
    {
        if (IndicesAreValid(cell, mask) == false)
            return;

        data[cell.x, cell.y] = tile;
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
    /// <summary>
    /// Sets a group of tiles starting at the specified cell to the specified 2D tile array.
    /// </summary>
    /// <param name="cell">The cell to start setting tiles from.</param>
    /// <param name="tiles">The 2D array of tiles to set.</param>
    /// <param name="mask">An optional mask that skips any tile outside of it.</param>
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

        return;

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
    /// <summary>
    /// Sets the tiles in a rectangular area of the tilemap to create a box with corners, borders
    /// and filling.
    /// </summary>
    /// <param name="area">The area of the rectangular box.</param>
    /// <param name="tileFill">The tile to use for the filling of the box.</param>
    /// <param name="borderTileId">The identifier of the tile to use for the 
    /// straight edges of the box.</param>
    /// <param name="cornerTileId">The identifier of the tile to use for the corners of the box.</param>
    /// <param name="borderTint">The color to tint the border tiles.</param>
    /// <param name="mask">An optional mask that skips any tile outside of it.</param>
    public void SetBox(Area area, Tile tileFill, int cornerTileId, int borderTileId, uint borderTint = uint.MaxValue, Area? mask = null)
    {
        var (x, y) = (area.X, area.Y);
        var (w, h) = (area.Width, area.Height);

        if (w <= 0 || h <= 0)
            return;

        if (w == 1 || h == 1)
        {
            SetArea(area, mask, tileFill);
            return;
        }

        SetTile((x, y), new(cornerTileId, borderTint), mask);
        SetArea((x + 1, y, w - 2, 1), mask, new Tile(borderTileId, borderTint));
        SetTile((x + w - 1, y), new(cornerTileId, borderTint, 1), mask);

        if (h != 2)
        {
            SetArea((x, y + 1, 1, h - 2), mask, new Tile(borderTileId, borderTint, 3));

            if (tileFill.Id != Tile.SHADE_TRANSPARENT)
                SetArea((x + 1, y + 1, w - 2, h - 2), mask, tileFill);

            SetArea((x + w - 1, y + 1, 1, h - 2), mask, new Tile(borderTileId, borderTint, 1));
        }

        SetTile((x, y + h - 1), new(cornerTileId, borderTint, 3), mask);
        SetTile((x + w - 1, y + h - 1), new(cornerTileId, borderTint, 2), mask);
        SetArea((x + 1, y + h - 1, w - 2, 1), mask, new Tile(borderTileId, borderTint, 2));
    }
    /// <summary>
    /// Sets the tiles in a rectangular area of the tilemap to create a vertical or horizontal bar.
    /// </summary>
    /// <param name="cell">The cell of the top-left corner of the rectangular area 
    /// to create the bar.</param>
    /// <param name="tileIdEdge">The identifier of the tile to use for the edges of the bar.</param>
    /// <param name="tileId">The identifier of the tile to use for the 
    /// straight part of the bar.</param>
    /// <param name="tint">The color to tint the bar tiles.</param>
    /// <param name="size">The length of the bar in tiles.</param>
    /// <param name="vertical">Whether the bar should be vertical or horizontal.</param>
    /// <param name="mask">An optional mask that skips any tile outside of it.</param>
    public void SetBar((int x, int y) cell, int tileIdEdge, int tileId, uint tint = uint.MaxValue, int size = 5, bool vertical = false, Area? mask = null)
    {
        var (x, y) = cell;
        var off = size == 1 ? 0 : 1;

        if (vertical)
        {
            if (size > 1)
            {
                SetTile(cell, new(tileIdEdge, tint, 1), mask);
                SetTile((x, y + size - 1), new(tileIdEdge, tint, 3), mask);
            }

            if (size != 2)
                SetArea((x, y + off, 1, size - 2), mask, new Tile(tileId, tint, 1));

            return;
        }

        if (size > 1)
        {
            SetTile(cell, new(tileIdEdge, tint), mask);
            SetTile((x + size - 1, y), new(tileIdEdge, tint, 2), mask);
        }

        if (size != 2)
            SetArea((x + off, y, size - 2, 1), mask, new Tile(tileId, tint));
    }

    public void SetAutoTiles(Area area, Area? mask = null)
    {
        var result = new Dictionary<(int x, int y), Tile>();

        for (var i = area.X; i < area.Height; i++)
            for (var j = area.Y; j < area.Width; j++)
                foreach (var (rule, replacement) in autoTiles)
                {
                    var isMatch = true;

                    for (var k = 0; k < rule.Count; k++)
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
    public void AddAutoTileRule(int[] matchIds, Tile replacedCenter)
    {
        if (matchIds is { Length: 9 })
            autoTiles.Add((matchIds.ToList(), replacedCenter));
    }
    public void ClearAutoTileRules()
    {
        autoTiles.Clear();
    }

    /// <summary>
    /// Configures the tile identifiers for text characters and numbers assuming they are sequential left to right.
    /// </summary>
    /// <param name="lowercase">The tile identifier for the lowercase 'a' character.</param>
    /// <param name="uppercase">The tile identifier for the uppercase 'A' character.</param>
    /// <param name="numbers">The tile identifier for the '0' character.</param>
    public void ConfigureText(int lowercase = Tile.LOWERCASE_A, int uppercase = Tile.UPPERCASE_A, int numbers = Tile.NUMBER_0)
    {
        textIdLowercase = lowercase;
        textIdUppercase = uppercase;
        textIdNumbers = numbers;
    }
    /// <summary>
    /// Configures the tile identifiers for a set of symbols sequentially left to right.
    /// </summary>
    /// <param name="symbols">The string of symbols to configure.</param>
    /// <param name="leftmostTileId">The leftmost tile identifier for the symbols.</param>
    public void ConfigureText(string symbols, int leftmostTileId)
    {
        for (var i = 0; i < symbols.Length; i++)
            symbolMap[symbols[i]] = leftmostTileId + i;
    }

    /// <summary>
    /// Checks if a cell is inside the tilemap.
    /// </summary>
    /// <param name="cell">The cell to check.</param>
    /// <returns>True if the cell is overlapping with the tilemap, false otherwise.</returns>
    public bool IsOverlapping((int x, int y) cell)
    {
        return cell is { x: >= 0, y: >= 0 } &&
               cell.x <= Size.width - 1 &&
               cell.y <= Size.height - 1;
    }

    /// <summary>
    /// Converts a symbol to its corresponding tile identifier.
    /// </summary>
    /// <param name="symbol">The symbol to convert.</param>
    /// <returns>The tile identifier corresponding to the given symbol.</returns>
    public int TileIdFrom(char symbol)
    {
        var id = default(int);
        if (symbol is >= 'A' and <= 'Z')
            id = symbol - 'A' + textIdUppercase;
        else if (symbol is >= 'a' and <= 'z')
            id = symbol - 'a' + textIdLowercase;
        else if (symbol is >= '0' and <= '9')
            id = symbol - '0' + textIdNumbers;
        else if (symbolMap.TryGetValue(symbol, out var value))
            id = value;

        return id;
    }
    /// <summary>
    /// Converts a text to an array of tile identifiers.
    /// </summary>
    /// <param name="text">The text to convert.</param>
    /// <returns>An array of tile identifiers corresponding to the given symbols.</returns>
    public int[] TileIdsFrom(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return Array.Empty<int>();

        var result = new int[text.Length];
        for (var i = 0; i < text.Length; i++)
            result[i] = TileIdFrom(text[i]);

        return result;
    }
    /// <summary>
    /// Gets the tile at the specified cell.
    /// </summary>
    /// <param name="cell">The cell to get the tile from.</param>
    /// <returns>The tile at the specified cell, 
    /// or the default tile value if the cell is out of bounds.</returns>
    public Tile TileAt((int x, int y) cell)
    {
        return IndicesAreValid(cell, null) ? data[cell.x, cell.y] : default;
    }
    /// <summary>
    /// Retrieves a rectangular region of tiles from the tilemap.
    /// </summary>
    /// <param name="area">A tuple representing the area's cell and size. 
    /// The x and y values represent the top-left corner of the area, 
    /// while the width and height represent the size of the area.</param>
    /// <returns>A 2D array of tiles representing the specified rectangular region in the tilemap. 
    /// If the area's dimensions are negative, the method will reverse the direction of the iteration.</returns>
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

    /// <summary>
    /// Updates the view of the tilemap.
    /// </summary>
    /// <returns>The updated tilemap view.</returns>
    public Tilemap ViewUpdate()
    {
        var (vx, vy, vw, vh, _) = View.ToBundle();
        var result = new Tilemap((View.Width, View.Height));
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

    public Tilemap Duplicate()
    {
        return new(ToBytes());
    }

    /// <summary>
    /// Implicitly converts a 2D array of tiles to a tilemap object.
    /// </summary>
    /// <param name="data">The 2D array of tiles to convert.</param>
    /// <returns>A new tilemap object containing the given tiles.</returns>
    public static implicit operator Tilemap(Tile[,] data)
    {
        return new(data);
    }
    /// <summary>
    /// Implicitly converts a tilemap object to a 2D array of tiles.
    /// </summary>
    /// <param name="tilemap">The tilemap object to convert.</param>
    /// <returns>A new 2D array of tiles containing the tiles from the tilemap object.</returns>
    public static implicit operator Tile[,](Tilemap tilemap)
    {
        return Duplicate(tilemap.data);
    }
    /// <summary>
    /// Implicitly converts a tilemap object to a 2D array of tile bundles.
    /// </summary>
    /// <param name="tilemap">The tilemap object to convert.</param>
    /// <returns>A new 2D array of tile bundles containing the tiles from the tilemap object.</returns>
    public static implicit operator (int id, uint tint, sbyte turns, bool mirror, bool flip)[,](Tilemap tilemap)
    {
        return tilemap.ToBundle();
    }
    public static implicit operator Area(Tilemap tilemap)
    {
        return (0, 0, tilemap.Size.width, tilemap.Size.height);
    }
    public static implicit operator int[,](Tilemap tilemap)
    {
        return tilemap.ids;
    }
    public static implicit operator byte[](Tilemap tilemap)
    {
        return tilemap.ToBytes();
    }
    public static implicit operator Tilemap(byte[] bytes)
    {
        return new(bytes);
    }

    #region Backend
    private int textIdNumbers = Tile.NUMBER_0,
        textIdUppercase = Tile.UPPERCASE_A,
        textIdLowercase = Tile.LOWERCASE_A;
    private readonly Dictionary<char, int> symbolMap = new()
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

        { '♩', 357 }, { '♪', 358 }, { '♫', 359 }, { '♬', 360 }, { '♭', 361 }, { '♮', 362 },
        { '♯', 363 },

        { '★', 333 }, { '☆', 334 }, { '✓', 338 }, { '⏎', 339 },

        { '●', 423 }, { '○', 427 }, { '■', 417 }, { '□', 420 }, { '▲', 428 }, { '△', 430 },

        { '♟', 404 }, { '♜', 405 }, { '♞', 406 }, { '♝', 407 }, { '♛', 408 }, { '♚', 409 },
        { '♙', 410 }, { '♖', 411 }, { '♘', 412 }, { '♗', 413 }, { '♕', 414 }, { '♔', 415 },
        { '♠', 396 }, { '♥', 397 }, { '♣', 398 }, { '♦', 399 },
        { '♤', 400 }, { '♡', 401 }, { '♧', 402 }, { '♢', 403 },

        { '▕', 432 }
    };
    private readonly List<(List<int> rule, Tile replacement)> autoTiles = new();

    private readonly Tile[,] data;
    private readonly (int, uint, sbyte, bool, bool)[,] bundleCache;
    private readonly int[,] ids;

    public static (int, int) FromIndex(int index, (int width, int height) size)
    {
        index = index < 0 ? 0 : index;
        index = index > size.width * size.height - 1 ? size.width * size.height - 1 : index;

        return (index % size.width, index / size.width);
    }
    private bool IndicesAreValid((int x, int y) indices, Area? mask)
    {
        var (x, y) = indices;
        mask ??= new(0, 0, Size.width, Size.height);
        var (mx, my, mw, mh, _) = mask.Value.ToBundle();

        return x >= mx && y >= my && x < mx + mw && y < my + mh;
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

    internal static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
            stream.Write(data, 0, data.Length);

        return output.ToArray();
    }
    internal static byte[] Decompress(byte[] data)
    {
        var input = new MemoryStream(data);
        var output = new MemoryStream();
        using (var stream = new DeflateStream(input, CompressionMode.Decompress))
            stream.CopyTo(output);

        return output.ToArray();
    }
    internal static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
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