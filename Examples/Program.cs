namespace Pure.Examples;

using Pure.Window;
using Pure.Tilemap;
using Pure.Utilities;

public class Program
{
	static void Main()
	{
		//Systems.DefaultGraphics.Run();
		//Systems.ChatLAN.Run();

		//Games.FlappyBird.Run();

		var tilemap = new Tilemap((16 * 3, 9 * 3));
		//tilemap.SetTextLine((0, 0), "Hello, World!", Color.Red);

		var tiles = new Tile[,]
		{
			{ 78, 79, 80 },
			{ 81, 82, 83 },
			{ 84, 85, 86 },
			{ 87, 88, 89 },
			{ 90, 91, 92 },
		};
		tilemap.SetGroup((5, 5), tiles);

		Window.Create(Window.Mode.Windowed);
		while (Window.IsOpen)
		{
			Window.Activate(true);
			Window.DrawTiles(tilemap.ToBundle());
			Window.Activate(false);
		}
	}
}