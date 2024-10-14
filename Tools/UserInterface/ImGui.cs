using Pure.Engine.Tilemap;
using Pure.Engine.UserInterface;
using Pure.Engine.Utilities;
using Pure.Engine.Window;
using Pure.Tools.Tilemap;

namespace Pure.Tools.ImmediateGraphicalUserInterface;

public static class ImGui
{
    public static Tile Cursor { get; set; } = new(546, 3789677055);

    public static (Side side, float alignment) Tooltip { get; set; } = (Side.Top, 0.5f);

    public static void ShowText((int x, int y) cell, string text, int zOrder = 0, uint tint = 4294967295U, char tintBrush = '#', Area? mask = null)
    {
        if (maps.Tilemaps.Count <= zOrder)
            return;

        maps.Tilemaps[zOrder].SetText(cell, text, tint, tintBrush, mask);
    }
    public static bool ShowButton((int x, int y) position, string text, Interaction trueWhen = Interaction.Trigger, string tooltip = "")
    {
        var block = TryCache<Button>(text, (position.x, position.y, text.Length + 2, 1), tooltip);
        maps.SetButton(block);
        return block.IsJustInteracted(trueWhen);
    }
    public static bool ShowCheckbox((int x, int y) position, string text, string tooltip = "")
    {
        var block = TryCache<Button>(text, (position.x, position.y, text.Length + 2, 1), tooltip);
        maps.SetCheckbox(block);
        return block.IsSelected;
    }
    public static string? ShowInputBox(Area area, string placeholder = "Type…", string tooltip = "")
    {
        var block = TryCache<InputBox>("", area, tooltip);
        block.Placeholder = placeholder;
        maps.SetInputBox(block);

        if (block.Height == 1)
            return block.IsJustInteracted(Interaction.Select) ? block.Value : null;

        return block.Value;
    }
    public static float ShowSlider((int x, int y) position, int size, bool vertical = false, string tooltip = "")
    {
        var (w, h) = (vertical ? 1 : size, vertical ? size : 1);
        var block = TryCache<Slider>("", (position.x, position.y, w, h), tooltip);
        maps.SetSlider(block);
        return block.IsJustInteracted(Interaction.Select) ? block.Progress : float.NaN;
    }
    public static float ShowScroll((int x, int y) position, int size, bool vertical = false, string tooltip = "")
    {
        var (w, h) = (vertical ? 1 : size, vertical ? size : 1);
        var block = TryCache<Scroll>("", (position.x, position.y, w, h), tooltip);
        maps.SetScroll(block);
        return block.IsJustInteracted(Interaction.Select) ? block.Slider.Progress : float.NaN;
    }
    public static float ShowStepper((int x, int y) position, string text, float step = 1f, float min = float.MinValue, float max = float.MaxValue, string tooltip = "")
    {
        var (w, h) = (text.Length + 1, 2);
        var block = TryCache<Stepper>(text, (position.x, position.y, w, h), tooltip);
        block.Step = step;
        block.Range = (min, max);
        maps.SetStepper(block);
        return block.IsJustInteracted(Interaction.Select) ? block.Value : float.NaN;
    }
    public static float ShowPages((int x, int y) position, int size = 12, int totalCount = 10, int itemWidth = 2, string tooltip = "")
    {
        var (w, h) = (size, 1);
        var block = TryCache<Pages>("", (position.x, position.y, w, h), tooltip);
        block.Count = totalCount;
        block.ItemWidth = itemWidth;
        return block.IsJustInteracted(Interaction.Select) ? block.Current : float.NaN;
    }
    public static string[]? ShowList(Area area, string[]? items, bool dropdown = false, bool multiselect = true, string tooltip = "")
    {
        if (items == null || items.Length == 0)
            return null;

        var block = TryCache<List>("", area, tooltip, dropdown ? Span.Dropdown : Span.Vertical);
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

    public static void DrawImGui(this Layer layer)
    {
        if (layer.Size != maps.Size)
            maps = new(6, layer.Size);

        Mouse.CursorCurrent = (Mouse.Cursor)Input.CursorResult;

        Input.TilemapSize = layer.Size;
        Input.PositionPrevious = Input.Position;
        Input.Position = layer.PixelToPosition(Mouse.CursorPosition);
        Input.Update(Mouse.ButtonIdsPressed, Mouse.ScrollDelta, Keyboard.KeyIdsPressed, Keyboard.KeyTyped);

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

#region Backend
    private static TilemapPack maps = new(0, (0, 0));
    private static readonly Dictionary<Area, (int framesLeft, Block block)> imGuiCache = [];

    private static T TryCache<T>(string text, Area area, string tooltip, Span span = Span.Vertical) where T : Block
    {
        if (imGuiCache.ContainsKey(area) == false)
        {
            if (typeof(T) == typeof(Button))
                imGuiCache[area] = (2, new Button { Text = text });
            else if (typeof(T) == typeof(InputBox))
                imGuiCache[area] = (2, new InputBox());
            else if (typeof(T) == typeof(Slider))
                imGuiCache[area] = (2, new Slider(vertical: area.Width == 1));
            else if (typeof(T) == typeof(Scroll))
                imGuiCache[area] = (2, new Scroll(vertical: area.Width == 1));
            else if (typeof(T) == typeof(Stepper))
                imGuiCache[area] = (2, new Stepper { Text = text });
            else if (typeof(T) == typeof(Tooltip))
                imGuiCache[area] = (2, new Tooltip { Text = text });
            else if (typeof(T) == typeof(Pages))
            {
                var pages = new Pages();
                imGuiCache[area] = (2, pages);
                pages.OnDisplay(() => maps.SetPages(pages));
                pages.OnItemDisplay(page => maps.SetPagesItem(pages, page));
            }
            else if (typeof(T) == typeof(List))
            {
                var list = new List(span: span);
                imGuiCache[area] = (2, list);
                list.OnDisplay(() => maps.SetList(list));
                list.OnItemDisplay(item => maps.SetListItem(list, item));
            }
        }

        var cache = imGuiCache[area];
        imGuiCache[area] = (2, cache.block); // reset frame timer
        cache.block.Area = area;
        cache.block.Update();

        if (string.IsNullOrWhiteSpace(tooltip) == false && cache.block.IsHovered)
            ShowTooltip(area, tooltip);

        return (T)cache.block;
    }
    private static void ShowTooltip((int x, int y, int width, int height) aroundArea, string text)
    {
        var block = TryCache<Tooltip>(text, (0, 0, 1, 1), "");
        block.Side = Tooltip.side;
        block.Alignment = Tooltip.alignment;
        block.Show(aroundArea);
        maps.SetTooltip(block);
    }
#endregion
}