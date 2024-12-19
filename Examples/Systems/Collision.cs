using Pure.Engine.Collision;
using Pure.Engine.Tiles;
using Pure.Engine.Utility;
using Pure.Engine.Window;
using static Pure.Engine.Utility.Color;
using Monitor = Pure.Engine.Window.Monitor;

namespace Pure.Examples.Systems;

public static class Collision
{
    public static void Run()
    {
        Window.Title = "Pure - Collision Example";

        var aspectRatio = Monitor.Current.AspectRatio;
        var maps = new TileMapPack(2, (aspectRatio.width * 3, aspectRatio.height * 3));
        var (w, h) = maps.Size;

        var collisionMap = new SolidMap();
        collisionMap.AddSolids(Tile.ICON_WAVE, new Solid(0, 0, 1f, 1f, Yellow)); // lake
        collisionMap.AddSolids(Tile.ICON_WAVES, new Solid(0, 0, 1f, 1f, Yellow)); // lake
        collisionMap.AddSolids(Tile.GEOMETRY_ANGLE, new Solid(0, 0, 1, 1)); // house roof
        collisionMap.AddSolids(Tile.GEOMETRY_ANGLE_RIGHT, new Solid(0, 0, 1, 1)); // house wall
        collisionMap.AddSolids(Tile.UPPERCASE_I, new Solid(0, 0, 1, 1)); // tree trunk
        collisionMap.AddSolids(Tile.PATTERN_33, new Solid(0, 0, 1, 1)); // tree top

        // icon tiles are 7x7, not 8x8, cut one row & column,
        // hitbox and tile on screen might mismatch since the tile is pixel perfect
        // and the hitbox is not
        const float SCALE = 1f - 1f / 8f;
        var hitbox = new SolidPack(new Solid(0, 0, 1, 1)) { Scale = (SCALE, SCALE) };
        var layer = new Layer((w, h));

        maps.TileMaps[1].FillWithRandomGrass();
        maps.TileMaps[1].SetLake((0, 0), (14, 9));
        maps.TileMaps[1].SetLake((26, 18), (5, 7));
        maps.TileMaps[1].SetLake((16, 24), (12, 6));
        maps.TileMaps[1].SetHouses((30, 10), (34, 11), (33, 8));
        maps.TileMaps[1].SetBridge((21, 16), (31, 16));
        maps.TileMaps[1].SetRoad((32, 0), (32, 26));
        maps.TileMaps[1].SetRoad((33, 10), (47, 10));
        maps.TileMaps[1].SetRoad((20, 16), (0, 16));
        maps.TileMaps[1].SetTrees((31, 5), (26, 8), (20, 12), (39, 11), (36, 18), (38, 19));
        maps.TileMaps[1].SetBackgrounds(maps.TileMaps[0]);

        collisionMap.Update(maps.TileMaps[1]);
        var collisionPack = collisionMap.ToArray();

        var waves = new SolidMap();
        waves.AddSolids(Tile.ICON_WAVE, new Solid(0, 0, 1, 1, Blue.ToDark()));
        waves.AddSolids(Tile.ICON_WAVES, new Solid(0, 0, 1, 1, Blue.ToDark()));
        waves.AddSolids(Tile.PATTERN_33, new Solid(0, 0, 1, 1, Green.ToDark(0.7f).ToDark()));
        waves.Update(maps.TileMaps[1]);
        var wavesRects = waves.ToBundle();

        while (Window.KeepOpen())
        {
            Time.Update();

            var mousePosition = layer.PixelToPosition(Mouse.CursorPosition);
            var isOverlapping = collisionMap.IsOverlapping(hitbox);
            var id = isOverlapping ? Tile.FACE_SAD : Tile.FACE_SMILING;
            var tint = isOverlapping ? Red : Green;
            var tile = new Tile(id, tint);
            var line = new Line((mousePosition.x - 1, mousePosition.y), (15, 15), Red);
            var crossPoints = line.CrossPoints(collisionPack);

            hitbox.Position = mousePosition;
            line.Color = crossPoints.Length > 0 ? Red : Green;

            layer.DrawTiles(maps.TileMaps[0]);
            layer.DrawTiles(maps.TileMaps[1]);

            //layer.DrawRectangles(collisionMap);
            layer.DrawLines(line);
            layer.DrawPoints(crossPoints);
            layer.DrawTiles(mousePosition, tile);

            layer.ApplyBlur((127, 127), (0, 0, w, h, Blue));
            layer.ApplyWaves((0, 50), (0, 50), wavesRects);

            layer.Draw();
        }
    }

