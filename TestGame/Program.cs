using Pure.Tilemap;
using Pure.Window;

namespace TestGame
{
	public class Program
	{
		// https://chillmindscapes.itch.io/
		// https://gigi.nullneuron.net/gigilabs/a-pathfinding-example-in-c/
		// noise function
		// inputline double click selection and ctrl+z/y
		// tilemap editor collisions

		static void Main()
		{
			var t = new Tilemap("world.map");

			Window.IsRetro = true;

			t.SetTile((47, 26), Tile.ARROW_DOWN);
			t.CameraSize = (32, 18);
			t.CameraPosition = (0, 10);
			while(Window.IsExisting)
			{
				Window.Activate(true);

				var cam = t.CameraUpdate();
				Window.DrawTilemap(cam, cam, (12, 12), (1, 1), "urizen.png");
				
				Window.Activate(false);
			}
		}
	}
}