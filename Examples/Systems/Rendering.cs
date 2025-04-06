using Pure.Engine.Utility;
using Pure.Engine.Window;
using Monitor = Pure.Engine.Window.Monitor;

namespace Pure.Examples.Systems;

public static class Rendering
{
    public static void Run()
    {
        Window.Title = "Pure - Rendering Example";
        Window.PixelScale = 1f;
        Mouse.IsCursorVisible = true;

        LayerTiles.DefaultGraphicsToFile("default-graphics.png");

        var (w, h) = Monitor.Current.Size;
        var layer = new LayerSprites((w / 3, h / 3)) { Effect = new(), TexturePath = "default-graphics.png" };

        while (Window.KeepOpen())
        {
            Time.Update();

            var (x, y) = layer.MouseCursorPosition;
            layer.DrawLine([(x, y), (0, 0)], 1);
            layer.Render();
        }
    }
}