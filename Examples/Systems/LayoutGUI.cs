using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
using Pure.Engine.Window;
using Pure.Tools.Tiles;
using Monitor = Pure.Engine.Window.Monitor;

namespace Pure.Examples.Systems;

public static class LayoutGUI
{
    public static void Run()
    {
        Window.Title = "Pure - Layout Graphical User Interface Example";

        var (w, h) = Monitor.Current.AspectRatio;
        var layer = new LayerTiles((w * 3, h * 3));
        var maps = new TileMap[] { new(layer.Size), new(layer.Size), new(layer.Size) };

        var panel = new Panel() { IsDisabled = true };
        panel.OnDisplay += () => maps.SetPanel(panel);

        while (Window.KeepOpen())
        {
            Input.TileMapSize = layer.Size;
            Input.PositionPrevious = Input.Position;
            Input.Position = layer.PositionFromPixel(Mouse.CursorPosition);
            Input.Update(Mouse.ButtonIdsPressed, Mouse.ScrollDelta, Keyboard.KeyIdsPressed, Keyboard.KeyTyped, Window.Clipboard);
            panel.Update();

            Mouse.CursorCurrent = (Mouse.Cursor)Input.CursorResult;

            foreach (var map in maps)
            {
                layer.DrawTileMap(map);
                map.Flush();
            }

            layer.DrawMouseCursor();
            layer.Render();
        }
    }
}