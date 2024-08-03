namespace Pure.Examples.ExamplesSystems;

using Engine.Pathfinding;
using Engine.Window;
using Engine.Tilemap;
using Engine.Utilities;

public static class Pathfinding
{
    public static void Run()
    {
        Window.Title = "Pure - Pathfinding Example";

        var (w, h) = Monitor.Current.AspectRatio;
        var tilemap = new Tilemap((w * 3, h * 3));
        var layer = new Layer(tilemap.Size);
        var pathMap = new PathMap(tilemap.Size);

        tilemap.SetEllipse((21, 8), (10, 7), true, null, Tile.SHADE_OPAQUE);
        tilemap.SetEllipse((5, 9), (4, 7), true, null, Tile.SHADE_OPAQUE);
        tilemap.SetEllipse((32, 20), (9, 3), true, null, Tile.SHADE_OPAQUE);
        tilemap.SetLine((0, 0), (48, 27), null, Tile.SHADE_1);
        tilemap.SetLine((0, 1), (48, 27), null, Tile.SHADE_1);
        tilemap.SetLine((1, 0), (48, 27), null, Tile.SHADE_1);

        pathMap.SetObstacle(float.PositiveInfinity, Tile.SHADE_OPAQUE, tilemap);
        pathMap.SetObstacle(10, Tile.SHADE_1, tilemap);

        while (Window.KeepOpen())
        {
            var (mx, my) = layer.PixelToPosition(Mouse.CursorPosition);
            var lines = pathMap.FindPath((25.5f, 0.5f), (mx, my), Color.Red);
            var points = pathMap.FindPath((25.5f, 0.5f), (mx, my), Color.Green);

            layer.DrawTilemap(tilemap);
            layer.DrawLines(lines);
            layer.DrawPoints(points);
            layer.DrawCursor();
            layer.Draw();
        }
    }
}