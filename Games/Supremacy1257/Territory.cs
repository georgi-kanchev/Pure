using Pure.Engine.Collision;
using Pure.Engine.Tilemap;
using Pure.Engine.Utilities;

namespace Supremacy1257;

public class Territory
{
    public LinePack Border { get; } = new();

    public Territory(NavPoint navPoint, World world, Tile tile)
    {
        this.navPoint = navPoint;
        this.world = world;
        this.tile = tile;

        GenerateTerritory();
        EnsurePointIsInsideBorder();
        ApplyTerritoryTiles();
    }

    public void Update()
    {
        world.Layer.DrawLines(Border.ToBundle());
    }

#region Backend
    private readonly Tile tile;
    private readonly NavPoint navPoint;
    private readonly World world;

    private void GenerateTerritory()
    {
        var sortedPaths = new SortedDictionary<float, Point>();

        foreach (var path in navPoint.Connections)
        {
            var oppositePoint = path.A == navPoint ? path.B : path.A;
            var angle = new Line(navPoint.Position, oppositePoint.Position).Angle;
            sortedPaths[angle] = oppositePoint.Position;
        }

        var borderPoints = sortedPaths.Values.ToList();

        for (var i = 1; i < borderPoints.Count; i++)
            Border.Add(new Line(borderPoints[i - 1], borderPoints[i], tile.Tint));

        Border.Add(new Line(Border[0].A, Border[^1].B, tile.Tint));
    }
    private void EnsurePointIsInsideBorder()
    {
        var insideLine = new Line();
        var addLines = false;

        for (var i = 0; i < Border.Count; i++)
        {
            var line = Border[i];
            var remove = false;
            var closestPoint = (Point)line.ClosestPoint(navPoint.Position.XY);

            foreach (var path in navPoint.Connections)
            {
                var center = ((Point)path.Line.A).PercentTo(50f, path.Line.B);
                var crossPoint = path.Line.CrossPoint(line);

                if ((!line.IsOverlapping(path.Line) ||
                     (!(center.Distance(crossPoint) > 0.05f) &&
                      !(closestPoint.Distance(navPoint.Position) < 1f))) &&
                    Border.Count != 2)
                    continue;

                remove = true;
                break;
            }

            if (remove == false)
                continue;

            Border.RemoveAt(i);
            i--;
            insideLine = line;
            addLines = true;
        }

        if (addLines == false)
            return;

        Border.Add(new Line(insideLine.A, navPoint.Position));
        Border.Add(new Line(insideLine.B, navPoint.Position));
    }
    private void ApplyTerritoryTiles()
    {
        var points = new List<Point>();
        for (var i = 0; i < Border.Count; i++)
        {
            var (la, lb) = (Border[i].A, Border[i].B);
            var (a, b) = (((int)la.x, (int)la.y), ((int)lb.x, (int)lb.y));
            world.Territory.SetLine(a, b, null, tile);
            points.Add(la);
            points.Add(lb);
        }

        var center = Point.Average(points.ToArray()) ?? new Point(-1, -1);
        world.Territory.Flood(center, false, null, tile);
    }
#endregion
}