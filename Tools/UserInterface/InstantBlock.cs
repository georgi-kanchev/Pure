﻿using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
using Pure.Engine.Window;
using Pure.Tools.Tiles;
using static Pure.Engine.UserInterface.SymbolGroup;

namespace Pure.Tools.UserInterface;

public static class InstantBlock
{
    public static List<TileMap> TileMaps { get; } = [];
    public static Tile Cursor { get; set; } = new(546, 3789677055);
    public static string? CurrentPrompt { get; private set; }

    public static (string? text, Pivot side, float alignment) Tooltip { get; set; } = ("", Pivot.Top, 0.5f);

    public static void Text(this string? text, (int x, int y) cell = default, int zOrder = 0, uint tint = uint.MaxValue, char tintBrush = '#')
    {
        if (zOrder < TileMaps.Count)
            TileMaps[zOrder].SetText(cell, text, tint, tintBrush);
    }
    public static bool Button(Area area, string text, Interaction trueWhen = Interaction.Trigger)
    {
        var block = TryCache<Button>(text, area, out _);
        TileMaps.SetButton(block);
        return block.IsJustInteracted(trueWhen);
    }
    public static bool? Checkbox((int x, int y) cell, string text, bool selected = false)
    {
        var block = TryCache<Button>(text, (cell.x, cell.y, text.Length + 2, 1), out var cached);

        if (cached == false)
            block.IsSelected = selected;

        TileMaps.SetCheckbox(block);
        return block.IsJustInteracted(Interaction.Select) ? block.IsSelected : null;
    }
    public static bool? Switch((int x, int y) cell, (string left, string right) text, bool right = false)
    {
        var label = $"{text.left}—{text.right}";
        var block = TryCache<Button>(label, (cell.x, cell.y, label.Length, 1), out var cached);

        if (cached == false)
            block.IsSelected = right;

        block.Text = label;
        TileMaps.SetSwitch(block, '—');
        return block.IsJustInteracted(Interaction.Select) ? block.IsSelected : null;
    }
    public static string? InputBox(Area area, string? value = "", string? placeholder = "Type…", SymbolGroup symbolGroup = All, int symbolLimit = int.MaxValue)
    {
        var block = TryCache<InputBox>("", area, out var cached);
        block.Placeholder = placeholder;
        block.SymbolGroup = symbolGroup;
        block.SymbolLimit = symbolLimit;

        if (cached == false)
            block.Value = value;

        TileMaps.SetInputBox(block);

        if (block.Height == 1)
            return block.IsJustValueChanged ? block.Value : null;

        return block.Value;
    }
    public static float Slider((int x, int y) cell, int size, float progress = 0, bool vertical = false)
    {
        var (w, h) = (vertical ? 1 : size, vertical ? size : 1);
        var block = TryCache<Slider>("", (cell.x, cell.y, w, h), out var cached);

        if (cached == false)
            block.Progress = progress;

        TileMaps.SetSlider(block);
        return block.IsJustInteracted(Interaction.Select) ? block.Progress : float.NaN;
    }
    public static float Scroll((int x, int y) cell, int size, float progress = 0, bool vertical = false)
    {
        var (w, h) = (vertical ? 1 : size, vertical ? size : 1);
        var block = TryCache<Scroll>("", (cell.x, cell.y, w, h), out var cached);

        if (cached == false)
            block.Slider.Progress = progress;

        TileMaps.SetScroll(block);
        return block.IsJustInteracted(Interaction.Select) ? block.Slider.Progress : float.NaN;
    }
    public static float Stepper((int x, int y) cell, string text, float value = 0, float step = 1f, float min = float.MinValue, float max = float.MaxValue)
    {
        var (w, h) = (text.Length + 1, 2);
        var block = TryCache<Stepper>(text, (cell.x, cell.y, w, h), out var cached);
        block.Step = step;
        block.Range = (min, max);

        if (cached == false)
            block.Value = value;

        TileMaps.SetStepper(block);
        return block.IsJustInteracted(Interaction.Select) ? block.Value : float.NaN;
    }
    public static float Pages((int x, int y) cell, int size = 12, int current = 0, int totalCount = 10, int itemWidth = 2)
    {
        var (w, h) = (size, 1);
        var block = TryCache<Pages>("", (cell.x, cell.y, w, h), out var cached);
        block.Count = totalCount;
        block.ItemWidth = itemWidth;

        if (cached == false)
            block.Current = current;

        return block.IsJustInteracted(Interaction.Select) ? block.Current : float.NaN;
    }
    public static string[]? List(Area area, string[]? items, bool dropdown = false, bool multiselect = true, string[]? selected = null)
    {
        if (items == null || items.Length == 0)
            return null;

        var block = TryCache<List>("", area, out var cached, dropdown ? Span.Dropdown : Span.Vertical);

        if (items.Length != block.Items.Count)
        {
            block.Items.Clear();
            for (var i = 0; i < items.Length; i++)
                block.Items.Add(new());
        }

        block.Edit(items);
        block.IsSingleSelecting = multiselect == false;
        block.ItemSize = (area.Width, 1);
        block.ItemGap = 0;

        if (cached == false)
        {
            var selectedList = (selected ?? []).ToList();
            foreach (var item in block.Items)
                if (selectedList.Contains(item.Text))
                    block.Select(item);
        }

        if (block.IsJustInteracted(Interaction.Select) == false)
            return null;

        var result = new List<string>();
        foreach (var item in block.SelectedItems)
            result.Add(item.Text);

        return result.ToArray();
    }
    public static uint? Palette((int x, int y) cell, uint selectedColor = uint.MaxValue)
    {
        var block = TryCache<Palette>("", (cell.x, cell.y, 1, 1), out var cached);
        block.Pick.IsHidden = true;
        block.Pick.IsDisabled = true;

        if (cached == false)
            block.SelectedColor = selectedColor;

        return block.IsJustInteracted(Interaction.Select) ? block.SelectedColor : null;
    }
    public static string[]? FileViewer(Area area, string? fileFilter = null, bool multiSelect = false, string[]? selected = null)
    {
        var block = TryCache<FileViewer>("", area, out var cached);
        block.FileFilter = fileFilter;
        block.FilesAndFolders.IsSingleSelecting = multiSelect == false;

        if (cached)
            return block.IsJustInteracted(Interaction.Select) ? block.SelectedPaths : null;

        var selectedList = (selected ?? []).ToList();
        foreach (var item in block.FilesAndFolders.Items)
            if (selectedList.Contains(item.Text))
                block.FilesAndFolders.Select(item);

        return block.IsJustInteracted(Interaction.Select) ? block.SelectedPaths : null;
    }
    public static string[]? FolderViewer(Area area, bool multiSelect = false, string[]? selected = null)
    {
        var block = TryCache<FileViewer>("", area, out var cached);
        block.IsSelectingFolders = true;
        block.FilesAndFolders.IsSingleSelecting = multiSelect == false;

        if (cached)
            return block.IsJustInteracted(Interaction.Select) ? block.SelectedPaths : null;

        var selectedList = (selected ?? []).ToList();
        foreach (var item in block.FilesAndFolders.Items)
            if (selectedList.Contains(item.Text))
                block.FilesAndFolders.Select(item);

        return block.IsJustInteracted(Interaction.Select) ? block.SelectedPaths : null;
    }

