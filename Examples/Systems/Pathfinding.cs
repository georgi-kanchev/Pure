using Pure.Engine.Pathfinding;
using Pure.Engine.Window;
using Pure.Engine.Tilemap;
using Pure.Engine.Utilities;
using Monitor = Pure.Engine.Window.Monitor;

namespace Pure.Examples.Systems;

public static class Pathfinding
{
    public static void Run()
    {
        Window.Title = "Pure - Pathfinding Example";

        var (w, h) = Monitor.Current.AspectRatio;
        var tilemap = new Tilemap((w * 3, h * 3));
        var layer = new Layer(tilemap.Size);
        var pathMap = new PathMap(tilemap.Size);

        tilemap.SetEllipse((21, 8), (10, 7), true, null, Tile.FULL);
        tilemap.SetEllipse((5, 9), (4, 7), true, null, Tile.FULL);
        tilemap.SetEllipse((32, 20), (9, 3), true, null, Tile.FULL);
        tilemap.SetLine((0, 0), (48, 27), null, Tile.SHADE_1);
        tilemap.SetLine((0, 1), (48, 27), null, Tile.SHADE_1);
        tilemap.SetLine((1, 0), (48, 27), null, Tile.SHADE_1);

        pathMap.SetObstacle(float.PositiveInfinity, Tile.FULL, tilemap);
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