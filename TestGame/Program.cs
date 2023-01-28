using Pure.Tilemap;
using Pure.Utilities;
using Pure.Window;

namespace TestGame
{
	public class Program
	{
		// https://gigi.nullneuron.net/gigilabs/a-pathfinding-example-in-c/

		static void Main()
		{
			var layer = new Tilemap((48, 27));

			var c = new Color(0b_111_011_00);
			layer.SetTile((0, 0), 45, c);

			while(Window.IsExisting)
			{
				Window.Activate(true);

				Window.DrawTilemap(layer, layer, (8, 8), (0, 0));

				Window.Activate(false);
			}
		}
	}
}