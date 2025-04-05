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

        // LayerTiles.DefaultGraphicsToFile("default-graphics.png");

        var (w, h) = Monitor.Current.Size;
        var layer = new LayerSprites((w / 3, h / 3)) { Effect = new() /*TexturePath = "default-graphics.png"*/ };
        layer.Effect.ColorOutline(Color.Red, Edge.AllEdgesAndCorners);
        layer.Effect.Wave((127, 127), (255, 255));

        var angle = 0f;
        while (Window.KeepOpen())
        {
            Time.Update();
            angle += Time.Delta * 10;

            var (x, y) = layer.MouseCursorPosition;
            layer.DrawRectangle((x, y, 600, 600), tint: Color.Red);
            layer.Render();
        }
    }
}