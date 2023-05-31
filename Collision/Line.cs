namespace Pure.Collision;

using System.Numerics;

/// <summary>
/// Represents a line segment in 2D space defined by two points. Useful for
/// collision detection, debugging, raycasting and many other things.
/// </summary>
public struct Line
{
	/// <summary>
	/// Gets or sets the start point of the line.
	/// </summary>
	public (float x, float y) A { get; set; }
	/// <summary>
	/// Gets or sets the end point of the line.
	/// </summary>
	public (float x, float y) B { get; set; }
	/// <summary>
	/// Gets the length of the line.
	/// </summary>
	public float Length => Vector2.Distance(new(A.Item1, A.Item2), new(B.Item1, B.Item2));
	/// <summary>
	/// Gets the angle of the line in degrees.
	/// </summary>
	public float Angle => ToAngle(Direction);
	/// <summary>
	/// Gets the direction of the line as a normalized vector.
	/// </summary>
	public (float x, float y) Direction => Normalize((B.Item1 - A.Item1, B.Item2 - A.Item2));
	/// <summary>
	/// Gets or sets the color of the line.
	/// </summary>
	public uint Color { get; set; }

	/// <summary>
	/// Initializes a new instance of the line with the specified 
	/// start and end points.
	/// </summary>
	/// <param name="a">The start point of the line.</param>
	/// <param name="b">The end point of the line.</param>
	public Line((float x, float y) a, (float x, float y) b, uint color = uint.MaxValue)
	{
		A = a;
		B = b;
		Color = color;
	}

	/// <summary>
	/// Checks if this line is crossing any rectangles in the given map.
	/// </summary>
	/// <param name="map">The map to check for crossing.</param>
	/// <returns>True if this line is crossing with the specified 
	/// map, otherwise false.</returns>
	public bool IsCrossing(Map map)
	{
		return CrossPoints(map).Length > 0;
	}
	/// <summary>
	/// Checks if this line is crossing with any of the rectangles in the 
	/// specified hitbox.
	/// </summary>
	/// <param name="hitbox">The hitbox to check for crossing.</param>
	/// <returns>True if this line is crossing with the specified hitbox, 
	/// otherwise false.</returns>
	public bool IsCrossing(Hitbox hitbox)
	{
		for (int i = 0; i < hitbox.RectangleCount; i++)
			if (IsCrossing(hitbox[i]))
				return true;

		return false;
	}
	/// <param name="rectangle">
	/// The rectangle to check for crossing.</param>
	/// <returns>True if this line is crossing with the specified 
	/// rectangle, otherwise false.</returns>
	public bool IsCrossing(Rectangle rectangle)
	{
		return CrossPoints(rectangle).Length > 0;
	}
	/// <summary>
	/// Determines if this line is crossing another line.
	/// </summary>
	/// <param name="line">The other line to check for crossing.</param>
	/// <returns>True if the lines cross, false otherwise.</returns>
	public bool IsCrossing(Line line)
	{
		var (x, y, color) = CrossPoint(line);
		return float.IsNaN(x) == false && float.IsNaN(y) == false;
	}
	/// <param name="point">
	/// The point to check for crossing.</param>
	/// <returns>True if this line is crossing with the specified 
	/// rectangle, otherwise false.</returns>
	public bool IsCrossing((float x, float y) point)
	{
		var length = Length;
		var sum = Distance(A, point) + Distance(B, point);
		return IsBetween(sum, length - 0.01f, length + 0.01f);
	}
	/// <param name="point">
	/// The point to check for crossing.</param>
	/// <returns>True if this line is crossing with the specified 
	/// rectangle, otherwise false.</returns>
	public bool IsCrossing((float x, float y, uint color) point) => IsCrossing((point.x, point.y));

