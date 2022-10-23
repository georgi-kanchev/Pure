using Purity.Engine;
using Purity.Utilities;

namespace TestGame
{
	internal class Program
	{
		static void Main()
		{
			var window = new Window(scale: 60);
			var time = new Time();

			for(int i = 0; i < window.Height * window.Width; i++)
			{
				window.SetAtIndex(i, i, (byte)i);
			}

			while(window.IsOpen)
			{
				time.Update();
				window.Update();
			}
		}
	}
}