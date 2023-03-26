namespace Pure.Examples;

using Pure.Window;
using Pure.Tilemap;
using Pure.Utilities;

public class Game
{
	static void Main()
	{
		//ExampleDefaultGraphics.Run();

		var tilemap = new Tilemap((16 * 3, 9 * 3));
		tilemap.SetBorder((10, 10), (10, 10), Tile.BORDER_GRID_CORNER, Tile.BORDER_GRID_STRAIGHT);

		tilemap.SetBar((10, 5), Tile.BAR_HOLLOW_BIG_EDGE, Tile.BAR_HOLLOW_BIG_STRAIGHT, Color.Red, 5, true);
		while (Window.IsExisting)
		{
			Window.Activate(true);
			Window.DrawTilemap(tilemap, (8, 8));
			Window.DrawSprite((5, 5), Tile.ICON_CALENDAR, Color.Red);
			Window.Activate(false);
		}
	}
}