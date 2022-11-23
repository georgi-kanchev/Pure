using Purity.Tools;
using Purity.Utilities;

using SFML.Window;

using static Purity.Tools.UserInterface;

namespace TestGame
{
	public class Program
	{
		static void Main()
		{
			var window = new Purity.Graphics.Window();
			var layer = new Layer((48, 27));
			var time = new Time();

			var color = Color.White;
			while(window.IsOpen)
			{
				time.Update();

				layer.Fill(26 * 2 + 7, Color.Azure);
				for(uint i = 0; i < 26 * 19; i++)
					layer.SetCell(i, i, (byte)i);

				var hoveredCell = window.GetHoveredCell(layer.Cells);
				var hoveredIndices = window.GetHoveredIndicies(layer.CellCount);
				layer.SetTextLine((0, 0), hoveredCell.ToString(), Color.White);

				window.DrawOn();

				var isPressed = Keyboard.IsKeyPressed(Keyboard.Key.A);
				ProcessButton((10, 20), (3, 2), hoveredIndices, isPressed, (state) =>
				{
					var btnCell = (uint)473;
					if(state == StateButton.Hovered)
						btnCell = 470;
					else if(state == StateButton.Pressed)
						btnCell = 469;
					else if(state == StateButton.Clicked)
						color = color == Color.White ? Color.Red : Color.White;

					layer.SetCell((10, 20), btnCell, color);
				});

				window.DrawLayer(layer.Cells, layer.Colors, (8, 8));
				window.DrawOff();
			}
		}
	}
}