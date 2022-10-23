using Purity.Graphics;
using Purity.Utilities;

namespace TestGame
{
	internal class Program
	{
		static void Main()
		{
			var window = new Window(scale: 60);
			var layer = new Layer(window.CellCount);
			var time = new Time();

			for(uint i = 0; i < layer.CellTotalCount; i++)
			{
				layer.SetCell(i, i, (byte)i);
			}

			while(window.IsOpen)
			{
				time.Update();

				window.DrawBegin();
				window.Draw(layer.Cells, layer.Colors);
				window.DrawEnd();
			}
		}
	}
}