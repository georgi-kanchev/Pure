using Pure.Engine.Window;
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
    public static Tile DisablingOverlay { get; set; }
    public static Color TintSelection { get; set; }
    public static Color TintText { get; set; }

    public static (Tile corner, Tile edge, Tile fill) ThemeButtonBox { get; set; }
    public static Tile[,] ThemeButtonPatch { get; set; }
    public static (Tile left, Tile fill, Tile right) ThemeButtonBar { get; set; }
    public static (Tile background, Tile cursor) ThemeInputBox { get; set; }
    public static (Tile on, Tile off) ThemeCheckbox { get; set; }
    public static (Tile arrow, Color tintOn, Color tintOff) ThemeSwitch { get; set; }
    public static (Tile edge1, Tile fill, Tile edge2, Tile handle) ThemeSlider { get; set; }
    public static Tile ThemeScrollArrow { get; set; }
    public static (Tile corner, Tile fill, Tile arrow, Tile min, Tile mid, Tile max) ThemeStepper { get; set; }
    public static (Tile corner, Tile edge, Tile fill) ThemeTooltipBox { get; set; }
    public static (Tile left, Tile fill, Tile right) ThemeTooltipBar { get; set; }
    public static (Tile first, Tile previous, Tile next, Tile last) ThemePages { get; set; }
    public static (Tile corner, Tile edge, Tile fill, Tile dim) ThemePrompt { get; set; }
    public static Tile[]? ThemePromptItems { get; set; }
    public static (Tile full, Tile pick, Tile halfShade, Tile handle) ThemePalette { get; set; }
    public static (Tile corner, Tile edge, Tile fill) ThemePanel { get; set; }
    public static Tile[,] ThemeListPatch { get; set; }
    public static (Tile left, Tile fill, Tile right, Tile arrow) ThemeListBar { get; set; }
    public static (Tile img, Tile audio, Tile font, Tile txt, Tile zip, Tile vid, Tile cfg, Tile exe) ThemeFileViewer { get; set; }

    public static void SetTooltip(this IList<TileMap> maps, Tooltip tooltip, int zOrder = 4)
    {
        if (maps.Count <= zOrder + 2 || tooltip.IsHidden)
            return;

        var (boxCorner, boxEdge, boxFill) = ThemeTooltipBox;
        var (barLeft, barFill, barRight) = ThemeTooltipBar;
        var (x, y, w, _) = tooltip.Area;

        ApplyMasks(maps, tooltip.Mask);
        Clear(maps, tooltip, zOrder, 3);

        if (tooltip.Height == 1)
            maps[zOrder].SetBar((x, y), barLeft, barFill, barRight, w);
        else
            maps[zOrder].SetBox(tooltip.Area, boxFill, boxCorner, boxEdge);

        maps[zOrder + 1].SetText((x + 1, y), tooltip.Text, TintText);
        TryDisable(maps, tooltip, zOrder + 2);
        RestoreMasks(maps);
    }
    public static void SetCheckbox(this IList<TileMap> maps, Button button, int zOrder = 1)
    {
        if (maps.Count <= zOrder + 1 || button.IsHidden)
            return;

        var (on, off) = ThemeCheckbox;
        var tile = button.IsSelected ? on : off;
        var textPos = (button.X + 2, button.Y);

        tile.Tint = button.GetInteractionColor(tile.Tint, InteractionShade);

        ApplyMasks(maps, button.Mask);
        Clear(maps, button, zOrder, 2);
        maps[zOrder].SetTile(button.Position, tile, button.Mask);
        maps[zOrder].SetText(textPos, button.Text, tile.Tint);
        TryDisable(maps, button, zOrder + 1);
        RestoreMasks(maps);
    }
    public static void SetSwitch(this IList<TileMap> maps, Button button, char arrowAtSymbol = ' ', int zOrder = 1)
    {
        if (maps.Count <= zOrder + 1 || button.IsHidden)
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
            Clear(maps, button, zOrder, 2);
            maps[zOrder].SetTile(arrowPos, arrow, button.Mask);
            maps[zOrder].SetText((x, y), split[0], on);
            maps[zOrder].SetText((x + split[0].Length + 1, y), split[1], off);
        }

        TryDisable(maps, button, zOrder + 1);
        RestoreMasks(maps);
    }
    public static void SetButton(this IList<TileMap> maps, Button button, int zOrder = 1, bool selectable = false)
    {
        if (maps.Count <= zOrder + 2 || button.IsHidden)
            return;

        var (w, h) = button.Size;
        var offsetW = w / 2 - Math.Min(button.Text.Length, h == 1 ? w : w - 2) / 2;
        var (boxCorner, boxEdge, boxFill) = ThemeButtonBox;
        var (barLeft, barFill, barRight) = ThemeButtonBar;
        var text = button.Text.Shorten(h == 1 ? w : w - 2);
        var textPos = (button.X + offsetW, button.Y + h / 2);
        var isBar = button.Height == 1;
        var sel = button.IsSelected && selectable;
        var (rTextTint, bTextTint) = (TintText, TintText);
        var selTint = TintSelection;

        ApplyMasks(maps, button.Mask);
        Clear(maps, button, zOrder, 3);
        if (isBar)
        {
            barLeft.Tint = button.GetInteractionColor(sel ? selTint : barLeft.Tint, InteractionShade);
            barFill.Tint = button.GetInteractionColor(sel ? selTint : barFill.Tint, InteractionShade);
            barRight.Tint = button.GetInteractionColor(sel ? selTint : barRight.Tint, InteractionShade);
            rTextTint = button.GetInteractionColor(sel ? selTint.ToBright() : rTextTint, InteractionShade);
            maps[zOrder].SetBar(button.Position, barLeft, barFill, barRight, button.Width);
        }
        else
        {
            boxCorner.Tint = button.GetInteractionColor(sel ? selTint : boxCorner.Tint, InteractionShade);
            boxEdge.Tint = button.GetInteractionColor(sel ? selTint : boxEdge.Tint, InteractionShade);
            boxFill.Tint = button.GetInteractionColor(sel ? selTint : boxFill.Tint, InteractionShade);
            bTextTint = button.GetInteractionColor(sel ? selTint.ToBright() : bTextTint, InteractionShade);
            maps[zOrder].SetBox(button.Area, boxFill, boxCorner, boxEdge);
        }

        maps[zOrder + 1].SetText(textPos, text, isBar ? rTextTint : bTextTint);
        TryDisable(maps, button, zOrder + 2);
        RestoreMasks(maps);
    }
    public static void SetButtonTile(this IList<TileMap> maps, Button button, Tile tile, int zOrder = 0, bool selectable = false)
    {
        if (maps.Count <= zOrder + 1 || button.IsHidden)
            return;

        tile.Tint = button.GetInteractionColor(button.IsSelected && selectable ? TintSelection : tile.Tint, InteractionShade);
        Clear(maps, button, zOrder, 2);
        TryDisable(maps, button, zOrder + 1);
        maps[zOrder].SetTile(button.Position, tile, button.Mask);
    }
    public static void SetInputBox(this IList<TileMap> maps, InputBox inputBox, int zOrder = 0)
    {
        if (maps.Count <= zOrder + 2 || inputBox.IsHidden)
            return;

        var (background, cursor) = ThemeInputBox;
        var selectColor = inputBox.IsFocused ? TintSelection : TintSelection.ToBright();
        var selection = inputBox.Selection.Constrain(inputBox.Size, false);
        var text = inputBox.Text.Constrain(inputBox.Size, false);
        var placeholder = inputBox.Placeholder?.Constrain(inputBox.Size);
        var scrollY = inputBox.ScrollIndices.y;
        var textAboveOrBelow = new Tile(background.Id, new Color(background.Tint).ToDark(0.3f));
        var (w, h) = inputBox.Size;
        var cursorPos = inputBox.PositionFromIndices(inputBox.CursorIndices);
        var placeholderTint = TintText.ToDark();

        background.Tint = inputBox.GetInteractionColor(background.Tint, InteractionShade / 2f);

        ApplyMasks(maps, inputBox.Mask);
        Clear(maps, inputBox, zOrder, 3);
        maps[zOrder].SetArea(inputBox.Area, [background]);
        maps[zOrder].SetText(inputBox.Position, selection, selectColor);
        maps[zOrder + 1].SetText(inputBox.Position, text, TintText);

        if (string.IsNullOrEmpty(inputBox.Value))
            maps[zOrder + 1].SetText(inputBox.Position, placeholder, placeholderTint);

        if (scrollY > 0)
            maps[zOrder + 0].SetArea((inputBox.X, inputBox.Y, w, 1), [textAboveOrBelow]);

        if (scrollY < inputBox.LineCount - inputBox.Height)
            maps[zOrder + 0].SetArea((inputBox.X, inputBox.Y + h - 1, w, 1), [textAboveOrBelow]);

        if (inputBox.IsCursorVisible)
            maps[zOrder + 2].SetTile(cursorPos, cursor, inputBox.Mask);
        TryDisable(maps, inputBox, zOrder + 2);
        RestoreMasks(maps);
    }
    public static void SetFileViewerItem(this IList<TileMap> maps, FileViewer fileViewer, Button item, int zOrder = 1)
    {
        if (maps.Count <= zOrder + 1 || item.IsHidden)
            return;

        var (img, audio, font, txt, zip, vid, cfg, exe) = ThemeFileViewer;
        var isFolder = fileViewer.IsFolder(item);
        var isHardDrive = fileViewer.HardDrives.Items.Contains(item);
        var isFileOrFolder = fileViewer.FilesAndFolders.Items.Contains(item);
        var color = item.IsSelected && isFileOrFolder ? TintSelection : TintText;
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
        Clear(maps, item, zOrder, 2);
        maps[zOrder].SetTile(item.Position, icon, item.Mask);
        maps[zOrder].SetText((item.X + 1, item.Y), text, color);
        TryDisable(maps, item, zOrder + 1);
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
        if (maps.Count <= zOrder + 2 || fileViewer.IsHidden)
            return;

        ApplyMasks(maps, fileViewer.Mask);
        Clear(maps, fileViewer, zOrder, 3);
        maps.SetList(fileViewer.FilesAndFolders, zOrder);
        maps.SetFileViewerItem(fileViewer, fileViewer.User, zOrder + 1);
        maps.SetFileViewerItem(fileViewer, fileViewer.Back, zOrder + 1);
        TryDisable(maps, fileViewer, zOrder + 2);
        RestoreMasks(maps);
    }
    public static void SetSlider(this IList<TileMap> maps, Slider slider, int zOrder = 0)
    {
        if (maps.Count <= zOrder + 2 || slider.IsHidden)
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
        Clear(maps, slider, zOrder, 3);
        maps[zOrder].SetBar(slider.Position, edge1, fill, edge2, size, slider.IsVertical);
        maps[zOrder + 1].SetTile(slider.Handle.Position, handle, slider.Mask);
        TryDisable(maps, slider, zOrder + 2);
        RestoreMasks(maps);
    }
    public static void SetScroll(this IList<TileMap> maps, Scroll scroll, int zOrder = 0)
    {
        if (maps.Count <= zOrder + 2 || scroll.IsHidden)
            return;

        var arrow = ThemeScrollArrow;
        var scrollUpAngle = scroll.IsVertical ? Pose.Right : Pose.Default;
        var scrollDownAngle = scroll.IsVertical ? Pose.Left : Pose.Down;
        var up = scroll.Increase.Position;
        var down = scroll.Decrease.Position;

        var upTint = scroll.Increase.GetInteractionColor(arrow.Tint, InteractionShade);
        var downTint = scroll.Decrease.GetInteractionColor(arrow.Tint, InteractionShade);

        ApplyMasks(maps, scroll.Mask);
        Clear(maps, scroll, zOrder, 3);
        maps.SetSlider(scroll.Slider, zOrder);
        maps[zOrder + 1].SetTile(up, new(arrow.Id, upTint, scrollUpAngle), scroll.Mask);
        maps[zOrder + 1].SetTile(down, new(arrow.Id, downTint, scrollDownAngle), scroll.Mask);
        TryDisable(maps, scroll, zOrder + 2);
        RestoreMasks(maps);
    }
    public static void SetStepper(this IList<TileMap> maps, Stepper stepper, int zOrder = 0)
    {
        if (maps.Count <= zOrder + 2 || stepper.IsHidden)
            return;

        var (corner, fill, arrow, min, mid, max) = ThemeStepper;
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
        Clear(maps, stepper, zOrder, 3);
        maps[zOrder].SetBox(stepper.Area, fill, corner, fill);
        maps[zOrder + 1].SetTile(upPos, new(arrow.Id, upTint, Pose.Left), mask);
        maps[zOrder + 1].SetTile(downPos, new(arrow.Id, downTint, Pose.Right), mask);
        maps[zOrder + 1].SetText((x + 1, y), text, TintText.ToDark());
        maps[zOrder + 1].SetText((x + 1, y + 1), value, TintText);
        maps[zOrder + 1].SetTile(stepper.Minimum.Position, min, mask);
        maps[zOrder + 1].SetTile(stepper.Middle.Position, mid, mask);
        maps[zOrder + 1].SetTile(stepper.Maximum.Position, max, mask);
        TryDisable(maps, stepper, zOrder + 2);
        RestoreMasks(maps);
    }
    public static void SetPrompt(this IList<TileMap> maps, Prompt prompt, int zOrder = 0)
    {
        if (maps.Count <= zOrder + 3 || prompt.IsHidden)
            return;

        var (corner, edge, fill, dim) = ThemePrompt;
        var newLines = prompt.Text.Count("\n") + 1;
        var text = prompt.Text.Constrain((prompt.Width, newLines), alignment: Alignment.Center);
        var (w, h) = Input.Bounds;
        var (x, y) = prompt.Position;

        ApplyMasks(maps, prompt.Mask);
        Clear(maps, prompt, zOrder, 4);
        maps[zOrder].SetArea((0, 0, w, h), [dim]);
        maps[zOrder + 1].SetBox(prompt.Area, fill, corner, edge);
        maps[zOrder + 2].SetText((x, y + 1), text, TintText);
        TryDisable(maps, prompt, zOrder + 3);
        RestoreMasks(maps);
    }
    public static void SetPromptItem(this IList<TileMap> maps, Prompt prompt, Button item, int zOrder = 2)
    {
        var theme = ThemePromptItems;

        if (maps.Count <= zOrder + 1 || theme == null || theme.Length == 0 || item.IsHidden)
            return;

        var index = prompt.IndexOf(item);

        var tile = new Tile(PUNCTUATION_QUESTION_MARK, Gray);
        if (index < theme.Length)
            tile = theme[index];

        ApplyMasks(maps, item.Mask);
        Clear(maps, item, zOrder, 2);
        maps.SetButtonTile(item, tile, zOrder);
        TryDisable(maps, item, zOrder + 1);
        RestoreMasks(maps);
    }
    public static void SetPanel(this IList<TileMap> maps, Panel panel, int zOrder = 0)
    {
        if (maps.Count <= zOrder + 2 || panel.IsHidden)
            return;

        var (corner, edge, fill) = ThemePanel;
        var (tx, ty) = (panel.X + panel.Width / 2 - panel.Text.Length / 2, panel.Y);
        var text = panel.Text.Shorten(Math.Min(panel.Width, panel.Text.Length));

        tx = Math.Clamp(tx, panel.X, panel.X + panel.Width);

        ApplyMasks(maps, panel.Mask);
        Clear(maps, panel, zOrder, 3);
        maps[zOrder].SetBox(panel.Area, fill, corner, edge);
        maps[zOrder + 1].SetText((tx, ty), text, TintText);
        TryDisable(maps, panel, zOrder + 2);
        RestoreMasks(maps);
    }
    public static void SetPalette(this IList<TileMap> maps, Palette palette, int zOrder = 0)
    {
        if (maps.Count <= zOrder + 3)
            return;

        var (full, pick, halfShade, handle) = ThemePalette;
        var resultTile = new Tile(full, palette.SelectedColor);
        var resultColor = new Color(palette.SelectedColor);

        if (resultColor.R == resultColor.G && resultColor.G == resultColor.B)
            resultColor = Yellow;

        handle.Tint = resultColor.ToOpposite();

        pick.Tint = palette.Pick.GetInteractionColor(pick.Tint, InteractionShade);

        ApplyMasks(maps, palette.Mask);
        Clear(maps, palette, zOrder, 4);
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
        TryDisable(maps, palette, zOrder + 3);
        RestoreMasks(maps);
    }
    public static void SetPages(this IList<TileMap> maps, Pages pages, int zOrder = 0)
    {
        if (maps.Count <= zOrder + 1)
            return;

        var (first, previous, next, last) = ThemePages;

        first.Tint = pages.First.GetInteractionColor(first.Tint, InteractionShade);
        previous.Tint = pages.Previous.GetInteractionColor(previous.Tint, InteractionShade);
        next.Tint = pages.Next.GetInteractionColor(next.Tint, InteractionShade);
        last.Tint = pages.Last.GetInteractionColor(last.Tint, InteractionShade);

        ApplyMasks(maps, pages.Mask);
        Clear(maps, pages, zOrder, 2);

        if (pages.First.IsHidden == false)
            maps[zOrder].SetTile(pages.First.Position, first, pages.Mask);
        if (pages.Previous.IsHidden == false)
            maps[zOrder].SetTile(pages.Previous.Position, previous, pages.Mask);
        if (pages.Next.IsHidden == false)
            maps[zOrder].SetTile(pages.Next.Position, next, pages.Mask);
        if (pages.Last.IsHidden == false)
            maps[zOrder].SetTile(pages.Last.Position, last, pages.Mask);

        TryDisable(maps, pages, zOrder + 1);
        RestoreMasks(maps);
    }
    public static void SetPagesItem(this IList<TileMap> maps, Pages pages, Button item, int zOrder = 1)
    {
        if (maps.Count <= zOrder + 1)
            return;

        var color = GetInteractionColor(item, item.IsSelected ? TintSelection : TintText, InteractionShade);
        var text = item.Text.ToNumber().PadZeros(-pages.ItemWidth);
        text = text.Constrain(item.Size, alignment: Alignment.Center);

        ApplyMasks(maps, item.Mask);
        Clear(maps, item, zOrder, 2);
        maps[zOrder].SetText(item.Position, text, color);
        TryDisable(maps, item, zOrder + 1);
        RestoreMasks(maps);
    }
    public static void SetPagesItemTile(this IList<TileMap> maps, Pages pages, Button item, Tile[] tiles, int zOrder = 1)
    {
        if (tiles.Length == 0)
            return;

        var index = Math.Clamp(pages.IndexOf(item), 0, tiles.Length - 1);
        SetButtonTile(maps, item, tiles[index], zOrder, true);
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
        Clear(maps, list, zOrder, 3);

        if (list.IsScrollAvailable)
            SetScroll(maps, list.Scroll, zOrder + 1);

        if (list.IsCollapsed)
        {
            maps[zOrder].SetBar(list.Position, left, fill, right, list.Width);
            maps[zOrder + 1].SetTile(arrowPos, arrow, list.Mask);
        }
        else
        {
            if (list.Height == 1)
                maps[zOrder].SetBar(list.Position, left, fill, right, list.Width);
            else
                maps[zOrder].SetPatch(list.Area, ThemeListPatch);
        }

        TryDisable(maps, list, zOrder + 2);
        RestoreMasks(maps);
    }
    public static void SetListItem(this IList<TileMap> maps, List list, Button item, int zOrder = 1, bool selectable = true)
    {
        if (maps.Count <= zOrder + 1 || item.IsHidden)
            return;

        var color = item.IsSelected && selectable ? TintSelection : TintText;
        var isLeftCrop = list.Span == Span.Horizontal &&
                         item.Width < list.ItemSize.width &&
                         item.Position == list.Position;
        var text = item.Text.Shorten(item.Size.width * (isLeftCrop ? -1 : 1));
        var pos = (item.X, item.Y + item.Height / 2);

        color = item.GetInteractionColor(color, InteractionShade);

        ApplyMasks(maps, item.Mask);
        Clear(maps, item, zOrder, 2);
        maps[zOrder].SetText(pos, text, color);
        TryDisable(maps, item, zOrder + 1);
        RestoreMasks(maps);
    }

    public static Color GetInteractionColor(this Block block, Color baseColor, float amount = 0.15f)
    {
        var hotkeyIsPressed = block is Button btn &&
                              ((Key)btn.Hotkey.id).IsPressed() &&
                              KeysPressed.Length == 1 &&
                              Input.IsTyping == false;

        if (block.IsDisabled) return baseColor.ToDark();
        if (block.IsPressedAndHeld || hotkeyIsPressed) return baseColor.ToDark(amount);
        if (block.IsHovered) return baseColor.ToBright(amount);

        return baseColor;
    }

