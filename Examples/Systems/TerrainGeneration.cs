using Pure.Engine.Tiles;
using Pure.Engine.Utility;
using Pure.Engine.Window;
using Pure.Tools.Tiles;
using Monitor = Pure.Engine.Window.Monitor;

namespace Pure.Examples.Systems;

public static class TerrainGeneration
{
    public static void Run()
    {
        Window.Title = "Pure - Terrain Generation Example";
        Window.PixelScale = 10f;
        Window.RenderArea = RenderArea.Fill;

        var layer = new LayerTiles((160, 160));
        var terrain = new TileMap(layer.Size);
        var generator = new MapGenerator();

        layer.Fill();
        RegenerateMap();

        Keyboard.Key.ArrowLeft.OnPressAndHold(() => Scroll(-3, 0));
        Keyboard.Key.ArrowRight.OnPressAndHold(() => Scroll(3, 0));
        Keyboard.Key.ArrowUp.OnPressAndHold(() => Scroll(0, -3));
        Keyboard.Key.ArrowDown.OnPressAndHold(() => Scroll(0, 3));

        while (Window.KeepOpen())
        {
            layer.DragAndZoom();
            layer.DrawTileMap(terrain);
            layer.DrawMouseCursor();
            layer.Render();
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