namespace Pure.Examples.Games;

using Engine.Collision;
using Engine.Tiles;
using Engine.Utility;
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
        var background = new TileMap((ratio.width * 3, ratio.height * 3));
        var foreground = new TileMap(background.Size);
        var (width, height) = background.Size;

        var birdY = 5f;
        var birdVelocity = 0f;
        var birdAnimation = new[]
        {
            new Tile(Tile.ARROW_DIAGONAL, Color.Yellow),
            new Tile(Tile.ARROW_DIAGONAL, Color.Yellow), // first frame twice for a bit more upward time
            new Tile(Tile.ARROW, Color.Yellow),
            new Tile(Tile.ARROW_DIAGONAL, Color.Yellow, Pose.Right) // rotated 1 time (90 degrees clockwise)
        };
        var pipes = new List<(float, int, int)>();
        var collisionMap = new SolidMap();
        var layer = new LayerTiles(background.Size);

        collisionMap.AddSolids(Tile.PIPE_SOLID_CORNER, new Solid(0, 0, 1, 1));
        collisionMap.AddSolids(Tile.PIPE_SOLID_STRAIGHT, new Solid(0, 0, 1, 1));

        InitializePipes();

        layer.BackgroundColor = Color.Black;

        Keyboard.Key.Space.OnPress(() =>
        {
            birdVelocity = -8f;
            birdAnimation = birdAnimation.ToArray(); // reset the animation

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

            background.Fill(new Tile(Tile.FULL, Color.Blue.ToDark()));
            foreground.Flush();

            UpdateBird();
            UpdatePipes();
            collisionMap.Update(foreground);
            isGameOver = IsGameOver();
            Draw();
        }

        void InitializePipes()
        {
            pipes =
            [
                (width + 00, (-15, -3).Random(), (3, 10).Random()),
                (width + 10, (-15, -3).Random(), (3, 10).Random()),
                (width + 20, (-15, -3).Random(), (3, 10).Random()),
                (width + 30, (-15, -3).Random(), (3, 10).Random()),
                (width + 40, (-15, -3).Random(), (3, 10).Random())
            ];
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
                background.SetArea(((int)pipeX, pipeY, PIPE_WIDTH, PIPE_HEIGHT),
                    new Tile(Tile.FULL, Color.Green.ToDark(0.8f)));
                background.SetArea(((int)pipeX, lowerPipeY, PIPE_WIDTH, PIPE_HEIGHT),
                    new Tile(Tile.FULL, Color.Green.ToDark(0.8f)));
                foreground.SetBox(((int)pipeX, pipeY, PIPE_WIDTH, PIPE_HEIGHT), Tile.EMPTY,
                    new(Tile.PIPE_SOLID_CORNER, Color.Green),
                    new(Tile.PIPE_SOLID_STRAIGHT, Color.Green));
                foreground.SetBox(((int)pipeX, lowerPipeY, PIPE_WIDTH, PIPE_HEIGHT),
                    Tile.EMPTY,
                    new(Tile.PIPE_SOLID_CORNER, Color.Green),
                    new(Tile.PIPE_SOLID_STRAIGHT, Color.Green));
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
            var birdTile = birdAnimation.Animate(3f, false);

            var scoreText = $"Score: {score}";
            foreground.SetText((width / 2 - scoreText.Length / 2, 1), scoreText);

            var text = $"Game Over!\n\n<Space> to play again"
                .Constrain((width, height), alignment: Alignment.Center);
            if (isGameOver)
                foreground.SetText((0, 0), text);

            layer.DrawTileMap(background.ToBundle());
            layer.DrawTileMap(foreground.ToBundle());
            var tile = isGameOver ? new(Tile.UPPERCASE_X) : birdTile;
            layer.DrawTiles((BIRD_X, birdY), tile);
            layer.DrawMouseCursor();
            layer.Render();
        }
    }
}