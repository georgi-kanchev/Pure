using Pure.Engine.Collision;
using Pure.Engine.Tilemap;
using Pure.Engine.Utilities;

namespace Supremacy1257;

public class Territory
{
    public LinePack Border { get; } = new();

    public Territory(NavPoint navPoint, World world, Color color)
    {
        this.navPoint = navPoint;
        this.world = world;
        this.color = color;

        GenerateTerritory();
        EnsurePointIsInsideBorder();
        //ApplyTerritoryTiles();
    }

    public void Update()
    {
        world.Layer.DrawLines(Border.ToBundle());
    }

#region Backend
    private readonly Color color;
    private readonly NavPoint navPoint;
    private readonly World world;

    private void GenerateTerritory()
    {
        var paths = navPoint.Connections;
        var midwayPoints = new List<Point>();
        var sortedLines = new SortedDictionary<float, Line>();

        foreach (var path in paths)
        {
            var distA = ((Point)path.Line.A).Distance(navPoint.Position);
            var distB = ((Point)path.Line.B).Distance(navPoint.Position);
            var closestPoint = distA < distB ? path.Line.A : path.Line.B;
            var ang = navPoint.Position.Angle(closestPoint);
            sortedLines[ang] = path.Line;
        }

        foreach (var (ang, line) in sortedLines)
        {
            var a = (Point)line.A;
            var b = (Point)line.B;
            midwayPoints.Add(a.PercentTo(50f, b));
        }

        for (var i = 1; i < midwayPoints.Count; i++)
        {
            var a = midwayPoints[i - 1];
            var b = midwayPoints[i];
            Border.Add(new Line(a, b, color));
        }

        Border.Add(new Line(Border[0].A, Border[^1].B, color));
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

        Border.Add(new Line(insideLine.A, navPoint.Position, color));
        Border.Add(new Line(insideLine.B, navPoint.Position, color));
    }
    private void ApplyTerritoryTiles()
    {
        var points = new List<Point>();
        var tile = new Tile(world.Full, color);
        for (var i = 0; i < Border.Count; i++)
        {
            var (la, lb) = (Border[i].A, Border[i].B);
            var (a, b) = (((int)la.x, (int)la.y), ((int)lb.x, (int)lb.y));
            world.Territories.SetLine(a, b, null, tile);
            points.Add(la);
            points.Add(lb);
        }

        var center = Point.Average(points.ToArray()) ?? new Point(-1, -1);
        world.Territories.Flood(center, false, null, tile);
    }
#endregion
}