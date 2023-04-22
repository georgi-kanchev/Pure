namespace Pure.Examples.Games;

using Pure.Window;
using Pure.Tilemap;
using Pure.Utilities;
using Pure.Animation;
using Pure.Tracker;
using Pure.Collision;

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
		collisionMap.AddRectangle(new((1, 1)), Tile.BORDER_DEFAULT_CORNER);
		collisionMap.AddRectangle(new((1, 1)), Tile.BORDER_DEFAULT_STRAIGHT);

		InitializePipes();

		Window.Create(Window.State.Windowed);
		Window.IsRetro = true;

		while (Window.IsOpen) // the default game loop
		{
			Window.Activate(true);

			// track the spacebar
			Tracker<string>.Track("space-down", Keyboard.IsKeyPressed(Keyboard.Key.SPACE));
			Tracker<string>.When("space-down", () =>
			{
				birdVelocity = -0.006f;
				birdAnimation.CurrentProgress = 0;

				if (isGameOver) // restart game in case it's over
				{
					birdY = 5f;
					birdVelocity = 0f;
					isGameOver = false;
					score = 0;
					InitializePipes();
				}
			});

			// update some of the systems
			Time.Update();
			birdAnimation.Update(Time.Delta);

			// clear the tilemaps from the previous frame
			background.Fill(new(Tile.SHADE_OPAQUE, ((Color)Color.Blue).ToDark()));
			foreground.Fill();

			// apply gravity, unless game over
			if (isGameOver == false)
			{
				birdVelocity += Time.Delta * 0.01f;
				birdY += birdVelocity;
			}

			// prevent jumping "over" pipes and going offscreen
			if (birdY < 0)
			{
				birdY = 0;
				birdVelocity = 0;
			}

			// update pipes
			for (int i = 0; i < pipes?.Count; i++)
			{
				var (pipeX, pipeY, holeSize) = pipes[i];

				if (isGameOver == false) // move pipe, unless game over
					pipeX -= Time.Delta * SCROLL_SPEED;

				if (pipeX < -2) // "wrap" pipes around and reuse them
				{
					pipeX = width + 2;
					pipeY = (-15).Random(-1);
					holeSize = 3.Random(10);
					score++;
				}

				pipes[i] = (pipeX, pipeY, holeSize); // update the pipe itself

				var size = (PIPE_WIDTH, PIPE_HEIGHT);
				var lowerPipeY = pipeY + PIPE_HEIGHT + holeSize;
				background.SetSquare(((int)pipeX, pipeY), size, new(Tile.SHADE_OPAQUE, ((Color)Color.Green).ToDark(0.8f)));
				background.SetSquare(((int)pipeX, lowerPipeY), size, new(Tile.SHADE_OPAQUE, ((Color)Color.Green).ToDark(0.8f)));
				foreground.SetBorder(((int)pipeX, pipeY), size, Tile.BORDER_DEFAULT_CORNER, Tile.BORDER_DEFAULT_STRAIGHT, (Color)Color.Green);
				foreground.SetBorder(((int)pipeX, lowerPipeY), size, Tile.BORDER_DEFAULT_CORNER, Tile.BORDER_DEFAULT_STRAIGHT, (Color)Color.Green);
			}

			collisionMap.Update(foreground.IDs);

			// whether the bird fell out of the map or bonked into a pipe
			var birdRect = new Rectangle((1, 1), (BIRD_X, birdY));
			if (birdY + 1 >= height || collisionMap.IsOverlapping(birdRect))
				isGameOver = true;

			// finish by drawing everything
			var (birdTile, birdAngle) = birdAnimation.CurrentValue;

			var scoreText = $"Score: {score}";
			foreground.SetTextLine((width / 2 - scoreText.Length / 2, 1), scoreText);

			if (isGameOver)
				foreground.SetTextLine((width / 2 - GAME_OVER.Length / 2, height / 2), GAME_OVER);

			Window.DrawBundleTiles(background.ToBundle());
			Window.DrawBundleTiles(foreground.ToBundle());
			Window.DrawBasicTile((BIRD_X, birdY), isGameOver ? Tile.CAPITAL_X : birdTile, Color.Yellow, birdAngle);

			Window.Activate(false);
		}

		void InitializePipes()
		{
			pipes = new(){
			// pairs of (x, y, holeSize) - some initial offscreen, later they will be reused
				(width + 00, (-15).Random(-3), 3.Random(10)),
				(width + 10, (-15).Random(-3), 3.Random(10)),
				(width + 20, (-15).Random(-3), 3.Random(10)),
				(width + 30, (-15).Random(-3), 3.Random(10)),
				(width + 40, (-15).Random(-3), 3.Random(10)),
			};
		}
	}
}