	/// <summary>
	/// Calculates all points of intersection between this line and the rectangles of the
	/// specified map.
	/// </summary>
	/// <param name="map">The map to calculate the intersection points with.</param>
	/// <returns>An array of all points of intersection between this line and the specified 
	/// map.</returns>
	public (float x, float y, uint color)[] CrossPoints(Map map)
	{
		var (x0, y0) = ((int)A.Item1, (int)A.Item2);
		var (x1, y1) = ((int)B.Item1, (int)B.Item2);
		var dx = (int)MathF.Abs(x1 - x0);
		var dy = (int)-MathF.Abs(y1 - y0);
		var sx = x0 < x1 ? 1 : -1;
		var sy = y0 < y1 ? 1 : -1;
		var err = dx + dy;
		var rects = new List<Rectangle>();
		int e2;

		for (int k = 0; k < MAX_ITERATIONS; k++)
		{
			var ix = (int)x0;
			var iy = (int)y0;

			var neighbourCells = map.GetNeighborRects(new((1, 1), (ix, iy)));
			for (int i = 0; i < neighbourCells.Count; i++)
				if (rects.Contains(neighbourCells[i]) == false)
					rects.Add(neighbourCells[i]);

			if (x0 == x1 && y0 == y1)
				break;

			e2 = 2 * err;

			if (e2 > dy)
			{
				err += dy;
				x0 += sx;
			}
			if (e2 < dx)
			{
				err += dx;
				y0 += sy;
			}
		}

		var result = new List<(float, float, uint)>();
		for (int i = 0; i < rects.Count; i++)
		{
			var crossPoints = CrossPoints(rects[i]);
			for (int j = 0; j < crossPoints.Length; j++)
			{
				var (x, y, color) = crossPoints[j];
				if (float.IsNaN(x) == false && float.IsNaN(y) == false &&
					result.Contains((x, y, uint.MaxValue)) == false)
					result.Add((x, y, uint.MaxValue));
			}
		}

		return result.ToArray();
	}
	/// <summary>
	/// Calculates all points of intersection between this line and the rectangles of the
	/// specified hitbox.
	/// </summary>
	/// <param name="hitbox">The hitbox to calculate the intersection points with.</param>
	/// <returns>An array of all points of intersection between this line and the specified 
	/// hitbox.</returns>
	public (float x, float y, uint color)[] CrossPoints(Hitbox hitbox)
	{
		var result = new List<(float, float, uint)>();
		for (int i = 0; i < hitbox.RectangleCount; i++)
			result.AddRange(CrossPoints(hitbox[i]));

		return result.ToArray();
	}
	/// <param name="rectangle">
	/// The rectangle to calculate the intersection points with.</param>
	/// <returns>An array of all points of intersection between this line and the specified 
	/// rectangle.</returns>
	public (float x, float y, uint color)[] CrossPoints(Rectangle rectangle)
	{
		var (x, y) = rectangle.Position;
		var (w, h) = rectangle.Size;
		var tl = (x, y);
		var tr = (x + w, y);
		var br = (x + w, y + h);
		var bl = (x, y + h);

		var up = new Line(tl, tr);
		var right = new Line(tr, br);
		var down = new Line(br, bl);
		var left = new Line(bl, tl);
		var result = new List<(float, float, uint)>();
		var points = new List<(float, float, uint)>()
			{ CrossPoint(up), CrossPoint(right), CrossPoint(down), CrossPoint(left) };

		for (int i = 0; i < points.Count; i++)
			if (float.IsNaN(points[i].Item1) == false && float.IsNaN(points[i].Item2) == false)
				result.Add(points[i]);

		return result.ToArray();
	}
	/// <param name="line">
	/// The line to calculate the intersection with.</param>
	/// <returns>The point of intersection between this line and the specified 
	/// line, or (<see cref="float.NaN, <see cref="float.NaN) if 
	/// the two lines do not intersect.</returns>
	public (float x, float y, uint color) CrossPoint(Line line)
	{
		var p = CrossPoint(A, B, line.A, line.B);
		return IsCrossing(p) && line.IsCrossing(p) ? p : (float.NaN, float.NaN, uint.MaxValue);
	}
	/// <param name="point">
	/// The point to find the closest point on the line to.</param>
	/// <returns>The point on the line that is closest to the given 
	/// line.</returns>
	public (float x, float y, uint color) ClosestPoint((float x, float y) point)
	{
		var AP = (point.Item1 - A.Item1, point.Item2 - A.Item2);
		var AB = (B.Item1 - A.Item1, B.Item2 - A.Item2);

		var magnitudeAB = LengthSquared(AB);
		var ABAPproduct = Dot(AP, AB);
		var distance = ABAPproduct / magnitudeAB;

		return distance < 0 ?
			(A.x, A.y, uint.MaxValue) : distance > 1 ?
				(B.x, A.y, uint.MaxValue) :
				(A.Item1 + AB.Item1 * distance, A.Item2 + AB.Item2 * distance, uint.MaxValue);
	}

