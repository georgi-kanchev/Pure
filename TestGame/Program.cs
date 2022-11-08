using Purity.Graphics;
using Purity.Tools;
using Purity.Utilities;

namespace TestGame
{
	internal class Program
	{
		static void Main()
		{
			var window = new Window();
			var layer = new Layer((48, 27));
			var time = new Time();

			while(window.IsOpen)
			{
				time.Update();

				layer.Fill(26 * 2 + 7, Color.Gray);
				for(uint i = 0; i < 26 * 19; i++)
					layer.SetCell(i, i, (byte)i);

				var cell = window.GetHoveredCell(layer.Cells);
				layer.SetTextLine((0, 0), cell.ToString(), Color.White);

				window.DrawOn();
				window.DrawLayer(layer.Cells, layer.Colors, (8, 8));
				window.DrawOff();
			}
		}
	}
}