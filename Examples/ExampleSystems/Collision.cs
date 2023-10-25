namespace Pure.Examples.ExamplesSystems;

using Engine.Collision;
using Engine.Tilemap;
using Engine.Utilities;
using Engine.Window;

public static class Collision
{
    public static void Run()
    {
        Window.Create();
        //Mouse.CursorGraphics = Mouse.Cursor.None;
        Mouse.IsCursorTile = false;

        var aspectRatio = Window.MonitorAspectRatio;
        var tilemap = new Tilemap((aspectRatio.width * 3, aspectRatio.height * 3));
        var background = new Tilemap(tilemap.Size);

        var collisionMap = new Map();
        collisionMap.AddRectangle(new((1, 1), (0, 0), Color.Red), Tile.ICON_WAVE); // lake
        collisionMap.AddRectangle(new((1, 1)), Tile.ICON_WAVE_DOUBLE); // lake
        collisionMap.AddRectangle(new((1, 1)), Tile.GEOMETRY_ANGLE); // house roof
        collisionMap.AddRectangle(new((1, 1)), Tile.GEOMETRY_ANGLE_RIGHT); // house wall
        collisionMap.AddRectangle(new((1, 1)), Tile.UPPERCASE_I); // tree trunk
        collisionMap.AddRectangle(new((1, 1)), Tile.PATTERN_33); // tree top

        // icon tiles are 7x7, not 8x8, cut one row & column,
        // hitbox and tile on screen might mismatch since the tile is pixel perfect
        // and the hitbox is not
        var scale = 1f - 1f / 8f;
        var hitbox = new Hitbox((0, 0), (scale, scale), new Rectangle((1f, 1f)));

        while (Window.IsOpen)
        {
            Window.Activate(true);
            Time.Update();

            FillWithRandomGrass();
            SetLake((0, 0), (14, 9));
            SetLake((26, 18), (5, 7));
            SetLake((16, 24), (12, 6));
            SetHouses((30, 10), (34, 11), (33, 8));
            SetBridge((21, 16), (31, 16));
            SetRoad((32, 0), (32, 26));
            SetRoad((33, 10), (47, 10));
            SetRoad((20, 16), (0, 16));
            SetTrees((31, 5), (26, 8), (20, 12), (39, 11), (36, 18), (38, 19));
            SetBackgrounds();

            collisionMap.Update(tilemap);

            Window.DrawTiles(background);
            Window.DrawTiles(tilemap);

            var mousePosition = Mouse.PixelToWorld(Mouse.CursorPosition);
            hitbox.Position = mousePosition;

            var isOverlapping = collisionMap.IsOverlapping(hitbox);
            var id = isOverlapping ? Tile.FACE_SAD : Tile.FACE_SMILING;
            var tint = isOverlapping ? Color.Red : Color.Green;
            var tile = new Tile(id, tint);
            var line = new Line((mousePosition.x - 1, mousePosition.y), (15, 15), Color.Red);
            var crossPoints = line.CrossPoints(collisionMap);
            line.Color = crossPoints.Length > 0 ? Color.Red : Color.Green;

            Window.DrawLines(line);
            Window.DrawPoints(crossPoints);
            Window.DrawTile(mousePosition, tile);
            Window.Activate(false);
        }

        return;

        void FillWithRandomGrass()
        {
            var color = Color.Green.ToDark(0.4f);
            tilemap.Flush();
            tilemap.Replace((0, 0), tilemap.Size, 0, default,
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
            // every sec slight mutation times 3
            var seed = (0, 0, (int)Time.RuntimeClock * 3);
            tilemap.SetEllipse(position, radius, Tile.MATH_APPROXIMATE);
            tilemap.Replace((0, 0), tilemap.Size, Tile.MATH_APPROXIMATE, seed,
                new Tile(Tile.ICON_WAVE, Color.Blue, 0),
                new Tile(Tile.ICON_WAVE, Color.Blue, 2),
                new Tile(Tile.ICON_WAVE_DOUBLE, Color.Blue, 0),
                new Tile(Tile.ICON_WAVE_DOUBLE, Color.Blue, 2));
        }
        void SetHouses(params (int x, int y)[] positions)
        {
            foreach (var t in positions)
            {
                var (x, y) = t;
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
            foreach (var t in positions)
            {
                var (x, y) = t;
                tilemap.SetEllipse((x, y - 1), (1, 1), new(Tile.PATTERN_33, Color.Green.ToDark(0.7f)));
                tilemap.SetTile((x, y), new(Tile.UPPERCASE_I, Color.Brown.ToDark(0.4f)));
            }
        }
        void SetBackgrounds()
        {
            for (var i = 0; i < tilemap.Size.height; i++)
                for (var j = 0; j < tilemap.Size.width; j++)
                {
                    var color = (Color)tilemap.TileAt((j, i)).Tint;
                    background.SetTile((j, i), new(Tile.SHADE_OPAQUE, color.ToDark()));
                }
        }
    }
}