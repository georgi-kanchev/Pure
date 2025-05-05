using Pure.Engine.Hardware;
using Pure.Engine.Utility;
using Pure.Engine.Window;

namespace Pure.Examples.Systems;

public static class Rendering
{
	public static void Run()
	{
		var window = new Window { Title = "Pure - Rendering Example", PixelScale = 1f };
		var hardware = new Hardware(window.Handle) { Mouse = { IsCursorVisible = true } };

		LayerTiles.DefaultGraphicsToFile("default-graphics.png");

		var (_, _, w, h) = hardware.Monitors[0].DesktopArea;
		var layer = new LayerSprites((w / 3, h / 3)) { Effect = new(), TexturePath = "default-graphics.png" };

		while (window.KeepOpen())
		{
			Time.Update();

			var (x, y) = layer.PositionFromPixel(window, hardware.Mouse.CursorPosition);
			layer.DrawLine([(x, y), (0, 0)], 1);
			layer.Render(window);
		}
	}
}