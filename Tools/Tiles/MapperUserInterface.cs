using Pure.Engine.Window;
using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
using Pure.Engine.Utility;
using static Pure.Engine.Tiles.Tile;
using static Pure.Engine.Utility.Color;
using static Pure.Engine.Window.Keyboard;

namespace Pure.Tools.Tiles;

public static class MapperUserInterface
{
    public static bool IsInteractable { get; set; } = true;

    public static (Tile corner, Tile edge, Tile fill, uint tintText) ThemeButtonBox { get; set; }
    public static (Tile edge, Tile fill, uint tintText) ThemeButtonBar { get; set; }
    public static (Tile background, Tile cursor, uint tintText, uint selectionTint) ThemeInputBox { get; set; }
    public static (Tile on, Tile off) ThemeCheckbox { get; set; }
    public static (Tile arrow, uint tintOn, uint tintOff) ThemeSwitch { get; set; }
    public static (Tile edge, Tile fill, Tile handle) ThemeSlider { get; set; }
    public static Tile ThemeScrollArrow { get; set; }
    public static (Tile corner, Tile fill, Tile arrow, Tile min, Tile mid, Tile max, uint tintText, uint valueTint) ThemeStepper { get; set; }
    public static (Tile corner, Tile edge, Tile fill, uint tintText) ThemeTooltip { get; set; }
    public static (Tile first, Tile previous, Tile next, Tile last) ThemePages { get; set; }
    public static (Tile corner, Tile edge, Tile fill, Tile dim, uint tintText) ThemePrompt { get; set; }
    public static Tile[]? ThemePromptItems { get; set; }
    public static (Tile full, Tile pick, Tile halfShade, Tile handle) ThemePalette { get; set; }
    public static (Tile corner, Tile edge, Tile fill, uint tintText) ThemePanel { get; set; }
    public static (Tile background, Tile arrow, uint tint, uint tintSelect, uint tintDisable) ThemeList { get; set; }
    public static (Tile img, Tile audio, Tile font, Tile txt, Tile zip, Tile vid, Tile cfg, Tile exe) ThemeFileViewer { get; set; }

