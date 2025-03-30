namespace Pure.Examples.Games;

using Engine.Window;
using Engine.Utility;
using Engine.Tiles;

public static class Minesweeper
{
    public static void Run()
    {
        Window.Title = "Pure - Minesweeper Example";

        const int TILE_MINE = Tile.ICON_SKY_STAR;
        const int TILE_0 = Tile.EMPTY;

        var (aw, ah) = Monitor.Current.AspectRatio;
        var (w, h) = (aw * 3, ah * 3);
        var maps = new List<TileMap> { new((w, h)), new((w, h)), new((w, h)) };
        var layer = new LayerTiles((w, h));
        var gameOver = false;

        PrepareGame();
        HandleInput();

        while (Window.KeepOpen())
        {
            Time.Update();

            maps.ForEach(map => layer.DrawTileMap(map));
            layer.DrawMouseCursor();
            layer.Render();
        }

        void PrepareGame()
        {
            Window.BackgroundColor = Color.Gray;
            gameOver = false;
            maps[0].Fill(Tile.NUMBER_0);

            for (var i = 0; i < 200; i++)
            {
                var x = (0, w - 1).Random();
                var y = (0, h - 1).Random();

                while (maps[0].TileAt((x, y)).Id == TILE_MINE)
                {
                    x = (0, w - 1).Random();
                    y = (0, h - 1).Random();
                }

                for (var j = -1; j <= 1; j++)
                    for (var k = -1; k <= 1; k++)
                    {
                        var pos = (x + j, y + k);
                        var id = maps[0].TileAt(pos).Id;
                        if (id != TILE_MINE)
                            maps[0].SetTile(pos, (ushort)(id + 1));
                    }

                maps[0].SetTile((x, y), new(TILE_MINE, Color.Black));
            }

            maps[0].Replace(maps[0].View, Tile.NUMBER_0, new Tile(TILE_0, Color.Gray.ToDark()));
            maps[0].Replace(maps[0].View, Tile.NUMBER_1, new Tile(Tile.NUMBER_1, Color.Blue));
            maps[0].Replace(maps[0].View, Tile.NUMBER_2, new Tile(Tile.NUMBER_2, Color.Green.ToDark(0.7f)));
            maps[0].Replace(maps[0].View, Tile.NUMBER_3, new Tile(Tile.NUMBER_3, Color.Red));
            maps[0].Replace(maps[0].View, Tile.NUMBER_4, new Tile(Tile.NUMBER_4, Color.Blue.ToDark()));
            maps[0].Replace(maps[0].View, Tile.NUMBER_5, new Tile(Tile.NUMBER_5, Color.Red.ToDark()));
            maps[0].Replace(maps[0].View, Tile.NUMBER_6, new Tile(Tile.NUMBER_6, Color.Azure.ToDark()));
            maps[0].Replace(maps[0].View, Tile.NUMBER_7, new Tile(Tile.NUMBER_7, Color.Black));
            maps[0].Replace(maps[0].View, Tile.NUMBER_8, new Tile(Tile.NUMBER_8, Color.Gray.ToDark()));
            maps[1].Fill(new Tile(Tile.FULL, Color.Gray.ToDark()));
            maps[2].Fill(new Tile(Tile.SHAPE_SQUARE_BIG_HOLLOW, Color.Gray));
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

                var (x, y) = layer.PositionFromPixel(Mouse.CursorPosition);
                var pos = ((int)x, (int)y);
                var id = maps[0].TileAt(pos).Id;

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
                var (x, y) = layer.PositionFromPixel(Mouse.CursorPosition);
                Flag(((int)x, (int)y));
            });
        }

        void Collapse((int x, int y) position)
        {
            var (x, y) = position;

            var id = maps[1].TileAt(position).Id;
            if (id == Tile.EMPTY)
                return;

            var left = maps[0].TileAt((x - 1, y)).Id == TILE_0;
            var right = maps[0].TileAt((x + 1, y)).Id == TILE_0;
            var up = maps[0].TileAt((x, y - 1)).Id == TILE_0;
            var down = maps[0].TileAt((x, y + 1)).Id == TILE_0;

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
                    if (maps[0].TileAt((x + i, y + j)).Id != TILE_0)
                        Reveal((x + i, y + j));
        }

        void Reveal((int x, int y) position)
        {
            for (var i = 1; i < 3; i++)
                maps[i].SetTile(position, Tile.EMPTY);
        }

        void Flag((int x, int y) position)
        {
            if (maps[2].TileAt(position).Id == Tile.SHAPE_SQUARE_BIG_HOLLOW)
                maps[2].SetTile(position, Tile.ICON_FLAG);
            else if (maps[2].TileAt(position).Id == Tile.ICON_FLAG)
                maps[2].SetTile(position, new(Tile.SHAPE_SQUARE_BIG_HOLLOW, Color.Gray));
        }
    }
}