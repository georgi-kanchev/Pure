namespace Pure.Engine.Utility;

public static class Particles
{
    public static (float x, float y, uint color)[] SpawnCluster(int amount, float ageSeconds)
    {
        var pts = new (float x, float y, uint color)[amount];
        var key = pts.GetHashCode();
        data[key] = new(ageSeconds);
        movement[key] = new (float x, float y)[amount];
        points.Add(pts);

        for (var i = 0; i < amount; i++)
            pts[i] = (0f, 0f, Color.White);

        return pts;
    }

    public static bool IsClusterExisting((float x, float y, uint color)[] particles)
    {
        return points.Contains(particles) && data.ContainsKey(particles.GetHashCode());
    }

    public static void MakeLine(this (float x, float y, uint color)[] particles, (float ax, float ay, float bx, float by) line, bool keepMovement = false)
    {
        var key = particles.GetHashCode();

        if (data.ContainsKey(key) == false || points.Contains(particles) == false)
            return;

        var (a, b) = (new Point(line.ax, line.ay), new Point(line.bx, line.by));
        var step = a.Distance(b) / particles.Length;
        var point = a;
        var index = points.IndexOf(particles);

        for (var i = 0; i < particles.Length; i++)
        {
            point = point.MoveTo(b, step);

            if (keepMovement == false)
                movement[key][i] = default;

            points[index][i] = (point.X, point.Y, points[index][i].color);
        }
    }
    public static void MakeCircle(this (float x, float y, uint color)[] particles, (float x, float y) position, float radius = 1f, bool keepMovement = false, bool distribute = false)
    {
        var key = particles.GetHashCode();

        if (data.ContainsKey(key) == false || points.Contains(particles) == false)
            return;

        if (distribute)
            return;

        var angStep = 360f / particles.Length;
        var index = points.IndexOf(particles);
        for (var i = 0; i < particles.Length; i++)
        {
            var dir = new Angle(angStep * i).Direction;
            var point = new Point(position).MoveIn(dir, radius);

            if (keepMovement == false)
                movement[key][i] = default;

            points[index][i] = (point.X, point.Y, points[index][i].color);
        }
    }
    public static void MakeRectangle(this (float x, float y, uint color)[] particles, (float x, float y, float width, float height) rectangle, bool keepMovement = false)
    {
        var key = particles.GetHashCode();

        if (data.ContainsKey(key) == false || points.Contains(particles) == false)
            return;
    }

    public static (float x, float y)[] DistributePointsAlongRadius(double radius, int numberOfPoints, double angleDegrees)
    {
        var angleRadians = angleDegrees * (MathF.PI / 180.0);
        var spacing = radius / (numberOfPoints - 1);
        var pts = new (float x, float y)[numberOfPoints];

        for (var i = 0; i < numberOfPoints; i++)
        {
            var r = i * spacing;
            var x = (float)(r * Math.Cos(angleRadians));
            var y = (float)(r * Math.Sin(angleRadians));

            pts[i] = (x, y);
        }

        return pts;
    }

    public static void ApplyGravity(this (float x, float y, uint color)[] particles, (float x, float y) force)
    {
        if (data.TryGetValue(particles.GetHashCode(), out var cluster) && points.Contains(particles))
            cluster.gravity = force;
    }
    public static void ApplyFriction(this (float x, float y, uint color)[] particles, float strength)
    {
        if (data.TryGetValue(particles.GetHashCode(), out var cluster) && points.Contains(particles))
            cluster.friction = strength;
    }
    public static void ApplyOrbit(this (float x, float y, uint color)[] particles, (float x, float y) point, float radius)
    {
        if (data.TryGetValue(particles.GetHashCode(), out var cluster) == false || points.Contains(particles) == false)
            return;

        cluster.orbitPoint = point;
        cluster.orbitRadius = radius;
    }
    public static void ApplyAge(this (float x, float y, uint color)[] particles, float ageSeconds)
    {
        data[particles.GetHashCode()].timeLeft = Math.Max(ageSeconds, 0f);
    }
    public static void ApplyBounce(this (float x, float y, uint color)[] particles, float strength)
    {
        data[particles.GetHashCode()].bounceStrength = strength;
    }
    public static void ApplyTimeScale(this (float x, float y, uint color)[] particles, float timeScale)
    {
        data[particles.GetHashCode()].timeScale = timeScale;
    }
    public static void ApplySize(this (float x, float y, uint color)[] particles, float size)
    {
        data[particles.GetHashCode()].size = Math.Max(size / 2f, 0f);
    }

    public static void BounceFromNothing(this (float x, float y, uint color)[] particles)
    {
        data[particles.GetHashCode()].bounceRects.Clear();
    }
    public static void BounceFromObstacles(this (float x, float y, uint color)[] particles, params (float x, float y, float width, float height)[] obstacles)
    {
        data[particles.GetHashCode()].bounceRects.AddRange(obstacles);
    }

    public static void PushFromPoint(this (float x, float y, uint color)[] particles, (float x, float y) point, float radius, float force, bool weakerFurther = true)
    {
        PushOrPull(true, particles, point, force, radius, weakerFurther);
    }
    public static void PullToPoint(this (float x, float y, uint color)[] particles, (float x, float y) point, float radius, float force, bool weakerFurther = true)
    {
        PushOrPull(false, particles, point, force, radius, weakerFurther);
    }
    public static void PushAtAngle(this (float x, float y, uint color)[] particles, float angle, float force)
    {
        var key = particles.GetHashCode();

        if (data.TryGetValue(key, out _) == false || points.Contains(particles) == false)
            return;

        var dir = new Angle(angle).Direction;
        for (var i = 0; i < particles.Length; i++)
        {
            var (mx, my) = movement[key][i];
            movement[key][i] = (mx + dir.x * force, my + dir.y * force);
        }
    }

