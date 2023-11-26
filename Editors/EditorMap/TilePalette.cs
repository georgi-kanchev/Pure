using Pure.Engine.Collision;

namespace Pure.Editors.EditorMap;

internal class TilePalette
{
    public const float ZOOM_DEFAULT = 3.8f;

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
        layer = new(map.ViewSize) { Zoom = ZOOM_DEFAULT, Offset = (755, 340) };
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
        var tool = inspector.tools.Current;
        var (sx, sy) = selected.Position;
        sx += map.ViewPosition.x;
        sy += map.ViewPosition.y;
        var (sw, sh) = selected.Size;
        var tile = map.TileAt(((int)sx, (int)sy));
        var color = new Color(tile.Tint) { A = 200 };
        var seed = ((int)mx, (int)my).ToSeed() + clickSeed;
        var selectedTiles = map.TilesIn((sx, sy, sw, sh));
        var tiles = selectedTiles.Flatten();
        var randomTile = tiles.ChooseOne(seed);
        var index = drawLayer.Length != 1 ? -1 : inspector.layers.IndexOf(drawLayer[0]);
        var tilemap = index == -1 ? null : editor.MapsEditor[index];
        var (szw, szh) = (end.x - start.x, end.y - start.y);
        tile.Tint = color;

        if (Mouse.IsButtonPressed(Mouse.Button.Left).Once("lmb-press-paint"))
        {
            start = ((int)mx, (int)my);
            end = start;

            if (drawLayer.Length != 1)
            {
                Program.PromptMessage("Select a single layer to draw on.");
                return;
            }

            if (tilemap != null)
                tilemap.SeedOffset = (0, 0, clickSeed);
        }

        if (tool == 1) // group of tiles
            editor.LayerMap.DrawTiles(((int)mx, (int)my), tile, ((int)sw, (int)sh));
        else if (tool == 2) // single random tile of tiles
            editor.LayerMap.DrawTiles(((int)mx, (int)my), randomTile);
        else if (tool == 3) // rectangle of random tiles
            editor.LayerMap.DrawRectangles((start.x, start.y, szw, szh, color));

        if (Mouse.IsButtonPressed(Mouse.Button.Left))
        {
            end = ((int)mx + 1, (int)my + 1);

            var pos = ((int)mx, (int)my);

            if (tool == 1) // group of tiles
                tilemap?.SetGroup(pos, selectedTiles);
            else if (tool == 2) // single random tile of tiles
                tilemap?.SetTile(pos, randomTile);
        }

        if (tool == 3 && // rectangle of random tiles
            (Mouse.IsButtonPressed(Mouse.Button.Left) == false).Once("lmb-release-square"))
            tilemap?.SetRectangle((start.x, start.y, szw, szh), selectedTiles.Flatten());

        if ((Mouse.IsButtonPressed(Mouse.Button.Left) == false).Once("lmb-release-paint"))
            start = end;
    }

#region Backend
    private Inspector? inspector;
    private (int x, int y) prevMousePos;
    private readonly Editor editor;
    private (float x, float y) selectedPos;
    private (float w, float h) selectedSz = (1, 1);
    private static int clickSeed;

    private Rectangle selected;
    private (int x, int y) start, end;

    static TilePalette()
    {
        Mouse.OnButtonPress(Mouse.Button.Left, () => clickSeed++);
    }
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

        if (inspector is { tools.Current: 6 or 7 }) // fill or replace
            selectedSz = (1, 1);
    }
#endregion
}