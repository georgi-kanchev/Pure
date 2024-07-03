namespace Pure.Examples.ExamplesSystems;

using Engine.Collision;
using Engine.Tilemap;
using Engine.Utilities;
using Engine.Window;

public static class Collision
{
    public static void Run()
    {
        Window.Title = "Pure - Collision Example";

        var aspectRatio = Monitor.Current.AspectRatio;
        var tilemap = new Tilemap((aspectRatio.width * 3, aspectRatio.height * 3));
        var background = new Tilemap(tilemap.Size);
        var (w, h) = tilemap.Size;

        var collisionMap = new SolidMap();
        collisionMap.SolidsAdd(Tile.ICON_WAVE, new Solid(0, 0, 0.5f, 1, Color.Yellow)); // lake
        collisionMap.SolidsAdd(Tile.ICON_WAVES, new Solid(0, 0, 1, 0.5f, Color.Yellow)); // lake
        collisionMap.SolidsAdd(Tile.GEOMETRY_ANGLE, new Solid(0, 0, 1, 1)); // house roof
        collisionMap.SolidsAdd(Tile.GEOMETRY_ANGLE_RIGHT, new Solid(0, 0, 1, 1)); // house wall
        collisionMap.SolidsAdd(Tile.UPPERCASE_I, new Solid(0, 0, 1, 1)); // tree trunk
        collisionMap.SolidsAdd(Tile.PATTERN_33, new Solid(0, 0, 1, 1)); // tree top

        // icon tiles are 7x7, not 8x8, cut one row & column,
        // hitbox and tile on screen might mismatch since the tile is pixel perfect
        // and the hitbox is not
        const float SCALE = 1f - 1f / 8f;
        var hitbox = new SolidPack(new Solid(0, 0, 1, 1)) { Scale = (SCALE, SCALE) };
        var layer = new Layer(tilemap.Size);

        tilemap.FillWithRandomGrass();
        tilemap.SetLake((0, 0), (14, 9));
        tilemap.SetLake((26, 18), (5, 7));
        tilemap.SetLake((16, 24), (12, 6));
        tilemap.SetHouses((30, 10), (34, 11), (33, 8));
        tilemap.SetBridge((21, 16), (31, 16));
        tilemap.SetRoad((32, 0), (32, 26));
        tilemap.SetRoad((33, 10), (47, 10));
        tilemap.SetRoad((20, 16), (0, 16));
        tilemap.SetTrees((31, 5), (26, 8), (20, 12), (39, 11), (36, 18), (38, 19));
        tilemap.SetBackgrounds(background);

        collisionMap.Update(tilemap);

        layer.Tint = Color.Blue.ToBright();

        Keyboard.Key.A.OnPress(() =>
        {
            var mousePos = layer.PixelToWorld(Mouse.CursorPosition);
            var (mx, my) = ((int)mousePos.x, (int)mousePos.y);
            collisionMap.IgnoredCellsAdd(new Solid(mx - 1, my - 1, 3, 3));
        });
        Keyboard.Key.S.OnPress(() => collisionMap.IgnoredCellsClear());

        while (Window.KeepOpen())
        {
            Time.Update();

            var mousePosition = layer.PixelToWorld(Mouse.CursorPosition);
            var isOverlapping = collisionMap.IsOverlapping(hitbox);
            var id = isOverlapping ? Tile.FACE_SAD : Tile.FACE_SMILING;
            var tint = isOverlapping ? Color.Red : Color.Green;
            var tile = new Tile(id, tint);
            var line = new Line((mousePosition.x - 1, mousePosition.y), (15, 15), Color.Red);
            var crossPoints = line.CrossPoints(collisionMap);

            hitbox.Position = mousePosition;
            line.Color = crossPoints.Length > 0 ? Color.Red : Color.Green;

            layer.DrawTilemap(background);
            layer.DrawTilemap(tilemap);
            //layer.DrawRectangles(collisionMap);
            //layer.DrawLines(line);
            //layer.DrawPoints(crossPoints);
            //layer.DrawTiles(mousePosition, tile);
            layer.DrawCursor();

            layer.Blur((0, 0, w, h), (2f, 2f), Color.Blue);
            layer.Distort((0, 0, w, h), (0, 4), (0, 100), Color.Blue.ToDark());
            layer.Distort((0, 0, w, h), (0, 1), (0, 100), Color.Green.ToDark(0.7f).ToDark());

            layer.Light((30, 8), 5f, Color.Red);
            layer.Light((39, 12), 5f, Color.Green);
            layer.Light(mousePosition, 5f, Color.Blue);
            layer.BlockLight((30, 10, 2, 2));
            layer.BlockLight((34, 11, 2, 2));
            layer.BlockLight((33, 8, 2, 2));

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
                new Tile(Tile.PATTERN_33, Color.Green.ToDark(0.7f)));
            tilemap.SetTile((x, y), new(Tile.UPPERCASE_I, Color.Brown.ToDark(0.4f)));
        }
    }
    private static void SetBridge(this Tilemap tilemap, (int x, int y) pointA, (int x, int y) pointB) { tilemap.SetLine(pointA, pointB, null, new Tile(Tile.BAR_STRIP_STRAIGHT, Color.Brown.ToDark())); }
    private static void SetRoad(this Tilemap tilemap, (int x, int y) pointA, (int x, int y) pointB)
    {
        var angle = pointA.x == pointB.x ? 1 : 0;
        tilemap.SetLine(pointA, pointB, null,
            new Tile(Tile.BAR_SPIKE_STRAIGHT, Color.Brown, (sbyte)angle));
    }
    private static void SetHouses(this Tilemap tilemap, params (int x, int y)[] positions)
    {
        foreach (var t in positions)
        {
            var (x, y) = t;
            var roof = new Tile(Tile.GEOMETRY_ANGLE, Color.Red.ToDark());
            var walls = new Tile(Tile.GEOMETRY_ANGLE_RIGHT, Color.Brown.ToBright());

            tilemap.SetTile((x, y), roof);
            roof.IsMirrored = true;
            tilemap.SetTile((x + 1, y), roof);

            tilemap.SetTile((x, y + 1), walls);
            walls.IsMirrored = true;
            tilemap.SetTile((x + 1, y + 1), walls);
        }
    }
    private static void SetLake(
        this Tilemap tilemap,
        (int x, int y) position,
        (int width, int height) radius)
    {
        tilemap.SetEllipse(position, radius, true, null, Tile.MATH_APPROXIMATE);
        tilemap.Replace((0, 0, tilemap.Size.width, tilemap.Size.height), Tile.MATH_APPROXIMATE, null,
            new Tile(Tile.ICON_WAVE, Color.Blue),
            new Tile(Tile.ICON_WAVE, Color.Blue, 2),
            new Tile(Tile.ICON_WAVES, Color.Blue),
            new Tile(Tile.ICON_WAVES, Color.Blue, 2));
    }
    private static void FillWithRandomGrass(this Tilemap tilemap)
    {
        var color = Color.Green.ToDark(0.4f);
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