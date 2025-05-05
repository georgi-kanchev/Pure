using Pure.Engine.Execution;
using Pure.Engine.Window;
using Pure.Engine.Utility;
using Pure.Engine.Tiles;
using Pure.Engine.Collision;
using Pure.Engine.Hardware;

namespace Pure.Examples.Games;

public static class Tetris
{
	public static void Run()
	{
		var window = new Window { Title = "Pure - Tetris Example" };
		var hardware = new Hardware(window.Handle);
		var (w, h) = hardware.Monitors[0].AspectRatio;
		var fallen = new SortedDictionary<(int y, int x), Box>();
		var map = new TileMap((w * 3, h * 3));
		var layer = new LayerTiles(map.Size);
		var playArea = new Area(map.Size.width / 3, 0, map.Size.width / 3 - 1, map.Size.height - 1);
		var (ax, ay, aw, ah) = playArea.ToBundle();
		map.SetBox((ax, ay, aw + 1, ah + 1), Tile.EMPTY, Tile.BOX_CORNER, Tile.FULL);
		var piece = new Piece((map.Size.width / 2, 0), fallen, playArea);

		// game flow ====================================================
		Flow.CallEvery(0.5f, () =>
		{
			if (piece.TryMoveAt(Angle.Down)) // falling & colliding
				return;

			// clearing line ====================================================
			var ys = piece.GetBoxYs();
			ys.Sort();

			foreach (var y in ys)
			{
				var count = 0;
				for (var x = playArea.X + 1; x < playArea.X + playArea.Width; x++)
					if (fallen.ContainsKey(((int)y, x)))
						count++;
					else
						break;

				if (count != playArea.Width - 1) // is line full?
					continue;

				for (var x = playArea.X + 1; x < playArea.X + playArea.Width; x++)
					fallen.Remove(((int)y, x)); // remove line

				var boxesToDrop = new List<Box>();
				foreach (var kvp in fallen.Reverse())
					if (kvp.Value.Position.Y < y) // gather all boxes above line
						boxesToDrop.Add(kvp.Value);

				foreach (var box in boxesToDrop)
				{
					fallen.Remove(((int)box.Position.Y, (int)box.Position.X));
					box.Position = box.Position.MoveAt(Angle.Down, 1); // drop all boxes gathered & update fallen
					fallen[((int)box.Position.Y, (int)box.Position.X)] = box;
				}
			}

			piece = new((map.Size.width / 2, 0), fallen, playArea);
		});

		hardware.Keyboard.OnPressAndHold(Keyboard.Key.ArrowLeft, () => piece.TryMoveAt(Angle.Left));
		hardware.Keyboard.OnPressAndHold(Keyboard.Key.ArrowRight, () => piece.TryMoveAt(Angle.Right));
		hardware.Keyboard.OnPressAndHold(Keyboard.Key.ArrowDown, () => piece.TryMoveAt(Angle.Down));
		hardware.Keyboard.OnPress(Keyboard.Key.Space, () => piece.Rotate());

		while (window.KeepOpen())
		{
			Time.Update();
			Flow.Update(Time.Delta);

			// rendering ====================================================
			foreach (var (_, box) in fallen)
				layer.DrawTiles(box.Position, box.Tile);

			piece.Draw(layer);
			layer.DrawTileMap(map);
			layer.DrawMouseCursor(window, hardware.Mouse.CursorPosition, (int)hardware.Mouse.CursorCurrent);
			layer.Render(window);
		}
	}

#region Backend
	private class Piece
	{
		public Piece(Point atPosition, SortedDictionary<(int y, int x), Box> fallen, Area playArea)
		{
			this.fallen = fallen;
			var (x, y) = atPosition.XY;
			var colors = new[] { Color.Cyan, Color.Yellow, Color.Purple, Color.Blue, Color.Orange, Color.Green, Color.Red };
			var positions = new[]
			{
				[(x - 1, y + 0), (x + 0, y + 0), (x + 1, y + 0), (x + 2, y + 0)], // I
				[(x - 1, y + 0), (x + 0, y + 0), (x - 1, y + 1), (x + 0, y + 1)], // O
				[(x - 1, y + 0), (x + 0, y + 0), (x + 1, y + 0), (x + 0, y + 1)], // T
				[(x + 0, y - 1), (x + 0, y + 0), (x + 0, y + 1), (x - 1, y + 1)], // J
				[(x + 0, y - 1), (x + 0, y + 0), (x + 0, y + 1), (x + 1, y + 1)], // L
				[(x - 1, y + 0), (x + 0, y + 0), (x + 0, y - 1), (x + 1, y - 1)], // S
				new Point[] { (x - 1, y + 0), (x + 0, y + 0), (x + 0, y + 1), (x + 1, y + 1) } // Z
			};
			var randomType = (0, positions.Length - 1).Random();
			var structure = new Box[4];

			for (var i = 0; i < structure.Length; i++)
				structure[i] = new(new(Tile.SHAPE_SQUARE, colors[randomType]), positions[randomType][i], fallen, playArea);

			boxes = structure;
		}

		public List<float> GetBoxYs()
		{
			var result = new List<float>();
			foreach (var box in boxes)
				if (result.Contains(box.Position.Y) == false)
					result.Add(box.Position.Y);
			return result;
		}
		public void Rotate()
		{
			var (cx, cy) = boxes[1].Position.XY;
			var newPositions = new List<Point>();

			foreach (var box in boxes)
			{
				var newPos = new Point(box.Position.Y - cy + cx, -(box.Position.X - cx) + cy);
				if (box.IsColliding(newPos))
					return; // prevent rotation when the new piece orientation is colliding

				newPositions.Add(newPos);
			}

			for (var i = 0; i < boxes.Length; i++)
				boxes[i].Position = newPositions[i];
		}
		public bool TryMoveAt(Angle angle)
		{
			if (isFrozen)
				return false;

			foreach (var box in boxes)
			{
				var newPos = box.Position.MoveIn(angle, 1);
				var hasCollided = box.IsColliding(newPos);
				var hasFrozen = hasCollided && angle == Angle.Down;

				if (hasFrozen)
				{
					isFrozen = true; // cannot fall anymore - freeze
					foreach (var b in boxes)
						fallen[((int)b.Position.Y, (int)b.Position.X)] = b;

					return false;
				}

				if (hasCollided)
					return true;
			}

			foreach (var box in boxes) // this piece is able to fall because it passed the previous freeze check
				box.Position = box.Position.MoveAt(angle, 1);

			return true;
		}
		public void Draw(LayerTiles layer)
		{
			foreach (var box in boxes)
				layer.DrawTiles(box.Position, box.Tile);
		}

		private bool isFrozen;
		private readonly Box[] boxes;
		private readonly SortedDictionary<(int y, int x), Box> fallen;
	}

	private class Box(Tile tile, Point position, SortedDictionary<(int y, int x), Box> fallen, Area playArea)
	{
		public Tile Tile { get; } = tile;
		public Point Position { get; set; } = position;

		public bool IsColliding(Point position)
		{
			var solid = new Solid(playArea.Position, playArea.Size);
			var isAtEdge = position.Y > 1 && solid.IsOverlapping(position.XY) == false;
			var isAtFallen = fallen.ContainsKey(((int)position.Y, (int)position.X));

			return isAtEdge || isAtFallen;
		}

		private readonly Area playArea = playArea;
	}
#endregion
}