namespace Pure.Engine.Tilemap;

using System.IO.Compression;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a tilemap consisting of a grid of tiles.
/// </summary>
public class Tilemap
{
    /// <summary>
    /// Specifies the alignment of the text in <see cref="SetTextRectangle"/>.
    /// </summary>
    public enum Alignment
    {
        TopLeft,
        Top,
        TopRight,
        Left,
        Center,
        Right,
        BottomLeft,
        Bottom,
        BottomRight
    }

    /// <summary>
    /// Gets the size of the tilemap in tiles.
    /// </summary>
    public (int width, int height) Size
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets or sets the position and the size of the camera viewport.
    /// </summary>
    public (int x, int y, int height, int width) Camera
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the identifiers of the tiles in the tilemap.
    /// </summary>
    public int[,] Ids
    {
        get
        {
            var result = new int[data.GetLength(0), data.GetLength(1)];
            for (var j = 0; j < data.GetLength(1); j++)
                for (var i = 0; i < data.GetLength(0); i++)
                    result[i, j] = data[i, j].Id;

            return result;
        }
    }

    public Tilemap(byte[] bytes)
    {
        var b = Decompress(bytes);
        var offset = 0;
        var w = BitConverter.ToInt32(Get<int>());
        var h = BitConverter.ToInt32(Get<int>());

        data = new Tile[w, h];
        Size = (w, h);
        Camera = (BitConverter.ToInt32(Get<int>()),
            BitConverter.ToInt32(Get<int>()),
            BitConverter.ToInt32(Get<int>()),
            BitConverter.ToInt32(Get<int>()));

        for (var i = 0; i < h; i++)
            for (var j = 0; j < w; j++)
            {
                var bTile = bytes[offset..(offset + Tile.BYTE_SIZE)];
                SetTile((j, i), new Tile(bTile));
                offset += Tile.BYTE_SIZE;
            }

        return;

        byte[] Get<T>()
        {
            return GetBytesFrom(b, Marshal.SizeOf(typeof(T)), ref offset);
        }
    }
    /// <summary>
    /// Initializes a new tilemap instance with the specified size.
    /// </summary>
    /// <param name="size">The size of the tilemap in tiles.</param>
    public Tilemap((int width, int height) size)
    {
        var (w, h) = size;
        Size = size;
        if (w < 1)
            w = 1;
        if (h < 1)
            h = 1;

        data = new Tile[w, h];
        Camera = (0, 0, size.width, size.height);
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
        data = Copy(tileData);
        Camera = (0, 0, w, h);
    }

    /// <summary>
    /// Updates the camera's view of the tilemap.
    /// </summary>
    /// <returns>The updated tilemap view.</returns>
    public Tilemap CameraUpdate()
    {
        var (cx, cy, w, h) = Camera;
        var newData = new Tile[Math.Abs(w), Math.Abs(h)];
        var xStep = w < 0 ? -1 : 1;
        var yStep = h < 0 ? -1 : 1;
        var i = 0;
        for (var x = cx; x != cx + w; x += xStep)
        {
            var j = 0;
            for (var y = cy; y != cy + h; y += yStep)
            {
                newData[i, j] = TileAt((x, y));
                j++;
            }

            i++;
        }

        return new(newData);
    }

    /// <summary>
    /// Gets the tile at the specified position.
    /// </summary>
    /// <param name="position">The position to get the tile from.</param>
    /// <returns>The tile at the specified position, 
    /// or the default tile value if the position is out of bounds.</returns>
    public Tile TileAt((int x, int y) position)
    {
        return IndicesAreValid(position) ? data[position.x, position.y] : default;
    }

    /// <summary>
    /// Fills the entire tilemap with the specified tile.
    /// </summary>
    /// <param name="tile">The tile to fill the tilemap with.</param>
    public void Fill(Tile tile = default)
    {
        for (var y = 0; y < Size.height; y++)
            for (var x = 0; x < Size.width; x++)
                SetTile((x, y), tile);
    }

