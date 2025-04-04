﻿using Pure.Engine.Window;
using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
using Pure.Engine.Utility;
using static Pure.Engine.Tiles.Tile;
using static Pure.Engine.Utility.Color;
using static Pure.Engine.Window.Keyboard;

namespace Pure.Tools.Tiles;

public static class TileMapperUI
{
    public static float InteractionShade { get; set; }
    public static (Tile corner, Tile edge, Tile fill, uint tintText) ThemeButtonBox { get; set; }
    public static (Tile[,] tiles3X3, uint tintText) ThemeButtonPatch { get; set; }
    public static (Tile left, Tile fill, Tile right, uint tintText) ThemeButtonBar { get; set; }
    public static (Tile background, Tile cursor, uint tintText, uint selectionTint) ThemeInputBox { get; set; }
    public static (Tile on, Tile off) ThemeCheckbox { get; set; }
    public static (Tile arrow, uint tintOn, uint tintOff) ThemeSwitch { get; set; }
    public static (Tile edge1, Tile fill, Tile edge2, Tile handle) ThemeSlider { get; set; }
    public static Tile ThemeScrollArrow { get; set; }
    public static (Tile corner, Tile fill, Tile arrow, Tile min, Tile mid, Tile max, uint tintText, uint valueTint) ThemeStepper { get; set; }
    public static (Tile corner, Tile edge, Tile fill, uint tintText) ThemeTooltip { get; set; }
    public static (Tile first, Tile previous, Tile next, Tile last) ThemePages { get; set; }
    public static (Tile corner, Tile edge, Tile fill, Tile dim, uint tintText) ThemePrompt { get; set; }
    public static Tile[]? ThemePromptItems { get; set; }
    public static (Tile full, Tile pick, Tile halfShade, Tile handle) ThemePalette { get; set; }
    public static (Tile corner, Tile edge, Tile fill, uint tintText) ThemePanel { get; set; }
    public static (uint tint, uint tintSelect, uint tintDisable) ThemeListText { get; set; }
    public static Tile[,] ThemeListPatch { get; set; }
    public static (Tile left, Tile fill, Tile right, Tile arrow) ThemeListBar { get; set; }
    public static (Tile img, Tile audio, Tile font, Tile txt, Tile zip, Tile vid, Tile cfg, Tile exe) ThemeFileViewer { get; set; }

