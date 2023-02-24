namespace TestGame;

using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;
using Pure.Window;

public class Program
{
	// https://chillmindscapes.itch.io/
	// inputline
	// - double click selection
	// - ctrl+z/y
	// - ctrl+delete/backspace
	// tilemap editor collisions
	// checkbox do toggle button with bigger widths

	static void Main()
	{
		var t1 = new Tilemap((48, 27));
		var t2 = new Tilemap((48, 27));
		var t3 = new Tilemap((48, 27));
		var i = new InputBox((5, 5), (10, 3));

		while (Window.IsExisting)
		{
			Window.Activate(true);

			UserInterface.ApplyInput(
				MouseButton.IsPressed(MouseButton.LEFT),
				t1.PointFrom(MouseCursor.Position, Window.Size),
				MouseButton.ScrollDelta,
				KeyboardKey.Pressed,
				KeyboardKey.Typed,
				t1.Size);

			i.Update();

			t1.Fill();
			t2.Fill();
			t3.Fill();

			t1.SetSquare(i.Position, i.Size, Tile.SHADE_OPAQUE, Color.Gray);
			t1.SetTextSquare(i.Position, i.Size, i.Selection, Color.Blue, false);
			t2.SetTextSquare(i.Position, i.Size, i.Text, Color.White, false);
			t3.SetInputBoxCursor(i.Position, i.Size, i.IsFocused, i.IndexCursor, Color.Red);

			Window.DrawTilemap(t1, t1, (8, 8));
			Window.DrawTilemap(t2, t2, (8, 8));
			Window.DrawTilemap(t3, t3, (8, 8));
			MouseCursor.Type = UserInterface.MouseCursorTile;

			Window.Activate(false);
		}
	}
}
