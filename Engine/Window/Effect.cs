namespace Pure.Engine.Window;

[Flags]
public enum Edge
{
    Top = 1 << 0, Bottom = 1 << 1, Left = 1 << 2, Right = 1 << 3, Corners = 1 << 4,
    AllEdges = Top | Bottom | Left | Right, AllEdgesAndCorners = AllEdges | Corners
}

[Flags]
public enum Light { Default = 0, Flat = 1 << 0, Mask = 1 << 1, Inverted = 1 << 2, ObstaclesInShadow = 1 << 3 }

public class Effect
{
    public Effect(string? codeFragment = null, string? codeVertex = null)
    {
        codeFragment ??= ShaderCode.FRAGMENT_LAYER;
        codeVertex ??= ShaderCode.VERTEX_DEFAULT;

        if (Shader.IsAvailable)
            shader = Shader.FromString(codeVertex, null, codeFragment);

        if (white != null)
            return;

        var image = new Image(new[,] { { Color.White } });
        white = new(image) { Repeated = true };
        image.Dispose();
    }

    public Light EffectLight
    {
        get => effectLight;
        set
        {
            effectLight = value;
            shader?.SetUniform("lightFlags", (int)value);
        }
    }

    public void ColorChange(int id, uint oldColor, uint newColor, params AreaF[]? areas)
    {
        if (id is < 0 or > 8 || oldColor == newColor)
            return;

        if (areas == null || areas.Length == 0)
            areas = [(0, 0, layerSize.width, layerSize.height)];

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
    public void ColorAdjust(sbyte gamma, sbyte saturation, sbyte contrast, sbyte brightness, params AreaToColor[]? areas)
    {
        if (areas == null || areas.Length == 0)
            areas = [(0, 0, layerSize.width, layerSize.height, 0)];

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
    public void ColorTint(uint tint, params AreaToColor[]? areas)
    {
        if (areas == null || areas.Length == 0)
            areas = [(0, 0, layerSize.width, layerSize.height, 0)];

        for (var i = 0; i < areas.Length; i++)
        {
            var (x, y, w, h, c) = areas[i];
            SetShaderData((x, y, w, h), (0, 0), Color.Transparent, true);
            SetShaderData((x, y, w, h), (1, 0), new(tint), false);
            SetShaderData((x, y, w, h), (2, 0), new(c), false);
        }
    }
    public void ColorOutline(uint color, Edge edges, params AreaToColor[]? areas)
    {
        if (areas == null || areas.Length == 0)
            areas = [(0, 0, layerSize.width, layerSize.height, 0)];

        for (var i = 0; i < areas.Length; i++)
        {
            var (x, y, w, h, c) = areas[i];
            SetShaderData((x, y, w, h), (0, 5), Color.Transparent, true);
            SetShaderData((x, y, w, h), (1, 5), new(c), false);
            SetShaderData((x, y, w, h), (2, 5), new(color), false);
            SetShaderData((x, y, w, h), (3, 5), new((byte)edges, 0, 0, 0), false);
        }
    }
    public void AddLight(float radius, (float width, float angle) cone, params PointColored[] points)
    {
        radius /= layerSize.height;
        var (w, ang) = cone;

        for (var i = 0; i < points?.Length; i++)
        {
            var (x, y, color) = points[i];
            x /= layerSize.width;
            y /= layerSize.height;

            lightCount++;
            shader?.SetUniform("lightCount", lightCount);

            shader?.SetUniform($"light[{lightCount - 1}]", new Vec3(x, 1 - y, radius));
            shader?.SetUniform($"lightCone[{lightCount - 1}]", new Vec2(w, ang));
            shader?.SetUniform($"lightColor[{lightCount - 1}]", new Color(color));
        }
    }
    public void AddLightObstacles(params AreaF[] areas)
    {
        for (var i = 0; i < areas?.Length; i++)
        {
            var (x, y, w, h) = areas[i];

            x /= layerSize.width;
            y /= layerSize.height;
            w /= layerSize.width;
            h /= layerSize.height;

            obstacleCount++;
            shader?.SetUniform("obstacleCount", obstacleCount);
            shader?.SetUniform($"obstacleArea[{obstacleCount - 1}]", new Vec4(x, 1 - y, w, h));
        }
    }
    public void AddLightObstacles(params AreaColored[] areas)
    {
        for (var i = 0; i < areas?.Length; i++)
        {
            var (x, y, w, h, _) = areas[i];
            AddLightObstacles((x, y, w, h));
        }
    }
    public void Blur((byte x, byte y) strength, params AreaToColor[]? areas)
    {
        if (areas == null || areas.Length == 0)
            areas = [(0, 0, layerSize.width, layerSize.height, 0)];

        var (sx, sy) = strength;
        for (var i = 0; i < areas.Length; i++)
        {
            var (x, y, w, h, c) = areas[i];
            SetShaderData((x, y, w, h), (0, 1), Color.Transparent, true);
            SetShaderData((x, y, w, h), (1, 1), new(c), false);
            SetShaderData((x, y, w, h), (2, 1), new(sx, sy, 0, 0), false);
        }
    }
    public void Wave((sbyte x, sbyte y) speed, (byte x, byte y) frequency, params AreaToColor[]? areas)
    {
        if (areas == null || areas.Length == 0)
            areas = [(0, 0, layerSize.width, layerSize.height, 0)];

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
    public void ClearAll()
    {
        data?.Clear(Color.Transparent);
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

    private Light effectLight;
    private bool drawShaderDataOnce;
    private int lightCount, obstacleCount;
    private SizeI layerSize;
    private SizeI atlasTileSize;

    [DoNotSave]
    internal readonly Shader? shader;
    [DoNotSave]
    private readonly VertexArray? shaderParams = new(PrimitiveType.Points);
    [DoNotSave]
    internal RenderTexture? data;

    [DoNotSave]
    private static Texture? white;

    private void SetShaderData(AreaF area, PointI tilePixel, Color dataInColor, bool includeArea)
    {
        drawShaderDataOnce = true;

        var (x, y, w, h) = area;
        var (ix, iy) = ((int)x, (int)y);
        var (px, py) = tilePixel;

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
                var (vx, vy) = (tx * atlasTileSize.width, ty * atlasTileSize.height);

                shaderParams?.Append(new(new(vx + px, vy + 0.5f + py), dataInColor, new()));

                if (includeArea)
                    shaderParams?.Append(new(new(vx + px, vy + 0.5f + py), res, new()));
            }
    }
    internal void UpdateShader(SizeI size, SizeI tileSize)
    {
        layerSize = size;
        atlasTileSize = tileSize;
        var pixelSize = new Vector2u((uint)(size.width * tileSize.width), (uint)(size.height * tileSize.height));

        if (data == null || data.Size != pixelSize)
        {
            data?.Dispose();
            data = null;
            data = new(pixelSize.X, pixelSize.Y);
        }

        if (drawShaderDataOnce && data != null && shaderParams != null)
        {
            drawShaderDataOnce = false;
            data.Draw(shaderParams, new(BlendMode.None, Transform.Identity, white, null));
            data.Display();
        }

        lightCount = 0;
        obstacleCount = 0;
        shader?.SetUniform("tileSize", new Vec2(tileSize.width, tileSize.height));
        shader?.SetUniform("tileCount", new Vec2(size.width, size.height));
        shader?.SetUniform("time", Window.time.ElapsedTime.AsSeconds());
        shader?.SetUniform("data", data?.Texture);
        shaderParams?.Clear();
    }
#endregion
}