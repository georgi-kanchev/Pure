using Purity.Tools;
using Purity.Utilities;

using SFML.Window;

namespace TestGame
{
	public class Program
	{
		static void Main()
		{
			var window = new Purity.Graphics.Window();
			var layer = new Layer((48, 27));
			var time = new Time();

			while(window.IsOpen)
			{
				time.Update();

				layer.Fill(0, 0);
				window.DrawOn();
				var h = window.GetHoveredIndicies(layer.CellCount);

				UserInterface.ProcessButton(
					(0, 0), (3, 1), h, Mouse.IsButtonPressed(Mouse.Button.Left), (state) =>
					{
						var color = Color.Gray;
						if(state == UserInterface.StateButton.Hovered)
							color = Color.White;
						else if(state == UserInterface.StateButton.Pressed)
							color = Color.Red;
						else if(state == UserInterface.StateButton.Clicked)
							window.IsOpen = false;

						layer.SetSquare((0, 0), (3, 1), 10, color);

						layer.SetTextLine((0, 1), state.ToString(), Color.White);
					});
				window.DrawLayer(layer.Cells, layer.Colors, (8, 8));
				window.DrawOff();
			}
		}
	}
}