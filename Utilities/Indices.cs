namespace Pure.Utilities;

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
		get => val.Item1;
		set => val = (value, val.Item2);
	}
	/// <summary>
	/// Gets or sets the Y component of this coordinate.
	/// </summary>
	public int Y
	{
		get => val.Item2;
		set => val = (val.Item1, value);
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
		index = index > size.Item1 * size.Item2 - 1 ? size.Item1 * size.Item2 - 1 : index;

		return (index % size.Item1, index / size.Item1);
	}

	/// <returns>
	/// A bundle tuple containing the X and Y components of this 2D coordinate.</returns>
	public (int x, int y) ToBundle() => this;

	public override int GetHashCode() => base.GetHashCode();
	public override bool Equals(object? obj) => base.Equals(obj);
	/// <returns>
	/// A string that represents these index coordinates.</returns>
	public override string ToString() => val.ToString();

	/// <summary>
	/// Implicitly converts a bundle tuple of two integers to an indices instance.
	/// </summary>
	/// <param name="bundle">The bundle tuple of two integers to convert.</param>
	/// <returns>A new indices instance with the same X and Y components as the bundle tuple.</returns>
	public static implicit operator Indices((int x, int y) bundle)
	{
		return new Indices(bundle.Item1, bundle.Item2);
	}
	/// <summary>
	/// Implicitly converts an indices instance to a bundle tuple of two integers.
	/// </summary>
	/// <param name="vector">The indices instance to convert.</param>
	/// <returns>A new bundle tuple of two integers with the same values as the X and Y
	/// components of the indices instance.</returns>
	public static implicit operator (int x, int y)(Indices vector)
	{
		return (vector.val.Item1, vector.val.Item2);
	}
	/// <summary>
	/// Implicitly converts a bundle tuple of two floats to an indices instance.
	/// </summary>
	/// <param name="bundle">The bundle tuple of two floats to convert.</param>
	/// <returns>A new indices instance with the same X and Y components as the bundle tuple.</returns>
	public static implicit operator Indices((float x, float y) bundle)
	{
		return new Indices((int)bundle.Item1, (int)bundle.Item2);
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

	/// <summary>
	/// Adds two indices instances together component-wise.
	/// </summary>
	/// <param name="a">The first indices instance.</param>
	/// <param name="b">The second indices instance.</param>
	/// <returns>The sum of the two indices instances.</returns>
	public static Indices operator +(Indices a, Indices b) => new(a.X + b.X, a.Y + b.Y);
	/// <summary>
	/// Subtracts two indices instances component-wise.
	/// </summary>
	/// <param name="a">The first indices instances.</param>
	/// <param name="b">The second indices instances.</param>
	/// <returns>The difference of the two indices instances.</returns>
	public static Indices operator -(Indices a, Indices b) => new(a.X - b.X, a.Y - b.Y);
	/// <summary>
	/// Multiplies two indices instances component-wise.
	/// </summary>
	/// <param name="a">The first indices instances.</param>
	/// <param name="b">The second indices instances.</param>
	/// <returns>The product of the two indices instances.</returns>
	public static Indices operator *(Indices a, Indices b) => new(a.X * b.X, a.Y * b.Y);
	/// <summary>
	/// Divides two indices instances component-wise.
	/// </summary>
	/// <param name="a">The first indices instances.</param>
	/// <param name="b">The second indices instances.</param>
	/// <returns>The result of the division of the two indices instances.</returns>
	public static Indices operator /(Indices a, Indices b) => new(a.X / b.X, a.Y / b.Y);
	/// <summary>
	/// Adds a scalar value to each component of an indices instance.
	/// </summary>
	/// <param name="a">The indices instance.</param>
	/// <param name="b">The scalar value.</param>
	/// <returns>The result of adding the scalar value to each component of the indices instance.</returns>
	public static Indices operator +(Indices a, int b) => new(a.X + b, a.Y + b);
	/// <summary>
	/// Subtracts a scalar value from each component of an indices instance.
	/// </summary>
	/// <param name="a">The indices instance.</param>
	/// <param name="b">The scalar value.</param>
	/// <returns>The result of subtracting the scalar value from each component of 
	/// the indices instance.</returns>
	public static Indices operator -(Indices a, int b) => new(a.X - b, a.Y - b);
	/// <summary>
	/// Multiplies each component of an indices instance by a scalar value.
	/// </summary>
	/// <param name="a">The indices instance.</param>
	/// <param name="b">The scalar value.</param>
	/// <returns>The result of multiplying each component of the indices instance by the 
	/// scalar value.</returns>
	public static Indices operator *(Indices a, int b) => new(a.X * b, a.Y * b);
	/// <summary>
	/// Divides each component of an indices instance by a scalar value.
	/// </summary>
	/// <param name="a">The indices instance.</param>
	/// <param name="b">The scalar value.</param>
	/// <returns>The result of dividing each component of the indices instance by the scalar value.</returns>
	public static Indices operator /(Indices a, int b) => new(a.X / b, a.Y / b);

	/// <summary>
	/// Determines whether two indices instance are equal component-wise.
	/// </summary>
	/// <param name="a">The first indices instance.</param>
	/// <param name="b">The second indices instance.</param>
	/// <returns>True if the two indices instance are equal component-wise, false otherwise.</returns>
	public static bool operator ==(Indices a, Indices b) => a.val == b.val;
	/// <summary>
	/// Determines whether two indices instance are different component-wise.
	/// </summary>
	/// <param name="a">The first indices instance.</param>
	/// <param name="b">The second indices instance.</param>
	/// <returns>True if the two indices instance are different component-wise, false otherwise.</returns>
	public static bool operator !=(Indices a, Indices b) => a.val != b.val;

	#region Backend
	private (int, int) val;
	#endregion
}
