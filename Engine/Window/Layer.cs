namespace Pure.Engine.Window;

public class Layer
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

    public float Zoom
    {
        get => zoom;
        set => zoom = Math.Clamp(value, 0f, 1000f);
    }
    public PointF Position { get; set; }
    public PointF MouseCursorPosition
    {
        get => PositionFromPixel(Mouse.CursorPosition);
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

    public void DrawTextureArea(AreaI? area = null, PointF position = default, float angle = 0f, SizeF? scale = null, PointF? origin = null, uint tint = uint.MaxValue)
    {
        textures.TryGetValue(texturePath ?? "", out var texture);

        if (texture == null)
            return;

        var color = new Color(tint);
        var m = Matrix3x2.Identity;
        var (x, y, w, h) = area ?? (0, 0, (int)texture.Size.X, (int)texture.Size.Y);
        var (sw, sh) = scale ?? (w, h);
        var (ox, oy) = origin ?? (0.5f, 0.5f);

        m *= Matrix3x2.CreateRotation(MathF.PI / 180f * angle, new(ox * sw, oy * sh));
        m *= Matrix3x2.CreateTranslation(position.x, position.y);

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
    public PointF PositionToLayer(PointF position, Layer layer)
    {
        return layer.PositionFromPixel(PositionToPixel(position));
    }

#region Backend
    private string? texturePath;
    private float zoom = 1f;

    [DoNotSave]
    internal static readonly Dictionary<string, Texture> textures = [];
    [DoNotSave]
    internal readonly VertexArray verts = new(PrimitiveType.Quads);
#endregion
}