using Engine;

namespace TestGame
{
	internal class Program
	{
		static void Main()
		{
			Window.Create("Purity", 80, 45, "graphics.png");
			Window.Fill(300, Color.Green);

			while(Window.IsOpen)
			{
				Window.Update();
			}
		}
	}
}