#region Backend
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
        DisablingOverlay = new(DISABLED_2, Black.ToTransparent());
        TintSelection = Azure;
        TintText = g.ToBright();

        ThemeScrollArrow = arrow;
        ThemeButtonBox = (new(BOX_SHADOW_CORNER, g), new(BOX_SHADOW_EDGE, g), new(FULL, g));
        ThemeButtonBar = (line, line, line);
        ThemeInputBox = (new(FULL, ddg.ToDark(0.3f)), new(SHAPE_LINE, White, Pose.Down));
        ThemeCheckbox = (new(ICON_TICK, Green), new(ICON_X, Red));
        ThemeSwitch = (new(ARROW_TAILLESS_ROUND, White), Green, dg);
        ThemeSlider = (new(BAR_BIG_EDGE, g), new(BAR_BIG_STRAIGHT, g), new(BAR_BIG_EDGE, g, Pose.Down), new(SHAPE_CIRCLE_BIG, g.ToBright()));
        ThemeTooltipBox = (new(BOX_BIG_CORNER, dg), new(FULL, dg), new(FULL, dg));
        ThemeTooltipBar = (new(BAR_BIG_EDGE, dg), new(FULL, dg), new(BAR_BIG_EDGE, dg, Pose.Down));
        ThemePages = (new(MATH_MUCH_LESS, g), new(MATH_LESS, g), new(MATH_GREATER, g), new(MATH_MUCH_GREATER, g));
        ThemePrompt = (new(BOX_CORNER, ddg), new(BOX_EDGE, ddg), new(FULL, dg), new(FULL, dim));
        ThemePromptItems = [new(ICON_YES, Green), new(ICON_NO, Red)];
        ThemePalette = (FULL, new(ICON_PICK, g), new(SHADE_5, dg), new(SHAPE_CIRCLE_SMALL, g));
        ThemePanel = (new(BOX_CORNER, ddg), new(BOX_EDGE, ddg), new(FULL, dg));
        ThemeListBar = (new(FULL, dark), new(FULL, dark), new(FULL, dark), new(MATH_GREATER, g, Pose.Right));
        ThemeListPatch = new Tile[,]
        {
            { new(FULL, dark), new(FULL, dark), new(FULL, dark) },
            { new(FULL, dark), new(FULL, dark), new(FULL, dark) },
            { new(FULL, dark), new(FULL, dark), new(FULL, dark) }
        };
        ThemeButtonPatch = new[,]
        {
            { new(CORNER, g), new(STRAIGHT, g), new Tile(CORNER, g).Rotate(1) },
            { new Tile(STRAIGHT, g).Rotate(1), new(), new Tile(STRAIGHT, g).Rotate(1) },
            { new Tile(CORNER, g).Rotate(3), new Tile(STRAIGHT, g).Rotate(2), new Tile(CORNER, g).Rotate(2) }
        };

        var min = new Tile(MATH_MUCH_LESS, g);
        var mid = new Tile(PUNCTUATION_PIPE, g);
        var max = new Tile(MATH_MUCH_GREATER, g);
        ThemeStepper = (new(BOX_BIG_CORNER, dg), new(FULL, dg), arrow, min, mid, max);

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

    private static void Clear(IList<TileMap> maps, Block block, int zOrder, int depth)
    {
        var (x, y) = block.Position;
        var (w, h) = block.Size;

        ApplyMasks(maps, block.Mask);
        for (var i = zOrder; i < Math.Min(zOrder + depth + 1, maps.Count); i++)
            maps[i].SetArea((x, y, w, h), [EMPTY]);
        RestoreMasks(maps);
    }
    private static void TryDisable(IList<TileMap> maps, Block block, int zOrder)
    {
        if (block.IsDisabled)
            maps[zOrder].SetArea(block.Area, [DisablingOverlay]);
    }
#endregion
}