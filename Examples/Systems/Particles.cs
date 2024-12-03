using Pure.Engine.Window;

namespace Pure.Examples.Systems;

public static class Particles
{
    public static void Run()
    {
        Window.Title = "Pure - Particles Example";

        var layer = new Layer((48, 27));

        while (Window.KeepOpen())
        {
            layer.DrawMouseCursor();
            layer.Draw();
        }
    }
}