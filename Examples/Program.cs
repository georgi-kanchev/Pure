namespace Pure.Examples;

using Pure.Collision;
using Pure.Tilemap;
using Pure.Utilities;
using Pure.Window;

public class Program
{
	static void Main()
	{
		//Systems.DefaultGraphics.Run();
		//Systems.UserInterface.Run();
		//Systems.ChatLAN.Run();

		//Games.FlappyBird.Run();

		Window.Create(Window.Mode.Windowed);
		var aspectRatio = Window.MonitorAspectRatio;
		var tilemap = new Tilemap((aspectRatio.width * 3, aspectRatio.height * 3));

		var hitbox = new Hitbox((0, 0), 1f, new Rectangle((10, 10)));
		var map = new Map();

		// make line and replace in rectangle tilemap.Set
		tilemap.SetEllipse((20, 10), (7, 7), Tile.SHAPE_CIRCLE, false);
		tilemap.Flood((0, 10), Tile.SHAPE_SQUARE);

		while (Window.IsOpen)
		{
			Window.Activate(true);

			//FillWithRandomGrass();
			//SetLake((0, 0), (14, 9));
			//SetLake((26, 18), (5, 7));


			Window.DrawTiles(tilemap.ToBundle());

			Window.Activate(false);
		}

		void FillWithRandomGrass()
		{
			for (int i = 0; i < tilemap.Size.height; i++)
				for (int j = 0; j < tilemap.Size.width; j++)
				{
					// this is controlled randomness, it will always give the same
					// tile in the particular coordinate (might still contain some visible patterns)
					var tile = new Tile(Tile.SHADE_1, Color.Green.ToDark());
					var seed = (i * 73856093) ^ (j * 19349663) + 83492791;
					tile.Angle = (sbyte)0.Random(3, seed);
					tilemap.SetTile((j, i), tile);
				}
		}
		void SetLake((int x, int y) position, (int width, int height) size)
		{
			tilemap.SetRectangle(position, size, new(Tile.MATH_APPROXIMATE, Color.Blue));
		}
	}
}