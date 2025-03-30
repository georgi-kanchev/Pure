namespace Pure.Engine.Window;

public enum TextAlign { Left, Center, Right }

public class LayerTiles
{
    public string AtlasPath
    {
        get => atlasPath;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                atlasPath = DEFAULT_GRAPHICS;
                Init();
                return;
            }

            if (LayerSprites.textures.TryGetValue(value, out var tileset))
            {
                atlasPath = value;
                tilesetPixelSize = tileset.Size;
                return;
            }

            try
            {
                LayerSprites.textures[value] = new(value) { Repeated = true };
                atlasPath = value;
                tilesetPixelSize = LayerSprites.textures[value].Size;
            }
            catch (Exception)
            {
                atlasPath = DEFAULT_GRAPHICS;
                Init();
            }
        }
    }
    public Count AtlasTileCount
    {
        get
        {
            var (w, h) = ((int)tilesetPixelSize.X, (int)tilesetPixelSize.Y);
            var tsz = (int)AtlasTileSize;
            var (rw, rh) = (w / (tsz + AtlasTileGap), h / (tsz + AtlasTileGap));
            return ((byte)rw, (byte)rh);
        }
    }
    public byte AtlasTileSize { get; set; }
    public byte AtlasTileGap { get; set; }
    public ushort AtlasTileIdFull { get; set; }

    public SizeI Size
    {
        get => size;
        set => size = (Math.Clamp(value.width, 1, 1000), Math.Clamp(value.height, 1, 1000));
    }
    public uint BackgroundColor { get; set; }

    public PointF PixelOffset { get; set; }
    public PointF Position
    {
        get
        {
            var (x, y) = PixelOffset;
            x /= AtlasTileSize;
            y /= AtlasTileSize;

            x -= Size.width / 2f;
            y -= Size.height / 2f;
            return (x, y);
        }
        set
        {
            var (x, y) = (-value.x, -value.y);
            x += Size.width / 2f;
            y += Size.height / 2f;

            x *= AtlasTileSize;
            y *= AtlasTileSize;
            PixelOffset = (x, y);
        }
    }
    public float Zoom
    {
        get => zoom;
        set => zoom = Math.Clamp(value, 0f, 1000f);
    }
    public float ZoomWindowFit
    {
        get
        {
            Window.TryCreate();

            var (mw, mh) = Monitor.Current.Size;
            var (ww, wh) = Window.Size;
            var (w, h) = (Size.width * AtlasTileSize, Size.height * AtlasTileSize);
            var (rw, rh) = ((float)mw / ww, (float)mh / wh);
            return Math.Min((float)ww / w * rw, (float)wh / h * rh) / Window.PixelScale;
        }
    }

    public bool IsHovered
    {
        get => IsOverlapping(PositionFromPixel(Mouse.CursorPosition));
    }
    public PointF MouseCursorPosition
    {
        get => PositionFromPixel(Mouse.CursorPosition);
    }
    public PointI MouseCursorCell
    {
        get => ((int)MouseCursorPosition.x, (int)MouseCursorPosition.y);
    }

    public Effect? Effect { get; set; }

    public LayerTiles(SizeI size = default, bool fitWindow = true)
    {
        Init();

        atlasPath = string.Empty;
        AtlasPath = string.Empty;

        Size = size;
        Zoom = 1f;

        if (fitWindow == false)
            return;

        Zoom = ZoomWindowFit;
        PixelOffset = (0, 0);
    }

    public void ToDefault()
    {
        Init();
        Zoom = 1f;
        AtlasTileGap = 0;
        atlasPath = string.Empty;
        AtlasPath = string.Empty;
        AtlasTileIdFull = 10;
    }

    public void Align(PointF alignment)
    {
        Window.TryCreate();

        var (w, h) = (Size.width * AtlasTileSize, Size.height * AtlasTileSize);
        var halfW = w / 2f;
        var halfH = h / 2f;
        var rendW = Window.rendTexViewSz.w / 2f / Zoom;
        var rendH = Window.rendTexViewSz.h / 2f / Zoom;
        var x = Window.Map(alignment.x, 0, 1, -rendW + halfW, rendW - halfW);
        var y = Window.Map(alignment.y, 0, 1, -rendH + halfH, rendH - halfH);
        PixelOffset = (x, y);
    }
    public void DragAndZoom(Mouse.Button dragButton = Mouse.Button.Middle, float zoomDelta = 0.05f, bool limit = true)
    {
        Window.TryCreate();

        if (Mouse.ScrollDelta != 0)
            Zoom *= Mouse.ScrollDelta > 0 ? 1f + zoomDelta : 1f - zoomDelta;
        if (dragButton.IsPressed())
            PixelOffset = (PixelOffset.x + Mouse.CursorDelta.x / Zoom, PixelOffset.y + Mouse.CursorDelta.y / Zoom);

        if (limit == false)
            return;

        var (w, h) = ((float)TilemapPixelSize.width, (float)TilemapPixelSize.height);
        var (x, y) = PixelOffset;
        PixelOffset = (Math.Clamp(x, -w / 2, w / 2), Math.Clamp(y, -h / 2, h / 2));
    }

    public void TextTileCrop(ushort symbolTileId, float newSymbolWidth)
    {
        textTileWidths[symbolTileId] = newSymbolWidth;
    }
    public void TextTilesAlign(AreaI area, TextAlign textAlign)
    {
        textAligns[area] = textAlign;
    }

    public void DrawMouseCursor(ushort tileId = 546, uint tint = 3789677055)
    {
        if (Mouse.IsCursorInWindow == false)
            return;

        var (offX, offY) = cursorOffsets[(int)Mouse.CursorCurrent];
        var pose = default(byte);

        if (Mouse.CursorCurrent is Mouse.Cursor.ResizeVertical or Mouse.Cursor.ResizeTopLeftBottomRight)
        {
            tileId--;
            pose = 1;
        }
        else if ((int)Mouse.CursorCurrent >= (int)Mouse.Cursor.ResizeBottomLeftTopRight)
            tileId -= 2;

        Tile tile = default;
        var cursor = (ushort)Mouse.CursorCurrent;
        tile.id = (ushort)(tileId + cursor);
        tile.tint = tint;
        tile.pose = pose;

        var (x, y) = PositionFromPixel(Mouse.CursorPosition);
        DrawTiles((x - offX, y - offY), tile);
    }
    public void DrawPoints(params PointColored[]? points)
    {
        for (var i = 0; i < points?.Length; i++)
        {
            var p = points[i];
            QueueRectangle((p.x, p.y), (1f / AtlasTileSize, 1f / AtlasTileSize), p.color);
        }
    }
    public void DrawRectangles(params AreaColored[]? rectangles)
    {
        for (var i = 0; i < rectangles?.Length; i++)
        {
            var (x, y, width, height, color) = rectangles[i];
            QueueRectangle((x, y), (width, height), color);
        }
    }
    public void DrawLine(params PointColored[]? points)
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
    public void DrawLines(params Line[]? lines)
    {
        if (lines == null || lines.Length == 0)
            return;

        foreach (var line in lines)
            QueueLine((line.ax, line.ay), (line.bx, line.by), line.color);
    }
    public void DrawTiles(PointF position, Tile tile, float scale = 1f, SizeI groupSize = default, bool sameTile = default)
    {
        if (verts == null)
            return;

        var (id, tint, pose) = tile;
        var (w, h) = groupSize;
        w = w == 0 ? 1 : w;
        h = h == 0 ? 1 : h;

        var tiles = new int[Math.Abs(w), Math.Abs(h)];
        var (tileX, tileY) = IndexToCoords(id);
        var (x, y) = position;
        var tsz = AtlasTileSize;
        var (flip, ang) = GetOrientation(pose);

        for (var i = 0; i < Math.Abs(h); i++)
            for (var j = 0; j < Math.Abs(w); j++)
                tiles[j, i] = CoordsToIndex(tileX + (sameTile ? 0 : j), tileY + (sameTile ? 0 : i));

        if (w < 0)
            FlipVertically(tiles);
        if (h < 0)
            FlipHorizontally(tiles);
        if (flip)
            FlipHorizontally(tiles);

        w = Math.Abs(w);
        h = Math.Abs(h);

        for (var i = 0; i < h; i++)
            for (var j = 0; j < w; j++)
            {
                var (texTl, texTr, texBr, texBl) = GetTexCoords(tiles[j, i], (1, 1));
                var (tx, ty) = (Math.Floor((x + j) * tsz), Math.Floor((y + i) * tsz));
                var c = new Color(tint);
                var tl = new Vector2f((int)tx, (int)ty);
                var br = new Vector2f((int)(tx + tsz * scale), (int)(ty + tsz * scale));

                if (ang is 1 or 3)
                    br = new((int)(tx + tsz * scale), (int)(ty + tsz * scale));

                var tr = new Vector2f((int)br.X, (int)tl.Y);
                var bl = new Vector2f((int)tl.X, (int)br.Y);
                var rotated = GetRotatedPoints(ang, texTl, texTr, texBr, texBl);
                texTl = rotated.p1;
                texTr = rotated.p2;
                texBr = rotated.p3;
                texBl = rotated.p4;

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

                verts.Append(new(tl, c, texTl));
                verts.Append(new(tr, c, texTr));
                verts.Append(new(br, c, texBr));
                verts.Append(new(bl, c, texBl));
            }
    }
    public void DrawTileMap(Tile[,]? tileMap)
    {
        if (tileMap == null || tileMap.Length == 0 || verts == null)
            return;

        var (cellCountW, cellCountH) = (tileMap.GetLength(0), tileMap.GetLength(1));
        var tsz = AtlasTileSize;

        for (var y = 0; y < cellCountH; y++)
        {
            var accumulativeX = 0f;
            var textOffset = 0f;
            var wasText = false;

            for (var x = 0; x < cellCountW; x++)
            {
                var (id, tint, pose) = tileMap[x, y];
                var isText = textTileWidths.ContainsKey(id);

                if (wasText == false && isText)
                {
                    textOffset = 0f;
                    accumulativeX = x;

                    foreach (var ((ax, ay, aw, ah), align) in textAligns)
                    {
                        if (x < ax || y < ay || x >= ax + aw || y >= ay + ah)
                            continue;

                        var offset = GetTextOffset((x, y), tileMap);

                        if (align == TextAlign.Left) textOffset = 0f;
                        else if (align == TextAlign.Center) textOffset = offset / 2f;
                        else if (align == TextAlign.Right) textOffset = offset;
                    }
                }

                if (wasText && isText == false)
                {
                    accumulativeX = x + 1f;
                    textOffset = 0f;
                }

                if (id == default && isText == false)
                    continue;

                var curX = isText ? accumulativeX - (1f - textTileWidths[id]) / 2f : x;
                curX += textOffset;
                var color = new Color(tint);
                var tl = new Vector2f(curX * tsz, y * tsz);
                var tr = new Vector2f((curX + 1) * tsz, y * tsz);
                var br = new Vector2f((curX + 1) * tsz, (y + 1) * tsz);
                var bl = new Vector2f(curX * tsz, (y + 1) * tsz);
                var (texTl, texTr, texBr, texBl) = GetTexCoords(id, (1, 1));
                var (flip, ang) = GetOrientation(pose);
                var rotated = GetRotatedPoints((sbyte)-ang, tl, tr, br, bl);

                if (flip)
                {
                    (texTl, texTr) = (texTr, texTl);
                    (texBl, texBr) = (texBr, texBl);
                }

                tl = rotated.p1;
                tr = rotated.p2;
                br = rotated.p3;
                bl = rotated.p4;

                tl = new((int)tl.X, (int)tl.Y);
                tr = new((int)tr.X, (int)tr.Y);
                br = new((int)br.X, (int)br.Y);
                bl = new((int)bl.X, (int)bl.Y);

                verts.Append(new(tl, color, texTl));
                verts.Append(new(tr, color, texTr));
                verts.Append(new(br, color, texBr));
                verts.Append(new(bl, color, texBl));

                if (isText)
                    accumulativeX += textTileWidths[id];

                wasText = isText;
            }
        }
    }

    public bool IsOverlapping(PointF position)
    {
        return position is { x: >= 0, y: >= 0 } &&
               position.x <= Size.width &&
               position.y <= Size.height;
    }
    public PointF PositionFromPixel(PointI pixel)
    {
        Window.TryCreate();

        var (px, py) = ((float)pixel.x, (float)pixel.y);
        var (vw, vh) = Window.rendTexViewSz;
        var (cw, ch) = Size;
        var (ox, oy) = PixelOffset;
        var (mw, mh) = (cw * AtlasTileSize, ch * AtlasTileSize);
        var (ww, wh, ow, oh) = Window.GetRenderOffset();

        px -= ow;
        py -= oh;

        ox /= mw;
        oy /= mh;

        px -= ww / 2f;
        py -= wh / 2f;

        var x = Window.Map(px, 0, ww, 0, cw);
        var y = Window.Map(py, 0, wh, 0, ch);

        x *= vw / Zoom / mw;
        y *= vh / Zoom / mh;

        x += cw / 2f;
        y += ch / 2f;

        x -= ox * cw;
        y -= oy * ch;

        return (x, y);
    }
    public PointI PositionToPixel(PointF position)
    {
        Window.TryCreate();

        var (posX, posY) = position;
        var (vw, vh) = Window.rendTexViewSz;
        var (cw, ch) = Size;
        var (ox, oy) = PixelOffset;
        var (mw, mh) = (cw * AtlasTileSize, ch * AtlasTileSize);
        var (ww, wh, ow, oh) = Window.GetRenderOffset();

        ox /= mw;
        oy /= mh;

        posX += ox * cw;
        posY += oy * ch;

        posX -= cw / 2f;
        posY -= ch / 2f;

        posX /= vw / Zoom / mw;
        posY /= vh / Zoom / mh;

        var px = Window.Map(posX, 0, cw, 0, ww);
        var py = Window.Map(posY, 0, ch, 0, wh);

        px += ww / 2f;
        py += wh / 2f;

        px += ow;
        py += oh;

        return ((int)px, (int)py);
    }
    public PointF PositionToLayer(PointF position, LayerTiles layerTiles)
    {
        return layerTiles.PositionFromPixel(PositionToPixel(position));
    }
    public PointF PositionToLayer(PointF position, LayerSprites layerSprites)
    {
        return layerSprites.PositionFromPixel(PositionToPixel(position));
    }

    public uint AtlasColorAt(PointI pixel)
    {
        if (pixel.x < 0 ||
            pixel.y < 0 ||
            pixel.x >= LayerSprites.textures[atlasPath].Size.X ||
            pixel.y >= LayerSprites.textures[atlasPath].Size.Y)
            return default;

        images.TryAdd(atlasPath, LayerSprites.textures[atlasPath].CopyToImage());
        var img = images[atlasPath];
        var color = img.GetPixel((uint)pixel.x, (uint)pixel.y).ToInteger();
        return color;
    }

    public static void DefaultGraphicsToFile(string filePath)
    {
        LayerSprites.textures["default"].CopyToImage().SaveToFile(filePath);
    }
    public static void ReloadGraphics()
    {
        var paths = LayerSprites.textures.Keys.ToArray();

        foreach (var path in paths)
        {
            if (path == DEFAULT_GRAPHICS)
                continue;

            LayerSprites.textures[path].Dispose();
            LayerSprites.textures[path] = null!;
            LayerSprites.textures[path] = new(path) { Repeated = true };
        }
    }

