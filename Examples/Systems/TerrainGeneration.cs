using Pure.Engine.Hardware;
using Pure.Engine.Tiles;
using Pure.Engine.Utility;
using Pure.Engine.Window;
using Pure.Tools.Tiles;

namespace Pure.Examples.Systems;

public static class TerrainGeneration
{
	public static void Run()
	{
		var window = new Window { Title = "Pure - Terrain Generation Example", PixelScale = 10f, RenderArea = RenderArea.Fill };
		var hardware = new Hardware(window.Handle);
		var layer = new LayerTiles((160, 160));
		var terrain = new TileMap(layer.Size);
		var generator = new MapGenerator();

		layer.Fill(window);
		RegenerateMap();

		hardware.Keyboard.OnPressAndHold(Keyboard.Key.ArrowLeft, () => Scroll(-3, 0));
		hardware.Keyboard.OnPressAndHold(Keyboard.Key.ArrowRight, () => Scroll(3, 0));
		hardware.Keyboard.OnPressAndHold(Keyboard.Key.ArrowUp, () => Scroll(0, -3));
		hardware.Keyboard.OnPressAndHold(Keyboard.Key.ArrowDown, () => Scroll(0, 3));

		while (window.KeepOpen())
		{
			layer.DragAndZoom(window, hardware.Mouse.IsPressed(Mouse.Button.Left) ? hardware.Mouse.CursorDelta : (0, 0), hardware.Mouse.ScrollDelta);
			layer.DrawTileMap(terrain);
			layer.DrawMouseCursor(window, hardware.Mouse.CursorPosition, (int)hardware.Mouse.CursorCurrent);
			layer.Render(window);
		}

		void RegenerateMap()
		{
			terrain.Flush();
			generator.Elevations.Clear();
			generator.Elevations[40] = new(Tile.ICON_WAVES, Color.Blue.ToDark()); // deep water
			generator.Elevations[50] = new(Tile.ICON_WAVES, Color.Blue); // shallow water
			generator.Elevations[65] = new(Tile.SHADE_2, Color.Yellow.ToDark()); // beaches/dirt patches
			generator.Elevations[125] = new(Tile.SHADE_1, Color.Green.ToDark()); // grass
			generator.Elevations[140] = new(Tile.BRACKET_ROUND_RIGHT, Color.Green.ToDark(), Pose.Left); // hills
			generator.Elevations[200] = new(Tile.NATURE_MOUNTAIN, Color.Gray); // rocky mountains
			generator.Elevations[255] = new(Tile.NATURE_MOUNTAIN, Color.White); // snowy mountains
			generator.TargetTileId = Tile.EMPTY;
			generator.Seed = 333;
			generator.Noise = Noise.OpenSimplex2S;
			generator.Scale = 20f;
			generator.Apply(terrain);

			generator.Elevations.Clear();
			generator.Elevations[100] = new(Tile.NATURE_TREE_DECIDUOUS, Color.Green.ToDark(0.65f));
			generator.Elevations[125] = new(Tile.NATURE_TREE_CONIFEROUS, Color.Green.ToDark(0.7f));
			generator.TargetTileId = Tile.SHADE_1; // some trees on the grass
			generator.Seed = 444;
			generator.Noise = Noise.OpenSimplex2S;
			generator.Scale = 20f;
			generator.Apply(terrain);
		}

		void Scroll(int deltaX, int deltaY)
		{
			generator.Offset = (generator.Offset.x + deltaX, generator.Offset.y + deltaY);
			RegenerateMap();
		}
	}
}