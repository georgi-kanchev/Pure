namespace TestGame;

using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;
using Pure.Window;

public class Program
{
	// https://chillmindscapes.itch.io/
	// tilemap editor collisions
	// loading cursor
	// checkbox do toggle button with bigger widths

	static void Main()
	{
		var t1 = new Tilemap((48, 27));
		var t2 = new Tilemap((48, 27));
		var t3 = new Tilemap((48, 27));
		var i = new InputBox((5, 5), (20, 10)) { Placeholder = "Type..." };

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

			t1.Fill();
			t2.Fill();
			t3.Fill();

			SetInputBox(i);

			DrawTilemap(t1);
			DrawTilemap(t2);
			DrawTilemap(t3);

			MouseCursor.Type = UserInterface.MouseCursorTile;

			Window.Activate(false);
		}

		void SetInputBox(InputBox i)
		{
			i.Update();

			i.GetGraphics(
				out var cursor,
				out var selection,
				out var placeholder
			);

			t1.SetSquare(i.Position, i.Size, Tile.SHADE_OPAQUE, Color.Gray);
			t1.SetTextSquare(i.Position, i.Size, selection, Color.Blue, false);
			t2.SetTextSquare(i.Position, i.Size, i.Text, Color.White, false);
			t2.SetTextSquare(i.Position, i.Size, placeholder, new Color(Color.Gray).ToBright(), false);
			t3.SetTextSquare(i.Position, i.Size, cursor, Color.Red, false);
		}
		void DrawTilemap(Tilemap tilemap)
		{
			Window.DrawTilemap(tilemap, tilemap, (8, 8));
		}
	}
}
