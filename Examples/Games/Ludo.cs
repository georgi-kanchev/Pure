using Pure.Animation;

namespace Pure.Examples.Games;

using Window;
using Utilities;
using Tilemap;

public static class Ludo
{
    public class Dice
    {
        private readonly Animation<int> diceAnimation =
            new(Tile.GAME_DICE_3, Tile.GAME_CARD_DIAMOND, Tile.GAME_DICE_6,
                Tile.SHAPE_SQUARE, Tile.GAME_DICE_4, Tile.GAME_DICE_5, Tile.GAME_CARD_DIAMOND,
                Tile.GAME_DICE_4, Tile.SHAPE_SQUARE, Tile.GAME_CARD_DIAMOND, Tile.GAME_DICE_2,
                Tile.GAME_DICE_1, Tile.GAME_DICE_6, Tile.SHAPE_SQUARE, Tile.GAME_CARD_DIAMOND)
            {
                Duration = 1.5f,
                IsRepeating = true
            };
        private readonly (int x, int y) position;

        public Dice((int x, int y) position) => this.position = position;

        public int CurrentValue { get; private set; }
        public bool IsRolling { get; set; }

        public void Update()
        {
            if (IsRolling)
                diceAnimation.Update(Time.Delta);

            if ((IsRolling == false).Once("onDiceStop"))
                CurrentValue = (1, 6).Random();

            var frame = diceAnimation.CurrentValue;
            var id = IsRolling ? frame : Tile.GAME_DICE_1 + CurrentValue - 1;
            Window.DrawTile(position, (id, Color.White, 0, false, false));
        }
    }

    public static void Run()
    {
        Window.Create(3f);

        var map = new Tilemap((48, 27));
        var dice = new Dice((map.Size.width / 2, map.Size.height / 2));

        while (Window.IsOpen)
        {
            Window.Activate(true);
            Time.Update();
            dice.Update();

            dice.IsRolling = Keyboard.IsKeyPressed(Keyboard.Key.A);

            map.Fill();

            RecreateMap(map);

            var mousePosition = map.PointFrom(Mouse.CursorPosition, Window.Size);
            map.SetTextLine((0, 0), ((Indices)mousePosition).ToString());

            Window.DrawTiles(map.ToBundle());
            Window.Activate(false);
        }
    }

    public static void RecreateMap(Tilemap map)
    {
        var (mapWidth, mapHeight) = map.Size;
        var (centerX, centerY) = (mapWidth / 2, mapHeight / 2);
        var (cellsHorX, cellsHorY) = (centerX - mapHeight / 2, centerY - 1);
        var (cellsVertX, cellsVertY) = (centerX - 1, centerY - mapHeight / 2);

        map.SetRectangle((cellsHorX, cellsHorY), (mapHeight, 3), Tile.SHAPE_SQUARE);
        map.SetRectangle((cellsVertX, cellsVertY), (3, mapHeight), Tile.SHAPE_SQUARE);
        map.SetTile((centerX, centerY), Tile.SHADE_TRANSPARENT);

        map.SetTile((centerX, centerY - 1), new(Tile.ICON_HOME, Color.Green, 2));
        map.SetRectangle((cellsVertX + 1, cellsVertY + 1), (1, 11), new(Tile.SHAPE_SQUARE, Color.Green));
        map.SetTile((cellsVertX + 2, cellsVertY), new(Tile.ARROW_BIG, Color.Green, 1));
        map.SetEllipse((cellsVertX + 8, cellsVertY + 6), (4, 4),
            new(Tile.SHADE_OPAQUE, Color.Green.ToDark()));

        map.SetTile((centerX + 1, centerY), new(Tile.ICON_HOME, Color.Red, 3));
        map.SetRectangle((centerX + 2, centerY), (11, 1), new(Tile.SHAPE_SQUARE, Color.Red));
        map.SetTile((cellsHorX + mapHeight - 1, cellsHorY + 2), new(Tile.ARROW_BIG, Color.Red, 2));
        map.SetEllipse((centerX + 7, centerY + 7), (4, 4), new(Tile.SHADE_OPAQUE, Color.Red.ToDark()));

        map.SetTile((centerX, centerY + 1), new(Tile.ICON_HOME, Color.Blue, 0));
        map.SetRectangle((centerX, centerY + 2), (1, 11), new(Tile.SHAPE_SQUARE, Color.Blue));
        map.SetTile((cellsVertX, cellsVertY + mapHeight - 1), new(Tile.ARROW_BIG, Color.Blue, 3));
        map.SetEllipse((cellsHorX + 6, cellsHorY + 8), (4, 4),
            new(Tile.SHADE_OPAQUE, Color.Blue.ToDark()));

        map.SetTile((centerX - 1, centerY), new(Tile.ICON_HOME, Color.Yellow, 1));
        map.SetRectangle((cellsHorX + 1, cellsHorY + 1), (11, 1), new(Tile.SHAPE_SQUARE, Color.Yellow));
        map.SetTile((cellsHorX, cellsHorY), new(Tile.ARROW_BIG, Color.Yellow));
        map.SetEllipse((cellsHorX + 6, cellsVertY + 6), (4, 4),
            new(Tile.SHADE_OPAQUE, Color.Yellow.ToDark(0.3f)));

        SetSquares((15, 4), (17, 4), (19, 4));
        SetSquares((15, 6), (19, 6));
        SetSquares((15, 8), (17, 8), (19, 8));

        SetSquares((29, 4), (31, 4), (33, 4));
        SetSquares((29, 6), (33, 6));
        SetSquares((29, 8), (31, 8), (33, 8));

        SetSquares((29, 18), (31, 18), (33, 18));
        SetSquares((29, 20), (33, 20));
        SetSquares((29, 22), (31, 22), (33, 22));

        SetSquares((15, 18), (17, 18), (19, 18));
        SetSquares((15, 20), (19, 20));
        SetSquares((15, 22), (17, 22), (19, 22));

        void SetSquares(params (int x, int y)[] positions)
        {
            foreach (var t in positions)
                map.SetTile(t, Tile.SHAPE_SQUARE);
        }
    }
}