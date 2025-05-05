global using static Pure.Tools.Tiles.TileMapperUI;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Tiles;
global using Pure.Engine.Utility;
global using Pure.Engine.Window;
using Pure.Engine.Hardware;

namespace Pure.Examples.UserInterface;

public static class Program
{
	public static LayerTiles? Layer { get; private set; }

	public static (List<TileMap>, List<Block>) Initialize(Hardware hardware)
	{
		var (width, height) = hardware.Monitors[0].AspectRatio;
		var sz = (width * 3, height * 3);
		var maps = new List<TileMap>();
		var blocks = new List<Block>();

		// Input.ApplyMouse(sz, default, ButtonIdsPressed, ScrollDelta);
		// Input.ApplyKeyboard(KeyIdsPressed, KeyTyped, Window.Clipboard);

		for (var i = 0; i < 8; i++)
			maps.Add(new(sz));

		return (maps, blocks);
	}
	public static void Run(Window window, Hardware hardware, List<TileMap> maps, List<Block> blocks)
	{
		Layer = new(maps[0].Size);

		window.MaximumFrameRate = 60;
		while (window.KeepOpen())
		{
			Time.Update();

			maps.ForEach(map => map.Flush());

			// Input.ApplyMouse(Layer.Size, Layer.MousePosition, ButtonIdsPressed, ScrollDelta);
			// Input.ApplyKeyboard(KeyIdsPressed, KeyTyped, Window.Clipboard);

			blocks.ForEach(block => block.Update());

			hardware.Mouse.CursorCurrent = (Mouse.Cursor)Input.CursorResult;

			maps.ForEach(map => Layer.DrawTileMap(map));
			Layer.DrawMouseCursor(window, hardware.Mouse.CursorPosition, (int)hardware.Mouse.CursorCurrent);
			Layer.Render(window);
		}
	}
}