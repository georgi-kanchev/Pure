namespace Pure.Examples.Games;

using Engine.Window;
using Engine.Utility;
using Engine.Tilemap;

public static class Minesweeper
{
    public static void Run()
    {
        Window.Title = "Pure - Minesweeper Example";

        const int TILE_MINE = Tile.ICON_SKY_STAR;
        const int TILE_0 = Tile.EMPTY;

        var (w, h) = Monitor.Current.AspectRatio;
        var maps = new TilemapPack(3, (w * 3, h * 3));
        var layer = new Layer(maps.Size);
        var gameOver = false;

        PrepareGame();
        HandleInput();

        while (Window.KeepOpen())
        {
            Time.Update();

            foreach (var map in maps.Tilemaps)
                layer.DrawTilemap(map);

            layer.DrawMouseCursor();
            layer.Draw();
        }

        void PrepareGame()
        {
            Window.BackgroundColor = Color.Gray;
            gameOver = false;
            maps.Tilemaps[0].Fill(null, Tile.NUMBER_0);

            for (var i = 0; i < 200; i++)
            {
                var x = (0, maps.Size.width - 1).Random();
                var y = (0, maps.Size.height - 1).Random();

                while (maps.Tilemaps[0].TileAt((x, y)).Id == TILE_MINE)
                {
                    x = (0, maps.Size.width - 1).Random();
                    y = (0, maps.Size.height - 1).Random();
                }

                for (var j = -1; j <= 1; j++)
                    for (var k = -1; k <= 1; k++)
                    {
                        var pos = (x + j, y + k);
                        var id = maps.Tilemaps[0].TileAt(pos).Id;
                        if (id != TILE_MINE)
                            maps.Tilemaps[0].SetTile(pos, (ushort)(id + 1));
                    }

                maps.Tilemaps[0].SetTile((x, y), new(TILE_MINE, Color.Black));
            }

            maps.Tilemaps[0].Replace(maps.View, Tile.NUMBER_0, null,
                new Tile(TILE_0, Color.Gray.ToDark()));
            maps.Tilemaps[0].Replace(maps.View, Tile.NUMBER_1, null, new Tile(Tile.NUMBER_1, Color.Blue));
            maps.Tilemaps[0].Replace(maps.View, Tile.NUMBER_2, null,
                new Tile(Tile.NUMBER_2, Color.Green.ToDark(0.7f)));
            maps.Tilemaps[0].Replace(maps.View, Tile.NUMBER_3, null, new Tile(Tile.NUMBER_3, Color.Red));
            maps.Tilemaps[0].Replace(maps.View, Tile.NUMBER_4, null,
                new Tile(Tile.NUMBER_4, Color.Blue.ToDark()));
            maps.Tilemaps[0].Replace(maps.View, Tile.NUMBER_5, null, new Tile(Tile.NUMBER_5, Color.Red.ToDark()));
            maps.Tilemaps[0].Replace(maps.View, Tile.NUMBER_6, null,
                new Tile(Tile.NUMBER_6, Color.Azure.ToDark()));
            maps.Tilemaps[0].Replace(maps.View, Tile.NUMBER_7, null, new Tile(Tile.NUMBER_7, Color.Black));
            maps.Tilemaps[0].Replace(maps.View, Tile.NUMBER_8, null,
                new Tile(Tile.NUMBER_8, Color.Gray.ToDark()));
            maps.Tilemaps[1].Fill(null, new Tile(Tile.FULL, Color.Gray.ToDark()));
            maps.Tilemaps[2].Fill(null, new Tile(Tile.SHAPE_SQUARE_BIG_HOLLOW, Color.Gray));
        }

        void HandleInput()
        {
            Mouse.Button.Left.OnPress(() =>
            {
                if (gameOver)
                {
                    PrepareGame();
                    return;
                }

                var (x, y) = layer.PixelToPosition(Mouse.CursorPosition);
                var pos = ((int)x, (int)y);
                var id = maps.Tilemaps[0].TileAt(pos).Id;

                if (id == TILE_0)
                    Collapse(pos);

                Reveal(pos);

                if (id != TILE_MINE)
                    return;

                Window.BackgroundColor = Color.Red.ToDark();
                gameOver = true;
            });
            Mouse.Button.Right.OnPress(() =>
            {
                var (x, y) = layer.PixelToPosition(Mouse.CursorPosition);
                Flag(((int)x, (int)y));
            });
        }

        void Collapse((int x, int y) position)
        {
            var (x, y) = position;

            var id = maps.Tilemaps[1].TileAt(position).Id;
            if (id == Tile.EMPTY)
                return;

            var left = maps.Tilemaps[0].TileAt((x - 1, y)).Id == TILE_0;
            var right = maps.Tilemaps[0].TileAt((x + 1, y)).Id == TILE_0;
            var up = maps.Tilemaps[0].TileAt((x, y - 1)).Id == TILE_0;
            var down = maps.Tilemaps[0].TileAt((x, y + 1)).Id == TILE_0;

            Reveal(position);

            if (left)
                Collapse((x - 1, y));
            if (right)
                Collapse((x + 1, y));
            if (up)
                Collapse((x, y - 1));
            if (down)
                Collapse((x, y + 1));

            for (var i = -1; i <= 1; i++)
                for (var j = -1; j <= 1; j++)
                    if (maps.Tilemaps[0].TileAt((x + i, y + j)).Id != TILE_0)
                        Reveal((x + i, y + j));
        }

        void Reveal((int x, int y) position)
        {
            for (var i = 1; i < 3; i++)
                maps.Tilemaps[i].SetTile(position, Tile.EMPTY);
        }

        void Flag((int x, int y) position)
        {
            if (maps.Tilemaps[2].TileAt(position).Id == Tile.SHAPE_SQUARE_BIG_HOLLOW)
                maps.Tilemaps[2].SetTile(position, Tile.ICON_FLAG);
            else if (maps.Tilemaps[2].TileAt(position).Id == Tile.ICON_FLAG)
                maps.Tilemaps[2].SetTile(position, new(Tile.SHAPE_SQUARE_BIG_HOLLOW, Color.Gray));
        }
    }
}