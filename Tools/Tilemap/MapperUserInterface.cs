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
    public static (Tile corner, Tile fill, Tile arrow, Tile min, Tile mid, Tile max, uint textTint, uint valueTint) ThemeStepper { get; set; }
    public static (Tile corner, Tile edge, Tile fill, uint textTint) ThemeTooltip { get; set; }
    public static (Tile first, Tile previous, Tile next, Tile last) ThemePages { get; set; }
    public static (Tile corner, Tile edge, Tile fill, Tile dim, uint textTint) ThemePrompt { get; set; }
    public static Tile[]? ThemePromptItems { get; set; }

    public static void SetTooltip(this Maps maps, Tooltip tooltip, int zOrder = 1)
    {
        if (maps.Tilemaps.Count <= zOrder + 1)
            return;

        var (corner, edge, fill, textTint) = ThemeTooltip;
        var (x, y) = tooltip.Position;

        Clear(maps, tooltip, zOrder);
        maps.Tilemaps[zOrder].SetBox(tooltip.Area, fill, corner, edge, tooltip.Mask);
        maps.Tilemaps[zOrder + 1].SetText((x + 1, y), tooltip.Text, textTint, mask: tooltip.Mask);
    }
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

        bCorner.Tint = button.GetInteractionColor(bCorner.Tint, 0.3f);
        bEdge.Tint = button.GetInteractionColor(bEdge.Tint, 0.3f);
        bFill.Tint = button.GetInteractionColor(bFill.Tint, 0.3f);
        bTextTint = button.GetInteractionColor(bTextTint, 0.3f);

        rEdge.Tint = button.GetInteractionColor(rEdge.Tint, 0.3f);
        rFill.Tint = button.GetInteractionColor(rFill.Tint, 0.3f);
        rTextTint = button.GetInteractionColor(rTextTint, 0.3f);

        Clear(maps, button, zOrder);

        if (isBar)
            maps.Tilemaps[zOrder].SetBar(button.Position, rEdge, rFill, button.Width, mask: button.Mask);
        else
            maps.Tilemaps[zOrder].SetBox(button.Area, bFill, bCorner, bEdge, button.Mask);

        maps.Tilemaps[zOrder + 1].SetText(textPos, text, isBar ? rTextTint : bTextTint, mask: button.Mask);
    }
    public static void SetButtonIcon(this Maps maps, Button button, Tile icon, int zOrder = 0)
    {
        if (maps.Tilemaps.Count <= zOrder)
            return;

        icon.Tint = button.GetInteractionColor(icon.Tint);
        Clear(maps, button, zOrder);
        maps.Tilemaps[zOrder].SetTile(button.Position, icon, button.Mask);
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
            maps.Tilemaps[zOrder + 0].SetArea((box.X, box.Y, w, 1), box.Mask, textAboveOrBelow);

        if (scrollY < box.LineCount - box.Height)
            maps.Tilemaps[zOrder + 0].SetArea((box.X, box.Y + h - 1, w, 1), box.Mask, textAboveOrBelow);

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
        // SetBackground(maps.Tilemaps[zOrder], fileViewer);

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
        if (maps.Tilemaps.Count <= zOrder + 1)
            return;

        var (corner, fill, arrow, min, mid, max, textTint, valueTint) = ThemeStepper;
        var (x, y) = stepper.Position;
        var stepPrecision = MathF.Round(stepper.Step, 2).Precision();
        var value = stepPrecision == 0 ? $"{stepper.Value}" : $"{stepper.Value:F2}";
        var maxTextSize = Math.Min(stepper.Width - 1, stepper.Text.Length);
        var upPos = stepper.Increase.Position;
        var downPos = stepper.Decrease.Position;
        var upTint = stepper.Increase.GetInteractionColor(arrow.Tint, 0.4f);
        var downTint = stepper.Decrease.GetInteractionColor(arrow.Tint, 0.4f);
        var text = stepper.Text.Shorten(maxTextSize);
        var mask = stepper.Mask;

        value = value.Shorten(stepper.Width - 4);
        fill.Tint = stepper.GetInteractionColor(fill.Tint, 0.05f);
        corner.Tint = stepper.GetInteractionColor(corner.Tint, 0.05f);
        min.Tint = stepper.Minimum.GetInteractionColor(min.Tint, 0.3f);
        mid.Tint = stepper.Middle.GetInteractionColor(mid.Tint, 0.3f);
        max.Tint = stepper.Maximum.GetInteractionColor(max.Tint, 0.3f);

        Clear(maps, stepper, zOrder);
        maps.Tilemaps[zOrder].SetBox(stepper.Area, fill, corner, fill, mask);
        maps.Tilemaps[zOrder + 1].SetTile(upPos, new(arrow.Id, upTint, 3), mask);
        maps.Tilemaps[zOrder + 1].SetTile(downPos, new(arrow.Id, downTint, 1), mask);
        maps.Tilemaps[zOrder + 1].SetText((x + 1, y), text, textTint, mask: mask);
        maps.Tilemaps[zOrder + 1].SetText((x + 1, y + 1), value, valueTint, mask: mask);
        maps.Tilemaps[zOrder + 1].SetTile(stepper.Minimum.Position, min, mask);
        maps.Tilemaps[zOrder + 1].SetTile(stepper.Middle.Position, mid, mask);
        maps.Tilemaps[zOrder + 1].SetTile(stepper.Maximum.Position, max, mask);
    }
    public static void SetPrompt(this Maps maps, Prompt prompt, int zOrder = 0)
    {
        if (maps.Tilemaps.Count <= zOrder + 2)
            return;

        var (corner, edge, fill, dim, textTint) = ThemePrompt;
        var newLines = prompt.Text.Count("\n") + 1;
        var text = prompt.Text.Constrain((prompt.Width, newLines), alignment: Alignment.Center);

        Clear(maps, prompt, zOrder);
        maps.Tilemaps[zOrder].SetArea((0, 0, maps.Size.width, maps.Size.height), prompt.Mask, dim);
        maps.Tilemaps[zOrder + 1].SetBox(prompt.Area, fill, corner, edge, prompt.Mask);
        maps.Tilemaps[zOrder + 2].SetText(prompt.Position, text, textTint, mask: prompt.Mask);
    }
    public static void SetPromptItem(this Maps maps, Prompt prompt, Button item, int zOrder = 2)
    {
        var theme = ThemePromptItems;

        if (maps.Tilemaps.Count <= zOrder || theme == null || theme.Length == 0)
            return;

        var index = prompt.IndexOf(item);

        var tile = new Tile(PUNCTUATION_QUESTION_MARK, Gray);
        if (index < theme.Length)
            tile = theme[index];

        maps.SetButtonIcon(item, tile, zOrder);
    }
    public static void SetPanel(this Maps maps, Panel panel, int zOrder = 0)
    {
        var p = panel;
        var (x, y) = p.Position;
        var (w, _) = p.Size;

        Clear(maps, p, zOrder);
        // SetBackground(maps.Tilemaps[zOrder], p, 0.6f);

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
        if (maps.Tilemaps.Count <= zOrder)
            return;

        var (first, previous, next, last) = ThemePages;

        first.Tint = pages.First.GetInteractionColor(first.Tint);
        previous.Tint = pages.Previous.GetInteractionColor(previous.Tint);
        next.Tint = pages.Next.GetInteractionColor(next.Tint);
        last.Tint = pages.Last.GetInteractionColor(last.Tint);

        Clear(maps, pages, zOrder);

        if (pages.First.IsHidden == false)
            maps.Tilemaps[zOrder].SetTile(pages.First.Position, first, pages.Mask);
        if (pages.Previous.IsHidden == false)
            maps.Tilemaps[zOrder].SetTile(pages.Previous.Position, previous, pages.Mask);
        if (pages.Next.IsHidden == false)
            maps.Tilemaps[zOrder].SetTile(pages.Next.Position, next, pages.Mask);
        if (pages.Last.IsHidden == false)
            maps.Tilemaps[zOrder].SetTile(pages.Last.Position, last, pages.Mask);
    }
    public static void SetPagesItem(this Maps maps, Pages pages, Button item, int zOrder = 0)
    {
        if (maps.Tilemaps.Count <= zOrder)
            return;

        var color = GetInteractionColor(item, item.IsSelected ? Green : Gray.ToBright(0.2f));
        var text = item.Text.ToNumber().PadZeros(-pages.ItemWidth);
        text = text.Constrain(item.Size, alignment: Alignment.Center);

        maps.Tilemaps[zOrder].SetText(item.Position, text, color, mask: item.Mask);
    }
    public static void SetList(this Maps maps, List list, int zOrder = 0)
    {
        if (maps.Tilemaps.Count <= zOrder + 2)
            return;

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
        if (maps.Tilemaps.Count <= zOrder)
            return;

        var color = item.IsSelected && showSelected ? Green : Gray.ToBright(0.3f);
        var (x, y) = item.Position;
        var (_, h) = item.Size;
        var isLeftCrop =
            list.Span == Span.Horizontal &&
            item.Size.width < list.ItemSize.width &&
            item.Position == list.Position;
        var text = item.Text.Shorten(item.Size.width * (isLeftCrop ? -1 : 1));

        color = item.GetInteractionColor(item.IsDisabled ? Gray : color);

        maps.Tilemaps[zOrder].SetText((x, y + h / 2), text, color, mask: item.Mask);
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
        if (block.IsHovered) return baseColor.ToBright(amount);

        return baseColor;
    }

