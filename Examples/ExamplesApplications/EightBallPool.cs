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

        var collisions = new SolidPack(COLLISIONS);
        var tilemaps = new TilemapPack(MAP);
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
            Tile.SUBSCRIPT_8_TH, Color.White, Tile.SHAPE_CIRCLE_BIG, Color.Black);

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

            balls[0].MoveAngle = Angle.FromPoints(mousePos, balls[0].Position + new Point(0.5f));
            balls[0].Speed = power * 75;
            isCharging = false;
        });
        Mouse.Button.Right.OnPress(() => isCanceled = true);

        while (Window.KeepOpen())
        {
            Time.Update();

            for (var i = 0; i < tilemaps.Count; i++)
                layer.DrawTilemap(tilemaps[i]);

            isAnyBallMoving = false;
            foreach (var ball in balls)
                UpdateAndDrawBall(ball);

            DrawStick();

            layer.DrawCursor();
            layer.Draw();
        }

        void ResetBalls()
        {
            const int X = 34;
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
        void DrawStick()
        {
            if (isAnyBallMoving)
                return;

            var (mx, my) = layer.PixelToWorld(Mouse.CursorPosition);
            var a = balls[0].Position + (0.5f, 0.5f);
            var angle = Angle.FromPoints(a, (mx, my));
            var power = isCharging && isCanceled == false ? Math.Min(Time.Clock - timeAtPress, 2) : 0;
            var b = a.MoveAt(angle, 8 + power * 4);

            a = a.MoveAt(angle, 1 + power * 4);

            layer.DrawLines(new Line(a, b, Color.Brown.ToDark()));
        }
        void UpdateAndDrawBall(Ball ball)
        {
            ball.Position = ball.Position.MoveAt(ball.MoveAngle, ball.Speed, Time.Delta);
            ball.Speed = Math.Max(ball.Speed - Time.Delta * 10, 0);

            if (ball.Speed > 0)
                isAnyBallMoving = true;

            var solid = new Solid(new Point(1 - 1 / 6f), ball.Position + 1 / 8f);
            for (var i = 0; i < collisions.Count; i++)
                if (collisions[i].IsOverlapping(solid))
                {
                    var reflectAngle = 180;
                    reflectAngle = collisions[i].Position.x > ball.Position.X ? 0 : reflectAngle;
                    reflectAngle = collisions[i].Position.y < ball.Position.Y ? 90 : reflectAngle;
                    reflectAngle = collisions[i].Position.y > ball.Position.Y ? 270 : reflectAngle;
                    ball.MoveAngle = ball.MoveAngle.Reflect(reflectAngle);
                    ball.Position = ball.Position.MoveAt(reflectAngle, 0.2f);
                    ball.Speed *= 0.75f;
                }

            if (ball.Number <= 8)
            {
                layer.DrawTiles(ball.Position, new Tile(Tile.SHAPE_CIRCLE_BIG, ball.Color));
                DrawBallNumber();
                return;
            }

            layer.DrawTiles(ball.Position, new Tile(Tile.SHAPE_CIRCLE_BIG, new Color(220)));
            layer.DrawTiles(ball.Position, new Tile(Tile.BAR_DEFAULT_STRAIGHT, ball.Color));
            DrawBallNumber();

            void DrawBallNumber()
            {
                var numberTile = new Tile(Tile.SUBSCRIPT_0_TH + ball.Number);
                if (ball.Number > 9)
                {
                    numberTile -= 9;
                    var secondDigitTile = new Tile(Tile.SUBSCRIPT_1_ST);
                    layer.DrawTiles((ball.Position.X + 0.1f, ball.Position.Y - 0.1f), secondDigitTile);
                    layer.DrawTiles((ball.Position.X + 0.6f, ball.Position.Y - 0.1f), numberTile);
                    return;
                }

                layer.DrawTiles((ball.Position.X + 0.3f, ball.Position.Y - 0.1f), numberTile);
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

    private const string MAP =
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
    private const string COLLISIONS =
        "bVNJTsNAEJyn+AkoLAqL4ukRhxzzhBx4iJ+QI8c8AgEHBCbiEKIIOHHmCfwAXO4a3O7BkiOnVDNdXV19GezT1Prmp4" +
        "r4fxV++N1zZiGsiB0QCx22JjY1vJbY3PC+iC0M75vYkti1dD9RsRvh2Xq4D1hbs8ZMsVtiqNEQ2/5z9pW81vB2Al16" +
        "XyC2l1LLfb4vDmetbxV5+k0trLuq6VeHRWLrWv1C3YoYtM3juDdoW+T7TN3er+5dy4A9CPwc8x7ZXzDY05+HA/Ysqs" +
        "nyNqLaLe9Fyn4/pMzLO3n5PvT/Juh/wNA/vK5MDfSPmdgZoX/Mzs4I/WPGPi+Zl3OFbOA+m7874YxNTjHfTxnPd5t5" +
        "LkNe305KfTjn9U1Sqe8wFfoaYHsZ7VFzlNQ/u2/HiT6bvTxJ5TwukmbDzK2ZElsa70+T5sXO6CxpXoLhnSfNi+m3Ab" +
        "aRcU5Rw+cUWvzOwJec6ax5YnhZCzDkMToMubUZAub3HJjb816z31Vgflfhgd9VeOV2tfd0vKu/";
}