namespace Pure.Engine.Collision;

public class PathMap
{
    public SizeI Size
    {
        get => pathfind.Size;
    }

    public PathMap(SizeI size)
    {
        pathfind.Size = size;
        Init();
    }

    public void SetObstacle(float penalty, VecI[]? cells)
    {
        if (cells == null || cells.Length == 0)
            return;

        foreach (var cell in cells)
            pathfind.SetNode(cell, penalty);
    }
    public void SetObstacle(float penalty, ushort tileId, ushort[,]? tileIds)
    {
        if (tileIds == null || tileIds.Length == 0)
            return;

        for (var i = 0; i < tileIds.GetLength(1); i++)
            for (var j = 0; j < tileIds.GetLength(0); j++)
                if (tileIds[j, i] == tileId)
                    pathfind.SetNode((j, i), penalty);
    }

    public bool IsSolid(VecI cell)
    {
        return float.IsNaN(PenaltyAt(cell)) || float.IsInfinity(PenaltyAt(cell));
    }
    public bool IsObstacle(VecI cell)
    {
        return float.IsFinite(PenaltyAt(cell));
    }
    public bool IsEmpty(VecI cell)
    {
        return PenaltyAt(cell) is > -0.001f and < 0.001f;
    }

    public float PenaltyAt(VecI cell)
    {
        return pathfind.GetNode(cell)?.penalty ?? float.NaN;
    }

    public VecF[] FindPath(VecF start, VecF goal, int slopeFactor = 1)
    {
        if (Size.width < 1 || Size.height < 1)
            return [];

        return pathfind.FindPath(start, goal, slopeFactor);
    }

#region Backend
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
    internal class DoNotSave : Attribute;

    private readonly Astar pathfind = new();

    private void Init()
    {
        SetObstacle(0, 0, new ushort[Size.width, Size.height]);
    }
#endregion
}