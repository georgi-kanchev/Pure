namespace Pure.Examples;

using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;
using Pure.Window;

public class Program
{
	static void Main()
	{
		Systems.UserInterface.Run();
		//Systems.DefaultGraphics.Run();
		//Systems.ChatLAN.Run();

		//Games.FlappyBird.Run();

		var tilemap = new Tilemap((16 * 3, 9 * 3));

		Window.Create(Window.Mode.Windowed);

		while (Window.IsOpen)
		{
			Window.Activate(true);

			tilemap.Fill();

			Window.DrawTiles(tilemap.ToBundle());

			Window.Activate(false);
		}
	}
}