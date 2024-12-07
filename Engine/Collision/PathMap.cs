namespace Pure.Engine.Collision;

public class PathMap
{
    public (int width, int height) Size
    {
        get => pathfind.Size;
    }

    public PathMap((int width, int height) size)
    {
        pathfind.Size = size;
        Init();
    }

    public void SetObstacle(float penalty, params (int x, int y)[]? cells)
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

    public bool IsSolid((int x, int y) cell)
    {
        return float.IsNaN(PenaltyAt(cell)) || float.IsInfinity(PenaltyAt(cell));
    }
    public bool IsObstacle((int x, int y) cell)
    {
        return float.IsFinite(PenaltyAt(cell));
    }
    public bool IsEmpty((int x, int y) cell)
    {
        return PenaltyAt(cell) is > -0.001f and < 0.001f;
    }

    public float PenaltyAt((int x, int y) cell)
    {
        return pathfind.GetNode(cell)?.penalty ?? float.NaN;
    }

    public (float x, float y)[] FindPath((float x, float y) start, (float x, float y) goal, int slopeFactor = 1)
    {
        if (Size.width < 1 || Size.height < 1)
            return [];

        return pathfind.FindPath(start, goal, false, out _, slopeFactor, uint.MaxValue);
    }
    public (float x, float y, uint color)[] FindPath((float x, float y) start, (float x, float y) goal, uint color, int slopeFactor = 1)
    {
        if (Size.width < 1 || Size.height < 1)
            return [];

        pathfind.FindPath(start, goal, true, out var withColors, slopeFactor, color);
        return withColors;
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