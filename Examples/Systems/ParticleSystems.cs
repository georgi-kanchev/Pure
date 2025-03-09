using Pure.Engine.Execution;
using Pure.Engine.Tiles;
using Pure.Engine.Utility;
using Pure.Engine.Window;
using Pure.Tools.Particles;

namespace Pure.Examples.Systems;

public static class ParticleSystems
{
    public static void Run()
    {
        Window.Title = "Pure - Particles Example";

        var layer = new Layer((48, 27)) { BackgroundColor = Color.Gray.ToDark() };
        var (w, h) = layer.Size;
        var b = new Area(5, 20, 16, 1, Color.Red);
        var t = new Area(5, 5, 16, 1, Color.Red);
        var l = new Area(5, 5, 1, 16, Color.Red);
        var r = new Area(20, 5, 1, 16, Color.Red);
        var c = new Area(15, 15, 2, 4, Color.Red);
        var c2 = new Area(9, 12, 4, 3, Color.Red);
        var particles = Particles.SpawnCluster(30);

        particles.MakeCircle((10f, 10f), 4f, distribution: Distribution.Outline);
        particles.ApplyBounciness(float.NaN);
        particles.ApplyTriggers(t, b, l, r, c, c2);
        particles.ApplyGravity((0, 10f));
        // particles.ApplyAge(10f);
        particles.ApplyVarietyTeleport(35, 0);

        particles.OnTriggerEnter(indices =>
        {
            if (indices.areaIndex != 0)
                return;

            var p = particles[indices.particleIndex];
            (p.x, p.y) = layer.MouseCursorPosition;
            particles[indices.particleIndex] = (p.x, p.y, p.color);
            particles.ForceSet((0f, 0f), indices.particleIndex);
            particles.ForcePushAtAngle(0f, 10f, indices.particleIndex);
        });

        Mouse.Button.Left.OnPress(() => particles.ForcePushFromPoint(layer.MouseCursorPosition, 10f, 10f));
        Mouse.Button.Right.OnPress(() => particles.MakeCircle((10f, 10f), 4f, distribution: Distribution.FillEvenly));
        Mouse.Button.Middle.OnPress(() => particles.FadeToColor(Color.Blue, 0f));

        // var rain = Particles.SpawnCluster(200);
        // rain.MakeRectangle((0, 0, w, h));
        // rain.PushAtAngle(Angle.Down + 30, 1f);
        // rain.ApplyWrap((0, 0, w, h));
        // rain.ApplySize(1f);

        // var allDrops = new List<(float x, float y, uint color)[]>();
        // for (var i = 0; i < 5; i++)
        //     allDrops.Add(Particles.SpawnCluster(5));

        // Flow.CallEvery(0.75f, () =>
        // {
        //     for (var i = 2; i < allDrops.Count; i++)
        //     {
        //         allDrops[i].MakeRectangle((0, h - h / 4f, w, h / 4f), false, Distribution.FillRandomly);
        //         allDrops[i].ApplyColor(Color.Cyan);
        //         allDrops[i].ApplyColorFade(0);
        //     }
        // });
        // Flow.CallEvery(0.5f, () =>
        // {
        //     for (var i = 0; i < allDrops.Count - 3; i++)
        //     {
        //         allDrops[i].MakeRectangle((0, h - h / 4f, w, h / 4f), false, Distribution.FillRandomly);
        //         allDrops[i].ApplyColor(Color.Cyan);
        //         allDrops[i].ApplyColorFade(0);
        //     }
        // });

        while (Window.KeepOpen())
        {
            Time.Update();
            Flow.Update(Time.Delta);
            Particles.Update();

            layer.DrawRectangles(t, b, l, r, c, c2);

            // for (var i = 0; i < rain.Length; i++)
            // {
            //     var (x, y, color) = rain[i];
            //     var end = new Point(x, y).MoveAt(Angle.Down + 30, (0.1f, 1f).Random(i));
            //     layer.DrawLines((x, y, end.X, end.Y, Color.Cyan));
            // }
            //
            // for (var i = 0; i < allDrops.Count; i++)
            //     for (var j = 0; j < allDrops[i].Length; j++)
            //     {
            //         var (x, y, color) = allDrops[i][j];
            //         layer.DrawTiles((x, y), (Tile.ICON_EYE_OPENED, color, 0));
            //     }

            layer.DrawPoints(particles);
            layer.DrawMouseCursor();
            layer.Draw();
        }
    }
}