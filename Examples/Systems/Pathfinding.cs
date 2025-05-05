using Pure.Engine.Collision;
using Pure.Engine.Hardware;
using Pure.Engine.Window;
using Pure.Engine.Tiles;
using Pure.Engine.Utility;

namespace Pure.Examples.Systems;

public static class Pathfinding
{
	public static void Run()
	{
		var window = new Window { Title = "Pure - Pathfinding Example" };
		var hardware = new Hardware(window.Handle);
		var (w, h) = hardware.Monitors[0].AspectRatio;
		var tilemap = new TileMap((w * 3, h * 3));
		var layer = new LayerTiles(tilemap.Size);
		var pathMap = new PathMap(tilemap.Size);
		var tilemap2 = new TileMap((w * 3, h * 3));

		tilemap.SetEllipse((21, 8), (10, 7), true, [Tile.FULL]);
		tilemap.SetEllipse((5, 9), (4, 7), true, [Tile.FULL]);
		tilemap.SetEllipse((32, 20), (9, 3), true, [Tile.FULL]);
		tilemap.SetLine((0, 0), (48, 27), [Tile.SHADE_1]);
		tilemap.SetLine((0, 1), (48, 27), [Tile.SHADE_1]);
		tilemap.SetLine((1, 0), (48, 27), [Tile.SHADE_1]);

		pathMap.SetObstacle(float.PositiveInfinity, Tile.FULL, tilemap);
		pathMap.SetObstacle(10, Tile.SHADE_1, tilemap);

		while (window.KeepOpen())
		{
			var (mx, my) = layer.PositionFromPixel(window, hardware.Mouse.CursorPosition);
			var lines = pathMap.FindPath((0.5f, 0.5f), (mx, my));
			var points = pathMap.FindPath((0.5f, 0.5f), (mx, my));

			tilemap2.ApplySeed(0);
			tilemap2.Flush();
			tilemap2.SetLineSquiggle((10, 10), ((int)mx, (int)my), 3f, [new(Tile.FULL, Color.Red)]);

			layer.DrawTileMap(tilemap);
			layer.DrawLine(lines);
			layer.DrawPoints(points);
			layer.DrawMouseCursor(window, hardware.Mouse.CursorPosition, (int)hardware.Mouse.CursorCurrent);

			layer.DrawTileMap(tilemap2);

			layer.Render(window);
		}
	}
}