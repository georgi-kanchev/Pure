namespace Pure.Examples.Systems;

using Pure.Window;
using Pure.Tilemap;
using Pure.Utilities;

public static class DefaultGraphics
{
	public static void Run()
	{
		var tilemap = new Tilemap((16 * 3, 9 * 3));

		Window.Create(3);
		while (Window.IsOpen)
		{
			Window.Activate(true);

			tilemap.Fill();
			for (var i = 0; i < 26; i++)
				for (var j = 0; j < 26; j++)
					tilemap.SetTile((j, i), new Indices(i, j).ToIndex(26));

			var (x, y) = tilemap.PointFrom(Mouse.CursorPosition, Window.Size);
			var id = tilemap.TileAt(((int)x, (int)y)).ID;
			tilemap.SetTextLine((27, 13), $"{id}");

			Window.DrawTiles(tilemap.ToBundle());
			Window.Activate(false);
		}
	}
}