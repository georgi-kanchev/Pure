using Pure.Engine.Utilities;

namespace Supremacy1257;

public class NavMap
{
    public List<NavPath> Paths { get; } = new();
    public List<NavPoint> Points { get; } = new();

    public NavMap(World world, int pointCount = 300, int spreadSteps = 15, float targetDistance = 7f)
    {
        this.world = world;

        Generate(pointCount, spreadSteps, targetDistance);
    }

    public void ReducePointsOnTile(float percent, int tileId)
    {
        for (var i = 0; i < Points.Count; i++)
        {
            var p = Points[i];
            var tile = world.Terrain.TileAt(((int)p.Position.X, (int)p.Position.Y)).Id;
            var seed = Game.SEED.ToSeed((int)p.Position.Y, (int)p.Position.X);

            if (tile != tileId || percent.HasChance(seed) == false)
                continue;

            Points.Remove(p);

            for (var j = 0; j < p.Connections.Count; j++)
            {
                var path = p.Connections[j];
                path.A.Connections.Remove(path);
                path.B.Connections.Remove(path);
                Paths.Remove(path);

                j--;
            }

            i--;
        }

        GeneratePaths();
    }

    public void Update()
    {
        foreach (var path in Paths)
        {
            var (ax, ay) = path.A.Position.XY;
            var (bx, by) = path.B.Position.XY;

            var colorA = world.Territory.TileAt(((int)ax, (int)ay)).Tint;
            var colorB = world.Territory.TileAt(((int)bx, (int)by)).Tint;
            var color = Color.White;
            color = colorA == colorB ? Color.White : color;
            color = colorA != colorB ? Color.Red : color;
            color = colorA == 0 || colorB == 0 ? Color.Gray : color;

            path.Line = new(path.Line.A, path.Line.B, color);

            world.Layer.DrawLines(path.Line);
        }
    }

#region Backend
    private readonly World world;

    private void Generate(int pointCount, int spreadSteps, float targetDistance)
    {
        GeneratePoints(pointCount, spreadSteps, targetDistance);
        GeneratePaths();
    }
    private void GeneratePoints(int pointCount, int spreadSteps, float targetDistance)
    {
        Points.Clear();

        for (var i = 0; i < pointCount; i++)
        {
            var seed1 = Game.SEED.ToSeed(i, 1);
            var seed2 = Game.SEED.ToSeed(i, 2);
            var pos = new Point(
                (2f, world.Size.width - 3f).Random(seed: seed1),
                (2f, world.Size.height - 3f).Random(seed: seed2));

            Points.Add(new(pos));
        }

        for (var i = 0; i < spreadSteps; i++)
        {
            var newPoints = new List<NavPoint>();

            foreach (var point in Points)
                newPoints.Add(ComputeCentroid(point, targetDistance));

            Points.Clear();
            Points.AddRange(newPoints);
        }

        RemovePointClusters();
        CenterPoints();

        NavPoint ComputeCentroid(NavPoint pt, float distance)
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
        void RemovePointClusters()
        {
            // remove point clusters
            var remainingPoints = new List<NavPoint>();

            for (var i = 0; i < Points.Count; i++)
            {
                var tooClose = false;

                for (var j = 0; j < remainingPoints.Count; j++)
                {
                    var dist = remainingPoints[j].Position.Distance(Points[i].Position);
                    if (dist >= 0.1f) // min distance
                        continue;

                    tooClose = true;
                    break;
                }

                if (tooClose == false)
                    remainingPoints.Add(Points[i]);
            }

            Points.Clear();
            Points.AddRange(remainingPoints);
        }
        void CenterPoints()
        {
            foreach (var point in Points)
                point.Position = new(((int)point.Position.X + 0.5f, (int)point.Position.Y + 0.5f));
        }
    }
    private void GeneratePaths()
    {
        Paths.Clear();

        foreach (var p in Points)
        {
            var closestPts = new SortedDictionary<float, NavPoint>();
            foreach (var pt in Points)
                closestPts[p.Position.Distance(pt.Position)] = pt;

            var count = 0;
            foreach (var (dist, pt) in closestPts)
            {
                if (count < 5)
                {
                    var p1 = p.Position.MoveTo(pt.Position, 1f);
                    var p2 = pt.Position.MoveTo(p.Position, 1f);

                    if (Math.Abs(p.Position.X - pt.Position.X) < 0.01f &&
                        Math.Abs(p.Position.Y - pt.Position.Y) < 0.01f)
                        continue;

                    var path = new NavPath(p, pt, new(p1, p2));

                    if (p.Connections.Contains(path) == false)
                        p.Connections.Add(path);

                    if (pt.Connections.Contains(path) == false)
                        pt.Connections.Add(path);

                    Paths.Add(path);
                }

                count++;
            }
        }

        RemoveOverlappingAndInvalidPaths();
        RemovePathsTooCloseToPoints();

        void RemovePath(NavPath path)
        {
            path.A.Connections.Remove(path);
            path.B.Connections.Remove(path);
            Paths.Remove(path);
        }
        void RemoveOverlappingAndInvalidPaths()
        {
            for (var i = 0; i < Paths.Count; i++)
                for (var j = 0; j < Paths.Count; j++)
                {
                    if (Paths[i] == Paths[j] ||
                        float.IsNaN(Paths[j].Line.Angle) ||
                        Paths[i].Line.IsOverlapping(Paths[j].Line) == false)
                        continue;

                    RemovePath(Paths[j]);
                    j--;
                }
        }
        void RemovePathsTooCloseToPoints()
        {
            for (var i = 0; i < Paths.Count; i++)
            {
                var path = Paths[i];
                for (var j = 0; j < Points.Count; j++)
                {
                    var pt = Points[j];
                    if (pt.Connections.Contains(path))
                        continue;

                    var closestPointToPath = (Point)path.Line.ClosestPoint(pt.Position.XY);
                    var dist = closestPointToPath.Distance(pt.Position);
                    if (dist >= 3f)
                        continue;

                    RemovePath(path);
                    i--;
                }
            }
        }
    }
#endregion
}