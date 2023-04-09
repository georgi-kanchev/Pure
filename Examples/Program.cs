namespace Pure.Examples;

using Pure.Window;
using Pure.Tilemap;
using Pure.Utilities;

public class Program
{
	static void Main()
	{
		//Systems.DefaultGraphics.Run();
		//Systems.ChatLAN.Run();

		//Games.FlappyBird.Run();

		var tilemap = new Tilemap((16 * 3, 9 * 3));

		Window.Create(Window.State.Windowed);

		//Tracker.Tracker<string>.When("a", () => Window.IsFullscreen = Window.IsFullscreen == false);
		while (Window.IsOpen)
		{
			//Tracker.Tracker<string>.Track("a", Keyboard.IsKeyPressed(Keyboard.Key.A));

			Window.Activate(true);

			Window.DrawTilemap(tilemap, (8, 8));

			var hov = tilemap.PointFrom(Mouse.CursorPosition, Window.Size);
			Window.DrawSprite(hov, 78, size: (2, 2));
			//Window.DrawSprite((5, 5), 0, (10, 10));
			//Window.DrawSprite((10, 5), 2);

			Window.Activate(false);
		}
	}
}