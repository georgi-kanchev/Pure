using Pure.Engine.Utility;
using Pure.Engine.Window;

namespace Pure.Examples.Systems;

public static class ParticleSystems
{
    public static void Run()
    {
        Window.Title = "Pure - Particles Example";

        var layer = new Layer((48, 27));

        Particles.SpawnInCircle(100, (10f, 10f));

        while (Window.KeepOpen())
        {
            Time.Update();
            layer.DrawPoints(Particles.ToBundle());
            layer.DrawMouseCursor();
            layer.Draw();
        }
    }
}