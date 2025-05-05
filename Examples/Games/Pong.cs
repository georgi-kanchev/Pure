using Pure.Engine.Tiles;
using Pure.Engine.Utility;
using Pure.Engine.Window;
using Pure.Engine.Collision;
using Monitor = Pure.Engine.Window.Monitor;

namespace Pure.Examples.Games;

public static class Pong
{
    public static void Run()
    {
        Window.Title = "Pure - Pong Example";

        const float BALL_SPEED = 20f, PADDLE_SPEED = 10f, PADDLE_HEIGHT = 4f;
        var (w, h) = Monitor.Current.AspectRatio;
        var layer = new LayerTiles((w * 3, h * 3));
        var tilemap = new TileMap(layer.Size);
        var center = new Point(layer.Size.width / 2f, layer.Size.height / 2f);
        var (ballAngle, ballPos) = (new Angle(45f), center);
        var (paddleLeft, paddleRight) = (new Point(0, 0), new Point(layer.Size.width - 1, 0));

        while (Window.KeepOpen())
        {
            Time.Update();

            // player controls ====================================================
            paddleLeft = paddleLeft.MoveAt(270, Keyboard.Key.ArrowUp.IsPressed() ? PADDLE_SPEED : 0, Time.Delta);
            paddleLeft = paddleLeft.MoveAt(90, Keyboard.Key.ArrowDown.IsPressed() ? PADDLE_SPEED : 0, Time.Delta);
            paddleLeft = (paddleLeft.X, Math.Clamp(paddleLeft.Y, 0, tilemap.Size.height - PADDLE_HEIGHT));

            // AI controls ====================================================
            var aiAim = ballPos.Y < paddleRight.Y + PADDLE_HEIGHT / 2f ? 270 : 90;
            paddleRight = paddleRight.MoveAt(aiAim, PADDLE_SPEED, Time.Delta);
            paddleRight = (paddleRight.X, paddleRight.Y.Limit((0, tilemap.Size.height - PADDLE_HEIGHT)));

            // ball ====================================================
            var (x, y) = ballPos.XY;
            var ball = new Solid(ballPos, (1, 1));
            ballPos = ballPos.MoveAt(ballAngle, BALL_SPEED, Time.Delta);

            if (x.IsBetween((0, tilemap.Size.width)) == false) // restart play when outside
                (ballAngle, ballPos) = ((0f, 360f).Random(), center);

            if (y < 0 || y > layer.Size.height - 1) // bounce from top/bottom of the layer
            {
                ballPos = ballPos.MoveAt(ballAngle > 180 ? 90 : 270, 1f);
                ballAngle = ballAngle.Reflect(ballAngle > 180 ? 90 : 270);
            }

            if (new Solid(paddleLeft, (1, PADDLE_HEIGHT)).IsOverlapping(ball) ||
                new Solid(paddleRight, (1, PADDLE_HEIGHT)).IsOverlapping(ball))
            {
                var bounceAngle = ball.X < layer.Size.width / 2f ? 0 : 180; // bounce from paddles
                (ballAngle, ballPos) = (ballAngle.Reflect(bounceAngle), ballPos.MoveAt(bounceAngle, 1f));
            }

            // rendering ====================================================
            layer.DrawTiles(paddleLeft, new Tile(Tile.SHADE_4), 1f, (1, (int)PADDLE_HEIGHT), true);
            layer.DrawTiles(paddleRight, new Tile(Tile.SHADE_4), 1f, (1, (int)PADDLE_HEIGHT), true);
            layer.DrawTiles(ballPos, new Tile(Tile.SHAPE_CIRCLE), 1f, (1, 1), true);
            layer.DrawMouseCursor();
            layer.Render();
        }
    }
}