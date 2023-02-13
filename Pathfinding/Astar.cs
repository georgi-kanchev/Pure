// credit: https://github.com/davecusatis/A-Star-Sharp
//
// MIT License
// 
// Copyright (c) 2021 davecusatis
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace Pure.Pathfinding;

internal class Node
{
	public Node? Parent;
	public (int, int) Position;
	public float distanceToTarget, cost;
	public int weight;
	public float F => distanceToTarget == -1 || cost == -1 ? -1 : distanceToTarget + cost;
	public bool isWalkable;

	public Node((int, int) pos, int weight, bool walkable)
	{
		Parent = null;
		Position = pos;
		distanceToTarget = -1;
		cost = 1;
		this.weight = weight;
		isWalkable = walkable;
	}
	public override string ToString()
	{
		return $"{Position} {isWalkable}";
	}
}

internal class Astar
{
	public (int, int) Size
	{
		get => (grid.GetLength(0), grid.GetLength(1));
		set => grid = ResizeArray(grid, value);
	}

	public void SetNode((int, int) pos, int weight, bool isWalkable)
	{
		if (HasPosition(pos) == false)
			return;

		var n = grid[pos.Item1, pos.Item2];
		n.Position = pos;
		n.weight = weight;
		n.isWalkable = isWalkable;
	}
	public Node? GetNode((int, int) pos)
	{
		return HasPosition(pos) ? grid[pos.Item1, pos.Item2] : null;
	}
	public (int, int)[] FindPath((int, int) a, (int, int) b)
	{
		var start = new Node(a, 0, true);
		var end = new Node(b, 0, true);
		var path = new Stack<Node>();
		var open = new PriorityQueue<Node, float>();
		var closed = new List<Node>();
		var neighbours = new List<Node>();
		var current = start;
		var maxIter = Size.Item1 * Size.Item2;

		open.Enqueue(start, start.F);

		for (int i = 0; i < maxIter; i++)
		{
			if (open.Count == 0)
				break;

			current = open.Dequeue();
			closed.Add(current);

			if (current.Position == b)
				break;

			neighbours = GetAdjacentNodes(current);

			foreach (var n in neighbours)
			{
				if (closed.Contains(n) || n.isWalkable == false || Contains(open, n))
					continue;

				n.Parent = current;

				var x = Math.Abs(n.Position.Item1 - end.Position.Item1);
				var y = Math.Abs(n.Position.Item2 - end.Position.Item2);

				n.distanceToTarget = x + y;
				n.cost = n.weight + n.Parent.cost;
				open.Enqueue(n, n.F);
			}
		}

		if (closed.Exists(x => x.Position == end.Position) == false)
			return Array.Empty<(int, int)>();

		var temp = closed[closed.IndexOf(current)];
		if (temp == null)
			return Array.Empty<(int, int)>();

		var result = new List<(int, int)>() { start.Position };
		for (int i = closed.Count - 1; i >= 0; i--)
		{
			if (temp == null || temp == start)
				break;

			result.Add(temp.Position);
			temp = temp.Parent;
		}
		return result.ToArray();
	}

	#region Backend
	private Node[,] grid = new Node[0, 0];

	private bool Contains(PriorityQueue<Node, float> prioQueue, Node item)
	{
		foreach (var i in prioQueue.UnorderedItems)
			if (i.Element == item)
				return true;

		return false;
	}
	private bool HasPosition((int, int) pos)
	{
		var (x, y) = pos;
		return
			x >= 0 && x < Size.Item1 &&
			y >= 0 && y < Size.Item2;
	}
	private List<Node> GetAdjacentNodes(Node n)
	{
		var result = new List<Node>();

		TryAdd(-1, 0);
		TryAdd(1, 0);
		TryAdd(0, -1);
		TryAdd(0, 1);

		return result;

		void TryAdd(int offX, int offY)
		{
			var (x, y) = n.Position;
			var p = (x + offX, y + offY);
			var node = GetNode(p);

			if (node != null)
				result.Add(node);
		}
	}
	private Node[,] ResizeArray(Node[,] original, (int, int) size)
	{
		var (rows, cols) = size;
		var result = new Node[rows, cols];

		for (int i = 0; i < rows; i++)
			for (int j = 0; j < cols; j++)
			{
				var o = GetNode((i, j));
				result[i, j] = o == null ? new((i, j), 0, true) : o;
			}
		return result;
	}
	#endregion
}