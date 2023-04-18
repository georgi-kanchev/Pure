namespace Pure.NoteEditor;

using Pure.Window;
using Pure.Tilemap;

public class NoteEditor
{
	static void Main()
	{
		Window.Create(Window.State.Windowed, 0);

		var (aw, ah) = Window.MonitorAspectRatio;
		var tilemap = new Tilemap((aw * 1, ah * 1));
		var (w, h) = tilemap.Size;

		tilemap.SetBorder((w - 5, 0), (5, h), Tile.BORDER_DEFAULT_CORNER, Tile.BORDER_DEFAULT_STRAIGHT);
		while (Window.IsOpen)
		{
			Window.Activate(true);

			Window.DrawSprite((5, 5), Tile.ICON_BOLT);
			//Window.DrawTilemap(tilemap);

			Window.SetLayer();

			Window.Activate(false);
		}
	}
}