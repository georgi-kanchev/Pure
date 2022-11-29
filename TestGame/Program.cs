using Purity.Input;
using Purity.Tools;
using Purity.UserInterface;
using Purity.Utilities;

namespace TestGame
{
	public class Program
	{
		static void Main()
		{
			var window = new Purity.Graphics.Window();
			var bg = new Layer((48, 27));
			var layer = new Layer((48, 27));
			var over = new Layer((48, 27));
			var time = new Time();
			var inputBox = new InputLine((10, 10), (10, 1), "Hello");
			var inputBox2 = new InputLine((10, 15), (10, 1), "Test");

			while(window.IsOpen)
			{
				var h = window.GetHoveredIndicies(layer.CellCount);

				Mouse.Update();
				UserInterface.Input(h, Mouse.ArePressed(Mouse.Button.Left));

				time.Update();

				bg.Fill(0, 0);
				layer.Fill(0, 0);
				over.Fill(0, 0);

				window.DrawOn();

				MyCoolInputBoxUpdate(bg, layer, over, inputBox);
				MyCoolInputBoxUpdate(bg, layer, over, inputBox2);

				window.DrawLayer(bg.Cells, bg.Colors, (8, 8), (0, 0));
				window.DrawLayer(layer.Cells, layer.Colors, (8, 8), (0, 0));
				window.DrawLayer(over.Cells, over.Colors, (8, 8), (0, 0));

				window.DrawOff();
			}
		}
		static void MyCoolInputBoxUpdate(Layer bg, Layer layer, Layer over, InputLine inputBox)
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
				bg.SetSquare(selectedPos, (size, 1), Cell.SHADE_OPAQUE, Color.Blue);
				layer.SetTextLine(inputBox.Position, inputBox.Text, Color.Red);

				if(inputBox.IsFocused)
					over.SetCell(cursorPos, Cell.SHAPE_TRIANGLE_BIG_HOLLOW + 3, Color.White);
			});
		}
	}
}