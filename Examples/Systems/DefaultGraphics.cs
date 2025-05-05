using Pure.Engine.Hardware;
using Pure.Engine.Window;
using Pure.Engine.Tiles;
using Pure.Engine.Utility;
using Monitor = System.Threading.Monitor;

namespace Pure.Examples.Systems;

public static class DefaultGraphics
{
	public static void Run()
	{
		var window = new Window { Title = "Pure - Default Graphics Example" };
		var hardware = new Hardware(window.Handle);
		var (w, h) = hardware.Monitors[0].AspectRatio;
		var tilemap = new TileMap((w * 3, h * 3));
		var layer = new LayerTiles(tilemap.Size);

		while (window.KeepOpen())
		{
			tilemap.Flush();

			for (var i = 0; i < 26; i++)
				for (var j = 0; j < 26; j++)
					tilemap.SetTile((j, i), (ushort)(i, j).ToIndex((26, 26)));

			var (x, y) = layer.PositionFromPixel(window, hardware.Mouse.CursorPosition);
			var id = tilemap.TileAt(((int)x, (int)y)).Id;
			tilemap.SetText((27, 13), $"{id}");

			layer.DrawTileMap(tilemap.ToBundle());
			layer.DrawMouseCursor(window, hardware.Mouse.CursorPosition, (int)hardware.Mouse.CursorCurrent);
			layer.Render(window);
		}
	}
}