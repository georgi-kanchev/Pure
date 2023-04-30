namespace Pure.NoteEditor;

using Pure.Window;
using Pure.Tilemap;
using Pure.Collision;

public class NoteEditor
{
	static void Main()
	{
		Window.Create(Window.Mode.Windowed, 1);

		var (aw, ah) = Window.MonitorAspectRatio;
		var tilemap = new Tilemap((aw * 3, ah * 3));
		var (w, h) = tilemap.Size;

		tilemap.SetBorder((w - 5, 0), (5, h), Tile.BORDER_DEFAULT_CORNER, Tile.BORDER_DEFAULT_STRAIGHT);
		while (Window.IsOpen)
		{
			Window.Activate(true);

			Window.DrawTiles(tilemap.ToBundle());

			Window.Activate(false);
		}
	}
}