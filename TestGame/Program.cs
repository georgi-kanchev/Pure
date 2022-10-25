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

			var x = 0f;
			while(window.IsOpen)
			{
				time.Update();

				window.DrawBegin();
				window.DrawLayer(layer.Cells, layer.Colors, (8, 8));
				window.DrawSprite((x, 3f), 27, Color.Red);
				x += time.Delta;
				window.DrawEnd();
			}
		}
	}
}