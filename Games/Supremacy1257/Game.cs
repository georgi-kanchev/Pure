using Pure.Engine.Collision;
using Pure.Engine.Tilemap;
using Pure.Engine.Utilities;
using Pure.Engine.Window;
using Monitor = Pure.Engine.Window.Monitor;

namespace Supremacy1257;

public class Game
{
    private static void Main()
    {
        Window.Title = "Supremacy 1257";
        var aspect = Monitor.Current.AspectRatio;
        var ui = new Layer((aspect.width * 5, aspect.height * 5))
        {
            AtlasPath = "urizen.png",
            AtlasTileGap = (1, 1),
            AtlasTileSize = (12, 12)
        };
        var world = new World(100, 100);

        Window.PixelScale = 1f;
        ui.Zoom = 2f;

        while (Window.KeepOpen())
        {
            world.Update();

            ui.DrawCursor((69, 0).ToIndex1D(ui.AtlasTileCount));
            ui.Draw();
        }
    }

    private static List<(float x, float y, uint color)> GeneratePoints(int w, int h, float steps, float targetDistance)
    {
        var points = new List<(float x, float y, uint color)>();
        for (var i = 0; i < 300; i++)
            points.Add(new Point((2f, w - 3f).Random(), (2f, h - 3f).Random()));

        for (var i = 0; i < steps; i++)
        {
            var newPoints = new List<(float x, float y, uint color)>();

            foreach (var point in points)
            {
                var centroid = ComputeCentroid(point);
                newPoints.Add((centroid.X, centroid.Y, Color.Red));
            }

            points = newPoints; // Replace old points with the new relaxed points
        }

        return points;

        Point ComputeCentroid(Point currentPoint)
        {
            var (sumX, sumY) = (0f, 0f);
            var count = 0;

            foreach (var point in points)
            {
                var distance = MathF.Sqrt(MathF.Pow(point.x - currentPoint.X, 2) +
                                          MathF.Pow(point.y - currentPoint.Y, 2));
                if (distance >= targetDistance)
                    continue;

                sumX += point.x;
                sumY += point.y;
                count++;
            }

            if (count == 0)
                return currentPoint;

            return (sumX / count, sumY / count);
        }
    }
    private static (float ax, float ay, float bx, float by, uint color)[] GeneratePaths((float x, float y, uint color)[] points)
    {
        var pts = points.ToList();
        var result = new List<(float ax, float ay, float bx, float by, uint color)>();

        foreach (Point p in pts)
        {
            var closestPts = new SortedDictionary<float, Point>();
            foreach (Point pt in pts)
                closestPts[p.Distance(pt)] = pt;

            var count = 0;
            foreach (var (dist, pt) in closestPts)
            {
                if (count < 5)
                    result.Add(new Line(p.PercentTo(5f, pt), pt.PercentTo(5f, p)));

                count++;
            }
        }

        for (var i = 0; i < result.Count; i++)
            for (var j = 0; j < result.Count; j++)
            {
                if (result[i] == result[j] || ((Line)result[i]).IsOverlapping((Line)result[j]) == false)
                    continue;

                result.Remove(result[j]);
                j--;
            }

        return result.ToArray();
    }
    private static void ReduceWaterPoints(List<(float x, float y, uint color)> pts, Tilemap terrain, int water, int mount1, int mount2)
    {
        for (var i = 0; i < pts.Count; i++)
        {
            var tile = terrain.TileAt(((int)pts[i].x, (int)pts[i].y)).Id;
            var removePath = tile == water || tile == mount1 || tile == mount2;

            if (removePath == false || 50f.HasChance() == false)
                continue;

            pts.Remove(pts[i]);
            i--;
        }
    }
}