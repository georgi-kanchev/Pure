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

		KeyboardKey.OnPressed(KeyboardKey.A, () =>
		{
			var (x, y) = t.PointFrom(MouseCursor.Position, Window.Size);
			var path = m.FindPath((0, 0), ((int)x, (int)y));

			t.Fill();

			for (int i = 0; i < 10; i++)
			{
				var p = (i, 2);
				m.SetSolid(p);
				t.SetTile((p), Tile.SHADE_OPAQUE, Color.Red);
			}
			for (int i = 0; i < 10; i++)
			{
				var p = (9, 3 + i);
				m.SetSolid(p);
				t.SetTile((p), Tile.SHADE_OPAQUE, Color.Red);
			}
			for (int i = 0; i < 10; i++)
			{
				var p = (5 + i, 12 + i);
				m.SetSolid(p);
				t.SetTile((p), Tile.SHADE_OPAQUE, Color.Red);
			}

			for (int i = 0; i < path.Length; i++)
				t.SetTile(path[i], Tile.SHADE_OPAQUE, Color.Green);
		});


		while (Window.IsExisting)
		{
			Window.Activate(true);

			Window.DrawTilemap(t, t, (8, 8));

			Window.Activate(false);
		}
	}
}