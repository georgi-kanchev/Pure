namespace Pure.Examples.ExamplesSystems;

using Engine.Pathfinding;
using Engine.Window;
using Engine.Tilemap;
using Engine.Utilities;

public static class Pathfinding
{
    public static void Run()
    {
        Window.Create();
        Window.Title = "Pure - Pathfinding Example";

        var (w, h) = Monitor.Current.AspectRatio;
        var tilemap = new Tilemap((w * 3, h * 3));
        var layer = new Layer(tilemap.Size);
        var grid = new Grid(tilemap.Size);

        tilemap.SetEllipse((21, 8), (10, 7), true, Tile.SHAPE_SQUARE_HOLLOW);
        tilemap.SetEllipse((5, 9), (4, 7), true, Tile.SHAPE_SQUARE_HOLLOW);
        tilemap.SetEllipse((32, 20), (9, 3), true, Tile.SHAPE_SQUARE_HOLLOW);
        tilemap.SetLine((0, 0), (48, 27), Tile.SHADE_TRANSPARENT);
        tilemap.SetLine((0, 1), (48, 27), Tile.SHADE_TRANSPARENT);
        tilemap.SetLine((1, 0), (48, 27), Tile.SHADE_TRANSPARENT);
        grid.SetObstacle(float.PositiveInfinity, Tile.SHAPE_SQUARE_HOLLOW, tilemap);

        while (Window.KeepOpen())
        {
            var (mx, my) = layer.PixelToWorld(Mouse.CursorPosition);
            var path = grid.FindPath((0.5f, 0.5f), ((int)mx, (int)my), Color.Red);

            layer.DrawLines(path);
            layer.DrawTilemap(tilemap);
            layer.DrawCursor();
            Window.DrawLayer(layer);
        }
    }
}