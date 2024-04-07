namespace Pure.Examples.ExamplesApplications;

using Engine.Animation;
using Engine.Collision;
using Engine.Tilemap;
using Engine.Utilities;
using Engine.Window;

public static class FlappyBird
{
    public static void Run()
    {
        Window.Title = "Pure - Flappy Bird Example";
        Window.IsRetro = true;
        Window.BackgroundColor = Window.IsRetro ? Color.Black : Color.Blue.ToDark();

        const int SCROLL_SPEED = 4, BIRD_X = 10, PIPE_WIDTH = 2, PIPE_HEIGHT = 20;
        var score = 0;
        var isGameOver = false;
        var ratio = Monitor.Current.AspectRatio;
        var background = new Tilemap((ratio.width * 3, ratio.height * 3));
        var foreground = new Tilemap(background.Size);
        var (width, height) = background.Size;

        var birdY = 5f;
        var birdVelocity = 0f;
        var birdAnimation = new Animation<(int, sbyte)>(0.8f, false,
            // pairs of (tile, angle)
            (Tile.ARROW_DIAGONAL, 0),
            (Tile.ARROW_DIAGONAL, 0), // first frame twice for a bit more upward time
            (Tile.ARROW, 0),
            (Tile.ARROW_DIAGONAL, 1)); // rotated 1 time (90 degrees clockwise)

        var pipes = new List<(float, int, int)>();
        var collisionMap = new SolidMap();
        var layer = new Layer(background.Size);

        collisionMap.SolidsAdd(Tile.BOX_DEFAULT_CORNER, new Solid(0, 0, 1, 1));
        collisionMap.SolidsAdd(Tile.BOX_DEFAULT_STRAIGHT, new Solid(0, 0, 1, 1));

        InitializePipes();

        Keyboard.Key.Space.OnPress(() =>
        {
            birdVelocity = -8f;
            birdAnimation.CurrentProgress = 0;

            if (isGameOver == false)
                return;

            birdY = 5f;
            birdVelocity = 0f;
            isGameOver = false;
            score = 0;
            InitializePipes();
        });

        while (Window.KeepOpen())
        {
            Time.Update();
            birdAnimation.Update(Time.Delta);

            background.Fill(null, new Tile(Tile.SHADE_OPAQUE, Color.Blue.ToDark()));
            foreground.Flush();

            UpdateBird();
            UpdatePipes();
            collisionMap.Update(foreground);
            isGameOver = IsGameOver();
            Draw();
        }

        void InitializePipes()
        {
            pipes = new()
            {
                // pairs of (x, y, holeSize) - some initial offscreen, later they will be reused
                (width + 00, (-15, -3).Random(), (3, 10).Random()),
                (width + 10, (-15, -3).Random(), (3, 10).Random()),
                (width + 20, (-15, -3).Random(), (3, 10).Random()),
                (width + 30, (-15, -3).Random(), (3, 10).Random()),
                (width + 40, (-15, -3).Random(), (3, 10).Random())
            };
        }
        void UpdatePipes()
        {
            for (var i = 0; i < pipes?.Count; i++)
            {
                var (pipeX, pipeY, holeSize) = pipes[i];

                if (isGameOver == false) // move pipe, unless game over
                    pipeX -= Time.Delta * SCROLL_SPEED;

                if (pipeX < -2) // "wrap" pipes around and reuse them
                {
                    pipeX = width + 2;
                    pipeY = (-15, -1).Random();
                    holeSize = (3, 10).Random();
                    score++;
                }

                pipes[i] = (pipeX, pipeY, holeSize); // update the pipe itself

                var lowerPipeY = pipeY + PIPE_HEIGHT + holeSize;
                background.SetArea(((int)pipeX, pipeY, PIPE_WIDTH, PIPE_HEIGHT), null,
                    new Tile(Tile.SHADE_OPAQUE, Color.Green.ToDark(0.8f)));
                background.SetArea(((int)pipeX, lowerPipeY, PIPE_WIDTH, PIPE_HEIGHT), null,
                    new Tile(Tile.SHADE_OPAQUE, Color.Green.ToDark(0.8f)));
                foreground.SetBox(((int)pipeX, pipeY, PIPE_WIDTH, PIPE_HEIGHT), Tile.SHADE_TRANSPARENT,
                    Tile.BOX_DEFAULT_CORNER,
                    Tile.BOX_DEFAULT_STRAIGHT, Color.Green);
                foreground.SetBox(((int)pipeX, lowerPipeY, PIPE_WIDTH, PIPE_HEIGHT),
                    Tile.SHADE_TRANSPARENT,
                    Tile.BOX_DEFAULT_CORNER,
                    Tile.BOX_DEFAULT_STRAIGHT, Color.Green);
            }
        }
        void UpdateBird()
        {
            if (isGameOver)
                return;

            // apply gravity
            birdVelocity += Time.Delta * 10f;
            birdY += birdVelocity * Time.Delta;

            // prevent jumping "over" pipes and going offscreen
            if (birdY > 0)
                return;

            birdY = 0;
            birdVelocity = 0;
        }
        bool IsGameOver()
        {
            var birdRect = new Solid(BIRD_X, birdY, 1, 1, Color.Red);
            return birdY + 1 >= height || collisionMap.IsOverlapping(birdRect);
        }
        void Draw()
        {
            var (birdTile, birdAngle) = birdAnimation.CurrentValue;

            var scoreText = $"Score: {score}";
            foreground.SetTextLine((width / 2 - scoreText.Length / 2, 1), scoreText);

            if (isGameOver)
                foreground.SetTextRectangle((0, 0, width, height),
                    $"Game Over!{Environment.NewLine}{Environment.NewLine}<Space> to play again",
                    alignment: Alignment.Center);

            layer.DrawTilemap(background.ToBundle());
            layer.DrawTilemap(foreground.ToBundle());
            var tile = new Tile(isGameOver ? Tile.UPPERCASE_X : birdTile,
                Color.Yellow, birdAngle);
            layer.DrawTiles((BIRD_X, birdY), tile);
            layer.DrawCursor();
            layer.Draw();
        }
    }
}