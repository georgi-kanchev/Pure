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

        foreach (var map in maps)
            map.ConfigureText(Tile.ICON_ZAP, "~");

        Input.ApplyMouse(layer.Size, layer.MousePosition, Mouse.ButtonIdsPressed, Mouse.ScrollDelta);

        var steam = new Button { Size = (6, 1), Text = "~Steam" };
        steam.OnDisplay += () => maps.SetButton(steam);

        var view = new Button { Size = (4, 1), Text = "View" };
        view.OnDisplay += () => maps.SetButton(view);

        var layoutUI = new Layout();
        layoutUI.SetContainer("top", pivot: Pivot.TopLeft);
        layoutUI.SetBlock("top", "steam", steam);
        layoutUI.SetBlock("top", "view", view);

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