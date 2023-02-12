namespace Pure.Pathfinding;

public class Map
{
	public (int, int) Size
	{
		get => pathfind.size;
		set => pathfind.size = value;
	}

	public Map((int, int) size) => Size = size;

	public bool IsSolid((int, int) cell)
	{
		return pathfind.nodes.ContainsKey(cell) && pathfind.nodes[cell].isWalkable;
	}
	public bool IsObstacle((int, int) cell)
	{
		return pathfind.nodes.ContainsKey(cell) && pathfind.nodes[cell].weight > 0;
	}

	public void SetSolid((int, int) cell, bool isSolid = true)
	{
		if (isSolid == false)
		{
			pathfind.nodes.Remove(cell);
			return;
		}

		pathfind.nodes[cell] = new(cell, 0, false);
	}
	public void SetObstacle((int, int) cell, int penalty = 1)
	{
		pathfind.nodes[cell] = new(cell, penalty, true);
	}
	public void SetObstacle(int tile, int[,] tiles, int penalty = 1)
	{
		if (tiles == null)
			return;

		for (int i = 0; i < tiles.GetLength(1); i++)
			for (int j = 0; j < tiles.GetLength(0); j++)
				if (tiles[j, i] == tile)
					SetObstacle((j, i), penalty);
	}

	public (int, int)[] FindPath((int, int) cellA, (int, int) cellB)
	{
		if (Size.Item1 < 1 || Size.Item2 < 1)
			return Array.Empty<(int, int)>();

		var path = pathfind.FindPath(cellA, cellB);
		var result = new (int, int)[path.Count];
		for (int i = 0; i < path.Count; i++)
			result[i] = path.Pop().position;
		
		return result;
	}
	#region Backend
	private readonly Astar pathfind = new();
	#endregion
}
