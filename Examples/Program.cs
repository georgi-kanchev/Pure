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

		while (Window.IsExisting)
		{
			Window.Activate(true);

			Window.DrawTilemap(tilemap, (8, 8));

			Window.DrawSprite((5, 5), 78, (3, 3));
			//Window.DrawSprite((5, 5), 0, (10, 10));
			//Window.DrawSprite((10, 5), 2);

			Window.Activate(false);
		}
	}
}