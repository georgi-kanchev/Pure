using Pure.Engine.Tilemap;
using Pure.Engine.UserInterface;
using Pure.Engine.Window;
using Pure.Tools.Tilemap;

namespace Pure.Tools.ImmediateGraphicalUserInterface;

public static class GUI
{
    public static Tile Cursor { get; set; } = new(546, 3789677055);

    public static (string? text, Side side, float alignment) Tooltip { get; set; } = ("", Side.Top, 0.5f);

    public static void Text((int x, int y) cell, string text, int zOrder = 0, uint tint = 4294967295U, char tintBrush = '#', Area? mask = null)
    {
        if (maps.Tilemaps.Count <= zOrder)
            return;

        maps.Tilemaps[zOrder].SetText(cell, text, tint, tintBrush);
    }
    public static bool Button((int x, int y, int width, int height) area, string text, Interaction trueWhen = Interaction.Trigger)
    {
        var block = TryCache<Button>(text, area);
        maps.SetButton(block);
        return block.IsJustInteracted(trueWhen);
    }
    public static bool? Checkbox((int x, int y) cell, string text)
    {
        var block = TryCache<Button>(text, (cell.x, cell.y, text.Length + 2, 1));
        maps.SetCheckbox(block);
        return block.IsJustInteracted(Interaction.Select) ? block.IsSelected : null;
    }
    public static string? InputBox(Area area, string placeholder = "Type…", SymbolGroup symbolGroup = SymbolGroup.All, int symbolLimit = int.MaxValue)
    {
        var block = TryCache<InputBox>("", area);
        block.Placeholder = placeholder;
        block.SymbolGroup = symbolGroup;
        block.SymbolLimit = symbolLimit;
        maps.SetInputBox(block);

        if (block.Height == 1)
            return block.IsJustInteracted(Interaction.Select) ? block.Value : null;

        return block.Value;
    }
    public static float Slider((int x, int y) cell, int size, bool vertical = false)
    {
        var (w, h) = (vertical ? 1 : size, vertical ? size : 1);
        var block = TryCache<Slider>("", (cell.x, cell.y, w, h));
        maps.SetSlider(block);
        return block.IsJustInteracted(Interaction.Select) ? block.Progress : float.NaN;
    }
    public static float Scroll((int x, int y) cell, int size, bool vertical = false)
    {
        var (w, h) = (vertical ? 1 : size, vertical ? size : 1);
        var block = TryCache<Scroll>("", (cell.x, cell.y, w, h));
        maps.SetScroll(block);
        return block.IsJustInteracted(Interaction.Select) ? block.Slider.Progress : float.NaN;
    }
    public static float Stepper((int x, int y) cell, string text, float step = 1f, float min = float.MinValue, float max = float.MaxValue)
    {
        var (w, h) = (text.Length + 1, 2);
        var block = TryCache<Stepper>(text, (cell.x, cell.y, w, h));
        block.Step = step;
        block.Range = (min, max);
        maps.SetStepper(block);
        return block.IsJustInteracted(Interaction.Select) ? block.Value : float.NaN;
    }
    public static float Pages((int x, int y) cell, int size = 12, int totalCount = 10, int itemWidth = 2)
    {
        var (w, h) = (size, 1);
        var block = TryCache<Pages>("", (cell.x, cell.y, w, h));
        block.Count = totalCount;
        block.ItemWidth = itemWidth;
        return block.IsJustInteracted(Interaction.Select) ? block.Current : float.NaN;
    }
    public static string[]? List(Area area, string[]? items, bool dropdown = false, bool multiselect = true)
    {
        if (items == null || items.Length == 0)
            return null;

        var block = TryCache<List>("", area, dropdown ? Span.Dropdown : Span.Vertical);
        var selected = new List<string>();

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

        if (block.IsJustInteracted(Interaction.Select) == false)
            return null;

        foreach (var item in block.SelectedItems)
            selected.Add(item.Text);

        return selected.ToArray();
    }
    public static uint? Palette((int x, int y) cell)
    {
        var block = TryCache<Palette>("", (cell.x, cell.y, 1, 1));
        block.Pick.IsHidden = true;
        block.Pick.IsDisabled = true;
        return block.IsJustInteracted(Interaction.Select) ? block.SelectedColor : null;
    }
    public static string[]? FileViewer((int x, int y, int width, int height) area, string? fileFilter = null, bool multiSelect = false)
    {
        var block = TryCache<FileViewer>("", area);
        block.FileFilter = fileFilter;
        block.FilesAndFolders.IsSingleSelecting = multiSelect == false;
        return block.IsJustInteracted(Interaction.Select) ? block.SelectedPaths : null;
    }
    public static string[]? FolderViewer((int x, int y, int width, int height) area, bool multiSelect = false)
    {
        var block = TryCache<FileViewer>("", area);
        block.IsSelectingFolders = true;
        block.FilesAndFolders.IsSingleSelecting = multiSelect == false;
        return block.IsJustInteracted(Interaction.Select) ? block.SelectedPaths : null;
    }

