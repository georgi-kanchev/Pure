using Pure.Engine.Tilemap;
using Pure.Engine.Utilities;
using Pure.Engine.Window;
using Pure.Tools.Tilemap;

using Monitor = Pure.Engine.Window.Monitor;

namespace Pure.Examples.Systems;

public static class TerrainGeneration
{
    public static void Run()
    {
        Window.Title = "Pure - Terrain Generation Example";
        Window.PixelScale = 3f;

        var aspectRatio = Monitor.Current.AspectRatio;
        var layer = new Layer((aspectRatio.width * 5, aspectRatio.height * 5));
        var terrain = new Tilemap(layer.Size);
        var generator = new MapGenerator();

        RegenerateMap();

        Keyboard.Key.ArrowLeft.OnPressAndHold(() => Scroll(-3, 0));
        Keyboard.Key.ArrowRight.OnPressAndHold(() => Scroll(3, 0));
        Keyboard.Key.ArrowUp.OnPressAndHold(() => Scroll(0, -3));
        Keyboard.Key.ArrowDown.OnPressAndHold(() => Scroll(0, 3));

        while (Window.KeepOpen())
        {
            layer.DrawTilemap(terrain);
            layer.DrawCursor();
            layer.Draw();
        }

        void RegenerateMap()
        {
            terrain.Flush();
            generator.Elevations.Clear();
            generator.Elevations[40] = new(Tile.ICON_WAVES, Color.Blue.ToDark()); // deep water
            generator.Elevations[50] = new(Tile.ICON_WAVES, Color.Blue); // shallow water
            generator.Elevations[65] = new(Tile.SHADE_2, Color.Yellow.ToDark()); // beaches/dirt patches
            generator.Elevations[125] = new(Tile.SHADE_1, Color.Green.ToDark()); // grass
            generator.Elevations[140] = new(Tile.BRACKET_ROUND_RIGHT, Color.Green.ToDark(), 3); // hills
            generator.Elevations[160] = new(Tile.NATURE_MOUNTAIN, Color.Gray); // rocky mountains
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