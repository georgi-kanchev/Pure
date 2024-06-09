namespace Pure.Engine.Window;

using System.Numerics;
using System.Diagnostics.CodeAnalysis;

public class Layer
{
    public string AtlasPath
    {
        get => atlasPath;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                atlasPath = "default";
                Init();
                return;
            }

            if (tilesets.TryGetValue(value, out var tileset))
            {
                atlasPath = value;
                tilesetPixelSize = tileset.Size;
                return;
            }

            try
            {
                tilesets[value] = new(value) { Repeated = true };
                atlasPath = value;
                tilesetPixelSize = tilesets[value].Size;
            }
            catch (Exception)
            {
                atlasPath = "default";
                Init();
            }
        }
    }
    public int TileIdFull { get; set; }
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
    public (int width, int height) AtlasSize
    {
        get
        {
            var (w, h) = ((int)tilesetPixelSize.X, (int)tilesetPixelSize.Y);
            var (tw, th) = TileSize;
            var (gw, gh) = TileGap;
            return (w / (tw + gw), h / (th + gh));
        }
    }
    public bool IsOverflowing { get; set; }
    public bool IsHovered
    {
        get => IsOverlapping(PixelToWorld(Mouse.CursorPosition));
    }

    public float Gamma
    {
        get => gamma;
        set
        {
            gamma = value;
            shader?.SetUniform("gamma", gamma);
        }
    }
    public float Saturation
    {
        get => saturation;
        set
        {
            saturation = value;
            shader?.SetUniform("saturation", saturation);
        }
    }
    public float Contrast
    {
        get => contrast;
        set
        {
            contrast = value;
            shader?.SetUniform("contrast", contrast);
        }
    }
    public float Brightness
    {
        get => brightness;
        set
        {
            brightness = value;
            shader?.SetUniform("brightness", brightness);
        }
    }
    public uint Tint
    {
        get => currTint;
        set
        {
            currTint = value;
            shader?.SetUniform("tint", new Color(currTint));
        }
    }
    public uint OverlayColor
    {
        get => overlayColor;
        set
        {
            overlayColor = value;
            shader?.SetUniform("overlay", new Color(overlayColor));
        }
    }

    public (float x, float y) Offset { get; set; }
    public float Zoom
    {
        get => zoom;
        set => zoom = Math.Clamp(value, 0, 1000f);
    }

    public Layer((int width, int height) tilemapSize = default)
    {
        Init();
        TilemapSize = tilemapSize;
        Zoom = 1f;
        atlasPath = string.Empty;
        AtlasPath = string.Empty;
        verts = new(PrimitiveType.Quads);
        shader = new EffectColors().Shader;
        Tint = uint.MaxValue;
        Gamma = 1f;
        Saturation = 1f;
        Contrast = 1f;
        Brightness = 1f;
    }

    ~Layer()
    {
        verts.Dispose();
    }

    public void DrawCursor(int tileId = 546, uint tint = 3789677055)
    {
        if (Mouse.isOverWindow == false)
            return;

        var (offX, offY) = cursorOffsets[(int)Mouse.CursorCurrent];
        var angle = default(sbyte);

        if (Mouse.CursorCurrent == Mouse.Cursor.ResizeVertical)
        {
            tileId--;
            angle = 1;
        }
        else if (Mouse.CursorCurrent == Mouse.Cursor.ResizeTopLeftBottomRight)
        {
            tileId--;
            angle = 1;
        }
        else if ((int)Mouse.CursorCurrent >= (int)Mouse.Cursor.ResizeBottomLeftTopRight)
            tileId -= 2;

        (int id, uint tint, sbyte ang, bool h, bool v) tile = default;
        tile.id = tileId + (int)Mouse.CursorCurrent;
        tile.tint = tint;
        tile.ang = angle;

        var (x, y) = PixelToWorld(Mouse.CursorPosition);
        DrawTiles((x - offX, y - offY), tile);
    }
    public void DrawPoints(params (float x, float y, uint color)[]? points)
    {
        for (var i = 0; i < points?.Length; i++)
        {
            var p = points[i];
            QueueRectangle((p.x, p.y), (1f / TileSize.width, 1f / TileSize.height), p.color);
        }
    }
    public void DrawRectangles(params (float x, float y, float width, float height, uint color)[]? rectangles)
    {
        for (var i = 0; i < rectangles?.Length; i++)
        {
            var (x, y, width, height, color) = rectangles[i];
            QueueRectangle((x, y), (width, height), color);
        }
    }
    public void DrawLines(params (float ax, float ay, float bx, float by, uint color)[]? lines)
    {
        if (lines == null || lines.Length == 0)
            return;

        foreach (var line in lines)
            QueueLine((line.ax, line.ay), (line.bx, line.by), line.color);
    }
    public void DrawLines(params (float x, float y, uint color)[]? points)
    {
        if (points == null || points.Length == 0)
            return;

        if (points.Length == 1)
        {
            DrawPoints(points[0]);
            return;
        }

        for (var i = 1; i < points.Length; i++)
        {
            var a = points[i - 1];
            var b = points[i];
            QueueLine((a.x, a.y), (b.x, b.y), a.color);
        }
    }
    public void DrawTiles(
        (float x, float y) position, (int id, uint tint, sbyte turns, bool isMirrored,
            bool isFlipped) tile, (int width, int height) groupSize = default, bool isSameTile = default)
    {
        var (id, tint, angle, flipH, flipV) = tile;
        var (w, h) = groupSize;
        w = w == 0 ? 1 : w;
        h = h == 0 ? 1 : h;

        var tiles = new int[Math.Abs(w), Math.Abs(h)];
        var (tileX, tileY) = IndexToCoords(id);
        var (x, y) = position;
        var (tw, th) = TileSize;

        for (var i = 0; i < Math.Abs(h); i++)
            for (var j = 0; j < Math.Abs(w); j++)
                tiles[j, i] = CoordsToIndex(tileX + (isSameTile ? 0 : j), tileY + (isSameTile ? 0 : i));

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
                var (texTl, texTr, texBr, texBl) = GetTexCoords(tiles[j, i], (1, 1));
                var (tx, ty) = ((x + j) * tw, (y + i) * th);
                var c = new Color(tint);
                var tl = new Vector2f((int)tx, (int)ty);
                var br = new Vector2f((int)(tx + tw), (int)(ty + th));

                if (angle is 1 or 3)
                    br = new((int)(tx + th), (int)(ty + tw));

                var tr = new Vector2f((int)br.X, (int)tl.Y);
                var bl = new Vector2f((int)tl.X, (int)br.Y);
                var rotated = GetRotatedPoints((sbyte)-angle, texTl, texTr, texBr, texBl);
                texTl = rotated[0];
                texTr = rotated[1];
                texBr = rotated[2];
                texBl = rotated[3];

                if (groupSize.width < 0)
                {
                    (texTl, texTr) = (texTr, texTl);
                    (texBl, texBr) = (texBr, texBl);
                }

                if (groupSize.height < 0)
                {
                    (texTl, texBl) = (texBl, texTl);
                    (texTr, texBr) = (texBr, texTr);
                }

                verts.Append(TryCropVertex(new(tl, c, texTl)));
                verts.Append(TryCropVertex(new(tr, c, texTr)));
                verts.Append(TryCropVertex(new(br, c, texBr)));
                verts.Append(TryCropVertex(new(bl, c, texBl)));
            }
    }
    public void DrawTilemap((int id, uint tint, sbyte turns, bool isMirrored, bool isFlipped)[,] tilemap)
    {
        var (cellCountW, cellCountH) = (tilemap.GetLength(0), tilemap.GetLength(1));
        var (tw, th) = TileSize;

        for (var y = 0; y < cellCountH; y++)
            for (var x = 0; x < cellCountW; x++)
            {
                var (id, tint, angle, isFlippedHorizontally, isFlippedVertically) = tilemap[x, y];

                if (id == default)
                    continue;

                var color = new Color(tint);
                var tl = new Vector2f(x * tw, y * th);
                var tr = new Vector2f((x + 1) * tw, y * th);
                var br = new Vector2f((x + 1) * tw, (y + 1) * th);
                var bl = new Vector2f(x * tw, (y + 1) * th);
                var (texTl, texTr, texBr, texBl) = GetTexCoords(id, (1, 1));
                var rotated = GetRotatedPoints(angle, tl, tr, br, bl);
                var (flipX, flipY) = (isFlippedHorizontally, isFlippedVertically);

                if (flipX)
                {
                    (texTl, texTr) = (texTr, texTl);
                    (texBl, texBr) = (texBr, texBl);
                }

                if (flipY)
                {
                    (texTl, texBl) = (texBl, texTl);
                    (texTr, texBr) = (texBr, texTr);
                }

                tl = rotated[0];
                tr = rotated[1];
                br = rotated[2];
                bl = rotated[3];

                tl = new((int)tl.X, (int)tl.Y);
                tr = new((int)tr.X, (int)tr.Y);
                br = new((int)br.X, (int)br.Y);
                bl = new((int)bl.X, (int)bl.Y);

                verts.Append(new(tl, color, texTl));
                verts.Append(new(tr, color, texTr));
                verts.Append(new(br, color, texBr));
                verts.Append(new(bl, color, texBl));
            }
    }

    public bool IsOverlapping((float x, float y) position)
    {
        return position is { x: >= 0, y: >= 0 } &&
               position.x <= TilemapSize.width &&
               position.y <= TilemapSize.height;
    }
    public (float x, float y) PixelToWorld((int x, int y) pixelPosition)
    {
        if (Window.window == null || Window.renderTexture == null)
            return (float.NaN, float.NaN);

        var (px, py) = (pixelPosition.x * 1f, pixelPosition.y * 1f);
        var (vw, vh) = Window.renderTextureViewSize;
        var (cw, ch) = TilemapSize;
        var (tw, th) = TileSize;
        var (ox, oy) = Offset;
        var (mw, mh) = (cw * tw, ch * th);
        var (ww, wh, ow, oh) = Window.GetRenderOffset();

        px -= ow;
        py -= oh;

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

    public void ReplaceColor(uint oldColor, uint newColor)
    {
        var pair = (oldColor, newColor);
        if (oldColor == newColor || replaceColors.Contains(pair))
        {
            replaceColors.Remove(pair);
            return;
        }

        replaceColors.Add(pair);

        var index = replaceColors.Count - 1;
        shader?.SetUniform("replaceColorsCount", replaceColors.Count);
        shader?.SetUniform($"replaceColorsOld[{index}]", new Color(oldColor));
        shader?.SetUniform($"replaceColorsNew[{index}]", new Color(newColor));
    }
    public void ResetToDefaults()
    {
        Init();
        Zoom = 1f;
        TileGap = (0, 0);
        atlasPath = string.Empty;
        AtlasPath = string.Empty;
        TileIdFull = 10;
    }

    public static void SaveDefaultGraphics(string filePath)
    {
        tilesets["default"].CopyToImage().SaveToFile(filePath);
    }

#region Backend
    internal readonly Shader? shader;
    private readonly List<(uint, uint)> replaceColors = new();
    internal static readonly Dictionary<string, Texture> tilesets = new();
    internal readonly VertexArray verts;
    private static readonly List<(float, float)> cursorOffsets = new()
    {
        (0.0f, 0.0f), (0.0f, 0.0f), (0.4f, 0.4f), (0.4f, 0.4f), (0.3f, 0.0f), (0.4f, 0.4f),
        (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f)
    };

    internal Vector2u tilesetPixelSize;
    internal (int w, int h) TilemapPixelSize
    {
        get
        {
            var (mw, mh) = TilemapSize;
            var (tw, th) = TileSize;
            return (mw * tw, mh * th);
        }
    }

    static Layer()
    {
        //var str = DefaultGraphics.PngToBase64String("/home/gojur/code/Pure/Examples/bin/Debug/net6.0/graphics.png");
        //var str = DefaultGraphics.PngToBase64String("graphics.png");

        tilesets["default"] = DefaultGraphics.CreateTexture();
        //SaveDefaultGraphics("graphics.png");
    }

    private string atlasPath;
    private (int width, int height) tileGap, tileSize, tilemapSize;
    private uint currTint = uint.MaxValue, overlayColor;
    private float zoom, gamma, saturation, contrast, brightness;

    [MemberNotNull(nameof(TileIdFull), nameof(TileSize), nameof(tilesetPixelSize))]
    private void Init()
    {
        TileIdFull = 10;
        TileSize = (8, 8);
        tilesetPixelSize = new(208, 208);
    }

    private void QueueLine((float x, float y) a, (float x, float y) b, uint tint)
    {
        if (IsOverlapping(a) == false && IsOverlapping(b) == false) // fully outside?
            return;

        if (IsOverlapping(a) ^ IsOverlapping(b)) // halfway outside?
        {
            var line = TryCropLine(a, b);
            a = (line.ax, line.ay);
            b = (line.bx, line.by);
        }

        var (tw, th) = TileSize;
        a.x *= tw;
        a.y *= th;
        b.x *= tw;
        b.y *= th;

        const float THICKNESS = 0.5f;
        var color = new Color(tint);
        var dir = new Vector2(b.x - a.x, b.y - a.y);
        var length = dir.Length() / 2f;
        var center = new Vector2((a.x + b.x) / 2, (a.y + b.y) / 2);
        var tl = new Vector2(-length, -THICKNESS);
        var tr = new Vector2(length, -THICKNESS);
        var br = new Vector2(length, THICKNESS);
        var bl = new Vector2(-length, THICKNESS);
        var (texTl, texTr, texBr, texBl) = GetTexCoords(TileIdFull, (1, 1));

        var m = Matrix3x2.Identity;
        m *= Matrix3x2.CreateRotation((float)Math.Atan2(dir.Y, dir.X));
        m *= Matrix3x2.CreateTranslation(center);

        tl = Vector2.Transform(tl, m);
        tr = Vector2.Transform(tr, m);
        br = Vector2.Transform(br, m);
        bl = Vector2.Transform(bl, m);

        verts.Append(new(new(tl.X, tl.Y), color, texTl));
        verts.Append(new(new(tr.X, tr.Y), color, texTr));
        verts.Append(new(new(br.X, br.Y), color, texBr));
        verts.Append(new(new(bl.X, bl.Y), color, texBl));
    }
    private void QueueRectangle((float x, float y) position, (float w, float h) size, uint tint)
    {
        var (tw, th) = TileSize;
        var (w, h) = size;

        if (IsOverlapping(position) == false && IsOverlapping((position.x + w, position.y + h)) == false)
            return;

        var (x, y) = (position.x * tw, position.y * th);
        var color = new Color(tint);
        var (texTl, texTr, texBr, texBl) = GetTexCoords(TileIdFull, (1, 1));
        var tl = new Vector2f((int)x, (int)y);
        var br = new Vector2f((int)(x + tw * w), (int)(y + th * h));
        var tr = new Vector2f((int)br.X, (int)tl.Y);
        var bl = new Vector2f((int)tl.X, (int)br.Y);

        verts.Append(TryCropVertex(new(tl, color, texTl), true));
        verts.Append(TryCropVertex(new(tr, color, texTr), true));
        verts.Append(TryCropVertex(new(br, color, texBr), true));
        verts.Append(TryCropVertex(new(bl, color, texBl), true));
    }
    private Vertex TryCropVertex(Vertex vertex, bool skipTexCoords = false)
    {
        if (IsOverflowing)
            return vertex;

        var px = vertex.Position.X;
        var py = vertex.Position.Y;
        var x = Math.Clamp(vertex.Position.X, 0, TilemapPixelSize.w);
        var y = Math.Clamp(vertex.Position.Y, 0, TilemapPixelSize.h);

        vertex.Position = new(x, y);

        if (skipTexCoords)
            return vertex;

        var dx = x - px;
        var dy = y - py;
        var tx = vertex.TexCoords.X + dx;
        var ty = vertex.TexCoords.Y + dy;
        vertex.TexCoords = new(tx, ty);

        return vertex;
    }
    private (float ax, float ay, float bx, float by) TryCropLine((float x, float y) a, (float x, float y) b)
    {
        var (w, h) = tilemapSize;
        var newA = (a.x, a.y);
        var newB = (b.x, b.y);
        var isOutsideA = IsOverlapping(a) == false;
        var isOutsideB = IsOverlapping(b) == false;
        var crossT = LinesCrossPoint(a.x, a.y, b.x, b.y, 0, 0, w, 0);
        var crossR = LinesCrossPoint(a.x, a.y, b.x, b.y, w, 0, w, h);
        var crossB = LinesCrossPoint(a.x, a.y, b.x, b.y, w, h, 0, h);
        var crossL = LinesCrossPoint(a.x, a.y, b.x, b.y, 0, h, 0, 0);

        TryCropSide(crossT);
        TryCropSide(crossR);
        TryCropSide(crossB);
        TryCropSide(crossL);

        return (newA.x, newA.y, newB.x, newB.y);

        void TryCropSide((float x, float y) sideCrossPoint)
        {
            if (float.IsNaN(sideCrossPoint.x) || float.IsNaN(sideCrossPoint.y))
                return;

            newA = isOutsideA ? sideCrossPoint : a;
            newB = isOutsideB ? sideCrossPoint : b;
        }
    }

    private (Vector2f tl, Vector2f tr, Vector2f br, Vector2f bl) GetTexCoords(int tileId, (int w, int h) size)
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
        var (tw, th) = AtlasSize;
        index = index < 0 ? 0 : index;
        index = index > tw * th - 1 ? tw * th - 1 : index;

        return (index % tw, index / tw);
    }
    private int CoordsToIndex(int x, int y)
    {
        return y * AtlasSize.width + x;
    }
    private static int Wrap(int number, int targetNumber)
    {
        return (number % targetNumber + targetNumber) % targetNumber;
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
    private static Vector2f[] GetRotatedPoints(sbyte turns, params Vector2f[] points)
    {
        Shift(points, Wrap(-turns, 4));
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
    private static (float x, float y) LinesCrossPoint(float ax1, float ay1, float bx1, float by1, float ax2, float ay2, float bx2, float by2)
    {
        var dx1 = bx1 - ax1;
        var dy1 = by1 - ay1;
        var dx2 = bx2 - ax2;
        var dy2 = by2 - ay2;
        var det = dx1 * dy2 - dy1 * dx2;

        if (det == 0)
            return (float.NaN, float.NaN);

        var s = ((ay1 - ay2) * dx2 - (ax1 - ax2) * dy2) / det;
        var t = ((ay1 - ay2) * dx1 - (ax1 - ax2) * dy1) / det;

        if (s is < 0 or > 1 || t is < 0 or > 1)
            return (float.NaN, float.NaN);

        var intersectionX = ax1 + s * dx1;
        var intersectionY = ay1 + s * dy1;
        return (intersectionX, intersectionY);
    }
#endregion
}