﻿namespace Pure.Engine.Utilities;

using System.IO.Compression;
using System.Runtime.InteropServices;

public enum NoiseType
{
    OpenSimplex2, OpenSimplex2S, Cellular, Perlin, ValueCubic, Value
};

public struct Point
{
    public static Point NaN
    {
        get => new(float.NaN);
    }
    public static Point Zero
    {
        get => new();
    }
    public static Point One
    {
        get => new();
    }

    public (float x, float y) XY
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
    public Point((float x, float y, uint color) bundle) : this(bundle.x, bundle.y, bundle.color)
    {
    }
    public Point((float x, float y) position, uint color = uint.MaxValue)
        : this(position.x, position.y, color)
    {
    }
    public Point(byte[] bytes)
    {
        var b = Decompress(bytes);
        var offset = 0;

        val = (BitConverter.ToSingle(Get<float>()), BitConverter.ToSingle(Get<float>()));
        Color = BitConverter.ToUInt32(Get<uint>());

        byte[] Get<T>()
        {
            return GetBytesFrom(b, Marshal.SizeOf(typeof(T)), ref offset);
        }
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

        return Compress(result.ToArray());
    }
    public (float x, float y, uint color) ToBundle()
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
    public Point MoveIn((float x, float y) direction, float speed, float deltaTime = 1)
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
        return result.Distance(target) < speed * 1.1f ? target : result;
    }
    public Point PercentTo(float percent, Point target)
    {
        var x = Map(percent, 0, 100, X, target.X);
        var y = Map(percent, 0, 100, Y, target.Y);
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

    public static implicit operator Point((int x, int y) position)
    {
        return new(position.x, position.y);
    }
    public static implicit operator (int x, int y)(Point point)
    {
        return ((int)MathF.Round(point.val.x), (int)MathF.Round(point.val.y));
    }
    public static implicit operator Point((float x, float y) position)
    {
        return new(position);
    }
    public static implicit operator (float x, float y)(Point point)
    {
        return point.val;
    }
    public static implicit operator Point((float x, float y, uint color) bundle)
    {
        return new(bundle);
    }
    public static implicit operator (float x, float y, uint color)(Point point)
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
    private (float x, float y) val;

    private static float ToAngle((float x, float y) direction)
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

    private static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
        {
            stream.Write(data, 0, data.Length);
        }

        return output.ToArray();
    }
    private static byte[] Decompress(byte[] data)
    {
        var input = new MemoryStream(data);
        var output = new MemoryStream();
        using var stream = new DeflateStream(input, CompressionMode.Decompress);
        stream.CopyTo(output);
        return output.ToArray();
    }
    private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}