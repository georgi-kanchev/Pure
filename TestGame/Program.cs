namespace TestGame;

using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;
using Pure.Window;
using Pure.Act;

public class Program
{
	// raycast texture mapping https://youtu.be/fSjc8vLMg8c?t=504
	// https://chillmindscapes.itch.io/
	// tilemap editor collisions
	// loading cursor
	// checkbox do toggle button with bigger widths
	// sprite/tile mirror H & V

	private enum MyActs
	{
		InputBoxBeingFocused,
		KeyPressedA
	}

	static void Main()
	{
		var t1 = new Tilemap((48, 27));
		var t2 = new Tilemap((48, 27));
		var t3 = new Tilemap((48, 27));
		var i = new InputBox((5, 5), (20, 10)) { Placeholder = "Type..." };

		Act<MyActs>.When(MyActs.KeyPressedA, () => System.Console.WriteLine("key A down"));
		Act<MyActs>.When(MyActs.KeyPressedA, () => System.Console.WriteLine("another key A down"));

		while (Window.IsExisting)
		{
			Window.Activate(true);

			UserInterface.ApplyInput(
				Mouse.IsButtonPressed(Mouse.Button.LEFT),
				t1.PointFrom(Mouse.CursorPosition, Window.Size),
				Mouse.ScrollDelta,
				Keyboard.KeysPressed,
				Keyboard.KeyTyped,
				t1.Size);

			t1.Fill();
			t2.Fill();
			t3.Fill();

			Act<MyActs>.Update(MyActs.KeyPressedA, Keyboard.IsKeyPressed(Keyboard.Key.A));

			SetInputBox(i);

			DrawTilemap(t1);
			DrawTilemap(t2);
			DrawTilemap(t3);

			UserInterface.GetMouseCursorGraphics(out var tile, out _);
			Mouse.CursorGraphics = tile;

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