#region Backend
    private static readonly int seed;

    static MapperUserInterface()
    {
        seed = (-1_000_000, 1_000_000).Random();

        var g = Gray;
        var dg = g.ToDark();
        var dim = Black.ToTransparent();
        var arrow = new Tile(ARROW_TAILLESS_ROUND, g);

        ThemeScrollArrow = arrow;
        ThemeButtonBox = (new(BOX_CORNER_ROUND, g), new(FULL, g), new(FULL, g), g.ToBright());
        ThemeButtonBar = (new(BAR_BIG_EDGE, g), new(FULL, g), g.ToBright());
        ThemeInputBox = (new(FULL, g.ToDark(0.4f)), new(SHAPE_LINE, White, 2), g.ToBright(), Blue);
        ThemeCheckbox = (new(ICON_TICK, Green), new(ICON_X, Red));
        ThemeSlider = (new(BAR_BIG_EDGE, g), new(BAR_BIG_STRAIGHT, g), new(SHAPE_CIRCLE_BIG, g.ToBright()));
        ThemeTooltip = (new(BOX_CORNER_ROUND, dg), new(FULL, dg), new(FULL, dg), textTint: White);
        ThemePages = (new(MATH_MUCH_LESS, g), new(MATH_LESS, g), new(MATH_GREATER, g), new(MATH_MUCH_GREATER, g));
        ThemePrompt = (new(BOX_CORNER_ROUND, dg), new(FULL, dg), new(FULL, dg), new(FULL, dim), White);
        ThemePromptItems = [new(ICON_YES, Green), new(ICON_NO, Red)];

        var min = new Tile(MATH_MUCH_LESS, g);
        var mid = new Tile(PUNCTUATION_PIPE, g);
        var max = new Tile(MATH_MUCH_GREATER, g);
        ThemeStepper = (new(BOX_CORNER_ROUND, dg), new(FULL, dg), arrow, min, mid, max, g, White);
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