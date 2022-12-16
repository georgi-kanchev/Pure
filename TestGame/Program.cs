using Purity.Input;
using Purity.Tilemap;
using Purity.UserInterface;
using Purity.Utilities;

namespace TestGame
{
	public class Program
	{
		public enum Hotkey
		{
			PlayerJump
		}

		static void Main()
		{
			var mouse = new Mouse();
			var keyboard = new Keyboard();
			var window = new Purity.Graphics.Window();
			var bg = new Tilemap((48, 27));
			var layer = new Tilemap((48, 27));
			var over = new Tilemap((48, 27));
			var inputBox = new InputLine((0, 0), (10, 2), "Hello");
			var inputBox2 = new InputLine((10, 15), (10, 1), "Test");

			bg.CameraPosition = (-5, -5);
			layer.CameraPosition = (-5, -5);
			over.CameraPosition = (-5, -5);

			while(window.IsExisting)
			{
				var mousePos = window.MousePosition;
				var hov = layer.PixelToPosition(mousePos, window.Size);
				var h = ((int)hov.Item1, (int)hov.Item2);

				mouse.Update();
				keyboard.Update();

				var input = new Input()
				{
					Position = h,
					IsPressed = mouse.IsPressed(Purity.Input.Button.Left),
					TypedSymbol = keyboard.TypedSymbol,
					IsBackspaced = keyboard.IsPressed(Key.Backspace)
				};

				UserInterface.UpdateInput(input);

				Console.WriteLine(inputBox.TextSelected);

				Time.Update();

				bg.Fill(0, 0);
				layer.Fill(0, 0);
				over.Fill(0, 0);

				window.DrawEnable(true);

				MyCoolInputBoxUpdate(bg, layer, over, inputBox);
				MyCoolInputBoxUpdate(bg, layer, over, inputBox2);

				bg.UpdateCamera();
				layer.UpdateCamera();
				over.UpdateCamera();

				window.DrawLayer(bg.Camera.Item1, bg.Camera.Item2, (8, 8), (0, 0));
				window.DrawLayer(layer.Camera.Item1, layer.Camera.Item2, (8, 8), (0, 0));
				window.DrawLayer(over.Camera.Item1, over.Camera.Item2, (8, 8), (0, 0));

				window.DrawEnable(false);
			}
		}
		static void MyCoolInputBoxUpdate(Tilemap bg, Tilemap layer, Tilemap over, InputLine inputBox)
		{
			inputBox.Update((b) =>
			{
				var pos = inputBox.Position;
				var cursorPos = (pos.Item1 + inputBox.CursorPosition, pos.Item2);
				var selectedPos = (pos.Item1 + inputBox.SelectionPosition, pos.Item2);
				var size = cursorPos.Item1 - selectedPos.Item1;

				if(size < 0)
					selectedPos.Item1--;

				bg.SetSquare(inputBox.Position, inputBox.Size, 10, Color.Gray);
				bg.SetSquare(selectedPos, (size, 1), Tile.SHADE_OPAQUE, Color.Blue);
				layer.SetTextLine(inputBox.Position, inputBox.Text, Color.Red);

				if(inputBox.IsFocused)
					over.SetTile(cursorPos, Tile.SHAPE_TRIANGLE_BIG_HOLLOW + 3, Color.White);
			});
		}
	}
}