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

		var arr = new int[4, 2, 3]
		{
			{ { 1, 2, 3}, {4, 5, 6} },

			{ { 7, 8, 9}, { 10, 11, 12} },

			{ { 1, 6, 6}, { 3, 5, 2} },

			{ { 7, 2, 8}, { 1, 4, 7} },
		};
		var storage = new Storage<int>();
		storage.Set(0, arr);
		var val = storage.GetAsText(0);
		var split = val.Split(Environment.NewLine + Environment.NewLine);
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