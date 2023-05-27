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

		Window.Create(Window.Mode.Windowed, 0);
		var aspectRatio = Window.MonitorAspectRatio;
		var tilemap = new Tilemap((aspectRatio.width * 3, aspectRatio.height * 3));
		var collisionMap = new Map();
		collisionMap.AddRectangle(new((1, 1)), Tile.SHADE_1);
		collisionMap.AddRectangle(new((1, 1)), Tile.SHADE_2);
		//collisionMap.AddRectangle(new((1, 1)), Tile.ICON_WAVE); // lake
		//collisionMap.AddRectangle(new((1, 1)), Tile.ICON_WAVE_DOUBLE); // lake
		//collisionMap.AddRectangle(new((1, 1)), Tile.GEOMETRY_ANGLE); // house roof
		//collisionMap.AddRectangle(new((1, 1)), Tile.GEOMETRY_ANGLE_RIGHT); // house wall
		//collisionMap.AddRectangle(new((1, 1)), Tile.UPPERCASE_I); // tree trunk
		//collisionMap.AddRectangle(new((1, 1)), Tile.PATTERN_33); // tree top

		while (Window.IsOpen)
		{
			Window.Activate(true);

			FillWithRandomGrass();
			//SetLake((0, 0), (14, 9));
			//SetLake((26, 18), (5, 7));
			//SetLake((16, 24), (12, 6));
			//SetHouses((30, 10), (34, 11), (33, 8));
			//SetBridge((21, 16), (31, 16));
			//SetRoad((32, 0), (32, 26));
			//SetRoad((33, 10), (47, 10));
			//SetRoad((20, 16), (0, 16));
			//SetTrees((31, 5), (26, 8), (20, 12), (39, 11), (36, 18), (38, 19));

			collisionMap.Update(tilemap.IDs);

			Window.DrawRectangles(collisionMap.ToBundle());
			Window.DrawTiles(tilemap.ToBundle());
			Window.Activate(false);
		}

		void FillWithRandomGrass()
		{
			var targetTile = Tile.SHAPE_CIRCLE;
			var color = Color.Green.ToDark(0.4f);
			tilemap.Fill(targetTile);
			tilemap.Replace((0, 0), tilemap.Size, targetTile, 0,
				new Tile(Tile.SHADE_1, color, 0),
				new Tile(Tile.SHADE_1, color, 1),
				new Tile(Tile.SHADE_1, color, 2),
				new Tile(Tile.SHADE_1, color, 3),
				new Tile(Tile.SHADE_2, color, 0),
				new Tile(Tile.SHADE_2, color, 1),
				new Tile(Tile.SHADE_2, color, 2),
				new Tile(Tile.SHADE_2, color, 3));
		}
		void SetLake((int x, int y) position, (int width, int height) radius)
		{
			tilemap.SetEllipse(position, radius, Tile.MATH_APPROXIMATE);
			tilemap.Replace((0, 0), tilemap.Size, Tile.MATH_APPROXIMATE, 0,
				new Tile(Tile.ICON_WAVE, Color.Blue, 0),
				new Tile(Tile.ICON_WAVE, Color.Blue, 2),
				new Tile(Tile.ICON_WAVE_DOUBLE, Color.Blue, 0),
				new Tile(Tile.ICON_WAVE_DOUBLE, Color.Blue, 2));
		}
		void SetHouses(params (int x, int y)[] positions)
		{
			for (int i = 0; i < positions?.Length; i++)
			{
				var (x, y) = positions[i];
				var roof = new Tile(Tile.GEOMETRY_ANGLE, Color.Red.ToDark());
				var walls = new Tile(Tile.GEOMETRY_ANGLE_RIGHT, Color.Brown.ToBright());

				tilemap.SetTile((x, y), roof);
				roof.Flips = (true, false);
				tilemap.SetTile((x + 1, y), roof);

				tilemap.SetTile((x, y + 1), walls);
				walls.Flips = (true, false);
				tilemap.SetTile((x + 1, y + 1), walls);
			}
		}
		void SetRoad((int x, int y) pointA, (int x, int y) pointB)
		{
			var angle = pointA.x == pointB.x ? 1 : 0;
			tilemap.SetLine(pointA, pointB, new(Tile.BAR_SPIKE_STRAIGHT, Color.Brown, (sbyte)angle));
		}
		void SetBridge((int x, int y) pointA, (int x, int y) pointB)
		{
			tilemap.SetLine(pointA, pointB, new(Tile.BAR_STRIP_STRAIGHT, Color.Brown.ToDark()));
		}
		void SetTrees(params (int x, int y)[] positions)
		{
			for (int i = 0; i < positions?.Length; i++)
			{
				var (x, y) = positions[i];
				tilemap.SetEllipse((x, y - 1), (1, 1), new(Tile.PATTERN_33, Color.Green.ToDark(0.7f)));
				tilemap.SetTile((x, y), new(Tile.UPPERCASE_I, Color.Brown.ToDark(0.4f)));
			}
		}
	}
}