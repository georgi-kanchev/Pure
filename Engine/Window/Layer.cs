namespace Pure.Engine.Window;

using SFML.Graphics.Glsl;
using System.Numerics;
using System.Diagnostics.CodeAnalysis;

[Flags]
public enum Edge
{
    Top = 1 << 0, Bottom = 1 << 1, Left = 1 << 2, Right = 1 << 3, Corners = 1 << 4,
    AllEdges = Top | Bottom | Left | Right, AllEdgesAndCorners = AllEdges | Corners
}

[Flags]
public enum LightFlags
{
    Default = 0, Flat = 1 << 0, Mask = 1 << 1, Inverted = 1 << 2, ShadowInsideObstacles = 1 << 3
}

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
    public (byte width, byte height) AtlasTileCount
    {
        get
        {
            var (w, h) = ((int)tilesetPixelSize.X, (int)tilesetPixelSize.Y);
            var (tw, th) = ((int)AtlasTileSize.width, (int)AtlasTileSize.height);
            var (gw, gh) = ((int)AtlasTileGap.width, (int)AtlasTileGap.height);
            var (rw, rh) = (w / (tw + gw), h / (th + gh));
            return ((byte)rw, (byte)rh);
        }
    }
    public (byte width, byte height) AtlasTileSize
    {
        get => atlasTileSize;
        set => atlasTileSize =
            ((byte)Math.Clamp((int)value.width, 0, 512), (byte)Math.Clamp((int)value.height, 0, 512));
    }
    public (byte width, byte height) AtlasTileGap
    {
        get => atlasTileGap;
        set => atlasTileGap =
            ((byte)Math.Clamp((int)value.width, 0, 512), (byte)Math.Clamp((int)value.height, 0, 512));
    }
    public int AtlasTileIdFull { get; set; }

    public (int width, int height) Size
    {
        get => size;
        set => size = (Math.Clamp(value.width, 1, 1000), Math.Clamp(value.height, 1, 1000));
    }
    public uint BackgroundColor { get; set; }

    public (float x, float y) Offset { get; set; }
    public float Zoom
    {
        get => zoom;
        set => zoom = Math.Clamp(value, 0, 1000f);
    }

    public bool IsHovered
    {
        get => IsOverlapping(PixelToPosition(Mouse.CursorPosition));
    }

    public Layer((int width, int height) size = default)
    {
        Init();
        verts = new(PrimitiveType.Quads);
        shader = new EffectLayer().Shader;
        shaderParams = new(PrimitiveType.Points);

        atlasPath = string.Empty;
        AtlasPath = string.Empty;

        Size = size;
        Zoom = 1f;
    }
    public Layer(byte[] bytes)
    {
        var b = Decompress(bytes);
        var offset = 0;

        var bAtlasPath = GetInt();
        AtlasPath = Encoding.UTF8.GetString(GetBytesFrom(b, bAtlasPath, ref offset));
        atlasPath = AtlasPath;
        AtlasTileGap = (GetByte(), GetByte());
        AtlasTileSize = (GetByte(), GetByte());

        AtlasTileIdFull = GetInt();
        Size = (GetInt(), GetInt());

        BackgroundColor = GetUInt();

        Zoom = GetFloat();
        Offset = (GetFloat(), GetFloat());

        verts = new(PrimitiveType.Quads);
        shaderParams = new(PrimitiveType.Points);

        float GetFloat()
        {
            return BitConverter.ToSingle(GetBytesFrom(b, 4, ref offset));
        }

        int GetInt()
        {
            return BitConverter.ToInt32(GetBytesFrom(b, 4, ref offset));
        }

        uint GetUInt()
        {
            return BitConverter.ToUInt32(GetBytesFrom(b, 4, ref offset));
        }

        byte GetByte()
        {
            return GetBytesFrom(b, 1, ref offset)[0];
        }
    }
    public Layer(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public void ToDefault()
    {
        Init();
        Zoom = 1f;
        AtlasTileGap = (0, 0);
        atlasPath = string.Empty;
        AtlasPath = string.Empty;
        AtlasTileIdFull = 10;
    }
    public string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public byte[] ToBytes()
    {
        var result = new List<byte>();
        var bAtlasPath = Encoding.UTF8.GetBytes(AtlasPath);
        result.AddRange(BitConverter.GetBytes(bAtlasPath.Length));
        result.AddRange(bAtlasPath);
        result.Add(AtlasTileGap.width);
        result.Add(AtlasTileGap.height);
        result.Add(AtlasTileSize.width);
        result.Add(AtlasTileSize.height);

        result.AddRange(BitConverter.GetBytes(AtlasTileIdFull));
        result.AddRange(BitConverter.GetBytes(Size.width));
        result.AddRange(BitConverter.GetBytes(Size.height));

        result.AddRange(BitConverter.GetBytes(BackgroundColor));

        result.AddRange(BitConverter.GetBytes(Zoom));
        result.AddRange(BitConverter.GetBytes(Offset.x));
        result.AddRange(BitConverter.GetBytes(Offset.y));

        return Compress(result.ToArray());
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

        var (x, y) = PixelToPosition(Mouse.CursorPosition);
        DrawTiles((x - offX, y - offY), tile);
    }
    public void DrawPoints(params (float x, float y, uint color)[]? points)
    {
        for (var i = 0; i < points?.Length; i++)
        {
            var p = points[i];
            QueueRectangle((p.x, p.y), (1f / AtlasTileSize.width, 1f / AtlasTileSize.height), p.color);
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
    public void DrawTiles((float x, float y) position, (int id, uint tint, sbyte turns, bool mirror, bool flip) tile, (int width, int height) groupSize = default, bool sameTile = default)
    {
        var (id, tint, angle, flipH, flipV) = tile;
        var (w, h) = groupSize;
        w = w == 0 ? 1 : w;
        h = h == 0 ? 1 : h;

        var tiles = new int[Math.Abs(w), Math.Abs(h)];
        var (tileX, tileY) = IndexToCoords(id);
        var (x, y) = position;
        var (tw, th) = AtlasTileSize;

        for (var i = 0; i < Math.Abs(h); i++)
            for (var j = 0; j < Math.Abs(w); j++)
                tiles[j, i] = CoordsToIndex(tileX + (sameTile ? 0 : j), tileY + (sameTile ? 0 : i));

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
                var (tx, ty) = (Math.Floor((x + j) * tw), Math.Floor((y + i) * th));
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

                verts.Append(new(tl, c, texTl));
                verts.Append(new(tr, c, texTr));
                verts.Append(new(br, c, texBr));
                verts.Append(new(bl, c, texBl));
            }
    }
    public void DrawTilemap((int id, uint tint, sbyte turns, bool mirror, bool flip)[,] tilemap)
    {
        var (cellCountW, cellCountH) = (tilemap.GetLength(0), tilemap.GetLength(1));
        var (tw, th) = AtlasTileSize;

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

    public void ApplyColorReplacement(uint newColor, params (float x, float y, float width, float height, uint oldColor)[] areas)
    {
        for (var i = 0; i < areas?.Length; i++)
        {
            var (x, y, w, h, c) = areas[i];
            SetShaderData((x, y, w, h), (0, 2), Color.Transparent, true);
            SetShaderData((x, y, w, h), (1, 2), new(c), false);
            SetShaderData((x, y, w, h), (2, 2), new(newColor), false);
        }
    }
    public void ApplyColorAdjustments(sbyte gamma, sbyte saturation, sbyte contrast, sbyte brightness, params (float x, float y, float width, float height)[] areas)
    {
        for (var i = 0; i < areas?.Length; i++)
        {
            var (x, y, w, h) = areas[i];
            ApplyColorAdjustments(gamma, saturation, contrast, brightness, (x, y, w, h, 0));
        }
    }
    public void ApplyColorAdjustments(sbyte gamma, sbyte saturation, sbyte contrast, sbyte brightness, params (float x, float y, float width, float height, uint targetColor)[] areas)
    {
        var g = (byte)(gamma + 128);
        var s = (byte)(saturation + 128);
        var ct = (byte)(contrast + 128);
        var b = (byte)(brightness + 128);

        for (var i = 0; i < areas?.Length; i++)
        {
            var (x, y, w, h, c) = areas[i];
            SetShaderData((x, y, w, h), (0, 3), Color.Transparent, true);
            SetShaderData((x, y, w, h), (1, 3), new(g, s, ct, b), false);
            SetShaderData((x, y, w, h), (2, 3), new(c), false);
        }
    }
    public void ApplyColorTint(uint tint, params (float x, float y, float width, float height)[] areas)
    {
        for (var i = 0; i < areas?.Length; i++)
        {
            var (x, y, w, h) = areas[i];
            ApplyColorTint(tint, (x, y, w, h, 0));
        }
    }
    public void ApplyColorTint(uint tint, params (float x, float y, float width, float height, uint targetColor)[] areas)
    {
        for (var i = 0; i < areas?.Length; i++)
        {
            var (x, y, w, h, c) = areas[i];
            SetShaderData((x, y, w, h), (0, 0), Color.Transparent, true);
            SetShaderData((x, y, w, h), (1, 0), new(tint), false);
            SetShaderData((x, y, w, h), (2, 0), new(c), false);
        }
    }
    public void ApplyLights(float radius, LightFlags flags, params (float x, float y, uint color)[] points)
    {
        radius /= Size.height;

        shader?.SetUniform("lightFlags", (int)flags);

        for (var i = 0; i < points?.Length; i++)
        {
            var (x, y, color) = points[i];
            x /= Size.width;
            y /= Size.height;

            lightCount++;
            shader?.SetUniform("lightCount", lightCount);
            shader?.SetUniform($"light[{lightCount - 1}]", new Vec3(x, 1 - y, radius));
            shader?.SetUniform($"lightColor[{lightCount - 1}]", new Color(color));
        }
    }
    public void ApplyLightObstacles(params (float x, float y, float width, float height)[] areas)
    {
        for (var i = 0; i < areas?.Length; i++)
        {
            var (x, y, w, h) = areas[i];

            x /= Size.width;
            y /= Size.height;
            w /= Size.width;
            h /= Size.height;

            obstacleCount++;
            shader?.SetUniform("obstacleCount", obstacleCount);
            shader?.SetUniform($"obstacleArea[{obstacleCount - 1}]", new Vec4(x, 1 - y, w, h));
        }
    }
    public void ApplyLightObstacles(params (float x, float y, float width, float height, uint _)[] areas)
    {
        for (var i = 0; i < areas?.Length; i++)
        {
            var (x, y, w, h, _) = areas[i];
            ApplyLightObstacles((x, y, w, h));
        }
    }
    public void ApplyEdges(uint color, Edge edges, params (float x, float y, float width, float height, uint targetColor)[] areas)
    {
        for (var i = 0; i < areas?.Length; i++)
        {
            var (x, y, w, h, c) = areas[i];
            SetShaderData((x, y, w, h), (0, 5), Color.Transparent, true);
            SetShaderData((x, y, w, h), (1, 5), new(c), false);
            SetShaderData((x, y, w, h), (2, 5), new(color), false);
            SetShaderData((x, y, w, h), (3, 5), new((byte)edges, 0, 0, 0), false);
        }
    }
    public void ApplyBlur((byte x, byte y) strength, params (float x, float y, float width, float height, uint targetColor)[] areas)
    {
        var (sx, sy) = strength;

        for (var i = 0; i < areas?.Length; i++)
        {
            var (x, y, w, h, c) = areas[i];
            SetShaderData((x, y, w, h), (0, 1), Color.Transparent, true);
            SetShaderData((x, y, w, h), (1, 1), new(c), false);
            SetShaderData((x, y, w, h), (2, 1), new(sx, sy, 0, 0), false);
        }
    }
    public void ApplyBlur((byte x, byte y) strength, params (float x, float y, float width, float height)[] areas)
    {
        for (var i = 0; i < areas?.Length; i++)
        {
            var (x, y, w, h) = areas[i];
            ApplyBlur(strength, (x, y, w, h, 0));
        }
    }
    public void ApplyWaves((sbyte x, sbyte y) speed, (byte x, byte y) frequency, params (float x, float y, float width, float height, uint targetColor)[] areas)
    {
        for (var i = 0; i < areas?.Length; i++)
        {
            var (x, y, w, h, c) = areas[i];
            SetShaderData((x, y, w, h), (0, 4), Color.Transparent, true);
            SetShaderData((x, y, w, h), (1, 4), new((byte)speed.x, (byte)speed.y, frequency.x, frequency.y), false);
            SetShaderData((x, y, w, h), (2, 4), new(c), false);
        }
    }

    public bool IsOverlapping((float x, float y) position)
    {
        return position is { x: >= 0, y: >= 0 } &&
               position.x <= Size.width &&
               position.y <= Size.height;
    }
    public (float x, float y) PixelToPosition((int x, int y) pixel)
    {
        if (Window.window == null)
            return (float.NaN, float.NaN);

        var (px, py) = (pixel.x * 1f, pixel.y * 1f);
        var (vw, vh) = Window.rendTexViewSz;
        var (cw, ch) = Size;
        var (tw, th) = AtlasTileSize;
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

    public static void DefaultGraphicsToFile(string filePath)
    {
        tilesets["default"].CopyToImage().SaveToFile(filePath);
    }

    public static implicit operator byte[](Layer layer)
    {
        return layer.ToBytes();
    }

#region Backend
    // per tile shader data map (8x8 pixels)
    //
    // [tnta][tntc][tntt][    ][    ][    ][    ][    ]
    // [blra][blrt][blrs][    ][    ][    ][    ][    ]
    // [repa][repo][repn][    ][    ][    ][    ][    ]
    // [adja][adjd][adjt][    ][    ][    ][    ][    ]
    // [wava][wavd][wavt][    ][    ][    ][    ][    ]
    // [edga][edgt][edgc][edgy][    ][    ][    ][    ]
    // [blka][    ][    ][    ][    ][    ][    ][    ]
    // [    ][    ][    ][    ][    ][    ][    ][    ]
    //
    // tnta: tint area =          [x, y, w, h]
    // tntc: tint color =         [r, g, b, a]
    // tntt: tint target color =  [r, g, b, a]
    //
    // adja: adjustments area =   [x, y, w, h]
    // adjd: adjustments data =   [g, s, c, b] (g = gamma, s = saturation, c = contrast, b = brightness)
    // adjt: adj target color =   [r, g, b, a]
    //
    // repa: replace area =       [x, y, w, h]
    // repo: replace color old =  [r, g, b, a]
    // repn: replace color new =  [r, g, b, a]
    //
    // blra: blur area =          [x, y, w, h]
    // blrt: blur target color =  [r, g, b, a]
    // blrs: blur strength =      [x, y, _, _]
    //
    // wava: wave area =          [x, y, w, h]
    // wavd: wave data =          [x, y, z, w] (xy = speed, zw = frequency)
    // wavt: wave target color =  [r, g, b, a]
    //
    // edga: edges area =         [x, y, w, h]
    // edgt: edges target color = [r, g, b, a]
    // edgc: edges color =        [r, g, b, a]
    // edgy: edges type =         [t, _, _, _]
    //
    // blka: light block area =   [x, y, w, h]

    internal RenderTexture? queue, result, data;
    internal readonly Shader? shader;
    private readonly VertexArray verts, shaderParams;
    internal static readonly Dictionary<string, Texture> tilesets = new();
    private static readonly List<(float, float)> cursorOffsets = new()
    {
        (0.0f, 0.0f), (0.0f, 0.0f), (0.4f, 0.4f), (0.4f, 0.4f), (0.3f, 0.0f), (0.4f, 0.4f),
        (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f)
    };

    internal int lightCount, obstacleCount;
    internal Vector2u tilesetPixelSize;
    internal (int w, int h) TilemapPixelSize
    {
        get
        {
            var (mw, mh) = Size;
            var (tw, th) = AtlasTileSize;
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
    private (byte width, byte height) atlasTileGap, atlasTileSize;
    private (int width, int height) size;
    private float zoom;

    [MemberNotNull(nameof(AtlasTileIdFull), nameof(AtlasTileSize), nameof(tilesetPixelSize))]
    private void Init()
    {
        AtlasTileIdFull = 10;
        AtlasTileSize = (8, 8);
        tilesetPixelSize = new(208, 208);
    }

    private void QueueLine((float x, float y) a, (float x, float y) b, uint tint)
    {
        if (IsOverlapping(a) == false && IsOverlapping(b) == false) // fully outside?
            return;

        var (tw, th) = AtlasTileSize;
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
    private void QueueRectangle((float x, float y) position, (float w, float h) size, uint tint)
    {
        var (tw, th) = AtlasTileSize;
        var (w, h) = size;

        if (IsOverlapping(position) == false &&
            IsOverlapping((position.x + w, position.y)) == false &&
            IsOverlapping((position.x + w, position.y + h)) == false &&
            IsOverlapping((position.x, position.y + h)) == false)
            return;

        var (x, y) = (position.x * tw, position.y * th);
        var color = new Color(tint);
        var (texTl, texTr, texBr, texBl) = GetTexCoords(AtlasTileIdFull, (1, 1));
        var tl = new Vector2f((int)x, (int)y);
        var br = new Vector2f((int)(x + tw * w), (int)(y + th * h));
        var tr = new Vector2f((int)br.X, (int)tl.Y);
        var bl = new Vector2f((int)tl.X, (int)br.Y);

        verts.Append(new(tl, color, texTl));
        verts.Append(new(tr, color, texTr));
        verts.Append(new(br, color, texBr));
        verts.Append(new(bl, color, texBl));
    }
    private void SetShaderData((float x, float y, float width, float height) area, (int x, int y) tilePixel, Color data, bool includeArea)
    {
        var (x, y, w, h) = area;
        var (ix, iy) = ((int)x, (int)y);

        for (var j = y; j <= y + h; j++)
            for (var i = x; i <= x + w; i++)
            {
                var (tx, ty) = ((int)Math.Clamp(i, ix, x + w), (int)Math.Clamp(j, iy, y + h));
                var isLeft = Math.Abs(i - x) <= 0.01f;
                var isTop = Math.Abs(j - y) <= 0.01f;
                var isRight = Math.Abs(i - (x + w)) <= 0.01f;
                var isBottom = Math.Abs(j - (y + h)) <= 0.01f;

                var rx = isLeft ? i - tx : 0f;
                var ry = isTop ? j - ty : 0f;
                var rw = isLeft ? 1f - rx : 1f;
                var rh = isTop ? 1f - ry : 1f;

                rw = isRight ? i - tx : rw;
                rh = isBottom ? j - ty : rh;

                var res = new Color((byte)(rx * 255), (byte)(ry * 255), (byte)(rw * 255), (byte)(rh * 255));
                var (vx, vy) = (tx * AtlasTileSize.width, ty * AtlasTileSize.height);
                var full = GetTexCoords(AtlasTileIdFull, (1, 1));
                var (px, py) = tilePixel;

                shaderParams.Append(new(new(vx + px, vy + 0.5f + py), data, full.tl));

                if (includeArea)
                    shaderParams.Append(new(new(vx + px, vy + 0.5f + py), res, full.tl));
            }
    }

    internal void DrawQueue()
    {
        if (queue == null || queue.Size.X != Size.width || queue.Size.Y != Size.height)
        {
            queue?.Dispose();
            data?.Dispose();
            result?.Dispose();
            queue = null;
            data = null;
            result = null;

            var (tw, th) = AtlasTileSize;
            var (rw, rh) = ((uint)Size.width * tw, (uint)Size.height * th);
            queue = new(rw, rh);
            data = new(rw, rh);
            result = new(rw, rh);
        }

        var atlas = tilesets[AtlasPath];
        var view = queue?.GetView();
        var (w, h) = (queue?.Texture.Size.X ?? 0, queue?.Texture.Size.Y ?? 0);
        var r = new RenderStates(BlendMode.Alpha, Transform.Identity, queue?.Texture, shader);

        data?.Clear(Color.Transparent);
        data?.Draw(shaderParams, new(BlendMode.None, Transform.Identity, atlas, null));
        data?.Display();

        lightCount = 0;
        obstacleCount = 0;
        shader?.SetUniform("tileSize", new Vec2(AtlasTileSize.width, AtlasTileSize.height));
        shader?.SetUniform("tileCount", new Vec2(Size.width, Size.height));
        shader?.SetUniform("time", Window.time.ElapsedTime.AsSeconds());
        shader?.SetUniform("data", data?.Texture);

        queue?.Clear(new(BackgroundColor));
        queue?.Draw(verts, new(atlas));
        queue?.Display();

        Window.vertsWindow[0] = new(new(0, 0), Color.White, new(0, 0));
        Window.vertsWindow[1] = new(new(w, 0), Color.White, new(w, 0));
        Window.vertsWindow[2] = new(new(w, h), Color.White, new(w, h));
        Window.vertsWindow[3] = new(new(0, h), Color.White, new(0, h));

        result?.Clear(new(Color.Transparent));
        result?.Draw(Window.vertsWindow, PrimitiveType.Quads, r);
        result?.Display();

        //queue?.Texture.CopyToImage().SaveToFile("render.png");
        //data?.Texture.CopyToImage().SaveToFile("data.png");

        verts.Clear();
        shaderParams.Clear();
    }

    private (Vector2f tl, Vector2f tr, Vector2f br, Vector2f bl) GetTexCoords(int tileId, (int w, int h) size)
    {
        var (w, h) = size;
        var (tw, th) = AtlasTileSize;
        var (gw, gh) = AtlasTileGap;
        var (tx, ty) = IndexToCoords(tileId);
        var tl = new Vector2f(tx * (tw + gw), ty * (th + gh));
        var tr = tl + new Vector2f(tw * w, 0);
        var br = tl + new Vector2f(tw * w, th * h);
        var bl = tl + new Vector2f(0, th * h);
        return (tl, tr, br, bl);
    }
    private (int x, int y) IndexToCoords(int index)
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

    private static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
        {
            stream.Write(data, 0, data.Length);
        }

        return output.ToArray();
    }
    private static byte[] Decompress(byte[] data)
    {
        var input = new MemoryStream(data);
        var output = new MemoryStream();
        using (var stream = new DeflateStream(input, CompressionMode.Decompress))
        {
            stream.CopyTo(output);
        }

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