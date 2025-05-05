using Pure.Engine.Utility;
using Pure.Engine.Collision;
using Pure.Engine.Hardware;
using Pure.Engine.Tiles;
using Pure.Engine.Window;
using Monitor = System.Threading.Monitor;

namespace Pure.Examples.Systems;

public static class LineOfSightAndLights
{
	public static void Run()
	{
		var window = new Window { Title = "Pure - Sights/Lights Example", BackgroundColor = Color.Gray.ToDark(0.65f) };
		var hardware = new Hardware(window.Handle);
		var (w, h) = hardware.Monitors[0].AspectRatio;
		var tilemap = new TileMap((w * 3, h * 3));
		var solidMap = new SolidMap();
		var angle = 0f;
		var opaque = new Tile(Tile.FULL, Color.Green);
		var effect = new Effect { EffectLight = Light.Flat };
		var layer = new LayerTiles(tilemap.Size) { Effect = effect };

		tilemap.SetEllipse((21, 8), (10, 7), true, [opaque]);
		tilemap.SetEllipse((5, 9), (4, 7), true, [opaque]);
		tilemap.SetEllipse((32, 20), (9, 3), true, [opaque]);
		tilemap.SetLine((0, 0), (48, 27), [Tile.SHADE_1]);
		tilemap.SetLine((0, 1), (48, 27), [Tile.SHADE_1]);
		tilemap.SetLine((1, 0), (48, 27), [Tile.SHADE_1]);

		solidMap.AddSolids(Tile.FULL, [new(0, 0, 1, 1)]);
		solidMap.Update(tilemap);

		layer.BackgroundColor = Color.Gray;

		while (window.KeepOpen())
		{
			Time.Update();
			angle += Time.Delta * 60;

			var (mx, my) = layer.PositionFromPixel(window, hardware.Mouse.CursorPosition);

			effect.AddLightObstacles(solidMap);
			effect.AddLight([(mx, my)], 5f, (360f, angle));

			layer.DrawTileMap(tilemap);
			layer.DrawMouseCursor(window, hardware.Mouse.CursorPosition, (int)hardware.Mouse.CursorCurrent);
			layer.Render(window);
		}
	}
}