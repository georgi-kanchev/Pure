namespace Pure.NoteEditor;

using Pure.Window;
using Pure.Tilemap;
using Pure.Collision;

public class NoteEditor
{
	static void Main()
	{
		Window.Create(Window.Mode.Windowed, 0);

		var (aw, ah) = Window.MonitorAspectRatio;
		var tilemap = new Tilemap((aw * 3, ah * 3));
		var (w, h) = tilemap.Size;

		var map = new Map("");

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
			var tile = new Tile(Tile.SHADE_OPAQUE, 159233698);
			Window.DrawTile(hov, tile);

			Window.Activate(false);
		}
	}
}