namespace Pure.NoteEditor;

using Pure.Window;
using Pure.Tilemap;

public class NoteEditor
{
	static void Main()
	{
		Window.Create(Window.State.Windowed, 0);

		var (aw, ah) = Window.MonitorAspectRatio;
		var tilemap = new Tilemap((aw * 3, ah * 3));
		var (w, h) = tilemap.Size;

		tilemap.SetBorder((w - 5, 0), (5, h), Tile.BORDER_DEFAULT_CORNER, Tile.BORDER_DEFAULT_STRAIGHT);
		while (Window.IsOpen)
		{
			Window.Activate(true);

			//Window.DrawBasicTile((5, 5), Tile.ICON_BOLT);
			//Window.DrawBundleTiles(tilemap.ToBundle());
			var img = new SFML.Graphics.Image(0, 0);
			var hov = tilemap.PointFrom(Mouse.CursorPosition, Window.Size);

			if (Keyboard.IsKeyPressed(Keyboard.Key.A))
			{
				;
			}
			Window.DrawBasicTile(hov, Tile.SHADE_OPAQUE, tint: 159233698, size: (1, 1), angle: 0);

			Window.Activate(false);
		}
	}
}