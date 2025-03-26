namespace Pure.Engine.Window;

[Flags]
public enum Edge
{
    Top = 1 << 0, Bottom = 1 << 1, Left = 1 << 2, Right = 1 << 3, Corners = 1 << 4,
    AllEdges = Top | Bottom | Left | Right, AllEdgesAndCorners = AllEdges | Corners
}

[Flags]
public enum Light { Default = 0, Flat = 1 << 0, Mask = 1 << 1, Inverted = 1 << 2, ObstaclesInShadow = 1 << 3 }

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

            if (atlases.TryGetValue(value, out var tileset))
            {
                atlasPath = value;
                tilesetPixelSize = tileset.Size;
                return;
            }

            try
            {
                atlases[value] = new(value) { Repeated = true };
                atlasPath = value;
                tilesetPixelSize = atlases[value].Size;
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

    public Light EffectLight
    {
        get => effectLight;
        set
        {
            effectLight = value;
            shader?.SetUniform("lightFlags", (int)value);
        }
    }
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

    public void EffectChangeColor(int id, uint oldColor, uint newColor, params AreaF[]? areas)
    {
        if (id is < 0 or > 8 || oldColor == newColor)
            return;

        if (areas == null || areas.Length == 0)
            areas = [(0, 0, Size.width, Size.height)];

        var indexes = new List<PointI[]>
        {
            { [(0, 2), (1, 2), (2, 2)] }, { [(5, 0), (6, 0), (7, 0)] }, { [(5, 1), (6, 1), (7, 1)] },
            { [(5, 2), (6, 2), (7, 2)] }, { [(5, 3), (6, 3), (7, 3)] }, { [(5, 4), (6, 4), (7, 4)] },
            { [(5, 5), (6, 5), (7, 5)] }, { [(5, 6), (6, 6), (7, 6)] }, { [(5, 7), (6, 7), (7, 7)] }
        };

        for (var i = 0; i < areas.Length; i++)
        {
            SetShaderData(areas[i], indexes[id][0], Color.Transparent, true);
            SetShaderData(areas[i], indexes[id][1], new(oldColor), false);
            SetShaderData(areas[i], indexes[id][2], new(newColor), false);
        }
    }
    public void EffectAdjustColor(sbyte gamma, sbyte saturation, sbyte contrast, sbyte brightness, params AreaToColor[]? areas)
    {
        if (areas == null || areas.Length == 0)
            areas = [(0, 0, Size.width, Size.height, 0)];

        var g = (byte)(gamma + 128);
        var s = (byte)(saturation + 128);
        var ct = (byte)(contrast + 128);
        var b = (byte)(brightness + 128);

        for (var i = 0; i < areas.Length; i++)
        {
            var (x, y, w, h, c) = areas[i];
            SetShaderData((x, y, w, h), (0, 3), Color.Transparent, true);
            SetShaderData((x, y, w, h), (1, 3), new(g, s, ct, b), false);
            SetShaderData((x, y, w, h), (2, 3), new(c), false);
        }
    }
    public void EffectTintColor(uint tint, params AreaToColor[]? areas)
    {
        if (areas == null || areas.Length == 0)
            areas = [(0, 0, Size.width, Size.height, 0)];

        for (var i = 0; i < areas.Length; i++)
        {
            var (x, y, w, h, c) = areas[i];
            SetShaderData((x, y, w, h), (0, 0), Color.Transparent, true);
            SetShaderData((x, y, w, h), (1, 0), new(tint), false);
            SetShaderData((x, y, w, h), (2, 0), new(c), false);
        }
    }
    public void EffectAddLight(float radius, (float width, float angle) cone, params PointColored[] points)
    {
        radius /= Size.height;
        var (w, ang) = cone;

        for (var i = 0; i < points?.Length; i++)
        {
            var (x, y, color) = points[i];
            x /= Size.width;
            y /= Size.height;

            lightCount++;
            shader?.SetUniform("lightCount", lightCount);

            shader?.SetUniform($"light[{lightCount - 1}]", new Vec3(x, 1 - y, radius));
            shader?.SetUniform($"lightCone[{lightCount - 1}]", new Vec2(w, ang));
            shader?.SetUniform($"lightColor[{lightCount - 1}]", new Color(color));
        }
    }
    public void EffectAddLightObstacles(params AreaF[] areas)
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
    public void EffectAddLightObstacles(params AreaColored[] areas)
    {
        for (var i = 0; i < areas?.Length; i++)
        {
            var (x, y, w, h, _) = areas[i];
            EffectAddLightObstacles((x, y, w, h));
        }
    }
    public void EffectColorEdges(uint color, Edge edges, params AreaToColor[]? areas)
    {
        if (areas == null || areas.Length == 0)
            areas = [(0, 0, Size.width, Size.height, 0)];

        for (var i = 0; i < areas.Length; i++)
        {
            var (x, y, w, h, c) = areas[i];
            SetShaderData((x, y, w, h), (0, 5), Color.Transparent, true);
            SetShaderData((x, y, w, h), (1, 5), new(c), false);
            SetShaderData((x, y, w, h), (2, 5), new(color), false);
            SetShaderData((x, y, w, h), (3, 5), new((byte)edges, 0, 0, 0), false);
        }
    }
    public void EffectBlur((byte x, byte y) strength, params AreaToColor[]? areas)
    {
        if (areas == null || areas.Length == 0)
            areas = [(0, 0, Size.width, Size.height, 0)];

        var (sx, sy) = strength;
        for (var i = 0; i < areas.Length; i++)
        {
            var (x, y, w, h, c) = areas[i];
            SetShaderData((x, y, w, h), (0, 1), Color.Transparent, true);
            SetShaderData((x, y, w, h), (1, 1), new(c), false);
            SetShaderData((x, y, w, h), (2, 1), new(sx, sy, 0, 0), false);
        }
    }
    public void EffectWave((sbyte x, sbyte y) speed, (byte x, byte y) frequency, params AreaToColor[]? areas)
    {
        if (areas == null || areas.Length == 0)
            areas = [(0, 0, Size.width, Size.height, 0)];

        var speedX = (byte)Window.Map(speed.x, sbyte.MinValue, sbyte.MaxValue, byte.MinValue, byte.MaxValue);
        var speedY = (byte)Window.Map(speed.y, sbyte.MinValue, sbyte.MaxValue, byte.MinValue, byte.MaxValue);

        for (var i = 0; i < areas.Length; i++)
        {
            var (x, y, w, h, c) = areas[i];
            SetShaderData((x, y, w, h), (0, 4), Color.Transparent, true);
            SetShaderData((x, y, w, h), (1, 4), new(speedX, speedY, frequency.x, frequency.y), false);
            SetShaderData((x, y, w, h), (2, 4), new(c), false);
        }
    }
    public void ClearAllEffects()
    {
        data?.Clear(Color.Transparent);
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
    public PointF PositionToLayer(PointF position, Layer layer)
    {
        return layer.PositionFromPixel(PositionToPixel(position));
    }

    public uint AtlasColorAt(PointI pixel)
    {
        if (pixel.x < 0 ||
            pixel.y < 0 ||
            pixel.x >= atlases[atlasPath].Size.X ||
            pixel.y >= atlases[atlasPath].Size.Y)
            return default;

        images.TryAdd(atlasPath, atlases[atlasPath].CopyToImage());
        var img = images[atlasPath];
        var color = img.GetPixel((uint)pixel.x, (uint)pixel.y).ToInteger();
        return color;
    }

    public static void DefaultGraphicsToFile(string filePath)
    {
        atlases["default"].CopyToImage().SaveToFile(filePath);
    }
    public static void ReloadGraphics()
    {
        var paths = atlases.Keys.ToArray();

        foreach (var path in paths)
        {
            if (path == DEFAULT_GRAPHICS)
                continue;

            atlases[path].Dispose();
            atlases[path] = null!;
            atlases[path] = new(path) { Repeated = true };
        }
    }

#region Backend
    // per tile shader data map (8x8 pixels)
    //
    // [tnta][tntc][tntt][    ][    ][rea2][reo2][ren2]
    // [blra][blrt][blrs][    ][    ][rea3][reo3][ren3]
    // [rea1][reo1][ren1][    ][    ][rea4][reo4][ren4]
    // [adja][adjd][adjt][    ][    ][rea5][reo5][ren5]
    // [wava][wavd][wavt][    ][    ][rea6][reo6][ren6]
    // [edga][edgt][edgc][edgy][    ][rea7][reo7][ren7]
    // [blka][    ][    ][    ][    ][rea8][reo8][ren8]
    // [    ][    ][    ][    ][    ][rea9][reo9][ren9]
    //
    // tnta: tint area =          [x, y, w, h]
    // tntc: tint color =         [r, g, b, a]
    // tntt: tint target color =  [r, g, b, a]
    //
    // adja: adjustments area =   [x, y, w, h]
    // adjd: adjustments data =   [g, s, c, b] (g = gamma, s = saturation, c = contrast, b = brightness)
    // adjt: adj target color =   [r, g, b, a]
    //
    // rea#: replace area =       [x, y, w, h]
    // reo#: replace color old =  [r, g, b, a]
    // ren#: replace color new =  [r, g, b, a]
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

    private const string DEFAULT_GRAPHICS = "default";
    private bool drawShaderData;

    [DoNotSave]
    internal RenderTexture? queue, result, data;
    [DoNotSave]
    internal Shader? shader;
    [DoNotSave]
    private VertexArray? verts, shaderParams;

    internal int lightCount, obstacleCount;
    internal Vector2u tilesetPixelSize;
    internal SizeI TilemapPixelSize
    {
        get => (Size.width * AtlasTileSize, Size.height * AtlasTileSize);
    }

    private string atlasPath;
    private SizeI size;
    private float zoom;
    private Light effectLight;

    private readonly Dictionary<ushort, float> textTileWidths = [];
    private readonly Dictionary<(int x, int y, int w, int h), TextAlign> textAligns = [];

    [DoNotSave]
    internal static readonly Dictionary<string, Texture> atlases = [];
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
        atlases[DEFAULT_GRAPHICS] = DefaultGraphics.CreateTexture();
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
    private void SetShaderData(AreaF area, PointI tilePixel, Color dataInColor, bool includeArea)
    {
        TryInit();

        drawShaderData = true;

        var (x, y, w, h) = area;
        var (ix, iy) = ((int)x, (int)y);
        var full = GetTexCoords(AtlasTileIdFull, (1, 1));
        var (px, py) = tilePixel;
        var centerOff = new Vector2f(AtlasTileSize / 2f, AtlasTileSize / 2f);

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
                var (vx, vy) = (tx * AtlasTileSize, ty * AtlasTileSize);

                shaderParams?.Append(new(new(vx + px, vy + 0.5f + py), dataInColor, full.tl + centerOff));

                if (includeArea)
                    shaderParams?.Append(new(new(vx + px, vy + 0.5f + py), res, full.tl + centerOff));
            }
    }
    private static (bool mirrorH, sbyte angle) GetOrientation(byte pose)
    {
        return (pose > 3, (sbyte)(pose % 4));
    }

    private void TryInit()
    {
        shader ??= new EffectLayer().Shader;
        verts ??= new(PrimitiveType.Quads);
        shaderParams ??= new(PrimitiveType.Points);

        if (queue != null && queue.Size.X == Size.width * AtlasTileSize && queue.Size.Y == Size.height * AtlasTileSize)
            return;

        queue?.Dispose();
        data?.Dispose();
        result?.Dispose();
        queue = null;
        data = null;
        result = null;

        var (rw, rh) = ((uint)Size.width * AtlasTileSize, (uint)Size.height * AtlasTileSize);
        queue = new(rw, rh);
        data = new(rw, rh);
        result = new(rw, rh);
    }
    internal void DrawQueue()
    {
        TryInit();

        var atlas = atlases[AtlasPath];
        var (w, h) = (queue?.Texture.Size.X ?? 0, queue?.Texture.Size.Y ?? 0);
        var r = new RenderStates(BlendMode.Alpha, Transform.Identity, queue?.Texture, shader);

        if (drawShaderData && data != null && shaderParams != null)
        {
            drawShaderData = false;
            data.Draw(shaderParams, new(BlendMode.None, Transform.Identity, atlas, null));
            data.Display();
        }

        lightCount = 0;
        obstacleCount = 0;
        shader?.SetUniform("tileSize", new Vec2(AtlasTileSize, AtlasTileSize));
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

        // result?.Texture.CopyToImage().SaveToFile($"render-{GetHashCode()}.png");
        // data?.Texture.CopyToImage().SaveToFile($"shader-data-{GetHashCode()}.png");

        verts?.Clear();
        shaderParams?.Clear();
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