namespace Pure.Utilities;

/// <summary>
/// Represents an angle in degrees (ranged and wrapped 0 to 360).
/// </summary>
public struct Angle
{
	/// <summary>
	/// Returns a value that represents a NaN (not a number) angle.
	/// </summary>
	public static Angle NaN => float.NaN;
	/// <summary>
	/// Gets a value indicating whether the angle is NaN (not a number).
	/// </summary>
	public bool IsNaN => float.IsNaN(Value);

	/// <summary>
	/// Initializes a new angle instance with the specified value in degrees.
	/// </summary>
	/// <param name="degrees">The value in degrees.</param>
	public Angle(float degrees)
	{
		value = degrees;
	}

	/// <summary>
	/// Rotates the angle by the specified speed and delta time.
	/// </summary>
	/// <param name="speed">The rotation speed.</param>
	/// <param name="deltaTime">The delta time.</param>
	/// <returns>The rotated angle.</returns>
	public Angle Rotate(float speed, float deltaTime = 1)
	{
		return this + speed * deltaTime;
	}
	/// <summary>
	/// Rotates the angle to the specified target angle by the specified speed and delta time.
	/// </summary>
	/// <param name="targetDegrees">The target angle in degrees.</param>
	/// <param name="speed">The rotation speed.</param>
	/// <param name="deltaTime">The delta time.</param>
	/// <returns>The rotated angle.</returns>
	public Angle RotateTo(Angle targetDegrees, float speed, float deltaTime = 1)
	{
		speed = Math.Abs(speed);
		var angle = this;
		var difference = angle - targetDegrees;

		// stops the rotation with an else when close enough
		// prevents the rotation from staying behind after the stop
		var checkedSpeed = speed;
		checkedSpeed *= deltaTime;
		if (Math.Abs(difference) < checkedSpeed) angle = targetDegrees;
		else if (difference >= 0 && difference < 180) angle = angle.Rotate(-speed, deltaTime);
		else if (difference >= -180 && difference < 0) angle = angle.Rotate(speed, deltaTime);
		else if (difference >= -360 && difference < -180) angle = angle.Rotate(-speed, deltaTime);
		else if (difference >= 180 && difference < 360) angle = angle.Rotate(speed, deltaTime);

		// detects speed greater than possible
		// prevents jiggle when passing 0-360 & 360-0 | simple to fix yet took me half a day
		if (Math.Abs(difference) > 360 - checkedSpeed)
			angle = targetDegrees;

		return angle;
	}

	/// <summary>
	/// Calculates the angle between two points.
	/// </summary>
	/// <param name="point">The starting point.</param>
	/// <param name="targetPoint">The target point.</param>
	/// <returns>The angle between the two points.</returns>
	public static Angle FromPoints((float x, float y) point, (float x, float y) targetPoint)
	{
		var dir = (targetPoint.Item1 - point.Item1, targetPoint.Item2 - point.Item2);
		var m = MathF.Sqrt(dir.Item1 * dir.Item1 + dir.Item2 * dir.Item2);

		return (dir.Item1 / m, dir.Item2 / m);
	}
	public static float FromRadians(float radians)
	{
		return radians * (180f / MathF.PI);
	}

	/// <summary>
	/// Converts the angle from degrees to radians.
	/// </summary>
	/// <returns>The angle in radians.</returns>
	public float ToRadians() => MathF.PI / 180f * Value;
	/// <returns>
	/// A string that represents the current angle.</returns>
	public override string ToString()
	{
		return Value.ToString() + "°";
	}

	/// <summary>
	/// Implicitly converts a tuple of two integers representing a direction vector into an angle.
	/// </summary>
	/// <param name="direction">The direction vector to convert.</param>
	/// <returns>The angle instance representing the direction vector.</returns>
	public static implicit operator Angle((int x, int y) direction)
	{
		var result = MathF.Atan2(direction.Item2, direction.Item1) * (180f / MathF.PI);
		return new() { Value = result };
	}
	/// <summary>
	/// Implicitly converts an angle into a tuple of two integers representing a direction vector.
	/// </summary>
	/// <param name="angle">The angle instance to convert.</param>
	/// <returns>The direction vector representing the angle.</returns>
	public static implicit operator (int, int)(Angle angle)
	{
		var rad = MathF.PI / 180 * angle;
		return ((int)MathF.Round(MathF.Cos(rad)), (int)MathF.Round(MathF.Sin(rad)));
	}
	/// <summary>
	/// Implicitly converts a tuple of two floats representing a direction vector into an angle.
	/// </summary>
	/// <param name="direction">The direction vector to convert.</param>
	/// <returns>The angle instance representing the direction vector.</returns>
	public static implicit operator Angle((float x, float y) direction)
	{
		var result = MathF.Atan2(direction.Item2, direction.Item1) * (180f / MathF.PI);
		return new() { Value = result };
	}
	/// <summary>
	/// Implicitly converts an angle into a tuple of two floats representing a direction vector.
	/// </summary>
	/// <param name="angle">The angle instance to convert.</param>
	/// <returns>The direction vector representing the angle.</returns>
	public static implicit operator (float x, float y)(Angle angle)
	{
		var rad = MathF.PI / 180 * angle;
		return (MathF.Cos(rad), MathF.Sin(rad));
	}
	/// <summary>
	/// Implicitly converts an integer representing an angle in degrees into an angle.
	/// </summary>
	/// <param name="degrees">The angle in degrees to convert.</param>
	/// <returns>The angle instance representing the angle.</returns>
	public static implicit operator Angle(int degrees)
	{
		return new() { Value = degrees };
	}
	/// <summary>
	/// Implicitly converts an angle into an integer representing the angle in degrees.
	/// </summary>
	/// <param name="angle">The angle instance to convert.</param>
	/// <returns>The angle in degrees.</returns>
	public static implicit operator int(Angle angle)
	{
		return (int)MathF.Round(angle.value);
	}
	/// <summary>
	/// Implicitly converts a float representing an angle in degrees into an angle.
	/// </summary>
	/// <param name="degrees">The angle in degrees to convert.</param>
	/// <returns>The angle instance representing the angle.</returns>
	public static implicit operator Angle(float degrees)
	{
		return new() { Value = degrees };
	}
	/// <summary>
	/// Implicitly converts an angle into a float representing the angle in degrees.
	/// </summary>
	/// <param name="angle">The angle instance to convert.</param>
	/// <returns>The angle in degrees.</returns>
	public static implicit operator float(Angle angle)
	{
		return angle.value;
	}

	// no need of operator overloading since it implicitly casts to int/float

	#region Backend
	private float Value
	{
		get => value;
		set { this.value = value; Wrap(); }
	}
	private float value;

	private void Wrap()
	{
		value = ((value % 360) + 360) % 360;
	}
	#endregion
}
