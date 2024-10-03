namespace Pure.Tools.Tilemapper;

using Engine.Window;
using Engine.Tilemap;
using Engine.UserInterface;
using Engine.Utilities;
using static Engine.Tilemap.Tile;
using static Engine.Utilities.Color;

public static class MapperUI
{
    public static bool IsInteractable { get; set; } = true;

    public static void SetCheckbox(this TilemapPack maps, Button checkbox, int zOrder = 0)
    {
        var color = checkbox.IsSelected ? Green : Red;
        var tileId = checkbox.IsSelected ? ICON_TICK : UPPERCASE_X;
        var tile = new Tile(tileId, GetInteractionColor(checkbox, color));

        Clear(maps, checkbox, zOrder);
        maps.Tilemaps[zOrder].SetTile(checkbox.Position, tile, checkbox.Mask);
        maps.Tilemaps[zOrder].SetText(
            (checkbox.Position.x + 2, checkbox.Position.y),
            checkbox.Text,
            GetInteractionColor(checkbox, color),
            mask: checkbox.Mask);
    }
    public static void SetButton(this TilemapPack maps, Button button, int zOrder = 0, bool isDisplayingSelection = false)
    {
        var b = button;
        var (w, h) = b.Size;
        var offsetW = w / 2 - Math.Min(b.Text.Length, h == 1 ? w : w - 2) / 2;
        var c = b.IsSelected && isDisplayingSelection ? Green : Yellow;
        var color = GetInteractionColor(b, c.ToDark());
        var colorBack = Gray.ToDark(0.6f);

        Clear(maps, button, zOrder);
        maps.Tilemaps[zOrder].SetBox(b.Area,
            new(SHADE_OPAQUE, colorBack),
            BOX_CORNER_ROUND,
            SHADE_OPAQUE,
            colorBack,
            b.Mask);
        maps.Tilemaps[zOrder + 1].SetBox(b.Area,
            EMPTY,
            BOX_DEFAULT_CORNER,
            BOX_DEFAULT_STRAIGHT,
            color,
            b.Mask);
        maps.Tilemaps[zOrder + 1].SetText(
            (b.Position.x + offsetW, b.Position.y + h / 2),
            b.Text.Shorten(h == 1 ? w : w - 2),
            color,
            mask: b.Mask);
    }
    public static void SetButtonSelect(this TilemapPack maps, Button button, int zOrder = 0)
    {
        var b = button;
        var (w, h) = b.Size;
        var offsetW = w / 2 - Math.Min(b.Text.Length, w - 2) / 2;
        var selectColor = b.IsSelected ? Green : Gray;

        Clear(maps, button, zOrder);
        maps.Tilemaps[zOrder].SetBar(b.Position,
            BAR_BIG_EDGE,
            SHADE_OPAQUE,
            GetInteractionColor(b, Brown.ToDark(0.3f)),
            w,
            mask: b.Mask);
        maps.Tilemaps[zOrder + 1].SetText(
            (b.Position.x + offsetW, b.Position.y + h / 2),
            b.Text.Shorten(w - 2),
            GetInteractionColor(b, selectColor),
            mask: b.Mask);
    }
    public static void SetButtonIcon(this TilemapPack maps, Button button, Tile icon, int zOrder = 0)
    {
        var b = button;
        icon.Tint = GetInteractionColor(b, icon.Tint);
        Clear(maps, button, zOrder);
        maps.Tilemaps[zOrder].SetTile(b.Position, icon, b.Mask);
    }
    public static void SetInputBox(this TilemapPack maps, InputBox inputBox, int zOrder = 0)
    {
        var box = inputBox;
        var bgColor = Gray.ToDark(0.4f);
        var selectColor = box.IsFocused ? Blue : Blue.ToBright();
        var selection = box.Selection.Constrain(box.Size, false);
        var text = box.Text.Constrain(box.Size, false);
        var placeholder = box.Placeholder.Constrain(box.Size);
        var cursor = new Tile(SHAPE_LINE, White, 2);
        var scrollY = box.ScrollIndices.y;
        var textAboveOrBelow = new Tile(FULL, Gray.ToDark(0.3f), 3);
        var (w, h) = box.Size;

        Clear(maps, inputBox, zOrder);
        maps.Tilemaps[zOrder].SetArea(box.Area, box.Mask, new Tile(SHADE_OPAQUE, bgColor));
        maps.Tilemaps[zOrder].SetText(box.Position, selection, selectColor, mask: box.Mask);
        maps.Tilemaps[zOrder + 1].SetText(box.Position, text, mask: box.Mask);

        if (string.IsNullOrEmpty(box.Value))
            maps.Tilemaps[zOrder + 1].SetText(box.Position, placeholder, Gray, mask: box.Mask);

        if (scrollY > 0)
            maps.Tilemaps[zOrder + 0].SetArea((box.X, box.Y, w, 1), null, textAboveOrBelow);

        if (scrollY < box.LineCount - box.Height)
            maps.Tilemaps[zOrder + 0].SetArea((box.X, box.Y + h - 1, w, 1), null, textAboveOrBelow);

        if (box.IsCursorVisible)
            maps.Tilemaps[zOrder + 2].SetTile(box.PositionFromIndices(box.CursorIndices), cursor, box.Mask);
    }
    public static void SetFileViewerItem(this TilemapPack maps, FileViewer fileViewer, Button item, int zOrder = 1)
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
    public static void SetFileViewer(this TilemapPack maps, FileViewer fileViewer, int zOrder = 0)
    {
        Clear(maps, fileViewer, zOrder);
        SetBackground(maps.Tilemaps[zOrder], fileViewer);

        if (fileViewer.FilesAndFolders.Scroll.IsHidden == false)
            SetScroll(maps, fileViewer.FilesAndFolders.Scroll, zOrder);

        SetFileViewerItem(maps, fileViewer, fileViewer.User, zOrder + 1);
        SetFileViewerItem(maps, fileViewer, fileViewer.Back, zOrder + 1);
    }
    public static void SetSlider(this TilemapPack maps, Slider slider, string? text = "", int zOrder = 0)
    {
        var s = slider;
        var result = $"{text}{s.Progress * 100f:F0}%";
        var isHandle = s.Handle.IsPressedAndHeld;
        var color = GetInteractionColor(isHandle ? s.Handle : s, Gray.ToBright());

        Clear(maps, s, zOrder);
        SetBackground(maps.Tilemaps[zOrder], s);
        maps.Tilemaps[zOrder + 1].SetBar(s.Handle.Position,
            BAR_DEFAULT_EDGE,
            BAR_DEFAULT_STRAIGHT,
            color,
            s.IsVertical ? s.Width : s.Height,
            s.IsVertical == false,
            s.Mask);
        maps.Tilemaps[zOrder + 2].SetText(
            (s.X + s.Width / 2 - result.Length / 2, s.Y + s.Height / 2),
            result,
            mask: s.Mask);
    }
    public static void SetScroll(this TilemapPack maps, Scroll scroll, int zOrder = 0)
    {
        var s = scroll;
        var scrollUpAngle = (sbyte)(s.IsVertical ? 1 : 0);
        var scrollDownAngle = (sbyte)(s.IsVertical ? 3 : 2);
        var scrollColor = Gray.ToBright();
        var isHandle = s.Slider.Handle.IsPressedAndHeld;

        Clear(maps, s, zOrder);
        SetBackground(maps.Tilemaps[zOrder], s, 0.4f);
        maps.Tilemaps[zOrder + 1].SetTile(s.Slider.Handle.Position,
            new(SHAPE_CIRCLE,
                GetInteractionColor(isHandle ? s.Slider.Handle : s.Slider, scrollColor)),
            s.Mask);
        maps.Tilemaps[zOrder + 1].SetTile(s.Increase.Position,
            new(ARROW, GetInteractionColor(s.Increase, scrollColor), scrollUpAngle),
            s.Mask);
        maps.Tilemaps[zOrder + 1].SetTile(s.Decrease.Position,
            new(ARROW, GetInteractionColor(s.Decrease, scrollColor), scrollDownAngle),
            s.Mask);
    }
    public static void SetStepper(this TilemapPack maps, Stepper stepper, int zOrder = 0)
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
    public static void SetPrompt(this TilemapPack maps, Prompt prompt, int zOrder = 0)
    {
        if (prompt.IsHidden == false)
        {
            var tile = new Tile(SHADE_OPAQUE, new Color(0, 0, 0, 127));
            maps.Tilemaps[zOrder].SetArea((0, 0, maps.Size.width, maps.Size.height), prompt.Mask, tile);
            maps.Tilemaps[zOrder + 1].SetBox(prompt.Area,
                new(SHADE_OPAQUE, Gray.ToDark(0.6f)),
                BOX_CORNER_ROUND,
                SHADE_OPAQUE,
                Gray.ToDark(0.6f),
                prompt.Mask);
        }

        var lines = prompt.Text.Split(Environment.NewLine).Length;
        maps.Tilemaps[zOrder + 2].SetText(
            prompt.Position,
            prompt.Text.Constrain((prompt.Size.width, lines), alignment: Alignment.Center),
            mask: prompt.Mask);
    }
    public static void SetPromptItem(this TilemapPack maps, Prompt prompt, Button item, int zOrder = 2, params Tile[]? tiles)
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
    public static void SetPanel(this TilemapPack maps, Panel panel, int zOrder = 0)
    {
        var p = panel;
        var (x, y) = p.Position;
        var (w, _) = p.Size;

        Clear(maps, p, zOrder);
        SetBackground(maps.Tilemaps[zOrder], p, 0.6f);

        maps.Tilemaps[zOrder + 1].SetBox(p.Area, EMPTY, BOX_GRID_CORNER,
            BOX_GRID_STRAIGHT, Blue, p.Mask);
        maps.Tilemaps[zOrder + 1].SetText(
            (x + w / 2 - p.Text.Length / 2, y),
            p.Text.Shorten(Math.Min(w, p.Text.Length)),
            mask: p.Mask);
    }
    public static void SetPalette(this TilemapPack maps, Palette palette, int zOrder = 0)
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
    public static void SetPages(this TilemapPack maps, Pages pages, int zOrder = 0)
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

