namespace TestGame;

using Pure.Tilemap;
using Pure.Window;

public class Program
{
	// https://chillmindscapes.itch.io/
	// inputline double click selection and ctrl+z/y
	// tilemap editor collisions
	// checkbox do toggle button with bigger widths

	static void Main()
	{
		var t = new Tilemap((48, 27));

		while (Window.IsExisting)
		{
			Window.Activate(true);

			Window.DrawTilemap(t, t, (8, 8));

			Window.Activate(false);
		}
	}
}