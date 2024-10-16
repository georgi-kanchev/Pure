using Pure.Engine.Collision;
using Pure.Engine.Tilemap;
using Pure.Engine.Utilities;
using Pure.Engine.Window;
using static Pure.Engine.Utilities.Color;
using Monitor = Pure.Engine.Window.Monitor;

namespace Pure.Examples.Systems;

public static class Collision
{
    public static void Run()
    {
        Window.Title = "Pure - Collision Example";

        var aspectRatio = Monitor.Current.AspectRatio;
        var maps = new TilemapPack(2, (aspectRatio.width * 3, aspectRatio.height * 3));
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

        maps.Tilemaps[1].FillWithRandomGrass();
        maps.Tilemaps[1].SetLake((0, 0), (14, 9));
        maps.Tilemaps[1].SetLake((26, 18), (5, 7));
        maps.Tilemaps[1].SetLake((16, 24), (12, 6));
        maps.Tilemaps[1].SetHouses((30, 10), (34, 11), (33, 8));
        maps.Tilemaps[1].SetBridge((21, 16), (31, 16));
        maps.Tilemaps[1].SetRoad((32, 0), (32, 26));
        maps.Tilemaps[1].SetRoad((33, 10), (47, 10));
        maps.Tilemaps[1].SetRoad((20, 16), (0, 16));
        maps.Tilemaps[1].SetTrees((31, 5), (26, 8), (20, 12), (39, 11), (36, 18), (38, 19));
        maps.Tilemaps[1].SetBackgrounds(maps.Tilemaps[0]);

        collisionMap.Update(maps.Tilemaps[1]);
        var collisionPack = collisionMap.ToArray();

        var waves = new SolidMap();
        waves.AddSolids(Tile.ICON_WAVE, new Solid(0, 0, 1, 1, Blue.ToDark()));
        waves.AddSolids(Tile.ICON_WAVES, new Solid(0, 0, 1, 1, Blue.ToDark()));
        waves.AddSolids(Tile.PATTERN_33, new Solid(0, 0, 1, 1, Green.ToDark(0.7f).ToDark()));
        waves.Update(maps.Tilemaps[1]);
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

            layer.DrawTilemap(maps.Tilemaps[0]);
            layer.DrawTilemap(maps.Tilemaps[1]);

            //layer.DrawRectangles(collisionMap);
            layer.DrawLines(line);
            layer.DrawPoints(crossPoints);
            layer.DrawTiles(mousePosition, tile);

            layer.ApplyBlur((127, 127), (0, 0, w, h, Blue));
            layer.ApplyWaves((0, 50), (0, 50), wavesRects);

            layer.Draw();
        }
    }

    private static void SetBackgrounds(this Tilemap tilemap, Tilemap background)
    {
        for (var i = 0; i < tilemap.Size.height; i++)
            for (var j = 0; j < tilemap.Size.width; j++)
            {
                var color = (Color)tilemap.TileAt((j, i)).Tint;
                background.SetTile((j, i), new(Tile.FULL, color.ToDark()));
            }
    }
    private static void SetTrees(this Tilemap tilemap, params (int x, int y)[] positions)
    {
        foreach (var t in positions)
        {
            var (x, y) = t;
            tilemap.SetEllipse((x, y - 1), (1, 1), true, null,
                new Tile(Tile.PATTERN_33, Green.ToDark(0.7f)));
            tilemap.SetTile((x, y), new(Tile.UPPERCASE_I, Brown.ToDark(0.4f)));
        }
    }
    private static void SetBridge(this Tilemap tilemap, (int x, int y) pointA, (int x, int y) pointB)
    {
        tilemap.SetLine(pointA, pointB, null, new Tile(Tile.BAR_STRIP_STRAIGHT, Brown.ToDark()));
    }
    private static void SetRoad(this Tilemap tilemap, (int x, int y) pointA, (int x, int y) pointB)
    {
        var angle = pointA.x == pointB.x ? 1 : 0;
        tilemap.SetLine(pointA, pointB, null,
            new Tile(Tile.BAR_SPIKE_STRAIGHT, Brown, (sbyte)angle));
    }
    private static void SetHouses(this Tilemap tilemap, params (int x, int y)[] positions)
    {
        foreach (var t in positions)
        {
            var (x, y) = t;
            var roof = new Tile(Tile.GEOMETRY_ANGLE, Red.ToDark());
            var walls = new Tile(Tile.GEOMETRY_ANGLE_RIGHT, Brown.ToBright());

            tilemap.SetTile((x, y), roof);
            roof.IsMirrored = true;
            tilemap.SetTile((x + 1, y), roof);

            tilemap.SetTile((x, y + 1), walls);
            walls.IsMirrored = true;
            tilemap.SetTile((x + 1, y + 1), walls);
        }
    }
    private static void SetLake(this Tilemap tilemap, (int x, int y) position, (int width, int height) radius)
    {
        tilemap.SetEllipse(position, radius, true, null, Tile.MATH_APPROXIMATE);
        tilemap.Replace((0, 0, tilemap.Size.width, tilemap.Size.height), Tile.MATH_APPROXIMATE, null,
            new Tile(Tile.ICON_WAVE, Blue),
            new Tile(Tile.ICON_WAVE, Blue, 2),
            new Tile(Tile.ICON_WAVES, Blue),
            new Tile(Tile.ICON_WAVES, Blue, 2));
    }
    private static void FillWithRandomGrass(this Tilemap tilemap)
    {
        var color = Green.ToDark(0.4f);
        tilemap.Replace((0, 0, tilemap.Size.width, tilemap.Size.height), 0, null,
            new Tile(Tile.SHADE_1, color),
            new Tile(Tile.SHADE_1, color, 1),
            new Tile(Tile.SHADE_1, color, 2),
            new Tile(Tile.SHADE_1, color, 3),
            new Tile(Tile.SHADE_2, color),
            new Tile(Tile.SHADE_2, color, 1),
            new Tile(Tile.SHADE_2, color, 2),
            new Tile(Tile.SHADE_2, color, 3));
    }
}