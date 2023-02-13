namespace Pure.Pathfinding;

public class Map
{
	public (int, int) Size
	{
		get => pathfind.Size;
		set => pathfind.Size = value;
	}

	public Map((int, int) size) => Size = size;

	public bool IsSolid((int, int) cell)
	{
		var n = pathfind.GetNode(cell);
		return n == null ? true : n.isWalkable == false;
	}
	public bool IsObstacle((int, int) cell)
	{
		var n = pathfind.GetNode(cell);
		return n == null ? false : n.isWalkable && n.weight > 0;
	}
	public int PenaltyAt((int, int) cell)
	{
		var n = pathfind.GetNode(cell);
		return n == null ? 0 : n.weight;
	}

	public void SetSolid((int, int) cell, bool isSolid = true)
	{
		pathfind.SetNode(cell, 0, isSolid == false);
	}
	public void SetObstacle((int, int) cell, int penalty = 1)
	{
		pathfind.SetNode(cell, penalty, false);
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

		return pathfind.FindPath(cellA, cellB);
	}
	#region Backend
	private readonly Astar pathfind = new();
	#endregion
}
