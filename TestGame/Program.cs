using Engine;

namespace TestGame
{
	internal class Program
	{
		static void Main()
		{
			var window = new Window("graphics.png");

			while(window.IsOpen)
			{
				window.Update();
			}
		}
	}
}