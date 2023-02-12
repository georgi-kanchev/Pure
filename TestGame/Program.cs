namespace TestGame;

using Pure.Tilemap;
using Pure.Window;
using Pure.Pathfinding;
using Pure.Utilities;

public class Program
{
	// https://chillmindscapes.itch.io/
	// https://gigi.nullneuron.net/gigilabs/a-pathfinding-example-in-c/
	// inputline double click selection and ctrl+z/y
	// tilemap editor collisions

	static void Main()
	{
		var t = new Tilemap((48, 27));
		var m = new Map(t.Size);

		for (int i = 0; i < 10; i++)
		{
			var p = (5 + i, 5);
			m.SetSolid(p);
			t.SetTile((p), Tile.SHADE_OPAQUE, Color.Red);
		}

		var path = m.FindPath((8, 2), (8, 8));
		for (int i = 0; i < path.Length; i++)
			t.SetTile(path[i], Tile.SHADE_OPAQUE, Color.Green);

		while (Window.IsExisting)
		{
			Window.Activate(true);

			Window.DrawTilemap(t, t, (8, 8));

			Window.Activate(false);
		}
	}
}