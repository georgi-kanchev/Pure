using Pure.Engine.Utility;
using Pure.Engine.Window;

namespace Pure.Examples.Systems;

public static class Rendering
{
    public static void Run()
    {
        Window.Title = "Pure - Rendering Example";
        Window.PixelScale = 1f;
        Mouse.IsCursorVisible = true;

        var layer = new Layer { TexturePath = "Characters.png" };
        var animation = new (int x, int y, int w, int h)[]
        {
            (0, 0, 26, 39),
            (26, 0, 26, 39),
            (26 * 2, 0, 26, 39),
            (26 * 3, 0, 26, 39)
        };

        while (Window.KeepOpen())
        {
            Time.Update();

            layer.DragAndZoom();
            layer.DrawTextureArea(animation.Animate(5f), layer.MouseCursorPosition);
            layer.Draw();
        }
    }
}