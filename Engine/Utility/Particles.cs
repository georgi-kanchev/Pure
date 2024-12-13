namespace Pure.Engine.Utility;

public static class Particles
{
    public static (float x, float y, uint color)[] Spawn(int amount, float ageSeconds)
    {
        var pts = new (float x, float y, uint color)[amount];
        var key = pts.GetHashCode();
        data[key] = new ParticleData[amount];
        points.Add(pts);

        for (var i = 0; i < amount; i++)
        {
            pts[i] = (0f, 0f, Color.White);
            data[key][i] = new(ageSeconds);
        }

        return pts;
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
        if (data.TryGetValue(particles.GetHashCode(), out var value) == false || points.Contains(particles) == false)
            return;

        for (var i = 0; i < particles.Length; i++)
            value[i].Gravity = force;
    }
    public static void ApplyFriction(this (float x, float y, uint color)[] particles, float strength)
    {
        if (data.TryGetValue(particles.GetHashCode(), out var value) == false || points.Contains(particles) == false)
            return;

        for (var i = 0; i < particles.Length; i++)
            value[i].Friction = strength;
    }
    public static void ApplyOrbit(this (float x, float y, uint color)[] particles, (float x, float y) point, float radius)
    {
        if (data.TryGetValue(particles.GetHashCode(), out var value) == false || points.Contains(particles) == false)
            return;

        for (var i = 0; i < particles.Length; i++)
        {
            value[i].OrbitPoint = point;
            value[i].OrbitRadius = radius;
        }
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
        if (data.TryGetValue(particles.GetHashCode(), out var value) == false || points.Contains(particles) == false)
            return;

        var dir = new Angle(angle).Direction;
        for (var i = 0; i < particles.Length; i++)
        {
            var (mx, my) = value[i].Movement;
            value[i].Movement = (mx + dir.x * force, my + dir.y * force);
        }
    }

#region Backend
    private static readonly Dictionary<int, ParticleData[]> data = [];
    private static readonly List<(float x, float y, uint color)[]> points = [];

    private static void PushOrPull(bool push, (float x, float y, uint color)[] particles, (float x, float y) point, float radius, float force, bool weakerFurther = true)
    {
        if (data.TryGetValue(particles.GetHashCode(), out var value) == false || points.Contains(particles) == false)
            return;

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
            var (mx, my) = value[i].Movement;
            value[i].Movement = (mx + dir.x * speed, my + dir.y * speed);
        }
    }

    internal static void Update()
    {
        var dt = Time.Delta;
        for (var i = 0; i < points.Count; i++)
        {
            var pts = points[i];
            var key = pts.GetHashCode();
            var pData = data[key];

            for (var j = 0; j < pts.Length; j++)
            {
                var (x, y, color) = pts[j];
                var p = pData[j];
                var (mx, my) = p.Movement;

                mx += p.Gravity.x * dt;
                my += p.Gravity.y * dt;

                mx = mx.MoveTo(0f, p.Friction, dt);
                my = my.MoveTo(0f, p.Friction, dt);

                var dist = new Point(x, y).Distance(p.OrbitPoint);

                if (dist < p.OrbitRadius)
                {
                    var speed = -(p.OrbitRadius - dist);
                    var dir = Angle.BetweenPoints(p.OrbitPoint, (x, y)).Direction;
                    mx += dir.x * dt * speed;
                    my += dir.y * dt * speed;
                }

                x += mx * dt;
                y += my * dt;

                pts[j] = (x, y, color);
                p.Movement = (mx, my);
                p.TimeLeft -= dt;
            }
        }
    }
#endregion
}