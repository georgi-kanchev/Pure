using Pure.Engine.Utilities;
using Pure.Engine.Collision;
using Pure.Engine.Tilemap;
using Pure.Engine.Window;
using Monitor = Pure.Engine.Window.Monitor;

namespace Pure.Examples.Systems;

public static class LineOfSightAndLights
{
    public static void Run()
    {
        Window.Title = "Pure - Sights/Lights Example";

        var (w, h) = Monitor.Current.AspectRatio;
        var tilemap = new Tilemap((w * 3, h * 3));
        var layer = new Layer(tilemap.Size);
        var solidMap = new SolidMap();
        var angle = 0f;
        var opaque = new Tile(Tile.FULL, Color.Green);

        tilemap.SetEllipse((21, 8), (10, 7), true, null, opaque);
        tilemap.SetEllipse((5, 9), (4, 7), true, null, opaque);
        tilemap.SetEllipse((32, 20), (9, 3), true, null, opaque);
        tilemap.SetLine((0, 0), (48, 27), null, Tile.SHADE_1);
        tilemap.SetLine((0, 1), (48, 27), null, Tile.SHADE_1);
        tilemap.SetLine((1, 0), (48, 27), null, Tile.SHADE_1);

        solidMap.AddSolids(Tile.FULL, new Solid(0, 0, 1, 1, Color.Red));
        solidMap.Update(tilemap);

        Window.BackgroundColor = Color.Gray.ToDark(0.65f);
        layer.BackgroundColor = Color.Gray;
        layer.Light = Light.Mask | Light.ObstaclesInShadow;

        while (Window.KeepOpen())
        {
            Time.Update();
            angle += Time.Delta * 60;

            var (mx, my) = layer.PixelToPosition(Mouse.CursorPosition);

            layer.ApplyLightObstacles(solidMap);
            layer.ApplyLights(20f, (120f, angle), (mx, my, Color.White));

            layer.DrawTilemap(tilemap);
            layer.DrawMouseCursor();
            layer.Draw();
        }
    }
}