    public static void SetTooltip(this TileMapPack maps, Tooltip tooltip, int zOrder = 1)
    {
        if (maps.TileMaps.Count <= zOrder + 1 || tooltip.IsHidden)
            return;

        var (corner, edge, fill, textTint) = ThemeTooltip;
        var (x, y) = tooltip.Position;

        Clear(maps, tooltip, zOrder);
        maps.TileMaps[zOrder].SetBox(tooltip.Area, fill, corner, edge, tooltip.Mask);
        maps.TileMaps[zOrder + 1].SetText((x + 1, y), tooltip.Text, textTint, mask: tooltip.Mask);
    }
    public static void SetCheckbox(this TileMapPack maps, Button button, int zOrder = 1)
    {
        if (maps.TileMaps.Count <= zOrder || button.IsHidden)
            return;

        var (on, off) = ThemeCheckbox;
        var tile = button.IsSelected ? on : off;
        var textPos = (button.X + 2, button.Y);

        tile.Tint = button.GetInteractionColor(tile.Tint);

        Clear(maps, button, zOrder);
        maps.TileMaps[zOrder].SetTile(button.Position, tile, button.Mask);
        maps.TileMaps[zOrder].SetText(textPos, button.Text, tile.Tint, mask: button.Mask);
    }
    public static void SetSwitch(this TileMapPack maps, Button button, char arrowAtSymbol = ' ', int zOrder = 1)
    {
        if (maps.TileMaps.Count <= zOrder || button.IsHidden)
            return;

        var (arrow, on, off) = ThemeSwitch;
        var arrowPos = (button.X + button.Text.IndexOf(arrowAtSymbol), button.Y);
        var split = button.Text.Split(arrowAtSymbol);
        var (x, y) = button.Position;

        arrow.Tint = button.GetInteractionColor(arrow.Tint);
        arrow.Pose = button.IsSelected ? Pose.Default : Pose.Down;
        on = button.GetInteractionColor(on);
        off = button.GetInteractionColor(off);

        if (button.IsSelected)
            (on, off) = (off, on);

        if (split.Length != 2)
        {
            maps.TileMaps[zOrder].SetText((x, y), button.Text, on, mask: button.Mask);
            return;
        }

        Clear(maps, button, zOrder);
        maps.TileMaps[zOrder].SetTile(arrowPos, arrow, button.Mask);
        maps.TileMaps[zOrder].SetText((x, y), split[0], on, mask: button.Mask);
        maps.TileMaps[zOrder].SetText((x + split[0].Length + 1, y), split[1], off, mask: button.Mask);
    }
    public static void SetButton(this TileMapPack maps, Button button, int zOrder = 1)
    {
        if (maps.TileMaps.Count <= zOrder + 1 || button.IsHidden)
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
            maps.TileMaps[zOrder].SetBar(button.Position, rEdge, rFill, button.Width, mask: button.Mask);
        else
            maps.TileMaps[zOrder].SetBox(button.Area, bFill, bCorner, bEdge, button.Mask);

        maps.TileMaps[zOrder + 1].SetText(textPos, text, isBar ? rTextTint : bTextTint, mask: button.Mask);
    }
    public static void SetButtonIcon(this TileMapPack maps, Button button, Tile icon, int zOrder = 0)
    {
        if (maps.TileMaps.Count <= zOrder || button.IsHidden)
            return;

        icon.Tint = button.GetInteractionColor(icon.Tint);
        Clear(maps, button, zOrder);
        maps.TileMaps[zOrder].SetTile(button.Position, icon, button.Mask);
    }
    public static void SetInputBox(this TileMapPack maps, InputBox inputBox, int zOrder = 0)
    {
        if (maps.TileMaps.Count <= zOrder + 2 || inputBox.IsHidden)
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

        background.Tint = box.GetInteractionColor(background.Tint, 0.05f);

        Clear(maps, inputBox, zOrder);
        maps.TileMaps[zOrder].SetArea(box.Area, box.Mask, background);
        maps.TileMaps[zOrder].SetText(box.Position, selection, selectColor, mask: box.Mask);
        maps.TileMaps[zOrder + 1].SetText(box.Position, text, textTint, mask: box.Mask);

        if (string.IsNullOrEmpty(box.Value))
            maps.TileMaps[zOrder + 1].SetText(box.Position, placeholder, placeholderTint, mask: box.Mask);

        if (scrollY > 0)
            maps.TileMaps[zOrder + 0].SetArea((box.X, box.Y, w, 1), box.Mask, textAboveOrBelow);

        if (scrollY < box.LineCount - box.Height)
            maps.TileMaps[zOrder + 0].SetArea((box.X, box.Y + h - 1, w, 1), box.Mask, textAboveOrBelow);

        if (box.IsCursorVisible)
            maps.TileMaps[zOrder + 2].SetTile(cursorPos, cursor, box.Mask);
    }
    public static void SetFileViewerItem(this TileMapPack maps, FileViewer fileViewer, Button item, int zOrder = 1)
    {
        if (maps.TileMaps.Count <= zOrder || item.IsHidden)
            return;

        var (img, audio, font, txt, zip, vid, cfg, exe) = ThemeFileViewer;
        var (_, _, tint, select, _) = ThemeList;
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

        color = item.GetInteractionColor(color);

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

        icon.Tint = item.GetInteractionColor(icon.Tint);

        maps.TileMaps[zOrder].SetTile(item.Position, icon, item.Mask);
        maps.TileMaps[zOrder].SetText((item.X + 1, item.Y), text, color, mask: item.Mask);

        bool Ext(params string[] ext)
        {
            foreach (var ex in ext)
                if (Path.GetExtension(item.Text).Equals($".{ex}", StringComparison.CurrentCultureIgnoreCase))
                    return true;

            return false;
        }
    }
    public static void SetFileViewer(this TileMapPack maps, FileViewer fileViewer, int zOrder = 0)
    {
        if (maps.TileMaps.Count <= zOrder + 1 || fileViewer.IsHidden)
            return;

        var bg = ThemeList.background;
        bg.Tint = new Color(bg.Tint).ToDark(0.2f);

        maps.TileMaps[zOrder].SetArea(fileViewer.Area, fileViewer.Mask, bg);
        maps.SetList(fileViewer.FilesAndFolders, zOrder);
        maps.SetFileViewerItem(fileViewer, fileViewer.User, zOrder + 1);
        maps.SetFileViewerItem(fileViewer, fileViewer.Back, zOrder + 1);
    }
    public static void SetSlider(this TileMapPack maps, Slider slider, int zOrder = 0)
    {
        if (maps.TileMaps.Count <= zOrder + 1 || slider.IsHidden)
            return;

        var (edge, fill, handle) = ThemeSlider;
        var size = slider.IsVertical ? slider.Height : slider.Width;

        edge.Tint = slider.GetInteractionColor(edge.Tint, 0.3f);
        fill.Tint = slider.GetInteractionColor(fill.Tint, 0.3f);
        handle.Tint = slider.Handle.GetInteractionColor(handle.Tint, 0.3f);

        Clear(maps, slider, zOrder);
        maps.TileMaps[zOrder].SetBar(slider.Position, edge, fill, size, slider.IsVertical, slider.Mask);
        maps.TileMaps[zOrder + 1].SetTile(slider.Handle.Position, handle, slider.Mask);
    }
    public static void SetScroll(this TileMapPack maps, Scroll scroll, int zOrder = 0)
    {
        if (maps.TileMaps.Count <= zOrder + 1 || scroll.IsHidden)
            return;

        var arrow = ThemeScrollArrow;
        var scrollUpAngle = scroll.IsVertical ? Pose.Right : Pose.Default;
        var scrollDownAngle = scroll.IsVertical ? Pose.Left : Pose.Down;
        var up = scroll.Increase.Position;
        var down = scroll.Decrease.Position;

        var upTint = scroll.Increase.GetInteractionColor(arrow.Tint, 0.3f);
        var downTint = scroll.Decrease.GetInteractionColor(arrow.Tint, 0.3f);

        Clear(maps, scroll, zOrder);
        maps.SetSlider(scroll.Slider, zOrder);
        maps.TileMaps[zOrder + 1].SetTile(up, new(arrow.Id, upTint, scrollUpAngle), scroll.Mask);
        maps.TileMaps[zOrder + 1].SetTile(down, new(arrow.Id, downTint, scrollDownAngle), scroll.Mask);
    }
    public static void SetStepper(this TileMapPack maps, Stepper stepper, int zOrder = 0)
    {
        if (maps.TileMaps.Count <= zOrder + 1 || stepper.IsHidden)
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
        maps.TileMaps[zOrder].SetBox(stepper.Area, fill, corner, fill, mask);
        maps.TileMaps[zOrder + 1].SetTile(upPos, new(arrow.Id, upTint, Pose.Left), mask);
        maps.TileMaps[zOrder + 1].SetTile(downPos, new(arrow.Id, downTint, Pose.Right), mask);
        maps.TileMaps[zOrder + 1].SetText((x + 1, y), text, textTint, mask: mask);
        maps.TileMaps[zOrder + 1].SetText((x + 1, y + 1), value, valueTint, mask: mask);
        maps.TileMaps[zOrder + 1].SetTile(stepper.Minimum.Position, min, mask);
        maps.TileMaps[zOrder + 1].SetTile(stepper.Middle.Position, mid, mask);
        maps.TileMaps[zOrder + 1].SetTile(stepper.Maximum.Position, max, mask);
    }
    public static void SetPrompt(this TileMapPack maps, Prompt prompt, int zOrder = 0)
    {
        if (maps.TileMaps.Count <= zOrder + 2 || prompt.IsHidden)
            return;

        var (corner, edge, fill, dim, textTint) = ThemePrompt;
        var newLines = prompt.Text.Count("\n") + 1;
        var text = prompt.Text.Constrain((prompt.Width, newLines), alignment: Alignment.Center);

        Clear(maps, prompt, zOrder);
        maps.TileMaps[zOrder].SetArea((0, 0, maps.Size.width, maps.Size.height), prompt.Mask, dim);
        maps.TileMaps[zOrder + 1].SetBox(prompt.Area, fill, corner, edge, prompt.Mask);
        maps.TileMaps[zOrder + 2].SetText(prompt.Position, text, textTint, mask: prompt.Mask);
    }
    public static void SetPromptItem(this TileMapPack maps, Prompt prompt, Button item, int zOrder = 2)
    {
        var theme = ThemePromptItems;

        if (maps.TileMaps.Count <= zOrder || theme == null || theme.Length == 0 || item.IsHidden)
            return;

        var index = prompt.IndexOf(item);

        var tile = new Tile(PUNCTUATION_QUESTION_MARK, Gray);
        if (index < theme.Length)
            tile = theme[index];

        maps.SetButtonIcon(item, tile, zOrder);
    }
    public static void SetPanel(this TileMapPack maps, Panel panel, int zOrder = 0)
    {
        if (maps.TileMaps.Count <= zOrder + 1 || panel.IsHidden)
            return;

        var (corner, edge, fill, textTint) = ThemePanel;
        var textPos = (panel.X + panel.Width / 2 - panel.Text.Length / 2, panel.Y);
        var text = panel.Text.Shorten(Math.Min(panel.Width, panel.Text.Length));

        Clear(maps, panel, zOrder);
        maps.TileMaps[zOrder].SetBox(panel.Area, fill, corner, edge, panel.Mask);
        maps.TileMaps[zOrder + 1].SetText(textPos, text, textTint, mask: panel.Mask);
    }
    public static void SetPalette(this TileMapPack maps, Palette palette, int zOrder = 0)
    {
        if (maps.TileMaps.Count <= zOrder + 2)
            return;

        var (full, pick, halfShade, handle) = ThemePalette;
        var resultTile = new Tile(full, palette.SelectedColor);
        var resultColor = new Color(palette.SelectedColor);

        if (resultColor.R == resultColor.G && resultColor.G == resultColor.B)
            resultColor = Yellow;

        handle.Tint = resultColor.ToOpposite();

        pick.Tint = palette.Pick.GetInteractionColor(pick.Tint);

        Clear(maps, palette, zOrder);
        maps.TileMaps[zOrder].SetArea(palette.Opacity.Area, palette.Mask, halfShade);
        maps.TileMaps[zOrder + 1].SetArea(palette.Opacity.Area, palette.Mask, resultTile);
        maps.TileMaps[zOrder + 2].SetTile(palette.Opacity.Handle.Position, handle, palette.Mask);
        maps.TileMaps[zOrder + 2].SetTile(palette.Brightness.Handle.Position, handle, palette.Mask);

        for (var i = 0; i < palette.Width; i++)
        {
            var tile = new Tile(full, palette.GetSample(i));
            maps.TileMaps[zOrder + 1].SetTile((palette.X + i, palette.Y + 1), tile, palette.Mask);
        }

        for (var i = 0; i < palette.Brightness.Width; i++)
        {
            var col = new Color((byte)i.Map((0, palette.Brightness.Width - 1), (0, 255)));
            var cell = (palette.Brightness.X + i, palette.Brightness.Y);
            maps.TileMaps[zOrder].SetTile(cell, new(FULL, col), palette.Mask);
        }

        if (palette.Pick.IsHidden == false)
            maps.TileMaps[zOrder + 1].SetTile(palette.Pick.Position, pick, palette.Mask);
    }
    public static void SetPages(this TileMapPack maps, Pages pages, int zOrder = 0)
    {
        if (maps.TileMaps.Count <= zOrder)
            return;

        var (first, previous, next, last) = ThemePages;

        first.Tint = pages.First.GetInteractionColor(first.Tint);
        previous.Tint = pages.Previous.GetInteractionColor(previous.Tint);
        next.Tint = pages.Next.GetInteractionColor(next.Tint);
        last.Tint = pages.Last.GetInteractionColor(last.Tint);

        Clear(maps, pages, zOrder);

        if (pages.First.IsHidden == false)
            maps.TileMaps[zOrder].SetTile(pages.First.Position, first, pages.Mask);
        if (pages.Previous.IsHidden == false)
            maps.TileMaps[zOrder].SetTile(pages.Previous.Position, previous, pages.Mask);
        if (pages.Next.IsHidden == false)
            maps.TileMaps[zOrder].SetTile(pages.Next.Position, next, pages.Mask);
        if (pages.Last.IsHidden == false)
            maps.TileMaps[zOrder].SetTile(pages.Last.Position, last, pages.Mask);
    }
    public static void SetPagesItem(this TileMapPack maps, Pages pages, Button item, int zOrder = 0)
    {
        if (maps.TileMaps.Count <= zOrder)
            return;

        var color = GetInteractionColor(item, item.IsSelected ? Green : Gray.ToBright(0.2f));
        var text = item.Text.ToNumber().PadZeros(-pages.ItemWidth);
        text = text.Constrain(item.Size, alignment: Alignment.Center);

        maps.TileMaps[zOrder].SetText(item.Position, text, color, mask: item.Mask);
    }
    public static void SetList(this TileMapPack maps, List list, int zOrder = 0)
    {
        if (maps.TileMaps.Count <= zOrder + 2 || list.IsHidden)
            return;

        var (bg, arrow, _, _, _) = ThemeList;
        var arrowPos = (list.X + list.Width - 1, list.Y);

        arrow.Tint = list.GetInteractionColor(arrow.Tint);

        Clear(maps, list, zOrder);
        maps.TileMaps[zOrder].SetArea(list.Area, list.Mask, bg);

        if (list.IsScrollAvailable)
            SetScroll(maps, list.Scroll, zOrder + 1);

        if (list.IsCollapsed)
            maps.TileMaps[zOrder + 2].SetTile(arrowPos, arrow, list.Mask);
    }
    public static void SetListItem(this TileMapPack maps, List list, Button item, int zOrder = 1, bool showSelected = true)
    {
        if (maps.TileMaps.Count <= zOrder || item.IsHidden)
            return;

        var (_, _, tint, select, disable) = ThemeList;
        var color = item.IsSelected && showSelected ? select : tint;
        var isLeftCrop = list.Span == Span.Horizontal &&
                         item.Width < list.ItemSize.width &&
                         item.Position == list.Position;
        var text = item.Text.Shorten(item.Size.width * (isLeftCrop ? -1 : 1));
        var pos = (item.X, item.Y + item.Height / 2);

        color = item.GetInteractionColor(item.IsDisabled ? disable : color);

        maps.TileMaps[zOrder].SetText(pos, text, color, mask: item.Mask);
    }
    public static void SetLayoutSegment(this TileMapPack maps, (int x, int y, int width, int height) segment, int index, bool showIndex, int zOrder = 0)
    {
        var color = new Color(
            (byte)(20, 200).Random(seed / (index + 1f)),
            (byte)(20, 200).Random(seed / (index + 2f)),
            (byte)(20, 200).Random(seed / (index + 3f)));

        maps.TileMaps[zOrder].SetBox(
            segment, new(FULL, color), new(BOX_CORNER_ROUND, color), new(FULL, color), segment);

        if (showIndex)
            maps.TileMaps[zOrder + 1].SetText(
                (segment.x, segment.y), index.ToString().Constrain((segment.width, segment.height),
                    alignment: Alignment.Center), mask: segment);
    }

