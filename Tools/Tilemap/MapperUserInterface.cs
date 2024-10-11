using Pure.Engine.Window;
using Pure.Engine.Tilemap;
using Pure.Engine.UserInterface;
using Pure.Engine.Utilities;
using static Pure.Engine.Tilemap.Tile;
using static Pure.Engine.Utilities.Color;
using Maps = Pure.Engine.Tilemap.TilemapPack;
using static Pure.Engine.Window.Keyboard;

namespace Pure.Tools.Tilemap;

public static class MapperUserInterface
{
    public static bool IsInteractable { get; set; } = true;

    public static (Tile corner, Tile edge, Tile fill, uint textTint) ThemeButtonBox { get; set; }
    public static (Tile edge, Tile fill, uint textTint) ThemeButtonBar { get; set; }
    public static (Tile background, Tile cursor, uint textTint, uint selectionTint) ThemeInputBox { get; set; }
    public static (Tile yes, Tile no) ThemeCheckbox { get; set; }
    public static (Tile edge, Tile fill, Tile handle) ThemeSlider { get; set; }
    public static Tile ThemeScrollArrow { get; set; }

    public static void SetCheckbox(this Maps maps, Button checkbox, int zOrder = 1)
    {
        if (maps.Tilemaps.Count <= zOrder)
            return;

        var (yes, no) = ThemeCheckbox;
        var tile = checkbox.IsSelected ? yes : no;
        var textPos = (checkbox.X + 2, checkbox.Y);

        tile.Tint = checkbox.GetInteractionColor(tile.Tint);

        Clear(maps, checkbox, zOrder);
        maps.Tilemaps[zOrder].SetTile(checkbox.Position, tile, checkbox.Mask);
        maps.Tilemaps[zOrder].SetText(textPos, checkbox.Text, tile.Tint, mask: checkbox.Mask);
    }
    public static void SetButton(this Maps maps, Button button, int zOrder = 1)
    {
        if (maps.Tilemaps.Count <= zOrder + 1)
            return;

        var (w, h) = button.Size;
        var offsetW = w / 2 - Math.Min(button.Text.Length, h == 1 ? w : w - 2) / 2;
        var (bCorner, bEdge, bFill, bTextTint) = ThemeButtonBox;
        var (rEdge, rFill, rTextTint) = ThemeButtonBar;
        var text = button.Text.Shorten(h == 1 ? w : w - 2);
        var textPos = (button.X + offsetW, button.Y + h / 2);
        var isBar = button.Height == 1;

        bCorner.Tint = button.GetInteractionColor(bCorner.Tint);
        bEdge.Tint = button.GetInteractionColor(bEdge.Tint);
        bFill.Tint = button.GetInteractionColor(bFill.Tint);
        bTextTint = button.GetInteractionColor(bTextTint);

        rEdge.Tint = button.GetInteractionColor(rEdge.Tint);
        rFill.Tint = button.GetInteractionColor(rFill.Tint);
        rTextTint = button.GetInteractionColor(rTextTint);

        Clear(maps, button, zOrder);

        if (isBar)
            maps.Tilemaps[zOrder].SetBar(button.Position, rEdge, rFill, button.Width, mask: button.Mask);
        else
            maps.Tilemaps[zOrder].SetBox(button.Area, bFill, bCorner, bEdge, button.Mask);

        maps.Tilemaps[zOrder + 1].SetText(textPos, text, isBar ? rTextTint : bTextTint, mask: button.Mask);
    }
    public static void SetButtonSelect(this Maps maps, Button button, int zOrder = 0)
    {
        var b = button;
        var (w, h) = b.Size;
        var offsetW = w / 2 - Math.Min(b.Text.Length, w - 2) / 2;
        var selectColor = b.IsSelected ? Green : Gray;

        Clear(maps, button, zOrder);
        // maps.Tilemaps[zOrder].SetBar(b.Position, BAR_BIG_EDGE, FULL,
        //     GetInteractionColor(b, Brown.ToDark(0.3f)), w, mask: b.Mask);
        // maps.Tilemaps[zOrder + 1].SetText(
        //     (b.Position.x + offsetW, b.Position.y + h / 2),
        //     b.Text.Shorten(w - 2),
        //     GetInteractionColor(b, selectColor),
        //     mask: b.Mask);
    }
    public static void SetButtonIcon(this Maps maps, Button button, Tile icon, int zOrder = 0)
    {
        var b = button;
        icon.Tint = GetInteractionColor(b, icon.Tint);
        Clear(maps, button, zOrder);
        maps.Tilemaps[zOrder].SetTile(b.Position, icon, b.Mask);
    }
    public static void SetInputBox(this Maps maps, InputBox inputBox, int zOrder = 0)
    {
        if (maps.Tilemaps.Count <= zOrder + 2)
            return;

        var box = inputBox;
        var (background, cursor, textTint, selectionTint) = ThemeInputBox;
        var selectColor = box.IsFocused ? new(selectionTint) : new Color(selectionTint).ToBright();
        var selection = box.Selection.Constrain(box.Size, false);
        var text = box.Text.Constrain(box.Size, false);
        var placeholder = box.Placeholder.Constrain(box.Size);
        var scrollY = box.ScrollIndices.y;
        var textAboveOrBelow = new Tile(background.Id, new Color(background.Tint).ToDark(0.3f));
        var (w, h) = box.Size;
        var cursorPos = box.PositionFromIndices(box.CursorIndices);
        var placeholderTint = new Color(textTint).ToDark(0.4f);

        background.Tint = box.GetInteractionColor(background.Tint, 0.05f);

        Clear(maps, inputBox, zOrder);
        maps.Tilemaps[zOrder].SetArea(box.Area, box.Mask, background);
        maps.Tilemaps[zOrder].SetText(box.Position, selection, selectColor, mask: box.Mask);
        maps.Tilemaps[zOrder + 1].SetText(box.Position, text, textTint, mask: box.Mask);

        if (string.IsNullOrEmpty(box.Value))
            maps.Tilemaps[zOrder + 1].SetText(box.Position, placeholder, placeholderTint, mask: box.Mask);

        if (scrollY > 0)
            maps.Tilemaps[zOrder + 0].SetArea((box.X, box.Y, w, 1), null, textAboveOrBelow);

        if (scrollY < box.LineCount - box.Height)
            maps.Tilemaps[zOrder + 0].SetArea((box.X, box.Y + h - 1, w, 1), null, textAboveOrBelow);

        if (box.IsCursorVisible)
            maps.Tilemaps[zOrder + 2].SetTile(cursorPos, cursor, box.Mask);
    }
    public static void SetFileViewerItem(this Maps maps, FileViewer fileViewer, Button item, int zOrder = 1)
    {
        var color = item.IsSelected ? Green : Gray.ToBright();
        var (x, y) = item.Position;
        var isFolder = fileViewer.IsFolder(item);
        var isHardDrive = fileViewer.HardDrives.Items.Contains(item);
        var icon = new Tile(ICON_FOLDER, GetInteractionColor(item, Yellow));
        var isRight = isHardDrive || item == fileViewer.Back || item == fileViewer.User;

        if (isHardDrive)
        {
            color = Gray.ToBright(0.2f);
            icon = new(ICON_HOME, GetInteractionColor(item, color));
        }
        else if (item == fileViewer.Back)
        {
            color = Gray.ToBright(0.2f);
            icon = new(ICON_BACK, GetInteractionColor(item, color));
        }
        else if (item == fileViewer.User)
        {
            color = Gray.ToBright(0.2f);
            icon = new(ICON_PERSON, GetInteractionColor(item, color));
        }
        else if (isFolder == false)
        {
            var id = ICON_FILE;
            var file = item.Text;
            var iconColor = Gray.ToBright();

            if (Ext(".png") ||
                Ext(".jpg") ||
                Ext(".bmp") ||
                Ext(".jpeg") ||
                Ext(".svg") ||
                Ext(".gif") ||
                Ext(".psd") ||
                Ext(".tif") ||
                Ext(".tiff") ||
                Ext(".webp") ||
                Ext(".pdf") ||
                Ext(".ico"))
            {
                id = ICON_PICTURE;
                iconColor = Cyan;
            }
            else if (Ext(".wav") ||
                     Ext(".ogg") ||
                     Ext(".flac") ||
                     Ext(".mp3") ||
                     Ext(".aiff") ||
                     Ext(".aac") ||
                     Ext(".mid") ||
                     Ext(".cda") ||
                     Ext(".mpa") ||
                     Ext(".wma"))
            {
                id = AUDIO_NOTES_BEAMED_EIGHT;
                iconColor = Purple.ToBright(0.35f);
            }
            else if (Ext(".ttf") || Ext(".otf"))
            {
                id = UPPERCASE_F;
                iconColor = Gray.ToDark();
            }
            else if (Ext(".txt") || Ext(".xml") || Ext(".json") || Ext(".log") || Ext(".csv"))
            {
                id = ALIGN_HORIZONTAL_LEFT;
                iconColor = Azure;
            }
            else if (Ext(".zip") ||
                     Ext(".rar") ||
                     Ext(".7z") ||
                     Ext(".arj") ||
                     Ext(".deb") ||
                     Ext(".pkg") ||
                     Ext(".tar.gz") ||
                     Ext(".z"))
            {
                id = ICON_STACK_2;
                iconColor = Brown;
            }
            else if (Ext(".mp4") || Ext(".avi") || Ext(".flv") || Ext(".mkv"))
            {
                id = ICON_CAMERA_MOVIE;
                iconColor = Red;
            }
            else if (Ext(".dll") || Ext(".cfg") || Ext(".ini"))
            {
                id = ICON_SETTINGS;
                iconColor = Yellow.ToDark();
            }
            else if (Ext(".exe") || Ext(".bin") || Ext(".bat") || Ext(".jar") || Ext(".msi"))
            {
                id = FLOW_PLAY;
                iconColor = Orange;
            }

            icon = new(id, GetInteractionColor(item, iconColor));

            bool Ext(string ext)
            {
                return Path.GetExtension(file).ToLower() == ext;
            }
        }

        maps.Tilemaps[zOrder].SetTile((x, y), icon, item.Mask);
        maps.Tilemaps[zOrder].SetText(
            (x + 1, y),
            item.Text.Shorten(isRight ? -fileViewer.Size.width + 1 : item.Size.width - 1),
            GetInteractionColor(item, color),
            mask: item.Mask);
    }
    public static void SetFileViewer(this Maps maps, FileViewer fileViewer, int zOrder = 0)
    {
        Clear(maps, fileViewer, zOrder);
        SetBackground(maps.Tilemaps[zOrder], fileViewer);

        if (fileViewer.FilesAndFolders.Scroll.IsHidden == false)
            SetScroll(maps, fileViewer.FilesAndFolders.Scroll, zOrder);

        SetFileViewerItem(maps, fileViewer, fileViewer.User, zOrder + 1);
        SetFileViewerItem(maps, fileViewer, fileViewer.Back, zOrder + 1);
    }
    public static void SetSlider(this Maps maps, Slider slider, int zOrder = 0)
    {
        if (maps.Tilemaps.Count <= zOrder + 2)
            return;

        var (edge, fill, handle) = ThemeSlider;
        var size = slider.IsVertical ? slider.Height : slider.Width;

        edge.Tint = slider.GetInteractionColor(edge.Tint, 0.3f);
        fill.Tint = slider.GetInteractionColor(fill.Tint, 0.3f);
        handle.Tint = slider.Handle.GetInteractionColor(handle.Tint, 0.3f);

        Clear(maps, slider, zOrder);
        maps.Tilemaps[zOrder].SetBar(slider.Position, edge, fill, size, slider.IsVertical, slider.Mask);
        maps.Tilemaps[zOrder + 1].SetTile(slider.Handle.Position, handle, slider.Mask);
    }
    public static void SetScroll(this Maps maps, Scroll scroll, int zOrder = 0)
    {
        if (maps.Tilemaps.Count <= zOrder + 1)
            return;

        var arrow = ThemeScrollArrow;
        var scrollUpAngle = (sbyte)(scroll.IsVertical ? 1 : 0);
        var scrollDownAngle = (sbyte)(scroll.IsVertical ? 3 : 2);
        var up = scroll.Increase.Position;
        var down = scroll.Decrease.Position;

        var upTint = scroll.Increase.GetInteractionColor(arrow.Tint, 0.3f);
        var downTint = scroll.Decrease.GetInteractionColor(arrow.Tint, 0.3f);

        Clear(maps, scroll, zOrder);
        maps.SetSlider(scroll.Slider, zOrder);
        maps.Tilemaps[zOrder + 1].SetTile(up, new(arrow.Id, upTint, scrollUpAngle), scroll.Mask);
        maps.Tilemaps[zOrder + 1].SetTile(down, new(arrow.Id, downTint, scrollDownAngle), scroll.Mask);
    }
    public static void SetStepper(this Maps maps, Stepper stepper, int zOrder = 0)
    {
        var s = stepper;
        var (x, y) = s.Position;
        var (w, h) = s.Size;
        var text = MathF.Round(s.Step, 2).Precision() == 0 ? $"{s.Value}" : $"{s.Value:F2}";
        var color = Gray;
        var maxTextSize = Math.Min(w - 3, s.Text.Length);

        Clear(maps, s, zOrder);
        SetBackground(maps.Tilemaps[zOrder], stepper);

        maps.Tilemaps[zOrder + 1].SetTile(
            s.Decrease.Position,
            new(ARROW_TAILLESS_ROUND, GetInteractionColor(s.Decrease, color), 1),
            s.Mask);
        maps.Tilemaps[zOrder + 1].SetTile(
            s.Increase.Position,
            new(ARROW_TAILLESS_ROUND, GetInteractionColor(s.Increase, color), 3),
            s.Mask);
        maps.Tilemaps[zOrder + 1].SetText(
            (x + (int)MathF.Ceiling(w / 2f - maxTextSize / 2f), y),
            s.Text.Shorten(maxTextSize),
            Gray,
            mask: s.Mask);
        maps.Tilemaps[zOrder + 1].SetText(
            (x + 2, y + 1),
            text.Constrain((Math.Max(w - 5, text.Length), Math.Max(h - 2, 1)),
                alignment: Alignment.Left),
            mask: s.Mask);

        maps.Tilemaps[zOrder + 1].SetTile(
            s.Minimum.Position,
            new(MATH_MUCH_LESS, GetInteractionColor(s.Minimum, color)),
            s.Mask);
        maps.Tilemaps[zOrder + 1].SetTile(
            s.Middle.Position,
            new(PUNCTUATION_PIPE, GetInteractionColor(s.Middle, color)),
            s.Mask);
        maps.Tilemaps[zOrder + 1].SetTile(
            s.Maximum.Position,
            new(MATH_MUCH_GREATER, GetInteractionColor(s.Maximum, color)),
            s.Mask);
    }
    public static void SetPrompt(this Maps maps, Prompt prompt, int zOrder = 0)
    {
        if (prompt.IsHidden == false)
        {
            var tile = new Tile(FULL, new Color(0, 0, 0, 127));
            var gray = Gray.ToDark(0.6f);
            maps.Tilemaps[zOrder].SetArea((0, 0, maps.Size.width, maps.Size.height), prompt.Mask, tile);
            maps.Tilemaps[zOrder + 1].SetBox(prompt.Area,
                new(FULL, Gray.ToDark(0.6f)), new(BOX_CORNER_ROUND, gray), new(FULL, gray), prompt.Mask);
        }

        var lines = prompt.Text.Split(Environment.NewLine).Length;
        maps.Tilemaps[zOrder + 2].SetText(
            prompt.Position,
            prompt.Text.Constrain((prompt.Size.width, lines), alignment: Alignment.Center),
            mask: prompt.Mask);
    }
    public static void SetPromptItem(this Maps maps, Prompt prompt, Button item, int zOrder = 2, params Tile[]? tiles)
    {
        var index = prompt.IndexOf(item);
        var tile = new Tile(
            index == 0 ? ICON_TICK : ICON_CANCEL,
            GetInteractionColor(item, index == 0 ? Green : Red));

        if (tiles is { Length: > 0 } && index < tiles.Length)
        {
            var curTile = tiles[index];
            curTile.Tint = GetInteractionColor(item, curTile.Tint);
            tile = curTile;
        }

        maps.Tilemaps[zOrder].SetTile(item.Position, tile, item.Mask);
    }
    public static void SetPanel(this Maps maps, Panel panel, int zOrder = 0)
    {
        var p = panel;
        var (x, y) = p.Position;
        var (w, _) = p.Size;

        Clear(maps, p, zOrder);
        SetBackground(maps.Tilemaps[zOrder], p, 0.6f);

        maps.Tilemaps[zOrder + 1].SetBox(
            p.Area, EMPTY, new(BOX_GRID_CORNER, Blue), new(BOX_GRID_STRAIGHT, Blue), p.Mask);
        maps.Tilemaps[zOrder + 1].SetText(
            (x + w / 2 - p.Text.Length / 2, y),
            p.Text.Shorten(Math.Min(w, p.Text.Length)),
            mask: p.Mask);
    }
    public static void SetPalette(this Maps maps, Palette palette, int zOrder = 0)
    {
        var p = palette;
        var color = new Color(p.SelectedColor) { A = 255 };

        Clear(maps, p, zOrder);
        maps.Tilemaps[zOrder].SetArea(
            p.Area,
            tiles: new Tile(SHADE_5, Gray.ToDark()),
            mask: p.Mask);

        maps.Tilemaps[zOrder].SetTile(p.Pick.Position, EMPTY);
        if (p.Pick.IsHidden == false)
            maps.Tilemaps[zOrder + 1].SetTile(p.Pick.Position,
                new(ICON_PICK, GetInteractionColor(p.Pick, Gray)), p.Mask);

        var opVert = p.Opacity.IsVertical;
        var opMax = opVert ? p.Opacity.Height : p.Opacity.Width;
        for (var i = 0; i < opMax; i++)
        {
            var curTile = new Tile(i.Map((0, opMax), (0, 11)), color);
            var (offX, offY) = opVert ? (0, i) : (i, 0);
            maps.Tilemaps[zOrder].SetTile((p.Opacity.X + offX, p.Opacity.Y + offY), curTile, p.Mask);
        }

        var brVert = p.Brightness.IsVertical;
        var brMax = brVert ? p.Brightness.Height : p.Brightness.Width;
        for (var i = 0; i < brMax; i++)
        {
            var col = new Color((byte)i.Map((0, brMax - 1), (0, 255)));
            var (offX, offY) = brVert ? (0, i) : (i, 0);
            maps.Tilemaps[zOrder].SetTile((p.Brightness.X + offX, p.Brightness.Y + offY), new(FULL, col), p.Mask);
        }
    }
    public static void SetPages(this Maps maps, Pages pages, int zOrder = 0)
    {
        var p = pages;

        Clear(maps, p, zOrder);
        SetBackground(maps.Tilemaps[zOrder], p);
        Button(p.First, MATH_MUCH_LESS, GetInteractionColor(p.First, Red));
        Button(p.Previous, MATH_LESS, GetInteractionColor(p.Previous, Yellow));
        Button(p.Next, MATH_GREATER, GetInteractionColor(p.Next, Yellow));
        Button(p.Last, MATH_MUCH_GREATER, GetInteractionColor(p.Last, Red));

        return;

        void Button(Button button, int tileId, Color color)
        {
            if (button.IsHidden)
                return;

            // maps.Tilemaps[zOrder].SetBar(button.Position, BAR_BIG_EDGE, FULL, color.ToDark(0.75f),
            //     button.Size.height, true, p.Mask);
            // maps.Tilemaps[zOrder + 1].SetTile(
            //     (button.Position.x, button.Position.y + button.Size.height / 2),
            //     new(tileId, color), p.Mask);
        }
    }
    public static void SetPagesItem(this Maps maps, Pages pages, Button item, int zOrder = 1)
    {
        var color = GetInteractionColor(item, item.IsSelected ? Green : Gray.ToBright(0.2f));
        var text = item.Text.ToNumber().PadZeros(-pages.ItemWidth);

        SetBackground(maps.Tilemaps[zOrder], item, 0.33f);
        maps.Tilemaps[zOrder + 1].SetText(item.Position,
            text.Constrain(item.Size, alignment: Alignment.Center),
            color,
            mask: item.Mask);
    }
    public static void SetPagesIcon(this Maps maps, Button item, int tileId, int zOrder = 1)
    {
        var color = GetInteractionColor(item, item.IsSelected ? Green : Gray.ToBright(0.2f));
        SetBackground(maps.Tilemaps[zOrder], item, 0.33f);
        maps.Tilemaps[zOrder + 1].SetTile(item.Position, new(tileId + int.Parse(item.Text), color),
            item.Mask);
    }
    public static void SetList(this Maps maps, List list, int zOrder = 0)
    {
        Clear(maps, list, zOrder);
        maps.Tilemaps[zOrder].SetArea(
            list.Area,
            tiles: new Tile(FULL, Gray.ToDark()),
            mask: list.Mask);

        if (list.IsScrollAvailable)
            SetScroll(maps, list.Scroll, zOrder + 1);

        if (list.IsCollapsed)
            maps.Tilemaps[zOrder + 2].SetTile(
                (list.Position.x + list.Size.width - 1, list.Position.y),
                new(MATH_GREATER, GetInteractionColor(list, Gray.ToBright()), 1),
                list.Mask);
    }
    public static void SetListItem(this Maps maps, List list, Button item, int zOrder = 1, bool showSelected = true)
    {
        var color = item.IsSelected && showSelected ? Green : Gray.ToBright(0.3f);
        var (x, y) = item.Position;
        var (_, h) = item.Size;
        var isLeftCrop =
            list.Span == Span.Horizontal &&
            item.Size.width < list.ItemSize.width &&
            item.Position == list.Position;

        color = item.IsDisabled ? Gray : color;

        SetBackground(maps.Tilemaps[zOrder], item, 0.25f);
        maps.Tilemaps[zOrder + 1].SetText(
            (x, y + h / 2),
            item.Text.Shorten(item.Size.width * (isLeftCrop ? -1 : 1)),
            GetInteractionColor(item, color),
            mask: item.Mask);
    }
    public static void SetLayoutSegment(this Maps maps, (int x, int y, int width, int height) segment, int index, bool showIndex, int zOrder = 0)
    {
        var color = new Color(
            (byte)(20, 200).Random(seed / (index + 1f)),
            (byte)(20, 200).Random(seed / (index + 2f)),
            (byte)(20, 200).Random(seed / (index + 3f)));

        maps.Tilemaps[zOrder].SetBox(
            segment, new(FULL, color), new(BOX_CORNER_ROUND, color), new(FULL, color), segment);

        if (showIndex)
            maps.Tilemaps[zOrder + 1].SetText(
                (segment.x, segment.y), index.ToString().Constrain((segment.width, segment.height),
                    alignment: Alignment.Center), mask: segment);
    }

