namespace Pure.Examples;

using Pure.Window;
using Pure.Storage;
using Pure.Tilemap;
using Pure.Utilities;

public class Program
{
	static void Main()
	{
		//Systems.DefaultGraphics.Run();
		//Systems.ChatLAN.Run();

		//Games.FlappyBird.Run();

		var tilemap = new Tilemap((16 * 1, 9 * 1));
		tilemap.SetTextLine((0, 0), "Hello, World!", Color.Red);

		//var storage = new Storage();
		// var storage = new Storage();
		// storage.Load("test.storage", false);
		//storage.Set("0", tilemap.ToBundle());
		//storage.Save("test.storage", false);

		//var a = storage.GetAsObject<int[,]>(0);
		//storage.Add("map", tilemap);
		//storage.Save("test.map");

		Window.Create(Window.Mode.Windowed);

		while (Window.IsOpen)
		{
			Window.Activate(true);
			Window.DrawTiles(tilemap.ToBundle());
			Window.Activate(false);
		}
	}
}