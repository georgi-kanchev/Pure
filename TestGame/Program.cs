using Pure.Input;
using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;
using Pure.Window;

namespace TestGame
{
	public class Program
	{
		public enum Song
		{
			CoolVibe
		}

		static void Main()
		{
			var bg = new Tilemap((48, 27));
			var layer = new Tilemap((48, 27));
			var over = new Tilemap((48, 27));
			var inputBox = new InputLine((10, 15), (10, 1), "Test");
			var container = new Container((10, 2), (13, 5)) { Text = "Title" };

			while(Window.IsExisting)
			{
				var mousePos = Window.MousePosition;
				var hov = layer.PositionFrom(mousePos, Window.Size);

				Mouse.Update();
				Keyboard.Update();
				Time.Update();

				var input = new Input()
				{
					Position = hov,
					IsPressed = Mouse.IsPressed(Pure.Input.Button.Left),
					TypedSymbols = Keyboard.TypedSymbols,
					IsPressedBackspace = Keyboard.IsPressed(Key.Backspace),
					IsPressedLeft = Keyboard.IsPressed(Key.ArrowLeft),
					IsPressedRight = Keyboard.IsPressed(Key.ArrowRight),
				};

				UserInterface.UpdateInput(input);

				bg.Fill(0, 0);
				layer.Fill(0, 0);
				over.Fill(0, 0);

				Window.DrawEnable(true);

				MyCoolInputBoxUpdate(bg, layer, over, inputBox);
				MyCoolContainer(bg, layer, container);

				bg.UpdateCamera();
				layer.UpdateCamera();
				over.UpdateCamera();

				Window.DrawTilemap(bg.Camera, bg.Camera, (8, 8), (0, 0));
				Window.DrawTilemap(layer.Camera, layer.Camera, (8, 8), (0, 0));
				Window.DrawTilemap(over.Camera, over.Camera, (8, 8), (0, 0));

				Window.MouseCursor = (Cursor)UserInterface.MouseCursorTile;
				Window.DrawEnable(false);
			}
		}
		static void MyCoolInputBoxUpdate(Tilemap bg, Tilemap layer, Tilemap over, InputLine inputBox)
		{
			inputBox.Update((b) =>
			{
				bg.SetSquare(b.Position, b.Size, 10, Color.Gray);
				bg.SetInputLineSelection(b.Position, b.IndexCursor, b.IndexSelection, Color.Blue);
				layer.SetTextLine(b.Position, b.Text, Color.Red);
				over.SetInputLineCursor(b.Position, b.IsFocused, b.IndexCursor, Color.White);
			});
		}
		static void MyCoolContainer(Tilemap bg, Tilemap layer, Container container)
		{
			container.Update((c) =>
			{
				bg.SetSquare(c.Position, c.Size, Tile.PATTERN_24, Color.Gray);
				layer.SetNinePatch(c.Position, c.Size, Tile.BORDER_GRID_TOP_LEFT, Color.White);
				layer.SetTextLine((c.Position.Item1 + 1, c.Position.Item2), c.Text, Color.White);
			});
		}
	}
}