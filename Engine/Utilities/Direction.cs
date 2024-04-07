using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Pure.Engine.Utilities;

/// <summary>
/// Represents a 2D direction in space with an x and y component.
/// </summary>
public struct Direction
{
    public static Direction Up
    {
        get => new(0, -1);
    }
    public static Direction Down
    {
        get => new(0, 1);
    }
    public static Direction Left
    {
        get => new(-1, 0);
    }
    public static Direction Right
    {
        get => new(1, 0);
    }

    /// <summary>
    /// A direction instance representing an invalid direction.
    /// </summary>
    public static Direction NaN
    {
        get => new(float.NaN);
    }

    public (float x, float y) XY
    {
        get => (x, y);
        set => Value = (value.x, value.y);
    }
    /// <summary>
    /// Gets or sets the X component of the direction.
    /// </summary>
    public float X
    {
        get => x;
        set
        {
            x = value;
            Normalize();
        }
    }
    /// <summary>
    /// Gets or sets the Y component of the direction.
    /// </summary>
    public float Y
    {
        get => y;
        set
        {
            y = value;
            Normalize();
        }
    }

    /// <summary>
    /// Determines whether the direction is invalid.
    /// </summary>
    public bool IsNaN
    {
        get => float.IsNaN(X) || float.IsNaN(Y);
    }

    /// <summary>
    /// Initializes a new direction instance with the specified X and Y components.
    /// </summary>
    /// <param name="x">The X component of the direction.</param>
    /// <param name="y">The Y component of the direction.</param>
    public Direction(float x, float y)
    {
        this.x = x;
        this.y = y;
        Normalize();
    }
    public Direction(float angle)
    {
        var rad = MathF.PI / 180 * angle;
        x = MathF.Cos(rad);
        y = MathF.Sin(rad);
    }
    /// <summary>
    /// Initializes a new direction instance with the X and Y components specified as a bundle tuple.
    /// </summary>
    /// <param name="bundle">A bundle tuple containing the X and Y components of the direction.</param>
    public Direction((float x, float y) bundle) : this(bundle.x, bundle.y)
    {
    }
    public Direction(byte[] bytes)
    {
        var b = Decompress(bytes);
        var offset = 0;

        x = BitConverter.ToSingle(Get<float>());
        y = BitConverter.ToSingle(Get<float>());

        byte[] Get<T>()
        {
            return GetBytesFrom(b, Marshal.SizeOf(typeof(T)), ref offset);
        }
    }
    public Direction(string base64) : this(Convert.FromBase64String(base64))
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

