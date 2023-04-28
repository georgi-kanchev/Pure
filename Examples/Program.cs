namespace Pure.Examples;

using Pure.Window;
using Pure.Tilemap;
using Pure.Utilities;
using Pure.Storage;

public class Program
{
	static void Main()
	{
		//Systems.DefaultGraphics.Run();
		//Systems.ChatLAN.Run();

		//Games.FlappyBird.Run();

		var tilemap = new Tilemap((16 * 3, 9 * 3));
		tilemap.SetTextLine((0, 0), "Hello, World!", Color.Red);

		var storage = new Storage();
		storage.Set("0", ("123,5", "3,3,,,,5", "ab", "bvc"));
		var val = storage.GetAsText("0");

		var resultArr = storage.GetAsObject<(string, string, string, string)>("0");
		System.Console.WriteLine(val);
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