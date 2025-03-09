﻿using PointF = (float x, float y);
using Direction = (float x, float y);
using PointI = (int x, int y);
using Bundle = (float x, float y, uint color);

namespace Pure.Engine.Utility;

public struct Point : IEquatable<Point>
{
    public static Point NaN
    {
        get => new(float.NaN);
    }

    /// <summary>
    /// X = 0<br></br>
    /// Y = 0<br></br>
    /// Color = White
    /// </summary>
    public static Point Zero
    {
        get => default;
    }
    /// <summary>
    /// X = 1<br></br>
    /// Y = 1<br></br>
    /// Color = White
    /// </summary>
    public static Point One
    {
        get => new(1);
    }

    public PointF XY
    {
        get => (X, Y);
        set => val = (value.x, value.y);
    }
    public float X
    {
        get => val.x;
        set => val = (value, val.y);
    }
    public float Y
    {
        get => val.y;
        set => val = (val.x, value);
    }
    public uint Color { get; set; }

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
    public Point(float xy, uint color = uint.MaxValue) : this(xy, xy, color)
    {
    }
    public Point(Bundle bundle) : this(bundle.x, bundle.y, bundle.color)
    {
    }
    public Point(PointF position, uint color = uint.MaxValue) : this(position.x, position.y, color)
    {
    }
    public Point(byte[] bytes)
    {
        var offset = 0;

        val = (BitConverter.ToSingle(GetBytesFrom(bytes, 4, ref offset)),
            BitConverter.ToSingle(GetBytesFrom(bytes, 4, ref offset)));
        Color = BitConverter.ToUInt32(GetBytesFrom(bytes, 4, ref offset));
    }
    public Point(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public byte[] ToBytes()
    {
        var result = new List<byte>();

        result.AddRange(BitConverter.GetBytes(X));
        result.AddRange(BitConverter.GetBytes(Y));
        result.AddRange(BitConverter.GetBytes(Color));

        return result.ToArray();
    }
    public Bundle ToBundle()
    {
        return (X, Y, Color);
    }
    public override string ToString()
    {
        return val.ToString();
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
    public Point MoveIn(Direction direction, float speed, float deltaTime = 1)
    {
        if (direction == default)
            return this;

        // normalize
        var x = direction.x;
        var y = direction.y;
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
    public Point MoveTo(Point target, float speed, float deltaTime = 1)
    {
        if (target == this)
            return this;

        var result = MoveIn(target - this, speed, deltaTime);

        speed *= deltaTime;
        return result.Distance(target) < speed * 0.51f ? target : result;
    }
    public Point PercentTo(float percent, Point target)
    {
        var x = Map(percent, 0, 100, X, target.X);
        var y = Map(percent, 0, 100, Y, target.Y);
        return new(x, y);
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
    public PointF Direction(Point targetPoint)
    {
        var dir = targetPoint - this;
        var x = dir.X;
        var y = dir.Y;
        var m = MathF.Sqrt(x * x + y * y);
        x /= m;
        y /= m;
        return (x, y);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }
    public bool Equals(Point other)
    {
        return val.Equals(other.val) && Color == other.Color;
    }

    public static Point? Average(params Point[]? points)
    {
        if (points == null || points.Length == 0)
            return null;

        var sumX = 0f;
        var sumY = 0f;

        foreach (var p in points)
        {
            sumX += p.X;
            sumY += p.Y;
        }

        return new Point(sumX / points.Length, sumY / points.Length);
    }

    public static implicit operator Point(PointI position)
    {
        return new(position.x, position.y);
    }
    public static implicit operator PointI(Point point)
    {
        return ((int)MathF.Round(point.val.x), (int)MathF.Round(point.val.y));
    }
    public static implicit operator Point(PointF position)
    {
        return new(position);
    }
    public static implicit operator PointF(Point point)
    {
        return point.val;
    }
    public static implicit operator Point(Bundle bundle)
    {
        return new(bundle);
    }
    public static implicit operator Bundle(Point point)
    {
        return point.ToBundle();
    }
    public static implicit operator byte[](Point point)
    {
        return point.ToBytes();
    }
    public static implicit operator Point(byte[] bytes)
    {
        return new(bytes);
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
    private PointF val;

    private static float ToAngle(PointF direction)
    {
        return (MathF.Atan2(direction.y, direction.x) * (180f / MathF.PI)).Wrap(360);
    }
    private static float Wrap(float number, float range)
    {
        return (number % range + range) % range;
    }
    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }

    private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}