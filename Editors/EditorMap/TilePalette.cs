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

        Mouse.Button.Left.OnPress(() =>
        {
            if (Program.menu.IsHidden == false ||
                editor.Prompt.IsHidden == false ||
                inspector is { IsHovered: false } ||
                layer.IsHovered == false)
                return;

            var pos = layer.PixelToWorld(Mouse.CursorPosition);
            var (vx, vy) = map.ViewPosition;
            selectedPos = ((int)pos.x + vx, (int)pos.y + vy);
            selectedSz = (1, 1);
        });
        Mouse.Button.Left.OnPress(OnMousePressed);
        Mouse.Button.Left.OnRelease(OnMouseRelease);
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

        if (layer.IsHovered)
            layer.DrawTiles(((int)mx, (int)my),
                new Tile(layer.TileIdFull, new Color(50, 100, 255, 100)));

        UpdateSelected();
        var s = selected.ToBundle();
        layer.DrawRectangles((s.x, s.y, s.width, s.height, new Color(50, 255, 100, 100)));

        layer.Draw();
    }

    public void TryDraw()
    {
        if (IsPaintAllowed() == false)
            return;

        var (mx, my) = ((int)editor.MousePositionWorld.x, (int)editor.MousePositionWorld.y);
        var drawLayer = inspector.layers.ItemsSelected;
        var tool = inspector.tools.Current;
        var (sx, sy) = selected.Position;
        sx += map.ViewPosition.x;
        sy += map.ViewPosition.y;
        var (sw, sh) = selected.Size;
        var tile = map.TileAt(((int)sx, (int)sy));
        var color = new Color(tile.Tint) { A = 200 };
        var seed = (mx, my).ToSeed() + clickSeed;
        var tiles = GetSelectedTiles().Flatten();
        var randomTile = tiles.ChooseOne(seed);
        var index = drawLayer.Length != 1 ? -1 : inspector.layers.IndexOf(drawLayer[0]);
        var tilemap = index == -1 ? null : editor.MapsEditor[index];
        var (szw, szh) = (end.x - start.x, end.y - start.y);
        tile.Tint = color;

        if (tool is 1) // group of tiles
            editor.LayerMap.DrawTiles((mx, my), tile, ((int)sw, (int)sh));
        else if (tool is 2 or 7 or 8) // single random tile of tiles/replace/fill
            editor.LayerMap.DrawTiles((mx, my), randomTile);
        else if (rectangleTools.Contains(tool)) // rectangle/ellipse of random tiles
            editor.LayerMap.DrawRectangles((start.x, start.y, szw, szh, color));
        else if (tool == 4 && start != end) // line of random tiles
            editor.LayerMap.DrawLines(
                (start.x, start.y, end.x - 1, end.y - 1, color),
                (start.x + 1, start.y, end.x, end.y - 1, color),
                (start.x + 1, start.y + 1, end.x, end.y, color),
                (start.x, start.y + 1, end.x - 1, end.y, color));

        if (Mouse.Button.Left.IsPressed())
            OnMouseHold(randomTile, tilemap);
    }

