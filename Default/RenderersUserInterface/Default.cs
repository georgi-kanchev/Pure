namespace Pure.Default.RendererUserInterface;

using Engine.Tilemap;
using Engine.UserInterface;
using Engine.Utilities;

public static class Default
{
    public static void SetCheckbox(TilemapPack maps, Button checkbox, int zOrder)
    {
        var color = checkbox.IsSelected ? Color.Green : Color.Red;
        var tileId = checkbox.IsSelected ? Tile.ICON_TICK : Tile.UPPERCASE_X;
        var tile = new Tile(tileId, GetColor(checkbox, color));

        maps[zOrder].SetTile(checkbox.Position, tile);

        maps[zOrder].SetTextLine(
            position: (checkbox.Position.x + 2, checkbox.Position.y),
            text: checkbox.Text,
            tint: GetColor(checkbox, color));
    }
    public static void SetButton(
        TilemapPack maps,
        Button button,
        int zOrder,
        bool isDisplayingSelection = false)
    {
        var b = button;

        var (w, h) = b.Size;
        var offsetW = w / 2 - Math.Min(b.Text.Length, w - 2) / 2;
        var c = b.IsSelected && isDisplayingSelection ? Color.Green : Color.Yellow;
        var color = GetColor(b, c.ToDark());
        var colorBack = Color.Gray.ToDark(0.6f);

        maps[zOrder].SetBox(b.Position, b.Size,
            tileFill: new(Tile.SHADE_OPAQUE, colorBack),
            cornerTileId: Tile.BOX_CORNER_ROUND,
            borderTileId: Tile.SHADE_OPAQUE,
            borderTint: colorBack);

        maps[zOrder + 1].SetBox(b.Position, b.Size,
            tileFill: Tile.EMPTY,
            cornerTileId: Tile.BOX_DEFAULT_CORNER,
            borderTileId: Tile.BOX_DEFAULT_STRAIGHT,
            borderTint: color);

        maps[zOrder + 2].SetTextLine(
            position: (b.Position.x + offsetW, b.Position.y + h / 2),
            text: b.Text,
            tint: color,
            maxLength: w - 2);
    }
    public static void SetButtonSelect(TilemapPack maps, Button button, int zOrder)
    {
        var b = button;
        var (w, h) = b.Size;
        var offsetW = w / 2 - Math.Min(b.Text.Length, w - 2) / 2;
        var selColor = b.IsSelected ? Color.Green : Color.Gray;

        maps[zOrder].SetBar(b.Position,
            tileIdEdge: Tile.BAR_BIG_EDGE,
            tileId: Tile.SHADE_OPAQUE,
            tint: GetColor(b, Color.Brown.ToDark(0.3f)),
            size: w);
        maps[zOrder + 1].SetTextLine(
            position: (b.Position.x + offsetW, b.Position.y + h / 2),
            text: b.Text,
            tint: GetColor(b, selColor),
            maxLength: w - 2);
    }
    public static void SetInputBox(TilemapPack maps, InputBox inputBox, int zOrder)
    {
        var ib = inputBox;
        var bgColor = Color.Gray.ToDark(0.4f);
        var selectColor = ib.IsFocused ? Color.Blue : Color.Blue.ToBright();

        Clear(maps, inputBox, (zOrder, zOrder + 2));
        maps[zOrder].SetRectangle(ib.Position, ib.Size, new(Tile.SHADE_OPAQUE, bgColor));
        maps[zOrder].SetTextRectangle(ib.Position, ib.Size, ib.Selection, selectColor, false);
        maps[zOrder + 1].SetTextRectangle(ib.Position, ib.Size, ib.Text, isWordWrapping: false);

        if (string.IsNullOrWhiteSpace(ib.Value))
            maps[zOrder + 1].SetTextRectangle(ib.Position, ib.Size, ib.Placeholder,
                tint: Color.Gray.ToBright(),
                alignment: Tilemap.Alignment.TopLeft);

        if (ib.IsCursorVisible)
            maps[zOrder + 2].SetTile(ib.PositionFromIndices(ib.CursorIndices),
                new(Tile.SHAPE_LINE, Color.White, 2));
    }
    public static void SetFileViewerItem(
        TilemapPack maps,
        Button item,
        FileViewer fileViewer,
        int zOrder)
    {
        var color = item.IsSelected ? Color.Green : Color.Gray.ToBright();
        var (x, y) = item.Position;
        var icon = fileViewer.IsFolder(item) ?
            new Tile(Tile.ICON_FOLDER, GetColor(item, Color.Yellow)) :
            new(Tile.ICON_FILE, GetColor(item, Color.Gray.ToBright()));

        maps[zOrder].SetTile((x, y), icon);
        maps[zOrder].SetTextLine(
            position: (x + 1, y),
            item.Text,
            GetColor(item, color),
            maxLength: item.Size.width - 1);
    }
    public static void SetFileViewer(TilemapPack maps, FileViewer fileViewer, int zOrder)
    {
        var e = fileViewer;
        var color = GetColor(e.Back, Color.Gray);
        var (x, y) = e.Back.Position;
        var selected = e.SelectedPaths;
        var paths = "";

        SetBackground(maps[0], e);
        maps[zOrder].SetTextLine((e.Position.x, e.Position.y - 1), e.Text);

        if (e.FilesAndFolders.Scroll.IsHidden == false)
            SetScroll(maps, e.FilesAndFolders.Scroll, zOrder);

        maps[zOrder + 2].SetTile((x, y), new(Tile.ICON_BACK, color));
        maps[zOrder + 2].SetTextLine(
            position: (x + 1, y),
            text: e.CurrentDirectory,
            color,
            maxLength: -e.Back.Size.width + 1);

        foreach (var path in selected)
            paths += $"{Environment.NewLine}{Environment.NewLine}{path}";

        maps[zOrder].SetTextRectangle(
            position: (e.Position.x, e.Position.y + e.Size.height - 1),
            size: (e.Size.width, 20),
            paths);
    }
    public static void SetSlider(TilemapPack maps, Slider slider, int zOrder)
    {
        var e = slider;
        var (w, h) = e.Size;
        var text = e.IsVertical ? $"{e.Progress:F2}" : $"{e.Progress * 100f:F0}%";
        var isHandle = e.Handle.IsPressedAndHeld;
        var color = GetColor(isHandle ? e.Handle : e, Color.Gray.ToBright());

        SetBackground(maps[zOrder], e);
        maps[zOrder + 1].SetBar(e.Handle.Position,
            tileIdEdge: Tile.BAR_DEFAULT_EDGE,
            tileId: Tile.BAR_DEFAULT_STRAIGHT,
            color,
            size: e.Size.height,
            isVertical: e.IsVertical == false);
        maps[zOrder + 2].SetTextLine(
            position: (e.Position.x + w / 2 - text.Length / 2, e.Position.y + h / 2),
            text);
    }
    public static void SetScroll(TilemapPack maps, Scroll scroll, int zOrder)
    {
        var e = scroll;
        var scrollUpAng = (sbyte)(e.IsVertical ? 1 : 0);
        var scrollDownAng = (sbyte)(e.IsVertical ? 3 : 2);
        var scrollColor = Color.Gray.ToBright();
        var isHandle = e.Slider.Handle.IsPressedAndHeld;

        SetBackground(maps[zOrder], e, 0.4f);
        maps[zOrder + 1].SetTile(e.Increase.Position,
            new(Tile.ARROW, GetColor(e.Increase, scrollColor), scrollUpAng));
        maps[zOrder + 1].SetTile(e.Slider.Handle.Position,
            new(Tile.SHAPE_CIRCLE, GetColor(isHandle ? e.Slider.Handle : e.Slider, scrollColor)));
        maps[zOrder + 1].SetTile(e.Decrease.Position,
            new(Tile.ARROW, GetColor(e.Decrease, scrollColor), scrollDownAng));
    }
    public static void SetStepper(TilemapPack maps, Stepper stepper, int zOrder)
    {
        var e = stepper;
        var text = MathF.Round(e.Step, 2).Precision() == 0 ? $"{e.Value}" : $"{e.Value:F2}";
        var color = Color.Gray.ToBright();

        SetBackground(maps[zOrder], stepper);

        maps[zOrder + 1].SetTile(
            position: e.Decrease.Position,
            tile: new(Tile.ARROW, GetColor(e.Decrease, color), angle: 1));
        maps[zOrder + 1].SetTile(
            e.Increase.Position,
            tile: new(Tile.ARROW, GetColor(e.Increase, color), angle: 3));
        maps[zOrder + 1].SetTextLine(
            position: (e.Position.x + 2, e.Position.y),
            e.Text);
        maps[zOrder + 1].SetTextLine(
            position: (e.Position.x + 2, e.Position.y + 1),
            text);

        maps[zOrder + 1].SetTile(
            position: e.Minimum.Position,
            tile: new(Tile.MATH_MUCH_LESS, GetColor(e.Minimum, color)));
        maps[zOrder + 1].SetTile(
            position: e.Middle.Position,
            tile: new(Tile.PUNCTUATION_PIPE, GetColor(e.Middle, color)));
        maps[zOrder + 1].SetTile(
            position: e.Maximum.Position,
            tile: new(Tile.MATH_MUCH_GREATER, GetColor(e.Maximum, color)));
    }
    public static void SetPrompt(TilemapPack maps, Prompt prompt, Button[] buttons, int zOrder)
    {
        if (prompt.IsOpened)
        {
            var tile = new Tile(Tile.SHADE_OPAQUE, new Color(0, 0, 0, 127));
            maps[zOrder].SetRectangle((0, 0), maps.Size, tile);
            maps[zOrder + 1].SetBox(prompt.Position, prompt.Size,
                tileFill: new(Tile.SHADE_OPAQUE, Color.Gray.ToDark()),
                cornerTileId: Tile.BOX_CORNER_ROUND,
                borderTileId: Tile.SHADE_OPAQUE,
                borderTint: Color.Gray.ToDark());
        }

        var messageSize = (prompt.Size.width, prompt.Size.height - 1);
        maps[zOrder + 2].SetTextRectangle(prompt.Position, messageSize, prompt.Message,
            alignment: Tilemap.Alignment.Center);

        for (var i = 0; i < buttons.Length; i++)
        {
            var btn = buttons[i];
            var tile = new Tile(Tile.ICON_TICK, GetColor(btn, Color.Green));
            if (i == 1)
            {
                tile.Id = Tile.ICON_CANCEL;
                tile.Tint = GetColor(btn, Color.Red);
            }

            maps[zOrder + 3].SetTile(btn.Position, tile);
        }
    }
    public static void SetPanel(TilemapPack maps, Panel panel, int zOrder)
    {
        var e = panel;
        var (x, y) = e.Position;
        var (w, _) = e.Size;

        Clear(maps, panel, (zOrder, zOrder + 1));
        SetBackground(maps[zOrder], e, 0.6f);

        maps[zOrder + 1].SetBox(e.Position, e.Size, Tile.EMPTY, Tile.BOX_GRID_CORNER,
            Tile.BOX_GRID_STRAIGHT, Color.Blue);
        maps[zOrder + 1].SetTextLine(
            position: (x + w / 2 - e.Text.Length / 2, y),
            e.Text,
            maxLength: Math.Min(w, e.Text.Length));
    }
    public static void SetPalette(TilemapPack maps, Palette palette, int zOrder)
    {
        var p = palette;
        var tile = new Tile(Tile.SHADE_OPAQUE, GetColor(p.Opacity, Color.Gray.ToBright()));

        Clear(maps, p, zOrder: (zOrder, zOrder + 2));

        maps[zOrder].SetRectangle(
            position: p.Opacity.Position,
            size: p.Opacity.Size,
            tile: new(Tile.SHADE_5, Color.Gray.ToDark()));
        maps[zOrder + 1].SetBar(
            p.Opacity.Position,
            tileIdEdge: Tile.BAR_BIG_EDGE,
            tileId: Tile.BAR_BIG_STRAIGHT,
            p.SelectedColor,
            p.Opacity.Size.width);
        maps[zOrder + 1].SetTile(p.Opacity.Handle.Position, tile);

        maps[zOrder + 1].SetTile(p.Pick.Position, new(Tile.MATH_PLUS, GetColor(p.Pick, Color.Gray)));
    }
    public static void SetPages(TilemapPack maps, Pages pages, int zOrder)
    {
        var p = pages;

        SetBackground(maps[zOrder], p);
        Button(p.First, Tile.MATH_MUCH_LESS, GetColor(p.First, Color.Red));
        Button(p.Previous, Tile.MATH_LESS, GetColor(p.Previous, Color.Yellow));
        Button(p.Next, Tile.MATH_GREATER, GetColor(p.Next, Color.Yellow));
        Button(p.Last, Tile.MATH_MUCH_GREATER, GetColor(p.Last, Color.Red));

        return;

        void Button(Button button, int tileId, Color color)
        {
            maps[zOrder].SetBar(
                button.Position,
                tileIdEdge: Tile.BAR_BIG_EDGE,
                tileId: Tile.SHADE_OPAQUE,
                tint: color.ToDark(0.75f),
                button.Size.height,
                isVertical: true);
            maps[zOrder + 1].SetTile(
                position: (button.Position.x, button.Position.y + button.Size.height / 2),
                tile: new(tileId, color));
        }
    }
    public static void SetPagesItem(TilemapPack maps, Pages pages, Button page, int zOrder)
    {
        var color = GetColor(page, page.IsSelected ? Color.Green : Color.Gray.ToBright(0.2f));
        var text = page.Text.ToNumber().PadZeros(-pages.ItemWidth);
        maps[zOrder].SetTextLine(page.Position, text, color);
    }
    public static void SetPagesIcon(TilemapPack maps, Button page, int zOrder, int tileId)
    {
        var color = GetColor(page, page.IsSelected ? Color.Green : Color.Gray.ToBright(0.2f));
        maps[zOrder].SetTile(
            page.Position,
            tile: new(tileId + int.Parse(page.Text), color));
    }
    public static void SetList(TilemapPack maps, List list, int zOrder)
    {
        Clear(maps, list, (zOrder, zOrder + 2));

        maps[zOrder].SetRectangle(
            list.Position,
            list.Size,
            tile: new(Tile.SHADE_OPAQUE, Color.Gray.ToDark()));

        if (list.Scroll.IsHidden == false)
            SetScroll(maps, list.Scroll, zOrder + 1);

        if (list.IsCollapsed)
            maps[zOrder + 2].SetTile(
                position: (list.Position.x + list.Size.width - 1, list.Position.y),
                tile: new(Tile.MATH_GREATER, GetColor(list, Color.Gray.ToBright()), angle: 1));
    }
    public static void SetListItem(TilemapPack maps, List list, Button item, int zOrder)
    {
        var color = item.IsSelected ? Color.Green : Color.Gray.ToBright(0.3f);
        var (x, y) = item.Position;
        var (_, h) = item.Size;
        var isLeftCrop =
            list.Span == List.Spans.Horizontal &&
            item.Size.width < list.ItemSize.width &&
            item.Position == list.Position;

        SetBackground(maps[zOrder], item, 0.25f);
        maps[zOrder + 1].SetTextLine(
            position: (x, y + h / 2),
            item.Text,
            GetColor(item, color),
            maxLength: item.Size.width * (isLeftCrop ? -1 : 1));
    }
    public static void SetLayoutSegment(
        TilemapPack maps,
        (int x, int y, int width, int height) segment,
        int index,
        bool isIndexVisible)
    {
        var colors = new uint[]
        {
            Color.Red, Color.Blue, Color.Brown, Color.Violet, Color.Gray,
            Color.Orange, Color.Cyan, Color.Black, Color.Azure, Color.Purple,
            Color.Magenta, Color.Green, Color.Pink, Color.Yellow
        };
        maps[0].SetBox(
            position: (segment.x, segment.y),
            size: (segment.width, segment.height),
            tileFill: new(Tile.SHADE_OPAQUE, colors[index]),
            cornerTileId: Tile.BOX_CORNER_ROUND,
            borderTileId: Tile.SHADE_OPAQUE,
            borderTint: colors[index]);

        if (isIndexVisible)
            maps[1].SetTextRectangle(
                position: (segment.x, segment.y),
                size: (segment.width, segment.height),
                text: index.ToString(),
                alignment: Tilemap.Alignment.Center);
    }

#region Backend
    private static Color GetColor(Block block, Color baseColor)
    {
        if (block.IsDisabled) return baseColor;
        if (block.IsPressedAndHeld) return baseColor.ToDark();
        else if (block.IsHovered) return baseColor.ToBright();

        return baseColor;
    }
    private static void SetBackground(Tilemap map, Block block, float shade = 0.5f)
    {
        var e = block;
        var color = Color.Gray.ToDark(shade);
        var tile = new Tile(Tile.SHADE_OPAQUE, color);

        map.SetBox(e.Position, e.Size, tile, Tile.BOX_CORNER_ROUND, Tile.SHADE_OPAQUE, color);
    }
    private static void Clear(TilemapPack tilemaps, Block block, (int from, int to) zOrder)
    {
        for (var i = zOrder.from; i < zOrder.to; i++)
            tilemaps[i].SetRectangle(block.Position, block.Size, Tile.SHADE_TRANSPARENT);
    }
#endregion
}