        return Compress(result.ToArray());
    }
    /// <returns>
    /// A bundle tuple containing the X and Y components of the direction.</returns>
    public (float x, float y) ToBundle()
    {
        return this;
    }
    /// <returns>
    /// A string that represents this direction.</returns>
    public override string ToString()
    {
        return Value.ToString();
    }

    /// <summary>
    /// Calculates the dot product between this direction and another direction.
    /// </summary>
    /// <param name="target">The direction to calculate the dot product with.</param>
    /// <returns>The dot product of the two directions.</returns>
    public float Dot(Direction target)
    {
        return X * target.X + Y * target.Y;
    }
    /// <summary>
    /// Reflects this direction from a given surface normal.
    /// </summary>
    /// <param name="surfaceNormal">The normal of the surface to reflect the
    /// direction from.</param>
    /// <returns>A new direction reflecting this direction from the surface normal.</returns>
    public Direction Reflect(Direction surfaceNormal)
    {
        return this - 2f * Dot(surfaceNormal) * surfaceNormal;
    }
    public Direction Reverse()
    {
        return (-X, -Y);
    }

    /// <summary>
    /// Calculates a direction instance representing the direction between two points.
    /// </summary>
    /// <param name="point">The starting point.</param>
    /// <param name="targetPoint">The target point.</param>
    /// <returns>A direction instance representing the direction from the starting 
    /// point to the target point.</returns>
    public static Direction FromPoints((float x, float y) point, (float x, float y) targetPoint)
    {
        var px = point.Item1;
        var py = point.Item2;
        var tpx = targetPoint.Item1;
        var tpy = targetPoint.Item2;

        return new Direction(tpx - px, tpy - py);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    /// <summary>
    /// Implicitly converts an integer angle to a direction instance.
    /// </summary>
    /// <param name="angle">The angle in degrees.</param>
    /// <returns>A new direction instance representing the angle.</returns>
    public static implicit operator Direction(int angle)
    {
        var rad = MathF.PI / 180 * angle;
        return new(MathF.Cos(rad), MathF.Sin(rad));
    }
    /// <summary>
    /// Implicitly converts a direction instance to an integer angle.
    /// </summary>
    /// <param name="direction">The direction instance to convert.</param>
    /// <returns>The angle in degrees representing the direction.</returns>
    public static implicit operator int(Direction direction)
    {
        var result = MathF.Atan2(direction.Y, direction.X) * (180f / MathF.PI);
        var wrap = ((result % 360) + 360) % 360;
        return (int)wrap;
    }
    /// <summary>
    /// Implicitly converts a float angle to a direction instance.
    /// </summary>
    /// <param name="angle">The angle in degrees.</param>
    /// <returns>A new direction instance representing the angle.</returns>
    public static implicit operator Direction(float angle)
    {
        return new(angle);
    }
    /// <summary>
    /// Implicitly converts a direction instance to a float angle.
    /// </summary>
    /// <param name="direction">The direction instance to convert.</param>
    /// <returns>The angle in degrees representing the direction.</returns>
    public static implicit operator float(Direction direction)
    {
        var result = MathF.Atan2(direction.Y, direction.X) * (180f / MathF.PI);
        var wrap = ((result % 360) + 360) % 360;
        return wrap;
    }
    /// <summary>
    /// Implicitly converts a bundle tuple of integers to a direction instance.
    /// </summary>
    /// <param name="bundle">The bundle tuple to convert.</param>
    /// <returns>The direction instance representing the bundle.</returns>
    public static implicit operator Direction((int x, int y) bundle)
    {
        return new Direction(bundle.Item1, bundle.Item2);
    }
    /// <summary>
    /// Implicitly converts a direction instance to a bundle tuple of integers.
    /// </summary>
    /// <param name="direction">The direction instance to convert.</param>
    /// <returns>The bundle tuple representing the direction.</returns>
    public static implicit operator (int x, int y)(Direction direction)
    {
        return ((int)MathF.Round(direction.Value.Item1), (int)MathF.Round(direction.Value.Item2));
    }
    /// <summary>
    /// Implicitly converts a bundle tuple of floats to a direction instance.
    /// </summary>
    /// <param name="bundle">The bundle tuple to convert.</param>
    /// <returns>The direction instance representing the bundle.</returns>
    public static implicit operator Direction((float x, float y) bundle)
    {
        return new Direction(bundle.Item1, bundle.Item2);
    }
    /// <summary>
    /// Implicitly converts a direction instance to a bundle tuple of floats.
    /// </summary>
    /// <param name="direction">The direction instance to convert.</param>
    /// <returns>The bundle tuple representing the direction.</returns>
    public static implicit operator (float x, float y)(Direction direction)
    {
        return direction.Value;
    }
    public static implicit operator byte[](Direction point)
    {
        return point.ToBytes();
    }
    public static implicit operator Direction(byte[] bytes)
    {
        return new(bytes);
    }

    /// <summary>
    /// Adds two direction instance component-wise.
    /// </summary>
    /// <param name="a">The first direction instance to add.</param>
    /// <param name="b">The second direction instance to add.</param>
    /// <returns>The resulting direction instance after the addition.</returns>
    public static Direction operator +(Direction a, Direction b)
    {
        return new(a.X + b.X, a.Y + b.Y);
    }
    /// <summary>
    /// Subtracts two direction instance component-wise.
    /// </summary>
    /// <param name="a">The direction instance to subtract from.</param>
    /// <param name="b">The direction instance to subtract.</param>
    /// <returns>The resulting direction instance after the subtraction.</returns>
    public static Direction operator -(Direction a, Direction b)
    {
        return new(a.X - b.X, a.Y - b.Y);
    }
    /// <summary>
    /// Multiplies two direction instance component-wise.
    /// </summary>
    /// <param name="a">The first direction instance to multiply.</param>
    /// <param name="b">The second direction instance to multiply.</param>
    /// <returns>The resulting direction instance after the multiplication.</returns>
    public static Direction operator *(Direction a, Direction b)
    {
        return new(a.X * b.X, a.Y * b.Y);
    }
    /// <summary>
    /// Divides two direction instance component-wise.
    /// </summary>
    /// <param name="a">The direction instance to divide.</param>
    /// <param name="b">The direction instance to divide by.</param>
    /// <returns>The resulting direction instance after the division.</returns>
    public static Direction operator /(Direction a, Direction b)
    {
        return new(a.X / b.X, a.Y / b.Y);
    }
    /// <summary>
    /// Adds a scalar value to a direction instance's components.
    /// </summary>
    /// <param name="a">The direction instance to add to.</param>
    /// <param name="b">The scalar value to add.</param>
    /// <returns>The resulting direction instance after the addition.</returns>
    public static Direction operator +(Direction a, float b)
    {
        return new(a.X + b, a.Y + b);
    }
    /// <summary>
    /// Subtracts a scalar value from a direction instance's components.
    /// </summary>
    /// <param name="a">The direction instance to subtract from.</param>
    /// <param name="b">The scalar value to subtract.</param>
    /// <returns>The resulting Direction after the subtraction.</returns>
    public static Direction operator -(Direction a, float b)
    {
        return new(a.X - b, a.Y - b);
    }
    /// <summary>
    /// Multiplies a direction instance's components by a scalar value.
    /// </summary>
    /// <param name="a">The direction instance to multiply.</param>
    /// <param name="b">The scalar value to multiply by.</param>
    /// <returns>The resulting direction instance after the multiplication.</returns>
    public static Direction operator *(Direction a, float b)
    {
        return new(a.X * b, a.Y * b);
    }
    /// <summary>
    /// Divides a direction instance's components by a scalar value.
    /// </summary>
    /// <param name="a">The direction instance to divide.</param>
    /// <param name="b">The scalar value to divide by.</param>
    /// <returns>The resulting direction instance after the division.</returns>
    public static Direction operator /(Direction a, float b)
    {
        return new(a.X / b, a.Y / b);
    }

    /// <summary>
    /// Determines whether two directions have the same X and Y values.
    /// </summary>
    /// <param name="a">The first direction.</param>
    /// <param name="b">The second direction.</param>
    /// <returns>True if the directions have the same X and Y values, otherwise false.</returns>
    public static bool operator ==(Direction a, Direction b)
    {
        return a.Value == b.Value;
    }
    /// <summary>
    /// Determines whether two directions have different X and Y values.
    /// </summary>
    /// <param name="a">The first direction.</param>
    /// <param name="b">The second direction.</param>
    /// <returns>True if the directions have different X and Y values, otherwise false.</returns>
    public static bool operator !=(Direction a, Direction b)
    {
        return a.Value != b.Value;
    }

#region Backend
    private float x, y;
    private (float, float) Value
    {
        get => (x, y);
        set
        {
            x = value.Item1;
            y = value.Item2;
            Normalize();
        }
    }

    private void Normalize()
    {
        var m = MathF.Sqrt(x * x + y * y);
        x /= m;
        y /= m;
    }

    private static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
            stream.Write(data, 0, data.Length);
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