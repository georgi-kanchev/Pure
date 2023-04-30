namespace Pure.Utilities;

/// <summary>
/// Represents a 2D direction in space with an x and y component.
/// </summary>
public struct Direction
{
	/// <summary>
	/// A direction instance representing an invalid direction.
	/// </summary>
	public static Direction NaN => new(float.NaN);

	/// <summary>
	/// Gets or sets the X component of the direction.
	/// </summary>
	public float X
	{
		get => x;
		set { x = value; Normalize(); }
	}
	/// <summary>
	/// Gets or sets the Y component of the direction.
	/// </summary>
	public float Y
	{
		get => y;
		set { y = value; Normalize(); }
	}

	/// <summary>
	/// Determines whether the direction is invalid.
	/// </summary>
	public bool IsNaN => float.IsNaN(X) || float.IsNaN(Y);

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
	/// <summary>
	/// Initializes a new direction instance with the same X and Y components.
	/// </summary>
	/// <param name="xy">The value to use for both the X and Y components of the direction.</param>
	public Direction(float xy) : this(xy, xy) { }
	/// <summary>
	/// Initializes a new direction instance with the X and Y components specified as a bundle tuple.
	/// </summary>
	/// <param name="bundle">A bundle tuple containing the X and Y components of the direction.</param>
	public Direction((float x, float y) bundle) : this(bundle.x, bundle.y) { }

	/// <summary>
	/// Calculates the dot product between this direction and another direction.
	/// </summary>
	/// <param name="targetVector">The direction to calculate the dot product with.</param>
	/// <returns>The dot product of the two directions.</returns>
	public float Dot(Direction targetVector)
	{
		return X * targetVector.X + Y * targetVector.Y;
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

	/// <returns>
	/// A bundle tuple containing the X and Y components of the direction.</returns>
	public (float x, float y) ToBundle() => this;

	public override int GetHashCode() => base.GetHashCode();
	public override bool Equals(object? obj) => base.Equals(obj);
	/// <returns>
	/// A string that represents this direction.</returns>
	public override string ToString() => Value.ToString();

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
		var rad = MathF.PI / 180 * angle;
		return new(MathF.Cos(rad), MathF.Sin(rad));
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

	/// <summary>
	/// Adds two direction instance component-wise.
	/// </summary>
	/// <param name="a">The first direction instance to add.</param>
	/// <param name="b">The second direction instance to add.</param>
	/// <returns>The resulting direction instance after the addition.</returns>
	public static Direction operator +(Direction a, Direction b) => new(a.X + b.X, a.Y + b.Y);
	/// <summary>
	/// Subtracts two direction instance component-wise.
	/// </summary>
	/// <param name="a">The direction instance to subtract from.</param>
	/// <param name="b">The direction instance to subtract.</param>
	/// <returns>The resulting direction instance after the subtraction.</returns>
	public static Direction operator -(Direction a, Direction b) => new(a.X - b.X, a.Y - b.Y);
	/// <summary>
	/// Multiplies two direction instance component-wise.
	/// </summary>
	/// <param name="a">The first direction instance to multiply.</param>
	/// <param name="b">The second direction instance to multiply.</param>
	/// <returns>The resulting direction instance after the multiplication.</returns>
	public static Direction operator *(Direction a, Direction b) => new(a.X * b.X, a.Y * b.Y);
	/// <summary>
	/// Divides two direction instance component-wise.
	/// </summary>
	/// <param name="a">The direction instance to divide.</param>
	/// <param name="b">The direction instance to divide by.</param>
	/// <returns>The resulting direction instance after the division.</returns>
	public static Direction operator /(Direction a, Direction b) => new(a.X / b.X, a.Y / b.Y);
	/// <summary>
	/// Adds a scalar value to a direction instance's components.
	/// </summary>
	/// <param name="a">The direction instance to add to.</param>
	/// <param name="b">The scalar value to add.</param>
	/// <returns>The resulting direction instance after the addition.</returns>
	public static Direction operator +(Direction a, float b) => new(a.X + b, a.Y + b);
	/// <summary>
	/// Subtracts a scalar value from a direction instance's components.
	/// </summary>
	/// <param name="a">The direction instance to subtract from.</param>
	/// <param name="b">The scalar value to subtract.</param>
	/// <returns>The resulting Direction after the subtraction.</returns>
	public static Direction operator -(Direction a, float b) => new(a.X - b, a.Y - b);
	/// <summary>
	/// Multiplies a direction instance's components by a scalar value.
	/// </summary>
	/// <param name="a">The direction instance to multiply.</param>
	/// <param name="b">The scalar value to multiply by.</param>
	/// <returns>The resulting direction instance after the multiplication.</returns>
	public static Direction operator *(Direction a, float b) => new(a.X * b, a.Y * b);
	/// <summary>
	/// Divides a direction instance's components by a scalar value.
	/// </summary>
	/// <param name="a">The direction instance to divide.</param>
	/// <param name="b">The scalar value to divide by.</param>
	/// <returns>The resulting direction instance after the division.</returns>
	public static Direction operator /(Direction a, float b) => new(a.X / b, a.Y / b);

	/// <summary>
	/// Determines whether two directions have the same X and Y values.
	/// </summary>
	/// <param name="a">The first direction.</param>
	/// <param name="b">The second direction.</param>
	/// <returns>True if the directions have the same X and Y values, otherwise false.</returns>
	public static bool operator ==(Direction a, Direction b) => a.Value == b.Value;
	/// <summary>
	/// Determines whether two directions have different X and Y values.
	/// </summary>
	/// <param name="a">The first direction.</param>
	/// <param name="b">The second direction.</param>
	/// <returns>True if the directions have different X and Y values, otherwise false.</returns>
	public static bool operator !=(Direction a, Direction b) => a.Value != b.Value;

	#region Backend
	private float x, y;
	private (float, float) Value
	{
		get => (x, y);
		set { x = value.Item1; y = value.Item2; Normalize(); }
	}

	private void Normalize()
	{
		var m = MathF.Sqrt(x * x + y * y);
		x /= m;
		y /= m;
	}
	#endregion
}
