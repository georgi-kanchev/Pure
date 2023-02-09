using Pure.Tilemap;
using Pure.Window;

namespace TestGame
{
	public class Program
	{
		// https://chillmindscapes.itch.io/
		// https://gigi.nullneuron.net/gigilabs/a-pathfinding-example-in-c/
		// inputline double click selection and ctrl+z/y
		// tilemap editor collisions
		// https://babylonjs.medium.com/retro-crt-shader-a-post-processing-effect-study-1cb3f783afbcy

		static void Main()
		{
			var t = new Tilemap((48, 27));
			t.SetTextLine((10, 10), "Hello, World!");
			
			while(Window.IsExisting)
			{
				Window.Activate(true);
				
				Window.DrawTilemap(t, t, (8, 8));
				
				Window.Activate(false);
			}
			System.Console.WriteLine("hello, world");
		}
	}
}