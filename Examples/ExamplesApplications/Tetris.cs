namespace Pure.Examples.ExamplesApplications;

using Engine.Window;
using Engine.Utilities;
using Engine.Tilemap;
using Engine.Collision;

public static class Tetris
{
    public static void Run()
    {
        Window.Title = "Pure - Tetris Example";

        var (w, h) = Monitor.Current.AspectRatio;
        map = new((w * 3, h * 3));
        layer = new(map.Size);
        playArea = new(map.Size.width / 3, 0, map.Size.width / 3 - 1, map.Size.height - 1);
        var (ax, ay, aw, ah, _) = playArea.ToBundle();
        map.SetBox((ax, ay, aw + 1, ah + 1), Tile.EMPTY, Tile.BOX_CORNER_ROUND, Tile.FULL);

        tetromino = new((map.Size.width / 2, 0));

        Time.CallAfter(0.5f, () =>
        {
            var hasCollided = tetromino.MoveIn(Direction.Down);
            if (hasCollided == false)
                return;

            var ys = tetromino.GetBoxYs();
            ys.Sort();
            foreach (var y in ys)
            {
                var count = 0;
                for (var x = playArea.X + 1; x < playArea.X + playArea.Width; x++)
                    if (fallen.ContainsKey(((int)y, x)))
                        count++;
                    else
                        break;

                if (count != playArea.Width - 1)
                    continue;

                for (var x = playArea.X + 1; x < playArea.X + playArea.Width; x++)
                    fallen.Remove(((int)y, x));

                var boxesToDrop = new List<Box>();
                foreach (var kvp in fallen.Reverse())
                    if (kvp.Value.Position.Y < y)
                        boxesToDrop.Add(kvp.Value);

                foreach (var box in boxesToDrop)
                {
                    fallen.Remove(((int)box.Position.Y, (int)box.Position.X));
                    box.MoveIn(Direction.Down);
                    fallen[((int)box.Position.Y, (int)box.Position.X)] = box;
                }
            }

            tetromino = new((map.Size.width / 2, 0));
        }, true);
        Keyboard.Key.ArrowLeft.OnPressAndHold(() => tetromino.MoveIn(Direction.Left));
        Keyboard.Key.ArrowRight.OnPressAndHold(() => tetromino.MoveIn(Direction.Right));
        Keyboard.Key.ArrowDown.OnPressAndHold(() => tetromino.MoveIn(Direction.Down));
        Keyboard.Key.Space.OnPress(() => tetromino.Rotate());

        while (Window.KeepOpen())
        {
            Time.Update();

            tetromino.Draw();

            foreach (var kvp in fallen)
                kvp.Value.Draw();

            layer.DrawTilemap(map);
            layer.DrawCursor();
            layer.Draw();
        }
    }

#region Backend
    private static readonly SortedDictionary<(int y, int x), Box> fallen = new();
    private static Area playArea;
    private static Layer? layer;
    private static Tilemap? map;
    private static Tetromino? tetromino;

    private class Tetromino
    {
        public Tetromino(Point atPosition)
        {
            var (x, y) = atPosition.XY;
            var colors = new[]
            {
                Color.Cyan, Color.Yellow, Color.Purple, Color.Blue, Color.Orange, Color.Green, Color.Red
            };
            var positions = new[]
            {
                new Point[] { (x - 1, y + 0), (x + 0, y + 0), (x + 1, y + 0), (x + 2, y + 0) }, // I
                new Point[] { (x - 1, y + 0), (x + 0, y + 0), (x - 1, y + 1), (x + 0, y + 1) }, // O
                new Point[] { (x - 1, y + 0), (x + 0, y + 0), (x + 1, y + 0), (x + 0, y + 1) }, // T
                new Point[] { (x + 0, y - 1), (x + 0, y + 0), (x + 0, y + 1), (x - 1, y + 1) }, // J
                new Point[] { (x + 0, y - 1), (x + 0, y + 0), (x + 0, y + 1), (x + 1, y + 1) }, // L
                new Point[] { (x - 1, y + 0), (x + 0, y + 0), (x + 0, y - 1), (x + 1, y - 1) }, // S
                new Point[] { (x - 1, y + 0), (x + 0, y + 0), (x + 0, y + 1), (x + 1, y + 1) }, // Z
            };
            var randomType = (0, positions.Length - 1).Random();
            var structure = new Box[4];
            for (var i = 0; i < structure.Length; i++)
                structure[i] = new(new(Tile.SHAPE_SQUARE, colors[randomType]), positions[randomType][i]);

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
                if (Box.IsColliding(newPos))
                    return;

                newPositions.Add(newPos);
            }

            for (var i = 0; i < boxes.Length; i++)
                boxes[i].Position = newPositions[i];
        }
        public bool MoveIn(Direction direction)
        {
            if (isFrozen)
                return true;

            foreach (var box in boxes)
            {
                var newPos = box.Position.MoveIn(direction, 1);
                var hasCollided = Box.IsColliding(newPos);
                var hasFrozen = hasCollided && direction == Direction.Down;

                if (hasFrozen)
                {
                    isFrozen = true;
                    foreach (var b in boxes)
                        fallen[((int)b.Position.Y, (int)b.Position.X)] = b;

                    return true;
                }

                if (hasCollided)
                    return false;
            }

            foreach (var box in boxes)
                box.MoveIn(direction);

            return false;
        }
        public void Draw()
        {
            foreach (var box in boxes)
                box.Draw();
        }

#region Backend
        private bool isFrozen;
        private readonly Box[] boxes;
#endregion
    }

    private class Box
    {
        public Point Position { get; set; }

        public Box(Tile tile, Point position)
        {
            this.tile = tile;
            Position = position;
        }

        public void MoveIn(Direction direction)
        {
            Position = Position.MoveAt(direction, 1);
        }
        public static bool IsColliding(Point position)
        {
            var solid = new Solid(playArea.Position, playArea.Size);
            var isAtEdge = position.Y > 1 && solid.IsOverlapping(position) == false;
            var isAtFallen = fallen.ContainsKey(((int)position.Y, (int)position.X));

            return isAtEdge || isAtFallen;
        }
        public void Draw()
        {
            layer?.DrawTiles(Position, tile);
        }

#region Backend
        private readonly Tile tile;
#endregion
    }
#endregion
}