    public static string? PromptInput(string key, string text, int width = 20, SymbolGroup symbolGroup = All)
    {
        if (CurrentPrompt != key)
            return null;

        var prompt = TryCache<Prompt>(text, (-1, 0, 1, 1), out _);
        var input = TryCache<InputBox>("", (-2, 0, width, 1), out _, skipUpdate: true);
        input.SymbolGroup = symbolGroup;
        TileMaps.SetInputBox(input, 3);

        if (prompt.IsHidden)
            prompt.Open(input, onButtonTrigger: index =>
            {
                if (index == 0)
                    input.Interact(Interaction.Custom1);

                CurrentPrompt = null;
            });

        return input.IsJustInteracted(Interaction.Custom1) ? input.Value : null;
    }
    public static float PromptChoice(string key, string text, int choiceAmount = 2)
    {
        if (CurrentPrompt != key)
            return float.NaN;

        var prompt = TryCache<Prompt>(text, (-3, 0, 1, 1), out _);

        if (choiceAmount == 1)
            Keyboard.Key.Escape.OnPress(() => prompt.TriggerButton(0));

        if (prompt.IsHidden)
            prompt.Open(null, true, choiceAmount, 0, 1, index => lastChoice = index);

        if (float.IsNaN(lastChoice))
            return float.NaN;

        var result = lastChoice;
        lastChoice = float.NaN;
        CurrentPrompt = null;
        return result;
    }
    public static void Prompt(string key)
    {
        CurrentPrompt = key;
    }

    public static void DrawGUI(this LayerTiles layerTiles)
    {
        if (TileMaps.Count == 0 || layerTiles.Size != TileMaps[0].Size)
        {
            TileMaps.Clear();
            for (var i = 0; i < 6; i++)
                TileMaps.Add(new(layerTiles.Size));
        }

        Mouse.CursorCurrent = (Mouse.Cursor)Input.CursorResult;

        Input.ApplyMouse(layerTiles.Size, layerTiles.MousePosition, Mouse.ButtonIdsPressed, Mouse.ScrollDelta);
        Input.ApplyKeyboard(Keyboard.KeyIdsPressed, Keyboard.KeyTyped, Window.Clipboard);

        var toRemove = new List<string>();
        foreach (var (key, value) in imGuiCache)
        {
            var cache = value;
            cache.framesLeft--;
            imGuiCache[key] = cache;

            if (cache.framesLeft <= 0)
                toRemove.Add(key);
        }

        foreach (var cacheKey in toRemove)
            imGuiCache.Remove(cacheKey);

        TileMaps.ForEach(map => layerTiles.DrawTileMap(map));
        layerTiles.DrawMouseCursor(Cursor.Id, Cursor.Tint);
        layerTiles.Render();
        TileMaps.ForEach(map => map.Flush());
    }

