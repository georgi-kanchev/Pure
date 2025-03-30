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

        var layer = new LayerSprites { TexturePath = "Characters.png" };
        var animation = new (int x, int y, int w, int h)[]
        {
            (0, 0, 26, 38),
            (26, 0, 26, 38),
            (26 * 2, 0, 26, 38),
            (26 * 3, 0, 26, 38)
        };

        while (Window.KeepOpen())
        {
            Time.Update();

            layer.DragAndZoom();
            layer.DrawTextureArea(animation.Animate(2f), layer.MouseCursorPosition);
            layer.Render();
        }
    }
}