    public static Color GetInteractionColor(this Block block, Color baseColor, float amount = 0.5f)
    {
        var hotkeyIsPressed = block is Button btn &&
                              ((Key)btn.Hotkey.id).IsPressed() &&
                              KeysPressed.Length == 1 &&
                              Input.IsTyping == false;

        if (block.IsDisabled || IsInteractable == false) return baseColor;
        if (block.IsPressedAndHeld || hotkeyIsPressed) return baseColor.ToDark(amount);
        if (block.IsHovered) return baseColor.ToBright(amount);

        return baseColor;
    }

#region Backend
    private static readonly int seed = (-1_000_000, 1_000_000).Random();

    static MapperUserInterface()
    {
        var g = Gray;
        var dg = g.ToDark();
        var dim = Black.ToTransparent();
        var arrow = new Tile(ARROW_TAILLESS_ROUND, g);

        ThemeScrollArrow = arrow;
        ThemeButtonBox = (new(BOX_CORNER_ROUND, g), new(FULL, g), new(FULL, g), g.ToBright());
        ThemeButtonBar = (new(BAR_BIG_EDGE, g), new(FULL, g), g.ToBright());
        ThemeInputBox = (new(FULL, g.ToDark(0.4f)), new(SHAPE_LINE, White, Pose.Down), g.ToBright(), selectionTint: Blue);
        ThemeCheckbox = (new(ICON_TICK, Green), new(ICON_X, Red));
        ThemeSwitch = (new(ARROW_TAILLESS_ROUND, White), Green, dg);
        ThemeSlider = (new(BAR_BIG_EDGE, g), new(BAR_BIG_STRAIGHT, g), new(SHAPE_CIRCLE_BIG, g.ToBright()));
        ThemeTooltip = (new(BOX_CORNER_ROUND, dg), new(FULL, dg), new(FULL, dg), tintText: White);
        ThemePages = (new(MATH_MUCH_LESS, g), new(MATH_LESS, g), new(MATH_GREATER, g), new(MATH_MUCH_GREATER, g));
        ThemePrompt = (new(BOX_CORNER_ROUND, dg), new(FULL, dg), new(FULL, dg), new(FULL, dim), tintText: White);
        ThemePromptItems = [new(ICON_YES, Green), new(ICON_NO, Red)];
        ThemePalette = (FULL, new(ICON_PICK, g), new(SHADE_5, g.ToDark()), new(SHAPE_CIRCLE_SMALL, g));
        ThemePanel = (new(BOX_CORNER_ROUND, dg), new(FULL, dg), new(FULL, dg), tintText: White);
        ThemeList = (new(FULL, dg), new(MATH_GREATER, g, Pose.Right), g.ToBright(0.3f), Green, g.ToDark(0.3f));

        var min = new Tile(MATH_MUCH_LESS, g);
        var mid = new Tile(PUNCTUATION_PIPE, g);
        var max = new Tile(MATH_MUCH_GREATER, g);
        ThemeStepper = (new(BOX_CORNER_ROUND, dg), new(FULL, dg), arrow, min, mid, max, tintText: g, valueTint: White);

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

    private static void Clear(TileMapPack maps, Block block, int zOrder)
    {
        var (x, y) = block.Position;
        var (w, h) = block.Size;

        for (var i = zOrder; i < zOrder + 3; i++)
            if (i < maps.TileMaps.Count)
                maps.TileMaps[i].SetArea((x, y, w, h), block.Mask, EMPTY);
    }
#endregion
}