namespace Pure.Examples;

using Pure.Tilemap;
using Pure.Window;

public class Program
{
	static void Main()
	{
		//Systems.DefaultGraphics.Run();
		Systems.UserInterface.Run();
		//Systems.ChatLAN.Run();

		//Games.FlappyBird.Run();

		var tilemap = new Tilemap((16 * 3, 9 * 3));

		Window.Create(Window.Mode.Windowed);

		while(Window.IsOpen)
		{
			Window.Activate(true);

			Window.DrawTiles(tilemap.ToBundle());

			Window.Activate(false);
		}
	}
}