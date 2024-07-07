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

public enum LightType
{
    ColoredLight, Mask, InvertedMask
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

    public int TileIdFull { get; set; }
    public (int width, int height) TilemapSize
    {
        get => tilemapSize;
        set => tilemapSize = (Math.Clamp(value.width, 1, 1000), Math.Clamp(value.height, 1, 1000));
    }

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

    public bool IsLightFading
    {
        get => isLightFading;
        set
        {
            shader?.SetUniform("lightFade", value);
            isLightFading = value;
        }
    }
    public LightType LightType
    {
        get => lightType;
        set
        {
            shader?.SetUniform("lightMask", value is LightType.Mask or LightType.InvertedMask);
            shader?.SetUniform("lightInvert", value == LightType.InvertedMask);
            lightType = value;
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
    public uint BackgroundColor { get; set; }

    public (float x, float y) Offset { get; set; }
    public float Zoom
    {
        get => zoom;
        set => zoom = Math.Clamp(value, 0, 1000f);
    }

    public Layer((int width, int height) tilemapSize = default)
    {
        Init();
        verts = new(PrimitiveType.Quads);
        shader = new EffectLayer().Shader;

        atlasPath = string.Empty;
        AtlasPath = string.Empty;

        TilemapSize = tilemapSize;

        Zoom = 1f;

        Gamma = 1f;
        Saturation = 1f;
        Contrast = 1f;
        Brightness = 1f;
        Tint = uint.MaxValue;
        IsLightFading = true;
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

        TileIdFull = GetInt();
        TilemapSize = (GetInt(), GetInt());

        Gamma = GetFloat();
        Saturation = GetFloat();
        Contrast = GetFloat();
        Brightness = GetFloat();
        Tint = GetUInt();
        BackgroundColor = GetUInt();

        IsLightFading = GetBool();
        LightType = (LightType)GetByte();

        Zoom = GetFloat();
        Offset = (GetFloat(), GetFloat());

        verts = new(PrimitiveType.Quads);

        float GetFloat()
        {
            return BitConverter.ToSingle(GetBytesFrom(b, 4, ref offset));
        }
        int GetInt()
        {
            return BitConverter.ToInt32(GetBytesFrom(b, 4, ref offset));
        }
        bool GetBool()
        {
            return BitConverter.ToBoolean(GetBytesFrom(b, 1, ref offset));
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
        TileIdFull = 10;
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

        result.AddRange(BitConverter.GetBytes(TileIdFull));
        result.AddRange(BitConverter.GetBytes(TilemapSize.width));
        result.AddRange(BitConverter.GetBytes(TilemapSize.height));

        result.AddRange(BitConverter.GetBytes(Gamma));
        result.AddRange(BitConverter.GetBytes(Saturation));
        result.AddRange(BitConverter.GetBytes(Contrast));
        result.AddRange(BitConverter.GetBytes(Brightness));
        result.AddRange(BitConverter.GetBytes(Tint));
        result.AddRange(BitConverter.GetBytes(BackgroundColor));

        result.AddRange(BitConverter.GetBytes(IsLightFading));
        result.AddRange(BitConverter.GetBytes((byte)LightType));

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

        var (x, y) = PixelToWorld(Mouse.CursorPosition);
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
    public void DrawEdges((float x, float y, float width, float height) area, (uint target, uint edge) colors, Edge edges = Edge.AllEdges)
    {
        edgeCount++;

        var i = edgeCount - 1;
        var (x, y, w, h) = area;

        x /= TilemapSize.width;
        y /= TilemapSize.height;
        w /= TilemapSize.width;
        h /= TilemapSize.height;

        shader?.SetUniform("edgeCount", edgeCount);
        shader?.SetUniform($"edgeTarget[{i}]", new Color(colors.target));
        shader?.SetUniform($"edgeColor[{i}]", new Color(colors.edge));
        shader?.SetUniform($"edgeArea[{i}]", new Vec4(x, 1 - y, w, h));
        shader?.SetUniform($"edgeType[{i}]", (int)edges);
    }

    public void BlockLight((float x, float y, float width, float height) area)
    {
        var (x, y, w, h) = area;

        x /= TilemapSize.width;
        y /= TilemapSize.height;
        w /= TilemapSize.width;
        h /= TilemapSize.height;

        obstacleCount++;
        shader?.SetUniform("obstacleCount", obstacleCount);
        shader?.SetUniform($"obstacleArea[{obstacleCount - 1}]", new Vec4(x, 1 - y, w, h));
    }
    public void Light((float x, float y) position, float radius, uint color)
    {
        var (x, y) = position;

        x /= TilemapSize.width;
        y /= TilemapSize.height;
        radius /= TilemapSize.height;

        lightCount++;
        shader?.SetUniform("lightCount", lightCount);
        shader?.SetUniform($"light[{lightCount - 1}]", new Vec3(x, 1 - y, radius));
        shader?.SetUniform($"lightColor[{lightCount - 1}]", new Color(color));
    }
    public void Blur((float x, float y, float width, float height) area, (float x, float y) strength, uint targetColor = 0)
    {
        blurCount++;
        var (x, y, w, h) = area;
        var (sx, sy) = strength;

        x /= TilemapSize.width;
        y /= TilemapSize.height;
        w /= TilemapSize.width;
        h /= TilemapSize.height;

        shader?.SetUniform("blurCount", blurCount);
        shader?.SetUniform($"blurArea[{blurCount - 1}]", new Vec4(x, 1 - y, w, h));
        shader?.SetUniform($"blurStrength[{blurCount - 1}]", new Vec2(sx, sy));
        shader?.SetUniform($"blurTarget[{blurCount - 1}]", new Color(targetColor));
    }
    public void Distort((float x, float y, float width, float height) area, (float x, float y) speed, (float x, float y) frequency, uint targetColor = 0)
    {
        waveCount++;
        var (x, y, w, h) = area;
        var (sx, sy) = speed;
        var (fx, fy) = frequency;

        x /= TilemapSize.width;
        y /= TilemapSize.height;
        w /= TilemapSize.width;
        h /= TilemapSize.height;

        shader?.SetUniform("waveCount", waveCount);
        shader?.SetUniform($"waveArea[{waveCount - 1}]", new Vec4(x, 1 - y, w, h));
        shader?.SetUniform($"waveSpeedFreq[{waveCount - 1}]", new Vec4(sx, sy, fx, fy));
        shader?.SetUniform($"waveTarget[{waveCount - 1}]", new Color(targetColor));
    }
    public void ReplaceColor(uint oldColor, uint newColor)
    {
        var pair = (oldColor, newColor);

        if (replaceColors.Contains(pair))
            return;

        if (oldColor == newColor)
        {
            var foundIndex = -1;
            for (var i = 0; i < replaceColors.Count; i++)
                if (replaceColors[i].oldColor == oldColor)
                {
                    foundIndex = i;
                    break;
                }

            if (foundIndex < 0)
                return;

            replaceColors.RemoveAt(foundIndex);
            UpdateReplaceUniforms();

            return;
        }

        replaceColors.Add(pair);
        UpdateReplaceUniforms();

        void UpdateReplaceUniforms()
        {
            shader?.SetUniform("replaceCount", replaceColors.Count);
            for (var i = 0; i < replaceColors.Count; i++)
            {
                shader?.SetUniform($"replaceOld[{i}]", new Color(replaceColors[i].oldColor));
                shader?.SetUniform($"replaceNew[{i}]", new Color(replaceColors[i].newColor));
            }
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
    internal readonly Shader? shader;
    internal readonly VertexArray verts;
    private readonly List<(uint oldColor, uint newColor)> replaceColors = new();
    internal static readonly Dictionary<string, Texture> tilesets = new();
    private static readonly List<(float, float)> cursorOffsets = new()
    {
        (0.0f, 0.0f), (0.0f, 0.0f), (0.4f, 0.4f), (0.4f, 0.4f), (0.3f, 0.0f), (0.4f, 0.4f),
        (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f), (0.4f, 0.4f)
    };

    internal int edgeCount, waveCount, blurCount, lightCount, obstacleCount;
    internal Vector2u tilesetPixelSize;
    internal (int w, int h) TilemapPixelSize
    {
        get
        {
            var (mw, mh) = TilemapSize;
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
    private (int width, int height) tilemapSize;
    private uint currTint = uint.MaxValue;
    private float zoom, gamma, saturation, contrast, brightness;
    private bool isLightFading;
    private LightType lightType;

    [MemberNotNull(nameof(TileIdFull), nameof(AtlasTileSize), nameof(tilesetPixelSize))]
    private void Init()
    {
        TileIdFull = 10;
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
        var (tw, th) = AtlasTileSize;
        var (w, h) = size;

        if (IsOverlapping(position) == false &&
            IsOverlapping((position.x + w, position.y)) == false &&
            IsOverlapping((position.x + w, position.y + h)) == false &&
            IsOverlapping((position.x, position.y + h)) == false)
            return;

        var (x, y) = (position.x * tw, position.y * th);
        var color = new Color(tint);
        var (texTl, texTr, texBr, texBl) = GetTexCoords(TileIdFull, (1, 1));
        var tl = new Vector2f((int)x, (int)y);
        var br = new Vector2f((int)(x + tw * w), (int)(y + th * h));
        var tr = new Vector2f((int)br.X, (int)tl.Y);
        var bl = new Vector2f((int)tl.X, (int)br.Y);

        verts.Append(new(tl, color, texTl));
        verts.Append(new(tr, color, texTr));
        verts.Append(new(br, color, texBr));
        verts.Append(new(bl, color, texBl));
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
    private (int, int) IndexToCoords(int index)
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

    private static (float x, float y, float width, float height)[] MinimizeRectangles((float x, float y, float width, float height)[] areas)
    {
        var result = areas.ToList();
        var hasChanges = true;

        while (hasChanges)
        {
            hasChanges = false;

            for (var i = 0; i < result.Count; i++)
            {
                for (var j = i + 1; j < result.Count; j++)
                {
                    var merged = Merge(result[i], result[j]);
                    if (merged == null)
                        continue;

                    result[i] = merged ?? default;
                    result.RemoveAt(j);
                    hasChanges = true;
                    break;
                }

                if (hasChanges)
                    break;
            }
        }

        return result.ToArray();
    }
    private static (float x, float y, float width, float height)? Merge((float x, float y, float width, float height) r1, (float x, float y, float width, float height) r2)
    {
        if (Is(r1.y, r2.y) &&
            Is(r1.height, r2.height) &&
            (Is(r1.x + r1.width, r2.x) || Is(r2.x + r2.width, r1.x)))
            return (Math.Min(r1.x, r2.x), r1.y, r1.width + r2.width, r1.height);
        if (Is(r1.x, r2.x) &&
            Is(r1.width, r2.width) &&
            (Is(r1.y + r1.height, r2.y) || Is(r2.y + r2.height, r1.y)))
            return (r1.x, Math.Min(r1.y, r2.y), r1.width, r1.height + r2.height);

        return null;

        bool Is(float a, float b)
        {
            return Math.Abs(a - b) < 0.001f;
        }
    }
#endregion
}