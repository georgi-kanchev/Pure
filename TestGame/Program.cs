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
			var t = new Tilemap("world.map")
			{
				CameraSize = (32, 18)
			};

			while(Window.IsExisting)
			{
				Window.Activate(true);

				var pos = MouseCursor.Position;
				var (mx, my) = t.PositionFrom(pos, Window.Size, false);

				t.CameraPosition = ((int)mx, (int)my);
				var cam = t.CameraUpdate();
				Window.DrawTilemap(cam, cam, (12, 12), (1, 1), "urizen.png");

				Window.Activate(false);
			}
		}
	}
}