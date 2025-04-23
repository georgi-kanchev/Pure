namespace Pure.Engine.Window;

public class LayerSprites
{
    public string? TexturePath
    {
        get => texturePath;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (textures.ContainsKey(value))
            {
                texturePath = value;
                return;
            }

            textures[value] = new(value) { Repeated = true };
            texturePath = value;
        }
    }
    public bool TextureIsRepeated
    {
        get
        {
            textures.TryGetValue(TexturePath ?? "", out var texture);
            return texture?.Repeated ?? false;
        }
        set
        {
            textures.TryGetValue(TexturePath ?? "", out var texture);
            if (texture != null)
                texture.Repeated = value;
        }
    }
    public bool TextureIsSmooth
    {
        get
        {
            textures.TryGetValue(TexturePath ?? "", out var texture);
            return texture?.Smooth ?? false;
        }
        set
        {
            textures.TryGetValue(TexturePath ?? "", out var texture);
            if (texture != null)
                texture.Smooth = value;
        }
    }
    public SizeI TextureSize
    {
        get
        {
            textures.TryGetValue(TexturePath ?? "", out var texture);
            return texture != null ? ((int)texture.Size.X, (int)texture.Size.Y) : (0, 0);
        }
    }

    public VecF Position { get; set; }
    public SizeI Size { get; set; }
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
            var (w, h) = (Size.width, Size.height);
            var (rw, rh) = ((float)mw / ww, (float)mh / wh);
            return Math.Min((float)ww / w * rw, (float)wh / h * rh) / Window.PixelScale;
        }
    }

    public VecF MouseCursorPosition
    {
        get => PositionFromPixel(Mouse.CursorPosition);
    }
    public uint BackgroundColor { get; set; }
    public Effect? Effect
    {
        get => effect;
        set
        {
            effect = value;
            effect?.UpdateShader((1, 1), Size);
        }
    }

    public LayerSprites(SizeI size = default, bool fitWindow = true)
    {
        Size = size;
        Zoom = 1f;

        if (fitWindow == false)
            return;

        Zoom = ZoomWindowFit;
        Position = (0, 0);
    }

    public void Fit()
    {
        Window.TryCreate();

        var (mw, mh) = Monitor.Current.Size;
        var (ww, wh) = Window.Size;
        var (w, h) = Size;
        var (rw, rh) = ((float)mw / ww, (float)mh / wh);
        var zoomFit = Math.Min((float)ww / w * rw, (float)wh / h * rh) / Window.PixelScale;

        Zoom = zoomFit;
        Position = default;
    }
    public void Fill()
    {
        Window.TryCreate();

        var (mw, mh) = Monitor.Current.Size;
        var (ww, wh) = Window.Size;
        var (w, h) = Size;
        var (rw, rh) = ((float)mw / ww, (float)mh / wh);
        var zoomFit = Math.Max((float)ww / w * rw, (float)wh / h * rh) / Window.PixelScale;

        Zoom = zoomFit;
        Position = default;
    }
    public void Align(VecF alignment)
    {
        Window.TryCreate();

        var (w, h) = Size;
        var halfW = w / 2f;
        var halfH = h / 2f;
        var rendW = Window.rendTexViewSz.w / 2f / Zoom;
        var rendH = Window.rendTexViewSz.h / 2f / Zoom;
        var x = Window.Map(alignment.x, 0, 1, -rendW + halfW, rendW - halfW);
        var y = Window.Map(alignment.y, 0, 1, -rendH + halfH, rendH - halfH);
        Position = (x, y);
    }
    public void DragAndZoom(Mouse.Button dragButton = Mouse.Button.Middle, float zoomDelta = 0.05f)
    {
        Window.TryCreate();

        var (_, _, rew, reh) = Window.GetRenderArea();
        var (mw, mh) = Monitor.Current.Size;
        var (rw, rh) = (mw / rew, mh / reh);
        var (dx, dy) = (Mouse.CursorDelta.x / Window.PixelScale * rw, Mouse.CursorDelta.y / Window.PixelScale * rh);

        if (Mouse.ScrollDelta != 0)
            Zoom *= Mouse.ScrollDelta > 0 ? 1f + zoomDelta : 1f - zoomDelta;
        if (dragButton.IsPressed())
            Position = (Position.x + dx / Zoom, Position.y + dy / Zoom);
    }

    public void DrawLine(VecF[]? points, float width = 4f, uint tint = uint.MaxValue, AreaI? textureArea = null)
    {
        if (points == null || points.Length == 0)
            return;

        if (points.Length == 1)
        {
            DrawSprite(textureArea, (points[0].x, points[0].y), 0f, (width, width), tint: tint);
            return;
        }

        for (var i = 1; i < points.Length; i++)
        {
            var (a, b) = (points[i - 1], points[i]);
            QueueLine((a.x, a.y), (b.x, b.y), tint, width, textureArea);
        }
    }
    public void DrawLines(Line[]? lines, float width = 4f, uint tint = uint.MaxValue, AreaI? textureArea = null)
    {
        if (lines == null || lines.Length == 0)
            return;

        foreach (var line in lines)
            QueueLine((line.ax, line.ay), (line.bx, line.by), tint, width, textureArea);
    }
    public void DrawSprite(AreaI? textureArea = null, VecF position = default, float angle = 0f, SizeF? scale = null, VecF? origin = null, uint tint = uint.MaxValue)
    {
        QueueRect(textureArea, position, angle, scale, origin, tint);
    }

    public bool IsOverlapping(VecF position)
    {
        return position is { x: >= 0, y: >= 0 } &&
               position.x <= Size.width &&
               position.y <= Size.height;
    }
    public VecF PositionFromPixel(VecI pixel)
    {
        Window.TryCreate();

        var (px, py) = ((float)pixel.x, (float)pixel.y);
        var (vw, vh) = Window.rendTexViewSz;
        var (rx, ry, _, _) = Window.GetRenderArea();

        px = Window.Map(px, rx, Window.Size.width - rx, 0, vw);
        py = Window.Map(py, ry, Window.Size.height - ry, 0, vh);

        px -= vw / 2f;
        py -= vh / 2f;

        px /= Zoom;
        py /= Zoom;

        px -= Position.x;
        py -= Position.y;

        return (px, py);
    }
    public VecI PositionToPixel(VecF position)
    {
        Window.TryCreate();

        var (px, py) = (position.x, position.y);
        var (vw, vh) = Window.rendTexViewSz;
        var (rx, ry, _, _) = Window.GetRenderArea();

        px += Position.x;
        py += Position.y;

        px *= Zoom;
        py *= Zoom;

        px += vw / 2f;
        py += vh / 2f;

        px = Window.Map(px, 0, vw, rx, Window.Size.width - rx);
        py = Window.Map(py, 0, vh, ry, Window.Size.height - ry);

        return new((int)px, (int)py);
    }
    public VecF PositionToLayer(VecF position, LayerTiles layerTiles)
    {
        return layerTiles.PositionFromPixel(PositionToPixel(position));
    }
    public VecF PositionToLayer(VecF position, LayerSprites layerSprites)
    {
        return layerSprites.PositionFromPixel(PositionToPixel(position));
    }

