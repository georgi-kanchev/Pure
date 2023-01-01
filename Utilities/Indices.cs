namespace Pure.Utilities
{
	public struct Indices
	{
		public int X
		{
			get => val.Item1;
			set => val = (value, val.Item2);
		}
		public int Y
		{
			get => val.Item2;
			set => val = (val.Item1, value);
		}

		public Indices(int xy)
		{
			val = (xy, xy);
			X = xy;
			Y = xy;
		}
		public Indices(int x, int y)
		{
			val = (x, y);
			X = x;
			Y = y;
		}

		public int ToIndex(int width)
		{
			return X * width + Y;
		}
		public static Indices FromIndex(int index, (int, int) size)
		{
			index = index < 0 ? 0 : index;
			index = index > size.Item1 * size.Item2 - 1 ? size.Item1 * size.Item2 - 1 : index;

			return (index % size.Item1, index / size.Item1);
		}

		public static implicit operator Indices((int, int) value)
		{
			return new Indices(value.Item1, value.Item2);
		}
		public static implicit operator (int, int)(Indices vector)
		{
			return (vector.val.Item1, vector.val.Item2);
		}
		public static implicit operator Indices((float, float) value)
		{
			return new Indices((int)value.Item1, (int)value.Item2);
		}
		public static implicit operator (float, float)(Indices vector)
		{
			return vector.val;
		}

		public static Indices operator +(Indices a, Indices b) => new(a.X + b.X, a.Y + b.Y);
		public static Indices operator -(Indices a, Indices b) => new(a.X - b.X, a.Y - b.Y);
		public static Indices operator *(Indices a, Indices b) => new(a.X * b.X, a.Y * b.Y);
		public static Indices operator /(Indices a, Indices b) => new(a.X / b.X, a.Y / b.Y);
		public static Indices operator +(Indices a, int b) => new(a.X + b, a.Y + b);
		public static Indices operator -(Indices a, int b) => new(a.X - b, a.Y - b);
		public static Indices operator *(Indices a, int b) => new(a.X * b, a.Y * b);
		public static Indices operator /(Indices a, int b) => new(a.X / b, a.Y / b);
		public static Indices operator +(int a, Indices b) => new(b.X + a, b.Y + a);
		public static Indices operator -(int a, Indices b) => new(b.X - a, b.Y - a);
		public static Indices operator *(int a, Indices b) => new(b.X * a, b.Y * a);
		public static Indices operator /(int a, Indices b) => new(b.X / a, b.Y / a);
		public static bool operator ==(Indices a, Indices b) => a.val == b.val;
		public static bool operator !=(Indices a, Indices b) => a.val != b.val;

		public override int GetHashCode() => base.GetHashCode();
		public override bool Equals(object? obj) => base.Equals(obj);
		public override string ToString()
		{
			return val.ToString();
		}

		#region Backend
		private (int, int) val;
		#endregion
	}
}
