using Pure.Tilemap;
using Pure.Window;

namespace TestGame
{
	public class Program
	{
		// https://gigi.nullneuron.net/gigilabs/a-pathfinding-example-in-c/

		static void Main()
		{
			var layer = new Tilemap((48, 27));

			while(Window.IsExisting)
			{
				Window.Activate(true);

				Window.DrawTilemap(layer, layer, (8, 8), (0, 0));

				Window.Activate(false);
			}
		}
	}
}