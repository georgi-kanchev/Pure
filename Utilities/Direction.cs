namespace Purity.Utilities
{
	public struct Direction
	{
		public static Direction NaN => new(float.NaN);

		public float X
		{
			get => x;
			set { x = value; Normalize(); }
		}
		public float Y
		{
			get => y;
			set { y = value; Normalize(); }
		}
		public (float, float) Value
		{
			get => (x, y);
			set { x = value.Item1; y = value.Item2; Normalize(); }
		}

		public bool IsNaN => float.IsNaN(X) || float.IsNaN(Y);

		public Direction(float xy)
		{
			x = xy;
			y = xy;
			Normalize();
		}
		public Direction(float x, float y)
		{
			this.x = x;
			this.y = y;
			Normalize();
		}

		public float Dot(Direction targetVector)
		{
			return X * targetVector.X + Y * targetVector.Y;
		}
		public Direction Reflect(Direction surfaceNormal)
		{
			return this - 2f * Dot(surfaceNormal) * surfaceNormal;
		}

		public static Direction FromPoints((float, float) point, (float, float) targetPoint)
		{
			var px = point.Item1;
			var py = point.Item2;
			var tpx = targetPoint.Item1;
			var tpy = targetPoint.Item2;

			return new Direction(tpx - px, tpy - py);
		}

		public static implicit operator Direction(int angle)
		{
			var rad = MathF.PI / 180 * angle;
			return new(MathF.Cos(rad), MathF.Sin(rad));
		}
		public static implicit operator int(Direction direction)
		{
			var result = MathF.Atan2(direction.Y, direction.X) * (180f / MathF.PI);
			var wrap = ((result % 360) + 360) % 360;
			return (int)wrap;
		}
		public static implicit operator Direction(float angle)
		{
			var rad = MathF.PI / 180 * angle;
			return new(MathF.Cos(rad), MathF.Sin(rad));
		}
		public static implicit operator float(Direction direction)
		{
			var result = MathF.Atan2(direction.Y, direction.X) * (180f / MathF.PI);
			var wrap = ((result % 360) + 360) % 360;
			return wrap;
		}
		public static implicit operator Direction((int, int) value)
		{
			return new Direction(value.Item1, value.Item2);
		}
		public static implicit operator (int, int)(Direction direction)
		{
			return ((int)direction.Value.Item1, (int)direction.Value.Item2);
		}
		public static implicit operator Direction((float, float) value)
		{
			return new Direction(value.Item1, value.Item2);
		}
		public static implicit operator (float, float)(Direction direction)
		{
			return direction.Value;
		}

		public static Direction operator +(Direction a, Direction b) => new(a.X + b.X, a.Y + b.Y);
		public static Direction operator -(Direction a, Direction b) => new(a.X - b.X, a.Y - b.Y);
		public static Direction operator *(Direction a, Direction b) => new(a.X * b.X, a.Y * b.Y);
		public static Direction operator /(Direction a, Direction b) => new(a.X / b.X, a.Y / b.Y);
		public static Direction operator +(Direction a, float b) => new(a.X + b, a.Y + b);
		public static Direction operator -(Direction a, float b) => new(a.X - b, a.Y - b);
		public static Direction operator *(Direction a, float b) => new(a.X * b, a.Y * b);
		public static Direction operator /(Direction a, float b) => new(a.X / b, a.Y / b);
		public static Direction operator +(float a, Direction b) => new(b.X + a, b.Y + a);
		public static Direction operator -(float a, Direction b) => new(b.X - a, b.Y - a);
		public static Direction operator *(float a, Direction b) => new(b.X * a, b.Y * a);
		public static Direction operator /(float a, Direction b) => new(b.X / a, b.Y / a);
		public static bool operator ==(Direction a, Direction b) => a.Value == b.Value;
		public static bool operator !=(Direction a, Direction b) => a.Value != b.Value;

		public override int GetHashCode() => base.GetHashCode();
		public override bool Equals(object? obj) => base.Equals(obj);
		public override string ToString()
		{
			return $"{X} {Y}";
		}

		#region Backend
		private float x, y;
		private void Normalize()
		{
			var m = MathF.Sqrt(x * x + y * y);
			x /= m;
			y /= m;
		}
		#endregion
	}
}
