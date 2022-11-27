using Purity.Tools;
using Purity.UserInterface;
using Purity.Utilities;

using SFML.Window;

namespace TestGame
{
	public class Program
	{
		static string log;

		static void Main()
		{
			var window = new Purity.Graphics.Window();
			var layer = new Layer((48, 27));
			var time = new Time();
			var btn = new Button((0, 0), (3, 1));
			var btn2 = new Button((0, 1), (3, 1));

			while(window.IsOpen)
			{
				time.Update();

				layer.Fill(0, 0);
				window.DrawOn();
				var h = window.GetHoveredIndicies(layer.CellCount);

				UserInterface.UpdateInput(h, Mouse.IsButtonPressed(Mouse.Button.Left));

				if(MyCoolButtonIsTriggered(layer, btn))
					log = "test";
				if(MyCoolButtonIsTriggered(layer, btn2))
					log = "hello";

				layer.SetTextLine((0, 10), log, Color.White);
				window.DrawLayer(layer.Cells, layer.Colors, (8, 8));
				window.DrawOff();
			}
		}
		static bool MyCoolButtonIsTriggered(Layer layer, Button btn)
		{
			return btn.IsTriggered((b) =>
			{
				var color = Color.Gray;
				if(btn.IsPressed) color = Color.Red;
				else if(btn.IsHovered) color = Color.White;

				layer.SetSquare(btn.Position, btn.Size, 10, color);
			});
		}
	}
}