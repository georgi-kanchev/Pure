namespace Pure.Engine.Window;

public class Layer
{
    public string TilesetPath
    {
        get => tilesetPath;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                tilesetPath = "default";
                return;
            }

            if (tilesets.ContainsKey(value))
            {
                tilesetPath = value;
                return;
            }

            try
            {
                tilesets[value] = new(value) { Repeated = true };
                tilesetPath = value;

                var sz = tilesets[value].Size;
                var (tw, th) = TileSize;
                var (gw, gh) = TileSize;
                tilesetSize = ((int)sz.X / (tw + gw), (int)sz.Y / (th + gh));
            }
            catch (Exception)
            {
                tilesetPath = "default";
            }
        }
    }
    public (int empty, int full) TileIds
    {
        get;
        set;
    }
    public (int width, int height) TileGap
    {
        get => tileGap;
        set => tileGap = (Math.Clamp(value.width, 0, 512), Math.Clamp(value.height, 0, 512));
    }
    public (int width, int height) TileSize
    {
        get => tileSize;
        set => tileSize = (Math.Clamp(value.width, 0, 512), Math.Clamp(value.height, 0, 512));
    }
    public (int width, int height) TilemapSize
    {
        get => tilemapSize;
        set => tilemapSize = (Math.Clamp(value.width, 1, 1000), Math.Clamp(value.height, 1, 1000));
    }

    public (float x, float y) Offset
    {
        get;
        set;
    }
    public float Zoom
    {
        get => zoom;
        set => zoom = Math.Clamp(value, 0, 1000f);
    }

    public Layer()
    {
        Window.TryNoWindowException();

        Zoom = 1f;

        TileIds = (0, 10);
        TileSize = (8, 8);
        TilemapSize = (100, 100);

        tilesetPath = string.Empty;
        TilesetPath = string.Empty;
        tilesetSize = (26, 26);

        verts = new(PrimitiveType.Quads);
    }

    public void Clear()
    {
        verts.Clear();
    }

    public void DrawCursor(int tileId = 442, uint tint = uint.MaxValue)
    {
        var (offX, offY) = cursorOffsets[(int)Mouse.CursorCurrent];
        var ang = default(sbyte);

        if (Mouse.CursorCurrent == Mouse.Cursor.ResizeVertical)
        {
            tileId--;
            ang = 1;
        }
        else if (Mouse.CursorCurrent == Mouse.Cursor.ResizeDiagonal1)
        {
            tileId--;
            ang = 1;
        }
        else if ((int)Mouse.CursorCurrent >= (int)Mouse.Cursor.ResizeDiagonal2)
        {
            tileId -= 2;
        }

        (int id, uint tint, sbyte ang, bool h, bool v) tile = default;
        tile.id = tileId + (int)Mouse.CursorCurrent;
        tile.tint = tint;
        tile.ang = ang;

        var (x, y) = PixelToWorld(Mouse.CursorPosition);
        DrawTile((x - offX, y - offY), tile);
    }
    public void DrawPoints(params (float x, float y, uint color)[]? points)
    {
        for (var i = 0; i < points?.Length; i++)
        {
            var p = points[i];
            QueueRectangle((p.x, p.y), (1f / TileSize.width, 1f / TileSize.height), p.color);
        }
    }
    public void DrawRectangles(
        params (float x, float y, float width, float height, uint color)[]? rectangles)
    {
        for (var i = 0; i < rectangles?.Length; i++)
        {
            var (x, y, width, height, color) = rectangles[i];
            QueueRectangle((x, y), (width, height), color);
        }
    }
    public void DrawLines(params (float ax, float ay, float bx, float by, uint color)[]? lines)
    {
        for (var i = 0; i < lines?.Length; i++)
        {
            var l = lines[i];
            var (tw, th) = TileSize;
            var (ax, ay) = (l.ax, l.ay);
            var (bx, by) = (l.bx, l.by);
            var (dx, dy) = (MathF.Abs(bx - ax), -MathF.Abs(by - ay));
            var (stepX, stepY) = (1f / tw * 0.999f, 1f / th * 0.999f);
            var sx = ax < bx ? stepX : -stepY;
            var sy = ay < by ? stepX : -stepY;
            var err = dx + dy;
            var steps = (int)MathF.Max(dx / stepX, -dy / stepY);

            for (var t = 0; t <= steps; t++)
            {
                DrawPoints((ax, ay, l.color));

                if (IsWithin(ax, bx, stepX) && IsWithin(ay, by, stepY))
                    break;

                var e2 = 2f * err;

                if (e2 > dy)
                {
                    err += dy;
                    ax += sx;
                }

                if (e2 < dx == false)
                    continue;

                err += dx;
                ay += sy;
            }
        }
    }
    public void DrawTile(
        (float x, float y) position,
        (int id, uint tint, sbyte angle, bool isFlippedHorizontally, bool isFlippedVertically) tile,
        (int width, int height) size = default)
    {
        var (id, tint, angle, flipH, flipV) = tile;
        var (w, h) = size;
        w = w == 0 ? 1 : w;
        h = h == 0 ? 1 : h;

        var tiles = new int[Math.Abs(w), Math.Abs(h)];
        var (tileX, tileY) = IndexToCoords(id);
        var (x, y) = position;
        var (tw, th) = TileSize;

        for (var j = 0; j < Math.Abs(h); j++)
            for (var i = 0; i < Math.Abs(w); i++)
                tiles[i, j] = CoordsToIndex(tileX + i, tileY + j);

        if (w < 0)
            FlipVertically(tiles);
        if (h < 0)
            FlipHorizontally(tiles);
        if (flipH)
            FlipVertically(tiles);
        if (flipV)
            FlipHorizontally(tiles);

        w = Math.Abs(w);
        h = Math.Abs(h);

        for (var i = 0; i < h; i++)
            for (var j = 0; j < w; j++)
            {
                var (ttl, ttr, tbr, tbl) = GetTexCoords(id, (w, h));
                var (tx, ty) = ((x + j) * tw, (y + i) * th);
                var c = new Color(tint);
                var tl = new Vector2f((int)tx, (int)ty);
                var br = new Vector2f((int)(tx + tw * w), (int)(ty + th * h));

                if (angle is 1 or 3)
                    br = new((int)(tx + th * h), (int)(ty + tw * w));

                var tr = new Vector2f(br.X, tl.Y);
                var bl = new Vector2f(tl.X, br.Y);
                var rotated = GetRotatedPoints((sbyte)-angle, ttl, ttr, tbr, tbl);
                ttl = rotated[0];
                ttr = rotated[1];
                tbr = rotated[2];
                tbl = rotated[3];

                if (size.width < 0)
                {
                    (ttl, ttr) = (ttr, ttl);
                    (tbl, tbr) = (tbr, tbl);
                }

                if (size.height < 0)
                {
                    (ttl, tbl) = (tbl, ttl);
                    (ttr, tbr) = (tbr, ttr);
                }

                verts.Append(new(tl, c, ttl));
                verts.Append(new(tr, c, ttr));
                verts.Append(new(br, c, tbr));
                verts.Append(new(bl, c, tbl));
            }
    }
    public void DrawTilemap(
        (int id, uint tint, sbyte angle, bool isFlippedHorizontally,
            bool isFlippedVertically)[,] tilemap)
    {
        var (cellCountW, cellCountH) = (tilemap.GetLength(0), tilemap.GetLength(1));
        var (tw, th) = TileSize;

        for (var y = 0; y < cellCountH; y++)
            for (var x = 0; x < cellCountW; x++)
            {
                var (id, tint, angle, isFlippedHorizontally, isFlippedVertically) = tilemap[x, y];

                if (id == TileIds.empty)
                    continue;

                var color = new Color(tint);
                var tl = new Vector2f(x * tw, y * th);
                var tr = new Vector2f((x + 1) * tw, y * th);
                var br = new Vector2f((x + 1) * tw, (y + 1) * th);
                var bl = new Vector2f(x * tw, (y + 1) * th);
                var (ttl, ttr, tbr, tbl) = GetTexCoords(id, (1, 1));
                var rotated = GetRotatedPoints(angle, tl, tr, br, bl);
                var (flipX, flipY) = (isFlippedHorizontally, isFlippedVertically);

                if (flipX)
                {
                    (ttl, ttr) = (ttr, ttl);
                    (tbl, tbr) = (tbr, tbl);
                }

                if (flipY)
                {
                    (ttl, tbl) = (tbl, ttl);
                    (ttr, tbr) = (tbr, ttr);
                }

                tl = rotated[0];
                tr = rotated[1];
                br = rotated[2];
                bl = rotated[3];

                tl = new((int)tl.X, (int)tl.Y);
                tr = new((int)tr.X, (int)tr.Y);
                br = new((int)br.X, (int)br.Y);
                bl = new((int)bl.X, (int)bl.Y);

                verts.Append(new(tl, color, ttl));
                verts.Append(new(tr, color, ttr));
                verts.Append(new(br, color, tbr));
                verts.Append(new(bl, color, tbl));
            }
    }

    public (float x, float y) PixelToWorld((int x, int y) pixelPosition)
    {
        Window.TryNoWindowException();

        var (px, py) = (pixelPosition.x * 1f, pixelPosition.y * 1f);
        var (ww, wh) = (Window.Size.width, Window.Size.height);
        var (vw, vh) = Window.renderTextureViewSize;
        var (cw, ch) = TilemapSize;
        var (tw, th) = TileSize;
        var (ox, oy) = Offset;
        var (mw, mh) = (cw * tw, ch * th);

        ox /= mw;
        oy /= mh;

        px -= ww / 2f;
        py -= wh / 2f;

        var x = Map(px, 0, ww, 0, cw);
        var y = Map(py, 0, wh, 0, ch);

        x *= vw / Zoom / mw;
        y *= vh / Zoom / mh;

        x += cw / 2f;
        y += ch / 2f;

        x -= ox * cw / Zoom;
        y -= oy * ch / Zoom;

        return (x, y);
    }