    public void Flood((int x, int y) position, Tile tile)
    {
        var stack = new Stack<(int x, int y)>();
        var initialTile = TileAt(position);
        stack.Push(position);

        while (stack.Count > 0)
        {
            var currentPosition = stack.Pop();
            var curTile = TileAt(currentPosition);

            if (currentPosition.x < 0 || currentPosition.x >= Size.width ||
                currentPosition.y < 0 || currentPosition.y >= Size.height ||
                curTile == tile || curTile != initialTile)
                continue;

            SetTile(currentPosition, tile);

            stack.Push((currentPosition.x - 1, currentPosition.y));
            stack.Push((currentPosition.x + 1, currentPosition.y));
            stack.Push((currentPosition.x, currentPosition.y - 1));
            stack.Push((currentPosition.x, currentPosition.y + 1));
        }
    }
    public void Replace(
        (int x, int y) position,
        (int width, int height) size,
        Tile targetTile,
        (int x, int y, int z) seedOffset = default,
        params Tile[] tiles)
    {
        if (tiles.Length == 0)
            return;

        var xStep = size.width < 0 ? -1 : 1;
        var yStep = size.height < 0 ? -1 : 1;
        var i = 0;
        var (sx, sy, sz) = seedOffset;
        for (var x = position.x; x != position.x + size.width; x += xStep)
            for (var y = position.y; y != position.y + size.height; y += yStep)
            {
                if (i > Math.Abs(size.width * size.height))
                    return;

                if (TileAt((x, y)) != targetTile)
                    continue;

                SetTile((x, y), ChooseOne(tiles, HashCode.Combine(x + sx, y + sy) + sz));
                i++;
            }
    }

    /// <summary>
    /// Sets the tile at the specified position 
    /// to the specified tile.
    /// </summary>
    /// <param name="position">The position to set the tile at.</param>
    /// <param name="tile">The tile to set.</param>
    public void SetTile((int x, int y) position, Tile tile)
    {
        if (IndicesAreValid(position) == false)
            return;

        data[position.x, position.y] = tile;
    }
    /// <summary>
    /// Sets a rectangle region of tiles starting at the specified position with the 
    /// specified size to the specified tile.
    /// </summary>
    /// <param name="position">The position to start setting tiles from.</param>
    /// <param name="size">The size of the rectangle region to set tiles for.</param>
    /// <param name="tile">The tile to set the rectangle region to.</param>
    public void SetRectangle((int x, int y) position, (int width, int height) size, Tile tile)
    {
        var xStep = size.width < 0 ? -1 : 1;
        var yStep = size.height < 0 ? -1 : 1;
        var i = 0;
        for (var x = position.x; x != position.x + size.width; x += xStep)
            for (var y = position.y; y != position.y + size.height; y += yStep)
            {
                if (i > Math.Abs(size.width * size.height))
                    return;

                SetTile((x, y), tile);
                i++;
            }
    }
    /// <summary>
    /// Sets a group of tiles starting at the specified position to the 
    /// specified 2D tile array.
    /// </summary>
    /// <param name="position">The position to start setting tiles from.</param>
    /// <param name="tiles">The 2D array of tiles to set.</param>
    public void SetGroup((int x, int y) position, Tile[,] tiles)
    {
        if (tiles.Length == 0)
            return;

        for (var i = 0; i < tiles.GetLength(1); i++)
            for (var j = 0; j < tiles.GetLength(0); j++)
                SetTile((position.x + i, position.y + j), tiles[j, i]);
    }

