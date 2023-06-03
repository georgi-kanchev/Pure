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

using System.IO.Compression;
using System.Numerics;
using System.Runtime.InteropServices;

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

				var (nx, ny) = n.Position;
				var (ex, ey) = end.Position;
				var nPos = new Vector2((float)nx, (float)ny);
				var endPos = new Vector2((float)ex, (float)ey);

				n.distanceToTarget = Vector2.Distance(nPos, endPos);
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

	public void Load(string path)
	{
		var bytes = Decompress(File.ReadAllBytes(path));
		var offset = 0;

		var cellCount = BitConverter.ToInt32(GetBytesFrom(bytes, 4, ref offset));
		var w = BitConverter.ToInt32(GetBytesFrom(bytes, 4, ref offset));
		var h = BitConverter.ToInt32(GetBytesFrom(bytes, 4, ref offset));

		Size = (w, h);

	}
	public void Save(string path)
	{
		var (w, h) = Size;
		var bytes = new List<byte>();

		//for (int y = 0; y < h; y++)
		//	for (int x = 0; x < w; x++)
		//	{
		//		var node = grid[x, y];
		//		if (node.weight == 0 && node.isWalkable) // skip saving default cells
		//			continue;
		//		
		//		xs.Add(x);
		//		ys.Add(y);
		//		weights.Add(node.weight);
		//		solids.Add(node.isWalkable);
		//	}
		//
		//var bC = BitConverter.GetBytes(xs.Count);
		//var bW = BitConverter.GetBytes(w);
		//var bH = BitConverter.GetBytes(h);
		//var bXs = ToBytes(xs.ToArray());
		//var bYs = ToBytes(ys.ToArray());
		//var bWs = ToBytes(weights.ToArray());
		//var bSs = BoolsToBytes(solids);
		//
		//var result = new byte[bC.Length + bW.Length + bH.Length +
		//	bXs.Length + bYs.Length + bWs.Length + bSs.Length];
		//
		//Array.Copy(bC, 0, result, 0,
		//	bC.Length);
		//Array.Copy(bW, 0, result,
		//	bC.Length, bW.Length);
		//Array.Copy(bH, 0, result,
		//	bC.Length + bW.Length, bH.Length);
		//Array.Copy(bXs, 0, result,
		//	bC.Length + bW.Length + bH.Length, bXs.Length);
		//Array.Copy(bYs, 0, result,
		//	bC.Length + bW.Length + bH.Length + bXs.Length, bYs.Length);
		//Array.Copy(bWs, 0, result,
		//	bC.Length + bW.Length + bH.Length + bXs.Length + bYs.Length, bWs.Length);
		//Array.Copy(bSs, 0, result,
		//	bC.Length + bW.Length + bH.Length + bXs.Length + bYs.Length + bWs.Length, bSs.Length);
		//
		//File.WriteAllBytes(path, Compress(result));
	}
	#region Backend
	// save format
	// [amount of bytes]		- data
	// --------------------------------
	// [4]						- width
	// [4]						- height
	// [4]						- non-default cells count
	// [width * height * 4]		- xs
	// [width * height * 4]		- ys
	// [width * height * 4]		- weights
	// [remaining]				- is walkable bools (1 bit per bool)

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

	private static byte[] BoolsToBytes(List<bool> bools)
	{
		if (bools == null || bools.Count == 0)
			return Array.Empty<byte>();

		var result = new List<byte>();
		var bitCount = 0;
		var curByte = default(byte);
		for (int i = 0; i < bools.Count; i++)
		{
			var curBool = bools[i];
			var curBoolBit = (byte)(curBool ? 1 : 0);

			curByte <<= 1;
			curByte |= curBoolBit;

			bitCount++;

			if (bitCount == 8 || i == bools.Count - 1)
			{
				result.Add(curByte);
				curByte = 0;
				bitCount = 0;
			}
		}

		return result.ToArray();
	}
	private static bool[] BytesToBools(byte[] bytes, int boolCount)
	{
		if (bytes == null || bytes.Length == 0)
			return Array.Empty<bool>();

		var result = new List<bool>();
		var curBoolCount = 0;
		for (int i = 0; i < bytes.Length; i++)
		{
			var curByte = bytes[i];
			var curBools = new List<bool>();
			for (int j = 0; j < 8; j++)
			{
				curBoolCount++;

				var singleBit = curByte & 1;
				curByte >>= 1;

				curBools.Insert(0, singleBit == 1);

				if (curBoolCount == boolCount)
					break;
			}

			result.AddRange(curBools);
		}
		return result.ToArray();
	}

	private static byte[] Compress(byte[] data)
	{
		var output = new MemoryStream();
		using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
			stream.Write(data, 0, data.Length);

		return output.ToArray();
	}
	private static byte[] Decompress(byte[] data)
	{
		var input = new MemoryStream(data);
		var output = new MemoryStream();
		using (var stream = new DeflateStream(input, CompressionMode.Decompress))
			stream.CopyTo(output);

		return output.ToArray();
	}

	private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
	{
		var result = fromBytes[offset..(offset + amount)];
		offset += amount;
		return result;
	}

	#endregion
}