    public static string? PromptInput(string text, int width = 20, SymbolGroup symbolGroup = SymbolGroup.All)
    {
        if (showPrompt == false)
            return null;

        var prompt = TryCache<Prompt>(text, (-1, 0, 1, 1));
        var input = TryCache<InputBox>("", (-2, 0, width, 1), skipUpdate: true);
        input.SymbolGroup = symbolGroup;
        maps.SetInputBox(input, 3);

        if (prompt.IsHidden)
            prompt.Open(input, onButtonTrigger: index =>
            {
                if (index == 0)
                    input.Interact(Interaction.Select);

                showPrompt = false;
            });

        return input.IsJustInteracted(Interaction.Select) ? input.Value : null;
    }
    public static float PromptChoice(string text, int choiceAmount = 2)
    {
        if (showPrompt == false)
            return float.NaN;

        var prompt = TryCache<Prompt>(text, (-3, 0, 1, 1));

        if (prompt.IsHidden)
            prompt.Open(null, true, choiceAmount, -1, -2, index => lastChoice = index);

        if (float.IsNaN(lastChoice))
            return float.NaN;

        var result = lastChoice;
        lastChoice = float.NaN;
        showPrompt = false;
        return result;
    }
    public static void Prompt()
    {
        showPrompt = true;
    }

    public static void DrawImGui(this Layer layer)
    {
        if (layer.Size != maps.Size)
            maps = new(6, layer.Size);

        Mouse.CursorCurrent = (Mouse.Cursor)Input.CursorResult;

        Input.TilemapSize = layer.Size;
        Input.PositionPrevious = Input.Position;
        Input.Position = layer.PixelToPosition(Mouse.CursorPosition);
        Input.Update(Mouse.ButtonIdsPressed, Mouse.ScrollDelta,
            Keyboard.KeyIdsPressed, Keyboard.KeyTyped, Window.Clipboard);

        var toRemove = new List<Area>();
        foreach (var kvp in imGuiCache)
        {
            var cache = kvp.Value;
            cache.framesLeft--;
            imGuiCache[kvp.Key] = cache;

            if (cache.framesLeft <= 0)
                toRemove.Add(kvp.Key);
        }

        foreach (var cacheKey in toRemove)
            imGuiCache.Remove(cacheKey);

        foreach (var map in maps.Tilemaps)
            layer.DrawTilemap(map);

        layer.DrawCursor(Cursor.Id, Cursor.Tint);
        layer.Draw();
        maps.Flush();
    }

    public static void ConfigureText(int lowercase = Tile.LOWERCASE_A, int uppercase = Tile.UPPERCASE_A, int numbers = Tile.NUMBER_0)
    {
        maps.ConfigureText(lowercase, uppercase, numbers);
    }
    public static void ConfigureText(string symbols, int firstTileId)
    {
        maps.ConfigureText(symbols, firstTileId);
    }

#region Backend
    private static bool showPrompt;
    private static float lastChoice = float.NaN;
    private static TilemapPack maps = new(0, (0, 0));
    private static readonly Dictionary<Area, (int framesLeft, Block block)> imGuiCache = [];

    private static T TryCache<T>(string text, Area area, Span span = Span.Vertical, bool skipUpdate = false) where T : Block
    {
        var type = typeof(T);

        if (imGuiCache.ContainsKey(area) == false)
        {
            if (type == typeof(Button))
                imGuiCache[area] = (2, new Button { Text = text });
            else if (type == typeof(InputBox))
                imGuiCache[area] = (2, new InputBox());
            else if (type == typeof(Slider))
                imGuiCache[area] = (2, new Slider((0, 0), area.Width == 1));
            else if (type == typeof(Scroll))
                imGuiCache[area] = (2, new Scroll((0, 0), area.Width == 1));
            else if (type == typeof(Stepper))
                imGuiCache[area] = (2, new Stepper { Text = text });
            else if (type == typeof(Tooltip))
                imGuiCache[area] = (2, new Tooltip { Text = text });
            else if (type == typeof(FileViewer))
            {
                var fileViewer = new FileViewer();
                imGuiCache[area] = (2, fileViewer);
                fileViewer.OnDisplay(() => maps.SetFileViewer(fileViewer));
                fileViewer.FilesAndFolders.OnItemDisplay(item => maps.SetFileViewerItem(fileViewer, item));
                fileViewer.HardDrives.OnItemDisplay(item => maps.SetFileViewerItem(fileViewer, item));
            }
            else if (type == typeof(Palette))
            {
                var palette = new Palette();
                imGuiCache[area] = (2, palette);
                palette.OnDisplay(() => maps.SetPalette(palette));
            }
            else if (type == typeof(Prompt))
            {
                var prompt = new Prompt { Text = text, Size = maps.Size };
                prompt.AlignInside((0.5f, 0.5f));
                prompt.OnDisplay(() => maps.SetPrompt(prompt, 3));
                prompt.OnItemDisplay(item => maps.SetPromptItem(prompt, item, 5));
                imGuiCache[area] = (2, prompt);
            }
            else if (type == typeof(Pages))
            {
                var pages = new Pages();
                imGuiCache[area] = (2, pages);
                pages.OnDisplay(() => maps.SetPages(pages));
                pages.OnItemDisplay(page => maps.SetPagesItem(pages, page));
            }
            else if (type == typeof(List))
            {
                var list = new List((0, 0), span: span);
                imGuiCache[area] = (2, list);
                list.OnDisplay(() => maps.SetList(list));
                list.OnItemDisplay(item => maps.SetListItem(list, item));
            }
        }

        var cache = imGuiCache[area];
        imGuiCache[area] = (2, cache.block); // reset frame timer
        cache.block.Area = area;

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

        var tooltip = TryCache<Tooltip>(Tooltip.text ?? "", (0, 0, 1, 1));
        tooltip.Side = Tooltip.side;
        tooltip.Alignment = Tooltip.alignment;
        tooltip.Show(cache.block.Area);
        maps.SetTooltip(tooltip);

        return (T)cache.block;
    }
#endregion
}