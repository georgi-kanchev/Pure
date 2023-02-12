namespace Pure.Utilities;

public struct Angle
{
	public static Angle NaN => float.NaN;
	public bool IsNaN => float.IsNaN(Value);

	public float Radians => MathF.PI / 180f * Value;

	public Angle(float degrees)
	{
		value = degrees;
	}

	/// <summary>
	/// Rotates the <see cref="Angle"/> with <paramref name="speed"/>. The result is then returned.
	/// </summary>
	public Angle Rotate(float speed, float deltaTime = 1)
	{
		return this + speed * deltaTime;
	}
	/// <summary>
	/// Rotates the <see cref="Angle"/> toward a <paramref name="targetDegrees"/> with <paramref name="speed"/>
	/// taking the fastest route. The calculation ensures to stop exactly at the <paramref name="targetDegrees"/>. The result is then returned.
	/// </summary>
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

	public static Angle FromPoints((float, float) point, (float, float) targetPoint)
	{
		var dir = (targetPoint.Item1 - point.Item1, targetPoint.Item2 - point.Item2);
		var m = MathF.Sqrt(dir.Item1 * dir.Item1 + dir.Item2 * dir.Item2);

		return (dir.Item1 / m, dir.Item2 / m);
	}
	public static float FromRadians(float radians)
	{
		return radians * (180f / MathF.PI);
	}

	public static implicit operator Angle((int, int) direction)
	{
		var result = MathF.Atan2(direction.Item2, direction.Item1) * (180f / MathF.PI);
		return new() { Value = result };
	}
	public static implicit operator (int, int)(Angle angle)
	{
		var rad = MathF.PI / 180 * angle;
		return ((int)MathF.Round(MathF.Cos(rad)), (int)MathF.Round(MathF.Sin(rad)));
	}
	public static implicit operator Angle((float, float) direction)
	{
		var result = MathF.Atan2(direction.Item2, direction.Item1) * (180f / MathF.PI);
		return new() { Value = result };
	}
	public static implicit operator (float, float)(Angle angle)
	{
		var rad = MathF.PI / 180 * angle;
		return (MathF.Cos(rad), MathF.Sin(rad));
	}
	public static implicit operator Angle(int degrees)
	{
		return new() { Value = degrees };
	}
	public static implicit operator int(Angle angle)
	{
		return (int)MathF.Round(angle.value);
	}
	public static implicit operator Angle(float degrees)
	{
		return new() { Value = degrees };
	}
	public static implicit operator float(Angle angle)
	{
		return angle.value;
	}

	public override string ToString()
	{
		return Value.ToString() + "°";
	}

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
