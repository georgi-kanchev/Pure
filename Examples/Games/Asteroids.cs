namespace Pure.Examples.Games;

using Engine.Execution;
using Engine.Tiles;
using Engine.Window;
using Engine.Utility;
using Engine.Collision;

public static class Asteroids
{
    public static void Run()
    {
        Window.Title = "Pure - Asteroids Example";
        Mouse.CursorCurrent = Mouse.Cursor.Crosshair;

        var (w, h) = Monitor.Current.AspectRatio;
        var layer = new LayerTiles((w * 3, h * 3));
        var tilemap = new TileMap(layer.Size);
        var playArea = new Solid((0, 0), tilemap.Size);
        var bulletsAndAsteroids = new List<Shape>();
        var (score, highScore, shotCooldown) = (0, 0, 0f);
        var ship = new Shape([(-1, -1), (-1, 1), (2, 0), (-1, -1)])
        {
            Type = Type.Ship, Scale = (0.5f, 0.5f), Position = (5, 15)
        };
        var asteroidShapes = new (float x, float y)[][]
        {
            [(-2, 0), (-1, 1.5f), (1, 2), (2, 1), (1.5f, -1), (0, -2), (-1.5f, -1.2f), (-2, 0)],
            [(0, 2), (1.5f, 1), (2, -0.5f), (1, -2), (-0.5f, -2), (-1.8f, -1), (-2, 0.8f), (-1, 1.8f), (0, 2)],
            [(0, 1.5f), (1.5f, 0.8f), (2, 0), (1.2f, -1.5f), (-0.2f, -2), (-1.8f, -1), (-1.5f, 0.5f), (-0.5f, 1.2f), (0, 1.5f)],
            [(0, 2), (2, 1.5f), (1.8f, -0.5f), (0.5f, -2), (-1.5f, -1.8f), (-2, -0.2f), (-1.8f, 1.5f), (-0.5f, 2), (0, 2)]
        };

        //====================================================
        // spawning asteroids
        Flow.CallEvery(1f, () =>
        {
            bulletsAndAsteroids.Add(new(asteroidShapes.ChooseOne()!)
            {
                Type = Type.Asteroid,
                Velocity = (10f, 25f).Random(),
                Scale = ((0.2f, 4f).Random(), (0.2f, 4f).Random()),
                MoveAngle = (170f, 190f).Random(),
                Position = (playArea.Width + 10f, playArea.Height / 2f),
                Angle = (0f, 360f).Random(),
                Torque = (-10f, 10f).Random()
            });
        });
        //====================================================

        while (Window.KeepOpen())
        {
            highScore = score > highScore ? score : highScore;

            // shooting ====================================================
            if ((Keyboard.IsAnyJustPressed() || Mouse.IsAnyJustPressed()) && shotCooldown < 0)
            {
                shotCooldown = 5f;
                bulletsAndAsteroids.Add(new([(0, 0), (2, 0)])
                {
                    Type = Type.Shot,
                    Position = ship.Position,
                    Angle = ship.Angle,
                    MoveAngle = ship.Angle,
                    Velocity = 40f
                });
            }

            // ship ====================================================
            var mousePos = layer.PositionFromPixel(Mouse.CursorPosition);
            var targetAngle = new Point(ship.Position).Angle(mousePos);
            ship.Angle = new Angle(ship.Angle).RotateTo(targetAngle, 200f, Time.Delta).Limit((-30, 30));
            ship.MoveAngle = ship.Angle;
            ship.Velocity = 15f;
            ship.Position = (5f, ship.Position.y);
            ship.UpdateAndDraw(layer);

            // bullets & asteroids ====================================================
            var thingsToDestroy = new List<Shape>();
            foreach (var thing in bulletsAndAsteroids)
            {
                var type = thing.Type;
                thing.UpdateAndDraw(layer);

                if (type == Type.Shot)
                    foreach (var asteroid in bulletsAndAsteroids)
                        if (asteroid.Type == Type.Asteroid && thing.IsOverlapping(asteroid))
                        {
                            thingsToDestroy.AddRange([thing, asteroid]);
                            score++;
                        }

                if (type == Type.Asteroid && ship.IsOverlapping(thing))
                {
                    score = 0;
                    shotCooldown = 0f;
                    ship.Position = (5, 15);
                    bulletsAndAsteroids.Clear();
                    break;
                }

                var destroyShot = type == Type.Shot && playArea.IsOverlapping(thing) == false;
                var destroyAsteroid = type == Type.Asteroid && thing.Position.x < -10;

                if (destroyAsteroid)
                    score++;

                if (destroyShot || destroyAsteroid)
                    thingsToDestroy.Add(thing);
            }

            foreach (var shape in thingsToDestroy)
                bulletsAndAsteroids.Remove(shape);

            // time ====================================================
            Time.Update();
            Flow.Update(Time.Delta);
            shotCooldown -= Time.Delta;

            // rendering ====================================================
            tilemap.Flush();
            tilemap.SetText((0, 0), $"BEST: {highScore}\n" +
                                    $"SCORE: {score}\n\n" +
                                    $"SHOT: {(shotCooldown < 0 ? "READY!" : $"{shotCooldown:F1}")}");

            layer.DrawTileMap(tilemap);
            layer.DrawMouseCursor();
            layer.Render();
        }
    }

#region Backend
    private enum Type { Ship, Asteroid, Shot }

    private class Shape((float x, float y)[] points) : LinePack(points)
    {
        public Type Type { get; set; }
        public Angle MoveAngle { get; set; }
        public float Velocity { get; set; }
        public float Torque { get; set; }

        public void UpdateAndDraw(LayerTiles layerTiles)
        {
            Position = new Point(Position).MoveAt(MoveAngle, Velocity, Time.Delta);
            Angle = new Angle(Angle).Rotate(Torque, Time.Delta);
            layerTiles.DrawLine(ToBundlePoints());
        }
    }
#endregion
}