    public static void ConfigureText(ushort lowercase = Tile.LOWERCASE_A, ushort uppercase = Tile.UPPERCASE_A, ushort numbers = Tile.NUMBER_0)
    {
        TileMaps.ForEach(map => map.ConfigureText(lowercase, uppercase, numbers));
    }
    public static void ConfigureText(ushort firstTileId, string symbols)
    {
        TileMaps.ForEach(map => map.ConfigureText(firstTileId, symbols));
    }

#region Backend
    private static float lastChoice = float.NaN;
    private static readonly Dictionary<string, (int framesLeft, Block block)> imGuiCache = [];

    private static T TryCache<T>(string text, Area area, out bool wasCached, Span span = Span.Vertical, bool skipUpdate = false) where T : Block
    {
        var type = typeof(T);
        wasCached = true;

        var key = $"{area} {type.Name} {text}";
        if (imGuiCache.ContainsKey(key) == false)
        {
            wasCached = false;
            if (type == typeof(Button))
                imGuiCache[key] = (2, new Button { Text = text });
            else if (type == typeof(InputBox))
                imGuiCache[key] = (2, new InputBox());
            else if (type == typeof(Slider))
                imGuiCache[key] = (2, new Slider((0, 0), area.Width == 1));
            else if (type == typeof(Scroll))
                imGuiCache[key] = (2, new Scroll((0, 0), area.Width == 1));
            else if (type == typeof(Stepper))
                imGuiCache[key] = (2, new Stepper { Text = text });
            else if (type == typeof(Tooltip))
                imGuiCache[key] = (2, new Tooltip { Text = text });
            else if (type == typeof(FileViewer))
            {
                var fileViewer = new FileViewer();
                imGuiCache[key] = (2, fileViewer);

                fileViewer.OnDisplay += () => TileMaps.SetFileViewer(fileViewer);
                fileViewer.FilesAndFolders.OnItemDisplay += item => TileMaps.SetFileViewerItem(fileViewer, item);
                fileViewer.HardDrives.OnItemDisplay += item => TileMaps.SetFileViewerItem(fileViewer, item);
            }
            else if (type == typeof(Palette))
            {
                var palette = new Palette();
                imGuiCache[key] = (2, palette);
                palette.OnDisplay += () => TileMaps.SetPalette(palette);
            }
            else if (type == typeof(Prompt))
            {
                var prompt = new Prompt { Text = text };
                prompt.AlignInside((0.5f, 0.5f));
                prompt.OnDisplay += () => TileMaps.SetPrompt(prompt, 3);
                prompt.OnItemDisplay += item => TileMaps.SetPromptItem(prompt, item, 5);
                imGuiCache[key] = (2, prompt);
            }
            else if (type == typeof(Pages))
            {
                var pages = new Pages();
                imGuiCache[key] = (2, pages);
                pages.OnDisplay += () => TileMaps.SetPages(pages);
                pages.OnItemDisplay += page => TileMaps.SetPagesItem(pages, page);
            }
            else if (type == typeof(List))
            {
                var list = new List((0, 0), 0, span);
                imGuiCache[key] = (2, list);
                list.OnDisplay += () => TileMaps.SetList(list);
                list.OnItemDisplay += item => TileMaps.SetListItem(list, item);
                list.Area = area; // set for dropdown only once
            }
        }

        var cache = imGuiCache[key];
        var isDropdown = cache.block is List { Span: Span.Dropdown };
        imGuiCache[key] = (2, cache.block); // reset frame timer
        if (isDropdown == false)
            cache.block.Area = area;
        cache.block.Text = text;

        if (skipUpdate)
            cache.block.AlignInside((0.5f, 0.5f));
        else
            cache.block.Update();

        if (cache.block is Prompt || type == typeof(Prompt))
            return (T)cache.block;

        if (cache.block.IsHovered == false ||
            type == typeof(Tooltip) ||
            string.IsNullOrWhiteSpace(Tooltip.text))
            return (T)cache.block;

        var tooltip = TryCache<Tooltip>(Tooltip.text ?? "", (0, 0, 1, 1), out _);
        tooltip.Pivot = Tooltip.side;
        tooltip.Alignment = Tooltip.alignment;
        tooltip.Show(cache.block.Area);
        TileMaps.SetTooltip(tooltip);

        return (T)cache.block;
    }
#endregion
}