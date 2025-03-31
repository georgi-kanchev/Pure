namespace Pure.Engine.Window;

public class LayerSprites(SizeI size)
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

    public PointF Position { get; set; }
    public SizeI Size { get; set; } = size;
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

    public PointF MouseCursorPosition
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

    public void DragAndZoom(Mouse.Button dragButton = Mouse.Button.Middle, float zoomDelta = 0.05f)
    {
        Window.TryCreate();

        var (dx, dy) = (Mouse.CursorDelta.x / Window.PixelScale, Mouse.CursorDelta.y / Window.PixelScale);
        if (Mouse.ScrollDelta != 0)
            Zoom *= Mouse.ScrollDelta > 0 ? 1f + zoomDelta : 1f - zoomDelta;
        if (dragButton.IsPressed())
            Position = (Position.x + dx / Zoom, Position.y + dy / Zoom);
    }

    public void DrawSprite(AreaI? textureArea = null, PointF position = default, float angle = 0f, SizeF? scale = null, PointF? origin = null, uint tint = uint.MaxValue)
    {
        textures.TryGetValue(texturePath ?? "", out var texture);

        if (texture == null)
            return;

        var color = new Color(tint);
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

        verts.Append(new(new(tl.X, tl.Y), color, new(x, y)));
        verts.Append(new(new(tr.X, tr.Y), color, new(x + w, y)));
        verts.Append(new(new(br.X, br.Y), color, new(x + w, y + h)));
        verts.Append(new(new(bl.X, bl.Y), color, new(x, y + h)));
    }

    public PointF PositionFromPixel(PointI pixel)
    {
        Window.TryCreate();

        var (px, py) = ((float)pixel.x, (float)pixel.y);
        var (vw, vh) = Window.rendTexViewSz;
        var (ww, wh, ow, oh) = Window.GetRenderOffset();

        px = Window.Map(px, ow, Window.Size.width - ow, 0, vw);
        py = Window.Map(py, oh, Window.Size.height - oh, 0, vh);

        px -= vw / 2f;
        py -= vh / 2f;

        px /= Zoom;
        py /= Zoom;

        px -= Position.x;
        py -= Position.y;

        return (px, py);
    }
    public PointI PositionToPixel(PointF position)
    {
        Window.TryCreate();

        var (px, py) = (position.x, position.y);
        var (vw, vh) = Window.rendTexViewSz;
        var (ww, wh, ow, oh) = Window.GetRenderOffset();

        px += Position.x;
        py += Position.y;

        px *= Zoom;
        py *= Zoom;

        px += vw / 2f;
        py += vh / 2f;

        px = Window.Map(px, 0, vw, ow, Window.Size.width - ow);
        py = Window.Map(py, 0, vh, oh, Window.Size.height - oh);

        return new((int)px, (int)py);
    }
    public PointF PositionToLayer(PointF position, LayerTiles layerTiles)
    {
        return layerTiles.PositionFromPixel(PositionToPixel(position));
    }
    public PointF PositionToLayer(PointF position, LayerSprites layerSprites)
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

        if (textures.TryGetValue(TexturePath ?? "", out var texture) == false)
            return;

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

        result?.Texture.CopyToImage().SaveToFile($"render-{GetHashCode()}.png");

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