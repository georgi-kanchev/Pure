using Pure.Engine.Execution;
using Pure.Engine.Utility;
using Particle = (float x, float y, uint color);
using Pos = (float x, float y);
using Force = (float x, float y);
using Area = (float x, float y, float width, float height);
using Line = (float ax, float ay, float bx, float by);
using Indices = (int particleIndex, int areaIndex);
using static Pure.Tools.Particles.Distribution;

namespace Pure.Tools.Particles;

public enum Distribution { FillEvenly, FillRandomly, Outline }

public static class Particles
{
    public static Particle[] SpawnCluster(int amount, float ageSeconds = float.PositiveInfinity)
    {
        var pts = new Particle[amount];
        var key = pts.GetHashCode();
        clustersData[key] = new(ageSeconds);
        movement[key] = new (float x, float y)[amount];
        clusters.Add(pts);

        for (var i = 0; i < amount; i++)
            pts[i] = (0f, 0f, Color.White);

        return pts;
    }

    public static bool IsClusterAlive(Particle[] cluster)
    {
        return clusters.Contains(cluster) && clustersData.ContainsKey(cluster.GetHashCode());
    }

    public static void MakeLine(this Particle[] cluster, Line line, bool keepMovement = false, Distribution distribution = FillEvenly)
    {
        var key = cluster.GetHashCode();

        if (clustersData.ContainsKey(key) == false || clusters.Contains(cluster) == false)
            return;

        var (a, b) = (new Point(line.ax, line.ay), new Point(line.bx, line.by));
        var index = clusters.IndexOf(cluster);
        var rand = distribution == FillRandomly;

        for (var i = 0; i < cluster.Length; i++)
        {
            if (keepMovement == false)
                movement[key][i] = default;

            var progress = rand ? (0f, 100f).Random() : (float)i / cluster.Length * 100f;
            var (x, y) = a.PercentTo(progress, b).XY;
            clusters[index][i] = (x, y, clusters[index][i].color);
        }
    }
    public static void MakeCircle(this Particle[] cluster, (float x, float y) position, float radius = 1f, bool keepMovement = false, Distribution distribution = FillEvenly)
    {
        var key = cluster.GetHashCode();

        if (clustersData.ContainsKey(key) == false || clusters.Contains(cluster) == false)
            return;

        var index = clusters.IndexOf(cluster);

        for (var i = 0; i < cluster.Length; i++)
        {
            var r = distribution == FillRandomly ? (0f, radius).Random() : radius;
            var ang = 360f * i / cluster.Length;

            if (distribution == FillEvenly)
            {
                ang = 360f * (i * 0.6180339887f % 1f); // golden ratio
                r = MathF.Sqrt((float)i / cluster.Length) * radius;
            }

            var dir = new Angle(ang).Direction;
            var p = new Point(position).MoveIn(dir, r);

            if (keepMovement == false)
                movement[key][i] = default;

            clusters[index][i] = (p.X, p.Y, clusters[index][i].color);
        }
    }
    public static void MakeRectangle(this Particle[] cluster, Area area, bool keepMovement = false, Distribution distribution = FillEvenly)
    {
        var key = cluster.GetHashCode();

        if (clustersData.ContainsKey(key) == false || clusters.Contains(cluster) == false)
            return;

        var index = clusters.IndexOf(cluster);
        var (x, y, width, height) = area;

        for (var i = 0; i < cluster.Length; i++)
        {
            var (px, py) = (cluster[i].x, cluster[i].y);

            if (distribution == FillEvenly)
            {
                px = x + Halton(i + 1, 2) * width;
                py = y + Halton(i + 1, 3) * height;
            }
            else if (distribution == FillRandomly)
            {
                px = (x, x + width).Random();
                py = (y, y + height).Random();
            }
            else if (distribution == Outline)
            {
                var perimeter = 2 * (width + height);
                var progress = (float)i / cluster.Length * perimeter;

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

            clusters[index][i] = (px, py, clusters[index][i].color);
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
    public static void MakeTeleport(this Particle[] cluster, Pos point, Force force, float interval, int burst = 1)
    {
        var hash = cluster.GetHashCode();
        var mov = movement[hash];
        var c = clustersData[hash];
        var differentInterval = c.teleportInterval.IsWithin(interval, 0.001f) == false;

        c.teleportPoint = point;
        c.teleportForce = force;
        c.teleportStep = burst;

        if (c.teleportTick == null)
        {
            c.teleportTick = Tick;
            c.teleportInterval = interval;
            c.teleportIndex = 0;
            Flow.CallAfter(0f, Tick);
            Flow.CallEvery(interval, c.teleportTick);
            return;
        }

        if (differentInterval)
        {
            Flow.CancelCall(c.teleportTick);
            c.teleportInterval = interval;
            c.teleportIndex = 0;
            Flow.CallAfter(0f, Tick);
            Flow.CallEvery(interval, c.teleportTick);
        }

        void Tick()
        {
            var st = c.teleportStep;

            for (var i = 0; i < st; i++)
            {
                var index = (c.teleportIndex + i).Wrap((0, cluster.Length));
                var f = c.teleportForce;
                var ang = new Angle(f) + (-c.varietyTeleportAngle, c.varietyTeleportAngle).Random();
                var str = new Point().Distance(f) + (0f, c.varietyTeleportForce).Random();
                var dir = new Angle(ang).Direction;
                var (x, y) = c.teleportPoint;

                cluster[index] = (x, y, cluster[index].color);
                mov[index] = (dir.x * str, dir.y * str);

                c.teleport?.Invoke(index);
            }

            c.teleportIndex += st;
        }
    }

    public static void ApplyGravity(this Particle[] cluster, Force force)
    {
        if (clustersData.TryGetValue(cluster.GetHashCode(), out var c) && clusters.Contains(cluster))
            c.gravity = force;
    }
    public static void ApplyFriction(this Particle[] cluster, float strength)
    {
        if (clustersData.TryGetValue(cluster.GetHashCode(), out var c) && clusters.Contains(cluster))
            c.friction = strength;
    }
    public static void ApplyOrbit(this Particle[] cluster, Pos point, float radius)
    {
        if (clustersData.TryGetValue(cluster.GetHashCode(), out var c) == false || clusters.Contains(cluster) == false)
            return;

        c.orbitPoint = point;
        c.orbitRadius = radius;
    }
    public static void ApplyShake(this Particle[] cluster, Force force)
    {
        if (clustersData.TryGetValue(cluster.GetHashCode(), out var c) && clusters.Contains(cluster))
            c.shake = (force.x / 100f, force.y / 100f);
    }
    public static void ApplyObstacles(this Particle[] cluster, params Area[] obstacles)
    {
        var hash = cluster.GetHashCode();
        clustersData[hash].obstacles.Clear();

        foreach (var obstacle in obstacles)
            clustersData[hash].obstacles.Add(obstacle);
    }
    public static void ApplyBounciness(this Particle[] cluster, float strength)
    {
        clustersData[cluster.GetHashCode()].bounceStrength = strength;
    }
    public static void ApplyTriggers(this Particle[] cluster, params Area[] triggers)
    {
        var hash = cluster.GetHashCode();
        clustersData[hash].triggers.Clear();

        foreach (var trigger in triggers)
            clustersData[hash].triggers.Add(trigger);
    }

    public static void ApplyAge(this Particle[] cluster, float seconds)
    {
        clustersData[cluster.GetHashCode()].timeLeft = Math.Max(seconds, 0f);
    }
    public static void ApplyTimeScale(this Particle[] cluster, float timeScale)
    {
        clustersData[cluster.GetHashCode()].timeScale = timeScale;
    }
    public static void ApplySize(this Particle[] cluster, float size)
    {
        clustersData[cluster.GetHashCode()].size = Math.Max(size / 2f, 0f);
    }
    public static void ApplyWrap(this Particle[] cluster, Area area)
    {
        clustersData[cluster.GetHashCode()].wrapArea = area;
    }

    public static void ApplyVarietyColor(this Particle[] cluster, float variety)
    {
        clustersData[cluster.GetHashCode()].varietyColor = variety;
    }
    public static void ApplyVarietyForce(this Particle[] cluster, float angle, float strength)
    {
        var hash = cluster.GetHashCode();
        clustersData[hash].varietyPushPullAngle = angle;
        clustersData[hash].varietyPushPullForce = strength;
    }
    public static void ApplyVarietyTeleport(this Particle[] cluster, float angle, float force)
    {
        var hash = cluster.GetHashCode();
        clustersData[hash].varietyTeleportAngle = angle;
        clustersData[hash].varietyTeleportForce = force;
    }

    public static void ForcePushFromPoint(this Particle[] cluster, Pos point, float radius, float strength, bool weakerFurther = true, bool blockedByObstacles = false, int affectedIndex = -1)
    {
        PushOrPull(true, cluster, point, strength, radius, weakerFurther, blockedByObstacles, affectedIndex);
    }
    public static void ForcePullToPoint(this Particle[] cluster, Pos point, float radius, float strength, bool weakerFurther = true, bool blockedByObstacles = false, int affectedIndex = -1)
    {
        PushOrPull(false, cluster, point, strength, radius, weakerFurther, blockedByObstacles, affectedIndex);
    }
    public static void ForcePushAtAngle(this Particle[] cluster, float angle, float strength, int affectedIndex = -1)
    {
        var key = cluster.GetHashCode();

        if (clustersData.TryGetValue(key, out _) == false || clusters.Contains(cluster) == false)
            return;

        var dir = new Angle(angle).Direction;
        var mov = movement[key];

        if (affectedIndex > -1)
        {
            mov[affectedIndex] = (mov[affectedIndex].x + dir.x * strength, mov[affectedIndex].y + dir.y * strength);
            return;
        }

        for (var i = 0; i < cluster.Length; i++)
            mov[i] = (mov[i].x + dir.x * strength, mov[i].y + dir.y * strength);
    }
    public static void ForceSet(this Particle[] cluster, Force force, int affectedIndex = -1)
    {
        var hash = cluster.GetHashCode();
        var mov = movement[hash];
        var c = clustersData[hash];

        if (affectedIndex > -1)
        {
            mov[affectedIndex] = force;
            return;
        }

        for (var i = 0; i < mov.Length; i++)
            mov[i] = force;
    }
    public static void ForceAccumulate(this Particle[] cluster, Force force, int affectedIndex = -1)
    {
        var mov = movement[cluster.GetHashCode()];

        if (affectedIndex > -1)
        {
            mov[affectedIndex] = (mov[affectedIndex].x + force.x, mov[affectedIndex].y + force.y);
            return;
        }

        for (var i = 0; i < mov.Length; i++)
            mov[i] = (mov[i].x + force.x, mov[i].y + force.y);
    }

    public static void FadeToColor(this Particle[] cluster, uint targetColor, float fadeDuration = 0f)
    {
        var index = clusters.IndexOf(cluster);
        var pts = clusters[index];
        var hash = pts.GetHashCode();
        var c = clustersData[hash];
        var color = new Color(targetColor);
        var v = c.varietyColor;
        color.R = (byte)(color.R + (-v, v).Random(hash.ToSeed(index, 0))).Limit((0, 255));
        color.G = (byte)(color.G + (-v, v).Random(hash.ToSeed(index, 1))).Limit((0, 255));
        color.B = (byte)(color.B + (-v, v).Random(hash.ToSeed(index, 2))).Limit((0, 255));
        color.A = (byte)(color.A + (-v, v).Random(hash.ToSeed(index, 3))).Limit((0, 255));

        for (var i = 0; i < pts.Length; i++)
            c.colorFades[i] = (fadeDuration, color, fadeDuration, pts[i].color);
    }

    public static void OnTrigger(this Particle[] cluster, Action<Indices> method)
    {
        clustersData[cluster.GetHashCode()].trigger = method;
    }
    public static void OnTriggerEnter(this Particle[] cluster, Action<Indices> method)
    {
        clustersData[cluster.GetHashCode()].triggerEnter = method;
    }
    public static void OnTriggerExit(this Particle[] cluster, Action<Indices> method)
    {
        clustersData[cluster.GetHashCode()].triggerExit = method;
    }
    public static void OnCollision(this Particle[] cluster, Action<Indices> method)
    {
        clustersData[cluster.GetHashCode()].collision = method;
    }
    public static void OnTeleport(this Particle[] cluster, Action<int> method)
    {
        clustersData[cluster.GetHashCode()].teleport = method;
    }

    public static void Update()
    {
        var delta = Time.Delta;

        for (var i = 0; i < clusters.Count; i++)
        {
            var pts = clusters[i];
            var key = pts.GetHashCode();
            var c = clustersData[key];
            var dt = delta * c.timeScale;
            var sz = Math.Max(c.size, 0.01f);
            var bs = Math.Max(c.bounceStrength, 0);
            var obstacles = c.obstacles.ToBundle();
            var triggers = c.triggers.ToBundle();

            c.timeLeft -= dt;

            for (var j = 0; j < pts.Length; j++)
            {
                // color fade
                if (c.colorFades.TryGetValue(j, out var fade))
                {
                    var from = new Color(fade.fromColor);
                    var to = new Color(fade.toColor);
                    pts[j].color = new Color(
                        (byte)fade.timeLeft.Map((fade.duration, 0), (from.R, to.R)),
                        (byte)fade.timeLeft.Map((fade.duration, 0), (from.G, to.G)),
                        (byte)fade.timeLeft.Map((fade.duration, 0), (from.B, to.B)),
                        (byte)fade.timeLeft.Map((fade.duration, 0), (from.A, to.A)));

                    fade.timeLeft -= delta;
                    c.colorFades[j] = fade;

                    if (fade.timeLeft < 0f)
                    {
                        pts[j].color = fade.toColor;
                        c.colorFades.Remove(j);
                    }
                }

                // gravity
                movement[key][j].x += c.gravity.x * dt;
                movement[key][j].y += c.gravity.y * dt;

                // friction
                movement[key][j].x = movement[key][j].x.MoveTo(0f, c.friction, dt);
                movement[key][j].y = movement[key][j].y.MoveTo(0f, c.friction, dt);

                // orbit
                var dist = new Point(pts[j].x, pts[j].y).Distance(c.orbitPoint);
                if (dist < c.orbitRadius)
                {
                    var speed = -(c.orbitRadius - dist);
                    var dir = Angle.BetweenPoints(c.orbitPoint, (pts[j].x, pts[j].y)).Direction;
                    movement[key][j].x += dir.x * dt * speed;
                    movement[key][j].y += dir.y * dt * speed;
                }

                // shake
                pts[j].x += (-c.shake.x, c.shake.x).Random();
                pts[j].y += (-c.shake.y, c.shake.y).Random();

                // wrap
                if (c.wrapArea != null)
                {
                    var (wx, wy, ww, wh) = c.wrapArea ?? default; // never null
                    pts[j].x = pts[j].x.Wrap((wx - sz, wx + ww + sz));
                    pts[j].y = pts[j].y.Wrap((wy - sz, wy + wh + sz));
                }

                // movement (should be before bounce & collision)
                pts[j].x += movement[key][j].x * dt;
                pts[j].y += movement[key][j].y * dt;

                // triggers
                var triggerIndex = -1;
                for (var t = 0; t < triggers.Length; t++)
                {
                    var rect = triggers[t];
                    var (left, right) = (rect.x - sz, rect.x + rect.width + sz);
                    var (top, bottom) = (rect.y - sz, rect.y + rect.height + sz);

                    if (pts[j].x.IsBetween((left, right)) == false || pts[j].y.IsBetween((top, bottom)) == false)
                        continue;

                    triggerIndex = t;
                    c.trigger?.Invoke((j, triggerIndex));
                    break;
                }

                if (triggerIndex > -1 && c.triggering.TryAdd(j, triggerIndex))
                    c.triggerEnter?.Invoke((j, triggerIndex));

                if (triggerIndex == -1 && c.triggering.Remove(j, out triggerIndex))
                    c.triggerExit?.Invoke((j, triggerIndex));

                // bounce & collision (should be last)
                for (var o = 0; o < obstacles.Length; o++)
                {
                    var rect = obstacles[o];
                    var (left, right) = (rect.x - sz, rect.x + rect.width + sz);
                    var (top, bottom) = (rect.y - sz, rect.y + rect.height + sz);

                    if (pts[j].x.IsBetween((left, right)) == false ||
                        pts[j].y.IsBetween((top, bottom)) == false)
                        continue;

                    var (l, r) = (Math.Abs(pts[j].x - left), Math.Abs(pts[j].x - right));
                    var (t, b) = (Math.Abs(pts[j].y - top), Math.Abs(pts[j].y - bottom));
                    var (mx, my) = movement[key][j];
                    var (rx, ry, ang) = (mx, my, new Angle(mx, my));
                    var speed = new Point(mx, my).Distance((0f, 0f));

                    if (l < r && l < t && l < b)
                    {
                        (rx, ry) = ang.Reflect(Angle.Left).Direction;
                        pts[j].x = left - sz;
                        c.collision?.Invoke((j, o));
                    }
                    else if (r < l && r < t && r < b)
                    {
                        (rx, ry) = ang.Reflect(Angle.Right).Direction;
                        pts[j].x = right + sz;
                        c.collision?.Invoke((j, o));
                    }
                    else if (t < l && t < r && t < b)
                    {
                        (rx, ry) = ang.Reflect(Angle.Up).Direction;
                        pts[j].y = top - sz;
                        c.collision?.Invoke((j, o));
                    }
                    else if (b < t && b < l && b < r)
                    {
                        (rx, ry) = ang.Reflect(Angle.Down).Direction;
                        pts[j].y = bottom + sz;
                        c.collision?.Invoke((j, o));
                    }

                    movement[key][j].x = rx * speed;
                    movement[key][j].y = ry * speed;
                    movement[key][j].x *= bs;
                    movement[key][j].y *= bs;
                }
            }

            if (c.timeLeft > 0f)
                continue;

            for (var j = 0; j < pts.Length; j++)
                pts[j] = default;

            if (c.teleportTick != null)
                Flow.CancelCall(c.teleportTick);

            clustersData.Remove(key);
            movement.Remove(key);
            clusters.Remove(pts);
            i--;
        }
    }

#region Backend
    private static readonly Dictionary<int, ClusterData> clustersData = [];
    private static readonly Dictionary<int, Force[]> movement = [];
    private static readonly List<Particle[]> clusters = [];

    private static void PushOrPull(bool push, Particle[] cluster, Pos point, float radius, float force, bool weakerFurther, bool blockedByObstacles, int affectedIndex)
    {
        if (clustersData.TryGetValue(cluster.GetHashCode(), out var c) == false || clusters.Contains(cluster) == false)
            return;

        var key = cluster.GetHashCode();
        var index = clusters.IndexOf(cluster);
        var pts = clusters[index];
        var mov = movement[key];

        for (var i = 0; i < cluster.Length; i++)
        {
            var (x, y, _) = pts[i];
            var dist = new Point(x, y).Distance(point);

            if (dist > radius)
                continue;

            var (mx, my) = mov[i];
            var f = force + (-c.varietyPushPullForce, c.varietyPushPullForce).Random();
            var speed = weakerFurther ? dist.Map((0f, radius), (f, 0f)) : f;
            var ang = Angle.BetweenPoints((x, y), point);

            if (blockedByObstacles && c.obstacles.IsOverlapping(new Pure.Engine.Collision.Line((x, y), point)))
                continue;

            ang += (-c.varietyPushPullAngle, c.varietyPushPullAngle).Random();
            speed *= push ? -1 : 1;

            var dir = ang.Direction;
            movement[key][i] = (mx + dir.x * speed, my + dir.y * speed);
        }
    }
#endregion
}