    public static Color GetInteractionColor(this Block block, Color baseColor, float amount = 0.5f)
    {
        var hotkeyIsPressed = block is Button btn &&
                              ((Key)btn.Hotkey.id).IsPressed() &&
                              KeysPressed.Length == 1;

        if (block.IsDisabled || IsInteractable == false) return baseColor;
        if (block.IsPressedAndHeld || hotkeyIsPressed) return baseColor.ToDark(amount);
        else if (block.IsHovered) return baseColor.ToBright(amount);

        return baseColor;
    }

#region Backend
    private static readonly int seed;

    static MapperUserInterface()
    {
        seed = (-1_000_000, 1_000_000).Random();

        ThemeButtonBox = (new(BOX_CORNER_ROUND, Gray), new(FULL, Gray), new(FULL, Gray), Gray.ToBright());
        ThemeButtonBar = (new(BAR_BIG_EDGE, Gray), new(FULL, Gray), Gray.ToBright());
        ThemeInputBox = (new(FULL, Gray.ToDark(0.4f)), new(SHAPE_LINE, White, 2), Gray.ToBright(), Blue);
        ThemeCheckbox = (new(ICON_TICK, Green), new(ICON_X, Red));
        ThemeSlider = (new(BAR_BIG_EDGE, Gray), new(BAR_BIG_STRAIGHT, Gray), new(SHAPE_CIRCLE_BIG, Gray.ToBright()));
        ThemeScrollArrow = new(ARROW_TAILLESS, Gray);
    }

    private static void SetBackground(Pure.Engine.Tilemap.Tilemap map, Block block, float shade = 0.5f)
    {
        var e = block;
        var color = Gray.ToDark(shade);
        var tile = new Tile(FULL, color);

        map.SetBox(e.Area, tile, new(BOX_CORNER_ROUND, color), new(FULL, color), block.Mask);
    }
    private static void Clear(Maps maps, Block block, int zOrder)
    {
        var (x, y) = block.Position;
        var (w, h) = block.Size;

        for (var i = zOrder; i < zOrder + 3; i++)
            if (i < maps.Tilemaps.Count)
                maps.Tilemaps[i].SetArea((x, y, w, h), block.Mask, EMPTY);
    }
#endregion
}