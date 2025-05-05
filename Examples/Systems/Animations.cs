using Pure.Engine.Hardware;
using Pure.Engine.Tiles;
using Pure.Engine.Utility;
using Pure.Engine.Window;

namespace Pure.Examples.Systems;

public static class Animations
{
	public static void Run()
	{
		var window = new Window { Title = "Pure - Animation Example" };
		var hardware = new Hardware(window.Handle);
		var (w, h) = hardware.Monitors[0].AspectRatio;
		var layer = new LayerTiles((w * 3, h * 3));
		var tilemap = new TileMap(layer.Size);
		var texts = new[] { "#1: This is a message", "#2: Another message", "#3: And yet another one" };
		var arrows = new[]
		{
			new Tile(Tile.ARROW, Color.Red),
			new Tile(Tile.ARROW_DIAGONAL, Color.Green, Pose.Right),
			new Tile(Tile.ARROW, Color.Blue, Pose.Right),
			new Tile(Tile.ARROW_DIAGONAL, Color.Cyan, Pose.Down),
			new Tile(Tile.ARROW, Color.Yellow, Pose.Down),
			new Tile(Tile.ARROW_DIAGONAL, Color.Brown, Pose.Left),
			new Tile(Tile.ARROW, Color.Orange, Pose.Left),
			new Tile(Tile.ARROW_DIAGONAL, Color.Azure)
		};

		while (window.KeepOpen())
		{
			Time.Update();

			tilemap.Flush();
			tilemap.SetText((0, 0), texts.Animate(0.3f));
			tilemap.SetTile((10, 10), arrows.Animate(1f));

			if (hardware.Keyboard.IsJustPressed(Keyboard.Key.A))
				texts[0] = "#1: Hello, World!";

			layer.DrawTileMap(tilemap);
			layer.DrawMouseCursor(window, hardware.Mouse.CursorPosition, (int)hardware.Mouse.CursorCurrent);
			layer.Render(window);
		}
	}
}