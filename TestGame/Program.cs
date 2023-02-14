namespace TestGame;

using Pure.Tilemap;
using Pure.Window;
using Pure.Pathfinding;
using Pure.Utilities;

public class Program
{
	// https://chillmindscapes.itch.io/
	// inputline double click selection and ctrl+z/y
	// tilemap editor collisions

	static void Main()
	{
		var t = new Tilemap((48, 27));
		var m = new Map("grid.path");

		KeyboardKey.OnPressed(KeyboardKey.A, () =>
		{
			var (x, y) = t.PointFrom(MouseCursor.Position, Window.Size);
			t.Fill();

			for (int i = 0; i < m.Size.Item2; i++)
				for (int j = 0; j < m.Size.Item1; j++)
					if (m.IsSolid((i, j)))
						t.SetTile((i, j), Tile.SHADE_OPAQUE, Color.Red);

			var path = m.FindPath((0, 0), ((int)x, (int)y));
			for (int i = 0; i < path.Length; i++)
				t.SetTile(path[i], Tile.SHAPE_CIRCLE_HOLLOW, Color.Green);

			m.Save("grid.path");
		});

		while (Window.IsExisting)
		{
			Window.Activate(true);

			Window.DrawTilemap(t, t, (8, 8));

			Window.Activate(false);
		}
	}
}