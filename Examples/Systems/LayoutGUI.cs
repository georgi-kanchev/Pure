using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
using Pure.Engine.Utility;
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

        var layoutGUI = new Layout(
            "Container: top\tPivot: TopLeft\n" +
            "\tButton: steam\t\tText: ~Steam\tSize: 6, 1\n" +
            "\tButton: view\t\tText: View\tSize: 4, 1");

        foreach (var map in layoutGUI.TileMaps)
            map.ConfigureText(Tile.ICON_ZAP, "~");

        while (Window.KeepOpen())
        {
            layoutGUI.DrawGUI(layer);
            layer.Render();
        }
    }
}