namespace Pure.NoteEditor;

using Pure.Window;
using Pure.Tilemap;

public class NoteEditor
{
	static void Main()
	{
		Window.Create(Window.State.Windowed, 1);

		var (aw, ah) = Window.MonitorAspectRatio;
		var tilemap = new Tilemap((aw * 3, ah * 3));
		var (w, h) = tilemap.Size;

		tilemap.SetBorder((w - 5, 0), (5, h), Tile.BORDER_DEFAULT_CORNER, Tile.BORDER_DEFAULT_STRAIGHT);
		while (Window.IsOpen)
		{
			Window.Activate(true);

			Window.DrawTilemap(tilemap, (8, 8));

			Window.Activate(false);
		}
	}
}