#region Backend
    internal static readonly Dictionary<string, Texture> tilesets = new();
    internal readonly VertexArray verts;
    private static readonly List<(float, float)> cursorOffsets = new()
    {
        (0.0f, 0.0f), (0.0f, 0.0f), (0.4f, 0.4f), (0.4f, 0.4f), (0.3f, 0.0f), (0.4f, 0.4f),
        (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f)
    };

    internal (int w, int h) TilemapPixelSize
    {
        get
        {
            var (mw, mh) = TilemapSize;
            var (tw, th) = TileSize;
            return (mw * tw, mh * th);
        }
    }
    internal (int w, int h) tilesetSize;

    static Layer()
    {
        //var str = DefaultGraphics.PngToBase64String(
        //    "/home/gojur/code/Pure/Examples/bin/Debug/net6.0/graphics.png");
        //var str = DefaultGraphics.PNGToBase64String("graphics.png");

        tilesets["default"] = DefaultGraphics.CreateTexture();
    }

    private string tilesetPath;
    private (int width, int height) tileGap;
    private (int width, int height) tileSize;
    private float zoom;
    private (int width, int height) tilemapSize;

    private void QueueRectangle((float x, float y) position, (float w, float h) size, uint tint)
    {
        var (tw, th) = TileSize;
        var (w, h) = size;
        var (x, y) = (position.x * tw, position.y * th);
        var color = new Color(tint);
        var (ttl, ttr, tbr, tbl) = GetTexCoords(TileIds.full, (1, 1));
        var tl = new Vector2f((int)x, (int)y);
        var br = new Vector2f((int)x + tw * w, (int)y + th * h);
        var tr = new Vector2f(br.X, tl.Y);
        var bl = new Vector2f(tl.X, br.Y);

        verts.Append(new(tl, color, ttl));
        verts.Append(new(tr, color, ttr));
        verts.Append(new(br, color, tbr));
        verts.Append(new(bl, color, tbl));
    }

    private static bool IsWithin(float number, float targetNumber, float range)
    {
        return IsBetween(number, targetNumber - range, targetNumber + range);
    }
    private static bool IsBetween(float number, float rangeA, float rangeB)
    {
        if (rangeA > rangeB)
            (rangeA, rangeB) = (rangeB, rangeA);

        var l = rangeA <= number;
        var u = rangeB >= number;
        return l && u;
    }
    private (Vector2f tl, Vector2f tr, Vector2f br, Vector2f bl) GetTexCoords(
        int tileId,
        (int w, int h) size)
    {
        var (w, h) = size;
        var (tw, th) = TileSize;
        var (gw, gh) = TileGap;
        var (tx, ty) = IndexToCoords(tileId);
        var tl = new Vector2f(tx * (tw + gw), ty * (th + gh));
        var tr = tl + new Vector2f(tw * w, 0);
        var br = tl + new Vector2f(tw * w, th * h);
        var bl = tl + new Vector2f(0, th * h);
        return (tl, tr, br, bl);
    }
    private (int, int) IndexToCoords(int index)
    {
        var (tw, th) = tilesetSize;
        index = index < 0 ? 0 : index;
        index = index > tw * th - 1 ? tw * th - 1 : index;

        return (index % tw, index / tw);
    }
    private int CoordsToIndex(int x, int y)
    {
        return y * tilesetSize.w + x;
    }
    private static int Wrap(int number, int targetNumber)
    {
        return ((number % targetNumber) + targetNumber) % targetNumber;
    }
    private static void Shift<T>(IList<T> collection, int offset)
    {
        if (offset == default)
            return;

        if (offset < 0)
        {
            offset = Math.Abs(offset);
            for (var j = 0; j < offset; j++)
            {
                var temp = new T[collection.Count];
                for (var i = 0; i < collection.Count - 1; i++)
                    temp[i] = collection[i + 1];
                temp[^1] = collection[0];

                for (var i = 0; i < temp.Length; i++)
                    collection[i] = temp[i];
            }

            return;
        }

        for (var j = 0; j < offset; j++)
        {
            var tmp = new T[collection.Count];
            for (var i = 1; i < collection.Count; i++)
                tmp[i] = collection[i - 1];
            tmp[0] = collection[tmp.Length - 1];

            for (var i = 0; i < tmp.Length; i++)
                collection[i] = tmp[i];
        }
    }
    private static Vector2f[] GetRotatedPoints(sbyte angle, params Vector2f[] points)
    {
        Shift(points, Wrap(-angle, 4));
        return points;
    }
    private static void FlipHorizontally<T>(T[,] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);

        for (var i = 0; i < rows; i++)
            for (var j = 0; j < cols / 2; j++)
                (matrix[i, j], matrix[i, cols - j - 1]) = (matrix[i, cols - j - 1], matrix[i, j]);
    }
    private static void FlipVertically<T>(T[,] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);

        for (var i = 0; i < rows / 2; i++)
            for (var j = 0; j < cols; j++)
                (matrix[i, j], matrix[rows - i - 1, j]) = (matrix[rows - i - 1, j], matrix[i, j]);
    }
    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
#endregion
}