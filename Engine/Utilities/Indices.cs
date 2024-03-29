﻿using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Pure.Engine.Utilities;

/// <summary>
/// Represents a 2D integer coordinate with X and Y components. Usually used for 2D arrays.
/// </summary>
public struct Indices
{
    /// <summary>
    /// Gets or sets the X component of this coordinate.
    /// </summary>
    public int X
    {
        get => val.x;
        set => val = (value, val.y);
    }
    /// <summary>
    /// Gets or sets the Y component of this coordinate.
    /// </summary>
    public int Y
    {
        get => val.y;
        set => val = (val.x, value);
    }

    /// <summary>
    /// Creates a new indices instance with the given value for both X and Y components.
    /// </summary>
    /// <param name="xy">The value for both X and Y components.</param>
    public Indices(int xy)
    {
        val = (xy, xy);
        X = xy;
        Y = xy;
    }
    /// <summary>
    /// Creates a new indices instance with the given values for the X and Y components.
    /// </summary>
    /// <param name="x">The value for the X component.</param>
    /// <param name="y">The value for the Y component.</param>
    public Indices(int x, int y)
    {
        val = (x, y);
        X = x;
        Y = y;
    }
    public Indices(byte[] bytes)
    {
        var b = Decompress(bytes);
        var offset = 0;

        val = (BitConverter.ToInt32(Get<int>()), BitConverter.ToInt32(Get<int>()));

        byte[] Get<T>()
        {
            return GetBytesFrom(b, Marshal.SizeOf(typeof(T)), ref offset);
        }
    }
    public Indices(string base64) : this(Convert.FromBase64String(base64))
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
    /// A bundle tuple containing the X and Y components of this 2D coordinate.</returns>
    public (int x, int y) ToBundle()
    {
        return this;
    }
    /// <returns>
    /// A string that represents these index coordinates.</returns>
    public override string ToString()
    {
        return val.ToString();
    }

