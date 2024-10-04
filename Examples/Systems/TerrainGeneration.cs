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
        var generator = new MapGenerator { Tilemap = terrain };

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
            generator.DepthRanges.Clear();
            generator.DepthRanges[90] = new(Tile.ICON_WAVES, Color.Blue.ToDark()); // deep water
            generator.DepthRanges[100] = new(Tile.ICON_WAVES, Color.Blue); // shallow water
            generator.DepthRanges[105] = new(Tile.SHADE_2, Color.Yellow.ToDark()); // beaches/dirt patches
            generator.DepthRanges[125] = new(Tile.SHADE_1, Color.Green.ToDark()); // grass
            generator.DepthRanges[140] = new(Tile.BRACKET_ROUND_RIGHT, Color.Green.ToDark(), 3); // hills
            generator.DepthRanges[160] = new(Tile.NATURE_MOUNTAIN, Color.Gray); // rocky mountains
            generator.DepthRanges[255] = new(Tile.NATURE_MOUNTAIN, Color.White); // snowy mountains
            generator.AffectedTileId = Tile.EMPTY;
            generator.Seed = 333;
            generator.Apply();

            generator.DepthRanges.Clear();
            generator.DepthRanges[100] = new(Tile.NATURE_TREE_DECIDUOUS, Color.Green.ToDark(0.65f));
            generator.DepthRanges[125] = new(Tile.NATURE_TREE_CONIFEROUS, Color.Green.ToDark(0.7f));
            generator.AffectedTileId = Tile.SHADE_1; // some trees on the grass
            generator.Seed = 444;
            generator.Apply();
        }
        void Scroll(int deltaX, int deltaY)
        {
            generator.Offset = (generator.Offset.x + deltaX, generator.Offset.y + deltaY);
            RegenerateMap();
        }
    }
}