    private static void SetBackgrounds(this TileMap tileMap, TileMap background)
    {
        for (var i = 0; i < tileMap.Size.height; i++)
            for (var j = 0; j < tileMap.Size.width; j++)
            {
                var color = (Color)tileMap.TileAt((j, i)).Tint;
                background.SetTile((j, i), new(Tile.FULL, color.ToDark()));
            }
    }
    private static void SetTrees(this TileMap tileMap, params (int x, int y)[] positions)
    {
        foreach (var t in positions)
        {
            var (x, y) = t;
            tileMap.SetEllipse((x, y - 1), (1, 1), true, new Tile(Tile.PATTERN_33, Green.ToDark(0.7f)));
            tileMap.SetTile((x, y), new(Tile.UPPERCASE_I, Brown.ToDark(0.4f)));
        }
    }
    private static void SetBridge(this TileMap tileMap, (int x, int y) pointA, (int x, int y) pointB)
    {
        tileMap.SetLine(pointA, pointB, new Tile(Tile.BAR_STRIP_STRAIGHT, Brown.ToDark()));
    }
    private static void SetRoad(this TileMap tileMap, (int x, int y) pointA, (int x, int y) pointB)
    {
        var pose = pointA.x == pointB.x ? Pose.Right : Pose.Default;
        tileMap.SetLine(pointA, pointB, new Tile(Tile.BAR_SPIKE_STRAIGHT, Brown, pose));
    }
    private static void SetHouses(this TileMap tileMap, params (int x, int y)[] positions)
    {
        foreach (var t in positions)
        {
            var (x, y) = t;
            var roof = new Tile(Tile.GEOMETRY_ANGLE, Red.ToDark());
            var walls = new Tile(Tile.GEOMETRY_ANGLE_RIGHT, Brown.ToBright());

            tileMap.SetTile((x, y), roof);
            roof.Pose = Pose.Flip;
            tileMap.SetTile((x + 1, y), roof);

            tileMap.SetTile((x, y + 1), walls);
            walls.Pose = Pose.Flip;
            tileMap.SetTile((x + 1, y + 1), walls);
        }
    }
    private static void SetLake(this TileMap tileMap, (int x, int y) position, (int width, int height) radius)
    {
        tileMap.SetEllipse(position, radius, true, Tile.MATH_APPROXIMATE);
        tileMap.Replace((0, 0, tileMap.Size.width, tileMap.Size.height), Tile.MATH_APPROXIMATE,
            new Tile(Tile.ICON_WAVE, Blue),
            new Tile(Tile.ICON_WAVE, Blue, Pose.Down),
            new Tile(Tile.ICON_WAVES, Blue),
            new Tile(Tile.ICON_WAVES, Blue, Pose.Down));
    }
    private static void FillWithRandomGrass(this TileMap tileMap)
    {
        var color = Green.ToDark(0.4f);
        tileMap.Replace((0, 0, tileMap.Size.width, tileMap.Size.height), 0,
            new Tile(Tile.SHADE_1, color),
            new Tile(Tile.SHADE_1, color, Pose.Right),
            new Tile(Tile.SHADE_1, color, Pose.Down),
            new Tile(Tile.SHADE_1, color, Pose.Left),
            new Tile(Tile.SHADE_2, color),
            new Tile(Tile.SHADE_2, color, Pose.Right),
            new Tile(Tile.SHADE_2, color, Pose.Down),
            new Tile(Tile.SHADE_2, color, Pose.Left));
    }
}