    /// <summary>
    /// Sets a single line of text starting from a position with optional tint and optional shortening.
    /// </summary>
    /// <param name="position">The starting position to place the text.</param>
    /// <param name="text">The text to display.</param>
    /// <param name="tint">Optional tint color value (defaults to white).</param>
    /// <param name="maxLength">Optional shortening that adds ellipis '…' if exceeded
    /// (defaults to none). Negative values reduce the text from the back.</param>
    public void SetTextLine(
        (int x, int y) position,
        string? text,
        uint tint = uint.MaxValue,
        int maxLength = int.MaxValue)
    {
        var errorOffset = 0;

        if (maxLength == 0)
            return;

        if (text != null)
        {
            var abs = Math.Abs(maxLength);
            if (maxLength > 0 && text.Length > maxLength)
                text = text[..Math.Max(abs - 1, 0)] + "…";
            else if (maxLength < 0 && text.Length > abs)
                text = "…" + text[^(abs - 1)..];
        }

        for (var i = 0; i < text?.Length; i++)
        {
            var symbol = text[i];
            var index = TileIdFrom(symbol);

            if (index == default && symbol != ' ')
            {
                errorOffset++;
                continue;
            }

            if (symbol == ' ')
                continue;

            SetTile((position.x + i - errorOffset, position.y), new(index, tint));
        }
    }
    /// <summary>
    /// Sets a rectangle of text with optional 
    /// alignment, scrolling, and word wrapping.
    /// </summary>
    /// <param name="position">The starting position to place the text.</param>
    /// <param name="size">The width and height of the rectangle.</param>
    /// <param name="text">The text to display.</param>
    /// <param name="tint">Optional tint color value (defaults to white).</param>
    /// <param name="isWordWrapping">Optional flag for enabling word wrapping.</param>
    /// <param name="alignment">Optional text alignment.</param>
    /// <param name="scrollProgress">Optional scrolling value (between 0 and 1).</param>
    public void SetTextRectangle(
        (int x, int y) position,
        (int width, int height) size,
        string? text,
        uint tint = uint.MaxValue,
        bool isWordWrapping = true,
        Alignment alignment = Alignment.TopLeft,
        float scrollProgress = 0)
    {
        if (string.IsNullOrEmpty(text) || size.width <= 0 || size.height <= 0)
            return;

        var x = position.x;
        var y = position.y;
        var lineList = text.TrimEnd().Split(Environment.NewLine).ToList();

        if (lineList.Count == 0)
            return;

        for (var i = 0; i < lineList.Count; i++)
        {
            var line = lineList[i];

            if (line.Length <= size.width) // line is valid length
                continue;

            var lastLineIndex = size.width - 1;
            var newLineIndex = isWordWrapping ?
                GetSafeNewLineIndex(line, (uint)lastLineIndex) :
                lastLineIndex;

            // end of line? can't word wrap, proceed to symbol wrap
            if (newLineIndex == 0)
            {
                lineList[i] = line[..size.width];
                lineList.Insert(i + 1, line[size.width..line.Length]);
                continue;
            }

            // otherwise wordwrap
            var endIndex = newLineIndex + (isWordWrapping ? 0 : 1);
            lineList[i] = line[..endIndex].TrimStart();
            lineList.Insert(i + 1, line[(newLineIndex + 1)..line.Length]);
        }

        var yDiff = size.height - lineList.Count;

        if (alignment is Alignment.Left or Alignment.Center or Alignment.Right)
            for (var i = 0; i < yDiff / 2; i++)
                lineList.Insert(0, string.Empty);
        else if (alignment is Alignment.BottomLeft or Alignment.Bottom or Alignment.BottomRight)
            for (var i = 0; i < yDiff; i++)
                lineList.Insert(0, string.Empty);

        // new lineList.Count
        yDiff = size.height - lineList.Count;

        var startIndex = 0;
        var end = size.height;
        var scrollValue = (int)Math.Round(scrollProgress * (lineList.Count - size.height));

        if (yDiff < 0)
        {
            startIndex += scrollValue;
            end += scrollValue;
        }

        var e = lineList.Count - size.height;
        startIndex = Math.Clamp(startIndex, 0, Math.Max(e, 0));
        end = Math.Clamp(end, 0, lineList.Count);

        for (var i = startIndex; i < end; i++)
        {
            var line = lineList[i];

            if (alignment is Alignment.TopRight or Alignment.Right or Alignment.BottomRight)
                line = line.PadLeft(size.width);
            else if (alignment is Alignment.Top or Alignment.Center or Alignment.Bottom)
                line = PadLeftAndRight(line, size.width);

            SetTextLine((x, y), line, tint);
            NewLine();
        }

        return;

        void NewLine()
        {
            x = position.x;
            y++;
        }
        int GetSafeNewLineIndex(string line, uint endLineIndex)
        {
            for (var i = (int)endLineIndex; i >= 0; i--)
                if (line[i] == ' ' && i <= size.width)
                    return i;

            return default;
        }
    }
    /// <summary>
    /// Sets the tint of the tiles in a rectangular area of the tilemap to 
    /// highlight a specific text (if found).
    /// </summary>
    /// <param name="position">The position of the top-left corner of the rectangular 
    /// area to search for the text.</param>
    /// <param name="size">The size of the rectangular area to search for the text.</param>
    /// <param name="text">The text to search for and highlight.</param>
    /// <param name="tint">The color to tint the matching tiles.</param>
    /// <param name="isMatchingWord">Whether to only match the text 
    /// as a whole word or any symbols.</param>
    public void SetTextRectangleTint(
        (int x, int y) position,
        (int width, int height) size,
        string? text,
        uint tint = uint.MaxValue,
        bool isMatchingWord = false)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        var xStep = size.width < 0 ? -1 : 1;
        var yStep = size.height < 0 ? -1 : 1;
        var tileList = TileIdsFrom(text).ToList();

