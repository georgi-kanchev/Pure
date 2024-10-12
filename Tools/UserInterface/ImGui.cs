using Pure.Engine.Tilemap;
using Pure.Engine.UserInterface;
using Pure.Engine.Window;
using Pure.Tools.Tilemap;

namespace Pure.Tools.ImmediateGraphicalUserInterface;

public static class ImGui
{
    public static Tile Cursor { get; set; } = new(546, 3789677055);

    public static void ShowText((int x, int y) cell, string text, int zOrder = 0, uint tint = 4294967295U, char tintBrush = '#', Area? mask = null)
    {
        if (maps.Tilemaps.Count <= zOrder)
            return;

        maps.Tilemaps[zOrder].SetText(cell, text, tint, tintBrush, mask);
    }
    public static bool ShowButton((int x, int y) position, string text, Interaction trueWhen = Interaction.Trigger)
    {
        var block = TryCache<Button>(text, (position.x, position.y, text.Length + 2, 1));
        block.Update();
        maps.SetButton(block);
        return block.IsJustInteracted(trueWhen);
    }
    public static bool ShowCheckbox((int x, int y) position, string text)
    {
        var block = TryCache<Button>(text, (position.x, position.y, text.Length + 2, 1));
        block.Update();
        maps.SetCheckbox(block);
        return block.IsSelected;
    }
    public static string ShowInputBox(Area area, string placeholder = "Type…")
    {
        var block = TryCache<InputBox>("", area);
        block.Placeholder = placeholder;
        block.Update();
        maps.SetInputBox(block);

        if (block.Height == 1)
            return block.IsJustInteracted(Interaction.Select) ? block.Value : "";

        return block.Value;
    }
    public static float ShowSlider((int x, int y) position, int size, bool vertical = false)
    {
        var (w, h) = (vertical ? 1 : size, vertical ? size : 1);
        var block = TryCache<Slider>("", (position.x, position.y, w, h));
        block.Update();
        maps.SetSlider(block);
        return block.IsJustInteracted(Interaction.Select) ? block.Progress : float.NaN;
    }
    public static float ShowScroll((int x, int y) position, int size, bool vertical = false)
    {
        var (w, h) = (vertical ? 1 : size, vertical ? size : 1);
        var block = TryCache<Scroll>("", (position.x, position.y, w, h));
        block.Update();
        maps.SetScroll(block);
        return block.IsJustInteracted(Interaction.Select) ? block.Slider.Progress : float.NaN;
    }
    public static float ShowStepper((int x, int y) position, string text, float step = 1f, float min = float.MinValue, float max = float.MaxValue)
    {
        var (w, h) = (text.Length + 1, 2);
        var block = TryCache<Stepper>(text, (position.x, position.y, w, h));
        block.Step = step;
        block.Range = (min, max);
        block.Update();
        maps.SetStepper(block);
        return block.IsJustInteracted(Interaction.Select) ? block.Value : float.NaN;
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

    private static T TryCache<T>(string text, Area area) where T : Block
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
        }

        var cache = imGuiCache[area];
        imGuiCache[area] = (2, cache.block); // reset frame timer
        cache.block.Area = area;
        return (T)cache.block;
    }
#endregion
}