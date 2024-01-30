namespace Pure.Examples.ExamplesApplications;

using Engine.Animation;
using Engine.UserInterface;
using Engine.Window;
using Engine.Utilities;
using Engine.Tilemap;

public static class Ludo
{
    private static readonly Dictionary<Team, Color> TeamColors = new()
    {
        { Team.Blue, Color.Blue },
        { Team.Red, Color.Red },
        { Team.Yellow, Color.Yellow },
        { Team.Green, Color.Green }
    };
    private static Dictionary<(int x, int y), Corner> Corners { get; } = new();
    private static Dictionary<Team, Corner> CornersStart { get; } = new();
    private static Dictionary<Team, Corner> CornersFinish { get; } = new();
    private static Dictionary<Team, (int x, int y)> PositionsHome { get; } = new();
    private static Dictionary<Team, List<(int x, int y)>> PositionsBase { get; } = new();
    private static List<Pawn> Pawns { get; } = new();
    private static Layer layer;

    private enum Team
    {
        Yellow,
        Green,
        Red,
        Blue
    }

    private struct Corner
    {
        public readonly (int x, int y) position;
        public readonly (int x, int y) direction;

        public Corner((int x, int y) position, (int x, int y) direction)
        {
            this.position = position;
            this.direction = direction;
        }
    }

    private class Pawn
    {
        private const float MOVE_TIMER = 0.2f;

        private float moveTimer;
        private int remainingMoves;
        private readonly (int x, int y) basePosition;

        public Team Team { get; }
        public bool IsInBase { get; private set; } = true;
        public bool IsInHome { get; private set; }

        public (int x, int y) Position { get; private set; }
        public (int x, int y) Direction { get; private set; }

        public Pawn(Team team, (int x, int y) position)
        {
            Team = team;
            Position = position;
            basePosition = position;
        }

        public void Update(float deltaTime)
        {
            if (remainingMoves <= 0)
                return;

            moveTimer -= deltaTime;

            if (moveTimer > 0)
                return;

            remainingMoves--;
            moveTimer = MOVE_TIMER;
            Position = (Position.x + Direction.x, Position.y + Direction.y);

            if (Corners.TryGetValue(Position, out var corner))
                Direction = corner.direction;

            if (CornersFinish.TryGetValue(Team, out var cor) && cor.position == Position)
            {
                Direction = cor.direction;
                IsInHome = true;
            }
        }
        public void Move(int dice)
        {
            remainingMoves = dice;
            moveTimer = MOVE_TIMER;

            if (dice == 6 && IsInBase)
            {
                IsInBase = false;
                Position = CornersStart[Team].position;
                Direction = CornersStart[Team].direction;
                remainingMoves = 0;
            }
        }
        public void Draw()
        {
            layer.DrawTiles(Position, (Tile.GAME_CHESS_ROOK, TeamColors[Team], 0, false, false));
            layer.DrawTiles(Position, (Tile.GAME_CHESS_ROOK_HOLLOW, Color.Black, 0, false, false));
        }
    }

    private class Dice
    {
        private readonly Animation<int> diceAnimation =
            new(Tile.GAME_DICE_3, Tile.GAME_CARD_DIAMOND, Tile.GAME_DICE_6,
                Tile.SHAPE_SQUARE, Tile.GAME_DICE_4, Tile.GAME_DICE_5, Tile.GAME_CARD_DIAMOND,
                Tile.GAME_DICE_4, Tile.SHAPE_SQUARE, Tile.GAME_CARD_DIAMOND, Tile.GAME_DICE_2,
                Tile.GAME_DICE_1, Tile.GAME_DICE_6, Tile.SHAPE_SQUARE, Tile.GAME_CARD_DIAMOND)
            {
                Duration = 1.5f,
                IsLooping = true
            };
        private readonly (int x, int y) position;

