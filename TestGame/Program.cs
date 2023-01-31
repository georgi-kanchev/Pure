using Pure.Tilemap;
using Pure.Utilities;
using Pure.Window;

namespace TestGame
{
	public class Program
	{
		// https://chillmindscapes.itch.io/
		// https://gigi.nullneuron.net/gigilabs/a-pathfinding-example-in-c/
		// inputline double click selection and ctrl+z/y
		// tilemap editor ctrl+z/y and collisions
		// https://babylonjs.medium.com/retro-crt-shader-a-post-processing-effect-study-1cb3f783afbcy

		static void Main()
		{
			var bg = new Tilemap("bg.map");
			var layer = new Tilemap("overworld.map");

			layer.SetTile((0, 0), 1, Color.Gray);

			while(Window.IsExisting)
			{
				Window.Activate(true);

				Window.DrawTilemap(bg, bg, (12, 12), (1, 1), "urizen.png");
				Window.DrawTilemap(layer, layer, (12, 12), (1, 1), "urizen.png");

				Window.Activate(false);
			}
		}
	}
}