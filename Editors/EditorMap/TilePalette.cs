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

        if (tool is 1) // group of tiles
            editor.LayerMap.DrawTiles(((int)mx, (int)my), tile, ((int)sw, (int)sh));
        else if (tool is 2 or 7 or 8) // single random tile of tiles/replace/fill
            editor.LayerMap.DrawTiles(((int)mx, (int)my), randomTile);
        else if (tool is 3 or 5 or 6) // rectangle/ellipse of random tiles
            editor.LayerMap.DrawRectangles((start.x, start.y, szw, szh, color));
        else if (tool == 4 && start != end) // line of random tiles
            editor.LayerMap.DrawLines(
                (start.x, start.y, end.x - 1, end.y - 1, color),
                (start.x + 1, start.y, end.x, end.y - 1, color),
                (start.x + 1, start.y + 1, end.x, end.y, color),
                (start.x, start.y + 1, end.x - 1, end.y, color));

        if (Mouse.IsButtonPressed(Mouse.Button.Left))
        {
            end = ((int)mx + 1, (int)my + 1);

            var pos = ((int)mx, (int)my);

            if (tool == 1) // group of tiles
                tilemap?.SetGroup(pos, selectedTiles);
            else if (tool == 2) // single random tile of tiles
                tilemap?.SetTile(pos, randomTile);
        }

        if ((Mouse.IsButtonPressed(Mouse.Button.Left) == false).Once("lmb-release") == false)
            return;

        if (tool == 3) // rectangle of random tiles
            tilemap?.SetRectangle((start.x, start.y, szw, szh), tiles);
        else if (tool == 4) // line of random tiles
            tilemap?.SetLine(start, (end.x - 1, end.y - 1), tiles);
        else if (tool is 5 or 6) // ellipse of random tiles
        {
            var center = new Point(start).ToTarget((end.x - 1, end.y - 1), (0.5f, 0.5f));
            var radius = ((int)((end.x - start.x - 1) / 2f), (int)((end.y - start.y - 1) / 2f));

            tilemap?.SetEllipse(center, radius, tool == 5, tiles);
        }
        else if (tool == 7)
            tilemap?.Replace((0, 0), tilemap.Size, tilemap.TileAt(start), tiles);
        else if (tool == 8)
            tilemap?.Flood(((int)mx, (int)my), tiles);

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
        Mouse.OnButtonPress(Mouse.Button.Left, () =>
            clickSeed = (-10000, 10000).Random());
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
    }
#endregion
}