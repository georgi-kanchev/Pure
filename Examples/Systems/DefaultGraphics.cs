namespace Pure.Examples.Systems;

using Pure.Window;
using Pure.Tilemap;
using Pure.Utilities;

public static class DefaultGraphics
{
	public static void Run()
	{
		var tilemap = new Tilemap((16 * 3, 9 * 3));

		Window.Create();
		while (Window.IsOpen)
		{
			Window.Activate(true);

			tilemap.Fill();
			for (int i = 0; i < 26; i++)
				for (int j = 0; j < 26; j++)
				{
					var tile = new Indices(i, j).ToIndex(26);
					tilemap.SetTile((j, i), tile);
				}

			var mousePos = Mouse.CursorPosition;
			var (x, y) = tilemap.PointFrom(mousePos, Window.Size);
			var index = tilemap.TileAt(((int)x, (int)y));
			tilemap.SetTextLine((28, 13), $"{index}");

			Window.DrawTilemap(tilemap);
			Window.Activate(false);
		}
	}
}