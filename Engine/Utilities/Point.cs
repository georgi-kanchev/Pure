namespace Pure.Engine.Utilities;

public enum NoiseType { OpenSimplex2, OpenSimplex2S, Cellular, Perlin, ValueCubic, Value };

public struct Point
{
    public static Point NaN
    {
        get => new(float.NaN);
    }

    public float X
    {
        get => val.Item1;
        set => val = (value, val.Item2);
    }
    public float Y
    {
        get => val.Item2;
        set => val = (val.Item1, value);
    }
    public uint Color
    {
        get;
        set;
    }

    public bool IsNaN
    {
        get => float.IsNaN(X) || float.IsNaN(Y);
    }

    public Point(float x, float y, uint color = uint.MaxValue)
    {
        val = (x, y);
        Color = color;
        X = x;
        Y = y;
    }
    public Point(float xy, uint color = uint.MaxValue)
        : this(xy, xy, color)
    {
    }
    public Point((float x, float y) position, uint color = uint.MaxValue)
        : this(position.x, position.y, color)
    {
    }
    public Point(((float x, float y) position, uint color) bundle)
        : this(bundle.position.x, bundle.position.y, bundle.color)
    {
    }

    public Point ToGrid(Point gridSize)
    {
        if (gridSize == default)
            return this;

        // this prevents -0 cells
        var x = X - (X < 0 ? gridSize.X : 0);
        var y = Y - (Y < 0 ? gridSize.Y : 0);

        x -= X % gridSize.X;
        y -= Y % gridSize.Y;
        return new(x, y);
    }
    public Point MoveIn((float x, float y) direction, float speed, float deltaTime = 1)
    {
        if (direction == default)
            return this;

        // normalize
        var x = direction.Item1;
        var y = direction.Item2;
        var m = MathF.Sqrt(x * x + y * y);
        x /= m;
        y /= m;

        var resultX = X + x * speed * deltaTime;
        var resultY = Y + y * speed * deltaTime;
        return new(resultX, resultY);
    }
    public Point MoveAt(float angle, float speed, float deltaTime = 1)
    {
        // angle to dir
        angle = Wrap(angle, 360);
        var rad = MathF.PI / 180 * angle;
        var dir = (MathF.Cos(rad), MathF.Sin(rad));

        return MoveIn(dir, speed, deltaTime);
    }
    public Point MoveTo(Point targetPoint, float speed, float deltaTime = 1)
    {
        if (targetPoint == this)
            return this;

        var result = MoveIn(targetPoint - this, speed, deltaTime);

        speed *= deltaTime;
        return result.Distance(targetPoint) < speed * 1.1f ? targetPoint : result;
    }
    public Point ToTarget(Point targetPoint, (float x, float y) unit)
    {
        var x = Map(unit.Item1, 0, 1, X, targetPoint.X);
        var y = Map(unit.Item2, 0, 1, Y, targetPoint.Y);
        return new(x, y);
    }
    public float ToNoise(NoiseType type = NoiseType.Perlin, float scale = 10f, int seed = 0)
    {
        var noise = new FastNoiseLite(seed);
        noise.SetNoiseType((FastNoiseLite.NoiseType)type);
        noise.SetFrequency(1f / scale);

        return noise.GetNoise(X, Y).Map((-1, 1), (0, 1));
    }
    public float Distance(Point targetPoint)
    {
        var distX = targetPoint.X - X;
        var distY = targetPoint.Y - Y;
        return MathF.Sqrt(distX * distX + distY * distY);
    }
    public float Angle(Point targetPoint)
    {
        return Wrap(ToAngle(targetPoint - this), 360);
    }
    public (float x, float y) Direction(Point targetPoint)
    {
        var dir = (targetPoint - this);
        var x = dir.X;
        var y = dir.Y;
        var m = MathF.Sqrt(x * x + y * y);
        x /= m;
        y /= m;
        return (x, y);
    }

    public ((float x, float y) position, uint color) ToBundle()
    {
        return this;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }
    public override string ToString()
    {
        return val.ToString();
    }

    public static implicit operator Point((int x, int y) position)
    {
        return new(position.Item1, position.Item2);
    }
    public static implicit operator (int x, int y)(Point point)
    {
        return ((int)MathF.Round(point.val.Item1), (int)MathF.Round(point.val.Item2));
    }
    public static implicit operator Point((float x, float y) position)
    {
        return new(position.Item1, position.Item2);
    }
    public static implicit operator (float x, float y)(Point point)
    {
        return point.val;
    }
    public static implicit operator Point(((float x, float y) position, uint color) bundle)
    {
        return new(bundle);
    }
    public static implicit operator ((float x, float y) position, uint color)(Point point)
    {
        return point.ToBundle();
    }

    public static Point operator +(Point a, Point b)
    {
        return new(a.X + b.X, a.Y + b.Y);
    }
    public static Point operator -(Point a, Point b)
    {
        return new(a.X - b.X, a.Y - b.Y);
    }
    public static Point operator *(Point a, Point b)
    {
        return new(a.X * b.X, a.Y * b.Y);
    }
    public static Point operator /(Point a, Point b)
    {
        return new(a.X / b.X, a.Y / b.Y);
    }
    public static Point operator +(Point a, float b)
    {
        return new(a.X + b, a.Y + b);
    }
    public static Point operator -(Point a, float b)
    {
        return new(a.X - b, a.Y - b);
    }
    public static Point operator *(Point a, float b)
    {
        return new(a.X * b, a.Y * b);
    }
    public static Point operator /(Point a, float b)
    {
        return new(a.X / b, a.Y / b);
    }
    public static bool operator ==(Point a, Point b)
    {
        return a.val == b.val;
    }
    public static bool operator !=(Point a, Point b)
    {
        return a.val != b.val;
    }

#region Backend
    private (float, float) val;

    private static float ToAngle((float, float) direction)
    {
        return (MathF.Atan2(direction.Item2, direction.Item1) * (180f / MathF.PI)).Wrap(360);
    }
    private static float Wrap(float number, float range)
    {
        return ((number % range) + range) % range;
    }
    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
#endregion
}