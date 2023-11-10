namespace Pure.Editors.EditorMap;

internal class TilePalette
{
    public Tilemap map;
    public Layer layer;
    public (int x, int y) mousePos;

    public TilePalette(Editor editor)
    {
        this.editor = editor;
        Create((26, 26));
    }

    [MemberNotNull(nameof(map), nameof(layer))]
    public void Create((int width, int height) size)
    {
        map = new(size) { ViewSize = (10, 10) };
        layer = new(map.ViewSize) { Zoom = 3.8f, Offset = (755, 340) };

        for (var i = 0; i < size.height; i++)
            for (var j = 0; j < size.width; j++)
                map.SetTile((j, i), new Indices(i, j).ToIndex(size.width));
    }

    public void Update(Inspector inspector)
    {
        if (editor.Prompt.IsHidden == false)
            return;

        layer.Clear();

        var (tw, th) = layer.TilesetSize;
        map.ViewSize = (Math.Min(10, tw), Math.Min(10, th));
        layer.TilemapSize = map.ViewSize;
        var (mw, mh) = map.Size;
        var (vw, vh) = map.ViewSize;
        inspector.paletteScrollH.Step = 1f / (mw - vw);
        inspector.paletteScrollV.Step = 1f / (mh - vh);
        var w = (int)MathF.Round(inspector.paletteScrollH.Slider.Progress * (mw - vw));
        var h = (int)MathF.Round(inspector.paletteScrollV.Slider.Progress * (mh - vh));
        map.ViewPosition = (w, h);

        var (mx, my) = layer.PixelToWorld(Mouse.CursorPosition);
        mousePos = ((int)mx + w, (int)my + h);

        var view = map.ViewUpdate();
        layer.DrawTilemap(view);

        var isHovering = Mouse.IsHovering(layer);
        if (isHovering)
            layer.DrawTile(
                position: ((int)mx, (int)my),
                tile: new Tile(layer.TileIdFull, new Color(50, 100, 255, 150)));

        Window.DrawLayer(layer);
    }

#region Backend
    private readonly Editor editor;
#endregion
}