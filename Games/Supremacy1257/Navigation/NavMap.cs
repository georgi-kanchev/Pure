using Pure.Engine.Utilities;
using Pure.Engine.Window;

namespace Supremacy1257;

public class NavMap
{
    public List<NavPath> Paths { get; } = new();
    public List<NavPoint> Points { get; } = new();

    public NavMap(World world, int pointCount = 300, int spreadSteps = 10, float targetDistance = 5f)
    {
        this.world = world;

        GeneratePoints(pointCount, spreadSteps, targetDistance);
        GeneratePaths();
        RemoveRedundantPaths();
    }

    public void ReducePointsOnTile(float percent, int tileId)
    {
        for (var i = 0; i < Points.Count; i++)
        {
            var tile = world.Terrain.TileAt(((int)Points[i].Position.X, (int)Points[i].Position.Y)).Id;
            var seed = Game.SEED.ToSeed((int)Points[i].Position.Y, (int)Points[i].Position.X);

            if (tile != tileId || percent.HasChance(seed) == false)
                continue;

            Points.Remove(Points[i]);
            i--;
        }

        GeneratePaths();
        RemoveRedundantPaths();
        GenerateDrawData();
    }

    public void Update()
    {
        world.Layer.DrawLines(paths);
        world.Layer.DrawPoints(points);
        world.Layer.Draw();
    }

#region Backend
    private readonly World world;
    private (float x, float y, uint color)[]? points;
    private (float ax, float ay, float bx, float by, uint color)[]? paths;

    private NavPoint ComputeCentroid(NavPoint pt, float distance)
    {
        var (sumX, sumY) = (0f, 0f);
        var count = 0;

        foreach (var point in Points)
        {
            if (point.Position.Distance(pt.Position) >= distance)
                continue;

            sumX += point.Position.X;
            sumY += point.Position.Y;
            count++;
        }

        return count == 0 ? pt : new(sumX / count, sumY / count);
    }

    private void GeneratePoints(int pointCount, int spreadSteps, float targetDistance)
    {
        Points.Clear();

        for (var i = 0; i < pointCount; i++)
        {
            var seed1 = Game.SEED.ToSeed(i, 1);
            var seed2 = Game.SEED.ToSeed(i, 2);

            Points.Add(new(
                (2f, world.Size.width - 3f).Random(seed: seed1),
                (2f, world.Size.height - 3f).Random(seed: seed2)));
        }

        for (var i = 0; i < spreadSteps; i++)
        {
            var newPoints = new List<NavPoint>();

            foreach (var point in Points)
                newPoints.Add(ComputeCentroid(point, targetDistance));

            Points.Clear();
            Points.AddRange(newPoints);
        }
    }
    private void GeneratePaths()
    {
        Paths.Clear();

        foreach (var p in Points)
        {
            var closestPts = new SortedDictionary<float, Point>();
            foreach (var pt in Points)
                closestPts[p.Position.Distance(pt.Position)] = pt.Position;

            var count = 0;
            foreach (var (dist, pt) in closestPts)
            {
                if (count < 5)
                {
                    var p1 = new NavPoint(p.Position.MoveTo(pt, 1f));
                    var p2 = new NavPoint(pt.MoveTo(p.Position, 1f));
                    var path = new NavPath(p1, p2);
                    p1.Connections.Add(path);
                    p2.Connections.Add(path);

                    Paths.Add(path);
                }

                count++;
            }
        }

        for (var i = 0; i < Paths.Count; i++)
            for (var j = 0; j < Paths.Count; j++)
            {
                if (Paths[i] == Paths[j] || Paths[i].Line.IsOverlapping(Paths[j].Line) == false)
                    continue;

                var path = Paths[j];
                path.A.Connections.Remove(path);
                path.B.Connections.Remove(path);
                Paths.Remove(path);
                j--;
            }
    }
    private void GenerateDrawData()
    {
        var drawPts = new List<(float x, float y, uint color)>();
        var drawLines = new List<(float ax, float ay, float bx, float by, uint color)>();

        foreach (var p in Points)
            drawPts.Add(p.Position);
        foreach (var path in Paths)
        {
            var l = path.Line;
            l.Color = path.Color;
            if (l.Color != uint.MaxValue)
                ;
            drawLines.Add(l);
        }

        points = drawPts.ToArray();
        paths = drawLines.ToArray();
    }

    private void RemoveRedundantPaths()
    {
        // remove paths that are close to a point but not connecting to it
        for (var i = 0; i < Paths.Count; i++)
        {
            var path = Paths[i];
            for (var j = 0; j < Points.Count; j++)
            {
                var pt = Points[j];
                if (pt.Connections.Contains(path) || path.Line.A == path.Line.B)
                    continue;

                var closestPointToPath = (Point)path.Line.ClosestPoint(pt.Position.XY);
                var dist = closestPointToPath.Distance(pt.Position);
                if (dist >= 0.99f)
                    continue;

                path.A.Connections.Remove(path);
                path.B.Connections.Remove(path);
                Paths.Remove(path);
                i--;
            }
        }

        GenerateDrawData();
    }
#endregion
}