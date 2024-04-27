namespace Pure.Examples.ExamplesApplications;

using Engine.Window;
using Engine.Utilities;
using Engine.Collision;

public static class Asteroids
{
    public static void Run()
    {
        Window.Title = "Pure - Asteroids Example";
        Mouse.CursorCurrent = Mouse.Cursor.Crosshair;

        var (w, h) = Monitor.Current.AspectRatio;
        layer = new((w * 3, h * 3));
        var c = Color.White;
        var ship = new Shape((-0.5f, -0.5f, c), (-0.5f, 0.5f, c), (1f, 0f, c), (-0.5f, -0.5f, c))
            { Position = (5, 15), IsSlowingDown = true };
        var asteroids = new List<Shape>();
        var shots = new List<Shape>();

        Time.CallAfter(2f, () =>
        {
            if (asteroids.Count > 20)
                return;

            var asteroid = new Shape(asteroidShapes[0])
            {
                Velocity = (1f, 5f).Random(),
                Scale = ((0.1f, 1f).Random(3), (0.1f, 1f).Random(3)),
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
            var shot = new Shape((0, 0, Color.White), (1, 0, Color.White), (0, 0, Color.White))
            {
                Position = ship.Position,
                Angle = ship.Angle,
                MoveAngle = ship.Angle,
                Velocity = 40f
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

            // foreach (var asteroid in asteroids)
            //     asteroid.UpdateAndDraw(layer);

            ship.UpdateAndDraw(layer);

            layer.DrawCursor();
            layer.Draw();
        }

        void HandleShip()
        {
            var mousePos = layer?.PixelToWorld(Mouse.CursorPosition) ?? (0, 0);
            var targetAngle = new Point(ship.Position).Angle(mousePos);
            ship.Angle = new Angle(ship.Angle).RotateTo(targetAngle, 200f, Time.Delta).Limit((-30, 30));
            ship.MoveAngle = ship.Angle;
            ship.Velocity = 15f;
            ship.Position = (5f, ship.Position.y);
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
        public Angle MoveAngle { get; set; }
        public float Velocity { get; set; }
        public bool IsSlowingDown { get; set; }
        public bool IsDestroyed { get; private set; }

        public Shape(string base64) : base(base64)
        {
        }
        public Shape(params (float x, float y, uint color)[] points) : base(points)
        {
        }
        public void UpdateAndDraw(Layer layer)
        {
            if (IsDestroyed)
                return;

            if (IsSlowingDown)
                Velocity = Math.Max(Velocity - Time.Delta * 5f, 0);

            Position = new Point(Position).MoveAt(MoveAngle, Velocity, Time.Delta);

            var playArea = new Solid(0, 0, layer.TilemapSize.width, layer.TilemapSize.height);
            if (IsOverlapping(playArea) == false)
                IsDestroyed = true;

            layer.DrawLines(ToBundlePoints());
        }
    }
#endregion
}