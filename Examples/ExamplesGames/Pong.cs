namespace Pure.Examples.ExamplesGames;

using Engine.Tilemap;
using Engine.Utilities;
using Engine.Window;
using Engine.Collision;

public static class Pong
{
    private const float BALL_SPEED = 20f;
    private const float PADDLE_SPEED = 10f;
    private const int PADDLE_HEIGHT = 4;

    public static void Run()
    {
        Window.Create();
        var tilemap = new Tilemap((48, 27));

        var center = new Point(x: tilemap.Size.width / 2f, y: tilemap.Size.height / 2f);
        var ballAngle = new Angle(45f);
        var ballPosition = center;
        var paddleLeftPosition = new Point(x: 0, y: 0);
        var paddleRightPosition = new Point(x: tilemap.Size.width - 1, y: 0);
        var layer = new Layer(tilemap.Size);

        while (Window.IsOpen)
        {
            Window.Activate(true);
            Time.Update();

            layer.Clear();
            ballPosition = ballPosition.MoveAt(ballAngle, BALL_SPEED, Time.Delta);

            TryToScorePoint();
            ControlPaddle(ref paddleLeftPosition);
            FollowBall(ref paddleRightPosition);
            LimitPaddleToWindow(ref paddleLeftPosition);
            LimitPaddleToWindow(ref paddleRightPosition);
            TryToBounceBallOffWindow();
            TryToBounceBallOffPaddle(ref paddleLeftPosition);
            TryToBounceBallOffPaddle(ref paddleRightPosition);

            layer.DrawTiles(paddleLeftPosition, new Tile(Tile.SHADE_4), (1, PADDLE_HEIGHT), true);
            layer.DrawTiles(paddleRightPosition, new Tile(Tile.SHADE_4), (1, PADDLE_HEIGHT), true);
            layer.DrawTiles(ballPosition, new Tile(Tile.SHAPE_CIRCLE), (1, 1), true);
            layer.DrawCursor();
            Window.DrawLayer(layer);
            Window.Activate(false);
        }

        void TryToBounceBallOffWindow()
        {
            if (ballPosition.Y < 0)
            {
                ballAngle = ballAngle.Reflect(90);
                ballPosition = ballPosition.MoveAt(90, 0.5f);
            }
            else if (ballPosition.Y > tilemap.Size.height - 1)
            {
                ballAngle = ballAngle.Reflect(270);
                ballPosition = ballPosition.MoveAt(270, 0.5f);
            }
        }
        void TryToBounceBallOffPaddle(ref Point paddlePosition)
        {
            var paddle = new Rectangle((1, PADDLE_HEIGHT), paddlePosition);
            var ball = new Rectangle((1, 1), ballPosition);

            if (paddle.IsOverlapping(ball) == false)
                return;

            var angle = paddlePosition.X < ballPosition.Y ? 0 : 180;
            ballAngle = ballAngle.Reflect(angle);
            ballPosition = ballPosition.MoveAt(angle, 1f);
        }
        void FollowBall(ref Point paddlePosition)
        {
            var ballIsAbove = ballPosition.Y < paddlePosition.Y + PADDLE_HEIGHT / 2f;
            var angle = ballIsAbove ? 270 : 90;
            paddlePosition = paddlePosition.MoveAt(angle, PADDLE_SPEED, Time.Delta);
        }
        void ControlPaddle(ref Point paddlePosition)
        {
            if (Keyboard.IsKeyPressed(Keyboard.Key.ArrowUp))
                paddlePosition = paddlePosition.MoveAt(270, PADDLE_SPEED, Time.Delta);
            if (Keyboard.IsKeyPressed(Keyboard.Key.ArrowDown))
                paddlePosition = paddlePosition.MoveAt(90, PADDLE_SPEED, Time.Delta);
        }
        void LimitPaddleToWindow(ref Point paddlePosition)
        {
            paddlePosition = (
                x: paddlePosition.X,
                y: Math.Clamp(paddlePosition.Y, 0, tilemap.Size.height - PADDLE_HEIGHT));
        }
        void TryToScorePoint()
        {
            if (ballPosition.X < 0 == false &&
                ballPosition.X > tilemap.Size.width == false)
                return;

            ballPosition = center;
            ballAngle = (0f, 360f).Random();
        }
    }
}