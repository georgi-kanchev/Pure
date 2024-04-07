namespace Pure.Examples.ExamplesApplications;

using Engine.Window;
using Engine.Utilities;
using Engine.Collision;

public static class Asteroids
{
    public static void Run()
    {
        Window.Title = "Pure - Minesweeper Example";
        Mouse.CursorCurrent = Mouse.Cursor.Crosshair;

        var (w, h) = Monitor.Current.AspectRatio;
        layer = new((w * 3, h * 3));
        var c = Color.White;
        var ship = new Shape((4.5f, 4.5f, c), (4.5f, 5.5f, c), (6f, 5f, c), (4.5f, 4.5f, c))
            { Position = (20, 20), IsSlowingDown = true, IsWrapping = true };
        var asteroids = new List<Shape>();
        var shots = new List<Shape>();

        Time.CallAfter(2f, () =>
        {
            if (asteroids.Count > 20)
                return;

            var asteroid = new Shape(asteroidShapes[0])
            {
                IsWrapping = true,
                Velocity = (1f, 5f).Random(),
                Scale = ((0.1f, 1f).Random(precision: 3), (0.1f, 1f).Random(precision: 3)),
                MoveAngle = (0f, 360f).Random(),
                Position =
                    (0f, 0f).ChooseOneFrom(float.NaN,
                        (layer.TilemapSize.width, 0f),
                        (layer.TilemapSize.width, layer.TilemapSize.height),
                        (0f, layer.TilemapSize.height))
            };
            asteroids.Add(asteroid);
        }, true);
        Mouse.Button.Right.OnPress(() =>
        {
            var shot = new Shape((0, 0, Color.White), (1, 0, Color.White))
            {
                Position = ship.Position,
                Rotation = ship.Rotation,
                Velocity = 40f,
            };
            shots.Add(shot);
        });

        while (Window.KeepOpen())
        {
            Time.Update();

            HandleShip();

            for (var i = 0; i < shots.Count; i++)
            {
                var shot = shots[i];
                shot.UpdateAndDraw(layer);

                if (shot.IsDestroyed == false)
                    continue;

                shots.Remove(shot);
                i--;
            }

            foreach (var asteroid in asteroids)
                asteroid.UpdateAndDraw(layer);

            ship.UpdateAndDraw(layer);

            layer.DrawCursor();
            layer.Draw();
        }

        void HandleShip()
        {
            var mousePos = layer?.PixelToWorld(Mouse.CursorPosition) ?? (0, 0);
            var targetAngle = ship.Position.Angle(mousePos);
            ship.Rotation = ship.Rotation.RotateTo(targetAngle, 400f, Time.Delta);
            ship.MoveAngle = ship.MoveAngle.RotateTo(ship.Rotation, 200f, Time.Delta);

            if (Mouse.Button.Left.IsPressed())
                ship.Velocity = 15f;
        }
    }

#region Backend
    private static readonly string[] asteroidShapes =
    {
        "TY9BDYAwDEW/AEI4IAAZ3NY5QBJHjpOAAA4IQAQSOCAAB/CXX2BNmr68dW1aoYwxKBcDEuvAeoSb8TLQs04mJwZ2+" +
        "uRODHTM051Y/+soJwY2+sv7xJzBbL1PDDTxnyfW27fXtDe/paIvO7DOfocYWC3fmt0D"
    };
    private static Layer? layer;

    private class Shape : LinePack
    {
        public Point Position { get; set; }
        public Angle Rotation { get; set; }
        public Angle MoveAngle { get; set; }
        public float Velocity { get; set; }
        public bool IsSlowingDown { get; set; }
        public bool IsWrapping { get; set; }
        public bool IsDestroyed { get; set; }

        public Shape(string base64) : base(base64)
        {
        }
        public Shape(params (float x, float y, uint color)[] points) : base(default, default, points)
        {
        }
        public void UpdateAndDraw(Layer layer)
        {
            if (IsDestroyed)
                return;

            var (w, h) = layer.TilemapSize;

            if (IsSlowingDown)
                Velocity = Math.Max(Velocity - Time.Delta * 5f, 0);

            Position = Position.MoveAt(MoveAngle, Velocity, Time.Delta);

            var linePack = CalculateWorldPoints();
            var playArea = new Solid(0, 0, layer.TilemapSize.width, layer.TilemapSize.height);
            var isOutsideView = linePack.IsOverlapping(playArea) == false;

            if (IsWrapping && isOutsideView)
                Position = (Position.X.Wrap(w), Position.Y.Wrap(h));

            if (IsWrapping == false && isOutsideView)
                IsDestroyed = true;

            layer.DrawLines(linePack.ToBundlePoints());
        }

#region Backend
        private LinePack CalculateWorldPoints()
        {
            var result = new (float x, float y, uint color)[Count + 1];

            for (var i = 0; i < Count; i++)
            {
                var point = this[i].A - new Point(5f, 5f);
                var direction = new Direction(Point.Zero.Angle(point) + Rotation);
                var nonUnit = new Point(direction.X, direction.Y);
                nonUnit.X *= Scale.width;
                nonUnit.Y *= Scale.height;

                var distance = Point.Zero.Distance(point);
                var target = Position + nonUnit * distance;
                result[i] = target;
            }

            result[^1] = result[0];

            return result;
        }
#endregion
    }
#endregion
}