namespace Pure.Engine.Utility;

[Flags]
public enum Behavior
{
    Gravity = 1 << 0,
    Bounce = 2 << 0,
    SlowDown = 3 << 0,
    FadeIn = 4 << 0,
    FadeOut = 5 << 0
}

public static class Particles
{
    public static (float x, float y) ForceGravity { get; set; } = (0, 0.5f);
    public static (float x, float y) ForceSlowDown { get; set; } = (1f, 1f);
    public static (float time, Behavior behavior)[] BehaviorChain { get; set; }

    public static void SpawnInCircle(int amount, (float x, float y) position, float scale = 1f)
    {
        var angStep = 360f / amount;
        for (var i = 0; i < amount; i++)
        {
            var point = new Point(position);
            var dir = new Angle(angStep * i).Direction;
            point = point.MoveIn(dir, (0f, scale).Random(5f));
            particles.Add(new(point, BehaviorChain) { Movement = new(dir.x, dir.y), Color = Color.White });
        }
    }
    public static void SpawnInLine(int amount, (float ax, float ay, float bx, float by) line)
    {
        var (a, b) = (new Point(line.ax, line.ay), new Point(line.bx, line.by));
        var dir = new Angle(Angle.BetweenPoints(a.XY, b.XY) - 90f);
        var step = a.Distance(b) / amount;
        var p = a;
        for (var i = 0; i < amount; i++)
        {
            particles.Add(new(p, BehaviorChain) { Movement = dir, Color = Color.White });
            p = p.MoveTo(b, step);
        }
    }

    public static (float x, float y, uint color)[] ToBundle()
    {
        var result = new (float x, float y, uint color)[particles.Count];
        for (var i = 0; i < particles.Count; i++)
            result[i] = (particles[i].Position.x, particles[i].Position.y, particles[i].Color);

        return result;
    }

    #region Backend
    private static readonly List<Particle> particles = [];

    internal static void Update()
    {
        var dt = Time.Delta;
        foreach (var p in particles)
        {
            var (x, y) = p.Position;
            var (mx, my) = p.Movement;

            // if (p.Behavior.HasFlag(Behavior.Gravity))
            // {
            //     mx += ForceGravity.x * dt;
            //     my += ForceGravity.y * dt;
            // }

            // if (p.Behavior.HasFlag(Behavior.SlowDown))
            // {
            //     mx = mx.MoveTo(0f, ForceSlowDown.x, dt);
            //     my = my.MoveTo(0f, ForceSlowDown.y, dt);
            // }

            // x += mx * dt;
            // y += my * dt;

            p.Age += dt;
            p.Position = (x, y);
            p.Movement = new(mx, my);
        }
    }
    #endregion
}