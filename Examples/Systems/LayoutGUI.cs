using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
using Pure.Engine.Window;
using Pure.Tools.Tiles;
using Pure.Tools.UserInterface;
using Monitor = Pure.Engine.Window.Monitor;

namespace Pure.Examples.Systems;

public static class LayoutGUI
{
    public static void Run()
    {
        Window.Title = "Pure - Layout Graphical User Interface Example";

        var (w, h) = Monitor.Current.AspectRatio;
        var layer = new LayerTiles((w * 3, h * 3));
        var maps = new TileMap[] { new(layer.Size), new(layer.Size), new(layer.Size), new(layer.Size) };

        Input.ApplyMouse(layer.Size, layer.MousePosition, Mouse.ButtonIdsPressed, Mouse.ScrollDelta);

        var button = new Button { Size = (10, 3) };
        button.OnDisplay += () => maps.SetButton(button);

        var layoutUI = new Layout();
        layoutUI.SetContainer("window", pivot: Pivot.TopRight);
        layoutUI.SetBlock("window", nameof(button), button);

        while (Window.KeepOpen())
        {
            Input.ApplyMouse(layer.Size, layer.MousePosition, Mouse.ButtonIdsPressed, Mouse.ScrollDelta);
            Input.ApplyKeyboard(Keyboard.KeyIdsPressed, Keyboard.KeyTyped, Window.Clipboard);

            layoutUI.Update();

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