namespace Pure.Examples.ExamplesApplications;

using Engine.Window;
using Engine.Tilemap;
using Engine.Collision;
using Engine.Utilities;

public static class EightBallPool
{
    public static void Run()
    {
        Window.Title = "Pure - Pool Example";

        var lineCollisions = new LinePack(LINE_COLLISIONS);
        var tilemaps = new TilemapPack(TILEMAPS);
        var layer = new Layer((48, 27));
        var balls = new List<Ball>();
        var timeAtPress = 0f;
        var isCharging = false;
        var isCanceled = false;
        var isAnyBallMoving = false;

        for (var i = 0; i < 16; i++)
            balls.Add(new((0, 0), 0));

        ResetBalls();
        Window.SetIconFromTile(layer,
            (Tile.SUBSCRIPT_8_TH, Color.White), (Tile.SHAPE_CIRCLE_BIG, Color.Black));

        HandleInput();

        while (Window.KeepOpen())
        {
            Time.Update();

            for (var i = 0; i < tilemaps.Count; i++)
                layer.DrawTilemap(tilemaps[i]);

            isAnyBallMoving = false;
            foreach (var ball in balls)
            {
                MoveBall(ball);
                TryBallCollision(ball);
                TryWallCollision(ball);
                DrawBall(ball);
            }

            DrawStick();
            layer.DrawCursor();
            layer.Draw();
        }

        void HandleInput()
        {
            Mouse.Button.Left.OnPress(() =>
            {
                if (isAnyBallMoving)
                    return;

                timeAtPress = Time.Clock;
                isCanceled = false;
                isCharging = true;
            });
            Mouse.Button.Left.OnRelease(() =>
            {
                if (isCanceled || isAnyBallMoving)
                    return;

                var power = Math.Min(Time.Clock - timeAtPress, 2);
                var mousePos = layer.PixelToWorld(Mouse.CursorPosition);

                balls[0].MoveAngle = Angle.BetweenPoints(mousePos, balls[0].Position + new Point(0.5f));
                balls[0].Speed = power * 75;
                isCharging = false;
            });
            Mouse.Button.Right.OnPress(() => isCanceled = true);
        }
        void DrawStick()
        {
            if (isAnyBallMoving)
                return;

            var (mx, my) = layer.PixelToWorld(Mouse.CursorPosition);
            var a = balls[0].Position + (0.5f, 0.5f);
            var angle = Angle.BetweenPoints(a, (mx, my));
            var power = isCharging && isCanceled == false ? Math.Min(Time.Clock - timeAtPress, 2) : 0;
            var b = a.MoveAt(angle, 8 + power * 4);

            a = a.MoveAt(angle, 1 + power * 4);

            layer.DrawLines(new Line(a, b, Color.Brown.ToDark()));
        }

        void ResetBalls()
        {
            var colors = new[]
            {
                Color.White, Color.Yellow.ToDark(0.3f), Color.Blue, Color.Red, Color.Purple,
                Color.Orange, Color.Green.ToDark(0.3f), Color.Red.ToDark(), Color.Black,
                Color.Yellow.ToDark(0.3f), Color.Blue, Color.Red, Color.Purple, Color.Orange,
                Color.Green.ToDark(0.3f), Color.Red.ToDark()
            };
            var order = new[] { 0, 9, 7, 12, 15, 8, 1, 6, 10, 3, 14, 11, 2, 13, 4, 5 };
            var positions = new Point[]
            {
                (11.5f, 13), (34, 13), (35, 13.5f), (35, 12.5f), (36, 14), (36, 13), (36, 12),
                (37, 14.5f), (37, 13.5f), (37, 12.5f), (37, 11.5f), (38, 15), (38, 14), (38, 13),
                (38, 12), (38, 11)
            };

            for (var i = 0; i < balls.Count; i++)
            {
                var n = order[i];
                balls[n].Color = colors[n];
                balls[n].Position = positions[i];
                balls[n].Number = n;
            }
        }
        void MoveBall(Ball ball)
        {
            ball.Position = ball.Position.MoveAt(ball.MoveAngle, ball.Speed, Time.Delta);
            ball.Speed = Math.Max(ball.Speed - Time.Delta * 10, 0);

            if (ball.Speed > 0)
                isAnyBallMoving = true;
        }
        void TryBallCollision(Ball ball)
        {
            foreach (var otherBall in balls)
            {
                var pos = ball.Position + 0.5f;
                var posOther = otherBall.Position + 0.5f;

                if (ball == otherBall || ball.Speed == 0 || pos.Distance(posOther) > 0.75f)
                    continue;

                var angleBetweenBalls = Angle.BetweenPoints(posOther.XY, pos.XY);
                var bounceLine = new Line
                {
                    A = posOther.MoveAt(angleBetweenBalls - 90, 3),
                    B = posOther.MoveAt(angleBetweenBalls + 90, 3),
                    Color = Color.White
                };

                var speed = ball.Speed;
                var angle = ball.MoveAngle;

                var reflectAngle = bounceLine.NormalizeToPoint(pos.XY).Angle - 90;
                ball.MoveAngle = ball.MoveAngle.Reflect(reflectAngle);
                ball.Position = ball.Position.MoveAt(reflectAngle, 0.2f);
                ball.Speed = speed * 0.5f;

                otherBall.MoveAngle = ball.MoveAngle - 180;
                otherBall.Position = otherBall.Position.MoveAt(reflectAngle - 180, 0.2f);
                otherBall.Speed = speed * 0.9f;

                var penaltyAngle = new Angle(angleBetweenBalls - angle + 180);
                var penaltyMultiplier = penaltyAngle > 270 ?
                    ((float)penaltyAngle).Map((290, 360), (0, 1)) :
                    ((float)penaltyAngle).Map((0, 70), (1, 0));
                otherBall.Speed *= penaltyMultiplier;
            }
        }
        void TryWallCollision(Ball ball)
        {
            for (var i = 0; i < lineCollisions.Count; i++)
            {
                var line = lineCollisions[i];
                var pos = ball.Position + 0.5f;
                var closestPoint = (Point)line.ClosestPoint(pos.XY);
                var distance = closestPoint.Distance(pos);

                if (distance > 0.5f)
                    continue;

                var reflectAngle = line.NormalizeToPoint(pos.XY).Angle - 90;
                ball.MoveAngle = ball.MoveAngle.Reflect(reflectAngle);
                ball.Position = ball.Position.MoveAt(reflectAngle, 0.2f);
                ball.Speed *= 0.75f;
            }
        }
        void DrawBall(Ball ball)
        {
            var p = ball.Position;

            if (ball.Number <= 8)
            {
                layer.DrawTiles(p, new Tile(Tile.SHAPE_CIRCLE_BIG, ball.Color));
                DrawBallNumber();
                return;
            }

            layer.DrawTiles(p, new Tile(Tile.SHAPE_CIRCLE_BIG, new Color(220)));
            layer.DrawTiles(p, new Tile(Tile.BAR_DEFAULT_STRAIGHT, ball.Color));
            DrawBallNumber();

            void DrawBallNumber()
            {
                var numberTile = new Tile(Tile.SUBSCRIPT_0_TH + ball.Number);
                if (ball.Number > 9)
                {
                    numberTile -= 9;
                    var secondDigitTile = new Tile(Tile.SUBSCRIPT_1_ST);
                    layer.DrawTiles((p.X + 0.1f, p.Y - 0.1f), secondDigitTile);
                    layer.DrawTiles((p.X + 0.6f, p.Y - 0.1f), numberTile);
                    return;
                }

                layer.DrawTiles((p.X + 0.3f, p.Y - 0.1f), numberTile);
            }
        }
    }

