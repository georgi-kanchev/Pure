namespace Pure.Engine.Utility;

public enum Distribution { FillEvenly, FillRandomly, Outline }

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

    public static bool IsClusterAlive((float x, float y, uint color)[] particles)
    {
        return points.Contains(particles) && data.ContainsKey(particles.GetHashCode());
    }

    public static void MakeLine(this (float x, float y, uint color)[] particles, (float ax, float ay, float bx, float by) line, bool keepMovement = false, Distribution distribution = Distribution.FillEvenly)
    {
        var key = particles.GetHashCode();

        if (data.ContainsKey(key) == false || points.Contains(particles) == false)
            return;

        var (a, b) = (new Point(line.ax, line.ay), new Point(line.bx, line.by));
        var index = points.IndexOf(particles);
        var rand = distribution == Distribution.FillRandomly;

        for (var i = 0; i < particles.Length; i++)
        {
            if (keepMovement == false)
                movement[key][i] = default;

            var progress = rand ? (0f, 100f).Random() : (float)i / particles.Length * 100f;
            var (x, y) = a.PercentTo(progress, b).XY;
            points[index][i] = (x, y, points[index][i].color);
        }
    }
    public static void MakeCircle(this (float x, float y, uint color)[] particles, (float x, float y) position, float radius = 1f, bool keepMovement = false, Distribution distribution = Distribution.FillEvenly)
    {
        var key = particles.GetHashCode();

        if (data.ContainsKey(key) == false || points.Contains(particles) == false)
            return;

        var index = points.IndexOf(particles);

        for (var i = 0; i < particles.Length; i++)
        {
            var r = distribution == Distribution.FillRandomly ? (0f, radius).Random(10f) : radius;
            var ang = 360f * i / particles.Length;

            if (distribution == Distribution.FillEvenly)
            {
                ang = 360f * (i * 0.6180339887f % 1f); // golden ratio
                r = MathF.Sqrt((float)i / particles.Length) * radius;
            }

            var dir = new Angle(ang).Direction;
            var point = new Point(position).MoveIn(dir, r);

            if (keepMovement == false)
                movement[key][i] = default;

            points[index][i] = (point.X, point.Y, points[index][i].color);
        }
    }
    public static void MakeRectangle(this (float x, float y, uint color)[] particles, (float x, float y, float width, float height) area, bool keepMovement = false, Distribution distribution = Distribution.FillEvenly)
    {
        var key = particles.GetHashCode();

        if (data.ContainsKey(key) == false || points.Contains(particles) == false)
            return;

        var index = points.IndexOf(particles);
        var (x, y, width, height) = area;

        for (var i = 0; i < particles.Length; i++)
        {
            var (px, py) = (particles[i].x, particles[i].y);

            if (distribution == Distribution.FillEvenly)
            {
                px = x + Halton(i + 1, 2) * width;
                py = y + Halton(i + 1, 3) * height;
            }
            else if (distribution == Distribution.FillRandomly)
            {
                px = (x, x + width).Random();
                py = (y, y + height).Random();
            }
            else if (distribution == Distribution.Outline)
            {
                var perimeter = 2 * (width + height);
                var progress = (float)i / particles.Length * perimeter;

                if (progress < width)
                {
                    px = x + progress;
                    py = y;
                }
                else if (progress < width + height)
                {
                    px = x + width;
                    py = y + (progress - width);
                }
                else if (progress < 2 * width + height)
                {
                    px = x + width - (progress - width - height);
                    py = y + height;
                }
                else
                {
                    px = x;
                    py = y + height - (progress - 2 * width - height);
                }
            }

            if (keepMovement == false)
                movement[key][i] = default;

            points[index][i] = (px, py, points[index][i].color);
        }

        float Halton(int ind, int baseValue)
        {
            var result = 0f;
            var f = 1f / baseValue;
            var i = ind;

            while (i > 0)
            {
                result += f * (i % baseValue);
                i /= baseValue;
                f /= baseValue;
            }

            return result;
        }
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
    public static void ApplyShake(this (float x, float y, uint color)[] particles, (float x, float y) strength)
    {
        if (data.TryGetValue(particles.GetHashCode(), out var cluster) && points.Contains(particles))
            cluster.shake = (strength.x / 100f, strength.y / 100f);
    }
    public static void ApplyBounceObstacles(this (float x, float y, uint color)[] particles, params (float x, float y, float width, float height)[] obstacles)
    {
        data[particles.GetHashCode()].bounceRects.Clear();
        data[particles.GetHashCode()].bounceRects.AddRange(obstacles);
    }
    public static void ApplyBounciness(this (float x, float y, uint color)[] particles, float strength)
    {
        var key = particles.GetHashCode();
        data[key].bounceStrength = strength;
    }

    public static void ApplyAge(this (float x, float y, uint color)[] particles, float ageSeconds)
    {
        data[particles.GetHashCode()].timeLeft = Math.Max(ageSeconds, 0f);
    }
    public static void ApplyTimeScale(this (float x, float y, uint color)[] particles, float timeScale)
    {
        data[particles.GetHashCode()].timeScale = timeScale;
    }
    public static void ApplySize(this (float x, float y, uint color)[] particles, float size)
    {
        data[particles.GetHashCode()].size = Math.Max(size / 2f, 0f);
    }
    public static void ApplyColor(this (float x, float y, uint color)[] particles, uint color, byte variety = 0)
    {
        var index = points.IndexOf(particles);
        var pts = points[index];
        var v = (int)variety;

        for (var i = 0; i < pts.Length; i++)
        {
            var (x, y, _) = pts[i];
            var c = new Color(color);
            var r = (byte)Math.Clamp(c.R + (-v, v).Random(), 0, 255);
            var g = (byte)Math.Clamp(c.G + (-v, v).Random(), 0, 255);
            var b = (byte)Math.Clamp(c.B + (-v, v).Random(), 0, 255);
            pts[i] = (x, y, new Color(r, g, b));
        }
    }
    public static void ApplyColorFade(this (float x, float y, uint color)[] particles, uint color, byte variety = 0, float duration = 1f)
    {
        var index = points.IndexOf(particles);
        var pts = points[index];
        var (tr, tg, tb, ta) = new Color(color).ToBundle();
        var copy = pts.ToArray();
        var key = particles.GetHashCode();
        var timeScale = data[key].timeScale;
        var v = (int)variety;

        Time.CallFor(duration / timeScale, progress =>
        {
            for (var i = 0; i < pts.Length; i++)
            {
                var (sr, sg, sb, sa) = new Color(copy[i].color).ToBundle();
                var r = (byte)progress.Map((0f, 1f), (sr, tr));
                var g = (byte)progress.Map((0f, 1f), (sg, tg));
                var b = (byte)progress.Map((0f, 1f), (sb, tb));
                var a = (byte)progress.Map((0f, 1f), (sa, ta));

                r = (byte)Math.Clamp(r + (-v, v).Random(key * i + 0), 0, 255);
                g = (byte)Math.Clamp(g + (-v, v).Random(key * i * 1), 0, 255);
                b = (byte)Math.Clamp(b + (-v, v).Random(key * i % 2), 0, 255);

                pts[i] = (pts[i].x, pts[i].y, new Color(r, g, b, a));
            }
        });
    }
    public static void ApplyWrap(this (float x, float y, uint color)[] particles, (float x, float y, float width, float height)? area)
    {
        data[particles.GetHashCode()].wrapArea = area;
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

                // gravity
                mx += cluster.gravity.x * dt;
                my += cluster.gravity.y * dt;

                // friction
                mx = mx.MoveTo(0f, cluster.friction, dt);
                my = my.MoveTo(0f, cluster.friction, dt);

                // orbit
                var dist = new Point(x, y).Distance(cluster.orbitPoint);
                if (dist < cluster.orbitRadius)
                {
                    var speed = -(cluster.orbitRadius - dist);
                    var dir = Angle.BetweenPoints(cluster.orbitPoint, (x, y)).Direction;
                    mx += dir.x * dt * speed;
                    my += dir.y * dt * speed;
                }

                x += (-cluster.shake.x, cluster.shake.x).Random();
                y += (-cluster.shake.y, cluster.shake.y).Random();

                if (cluster.wrapArea != null)
                {
                    var (wx, wy, ww, wh) = cluster.wrapArea ?? default;

                    x = x > wx + ww + sz ? wx - sz + x % (ww + sz) : x;
                    x = x < wx - sz ? wx + ww + sz + (x - wx) % (ww + sz) : x;
                    y = y > wy + wh + sz ? wy - sz + y % (wh + sz) : y;
                    y = y < wy - sz ? wy + wh + sz + (y - wy) % (wh + sz) : y;
                }

                // movement (should be before bounce & collision)
                x += mx * dt;
                y += my * dt;

                // bounce & collision (should be last)
                if (float.IsNaN(cluster.bounceStrength) == false)
                    foreach (var rect in rects)
                    {
                        var (left, right) = (rect.x - sz, rect.x + rect.w + sz);
                        var (top, bottom) = (rect.y - sz, rect.y + rect.h + sz);

                        if (x.IsBetween((left, right)) == false || y.IsBetween((top, bottom)) == false)
                            continue;

                        var (l, r) = (Math.Abs(x - left), Math.Abs(x - right));
                        var (t, b) = (Math.Abs(y - top), Math.Abs(y - bottom));
                        var (rx, ry, ang) = (mx, my, new Angle(mx, my));
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