            maps.Tilemaps[zOrder].SetBar(
                button.Position,
                BAR_BIG_EDGE,
                SHADE_OPAQUE,
                color.ToDark(0.75f),
                button.Size.height,
                true,
                p.Mask);
            maps.Tilemaps[zOrder + 1].SetTile(
                (button.Position.x, button.Position.y + button.Size.height / 2),
                new(tileId, color),
                p.Mask);
        }
    }
    public static void SetPagesItem(this TilemapPack maps, Pages pages, Button item, int zOrder = 1)
    {
        var color = GetInteractionColor(item, item.IsSelected ? Green : Gray.ToBright(0.2f));
        var text = item.Text.ToNumber().PadZeros(-pages.ItemWidth);

        SetBackground(maps.Tilemaps[zOrder], item, 0.33f);
        maps.Tilemaps[zOrder + 1].SetText(item.Position,
            text.Constrain(item.Size, alignment: Alignment.Center),
            color,
            mask: item.Mask);
    }
    public static void SetPagesIcon(this TilemapPack maps, Button item, int tileId, int zOrder = 1)
    {
        var color = GetInteractionColor(item, item.IsSelected ? Green : Gray.ToBright(0.2f));
        SetBackground(maps.Tilemaps[zOrder], item, 0.33f);
        maps.Tilemaps[zOrder + 1].SetTile(item.Position, new(tileId + int.Parse(item.Text), color),
            item.Mask);
    }
    public static void SetList(this TilemapPack maps, List list, int zOrder = 0)
    {
        Clear(maps, list, zOrder);
        maps.Tilemaps[zOrder].SetArea(
            list.Area,
            tiles: new Tile(SHADE_OPAQUE, Gray.ToDark()),
            mask: list.Mask);

        if (list.IsScrollAvailable)
            SetScroll(maps, list.Scroll, zOrder + 1);

        if (list.IsCollapsed)
            maps.Tilemaps[zOrder + 2].SetTile(
                (list.Position.x + list.Size.width - 1, list.Position.y),
                new(MATH_GREATER, GetInteractionColor(list, Gray.ToBright()), 1),
                list.Mask);
    }
    public static void SetListItem(this TilemapPack maps, List list, Button item, int zOrder = 1, bool isSelectHighlighting = true)
    {
        var color = item.IsSelected && isSelectHighlighting ? Green : Gray.ToBright(0.3f);
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
    public static void SetLayoutSegment(this TilemapPack maps, (int x, int y, int width, int height) segment, int index, bool isIndexVisible, int zOrder = 0)
    {
        var color = new Color(
            (byte)(20, 200).Random(seed / (index + 1f)),
            (byte)(20, 200).Random(seed / (index + 2f)),
            (byte)(20, 200).Random(seed / (index + 3f)));

        maps.Tilemaps[zOrder].SetBox(
            segment,
            new(SHADE_OPAQUE, color),
            BOX_CORNER_ROUND,
            SHADE_OPAQUE,
            color,
            segment);

        if (isIndexVisible)
            maps.Tilemaps[zOrder + 1].SetText(
                (segment.x, segment.y),
                index.ToString().Constrain((segment.width, segment.height),
                    alignment: Alignment.Center),
                mask: segment);
    }

    public static Color GetInteractionColor(this Block block, Color baseColor)
    {
        var hotkeyIsPressed = block is Button btn &&
                              ((Keyboard.Key)btn.Hotkey.id).IsPressed() &&
                              Keyboard.KeysPressed.Length == 1;

        if (block.IsDisabled || IsInteractable == false) return baseColor;
        if (block.IsPressedAndHeld || hotkeyIsPressed) return baseColor.ToDark();
        else if (block.IsHovered) return baseColor.ToBright();

        return baseColor;
    }

#region Backend
    private static readonly int seed;

    static MapperUI()
    {
        seed = (-1_000_000, 1_000_000).Random();
    }

    private static void SetBackground(Tilemap map, Block block, float shade = 0.5f)
    {
        var e = block;
        var color = Gray.ToDark(shade);
        var tile = new Tile(SHADE_OPAQUE, color);

        map.SetBox(e.Area, tile, BOX_CORNER_ROUND, SHADE_OPAQUE, color, block.Mask);
    }
    private static void Clear(TilemapPack maps, Block block, int zOrder)
    {
        var (x, y) = block.Position;
        var (w, h) = block.Size;

        for (var i = zOrder; i < zOrder + 3; i++)
            if (i < maps.Tilemaps.Count)
                maps.Tilemaps[i].SetArea((x, y, w, h), block.Mask, SHADE_TRANSPARENT);
    }
#endregion
}