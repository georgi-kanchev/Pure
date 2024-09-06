namespace Pure.Examples.ExamplesSystems;

using Engine.Collision;
using Engine.Tilemap;
using Engine.Utilities;
using Engine.Window;

using static Engine.Utilities.Color;

public static class Collision
{
    public static void Run()
    {
        Window.Title = "Pure - Collision Example";
        Window.PixelScale = 5f;

        var aspectRatio = Monitor.Current.AspectRatio;
        var tilemaps = new TilemapPack(2, (aspectRatio.width * 3, aspectRatio.height * 3));
        var (w, h) = tilemaps.Size;

        var collisionMap = new SolidMap();
        collisionMap.SolidsAdd(Tile.ICON_WAVE, new Solid(0, 0, 1f, 1f, Yellow)); // lake
        collisionMap.SolidsAdd(Tile.ICON_WAVES, new Solid(0, 0, 1f, 1f, Yellow)); // lake
        collisionMap.SolidsAdd(Tile.GEOMETRY_ANGLE, new Solid(0, 0, 1, 1)); // house roof
        collisionMap.SolidsAdd(Tile.GEOMETRY_ANGLE_RIGHT, new Solid(0, 0, 1, 1)); // house wall
        collisionMap.SolidsAdd(Tile.UPPERCASE_I, new Solid(0, 0, 1, 1)); // tree trunk
        collisionMap.SolidsAdd(Tile.PATTERN_33, new Solid(0, 0, 1, 1)); // tree top

        // icon tiles are 7x7, not 8x8, cut one row & column,
        // hitbox and tile on screen might mismatch since the tile is pixel perfect
        // and the hitbox is not
        const float SCALE = 1f - 1f / 8f;
        var hitbox = new SolidPack(new Solid(0, 0, 1, 1)) { Scale = (SCALE, SCALE) };
        var layer = new Layer((w, h));

        tilemaps[1].FillWithRandomGrass();
        tilemaps[1].SetLake((0, 0), (14, 9));
        tilemaps[1].SetLake((26, 18), (5, 7));
        tilemaps[1].SetLake((16, 24), (12, 6));
        tilemaps[1].SetHouses((30, 10), (34, 11), (33, 8));
        tilemaps[1].SetBridge((21, 16), (31, 16));
        tilemaps[1].SetRoad((32, 0), (32, 26));
        tilemaps[1].SetRoad((33, 10), (47, 10));
        tilemaps[1].SetRoad((20, 16), (0, 16));
        tilemaps[1].SetTrees((31, 5), (26, 8), (20, 12), (39, 11), (36, 18), (38, 19));
        tilemaps[1].SetBackgrounds(tilemaps[0]);

        collisionMap.Update(tilemaps[1]);
        var collisionPack = collisionMap.ToArray();

        var waves = new SolidMap();
        waves.SolidsAdd(Tile.ICON_WAVE, new Solid(0, 0, 1, 1, Blue.ToDark()));
        waves.SolidsAdd(Tile.ICON_WAVES, new Solid(0, 0, 1, 1, Blue.ToDark()));
        waves.SolidsAdd(Tile.PATTERN_33, new Solid(0, 0, 1, 1, Green.ToDark(0.7f).ToDark()));
        waves.Update(tilemaps[1]);
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

            layer.DrawTilemap(tilemaps[0]);
            layer.DrawTilemap(tilemaps[1]);

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
                background.SetTile((j, i), new(Tile.SHADE_OPAQUE, color.ToDark()));
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