    public static void FadeToColor(this (float x, float y, uint color)[] particles, uint color, float duration = 1f)
    {
        var index = points.IndexOf(particles);
        var pts = points[index];
        var (tr, tg, tb, ta) = new Color(color).ToBundle();
        var copy = pts.ToArray();
        var timeScale = data[particles.GetHashCode()].timeScale;

        Time.CallFor(duration / timeScale, progress =>
        {
            for (var i = 0; i < pts.Length; i++)
            {
                var (sr, sg, sb, sa) = new Color(copy[i].color).ToBundle();
                var r = (byte)progress.Map((0f, 1f), (sr, tr));
                var g = (byte)progress.Map((0f, 1f), (sg, tg));
                var b = (byte)progress.Map((0f, 1f), (sb, tb));
                var a = (byte)progress.Map((0f, 1f), (sa, ta));

                pts[i] = (pts[i].x, pts[i].y, new Color(r, g, b, a).Value);
            }
        });
    }

#region Backend
    private static readonly Dictionary<int, ClusterData> data = [];
    private static readonly Dictionary<int, (float x, float y)[]> movement = [];
    private static readonly List<(float x, float y, uint color)[]> points = [];

    private static void PushOrPull(bool push, (float x, float y, uint color)[] particles, (float x, float y) point, float radius, float force, bool weakerFurther = true)
    {
        if (data.TryGetValue(particles.GetHashCode(), out var value) == false || points.Contains(particles) == false)
            return;

        var key = particles.GetHashCode();
        var index = points.IndexOf(particles);
        for (var i = 0; i < particles.Length; i++)
        {
            var (x, y, _) = points[index][i];
            var dist = new Point(x, y).Distance(point);

            if (dist > radius)
                continue;

            var speed = weakerFurther ? dist.Map((0f, radius), (force, 0f)) : force;
            speed *= push ? -1 : 1;
            var dir = Angle.BetweenPoints((x, y), point).Direction;
            var (mx, my) = movement[key][i];
            movement[key][i] = (mx + dir.x * speed, my + dir.y * speed);
        }
    }

    internal static void Update()
    {
        var delta = Time.Delta;
        for (var i = 0; i < points.Count; i++)
        {
            var pts = points[i];
            var key = pts.GetHashCode();
            var cluster = data[key];
            var dt = delta * cluster.timeScale;
            var rects = cluster.bounceRects;
            var sz = Math.Max(cluster.size, 0.01f);
            var bs = cluster.bounceStrength;

            cluster.timeLeft -= dt;

            for (var j = 0; j < pts.Length; j++)
            {
                var (x, y, color) = pts[j];
                var (mx, my) = movement[key][j];

                mx += cluster.gravity.x * dt;
                my += cluster.gravity.y * dt;

                mx = mx.MoveTo(0f, cluster.friction, dt);
                my = my.MoveTo(0f, cluster.friction, dt);

                var dist = new Point(x, y).Distance(cluster.orbitPoint);

                if (dist < cluster.orbitRadius)
                {
                    var speed = -(cluster.orbitRadius - dist);
                    var dir = Angle.BetweenPoints(cluster.orbitPoint, (x, y)).Direction;
                    mx += dir.x * dt * speed;
                    my += dir.y * dt * speed;
                }

                x += mx * dt;
                y += my * dt;

                if (float.IsNaN(cluster.bounceStrength) == false)
                    foreach (var rect in rects)
                    {
                        var left = rect.x - sz;
                        var right = rect.x + rect.w + sz;
                        var top = rect.y - sz;
                        var bottom = rect.y + rect.h + sz;

                        if (x.IsBetween((left, right)) == false || y.IsBetween((top, bottom)) == false)
                            continue;

                        var ang = new Angle(mx, my);
                        var l = Math.Abs(x - left);
                        var r = Math.Abs(x - right);
                        var t = Math.Abs(y - top);
                        var b = Math.Abs(y - bottom);

                        var (rx, ry) = (mx, my);
                        var speed = new Point(mx, my).Distance((0f, 0f));
                        if (l < r && l < t && l < b)
                        {
                            (rx, ry) = ang.Reflect(Angle.Left).Direction;
                            x = left;
                        }
                        else if (r < l && r < t && r < b)
                        {
                            (rx, ry) = ang.Reflect(Angle.Right).Direction;
                            x = right;
                        }
                        else if (t < l && t < r && t < b)
                        {
                            (rx, ry) = ang.Reflect(Angle.Up).Direction;
                            y = top;
                        }
                        else if (b < t && b < l && b < r)
                        {
                            (rx, ry) = ang.Reflect(Angle.Down).Direction;
                            y = bottom;
                        }

                        mx = rx * speed;
                        my = ry * speed;
                        mx *= bs;
                        my *= bs;
                    }

                pts[j] = (x, y, color);
                movement[key][j] = (mx, my);
            }

            if (data[key].timeLeft > 0f)
                continue;

            for (var j = 0; j < pts.Length; j++)
                pts[j] = default;

            data.Remove(key);
            movement.Remove(key);
            points.Remove(pts);
            i--;
        }
    }
#endregion
}