	/// <returns>
	/// A bundle tuple containing the two points and the color of the line.</returns>
	public (float ax, float ay, float bx, float by, uint color) ToBundle() => this;
	/// <returns>
	/// A string representation of this line in the format of its bundle tuple.".</returns>
	public override string ToString()
	{
		return $"A{A} B{B} Color{Color}";
	}

	/// <summary>
	/// Implicitly converts a tuple of two points and a color into a line.
	/// </summary>
	/// <param name="bundle">The tuple to convert.</param>
	/// <returns>A new line instance.</returns>
	public static implicit operator Line((float ax, float ay, float bx, float by, uint color) bundle)
		=> new((bundle.ax, bundle.ay), (bundle.bx, bundle.by), bundle.color);
	/// <summary>
	/// Implicitly converts a line into a tuple bundle of two points and a color.
	/// </summary>
	/// <param name="line">The line to convert.</param>
	/// <returns>A tuple bundle containing the two points and the color of the line.</returns>
	public static implicit operator (float ax, float ay, float bx, float by, uint color)(Line line)
		=> (line.A.x, line.A.y, line.B.x, line.B.y, line.Color);

	#region Backend
	private const int MAX_ITERATIONS = 1000;

	private static (float, float, uint) CrossPoint(
		(float, float) A, (float, float) B, (float, float) C, (float, float) D)
	{
		var a1 = B.Item2 - A.Item2;
		var b1 = A.Item1 - B.Item1;
		var c1 = a1 * A.Item1 + b1 * A.Item2;
		var a2 = D.Item2 - C.Item2;
		var b2 = C.Item1 - D.Item1;
		var c2 = a2 * C.Item1 + b2 * C.Item2;
		var determinant = a1 * b2 - a2 * b1;

		if (determinant == 0)
			return (float.NaN, float.NaN, uint.MaxValue);

		var x = (b2 * c1 - b1 * c2) / determinant;
		var y = (a1 * c2 - a2 * c1) / determinant;
		return (x, y, uint.MaxValue);
	}
	private static float ToAngle((float, float) direction)
	{
		//Vector2 to Radians: atan2(Vector2.y, Vector2.x)
		//Radians to Angle: radians * (180 / Math.PI)

		var rad = MathF.Atan2(direction.Item2, direction.Item1);
		var result = rad * (180f / MathF.PI);
		return result;
	}
	private static (float, float) Normalize((float, float) direction)
	{
		var (x, y) = direction;
		var distance = MathF.Sqrt(x * x + y * y);
		return (x / distance, y / distance);
	}
	private static float Distance((float, float) a, (float, float) b)
	{
		var (x1, y1) = a;
		var (x2, y2) = b;
		return MathF.Sqrt(MathF.Pow(x2 - x1, 2) + MathF.Pow(y2 - y1, 2));
	}
	private static bool IsBetween(float number, float rangeA, float rangeB,
		bool inclusiveA = false, bool inclusiveB = false)
	{
		if (rangeA > rangeB)
			(rangeA, rangeB) = (rangeB, rangeA);

		var l = inclusiveA ? rangeA <= number : rangeA < number;
		var u = inclusiveB ? rangeB >= number : rangeB > number;
		return l && u;
	}
	private static float LengthSquared((float, float) vector)
	{
		var (x, y) = vector;
		var sum = x * x + y * y;

		return MathF.Pow(MathF.Sqrt(sum), 2);
	}
	private static float Dot((float, float) a, (float, float) b)
	{
		var (ax, ay) = a;
		var (bx, by) = b;
		return ax * bx + ay * by;
	}
	#endregion
}