        public Dice((int x, int y) position)
        {
            this.position = position;
        }

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
            layer.DrawTiles(position, (id, Color.White, 0, false, false));
        }
    }

    public static void Run()
    {
        Window.Create();
        Window.Title = "Pure - Ludo Example";

        var map = new Tilemap((48, 27));
        var dice = new Dice((map.Size.width / 2, map.Size.height / 2));
        var slider = new Slider((1, 1)) { Progress = 1f / 5f };
        var button = new Button((1, map.Size.height - 2)) { Text = "START" };

        layer = new(map.Size);

        CreateHomePositions();
        CreateBasePositions();
        SetCorners(map.Size);
        SetPawns();

        while (Window.KeepOpen())
        {
            Input.TilemapSize = map.Size;
            Input.Position = layer.PixelToWorld(Mouse.CursorPosition);
            Input.Update(Mouse.ButtonIDsPressed, Mouse.ScrollDelta);

            Time.Update();
            dice.Update();
            slider.Update();
            button.Update();

            foreach (var pawn in Pawns)
                pawn.Update(Time.Delta);

            map.Flush();
            SetMap(map);
            SetArrows(map);
            SetSlider(map, slider);
            SetButton(map, button);

            Mouse.CursorCurrent = (Mouse.Cursor)Input.CursorResult;

            layer.DrawTilemap(map.ToBundle());
            foreach (var pawn in Pawns)
                pawn.Draw();
        }
    }

    private static void CreateHomePositions()
    {
        PositionsHome.Add(Team.Yellow, (23, 13));
        PositionsHome.Add(Team.Green, (24, 12));
        PositionsHome.Add(Team.Red, (25, 13));
        PositionsHome.Add(Team.Blue, (24, 14));
    }
    private static void CreateBasePositions()
    {
        PositionsBase.Add(Team.Yellow,
            new() { (15, 4), (17, 4), (19, 4), (15, 6), (19, 6), (15, 8), (17, 8), (19, 8) });

        PositionsBase.Add(Team.Green,
            new() { (29, 4), (31, 4), (33, 4), (29, 6), (33, 6), (29, 8), (31, 8), (33, 8) });

        PositionsBase.Add(Team.Red,
            new() { (29, 18), (31, 18), (33, 18), (29, 20), (33, 20), (29, 22), (31, 22), (33, 22) });

        PositionsBase.Add(Team.Blue,
            new() { (15, 18), (17, 18), (19, 18), (15, 20), (19, 20), (15, 22), (17, 22), (19, 22) });
    }
    private static void SetPawns()
    {
        foreach (var kvp in PositionsBase)
            foreach (var position in kvp.Value)
                Pawns.Add(new(kvp.Key, position));
    }
    private static void SetCorners((int mapWidth, int mapHeight) mapSize)
    {
        var (mapWidth, mapHeight) = mapSize;
        var (centerX, centerY) = (mapWidth / 2, mapHeight / 2);
        var (cellsHorX, cellsHorY) = (centerX - mapHeight / 2, centerY - 1);
        var (cellsVertX, cellsVertY) = (centerX - 1, centerY - mapHeight / 2);
        var left = (-1, 0);
        var right = (1, 0);
        var up = (0, -1);
        var down = (0, 1);

        Corners.Add((cellsVertX, cellsVertY), new((cellsVertX, cellsVertY), right));
        Corners.Add((cellsVertX + 2, cellsVertY), new((cellsVertX + 2, cellsVertY), down));
        Corners.Add((centerX + 1, centerY - 1), new((centerX + 1, centerY - 1), right));
        Corners.Add((cellsHorX + mapHeight - 1, cellsHorY),
            new((cellsHorX + mapHeight - 1, cellsHorY), down));
        Corners.Add((cellsHorX + mapHeight - 1, cellsHorY + 2),
            new((cellsHorX + mapHeight - 1, cellsHorY + 2), left));
        Corners.Add((centerX + 1, centerY + 1), new((centerX + 1, centerY + 1), down));
        Corners.Add((cellsVertX + 2, cellsVertY + mapHeight - 1),
            new((cellsVertX + 2, cellsVertY + mapHeight - 1), left));
        Corners.Add((cellsVertX, cellsVertY + mapHeight - 1),
            new((cellsVertX, cellsVertY + mapHeight - 1), up));
        Corners.Add((centerX - 1, centerY + 1), new((centerX - 1, centerY + 1), left));
        Corners.Add((cellsHorX, cellsHorY + 2), new((cellsHorX, cellsHorY + 2), up));
        Corners.Add((cellsHorX, cellsHorY), new((cellsHorX, cellsHorY), right));
        Corners.Add((centerX - 1, centerY - 1), new((centerX - 1, centerY - 1), up));

        CornersStart.Add(Team.Green, new((cellsVertX + 2, cellsVertY), down));
        CornersStart.Add(Team.Red, new((cellsHorX + mapHeight - 1, cellsHorY + 2), left));
        CornersStart.Add(Team.Blue, new((cellsVertX, cellsVertY + mapHeight - 1), up));
        CornersStart.Add(Team.Yellow, new((cellsHorX, cellsHorY), right));

        CornersFinish.Add(Team.Green, new((cellsVertX + 1, cellsVertY), down));
        CornersFinish.Add(Team.Red, new((cellsHorX + mapHeight - 1, cellsHorY + 1), left));
        CornersFinish.Add(Team.Blue, new((cellsVertX + 1, cellsVertY + mapHeight - 1), up));
        CornersFinish.Add(Team.Yellow, new((cellsHorX, cellsHorY + 1), right));
    }
    private static void SetMap(Tilemap map)
    {
        var (mapWidth, mapHeight) = map.Size;
        var (centerX, centerY) = (mapWidth / 2, mapHeight / 2);
        var (cellsHorX, cellsHorY) = (centerX - mapHeight / 2, centerY - 1);
        var (cellsVertX, cellsVertY) = (centerX - 1, centerY - mapHeight / 2);
        var teamColorRed = TeamColors[Team.Red];
        var teamColorGreen = TeamColors[Team.Green];
        var teamColorBlue = TeamColors[Team.Blue];
        var teamColorYellow = TeamColors[Team.Yellow];

        map.SetRectangle((cellsHorX, cellsHorY, mapHeight, 3), Tile.SHAPE_SQUARE);
        map.SetRectangle((cellsVertX, cellsVertY, 3, mapHeight), Tile.SHAPE_SQUARE);
        map.SetTile((centerX, centerY), Tile.SHADE_TRANSPARENT);

        map.SetTile((centerX, centerY - 1), new(Tile.ICON_HOME, teamColorGreen, 2));
        map.SetRectangle((cellsVertX + 1, cellsVertY + 1, 1, 11),
            new Tile(Tile.SHAPE_SQUARE, teamColorGreen));
        map.SetEllipse((cellsVertX + 8, cellsVertY + 6), (4, 4), true,
            new Tile(Tile.SHADE_OPAQUE, teamColorGreen.ToDark()));
        map.SetEllipse((cellsVertX + 8, cellsVertY + 6), (4, 4), false,
            new Tile(Tile.SHADE_OPAQUE, teamColorGreen));

        map.SetTile((centerX + 1, centerY), new(Tile.ICON_HOME, teamColorRed, 3));
        map.SetRectangle((centerX + 2, centerY, 11, 1), new Tile(Tile.SHAPE_SQUARE, teamColorRed));
        map.SetEllipse((centerX + 7, centerY + 7), (4, 4), true,
            new Tile(Tile.SHADE_OPAQUE, teamColorRed.ToDark()));
        map.SetEllipse((centerX + 7, centerY + 7), (4, 4), false,
            new Tile(Tile.SHADE_OPAQUE, teamColorRed));

        map.SetTile((centerX, centerY + 1), new(Tile.ICON_HOME, teamColorBlue, 0));
        map.SetRectangle((centerX, centerY + 2, 1, 11), new Tile(Tile.SHAPE_SQUARE, teamColorBlue));
        map.SetEllipse((cellsHorX + 6, cellsHorY + 8), (4, 4), true,
            new Tile(Tile.SHADE_OPAQUE, teamColorBlue.ToDark()));
        map.SetEllipse((cellsHorX + 6, cellsHorY + 8), (4, 4), false,
            new Tile(Tile.SHADE_OPAQUE, teamColorBlue));

        map.SetTile((centerX - 1, centerY), new(Tile.ICON_HOME, teamColorYellow, 1));
        map.SetRectangle((cellsHorX + 1, cellsHorY + 1, 11, 1),
            new Tile(Tile.SHAPE_SQUARE, teamColorYellow));
        map.SetEllipse((cellsHorX + 6, cellsVertY + 6), (4, 4), true,
            new Tile(Tile.SHADE_OPAQUE, teamColorYellow.ToDark(0.3f)));
        map.SetEllipse((cellsHorX + 6, cellsVertY + 6), (4, 4), false,
            new Tile(Tile.SHADE_OPAQUE, teamColorYellow));

        foreach (var kvp in PositionsBase)
            foreach (var position in kvp.Value)
                map.SetTile(position, Tile.SHADE_OPAQUE);
    }
    private static void SetArrows(Tilemap map)
    {
        var i = 1;
        foreach (var kvp in CornersStart)
        {
            var hasPawn = false;
            foreach (var pawn in Pawns)
                if (kvp.Value.position == pawn.Position)
                {
                    hasPawn = true;
                    break;
                }

            var tile = hasPawn ?
                (Tile.SHAPE_SQUARE, (uint)Color.White, (sbyte)0, false, false) :
                (Tile.ARROW, (uint)TeamColors[kvp.Key], (sbyte)i, false, false);
            map.SetTile(kvp.Value.position, tile);
            i++;
        }
    }
    private static void SetSlider(Tilemap map, Slider slider)
    {
        var e = slider;
        map.SetBar(e.Position, Tile.BAR_BIG_EDGE, Tile.BAR_BIG_STRAIGHT, Color.Gray, e.Size.width);
        map.SetTile(e.Handle.Position, new(Tile.SHADE_OPAQUE, GetColor(e, Color.Magenta)));
        map.SetTextLine((e.Position.x + e.Size.width + 1, e.Position.y),
            $"{(int)(e.Progress * 4)} Players");
    }
    private static void SetButton(Tilemap map, Block button)
    {
        map.SetTextLine(button.Position, button.Text, GetColor(button, Color.Gray));
    }

    private static Color GetColor(Block block, Color baseColor)
    {
        if (block.IsDisabled) return baseColor;
        if (block.IsPressedAndHeld) return baseColor.ToDark();
        else if (block.IsHovered) return baseColor.ToBright();

        return baseColor;
    }
}