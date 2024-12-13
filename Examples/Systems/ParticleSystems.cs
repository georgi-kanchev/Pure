using Pure.Engine.Utility;
using Pure.Engine.Window;

namespace Pure.Examples.Systems;

public static class ParticleSystems
{
    public static void Run()
    {
        Window.Title = "Pure - Particles Example";

        var layer = new Layer((48, 27));

        var particles = Particles.SpawnCluster(100, 5f);
        particles.MakeCircle((10f, 10f));
        particles.ApplyGravity((0f, 0.5f));
        var particles2 = Particles.SpawnCluster(100, 5f);
        particles2.MakeLine((20f, 20f, 25f, 15f));

        Mouse.Button.Left.OnPress(() => particles.FadeTo(new Color(), 0f));

        while (Window.KeepOpen())
        {
            Time.Update();

            layer.DrawPoints(particles);
            layer.DrawPoints(particles2);
            layer.DrawMouseCursor();
            layer.Draw();
        }
    }
}