using Pure.Engine.Tilemap;
using Pure.Engine.UserInterface;
using Pure.Engine.Window;
using Pure.Tools.Tilemap;

namespace Pure.Tools.ImmediateGraphicalUserInterface;

public static class ImmediateGraphicalUserInterface
{
    public static Tile Cursor { get; set; } = new(546, 3789677055);

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
    private static readonly Dictionary<Area, (int framesLeft, Block block)> imGuiCache = [];
    private static TilemapPack maps = new(0, (0, 0));

    private static T TryCache<T>(string text, Area area) where T : Block
    {
        var key = (text, area);
        if (imGuiCache.ContainsKey(area) == false)
        {
            if (typeof(T) == typeof(Button))
                imGuiCache[area] = (2, new Button { Text = text });
            else if (typeof(T) == typeof(InputBox))
                imGuiCache[area] = (2, new InputBox());
        }

        var cache = imGuiCache[area];
        imGuiCache[area] = (2, cache.block); // reset frame timer
        cache.block.Area = area;
        return (T)cache.block;
    }
#endregion
}