#region Backend
    private string? texturePath;
    private float zoom = 1f;

    [DoNotSave]
    private Effect? effect;
    [DoNotSave]
    internal RenderTexture? queue, result;
    [DoNotSave]
    internal static readonly Dictionary<string, Texture> textures = [];
    [DoNotSave]
    internal readonly VertexArray verts = new(PrimitiveType.Quads);

    private void QueueLine(VecF a, VecF b, uint tint, float width, AreaI? textureArea = null)
    {
        var dir = new Vector2(b.x - a.x, b.y - a.y);
        var rad = MathF.Atan2(dir.Y, dir.X);
        var ang = rad * (180f / MathF.PI) - 90f;
        var length = Vector2.Distance(new(a.x, a.y), new(b.x, b.y));

        textures.TryGetValue(TexturePath ?? "", out var texture);
        var h = textureArea == null ? texture?.Size.Y ?? 1 : 1;
        DrawSprite(textureArea, (a.x, a.y), ang, (width / 2f, length / h), (0.5f, 0f), tint);
    }
    public void QueueRect(AreaI? textureArea, VecF position, float angle, SizeF? scale, VecF? origin, uint tint)
    {
        textures.TryGetValue(texturePath ?? "", out var texture);
        texture ??= Window.white;

        if (texture == null)
            return;

        var m = Matrix3x2.Identity;
        var (x, y, w, h) = textureArea ?? (0, 0, (int)texture.Size.X, (int)texture.Size.Y);
        var (sw, sh) = (w * (scale?.width ?? 1f), h * (scale?.height ?? 1f));
        var (ox, oy) = origin ?? (0.5f, 0.5f);

        m *= Matrix3x2.CreateRotation(MathF.PI / 180f * angle);
        m *= Matrix3x2.CreateTranslation(position.x + Size.width / 2f, position.y + Size.height / 2f);

        var tl = Vector2.Transform(new(-ox * sw, -oy * sh), m);
        var tr = Vector2.Transform(new((-ox + 1) * sw, -oy * sh), m);
        var br = Vector2.Transform(new((-ox + 1) * sw, (-oy + 1) * sh), m);
        var bl = Vector2.Transform(new(-ox * sw, (-oy + 1) * sh), m);

        verts.Append(new(new(tl.X, tl.Y), new(tint), new(x, y)));
        verts.Append(new(new(tr.X, tr.Y), new(tint), new(x + w, y)));
        verts.Append(new(new(br.X, br.Y), new(tint), new(x + w, y + h)));
        verts.Append(new(new(bl.X, bl.Y), new(tint), new(x, y + h)));
    }

    internal void DrawQueue()
    {
        var pixelSize = new Vector2u((uint)Size.width, (uint)Size.height);
        if (queue == null || queue.Size != pixelSize)
        {
            queue?.Dispose();
            result?.Dispose();
            queue = null;
            result = null;
            queue = new(pixelSize.X, pixelSize.Y);
            result = new(pixelSize.X, pixelSize.Y);

            var view = queue.GetView();
            view.Center = new(view.Size.X / 2f, view.Size.Y / 2f);
            queue.SetView(view);
            result.SetView(view);
        }

        var texture = textures!.GetValueOrDefault(TexturePath ?? "", Window.white);
        var (w, h) = (queue?.Texture.Size.X ?? 0, queue?.Texture.Size.Y ?? 0);
        var r = new RenderStates(BlendMode.Alpha, Transform.Identity, queue?.Texture, Effect?.shader);

        Effect?.UpdateShader((1, 1), Size);

        queue?.Clear(new(BackgroundColor));
        queue?.Draw(verts, new(texture));
        queue?.Display();

        Window.verts[0] = new(new(0, 0), Color.White, new(0, 0));
        Window.verts[1] = new(new(w, 0), Color.White, new(w, 0));
        Window.verts[2] = new(new(w, h), Color.White, new(w, h));
        Window.verts[3] = new(new(0, h), Color.White, new(0, h));

        result?.Clear(new(Color.Transparent));
        result?.Draw(Window.verts, PrimitiveType.Quads, r);
        result?.Display();

        verts.Clear();

        var tr = Transform.Identity;
        tr.Translate(Position.x, Position.y);
        tr.Scale(Zoom, Zoom, -Position.x, -Position.y);

        var (resW, resH) = (result?.Texture.Size.X ?? 0, result?.Texture.Size.Y ?? 0);
        Window.verts[0] = new(new(-resW / 2f, -resH / 2f), Color.White, new(0, 0));
        Window.verts[1] = new(new(resW / 2f, -resH / 2f), Color.White, new(resW, 0));
        Window.verts[2] = new(new(resW / 2f, resH / 2f), Color.White, new(resW, resH));
        Window.verts[3] = new(new(-resW / 2f, resH / 2f), Color.White, new(0, resH));

        Window.renderResult?.Draw(Window.verts, PrimitiveType.Quads, new(BlendMode.Alpha, tr, result?.Texture, null));
        // Window.renderResult?.Draw(Window.verts, PrimitiveType.Quads, new(BlendMode.Alpha, tr, data?.Texture, null));
    }
#endregion
}