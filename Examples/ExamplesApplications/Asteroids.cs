namespace Pure.Examples.ExamplesApplications;

using Engine.Flow;
using Engine.Tilemap;
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
        var tilemap = new Tilemap(layer.Size);
        var ship = new Shape("Y2ZABg32EMxwgIFhwX4obf8fCBBiDg4glUhi9shiAA==")
        {
            Type = Type.Ship,
            Scale = (0.5f, 0.5f),
            Position = (5, 15)
        };
        var shapes = new List<Shape>();
        var playArea = new Solid(0, 0, tilemap.View.Width, tilemap.View.Height);
        var highScore = 0;
        var score = 0;
        var shotCooldown = 0f;

        Delay.Wait(1f, SpawnAsteroid, true);

        while (Window.KeepOpen())
        {
            if (score > highScore)
                highScore = score;

            if (Keyboard.Key.Space.IsPressed())
                TrySpawnShot();

            tilemap.Flush();
            Time.Update();
            Delay.Update(Time.Delta);
            shotCooldown -= Time.Delta;

            tilemap.SetText((0, 0), $"BEST: {highScore}\n" +
                                    $"SCORE: {score}\n\n" +
                                    $"SHOT: {(shotCooldown < 0 ? "READY!" : $"{shotCooldown:F1}")}");

            HandleShip();
            HandleShapes();

            ship.UpdateAndDraw(layer);
            layer.DrawTilemap(tilemap);
            layer.DrawCursor();
            layer.Draw();
        }

        void HandleShip()
        {
            var mousePos = layer.PixelToPosition(Mouse.CursorPosition);
            var targetAngle = new Point(ship.Position).Angle(mousePos);
            ship.Angle = new Angle(ship.Angle).RotateTo(targetAngle, 200f, Time.Delta).Limit((-30, 30));
            ship.MoveAngle = ship.Angle;
            ship.Velocity = 15f;
            ship.Position = (5f, ship.Position.y);
        }
        void SpawnAsteroid()
        {
            shapes.Add(new(asteroidShapes.ChooseOne() ?? "")
            {
                Type = Type.Asteroid,
                Velocity = (10f, 25f).Random(),
                Scale = ((0.2f, 1.5f).Random(3), (0.2f, 1.5f).Random(3)),
                MoveAngle = (170f, 190f).Random(),
                Position = (playArea.Width + 10f, playArea.Height / 2f),
                Angle = (0f, 360f).Random(),
                Torque = (-10f, 10f).Random()
            });
        }
        void TrySpawnShot()
        {
            if (shotCooldown > 0)
                return;

            shotCooldown = 5f;
            shapes.Add(new((0, 0, Color.Orange), (2, 0, Color.Orange))
            {
                Type = Type.Shot,
                Position = ship.Position,
                Angle = ship.Angle,
                MoveAngle = ship.Angle,
                Velocity = 40f
            });
        }
        void HandleShapes()
        {
            var shapesToDestroy = new List<Shape>();

            foreach (var shape in shapes)
            {
                var type = shape.Type;
                shape.UpdateAndDraw(layer);

                if (type == Type.Shot)
                    foreach (var asteroid in shapes)
                        if (asteroid.Type == Type.Asteroid && shape.IsOverlapping(asteroid))
                        {
                            shapesToDestroy.AddRange(new[] { shape, asteroid });
                            score++;
                        }

                if (type == Type.Asteroid && ship.IsOverlapping(shape))
                {
                    score = 0;
                    shotCooldown = 0f;
                    ship.Position = (5, 15);
                    shapes.Clear();
                    return;
                }

                var destroyShot = type == Type.Shot && playArea.IsOverlapping(shape) == false;
                var destroyAsteroid = type == Type.Asteroid && shape.Position.x < -10;

                if (destroyAsteroid)
                    score++;

                if (destroyShot || destroyAsteroid)
                    shapesToDestroy.Add(shape);
            }

            foreach (var shape in shapesToDestroy)
                shapes.Remove(shape);
        }
    }

#region Backend
    private static readonly string[] asteroidShapes =
    {
        "Y2NABg32ELxgPwOD0kEGhhsHGBj2HPhfVVUFYzMwTADiNQ4QMQgbqGcfA8MXqBiEzcCwA4gn2EPEIGwGBgV7kB6IGIQNxEC7FA6CxAA=",
        "Y2dABg32EGxxgIFhAxAbODAwBBz4X1VVxcCgAWYzMCxxALEhYhA2UD0Q34CKQdgMDAv2g/RDxCBsBgYJoP4DUDEIm4FB6iDIZogYhA1UC5RbA7YXAA==",
        "Y2NABg32ECxwgIFhChB3APGD/f+rqqpgbIhciwNEDMJmYJgDxEegYhA2UA6IE6B6IWwgBtIbDiD0guwAia0BiwEA",
        "Y2VABg32EHxhP5A+wMAwB4gV9v+vqqpiYJgBZDMAxTmAtIYjRIwBzAZiBwYGCweIGITNwHAEiD2gYiB2gAPE3IoDIDEA"
    };
    private static Layer layer = new();

    private enum Type
    {
        Ship,
        Asteroid,
        Shot
    }

    private class Shape : LinePack
    {
        public Type Type { get; set; }
        public Angle MoveAngle { get; set; }
        public float Velocity { get; set; }
        public float Torque { get; set; }

        public Shape(string base64) : base(base64)
        {
        }
        public Shape(params (float x, float y, uint color)[] points) : base(points)
        {
        }
        public void UpdateAndDraw(Layer layer)
        {
            Position = new Point(Position).MoveAt(MoveAngle, Velocity, Time.Delta);
            Angle = new Angle(Angle).Rotate(Torque, Time.Delta);

            layer.DrawLines(ToBundlePoints());
        }
    }
#endregion
}