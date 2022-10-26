using Purity.Graphics;
using Purity.Utilities;

namespace TestGame
{
	internal class Program
	{
		static void Main()
		{
			var window = new Window();
			var layer = new Layer((32, 18));
			var time = new Time();

			for(uint i = 0; i < layer.CellTotalCount; i++)
			{
				layer.SetCell(i, i, (byte)i);
			}

			while(window.IsOpen)
			{
				var x = 3 + (MathF.Cos(time.Clock * 4f) / 2f);
				var y = 3 + (MathF.Sin(time.Clock * 4f) / 2f);

				Console.WriteLine(window.GetHoveredCell(layer.Cells));

				time.Update();

				window.DrawOn();
				window.DrawLayer(layer.Cells, layer.Colors, (8, 8));
				window.DrawParticles(Color.Red, (x, y), (1.3f, 1f));
				window.DrawOff();
			}
		}
	}
}