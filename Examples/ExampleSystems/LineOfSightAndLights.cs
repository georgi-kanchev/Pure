using Pure.Engine.Utilities;

namespace Pure.Examples.ExamplesSystems;

using Engine.Collision;
using Engine.Tilemap;
using Engine.Window;

public static class LineOfSightAndLights
{
    public static void Run()
    {
        Window.Create();
        Window.Title = "Pure - Sights Example";

        var (w, h) = Monitor.Current.AspectRatio;
        var tilemap = new Tilemap((w * 3, h * 3));
        var layer = new Layer(tilemap.Size);
        var solidMap = new SolidMap();
        var angle = 0f;
        var opaque = new Tile(Tile.SHADE_OPAQUE, Color.Green);

        tilemap.SetEllipse((21, 8), (10, 7), true, opaque);
        tilemap.SetEllipse((5, 9), (4, 7), true, opaque);
        tilemap.SetEllipse((32, 20), (9, 3), true, opaque);
        tilemap.SetLine((0, 0), (48, 27), Tile.SHADE_1);
        tilemap.SetLine((0, 1), (48, 27), Tile.SHADE_1);
        tilemap.SetLine((1, 0), (48, 27), Tile.SHADE_1);

        solidMap.SolidsAdd(Tile.SHADE_OPAQUE, new Solid(1f, 1f, 0f, 0f, Color.Red));
        solidMap.Update(tilemap);

        while (Window.KeepOpen())
        {
            Time.Update();
            angle += Time.Delta * 60;

            var (mx, my) = layer.PixelToWorld(Mouse.CursorPosition);
            var sight = (SolidPack)solidMap.CalculateSight((mx, my), angle, 20);

            layer.DrawTilemap(tilemap);
            //layer.DrawRectangles(solidMap);
            layer.DrawRectangles(sight);
            layer.DrawCursor();
            layer.Draw();
        }
    }
}