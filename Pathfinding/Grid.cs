namespace Pure.Pathfinding;

/// <summary>
/// Represents a grid used for pathfinding on a 2D plane.
/// </summary>
public class Grid
{
	/// <summary>
	/// Gets or sets the size of the grid.
	/// </summary>
	public (int width, int height) Size
	{
		get => pathfind.Size;
		set => pathfind.Size = value;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="Grid"/> class using the specified file path.
	/// </summary>
	/// <param name="path">The path of the file containing grid data.</param>
	public Grid(string path) => pathfind.Load(path);
	/// <summary>
	/// Initializes a new instance of the <see cref="Grid"/> class using the specified size.
	/// </summary>
	/// <param name="size">The size of the grid.</param>
	public Grid((int width, int height) size) => Size = size;

	/// <summary>
	/// Saves the grid data to the specified file.
	/// </summary>
	/// <param name="path">The path of the file to save to.</param>
	public void Save(string path) => pathfind.Save(path);

	/// <summary>
	/// Determines whether the cell at the specified position is solid (non-walkable).
	/// </summary>
	/// <param name="cell">The position of the cell to check.</param>
	/// <returns><c>true</c> if the cell is solid; otherwise, <c>false</c>.</returns>
	public bool IsSolid((int x, int y) cell)
	{
		var n = pathfind.GetNode(cell);
		return n == null ? true : n.isWalkable == false;
	}
	/// <summary>
	/// Determines whether the cell at the specified position is an obstacle (walkable but with a non-zero penalty).
	/// </summary>
	/// <param name="cell">The position of the cell to check.</param>
	/// <returns><c>true</c> if the cell is an obstacle; otherwise, <c>false</c>.</returns>
	public bool IsObstacle((int x, int y) cell)
	{
		var n = pathfind.GetNode(cell);
		return n == null ? false : n.isWalkable && n.weight > 0;
	}
	/// <summary>
	/// Gets the penalty value of the cell at the specified position.
	/// </summary>
	/// <param name="cell">The position of the cell to check.</param>
	/// <returns>The penalty value of the cell.</returns>
	public int PenaltyAt((int x, int y) cell)
	{
		var n = pathfind.GetNode(cell);
		return n == null ? 0 : n.weight;
	}
	/// <summary>
	/// Sets the specified cell as solid or non-solid (walkable or non-walkable).
	/// </summary>
	/// <param name="cell">The position of the cell to set.</param>
	/// <param name="isSolid">The value indicating whether the cell is solid or not.</param>
	public void SetSolid((int x, int y) cell, bool isSolid = true)
	{
		pathfind.SetNode(cell, 0, isSolid == false);
	}

	/// <summary>
	/// Sets the specified cell as an obstacle with the given penalty value.
	/// </summary>
	/// <param name="cell">The position of the cell to set as an obstacle.</param>
	/// <param name="penalty">The penalty value
	/// of the obstacle.</param>
	public void SetObstacle((int x, int y) cell, int penalty = 1)
	{
		pathfind.SetNode(cell, penalty, false);
	}
	/// <summary>
	/// Sets the penalties of the specified tile in the provided tilemap (2D array of tiles) to the given penalty value and marks the corresponding cells as obstacles.
	/// </summary>
	/// <param name="tile">The tile to set as an obstacle.</param>
	/// <param name="tiles">The tilemap to use as reference.</param>
	/// <param name="penalty">The penalty value to set for the obstacles.</param>
	public void SetObstacle(int tile, int[,] tiles, int penalty = 1)
	{
		if (tiles == null)
			return;

		for (int i = 0; i < tiles.GetLength(1); i++)
			for (int j = 0; j < tiles.GetLength(0); j++)
				if (tiles[j, i] == tile)
					SetObstacle((j, i), penalty);
	}

	/// <summary>
	/// Calculates a path from the start to the goal position using the A* algorithm.
	/// </summary>
	/// <param name="start">The position of the starting cell.</param>
	/// <param name="goal">The position of the goal cell.</param>
	/// <returns>The list of positions representing the calculated path, or <c>null</c> if no path could be found.</returns>
	public (int x, int y)[] FindPath((int x, int y) start, (int x, int y) goal)
	{
		if (Size.Item1 < 1 || Size.Item2 < 1)
			return Array.Empty<(int, int)>();

		return pathfind.FindPath(cellA, cellB);
	}

	#region Backend
	private readonly Astar pathfind = new();
	#endregion
}
