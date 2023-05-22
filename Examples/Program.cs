namespace Pure.Examples;

using Pure.Collision;
using Pure.Tilemap;
using Pure.Utilities;
using Pure.Window;

public class Program
{
	static void Main()
	{
		//Systems.DefaultGraphics.Run();
		//Systems.UserInterface.Run();
		//Systems.ChatLAN.Run();

		//Games.FlappyBird.Run();

		var tilemap = new Tilemap((16 * 3, 9 * 3));
		var hitbox = new Hitbox((0, 0), 1f, new Rectangle((10, 10)));
		var map = new Map();

		Window.Create(Window.Mode.Windowed);

		while (Window.IsOpen)
		{
			Window.Activate(true);

			//Window.DrawTiles(tilemap.ToBundle());

			Window.DrawRectangles(new Rectangle((1, 1)));
			Window.DrawLines((2, 2, 30, 4, Color.Red));

			Window.Activate(false);
		}
	}
}