    public static void SetTooltip(this IList<TileMap> maps, Tooltip tooltip, int zOrder = 1)
    {
        if (maps.Count <= zOrder + 1 || tooltip.IsHidden)
            return;

        var (corner, edge, fill, textTint) = ThemeTooltip;
        var (x, y) = tooltip.Position;

        ApplyMasks(maps, tooltip.Mask);
        Clear(maps, tooltip, zOrder);
        maps[zOrder].SetBox(tooltip.Area, fill, corner, edge);
        maps[zOrder + 1].SetText((x + 1, y), tooltip.Text, textTint);
        RestoreMasks(maps);
    }
    public static void SetCheckbox(this IList<TileMap> maps, Button button, int zOrder = 1)
    {
        if (maps.Count <= zOrder || button.IsHidden)
            return;

        var (on, off) = ThemeCheckbox;
        var tile = button.IsSelected ? on : off;
        var textPos = (button.X + 2, button.Y);

        tile.Tint = button.GetInteractionColor(tile.Tint, InteractionShade);

        ApplyMasks(maps, button.Mask);
        Clear(maps, button, zOrder);
        maps[zOrder].SetTile(button.Position, tile, button.Mask);
        maps[zOrder].SetText(textPos, button.Text, tile.Tint);
        RestoreMasks(maps);
    }
    public static void SetSwitch(this IList<TileMap> maps, Button button, char arrowAtSymbol = ' ', int zOrder = 1)
    {
        if (maps.Count <= zOrder || button.IsHidden)
            return;

        var (arrow, on, off) = ThemeSwitch;
        var arrowPos = (button.X + button.Text.IndexOf(arrowAtSymbol), button.Y);
        var split = button.Text.Split(arrowAtSymbol);
        var (x, y) = button.Position;

        arrow.Tint = button.GetInteractionColor(arrow.Tint, InteractionShade);
        arrow.Pose = button.IsSelected ? Pose.Default : Pose.Down;
        on = button.GetInteractionColor(on, InteractionShade);
        off = button.GetInteractionColor(off, InteractionShade);

        if (button.IsSelected)
            (on, off) = (off, on);

        ApplyMasks(maps, button.Mask);
        if (split.Length != 2)
            maps[zOrder].SetText((x, y), button.Text, on);
        else
        {
            Clear(maps, button, zOrder);
            maps[zOrder].SetTile(arrowPos, arrow, button.Mask);
            maps[zOrder].SetText((x, y), split[0], on);
            maps[zOrder].SetText((x + split[0].Length + 1, y), split[1], off);
        }

        RestoreMasks(maps);
    }
    public static void SetButton(this IList<TileMap> maps, Button button, int zOrder = 1)
    {
        if (maps.Count <= zOrder + 1 || button.IsHidden)
            return;

        var (w, h) = button.Size;
        var offsetW = w / 2 - Math.Min(button.Text.Length, h == 1 ? w : w - 2) / 2;
        var (bCorner, bEdge, bFill, bTextTint) = ThemeButtonBox;
        var (rLeft, rFill, rRight, rTextTint) = ThemeButtonBar;
        var text = button.Text.Shorten(h == 1 ? w : w - 2);
        var textPos = (button.X + offsetW, button.Y + h / 2);
        var isBar = button.Height == 1;

        ApplyMasks(maps, button.Mask);
        Clear(maps, button, zOrder);
        if (isBar)
        {
            rLeft.Tint = button.GetInteractionColor(rLeft.Tint, InteractionShade);
            rFill.Tint = button.GetInteractionColor(rFill.Tint, InteractionShade);
            rRight.Tint = button.GetInteractionColor(rRight.Tint, InteractionShade);
            rTextTint = button.GetInteractionColor(rTextTint, InteractionShade);
            maps[zOrder].SetBar(button.Position, rLeft, rFill, rRight, button.Width);
        }
        else
        {
            bCorner.Tint = button.GetInteractionColor(bCorner.Tint, InteractionShade);
            bEdge.Tint = button.GetInteractionColor(bEdge.Tint, InteractionShade);
            bFill.Tint = button.GetInteractionColor(bFill.Tint, InteractionShade);
            bTextTint = button.GetInteractionColor(bTextTint, InteractionShade);
            maps[zOrder].SetBox(button.Area, bFill, bCorner, bEdge);
        }

        maps[zOrder + 1].SetText(textPos, text, isBar ? rTextTint : bTextTint);
        RestoreMasks(maps);
    }
    public static void SetButtonIcon(this IList<TileMap> maps, Button button, Tile icon, int zOrder = 0)
    {
        if (maps.Count <= zOrder || button.IsHidden)
            return;

        icon.Tint = button.GetInteractionColor(icon.Tint, InteractionShade);
        Clear(maps, button, zOrder);
        maps[zOrder].SetTile(button.Position, icon, button.Mask);
    }
    public static void SetInputBox(this IList<TileMap> maps, InputBox inputBox, int zOrder = 0)
    {
        if (maps.Count <= zOrder + 2 || inputBox.IsHidden)
            return;

        var box = inputBox;
        var (background, cursor, textTint, selectionTint) = ThemeInputBox;
        var selectColor = box.IsFocused ? new(selectionTint) : new Color(selectionTint).ToBright();
        var selection = box.Selection.Constrain(box.Size, false);
        var text = box.Text.Constrain(box.Size, false);
        var placeholder = box.Placeholder?.Constrain(box.Size);
        var scrollY = box.ScrollIndices.y;
        var textAboveOrBelow = new Tile(background.Id, new Color(background.Tint).ToDark(0.3f));
        var (w, h) = box.Size;
        var cursorPos = box.PositionFromIndices(box.CursorIndices);
        var placeholderTint = new Color(textTint).ToDark(0.4f);

        background.Tint = box.GetInteractionColor(background.Tint, InteractionShade / 2f);

        ApplyMasks(maps, box.Mask);
        Clear(maps, inputBox, zOrder);
        maps[zOrder].SetArea(box.Area, [background]);
        maps[zOrder].SetText(box.Position, selection, selectColor);
        maps[zOrder + 1].SetText(box.Position, text, textTint);

        if (string.IsNullOrEmpty(box.Value))
            maps[zOrder + 1].SetText(box.Position, placeholder, placeholderTint);

        if (scrollY > 0)
            maps[zOrder + 0].SetArea((box.X, box.Y, w, 1), [textAboveOrBelow]);

        if (scrollY < box.LineCount - box.Height)
            maps[zOrder + 0].SetArea((box.X, box.Y + h - 1, w, 1), [textAboveOrBelow]);

        if (box.IsCursorVisible)
            maps[zOrder + 2].SetTile(cursorPos, cursor, box.Mask);
        RestoreMasks(maps);
    }
    public static void SetFileViewerItem(this IList<TileMap> maps, FileViewer fileViewer, Button item, int zOrder = 1)
    {
        if (maps.Count <= zOrder || item.IsHidden)
            return;

        var (img, audio, font, txt, zip, vid, cfg, exe) = ThemeFileViewer;
        var (tint, select, _) = ThemeListText;
        var isFolder = fileViewer.IsFolder(item);
        var isHardDrive = fileViewer.HardDrives.Items.Contains(item);
        var isFileOrFolder = fileViewer.FilesAndFolders.Items.Contains(item);
        var color = item.IsSelected && isFileOrFolder ? select : tint;
        var icon = new Tile(ICON_FOLDER, Yellow.ToDark(0.3f));
        var isRight = isHardDrive || item == fileViewer.Back || item == fileViewer.User;
        var text = item.Text.Shorten(isRight ? -fileViewer.Size.width + 1 : item.Size.width - 1);
        var extensionIcons = new Dictionary<string[], Tile>
        {
            { ["png", "jpg", "jpeg", "bmp", "svg", "gif", "psd", "tif", "tiff", "webp", "pdf", "ico"], img },
            { ["wav", "ogg", "flac", "mp3", "aiff", "aac", "mid", "cda", "mpa", "wma"], audio },
            { ["ttf", "otf"], font },
            { ["txt", "xml", "json", "log", "csv"], txt },
            { ["zip", "rar", "7z", "arj", "deb", "pkg", "tar.gz", "z"], zip },
            { ["mp4", "avi", "flv", "mkv"], vid },
            { ["dll", "cfg", "ini"], cfg },
            { ["exe", "bin", "bat", "jar", "msi"], exe }
        };

        color = item.GetInteractionColor(color, InteractionShade);

        if (isHardDrive)
        {
            icon.Id = ICON_HOME;
            icon.Tint = Blue.ToBright();
        }
        else if (item == fileViewer.Back)
        {
            icon.Id = ICON_BACK;
            icon.Tint = Blue.ToBright();
        }
        else if (item == fileViewer.User)
        {
            icon.Id = ICON_PERSON;
            icon.Tint = Blue.ToBright();
        }
        else if (isFolder == false)
        {
            icon.Id = ICON_FILE;
            icon.Tint = Gray;

            foreach (var (ext, tile) in extensionIcons)
                if (Ext(ext))
                {
                    icon = tile;
                    break;
                }
        }

        icon.Tint = item.GetInteractionColor(icon.Tint, InteractionShade);

        ApplyMasks(maps, item.Mask);
        maps[zOrder].SetTile(item.Position, icon, item.Mask);
        maps[zOrder].SetText((item.X + 1, item.Y), text, color);
        RestoreMasks(maps);

        bool Ext(params string[] ext)
        {
            foreach (var ex in ext)
                if (Path.GetExtension(item.Text).Equals($".{ex}", StringComparison.CurrentCultureIgnoreCase))
                    return true;

            return false;
        }
    }
    public static void SetFileViewer(this IList<TileMap> maps, FileViewer fileViewer, int zOrder = 0)
    {
        if (maps.Count <= zOrder + 1 || fileViewer.IsHidden)
            return;

        ApplyMasks(maps, fileViewer.Mask);
        maps.SetList(fileViewer.FilesAndFolders, zOrder);
        maps.SetFileViewerItem(fileViewer, fileViewer.User, zOrder + 1);
        maps.SetFileViewerItem(fileViewer, fileViewer.Back, zOrder + 1);
        RestoreMasks(maps);
    }
    public static void SetSlider(this IList<TileMap> maps, Slider slider, int zOrder = 0)
    {
        if (maps.Count <= zOrder + 1 || slider.IsHidden)
            return;

        var (edge1, fill, edge2, handle) = ThemeSlider;
        var size = slider.IsVertical ? slider.Height : slider.Width;

        edge1.Tint = slider.GetInteractionColor(edge1.Tint, InteractionShade);
        fill.Tint = slider.GetInteractionColor(fill.Tint, InteractionShade);
        edge2.Tint = slider.GetInteractionColor(edge2.Tint, InteractionShade);
        handle.Tint = slider.Handle.GetInteractionColor(handle.Tint, InteractionShade);

        if (slider.IsVertical)
        {
            edge1 = edge1.Rotate(1);
            fill = fill.Rotate(1);
            edge2 = edge2.Rotate(1);
        }

        ApplyMasks(maps, slider.Mask);
        Clear(maps, slider, zOrder);
        maps[zOrder].SetBar(slider.Position, edge1, fill, edge2, size, slider.IsVertical);
        maps[zOrder + 1].SetTile(slider.Handle.Position, handle, slider.Mask);
        RestoreMasks(maps);
    }
    public static void SetScroll(this IList<TileMap> maps, Scroll scroll, int zOrder = 0)
    {
        if (maps.Count <= zOrder + 1 || scroll.IsHidden)
            return;

        var arrow = ThemeScrollArrow;
        var scrollUpAngle = scroll.IsVertical ? Pose.Right : Pose.Default;
        var scrollDownAngle = scroll.IsVertical ? Pose.Left : Pose.Down;
        var up = scroll.Increase.Position;
        var down = scroll.Decrease.Position;

        var upTint = scroll.Increase.GetInteractionColor(arrow.Tint, InteractionShade);
        var downTint = scroll.Decrease.GetInteractionColor(arrow.Tint, InteractionShade);

        ApplyMasks(maps, scroll.Mask);
        Clear(maps, scroll, zOrder);
        maps.SetSlider(scroll.Slider, zOrder);
        maps[zOrder + 1].SetTile(up, new(arrow.Id, upTint, scrollUpAngle), scroll.Mask);
        maps[zOrder + 1].SetTile(down, new(arrow.Id, downTint, scrollDownAngle), scroll.Mask);
        RestoreMasks(maps);
    }
    public static void SetStepper(this IList<TileMap> maps, Stepper stepper, int zOrder = 0)
    {
        if (maps.Count <= zOrder + 1 || stepper.IsHidden)
            return;

        var (corner, fill, arrow, min, mid, max, textTint, valueTint) = ThemeStepper;
        var (x, y) = stepper.Position;
        var stepPrecision = MathF.Round(stepper.Step, 2).Precision();
        var value = stepPrecision == 0 ? $"{stepper.Value}" : $"{stepper.Value:F2}";
        var maxTextSize = Math.Min(stepper.Width - 1, stepper.Text.Length);
        var upPos = stepper.Increase.Position;
        var downPos = stepper.Decrease.Position;
        var upTint = stepper.Increase.GetInteractionColor(arrow.Tint, InteractionShade);
        var downTint = stepper.Decrease.GetInteractionColor(arrow.Tint, InteractionShade);
        var text = stepper.Text.Shorten(maxTextSize);
        var mask = stepper.Mask;

        value = value.Shorten(stepper.Width - 4);
        fill.Tint = stepper.GetInteractionColor(fill.Tint, InteractionShade / 2f);
        corner.Tint = stepper.GetInteractionColor(corner.Tint, InteractionShade / 2f);
        min.Tint = stepper.Minimum.GetInteractionColor(min.Tint, InteractionShade);
        mid.Tint = stepper.Middle.GetInteractionColor(mid.Tint, InteractionShade);
        max.Tint = stepper.Maximum.GetInteractionColor(max.Tint, InteractionShade);

        ApplyMasks(maps, mask);
        Clear(maps, stepper, zOrder);
        maps[zOrder].SetBox(stepper.Area, fill, corner, fill);
        maps[zOrder + 1].SetTile(upPos, new(arrow.Id, upTint, Pose.Left), mask);
        maps[zOrder + 1].SetTile(downPos, new(arrow.Id, downTint, Pose.Right), mask);
        maps[zOrder + 1].SetText((x + 1, y), text, textTint);
        maps[zOrder + 1].SetText((x + 1, y + 1), value, valueTint);
        maps[zOrder + 1].SetTile(stepper.Minimum.Position, min, mask);
        maps[zOrder + 1].SetTile(stepper.Middle.Position, mid, mask);
        maps[zOrder + 1].SetTile(stepper.Maximum.Position, max, mask);
        RestoreMasks(maps);
    }
    public static void SetPrompt(this IList<TileMap> maps, Prompt prompt, int zOrder = 0)
    {
        if (maps.Count <= zOrder + 2 || prompt.IsHidden)
            return;

        var (corner, edge, fill, dim, textTint) = ThemePrompt;
        var newLines = prompt.Text.Count("\n") + 1;
        var text = prompt.Text.Constrain((prompt.Width, newLines), alignment: Alignment.Center);
        var (w, h) = Input.TilemapSize;
        var (x, y) = prompt.Position;

        ApplyMasks(maps, prompt.Mask);
        Clear(maps, prompt, zOrder);
        maps[zOrder].SetArea((0, 0, w, h), [dim]);
        maps[zOrder + 1].SetBox(prompt.Area, fill, corner, edge);
        maps[zOrder + 2].SetText((x, y + 1), text, textTint);
        RestoreMasks(maps);
    }
    public static void SetPromptItem(this IList<TileMap> maps, Prompt prompt, Button item, int zOrder = 2)
    {
        var theme = ThemePromptItems;

        if (maps.Count <= zOrder || theme == null || theme.Length == 0 || item.IsHidden)
            return;

        var index = prompt.IndexOf(item);

        var tile = new Tile(PUNCTUATION_QUESTION_MARK, Gray);
        if (index < theme.Length)
            tile = theme[index];

        maps.SetButtonIcon(item, tile, zOrder);
    }
    public static void SetPanel(this IList<TileMap> maps, Panel panel, int zOrder = 0)
    {
        if (maps.Count <= zOrder + 1 || panel.IsHidden)
            return;

        var (corner, edge, fill, textTint) = ThemePanel;
        var textPos = (panel.X + panel.Width / 2 - panel.Text.Length / 2, panel.Y);
        var text = panel.Text.Shorten(Math.Min(panel.Width, panel.Text.Length));

        ApplyMasks(maps, panel.Mask);
        Clear(maps, panel, zOrder);
        maps[zOrder].SetBox(panel.Area, fill, corner, edge);
        maps[zOrder + 1].SetText(textPos, text, textTint);
        RestoreMasks(maps);
    }
    public static void SetPalette(this IList<TileMap> maps, Palette palette, int zOrder = 0)
    {
        if (maps.Count <= zOrder + 2)
            return;

        var (full, pick, halfShade, handle) = ThemePalette;
        var resultTile = new Tile(full, palette.SelectedColor);
        var resultColor = new Color(palette.SelectedColor);

        if (resultColor.R == resultColor.G && resultColor.G == resultColor.B)
            resultColor = Yellow;

        handle.Tint = resultColor.ToOpposite();

        pick.Tint = palette.Pick.GetInteractionColor(pick.Tint, InteractionShade);

        ApplyMasks(maps, palette.Mask);
        Clear(maps, palette, zOrder);
        maps[zOrder].SetArea(palette.Opacity.Area, [halfShade]);
        maps[zOrder + 1].SetArea(palette.Opacity.Area, [resultTile]);
        maps[zOrder + 2].SetTile(palette.Opacity.Handle.Position, handle, palette.Mask);
        maps[zOrder + 2].SetTile(palette.Brightness.Handle.Position, handle, palette.Mask);

        for (var i = 0; i < palette.Width; i++)
        {
            var tile = new Tile(full, palette.GetSample(i));
            maps[zOrder + 1].SetTile((palette.X + i, palette.Y + 1), tile, palette.Mask);
        }

        for (var i = 0; i < palette.Brightness.Width; i++)
        {
            var col = new Color((byte)i.Map((0, palette.Brightness.Width - 1), (0, 255)));
            var cell = (palette.Brightness.X + i, palette.Brightness.Y);
            maps[zOrder].SetTile(cell, new(FULL, col), palette.Mask);
        }

        if (palette.Pick.IsHidden == false)
            maps[zOrder + 1].SetTile(palette.Pick.Position, pick, palette.Mask);
        RestoreMasks(maps);
    }
    public static void SetPages(this IList<TileMap> maps, Pages pages, int zOrder = 0)
    {
        if (maps.Count <= zOrder)
            return;

        var (first, previous, next, last) = ThemePages;

        first.Tint = pages.First.GetInteractionColor(first.Tint, InteractionShade);
        previous.Tint = pages.Previous.GetInteractionColor(previous.Tint, InteractionShade);
        next.Tint = pages.Next.GetInteractionColor(next.Tint, InteractionShade);
        last.Tint = pages.Last.GetInteractionColor(last.Tint, InteractionShade);

        Clear(maps, pages, zOrder);

        if (pages.First.IsHidden == false)
            maps[zOrder].SetTile(pages.First.Position, first, pages.Mask);
        if (pages.Previous.IsHidden == false)
            maps[zOrder].SetTile(pages.Previous.Position, previous, pages.Mask);
        if (pages.Next.IsHidden == false)
            maps[zOrder].SetTile(pages.Next.Position, next, pages.Mask);
        if (pages.Last.IsHidden == false)
            maps[zOrder].SetTile(pages.Last.Position, last, pages.Mask);
    }
    public static void SetPagesItem(this IList<TileMap> maps, Pages pages, Button item, int zOrder = 0)
    {
        if (maps.Count <= zOrder)
            return;

        var color = GetInteractionColor(item, item.IsSelected ? Green : Gray.ToBright(0.2f), InteractionShade);
        var text = item.Text.ToNumber().PadZeros(-pages.ItemWidth);
        text = text.Constrain(item.Size, alignment: Alignment.Center);

        ApplyMasks(maps, item.Mask);
        maps[zOrder].SetText(item.Position, text, color);
        RestoreMasks(maps);
    }
    public static void SetList(this IList<TileMap> maps, List list, int zOrder = 0)
    {
        if (maps.Count <= zOrder + 2 || list.IsHidden)
            return;

        var (left, fill, right, arrow) = ThemeListBar;
        var arrowPos = (list.X + list.Width - 1, list.Y);
        var sel = list.SelectedItems;
        Block obj = list.IsCollapsed && sel.Count > 0 && sel[0].IsHovered ? sel[0] : list;

        arrow.Tint = obj.GetInteractionColor(arrow.Tint, InteractionShade);
        left.Tint = obj.GetInteractionColor(left.Tint, InteractionShade);
        fill.Tint = obj.GetInteractionColor(fill.Tint, InteractionShade);
        right.Tint = obj.GetInteractionColor(right.Tint, InteractionShade);

        ApplyMasks(maps, list.Mask);
        Clear(maps, list, zOrder);

        if (list.IsScrollAvailable)
            SetScroll(maps, list.Scroll, zOrder + 1);

        if (list.IsCollapsed)
        {
            maps[zOrder].SetBar(list.Position, left, fill, right, list.Width);
            maps[zOrder + 2].SetTile(arrowPos, arrow, list.Mask);
        }
        else
        {
            if (list.Height == 1)
                maps[zOrder].SetBar(list.Position, left, fill, right, list.Width);
            else
                maps[zOrder].SetPatch(list.Area, ThemeListPatch);
        }

        RestoreMasks(maps);
    }
    public static void SetListItem(this IList<TileMap> maps, List list, Button item, int zOrder = 1, bool showSelected = true)
    {
        if (maps.Count <= zOrder || item.IsHidden)
            return;

        var (tint, select, disable) = ThemeListText;
        var color = item.IsSelected && showSelected ? select : tint;
        var isLeftCrop = list.Span == Span.Horizontal &&
                         item.Width < list.ItemSize.width &&
                         item.Position == list.Position;
        var text = item.Text.Shorten(item.Size.width * (isLeftCrop ? -1 : 1));
        var pos = (item.X, item.Y + item.Height / 2);

        color = item.GetInteractionColor(item.IsDisabled ? disable : color, InteractionShade);

        ApplyMasks(maps, item.Mask);
        maps[zOrder].SetText(pos, text, color);
        RestoreMasks(maps);
    }
    public static void SetLayoutSegment(this IList<TileMap> maps, (int x, int y, int width, int height) segment, int index, bool showIndex, int zOrder = 0)
    {
        var color = new Color(
            (byte)(20, 200).Random(seed / (index + 1f)),
            (byte)(20, 200).Random(seed / (index + 2f)),
            (byte)(20, 200).Random(seed / (index + 3f)));

        ApplyMasks(maps, segment);
        maps[zOrder].SetBox(segment, new(FULL, color), new(BOX_CORNER, color), new(FULL, color));

        if (showIndex)
        {
            var text = index.ToString().Constrain((segment.width, segment.height), alignment: Alignment.Center);
            maps[zOrder + 1].SetText((segment.x, segment.y), text);
        }

        RestoreMasks(maps);
    }

