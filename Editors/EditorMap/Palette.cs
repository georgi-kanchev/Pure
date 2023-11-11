using Pure.Engine.Collision;

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

        Mouse.OnButtonPress(Mouse.Button.Left, () =>
        {
            if (Program.menu.IsHidden == false ||
                editor.Prompt.IsHidden == false ||
                inspector is { IsHovered: false } ||
                Mouse.IsHovering(layer) == false)
                return;

            var pos = layer.PixelToWorld(Mouse.CursorPosition);
            var (vx, vy) = map.ViewPosition;
            selectedPos = ((int)pos.x + vx, (int)pos.y + vy);
            selectedSz = (1, 1);
        });
    }
    [MemberNotNull(nameof(map), nameof(layer))]
    public void Create((int width, int height) size)
    {
        map = new(size) { ViewSize = (10, 10) };
        layer = new(map.ViewSize) { Zoom = 3.8f, Offset = (755, 340) };
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

        for (var i = 0; i < mh; i++)
            for (var j = 0; j < mw; j++)
            {
                var id = new Indices(i, j).ToIndex(mw);
                var tile = new Tile(id, inspector.paletteColor.SelectedColor);
                map.SetTile((j, i), tile);
            }

        this.inspector = inspector;
        inspector.paletteScrollH.Step = 1f / (mw - vw);
        inspector.paletteScrollV.Step = 1f / (mh - vh);
        var w = (int)MathF.Round(inspector.paletteScrollH.Slider.Progress * (mw - vw));
        var h = (int)MathF.Round(inspector.paletteScrollV.Slider.Progress * (mh - vh));
        map.ViewPosition = (w, h);

        var (mx, my) = layer.PixelToWorld(Mouse.CursorPosition);
        prevMousePos = mousePos;
        mousePos = ((int)mx + w, (int)my + h);

        var view = map.ViewUpdate();
        layer.DrawTilemap(view);

        if (Mouse.IsHovering(layer))
            layer.DrawTiles(
                position: ((int)mx, (int)my),
                tile: new Tile(layer.TileIdFull, new Color(50, 100, 255, 100)));

        UpdateSelected();
        var s = selected.ToBundle();
        layer.DrawRectangles((s.x, s.y, s.width, s.height, new Color(50, 255, 100, 100)));

        Window.DrawLayer(layer);
    }

    public void TryDraw()
    {
        if (Mouse.IsHovering(editor.LayerMap) == false ||
            inspector == null || inspector.IsHovered ||
            editor.Prompt.IsHidden == false ||
            Program.menu.IsHidden == false)
            return;

        var (mx, my) = editor.MousePositionWorld;
        var drawLayer = inspector.layers.ItemsSelected;
        var (sx, sy) = selected.Position;
        sx += map.ViewPosition.x;
        sy += map.ViewPosition.y;
        var (sw, sh) = selected.Size;
        var tile = map.TileAt(((int)sx, (int)sy));
        var color = new Color(tile.Tint) { A = 200 };
        tile.Tint = color;
        editor.LayerMap.DrawTiles(((int)mx, (int)my), tile, ((int)sw, (int)sh));

        if (Mouse.IsButtonPressed(Mouse.Button.Left) == false)
            return;

        if (drawLayer.Length != 1)
        {
            Program.PromptMessage("Select one layer to draw on.");
            return;
        }

        var index = inspector.layers.IndexOf(drawLayer[0]);
        editor.MapsEditor[index].SetGroup(((int)mx, (int)my), map.TilesIn((sx, sy, sw, sh)));
    }

#region Backend
    private Inspector? inspector;
    private (int x, int y) prevMousePos;
    private readonly Editor editor;
    private (float x, float y) selectedPos;
    private (float w, float h) selectedSz = (1, 1);

    private Rectangle selected;

    private void UpdateSelected()
    {
        if (Mouse.IsHovering(layer) &&
            Mouse.IsButtonPressed(Mouse.Button.Left) &&
            prevMousePos != mousePos)
        {
            var (szx, szy) = (mousePos.x - selectedPos.x, mousePos.y - selectedPos.y);
            selectedSz = (szx + (szx < 0 ? -1 : 1), szy + (szy < 0 ? -1 : 1));
        }

        var (sx, sy) = selectedPos;
        var (sw, sh) = selectedSz;
        var (ox, oy) = (sw < 0 ? 1 : 0, sh < 0 ? 1 : 0);
        var (vx, vy) = map.ViewPosition;
        selected = new((sw, sh), (sx + ox - vx, sy + oy - vy));
    }
#endregion
}