#region Backend
    private const string DEFAULT_GRAPHICS = "default";

    [DoNotSave]
    internal RenderTexture? queue, result;
    [DoNotSave]
    private readonly VertexArray? verts = new(PrimitiveType.Quads);

    internal Vector2u tilesetPixelSize;
    internal SizeI TilemapPixelSize
    {
        get => (Size.width * AtlasTileSize, Size.height * AtlasTileSize);
    }

    private string atlasPath;
    private SizeI size;
    private float zoom;

    private readonly Dictionary<ushort, float> textTileWidths = [];
    private readonly Dictionary<(int x, int y, int w, int h), TextAlign> textAligns = [];

    [DoNotSave]
    internal static readonly Dictionary<string, Image> images = new();
    [DoNotSave]
    private static readonly List<PointF> cursorOffsets =
    [
        (0.0f, 0.0f), (0.0f, 0.0f), (0.4f, 0.4f), (0.4f, 0.4f), (0.3f, 0.0f), (0.4f, 0.4f),
        (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f)
    ];

    static LayerTiles()
    {
        // var base64 = DefaultGraphics.PngToBase64String("graphics.png");
        LayerSprites.textures[DEFAULT_GRAPHICS] = DefaultGraphics.CreateTexture();
        // DefaultGraphicsToFile("graphics.png");
    }

    [MemberNotNull(nameof(AtlasTileIdFull), nameof(AtlasTileSize), nameof(tilesetPixelSize))]
    private void Init()
    {
        AtlasTileIdFull = 10;
        AtlasTileSize = 8;
        tilesetPixelSize = new(208, 208);
    }

    private float GetTextOffset(PointI cell, Tile[,] tileMap)
    {
        var totalWidth = 0f;
        for (var i = cell.x; i < tileMap.GetLength(0); i++)
        {
            var id = tileMap[i, cell.y].id;
            if (textTileWidths.TryGetValue(id, out var width) == false)
                return i - cell.x - totalWidth;

            totalWidth += width;
        }

        return tileMap.GetLength(0) - cell.x - totalWidth;
    }
    private void QueueLine(PointF a, PointF b, uint tint)
    {
        if (verts == null ||
            (IsOverlapping(a) == false && IsOverlapping(b) == false)) // fully outside?
            return;

        a.x *= AtlasTileSize;
        a.y *= AtlasTileSize;
        b.x *= AtlasTileSize;
        b.y *= AtlasTileSize;

        const float THICKNESS = 0.5f;
        var color = new Color(tint);
        var dir = new Vector2(b.x - a.x, b.y - a.y);
        var length = dir.Length() / 2f;
        var center = new Vector2((a.x + b.x) / 2, (a.y + b.y) / 2);
        var tl = new Vector2(-length, -THICKNESS);
        var tr = new Vector2(length, -THICKNESS);
        var br = new Vector2(length, THICKNESS);
        var bl = new Vector2(-length, THICKNESS);
        var (texTl, texTr, texBr, texBl) = GetTexCoords(AtlasTileIdFull, (1, 1));

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
    private void QueueRectangle(PointF position, (float w, float h) sz, uint tint)
    {
        if (verts == null)
            return;

        var (w, h) = sz;

        if (IsOverlapping(position) == false &&
            IsOverlapping((position.x + w, position.y)) == false &&
            IsOverlapping((position.x + w, position.y + h)) == false &&
            IsOverlapping((position.x, position.y + h)) == false)
            return;

        var (x, y) = (position.x * AtlasTileSize, position.y * AtlasTileSize);
        var color = new Color(tint);
        var (texTl, texTr, texBr, texBl) = GetTexCoords(AtlasTileIdFull, (1, 1));
        var tl = new Vector2f((int)x, (int)y);
        var br = new Vector2f((int)(x + AtlasTileSize * w), (int)(y + AtlasTileSize * h));
        var tr = new Vector2f((int)br.X, (int)tl.Y);
        var bl = new Vector2f((int)tl.X, (int)br.Y);

        verts.Append(new(tl, color, texTl));
        verts.Append(new(tr, color, texTr));
        verts.Append(new(br, color, texBr));
        verts.Append(new(bl, color, texBl));
    }

    private static (bool mirrorH, sbyte angle) GetOrientation(byte pose)
    {
        return (pose > 3, (sbyte)(pose % 4));
    }

    internal void DrawQueue()
    {
        if (queue == null || queue.Size.X != Size.width * AtlasTileSize || queue.Size.Y != Size.height * AtlasTileSize)
        {
            queue?.Dispose();
            result?.Dispose();
            queue = null;
            result = null;

            var (rw, rh) = ((uint)Size.width * AtlasTileSize, (uint)Size.height * AtlasTileSize);
            queue = new(rw, rh);
            result = new(rw, rh);
        }

        var texture = LayerSprites.textures[AtlasPath];
        var (w, h) = (queue?.Texture.Size.X ?? 0, queue?.Texture.Size.Y ?? 0);
        var r = new RenderStates(BlendMode.Alpha, Transform.Identity, queue?.Texture, Effect?.shader);

        Effect?.UpdateShader(texture, GetTexCoords(AtlasTileIdFull, (1, 1)), Size, AtlasTileSize);

        queue?.Clear(new(BackgroundColor));
        queue?.Draw(verts, new(texture));
        queue?.Display();

        Window.vertsWindow[0] = new(new(0, 0), Color.White, new(0, 0));
        Window.vertsWindow[1] = new(new(w, 0), Color.White, new(w, 0));
        Window.vertsWindow[2] = new(new(w, h), Color.White, new(w, h));
        Window.vertsWindow[3] = new(new(0, h), Color.White, new(0, h));

        result?.Clear(new(Color.Transparent));
        result?.Draw(Window.vertsWindow, PrimitiveType.Quads, r);
        result?.Display();

        verts?.Clear();
    }

    private CornersS GetTexCoords(int tileId, SizeI sz)
    {
        var (w, h) = sz;
        var tsz = AtlasTileSize;
        var (tx, ty) = IndexToCoords(tileId);
        var tl = new Vector2f(tx * (tsz + AtlasTileGap), ty * (tsz + AtlasTileGap));
        var tr = tl + new Vector2f(tsz * w, 0);
        var br = tl + new Vector2f(tsz * w, tsz * h);
        var bl = tl + new Vector2f(0, tsz * h);
        return (tl, tr, br, bl);
    }
    private PointI IndexToCoords(int index)
    {
        var (tw, th) = AtlasTileCount;
        index = index < 0 ? 0 : index;
        index = index > tw * th - 1 ? tw * th - 1 : index;

        return (index % tw, index / tw);
    }
    private int CoordsToIndex(int x, int y)
    {
        return y * AtlasTileCount.width + x;
    }

    private static CornersP GetRotatedPoints(sbyte turns, Vector2f p1, Vector2f p2, Vector2f p3, Vector2f p4)
    {
        var rotations = Math.Abs(turns) % 4;
        for (var i = 0; i < rotations; i++)
        {
            if (turns > 0)
            {
                var last = p4;
                p4 = p3;
                p3 = p2;
                p2 = p1;
                p1 = last;
                continue;
            }

            var first = p1;
            p1 = p2;
            p2 = p3;
            p3 = p4;
            p4 = first;
        }

        return (p1, p2, p3, p4);
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
#endregion
}