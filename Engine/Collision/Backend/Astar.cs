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

namespace Pure.Engine.Collision;

using System.Numerics;

internal class Node((int x, int y) pos, int penalty)
{
    public (int x, int y) position = pos;
    public float penalty = penalty;

    [PathMap.DoNotSave]
    public Node? parent;
    [PathMap.DoNotSave]
    public float distanceToTarget = -1, cost = 1;
    [PathMap.DoNotSave]
    public float F
    {
        get => MathF.Abs(distanceToTarget + 1) < 0.1f || MathF.Abs(cost + 1) < 0.1f ?
            -1 :
            distanceToTarget + cost;
    }

    public override string ToString()
    {
        return $"{position} | {nameof(penalty)}: {penalty}";
    }
}

internal class Astar
{
    public int NodeCount
    {
        get => grid.Count;
    }
    public (int width, int height) Size
    {
        get => size;
        set => size = (Math.Max(value.width, 1), Math.Max(value.height, 1));
    }

    public void SetNode((int x, int y) pos, float penalty)
    {
        if (IsInside(pos) == false)
            return;

        if (grid.ContainsKey(pos) == false)
            grid.Add(pos, new(pos, 0));

        grid[pos].position = pos;
        grid[pos].penalty = penalty;
    }
    public Node? GetNode((int x, int y) pos)
    {
        return IsInside(pos) && grid.TryGetValue(pos, out var value) ? value : null;
    }
    public (float x, float y)[] FindPath((float x, float y) a, (float x, float y) b, bool includeColors, out (float x, float y, uint color)[] withColors, int maxZigzag, uint color)
    {
        a = ((int)a.x, (int)a.y);
        b = ((int)b.x, (int)b.y);

        var start = new Node(((int)a.x, (int)a.y), 0);
        var end = new Node(((int)b.x, (int)b.y), 0);
        var open = new PriorityQueue<Node, float>();
        var closed = new List<Node>();
        var current = start;
        var max = Size.width * Size.height;

        open.Enqueue(start, start.F);

        var endNode = GetNode(((int)b.x, (int)b.y));
        if (endNode == null || float.IsPositiveInfinity(endNode.penalty))
            max = 0;

        for (var i = 0; i < max; i++)
        {
            if (open.Count == 0)
                break;

            current = open.Dequeue();
            closed.Add(current);

            if (current.position == b)
                break;

            var neighbours = GetAdjacentNodes(current);

            foreach (var n in neighbours)
            {
                if (Contains(open, n) ||
                    closed.Contains(n) ||
                    float.IsInfinity(n.penalty) ||
                    float.IsNaN(n.penalty))
                    continue;

                n.parent = current;

                var (nx, ny) = n.position;
                var (ex, ey) = end.position;

                n.distanceToTarget = Vector2.Distance(new(nx, ny), new(ex, ey));
                n.cost = n.penalty + n.parent.cost;
                open.Enqueue(n, n.F);
            }
        }

        if (closed.Exists(x => x.position == end.position) == false)
        {
            withColors = [];
            return [];
        }

        var index = closed.IndexOf(current);
        if (index == -1)
        {
            withColors = [];
            return [];
        }

        var temp = closed[index];
        var result = new List<(float x, float y)> { (start.position.x + 0.5f, start.position.y + 0.5f) };
        var resultWithColors = new List<(float x, float y, uint color)>
        {
            (start.position.x + 0.5f, start.position.y + 0.5f, color)
        };
        for (var i = closed.Count - 1; i >= 0; i--)
        {
            if (temp == null || temp == start)
                break;

            result.Insert(1, (temp.position.x + 0.5f, temp.position.y + 0.5f));

            if (includeColors)
                resultWithColors.Insert(1, (temp.position.x + 0.5f, temp.position.y + 0.5f, color));

            temp = temp.parent;
        }

        SmoothZigzag(result, resultWithColors, maxZigzag);
        RemoveRedundantPoints(result, resultWithColors);

        withColors = includeColors ?
            resultWithColors.ToArray() :
            [];
        return result.ToArray();
    }

#region Backend
    internal readonly Dictionary<(int x, int y), Node> grid = new();
    private (int width, int height) size;

    private static bool Contains(PriorityQueue<Node, float> queue, Node item)
    {
        foreach (var i in queue.UnorderedItems)
            if (i.Element == item)
                return true;

        return false;
    }
    private bool IsInside((int x, int y) pos)
    {
        return pos.x >= 0 &&
               pos.x < Size.width &&
               pos.y >= 0 &&
               pos.y < Size.height;
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
            var (x, y) = n.position;
            var node = GetNode((x + offX, y + offY));

            if (node != null)
                result.Add(node);
        }
    }

    private static void SmoothZigzag(List<(float x, float y)> points, List<(float x, float y, uint color)> withColor, int maxZigzag)
    {
        if (points.Count < 2)
            return;

        for (var i = 1; i < points.Count - 1; i++)
        {
            var horDiff = Math.Abs(points[i - 1].x - points[i + 1].x);
            var verDiff = Math.Abs(points[i - 1].y - points[i + 1].y);

            if (horDiff > maxZigzag || verDiff > maxZigzag)
                continue;

            points.RemoveAt(i);
            withColor.RemoveAt(i);
            i--;
        }
    }
    private static void RemoveRedundantPoints(List<(float x, float y)> points, List<(float x, float y, uint color)> withColor)
    {
        if (points.Count < 3)
            return;

        for (var i = 1; i < points.Count - 1; i++)
            if (IsLine(points[i - 1], points[i], points[i + 1]))
            {
                points.RemoveAt(i);
                withColor.RemoveAt(i);
                i--;
            }
    }
    private static bool IsLine((float x, float y) a, (float x, float y) b, (float x, float y) c)
    {
        return Math.Abs((a.x - b.x) * (b.y - c.y) - (b.x - c.x) * (a.y - b.y)) < 0.01f;
    }
#endregion
}