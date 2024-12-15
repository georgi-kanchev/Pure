using Pure.Engine.Tiles;
using Pure.Engine.Utility;
using Pure.Engine.Window;

namespace Pure.Examples.Systems;

public static class ParticleSystems
{
    public static void Run()
    {
        Window.Title = "Pure - Particles Example";

        var layer = new Layer((48, 27));
        var b = new Area(5, 20, 15, 1, Color.Red);
        var t = new Area(5, 5, 15, 1, Color.Red);
        var l = new Area(5, 5, 1, 15, Color.Red);
        var r = new Area(20, 5, 1, 16, Color.Red);
        // var c = new Area(15, 15, 2, 4, Color.Red);
        // var c2 = new Area(9, 12, 4, 3, Color.Red);
        var particles = Particles.SpawnCluster(27, float.PositiveInfinity);

        particles.MakeRectangle((10f, 10f, 8f, 8f), distribution: Distribution.FillRandomly);
        // particles.ApplyGravity((0f, 0.5f));
        particles.ApplyBounciness(1f);
        particles.ApplyTimeScale(0.5f);
        particles.ApplyFriction(1f);
        // particles.ApplySize(0.4f);
        particles.ApplyBounceObstacles(t, b, l, r);

        Mouse.Button.Left.OnPress(() => particles.PushFromPoint(layer.MouseCursorPosition, 3f, 3f));
        Mouse.Button.Right.OnPress(() => particles.MakeRectangle((10f, 10f, 8f, 8f), distribution: Distribution.Outline));
        Mouse.Button.Middle.OnPress(() => particles.MakeRectangle((10f, 10f, 8f, 8f), distribution: Distribution.FillRandomly));

        var randomColors = new Color[particles.Length];

        for (var i = 0; i < randomColors.Length; i++)
            randomColors[i] = Color.Random;

        while (Window.KeepOpen())
        {
            Time.Update();

            // particles.ApplyOrbit(layer.MouseCursorPosition, 3f);
            layer.DrawRectangles(t, b, l, r);

            // for (var i = 0; i < particles.Length; i++)
            // {
            //     var (x, y, color) = particles[i];
            //     layer.DrawTiles((x - 0.5f, y - 0.5f), new Tile(Tile.SHAPE_CIRCLE_SMALL_HOLLOW, randomColors[i]));
            // }
            layer.DrawPoints(particles);
            layer.DrawMouseCursor();
            layer.Draw();
        }
    }
}