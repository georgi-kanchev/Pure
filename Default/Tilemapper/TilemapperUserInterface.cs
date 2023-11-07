namespace Pure.Default.Tilemapper;

using Engine.Tilemap;
using Engine.UserInterface;
using Engine.Utilities;

public static class TilemapperUserInterface
{
    public static bool IsInteractable
    {
        get;
        set;
    } = true;

    public static void SetCheckbox(this TilemapPack maps, Button checkbox, int zOrder = 0)
    {
        var color = checkbox.IsSelected ? Color.Green : Color.Red;
        var tileId = checkbox.IsSelected ? Tile.ICON_TICK : Tile.UPPERCASE_X;
        var tile = new Tile(tileId, GetInteractionColor(checkbox, color));

        Clear(maps, checkbox, zOrder);
        maps[zOrder].SetTile(checkbox.Position, tile);
        maps[zOrder].SetTextLine(
            position: (checkbox.Position.x + 2, checkbox.Position.y),
            text: checkbox.Text,
            tint: GetInteractionColor(checkbox, color));
    }
    public static void SetButton(
        this TilemapPack maps,
        Button button,
        int zOrder = 0,
        bool isDisplayingSelection = false)
    {
        var b = button;
        var (w, h) = b.Size;
        var offsetW = w / 2 - Math.Min(b.Text.Length, h == 1 ? w : w - 2) / 2;
        var c = b.IsSelected && isDisplayingSelection ? Color.Green : Color.Yellow;
        var color = GetInteractionColor(b, c.ToDark());
        var colorBack = Color.Gray.ToDark(0.6f);

        Clear(maps, button, zOrder);
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
        maps[zOrder + 1].SetTextLine(
            position: (b.Position.x + offsetW, b.Position.y + h / 2),
            text: b.Text,
            tint: color,
            maxLength: h == 1 ? w : w - 2);
    }
    public static void SetButtonSelect(this TilemapPack maps, Button button, int zOrder = 0)
    {
        var b = button;
        var (w, h) = b.Size;
        var offsetW = w / 2 - Math.Min(b.Text.Length, w - 2) / 2;
        var selColor = b.IsSelected ? Color.Green : Color.Gray;

        Clear(maps, button, zOrder);
        maps[zOrder].SetBar(b.Position,
            tileIdEdge: Tile.BAR_BIG_EDGE,
            tileId: Tile.SHADE_OPAQUE,
            tint: GetInteractionColor(b, Color.Brown.ToDark(0.3f)),
            size: w);
        maps[zOrder + 1].SetTextLine(
            position: (b.Position.x + offsetW, b.Position.y + h / 2),
            text: b.Text,
            tint: GetInteractionColor(b, selColor),
            maxLength: w - 2);
    }
    public static void SetButtonIcon(this TilemapPack maps, Button button, Tile icon, int zOrder = 0)
    {
        var b = button;
        icon.Tint = GetInteractionColor(b, icon.Tint);
        Clear(maps, button, zOrder);
        maps[zOrder].SetTile(b.Position, icon);
    }
    public static void SetInputBox(this TilemapPack maps, InputBox inputBox, int zOrder = 0)
    {
        var ib = inputBox;
        var bgColor = Color.Gray.ToDark(0.4f);
        var selectColor = ib.IsFocused ? Color.Blue : Color.Blue.ToBright();

        Clear(maps, inputBox, zOrder);
        maps[zOrder].SetRectangle(ib.Position, ib.Size, new(Tile.SHADE_OPAQUE, bgColor));
        maps[zOrder].SetTextRectangle(ib.Position, ib.Size, ib.Selection, selectColor, false);
        maps[zOrder + 1].SetTextRectangle(ib.Position, ib.Size, ib.Text, isWordWrapping: false);

        if (string.IsNullOrEmpty(ib.Value))
            maps[zOrder + 1].SetTextRectangle(ib.Position, ib.Size, ib.Placeholder,
                tint: Color.Gray,
                alignment: Alignment.TopLeft);

        if (ib.IsCursorVisible)
            maps[zOrder + 2].SetTile(ib.PositionFromIndices(ib.CursorIndices),
                new(Tile.SHAPE_LINE, Color.White, 2));
    }
    public static void SetFileViewerItem(
        this TilemapPack maps,
        FileViewer fileViewer,
        Button item,
        int zOrder = 1)
    {
        var color = item.IsSelected ? Color.Green : Color.Gray.ToBright();
        var (x, y) = item.Position;
        var icon = fileViewer.IsFolder(item) ?
            new Tile(Tile.ICON_FOLDER, GetInteractionColor(item, Color.Yellow)) :
            new(Tile.ICON_FILE, GetInteractionColor(item, Color.Gray.ToBright()));

        maps[zOrder].SetTile((x, y), icon);
        maps[zOrder].SetTextLine(
            position: (x + 1, y),
            item.Text,
            GetInteractionColor(item, color),
            maxLength: item.Size.width - 1);
    }
    public static void SetFileViewer(this TilemapPack maps, FileViewer fileViewer, int zOrder = 0)
    {
        var f = fileViewer;
        var color = GetInteractionColor(f.Back, Color.Gray);
        var (x, y) = f.Back.Position;

        Clear(maps, f, zOrder);
        SetBackground(maps[zOrder], f);

        if (f.FilesAndFolders.Scroll.IsHidden == false)
            SetScroll(maps, f.FilesAndFolders.Scroll, zOrder);

        maps[zOrder + 1].SetTile((x, y), new(Tile.ICON_BACK, color));
        maps[zOrder + 1].SetTextLine(
            position: (x + 1, y),
            text: f.CurrentDirectory,
            color,
            maxLength: -f.Back.Size.width + 1);
    }
    public static void SetSlider(this TilemapPack maps, Slider slider, int zOrder = 0)
    {
        var s = slider;
        var (x, y) = s.Position;
        var (w, h) = s.Size;
        var text = $"{s.Progress * 100f:F0}%";
        var isHandle = s.Handle.IsPressedAndHeld;
        var color = GetInteractionColor(isHandle ? s.Handle : s, Color.Gray.ToBright());

        Clear(maps, s, zOrder);
        SetBackground(maps[zOrder], s);
        maps[zOrder + 1].SetBar(s.Handle.Position,
            tileIdEdge: Tile.BAR_DEFAULT_EDGE,
            tileId: Tile.BAR_DEFAULT_STRAIGHT,
            color,
            size: s.IsVertical ? w : h,
            isVertical: s.IsVertical == false);
        maps[zOrder + 2].SetTextLine(
            position: (x + w / 2 - text.Length / 2, y + h / 2),
            text);
    }
    public static void SetScroll(this TilemapPack maps, Scroll scroll, int zOrder = 0)
    {
        var s = scroll;
        var scrollUpAng = (sbyte)(s.IsVertical ? 1 : 0);
        var scrollDownAng = (sbyte)(s.IsVertical ? 3 : 2);
        var scrollColor = Color.Gray.ToBright();
        var isHandle = s.Slider.Handle.IsPressedAndHeld;

        Clear(maps, s, zOrder);
        SetBackground(maps[zOrder], s, 0.4f);
        maps[zOrder + 1].SetTile(s.Increase.Position,
            new(Tile.ARROW, GetInteractionColor(s.Increase, scrollColor), scrollUpAng));
        maps[zOrder + 1].SetTile(s.Slider.Handle.Position,
            new(Tile.SHAPE_CIRCLE,
                GetInteractionColor(isHandle ? s.Slider.Handle : s.Slider, scrollColor)));
        maps[zOrder + 1].SetTile(s.Decrease.Position,
            new(Tile.ARROW, GetInteractionColor(s.Decrease, scrollColor), scrollDownAng));
    }
    public static void SetStepper(this TilemapPack maps, Stepper stepper, int zOrder = 0)
    {
        var s = stepper;
        var (x, y) = s.Position;
        var (w, h) = s.Size;
        var text = MathF.Round(s.Step, 2).Precision() == 0 ? $"{s.Value}" : $"{s.Value:F2}";
        var color = Color.Gray;
        var maxTextSize = Math.Min(w - 3, s.Text.Length);

        Clear(maps, s, zOrder);
        SetBackground(maps[zOrder], stepper);

        maps[zOrder + 1].SetTile(
            position: s.Decrease.Position,
            tile: new(Tile.ARROW_NO_TAIL, GetInteractionColor(s.Decrease, color), angle: 1));
        maps[zOrder + 1].SetTile(
            s.Increase.Position,
            tile: new(Tile.ARROW_NO_TAIL, GetInteractionColor(s.Increase, color), angle: 3));
        maps[zOrder + 1].SetTextLine(
            position: (x + (int)MathF.Ceiling(w / 2f - maxTextSize / 2f), y),
            s.Text,
            Color.Gray,
            maxTextSize);
        maps[zOrder + 1].SetTextRectangle(
            position: (x + 2, y + 1),
            size: (Math.Max(w - 5, text.Length), Math.Max(h - 2, 1)),
            text,
            alignment: Alignment.Left);

        maps[zOrder + 1].SetTile(
            position: s.Minimum.Position,
            tile: new(Tile.MATH_MUCH_LESS, GetInteractionColor(s.Minimum, color)));
        maps[zOrder + 1].SetTile(
            position: s.Middle.Position,
            tile: new(Tile.PUNCTUATION_PIPE, GetInteractionColor(s.Middle, color)));
        maps[zOrder + 1].SetTile(
            position: s.Maximum.Position,
            tile: new(Tile.MATH_MUCH_GREATER, GetInteractionColor(s.Maximum, color)));
    }
    public static void SetPrompt(this TilemapPack maps, Prompt prompt, int zOrder = 0)
    {
        if (prompt.IsHidden == false)
        {
            var tile = new Tile(Tile.SHADE_OPAQUE, new Color(0, 0, 0, 127));
            maps[zOrder].SetRectangle((0, 0), maps.Size, tile);
            maps[zOrder + 1].SetBox(prompt.Position, prompt.Size,
                tileFill: new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.6f)),
                cornerTileId: Tile.BOX_CORNER_ROUND,
                borderTileId: Tile.SHADE_OPAQUE,
                borderTint: Color.Gray.ToDark(0.6f));
        }

        var lines = prompt.Text.Split(Environment.NewLine).Length;
        maps[zOrder + 2].SetTextRectangle(
            prompt.Position,
            size: (prompt.Size.width, lines),
            prompt.Text,
            alignment: Alignment.Center);
    }
    public static void SetPromptItem(this TilemapPack maps, Prompt prompt, Button item, int zOrder = 2)
    {
        var tile = new Tile(Tile.ICON_TICK, GetInteractionColor(item, Color.Green));
        if (prompt.IndexOf(item) == 1)
        {
            tile.Id = Tile.ICON_CANCEL;
            tile.Tint = GetInteractionColor(item, Color.Red);
        }

        maps[zOrder].SetTile(item.Position, tile);
    }
    public static void SetPanel(this TilemapPack maps, Panel panel, int zOrder = 0)
    {
        var p = panel;
        var (x, y) = p.Position;
        var (w, _) = p.Size;

        Clear(maps, p, zOrder);
        SetBackground(maps[zOrder], p, 0.6f);

        maps[zOrder + 1].SetBox(p.Position, p.Size, Tile.EMPTY, Tile.BOX_GRID_CORNER,
            Tile.BOX_GRID_STRAIGHT, Color.Blue);
        maps[zOrder + 1].SetTextLine(
            position: (x + w / 2 - p.Text.Length / 2, y),
            p.Text,
            maxLength: Math.Min(w, p.Text.Length));
    }
    public static void SetPalette(this TilemapPack maps, Palette palette, int zOrder = 0)
    {
        var p = palette;
        var tile = new Tile(Tile.SHADE_OPAQUE, GetInteractionColor(p.Opacity, Color.Gray.ToBright()));

        Clear(maps, p, zOrder);
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

        maps[zOrder + 1].SetTile(p.Pick.Position,
            new(Tile.MATH_PLUS, GetInteractionColor(p.Pick, Color.Gray)));
    }
    public static void SetPages(this TilemapPack maps, Pages pages, int zOrder = 0)
    {
        var p = pages;

        Clear(maps, p, zOrder);
        SetBackground(maps[zOrder], p);
        Button(p.First, Tile.MATH_MUCH_LESS, GetInteractionColor(p.First, Color.Red));
        Button(p.Previous, Tile.MATH_LESS, GetInteractionColor(p.Previous, Color.Yellow));
        Button(p.Next, Tile.MATH_GREATER, GetInteractionColor(p.Next, Color.Yellow));
        Button(p.Last, Tile.MATH_MUCH_GREATER, GetInteractionColor(p.Last, Color.Red));

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
    public static void SetPagesItem(this TilemapPack maps, Pages pages, Button item, int zOrder = 1)
    {
        var color = GetInteractionColor(item, item.IsSelected ? Color.Green : Color.Gray.ToBright(0.2f));
        var text = item.Text.ToNumber().PadZeros(-pages.ItemWidth);
        SetBackground(maps[zOrder], item, 0.33f);
        maps[zOrder + 1]
            .SetTextRectangle(item.Position, item.Size, text, color, alignment: Alignment.Center);
    }
    public static void SetPagesIcon(this TilemapPack maps, Button item, int tileId, int zOrder = 1)
    {
        var color = GetInteractionColor(item, item.IsSelected ? Color.Green : Color.Gray.ToBright(0.2f));
        SetBackground(maps[zOrder], item, 0.33f);
        maps[zOrder + 1].SetTile(
            item.Position,
            tile: new(tileId + int.Parse(item.Text), color));
    }
    public static void SetList(this TilemapPack maps, List list, int zOrder = 0)
    {
        Clear(maps, list, zOrder);
        maps[zOrder].SetRectangle(
            list.Position,
            list.Size,
            tile: new(Tile.SHADE_OPAQUE, Color.Gray.ToDark()));

        if (list.Scroll.IsHidden == false)
            SetScroll(maps, list.Scroll, zOrder + 1);

        if (list.IsCollapsed)
            maps[zOrder + 2].SetTile(
                position: (list.Position.x + list.Size.width - 1, list.Position.y),
                tile: new(Tile.MATH_GREATER, GetInteractionColor(list, Color.Gray.ToBright()),
                    angle: 1));
    }
    public static void SetListItem(this TilemapPack maps, List list, Button item, int zOrder = 1)
    {
        var color = item.IsSelected ? Color.Green : Color.Gray.ToBright(0.3f);
        var (x, y) = item.Position;
        var (_, h) = item.Size;
        var isLeftCrop =
            list.Span == Span.Horizontal &&
            item.Size.width < list.ItemSize.width &&
            item.Position == list.Position;

        SetBackground(maps[zOrder], item, 0.25f);
        maps[zOrder + 1].SetTextLine(
            position: (x, y + h / 2),
            item.Text,
            GetInteractionColor(item, color),
            maxLength: item.Size.width * (isLeftCrop ? -1 : 1));
    }
    public static void SetLayoutSegment(
        this TilemapPack maps,
        (int x, int y, int width, int height) segment,
        int index,
        bool isIndexVisible,
        int zOrder = 0)
    {
        var color = new Color(
            (byte)(20, 200).Random(seed / (index + 1f)),
            (byte)(20, 200).Random(seed / (index + 2f)),
            (byte)(20, 200).Random(seed / (index + 3f)));

        maps[zOrder].SetBox(
            position: (segment.x, segment.y),
            size: (segment.width, segment.height),
            tileFill: new(Tile.SHADE_OPAQUE, color),
            cornerTileId: Tile.BOX_CORNER_ROUND,
            borderTileId: Tile.SHADE_OPAQUE,
            borderTint: color);

        if (isIndexVisible)
            maps[zOrder + 1].SetTextRectangle(
                position: (segment.x, segment.y),
                size: (segment.width, segment.height),
                text: index.ToString(),
                alignment: Alignment.Center);
    }

    public static Color GetInteractionColor(this Block block, Color baseColor)
    {
        if (block.IsDisabled || IsInteractable == false) return baseColor;
        if (block.IsPressedAndHeld) return baseColor.ToDark();
        else if (block.IsHovered) return baseColor.ToBright();

        return baseColor;
    }

#region Backend
    private static int seed;

    static TilemapperUserInterface()
    {
        seed = (-1_000_000, 1_000_000).Random();
    }

    private static void SetBackground(Tilemap map, Block block, float shade = 0.5f)
    {
        var e = block;
        var color = Color.Gray.ToDark(shade);
        var tile = new Tile(Tile.SHADE_OPAQUE, color);

        map.SetBox(e.Position, e.Size, tile, Tile.BOX_CORNER_ROUND, Tile.SHADE_OPAQUE, color);
    }
    private static void Clear(TilemapPack maps, Block block, int zOrder)
    {
        for (var i = zOrder; i < zOrder + 3; i++)
            if (i < maps.Count)
                maps[i].SetRectangle(block.Position, block.Size, Tile.SHADE_TRANSPARENT);
    }
#endregion
}