    /// <summary>
    /// Calculates the 1D index for this 2D coordinate given the width of the 2D array.
    /// </summary>
    /// <param name="width">The width of the 2D array.</param>
    /// <returns>The calculated 1D index.</returns>
    public int ToIndex(int width)
    {
        return X * width + Y;
    }
    /// <summary>
    /// Creates a new indices instance from the given 1D index and size of the 2D array.
    /// </summary>
    /// <param name="index">The 1D index to convert to a 2D coordinate.</param>
    /// <param name="size">The size of the 2D array.</param>
    /// <returns>A new indices instance representing the 2D coordinate corresponding 
    /// to the given index.</returns>
    public static Indices FromIndex(int index, (int width, int height) size)
    {
        index = index < 0 ? 0 : index;
        index = index > size.width * size.height - 1 ? size.width * size.height - 1 : index;

        return (index % size.width, index / size.width);
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
    /// Implicitly converts a bundle tuple of two integers to an indices instance.
    /// </summary>
    /// <param name="bundle">The bundle tuple of two integers to convert.</param>
    /// <returns>A new indices instance with the same X and Y components as the bundle tuple.</returns>
    public static implicit operator Indices((int x, int y) bundle)
    {
        return new Indices(bundle.x, bundle.y);
    }
    /// <summary>
    /// Implicitly converts an indices instance to a bundle tuple of two integers.
    /// </summary>
    /// <param name="vector">The indices instance to convert.</param>
    /// <returns>A new bundle tuple of two integers with the same values as the X and Y
    /// components of the indices instance.</returns>
    public static implicit operator (int x, int y)(Indices vector)
    {
        return (vector.val.x, vector.val.y);
    }
    /// <summary>
    /// Implicitly converts a bundle tuple of two floats to an indices instance.
    /// </summary>
    /// <param name="bundle">The bundle tuple of two floats to convert.</param>
    /// <returns>A new indices instance with the same X and Y components as the bundle tuple.</returns>
    public static implicit operator Indices((float x, float y) bundle)
    {
        return new Indices((int)bundle.x, (int)bundle.y);
    }
    /// <summary>
    /// Implicitly converts an indices instance to a bundle tuple of two floats.
    /// </summary>
    /// <param name="vector">The indices instance to convert.</param>
    /// <returns>A new bundle tuple of two floats with the same values as the X and Y
    /// components of the indices instance.</returns>
    public static implicit operator (float x, float y)(Indices vector)
    {
        return vector.val;
    }
    public static implicit operator byte[](Indices point)
    {
        return point.ToBytes();
    }
    public static implicit operator Indices(byte[] bytes)
    {
        return new(bytes);
    }

    /// <summary>
    /// Adds two indices instances together component-wise.
    /// </summary>
    /// <param name="a">The first indices instance.</param>
    /// <param name="b">The second indices instance.</param>
    /// <returns>The sum of the two indices instances.</returns>
    public static Indices operator +(Indices a, Indices b)
    {
        return new(a.X + b.X, a.Y + b.Y);
    }
    /// <summary>
    /// Subtracts two indices instances component-wise.
    /// </summary>
    /// <param name="a">The first indices instances.</param>
    /// <param name="b">The second indices instances.</param>
    /// <returns>The difference of the two indices instances.</returns>
    public static Indices operator -(Indices a, Indices b)
    {
        return new(a.X - b.X, a.Y - b.Y);
    }
    /// <summary>
    /// Multiplies two indices instances component-wise.
    /// </summary>
    /// <param name="a">The first indices instances.</param>
    /// <param name="b">The second indices instances.</param>
    /// <returns>The product of the two indices instances.</returns>
    public static Indices operator *(Indices a, Indices b)
    {
        return new(a.X * b.X, a.Y * b.Y);
    }
    /// <summary>
    /// Divides two indices instances component-wise.
    /// </summary>
    /// <param name="a">The first indices instances.</param>
    /// <param name="b">The second indices instances.</param>
    /// <returns>The result of the division of the two indices instances.</returns>
    public static Indices operator /(Indices a, Indices b)
    {
        return new(a.X / b.X, a.Y / b.Y);
    }
    /// <summary>
    /// Adds a scalar value to each component of an indices instance.
    /// </summary>
    /// <param name="a">The indices instance.</param>
    /// <param name="b">The scalar value.</param>
    /// <returns>The result of adding the scalar value to each component of the indices instance.</returns>
    public static Indices operator +(Indices a, int b)
    {
        return new(a.X + b, a.Y + b);
    }
    /// <summary>
    /// Subtracts a scalar value from each component of an indices instance.
    /// </summary>
    /// <param name="a">The indices instance.</param>
    /// <param name="b">The scalar value.</param>
    /// <returns>The result of subtracting the scalar value from each component of 
    /// the indices instance.</returns>
    public static Indices operator -(Indices a, int b)
    {
        return new(a.X - b, a.Y - b);
    }
    /// <summary>
    /// Multiplies each component of an indices instance by a scalar value.
    /// </summary>
    /// <param name="a">The indices instance.</param>
    /// <param name="b">The scalar value.</param>
    /// <returns>The result of multiplying each component of the indices instance by the 
    /// scalar value.</returns>
    public static Indices operator *(Indices a, int b)
    {
        return new(a.X * b, a.Y * b);
    }
    /// <summary>
    /// Divides each component of an indices instance by a scalar value.
    /// </summary>
    /// <param name="a">The indices instance.</param>
    /// <param name="b">The scalar value.</param>
    /// <returns>The result of dividing each component of the indices instance by the scalar value.</returns>
    public static Indices operator /(Indices a, int b)
    {
        return new(a.X / b, a.Y / b);
    }

    /// <summary>
    /// Determines whether two indices instance are equal component-wise.
    /// </summary>
    /// <param name="a">The first indices instance.</param>
    /// <param name="b">The second indices instance.</param>
    /// <returns>True if the two indices instance are equal component-wise, false otherwise.</returns>
    public static bool operator ==(Indices a, Indices b)
    {
        return a.val == b.val;
    }
    /// <summary>
    /// Determines whether two indices instance are different component-wise.
    /// </summary>
    /// <param name="a">The first indices instance.</param>
    /// <param name="b">The second indices instance.</param>
    /// <returns>True if the two indices instance are different component-wise, false otherwise.</returns>
    public static bool operator !=(Indices a, Indices b)
    {
        return a.val != b.val;
    }

#region Backend
    private (int x, int y) val;

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