using Engine;

namespace TestGame
{
	internal class Program
	{
		static void Main()
		{
			var window = new Window(scale: 60);

			var j = 0;
			for(int i = 0; i < window.Height * window.Width; i++)
			{
				window.Set(i, i, Color.White);
			}

			while(window.IsOpen)
			{
				window.Update();
			}
		}
	}
}