#region Backend
    private readonly List<int> rectangleTools = new() { 3, 5, 6, 9, 10, 11, 12, 13 };
    private Inspector? inspector;
    private (int x, int y) prevMousePos;
    private readonly Editor editor;
    private (float x, float y) selectedPos;
    private (float w, float h) selectedSz = (1, 1);
    private static int clickSeed;

    private Solid selected;
    private (int x, int y) start, end;

    static TilePalette()
    {
        Mouse.Button.Left.OnRelease(() => clickSeed = (-10000, 10000).Random());
    }
    private void UpdateSelected()
    {
        if (layer.IsHovered &&
            Mouse.Button.Left.IsPressed() &&
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

    [MemberNotNullWhen(true, nameof(inspector))]
    private bool IsPaintAllowed()
    {
        return inspector is { IsHovered: false } &&
               editor.Prompt.IsHidden &&
               Program.menu.IsHidden &&
               editor.MapPanel.IsHovered == false;
    }
    private void OnMousePressed()
    {
        if (IsPaintAllowed() == false)
            return;

        start = ((int)editor.MousePositionWorld.x, (int)editor.MousePositionWorld.y);
        end = (start.x + 1, start.y + 1);

        if (inspector.layers.ItemsSelected.Length != 1)
        {
            editor.PromptMessage("Select a single layer to draw on.");
            return;
        }

        var tilemap = inspector.GetSelectedTilemap();
        if (tilemap != null)
            tilemap.SeedOffset = (0, 0, clickSeed);
    }
    private void OnMouseRelease()
    {
        if (inspector == null)
            return;

        var tilemap = inspector.GetSelectedTilemap();

        if (IsPaintAllowed() == false || tilemap == null)
            return;

        var (mx, my) = ((int)editor.MousePositionWorld.x, (int)editor.MousePositionWorld.y);
        var tool = inspector.tools.Current;
        var (szw, szh) = (end.x - start.x, end.y - start.y);
        var tiles = GetSelectedTiles().Flatten();

        if (rectangleTools.Contains(tool))
        {
            if (start.x > end.x)
                (start.x, end.x) = (end.x, start.x);
            if (start.y > end.y)
                (start.y, end.y) = (end.y, start.y);
        }

        if (tool == 3) // rectangle of random tiles
            tilemap.SetRectangle((start.x, start.y, Math.Abs(szw), Math.Abs(szh)), tiles);
        else if (tool == 4) // line of random tiles
            tilemap.SetLine(start, (end.x - 1, end.y - 1), tiles);
        else if (tool is 5 or 6) // ellipse of random tiles
        {
            var center = ((Point)start).ToTarget((end.x - 1, end.y - 1), (0.5f, 0.5f));
            var radius = ((int)((end.x - start.x - 1) / 2f), (int)((end.y - start.y - 1) / 2f));

            tilemap.SetEllipse(center, radius, tool == 5, tiles);
        }
        else if (tool == 7) // replace
            tilemap.Replace((0, 0), tilemap.Size, tilemap.TileAt(start), tiles);
        else if (tool == 8) // fill
            tilemap.Flood((mx, my), false, tiles);
        else if (tool == 9) // rotate
            ProcessRegion(tile =>
            {
                tile.Turns++;
                return tile;
            });
        else if (tool == 10) // mirror
            ProcessRegion(tile =>
            {
                tile.IsMirrored = tile.IsMirrored == false;
                return tile;
            });
        else if (tool == 11) // flip
            ProcessRegion(tile =>
            {
                tile.IsFlipped = tile.IsFlipped == false;
                return tile;
            });
        else if (tool == 12) // color
            ProcessRegion(tile =>
            {
                tile.Tint = inspector.paletteColor.SelectedColor;
                return tile;
            });
        else if (tool == 13) // pick
        {
            var tile = tilemap.TileAt((mx, my));
            var coords = Indices.FromIndex(tile.Id, layer.TilesetSize);
            inspector.paletteColor.SelectedColor = tile.Tint;
            selectedPos = coords;
            selectedSz = (1, 1);
        }

        start = end;

        void ProcessRegion(Func<Tile, Tile> editTile)
        {
            if (inspector == null)
                return;

            var region = tilemap.TilesIn((start.x, start.y, Math.Abs(szw), Math.Abs(szh)));
            for (var i = 0; i < region.GetLength(1); i++)
                for (var j = 0; j < region.GetLength(0); j++)
                    region[j, i] = editTile.Invoke(region[j, i]);

            tilemap.SetGroup(start, region);
        }
    }
    private void OnMouseHold(Tile randomTile, Tilemap? tilemap)
    {
        if (IsPaintAllowed() == false)
            return;

        var (mx, my) = ((int)editor.MousePositionWorld.x, (int)editor.MousePositionWorld.y);
        var pos = (mx, my);
        var tool = inspector.tools.Current;
        var lmb = Mouse.Button.Left.IsPressed();
        var (szw, szh) = (end.x - start.x, end.y - start.y);
        var (offX, offY) = (1, 1);

        if (rectangleTools.Contains(inspector.tools.Current))
        {
            offX = szw > 0 ? offX : 0;
            offY = szh > 0 ? offY : 0;
        }

        end = (mx + offX, my + offY);
        if (end.x - start.x == 0)
            end = (end.x + 1, end.y);
        if (end.y - start.y == 0)
            end = (end.x, end.y + 1);

        if (tool == 13) // pick
        {
            start = (mx, my);
            end = (mx + 1, my + 1);
        }

        if (lmb && tool == 1) // group of tiles
            tilemap?.SetGroup(pos, GetSelectedTiles());
        else if (lmb && tool == 2) // single random tile of tiles
            tilemap?.SetTile(pos, randomTile);
    }
    private Tile[,] GetSelectedTiles()
    {
        var (sx, sy) = selected.Position;
        sx += map.ViewPosition.x;
        sy += map.ViewPosition.y;
        var (sw, sh) = selected.Size;
        return map.TilesIn((sx, sy, sw, sh));
    }
#endregion
}