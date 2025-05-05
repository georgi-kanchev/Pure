using Pure.Engine.Collision;
using Pure.Engine.Hardware;
using Pure.Engine.Tiles;
using Pure.Engine.Utility;
using Pure.Engine.Window;

namespace Pure.Examples.Games;

public static class FlappyBird
{
	public static void Run()
	{
		var window = new Window
		{
			Title = "Pure - Flappy Bird Example",
			IsRetro = true,
			BackgroundColor = Color.Black
		};
		var hardware = new Hardware(window.Handle);

		const int SCROLL_SPEED = 4, BIRD_X = 10, PIPE_WIDTH = 2, PIPE_HEIGHT = 20;
		var (w, h) = hardware.Monitors[0].AspectRatio;
		var layer = new LayerTiles((w * 3, h * 3));
		var (background, foreground) = (new TileMap(layer.Size), new TileMap(layer.Size));
		var (birdY, birdVelocity, score, isGameOver) = (5f, 0f, 0, false);
		var birdAnimation = new[]
		{
			new Tile(Tile.ARROW_DIAGONAL, Color.Yellow),
			new Tile(Tile.ARROW_DIAGONAL, Color.Yellow), // first frame twice for a bit more upward time
			new Tile(Tile.ARROW, Color.Yellow),
			new Tile(Tile.ARROW_DIAGONAL, Color.Yellow, Pose.Right) // rotated 1 time (90 degrees clockwise)
		};
		var defaultPipes = new List<(float x, int y, int holeSize)>
		{
			(layer.Size.width + 00, (-15, -3).Random(), (3, 10).Random()),
			(layer.Size.width + 10, (-15, -3).Random(), (3, 10).Random()),
			(layer.Size.width + 20, (-15, -3).Random(), (3, 10).Random()),
			(layer.Size.width + 30, (-15, -3).Random(), (3, 10).Random()),
			(layer.Size.width + 40, (-15, -3).Random(), (3, 10).Random())
		};
		var pipes = defaultPipes.ToList();
		var collisionMap = new SolidMap();

		collisionMap.AddSolids(Tile.PIPE_SOLID_CORNER, [new(0, 0, 1, 1)]);
		collisionMap.AddSolids(Tile.PIPE_SOLID_STRAIGHT, [new(0, 0, 1, 1)]);

		// flapping ====================================================
		hardware.Keyboard.OnPress(Keyboard.Key.Space, () =>
		{
			birdVelocity = -8f;
			birdAnimation = birdAnimation.ToArray(); // resets the animation

			if (isGameOver == false)
				return; // disabled during game over screen

			birdY = 5f;
			birdVelocity = 0f;
			isGameOver = false;
			score = 0;
			pipes = defaultPipes.ToList();
		});

		while (window.KeepOpen())
		{
			Time.Update();

			// bird ====================================================
			isGameOver = birdY + 1 >= layer.Size.height || collisionMap.IsOverlapping(new Solid(BIRD_X, birdY, 1, 1));

			if (isGameOver == false)
			{
				birdVelocity += Time.Delta * 10f; //    apply gravity
				birdY += birdVelocity * Time.Delta; //  apply velocity

				if (birdY <= 0) // prevent jumping "over" pipes and going offscreen on the top
				{
					birdY = 0;
					birdVelocity = 0;
				}
			}

			// pipes ====================================================
			background.Fill([new(Tile.FULL, Color.Blue.ToDark())]);
			foreground.Flush();

			for (var i = 0; i < pipes?.Count; i++)
			{
				var (pipeX, pipeY, holeSize) = pipes[i];

				if (isGameOver == false) // move pipe, unless game over
					pipeX -= Time.Delta * SCROLL_SPEED;

				if (pipeX < -2) // "wrap" pipes around and reuse them
				{
					pipeX = layer.Size.width + 2;
					pipeY = (-15, -1).Random();
					holeSize = (3, 10).Random();
					score++;
				}

				pipes[i] = (pipeX, pipeY, holeSize); // update the pipe itself

				var lowerPipeY = pipeY + PIPE_HEIGHT + holeSize;
				background.SetArea(((int)pipeX, pipeY, PIPE_WIDTH, PIPE_HEIGHT), [new(Tile.FULL, Color.Green.ToDark(0.8f))]);
				background.SetArea(((int)pipeX, lowerPipeY, PIPE_WIDTH, PIPE_HEIGHT), [new(Tile.FULL, Color.Green.ToDark(0.8f))]);
				foreground.SetBox(((int)pipeX, pipeY, PIPE_WIDTH, PIPE_HEIGHT), Tile.EMPTY, new(Tile.PIPE_SOLID_CORNER, Color.Green),
					new(Tile.PIPE_SOLID_STRAIGHT, Color.Green));
				foreground.SetBox(((int)pipeX, lowerPipeY, PIPE_WIDTH, PIPE_HEIGHT), Tile.EMPTY,
					new(Tile.PIPE_SOLID_CORNER, Color.Green),
					new(Tile.PIPE_SOLID_STRAIGHT, Color.Green));
			}

			collisionMap.Update(foreground);

			// rendering ====================================================
			if (isGameOver)
				foreground.SetText((0, 0), "Game Over!\n\n" +
				                           "<Space> to play again".Constrain(layer.Size, alignment: Alignment.Center));

			var scoreText = $"Score: {score}";
			foreground.SetText((layer.Size.width / 2 - scoreText.Length / 2, 1), scoreText);
			layer.DrawTileMap(background.ToBundle());
			layer.DrawTileMap(foreground.ToBundle());
			layer.DrawTiles((BIRD_X, birdY), isGameOver ? new(Tile.UPPERCASE_X) : birdAnimation.Animate(3f, false));
			layer.DrawMouseCursor(window, hardware.Mouse.CursorPosition, (int)hardware.Mouse.CursorCurrent);
			layer.Render(window);
		}
	}
}