    private class Ball
    {
        public int Number { get; set; }
        public float Speed { get; set; }
        public Angle MoveAngle { get; set; }
        public Point Position { get; set; }
        public Color Color { get; set; }

        public Ball(Point position, Color color)
        {
            Position = position;
            Color = color;
        }
    }

    private const string TILEMAPS =
        "Y2VgYDAAYmkGBDCC4iAgfnt7I2eDg4DLxc7cg2ouOaeWrQtNfXDgxo977anxjLPfTUw5f91yua1kbHTl3R0ys5UdXH" +
        "CAP2efmaYXbqtk0VdjBJo6+yJvk4FAW79TTQHrY75jC2MOxd45/FJE5JBj91OvbO/f9Z0n5YMOTu5/3PjYyUVdaue3" +
        "+cWuXxa5b3/p0ZF6zLHJa8M2v8fLf2vHXWyv037JvWn1jwd72t/vN3acGJ+3yvpIrc0s3W053cZrWK9uY7VeNrG0lz" +
        "G1J66B3zDz2OW9br32AsfbT0mcTXnX/KVOcMaLvwdfOT87wH6gkb+BWZ6B3Y6Br+aGZON51hoPhsCjTS8YgPSPDS4X" +
        "7WzuKum6sT9wZlpgrxO4jD/sy/G1Cxv4rZnt/vQc/Skg8YPB4gNjwQPmBzt+PKjW6GKUT1KawyvbwD9z07H2mw8K3c" +
        "/ZidjKh54+dqJzS9hD6R8CAp9nvCgJqHr8Yd+H7JfHbkrsrGN9EvPx1dabe2qaXuj53Da9twUc/t58TQ4CLQ/FhPQT" +
        "9zR/PBAnOnPFzKYeHrWcNf/2t1zgjriyJ8x2e8cOl9J6+9t+Zy8dK+Uz1fX+fGS/h4mV6u2IZC/LK/1t+vb/N7+1FL" +
        "t78WME/6RHf6p8P2p02i/9XCVRVawT498ketLMzHu3rMKkMx8XShgzIIMTGp7fI3avvmT5Ua7z11vXzFXTLtl8LPz7" +
        "LSzM8HAFu875fXfLtW5957Npc/HYVmPut6vYZrq/Fsjdhw9yMhhwsGy4fVi16zf/c/cXDHXCJ7yDflYxo5hfzkyaah" +
        "CfBYiTEpOzWYF0WlF+Xgk7kFGQn5ydWlIMAA==";
    private const string LINE_COLLISIONS =
        "XZI9agMxEIXnGFtumTqFSUKIVjpFyj2KjuIjuAjERYoFG2ITL9mAwS59hK3cJnp6I0bxgtDj0/zP3kn9RcfTdyJzut" +
        "98gt2vyFWkSazXt8IGR4a36MhWJUY6kzJq8lZ9qRELMcnmKveHN7uDZ67JW97J0/9HGTQY/HfKqNnHXhm1yDqdUdl7" +
        "1ryPytZZow6RszLEQi27yu5T7baV3Sbp043vXn1Hb/WNGus+WH3snX1vvM0PMfs8CzL6iCyCzQAaM3gKNivoby/xOd" +
        "1fyqBRb80eAvPy7R+LuHVHWb92jNd0Fq9RNjhj2BPy678RHwN7WOTb+phvGGaC/2is/j9o2MG+MOzy4rirlbNdLpWJ" +
        "s10OL+xPhIyacyhsqwxzbataWldqAvsD";
}