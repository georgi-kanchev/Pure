namespace Pure.Examples.ExamplesGames;

using Engine.Animation;
using Engine.Collision;
using Engine.Tilemap;
using Engine.Utilities;
using Engine.Window;

public static class FlappyBird
{
    public static void Run()
    {
        // some data needed throughout the game
        const int SCROLL_SPEED = 4, BIRD_X = 10, PIPE_WIDTH = 2, PIPE_HEIGHT = 20;
        const string GAME_OVER = "Game Over! <Space> to play again.";
        var score = 0;
        var isGameOver = false;
        var background = new Tilemap((16 * 3, 9 * 3)); // 16:9 aspect ratio times 3
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
        var collisionMap = new Map();
        collisionMap.AddRectangle(new((1, 1)), Tile.BOX_DEFAULT_CORNER);
        collisionMap.AddRectangle(new((1, 1)), Tile.BOX_DEFAULT_STRAIGHT);

        Window.Create();
        //Window.IsRetro = true;

        InitializePipes();

        Keyboard.OnKeyPress(Keyboard.Key.Space, asText =>
        {
            birdVelocity = -0.08f;
            birdAnimation.CurrentProgress = 0;

            if (isGameOver == false)
                return; // restart game in case it's over

            birdY = 5f;
            birdVelocity = 0f;
            isGameOver = false;
            score = 0;
            InitializePipes();
        });

        var layer = new Layer();
        while (Window.IsOpen) // the default game loop
        {
            Window.Activate(true);

            // update some of the systems
            Time.Update();
            birdAnimation.Update(Time.Delta);

            // clear the tilemaps from the previous frame
            background.Fill(new(Tile.SHADE_OPAQUE, Color.Blue.ToDark()));
            foreground.Flush();

            // apply gravity, unless game over
            if (isGameOver == false)
            {
                birdVelocity += Time.Delta * 0.1f;
                birdY += birdVelocity;
            }

            // prevent jumping "over" pipes and going offscreen
            if (birdY < 0)
            {
                birdY = 0;
                birdVelocity = 0;
            }

            // update pipes
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

                var size = (PIPE_WIDTH, PIPE_HEIGHT);
                var lowerPipeY = pipeY + PIPE_HEIGHT + holeSize;
                background.SetRectangle(((int)pipeX, pipeY), size,
                    new(Tile.SHADE_OPAQUE, Color.Green.ToDark(0.8f)));
                background.SetRectangle(((int)pipeX, lowerPipeY), size,
                    new(Tile.SHADE_OPAQUE, Color.Green.ToDark(0.8f)));
                foreground.SetBox(((int)pipeX, pipeY), size, Tile.SHADE_TRANSPARENT,
                    Tile.BOX_DEFAULT_CORNER,
                    Tile.BOX_DEFAULT_STRAIGHT, Color.Green);
                foreground.SetBox(((int)pipeX, lowerPipeY), size, Tile.SHADE_TRANSPARENT,
                    Tile.BOX_DEFAULT_CORNER,
                    Tile.BOX_DEFAULT_STRAIGHT, Color.Green);
            }

            collisionMap.Update(foreground);

            // whether the bird fell out of the map or bonked into a pipe
            var birdRect = new Rectangle((1, 1), (BIRD_X, birdY), Color.Red);
            if (birdY + 1 >= height || collisionMap.IsOverlapping(birdRect))
                isGameOver = true;

            // finish by drawing everything
            var (birdTile, birdAngle) = birdAnimation.CurrentValue;

            var scoreText = $"Score: {score}";
            foreground.SetTextLine((width / 2 - scoreText.Length / 2, 1), scoreText);

            if (isGameOver)
                foreground.SetTextLine((width / 2 - GAME_OVER.Length / 2, height / 2), GAME_OVER);

            layer.DrawTilemap(background.ToBundle());
            layer.DrawTilemap(foreground.ToBundle());
            var tile = new Tile(isGameOver ? Tile.UPPERCASE_X : birdTile, Color.Yellow, birdAngle);
            layer.DrawTile((BIRD_X, birdY), tile);

            //Window.DrawRectangles(collisionMap);
            //Window.DrawRectangles(birdRect);

            Window.Activate(false);
        }

        return;

        void InitializePipes()
        {
            pipes = new()
            {
                // pairs of (x, y, holeSize) - some initial offscreen, later they will be reused
                (width + 00, (-15, -3).Random(), (3, 10).Random()),
                (width + 10, (-15, -3).Random(), (3, 10).Random()),
                (width + 20, (-15, -3).Random(), (3, 10).Random()),
                (width + 30, (-15, -3).Random(), (3, 10).Random()),
                (width + 40, (-15, -3).Random(), (3, 10).Random()),
            };
        }
    }
}