    public static Color GetInteractionColor(this Block block, Color baseColor, float amount = 0.15f)
    {
        var hotkeyIsPressed = block is Button btn &&
                              ((Key)btn.Hotkey.id).IsPressed() &&
                              KeysPressed.Length == 1 &&
                              Input.IsTyping == false;

        if (block.IsDisabled) return baseColor;
        if (block.IsPressedAndHeld || hotkeyIsPressed) return baseColor.ToDark(amount);
        if (block.IsHovered) return baseColor.ToBright(amount);

        return baseColor;
    }

#region Backend
    private static readonly int seed = (-1_000_000, 1_000_000).Random();
    private static readonly List<Area?> masks = [];

    static TileMapperUI()
    {
        var g = Gray;
        var dg = g.ToDark();
        var ddg = Gray.ToDark(0.6f);
        var dark = g.ToDark(0.7f);
        var dim = Black.ToTransparent();
        var arrow = new Tile(ARROW_TAILLESS_ROUND, g);
        var line = new Tile(SHAPE_LINE, g).Rotate(1);
        const ushort CORNER = PIPE_HOLLOW_CORNER!;
        const ushort STRAIGHT = PIPE_HOLLOW_STRAIGHT!;

        InteractionShade = 0.15f;
        ThemeScrollArrow = arrow;
        ThemeButtonBox = (new(BOX_SHADOW_CORNER, g), new(BOX_SHADOW_EDGE, g), new(FULL, g), g.ToBright());
        ThemeButtonBar = (line, line, line, g);
        ThemeInputBox = (new(FULL, dg), new(SHAPE_LINE, White, Pose.Down), g.ToBright(), selectionTint: Blue);
        ThemeCheckbox = (new(ICON_TICK, Green), new(ICON_X, Red));
        ThemeSwitch = (new(ARROW_TAILLESS_ROUND, White), Green, dg);
        ThemeSlider = (new(BAR_BIG_EDGE, g), new(BAR_BIG_STRAIGHT, g), new(BAR_BIG_EDGE, g, Pose.Down), new(SHAPE_CIRCLE_BIG, g.ToBright()));
        ThemeTooltip = (new(BOX_CORNER, dg), new(FULL, dg), new(FULL, dg), tintText: White);
        ThemePages = (new(MATH_MUCH_LESS, g), new(MATH_LESS, g), new(MATH_GREATER, g), new(MATH_MUCH_GREATER, g));
        ThemePrompt = (new(BOX_CORNER, dg), new(FULL, dg), new(FULL, dg), new(FULL, dim), tintText: White);
        ThemePromptItems = [new(ICON_YES, Green), new(ICON_NO, Red)];
        ThemePalette = (FULL, new(ICON_PICK, g), new(SHADE_5, dg), new(SHAPE_CIRCLE_SMALL, g));
        ThemePanel = (new(BOX_CORNER, ddg), new(BOX_EDGE, ddg), new(FULL, ddg), tintText: White);
        ThemeListText = (g.ToBright(0.3f), Green, g.ToDark(0.3f));
        ThemeListBar = (new(FULL, dark), new(FULL, dark), new(FULL, dark), new(MATH_GREATER, g, Pose.Right));
        ThemeListPatch = new Tile[,]
        {
            { new(FULL, dark), new(FULL, dark), new(FULL, dark) },
            { new(FULL, dark), new(FULL, dark), new(FULL, dark) },
            { new(FULL, dark), new(FULL, dark), new(FULL, dark) }
        };
        ThemeButtonPatch = (new[,]
        {
            { new(CORNER, g), new(STRAIGHT, g), new Tile(CORNER, g).Rotate(1) },
            { new Tile(STRAIGHT, g).Rotate(1), new(), new Tile(STRAIGHT, g).Rotate(1) },
            { new Tile(CORNER, g).Rotate(3), new Tile(STRAIGHT, g).Rotate(2), new Tile(CORNER, g).Rotate(2) }
        }, g.ToBright());

        var min = new Tile(MATH_MUCH_LESS, g);
        var mid = new Tile(PUNCTUATION_PIPE, g);
        var max = new Tile(MATH_MUCH_GREATER, g);
        ThemeStepper = (new(BOX_CORNER, dg), new(FULL, dg), arrow, min, mid, max, tintText: g, valueTint: White);

        var img = new Tile(ICON_PICTURE, Cyan);
        var audio = new Tile(AUDIO_NOTES_BEAMED_EIGHT, Purple.ToBright(0.35f));
        var font = new Tile(UPPERCASE_F, Violet);
        var txt = new Tile(ALIGN_HORIZONTAL_LEFT, Azure);
        var zip = new Tile(ICON_STACK_2, Brown);
        var vid = new Tile(ICON_CAMERA_MOVIE, Red);
        var cfg = new Tile(ICON_SETTINGS, Yellow.ToDark());
        var exe = new Tile(FLOW_PLAY, Orange);
        ThemeFileViewer = (img, audio, font, txt, zip, vid, cfg, exe);
    }

    private static void ApplyMasks(IList<TileMap> maps, Area? mask)
    {
        masks.Clear();
        for (var i = 0; i < maps.Count; i++)
        {
            masks.Add(maps[i].GetMask());
            maps[i].ApplyMask(mask);
        }
    }
    private static void RestoreMasks(IList<TileMap> maps)
    {
        for (var i = 0; i < maps.Count; i++)
            maps[i].ApplyMask(masks[i]);
    }

    private static void Clear(IList<TileMap> maps, Block block, int zOrder)
    {
        var (x, y) = block.Position;
        var (w, h) = block.Size;

        ApplyMasks(maps, block.Mask);
        for (var i = zOrder; i < zOrder + 3; i++)
            if (i < maps.Count)
                maps[i].SetArea((x, y, w, h), [EMPTY]);
        RestoreMasks(maps);
    }
#endregion
}