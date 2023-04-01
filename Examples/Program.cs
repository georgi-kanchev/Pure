namespace Pure.Examples;

using Pure.Window;
using Pure.Tilemap;

public class Program
{
	static void Main()
	{
		//Systems.DefaultGraphics.Run();
		//Systems.ChatLAN.Run();

		//Games.FlappyBird.Run();

		var tilemap = new Tilemap((16 * 3, 9 * 3));
		tilemap.SetBorder((5, 5), (10, 10), Tile.BORDER_GRID_CORNER, Tile.BORDER_GRID_STRAIGHT);

		while (Window.IsExisting)
		{
			Window.Activate(true);

			Window.DrawTilemap(tilemap, (8, 8));

			Window.Activate(false);
		}
	}
}