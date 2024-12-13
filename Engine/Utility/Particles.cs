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

    public static void MakeLine(this (float x, float y, uint color)[] particles, (float ax, float ay, float bx, float by) line)
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
            points[index][i] = (point.X, point.Y, points[index][i].color);
        }
    }
    public static void MakeCircle(this (float x, float y, uint color)[] particles, (float x, float y) position, float radius = 1f)
    {
        var key = particles.GetHashCode();

        if (data.ContainsKey(key) == false || points.Contains(particles) == false)
            return;

        var angStep = 360f / particles.Length;
        var index = points.IndexOf(particles);
        for (var i = 0; i < particles.Length; i++)
        {
            var dir = new Angle(angStep * i).Direction;
            var point = new Point(position).MoveIn(dir, radius);
            points[index][i] = (point.X, point.Y, points[index][i].color);
        }
    }

    public static void ApplyGravity(this (float x, float y, uint color)[] particles, (float x, float y) force)
    {
        if (data.TryGetValue(particles.GetHashCode(), out var cluster) && points.Contains(particles))
            cluster.Gravity = force;
    }
    public static void ApplyFriction(this (float x, float y, uint color)[] particles, float strength)
    {
        if (data.TryGetValue(particles.GetHashCode(), out var cluster) && points.Contains(particles))
            cluster.Friction = strength;
    }
    public static void ApplyOrbit(this (float x, float y, uint color)[] particles, (float x, float y) point, float radius)
    {
        if (data.TryGetValue(particles.GetHashCode(), out var cluster) == false || points.Contains(particles) == false)
            return;

        cluster.OrbitPoint = point;
        cluster.OrbitRadius = radius;
    }
    public static void ApplyAge(this (float x, float y, uint color)[] particles, float ageSeconds)
    {
        data[particles.GetHashCode()].TimeLeft = ageSeconds;
    }

    public static void PushFrom(this (float x, float y, uint color)[] particles, (float x, float y) point, float radius, float force, bool weakerFurther = true)
    {
        PushOrPull(true, particles, point, force, radius, weakerFurther);
    }
    public static void PullTo(this (float x, float y, uint color)[] particles, (float x, float y) point, float radius, float force, bool weakerFurther = true)
    {
        PushOrPull(false, particles, point, force, radius, weakerFurther);
    }
    public static void PushAt(this (float x, float y, uint color)[] particles, float angle, float force)
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

    public static void FadeTo(this (float x, float y, uint color)[] particles, uint color, float duration = 1f)
    {
        var index = points.IndexOf(particles);
        var pts = points[index];
        var (tr, tg, tb, ta) = new Color(color).ToBundle();
        var copy = pts.ToArray();

        Time.CallFor(duration, progress =>
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
        var dt = Time.Delta;
        for (var i = 0; i < points.Count; i++)
        {
            var pts = points[i];
            var key = pts.GetHashCode();
            var cluster = data[key];

            cluster.TimeLeft -= dt;

            for (var j = 0; j < pts.Length; j++)
            {
                var (x, y, color) = pts[j];
                var (mx, my) = movement[key][j];

                mx += cluster.Gravity.x * dt;
                my += cluster.Gravity.y * dt;

                mx = mx.MoveTo(0f, cluster.Friction, dt);
                my = my.MoveTo(0f, cluster.Friction, dt);

                var dist = new Point(x, y).Distance(cluster.OrbitPoint);

                if (dist < cluster.OrbitRadius)
                {
                    var speed = -(cluster.OrbitRadius - dist);
                    var dir = Angle.BetweenPoints(cluster.OrbitPoint, (x, y)).Direction;
                    mx += dir.x * dt * speed;
                    my += dir.y * dt * speed;
                }

                x += mx * dt;
                y += my * dt;

                pts[j] = (x, y, color);
                movement[key][j] = (mx, my);
            }

            if (data[key].TimeLeft > 0f)
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