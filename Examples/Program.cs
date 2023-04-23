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

		Window.Create(Window.State.Windowed);
		while (Window.IsOpen)
		{
			Window.Activate(true);
			Window.DrawTiles(tilemap.ToBundle());
			Window.Activate(false);
		}
	}
}