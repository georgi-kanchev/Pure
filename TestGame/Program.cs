namespace TestGame;

using Pure.Tilemap;
using Pure.Window;
using Pure.Pathfinding;
using Pure.Utilities;
using Pure.Collision;

public class Program
{
	// https://chillmindscapes.itch.io/
	// inputline double click selection and ctrl+z/y
	// tilemap editor collisions

	static void Main()
	{
		var t = new Tilemap((48, 27));
		var g = new Grid("grid.path");
		//var m = new Map();
		var m = new Map("map.collision");

		KeyboardKey.OnPressed(KeyboardKey.A, () =>
		{
			var (x, y) = t.PointFrom(MouseCursor.Position, Window.Size);
			t.Fill();

			for (int i = 0; i < g.Size.Item2; i++)
				for (int j = 0; j < g.Size.Item1; j++)
					if (g.IsSolid((i, j)))
						t.SetTile((i, j), Tile.SHADE_OPAQUE, Color.Red);

			var path = g.FindPath((0, 0), ((int)x, (int)y));
			for (int i = 0; i < path.Length; i++)
				t.SetTile(path[i], Tile.SHAPE_CIRCLE_HOLLOW, Color.Green);

			//m.AddRectangle(new((1, 1)), Tile.SHADE_OPAQUE);
			//m.AddRectangle(new((0.5f, 0.5f)), Tile.SHAPE_CIRCLE_HOLLOW);
			m.Update(t);
			//m.Save("map.collision");
		});

		while (Window.IsExisting)
		{
			Window.Activate(true);

			Window.DrawTilemap(t, t, (8, 8));

			for (int i = 0; i < m.RectangleCount; i++)
			{
				var rect = m[i];
				Window.DrawRectangle(rect.Position, rect.Size, Color.White);
			}

			Window.Activate(false);
		}
	}
}