        for (var x = position.x; x != position.x + size.width; x += xStep)
            for (var y = position.y; y != position.y + size.height; y += yStep)
            {
                if (tileList[0] != TileAt((x, y)).Id)
                    continue;

                var correctSymbolCount = 0;
                var curX = x;
                var curY = y;
                var startPos = (x - 1, y);

                for (var i = 0; i < text.Length; i++)
                {
                    if (tileList[i] != TileAt((curX, curY)).Id)
                        break;

                    correctSymbolCount++;
                    curX++;

                    // try new line
                    if (curX <= x + size.width)
                        continue;

                    curX = position.x;
                    curY++;
                }

                var endPos = (curX, curY);
                var left = TileAt(startPos).Id == 0 || curX == position.x;
                var right = TileAt(endPos).Id == 0 || curX == position.x + size.width;
                var isWord = left && right;

                if (isWord ^ isMatchingWord)
                    continue;

                if (text.Length != correctSymbolCount)
                    continue;

                curX = x;
                curY = y;
                for (var i = 0; i < text.Length; i++)
                {
                    if (curX > x + size.width) // try new line
                    {
                        curX = position.x;
                        curY++;
                    }

                    SetTile((curX, curY), new(TileAt((curX, curY)).Id, tint));
                    curX++;
                }
            }
    }

    public void SetEllipse(
        (int x, int y) center,
        (int width, int height) radius,
        Tile tile,
        bool isFilled = true)
    {
        var sqrRX = radius.width * radius.width;
        var sqrRY = radius.height * radius.height;
        var x = 0;
        var y = radius.height;
        var px = 0;
        var py = (sqrRX * 2) * y;

        // Region 1
        var p = (int)(sqrRY - (sqrRX * radius.height) + (0.25f * sqrRX));
        while (px < py)
        {
            Set();

            x++;
            px += (sqrRY * 2);

            if (p < 0)
                p += sqrRY + px;
            else
            {
                y--;
                py -= (sqrRX * 2);
                p += sqrRY + px - py;
            }
        }

        // Region 2
        p = (int)(sqrRY * (x + 0.5f) * (x + 0.5f) + sqrRX * (y - 1) * (y - 1) - sqrRX * sqrRY);
        while (y >= 0)
        {
            Set();

            y--;
            py -= (sqrRX * 2);

            if (p > 0)
                p += sqrRX - py;
            else
            {
                x++;
                px += (sqrRY * 2);
                p += sqrRX - py + px;
            }
        }

        return;

        void Set()
        {
            if (isFilled == false)
            {
                SetTile((center.x + x, center.y - y), tile);
                SetTile((center.x - x, center.y - y), tile);
                SetTile((center.x - x, center.y + y), tile);
                SetTile((center.x + x, center.y + y), tile);
                return;
            }

            for (var i = center.x - x; i <= center.x + x; i++)
            {
                SetTile((i, center.y - y), tile);
                SetTile((i, center.y + y), tile);
            }
        }
    }
    public void SetLine((int x, int y) pointA, (int x, int y) pointB, Tile tile)
    {
        var (x0, y0) = pointA;
        var (x1, y1) = pointB;
        var dx = Math.Abs(x1 - x0);
        var dy = Math.Abs(y1 - y0);
        var sx = x0 < x1 ? 1 : -1;
        var sy = y0 < y1 ? 1 : -1;
        var err = dx - dy;

        while (true)
        {
            SetTile((x0, y0), tile);

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
    /// <param name="position">The position of the top-left corner of the rectangular 
    /// area to create the box.</param>
    /// <param name="size">The size of the rectangular area to create the box.</param>
    /// <param name="tileFill">The tile to use for the filling of the box</param>
    /// <param name="borderTileId">The identifier of the tile to use for the 
    /// straight edges of the box.</param>
    /// <param name="cornerTileId">The identifier of the tile to use for the corners of the box.</param>
    /// <param name="borderTint">The color to tint the border tiles.</param>
    public void SetBox(
        (int x, int y) position,
        (int width, int height) size,
        Tile tileFill,
        int cornerTileId,
        int borderTileId,
        uint borderTint = uint.MaxValue)
    {
        var (x, y) = position;
        var (w, h) = size;

        if (w <= 0 || h <= 0)
            return;

        if (w == 1 || h == 1)
        {
            SetRectangle(position, size, tileFill);
            return;
        }

        SetTile(position, new(cornerTileId, borderTint));
        SetRectangle((x + 1, y), (w - 2, 1), new(borderTileId, borderTint));
        SetTile((x + w - 1, y), new(cornerTileId, borderTint, 1));

        if (h != 2)
        {
            SetRectangle((x, y + 1), (1, h - 2), new(borderTileId, borderTint, 3));
            SetRectangle((x + 1, y + 1), (w - 2, h - 2), tileFill);
            SetRectangle((x + w - 1, y + 1), (1, h - 2), new(borderTileId, borderTint, 1));
        }

        SetTile((x, y + h - 1), new(cornerTileId, borderTint, 3));
        SetTile((x + w - 1, y + h - 1), new(cornerTileId, borderTint, 2));
        SetRectangle((x + 1, y + h - 1), (w - 2, 1), new(borderTileId, borderTint, 2));
    }
    /// <summary>
    /// Sets the tiles in a rectangular area of the tilemap to create a vertical or horizontal bar.
    /// </summary>
    /// <param name="position">The position of the top-left corner of the rectangular area 
    /// to create the bar.</param>
    /// <param name="tileIdEdge">The identifier of the tile to use for the edges of the bar.</param>
    /// <param name="tileId">The identifier of the tile to use for the 
    /// straight part of the bar.</param>
    /// <param name="tint">The color to tint the bar tiles.</param>
    /// <param name="size">The length of the bar in tiles.</param>
    /// <param name="isVertical">Whether the bar should be vertical or horizontal.</param>
    public void SetBar(
        (int x, int y) position,
        int tileIdEdge,
        int tileId,
        uint tint = uint.MaxValue,
        int size = 5,
        bool isVertical = false)
    {
        var (x, y) = position;
        var off = size == 1 ? 0 : 1;

        if (isVertical)
        {
            if (size > 1)
            {
                SetTile(position, new(tileIdEdge, tint, 1));
                SetTile((x, y + size - 1), new(tileIdEdge, tint, 3));
            }

            if (size != 2)
                SetRectangle((x, y + off), (1, size - 2), new(tileId, tint, 1));

            return;
        }

        if (size > 1)
        {
            SetTile(position, new(tileIdEdge, tint));
            SetTile((x + size - 1, y), new(tileIdEdge, tint, 2));
        }

        if (size != 2)
            SetRectangle((x + off, y), (size - 2, 1), new(tileId, tint));
    }

    /// <summary>
    /// Converts a screen pixelPosition to a world point on the tilemap.
    /// </summary>
    /// <param name="pixelPosition">The screen pixel position to convert.</param>
    /// <param name="windowSize">The size of the application window.</param>
    /// <param name="isAccountingForCamera">Whether or not to account for the camera's position.</param>
    /// <returns>The world point corresponding to the given screen 
    /// pixelPosition.</returns>
    public (float x, float y) PointFrom(
        (int x, int y) pixelPosition,
        (int width, int height) windowSize,
        bool isAccountingForCamera = true)
    {
        var x = Map(pixelPosition.x, 0, windowSize.width, 0, Size.width);
        var y = Map(pixelPosition.y, 0, windowSize.height, 0, Size.height);

        if (isAccountingForCamera == false)
            return (x, y);

        x += Camera.x;
        y += Camera.y;

        return (x, y);
    }

    public void ConfigureText(
        int lowercase = Tile.LOWERCASE_A,
        int uppercase = Tile.UPPERCASE_A,
        int numbers = Tile.NUMBER_0)
    {
        textIdLowercase = lowercase;
        textIdUppercase = uppercase;
        textIdNumbers = numbers;
    }
    public void ConfigureText(string symbols, int startId)
    {
        for (var i = 0; i < symbols.Length; i++)
            symbolMap[symbols[i]] = startId + i;
    }

    public bool IsContaining(
        (int x, int y) position,
        (int width, int height) outsideMargin = default)
    {
        var (mx, my) = outsideMargin;
        return position.x >= -mx && position.y >= -my &&
               position.x <= Size.width - 1 + mx && position.y <= Size.height - 1 + my;
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
        return Copy(tilemap.data);
    }
    /// <summary>
    /// Implicitly converts a tilemap object to a 2D array of tile bundles.
    /// </summary>
    /// <param name="tilemap">The tilemap object to convert.</param>
    /// <returns>A new 2D array of tile bundles containing the tiles from the tilemap object.</returns>
    public static implicit operator (int tile, uint tint, sbyte angle, bool isFlippedHorizontally,
        bool isFlippedVertically)[,](Tilemap tilemap)
    {
        var data = tilemap.data;
        var result = new (int, uint, sbyte, bool, bool)[data.GetLength(0), data.GetLength(1)];
        for (var j = 0; j < data.GetLength(1); j++)
            for (var i = 0; i < data.GetLength(0); i++)
                result[i, j] = data[i, j];

        return result;
    }

    /// <returns>
    /// A 2D array of the bundle tuples of the tiles in the tilemap.</returns>
    public (int tile, uint tint, sbyte angle, bool isFlippedHorizontally, bool isFlippedVertically)
        [,] ToBundle()
    {
        return this;
    }
    public byte[] ToBytes()
    {
        var result = new List<byte>();
        var (w, h) = Size;
        result.AddRange(BitConverter.GetBytes(Camera.x));
        result.AddRange(BitConverter.GetBytes(Camera.y));
        result.AddRange(BitConverter.GetBytes(Camera.width));
        result.AddRange(BitConverter.GetBytes(Camera.height));
        result.AddRange(BitConverter.GetBytes(w));
        result.AddRange(BitConverter.GetBytes(h));

        for (var i = 0; i < h; i++)
            for (var j = 0; j < w; j++)
                result.AddRange(TileAt((j, i)).ToBytes());

        return Compress(result.ToArray());
    }

#region Backend
    // save format
    // [amount of bytes]		- data
    // --------------------------------
    // [4]						- camera x
    // [4]						- camera y
    // [4]						- camera width
    // [4]						- camera height
    // [4]						- width
    // [4]						- height
    // [width * height]			- tile bundles array

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

        { '▕', 432 },
    };

    private readonly Tile[,] data;

    public static (int, int) FromIndex(int index, (int width, int height) size)
    {
        index = index < 0 ? 0 : index;
        index = index > size.width * size.height - 1 ? size.width * size.height - 1 : index;

        return (index % size.width, index / size.width);
    }
    private bool IndicesAreValid((int, int) indices)
    {
        return indices is { Item1: >= 0, Item2: >= 0 } &&
               indices.Item1 < Size.width && indices.Item2 < Size.height;
    }
    private static string PadLeftAndRight(string text, int length)
    {
        var spaces = length - text.Length;
        var padLeft = spaces / 2 + text.Length;
        return text.PadLeft(padLeft).PadRight(length);
    }
    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
    private static T[,] Copy<T>(T[,] array)
    {
        var copy = new T[array.GetLength(0), array.GetLength(1)];
        Array.Copy(array, copy, array.Length);
        return copy;
    }
    private static float Random(
        float rangeA,
        float rangeB,
        float precision = 0,
        float seed = float.NaN)
    {
        if (rangeA > rangeB)
            (rangeA, rangeB) = (rangeB, rangeA);

        precision = (int)Limit(precision, 0, 5);
        precision = MathF.Pow(10, precision);

        rangeA *= precision;
        rangeB *= precision;

        var s = new Random(float.IsNaN(seed) ? Guid.NewGuid().GetHashCode() : (int)seed);
        var randInt = s.Next((int)rangeA, Limit((int)rangeB, (int)rangeA, (int)rangeB) + 1);

        return randInt / (precision);
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

    private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}