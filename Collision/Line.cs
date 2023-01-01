using System.Numerics;

namespace Pure.Collision
{
	/// <summary>
	/// Useful for collision detection, debugging, raycasting and much more.
	/// </summary>
	public struct Line
	{
		/// <summary>
		/// The first (starting) point of the <see cref="Line"/>.
		/// </summary>
		public (float, float) A { get; set; }
		/// <summary>
		/// The second (ending) point of the <see cref="Line"/>.
		/// </summary>
		public (float, float) B { get; set; }
		/// <summary>
		/// The distance between <see cref="A"/> and <see cref="B"/>.
		/// </summary>
		public float Length => Vector2.Distance(new(A.Item1, A.Item2), new(B.Item1, B.Item2));
		/// <summary>
		/// The 360 degree angle between <see cref="A"/> and <see cref="B"/>.
		/// </summary>
		public float Angle => ToAngle(Direction);
		/// <summary>
		/// The direction between <see cref="A"/> and <see cref="B"/>.
		/// </summary>
		public (float, float) Direction => Normalize((B.Item1 - A.Item1, B.Item2 - A.Item2));

		/// <summary>
		/// Creates the <see cref="Line"/> from two points: <paramref name="a"/> and
		/// <paramref name="b"/>.
		/// </summary>
		public Line((float, float) a, (float, float) b)
		{
			A = a;
			B = b;
		}

		/// <summary>
		/// Checks whether the <see cref="Line"/> crosses a <paramref name="grid"/> and
		/// returns the result.
		/// </summary>
		public bool IsCrossing(Grid grid)
		{
			return CrossPoints(grid).Length > 0;
		}
		/// <summary>
		/// Checks whether the <see cref="Line"/> crosses a <paramref name="hitbox"/> and
		/// returns the result.
		/// </summary>
		public bool IsCrossing(Hitbox hitbox)
		{
			for(int i = 0; i < hitbox.RectangleCount; i++)
				if(IsCrossing(hitbox[i]))
					return true;

			return false;
		}
		/// <summary>
		/// Checks whether the <see cref="Line"/> crosses a <paramref name="rectangle"/> and
		/// returns the result.
		/// </summary>
		public bool IsCrossing(Rectangle rectangle)
		{
			return CrossPoints(rectangle).Length > 0;
		}
		/// <summary>
		/// Checks whether <see langword="this"/> and another <paramref name="line"/> cross and
		/// returns the result.
		/// </summary>
		public bool IsCrossing(Line line)
		{
			var (x, y) = CrossPoint(line);
			return float.IsNaN(x) == false && float.IsNaN(y) == false;
		}
		/// <summary>
		/// Checks whether a <paramref name="point"/> is on top of the <see cref="Line"/>
		/// with a 0.01 units margin of error. Then the result is returned.
		/// </summary>
		public bool IsCrossing((float, float) point)
		{
			var length = Length;
			var sum = Distance(A, point) + Distance(B, point);
			return IsBetween(sum, length - 0.01f, length + 0.01f);
		}

		/// <summary>
		/// Calculates the cross points between the <see cref="Line"/> and a <paramref name="grid"/>.
		/// This calculation is faster than <see cref="CrossPoints(Hitbox)"/> because it checks its
		/// neighbouring cells only. The points are placed in an <see cref="Array"/> and returned
		/// afterwards.
		/// </summary>
		public (float, float)[] CrossPoints(Grid grid)
		{
			var (posX, posY) = grid.Position;
			var (x0, y0) = ((int)A.Item1, (int)A.Item2);
			var (x1, y1) = ((int)B.Item1, (int)B.Item2);
			var sz = grid.cellSize;
			var sc = grid.Scale;
			var dx = (int)MathF.Abs(x1 - x0);
			var dy = (int)-MathF.Abs(y1 - y0);
			var sx = x0 < x1 ? 1 : -1;
			var sy = y0 < y1 ? 1 : -1;
			var err = dx + dy;
			var rects = new List<Rectangle>();
			int e2;

			for(int k = 0; k < MAX_ITERATIONS; k++)
			{
				var ix = (int)(x0 / sz / sc - posX / sz);
				var iy = (int)(y0 / sz / sc - posY / sz);

				var neighbourCells = new List<Rectangle[]>()
				{
					grid.GetRectanglesAt((ix - 1, iy - 1)),
					grid.GetRectanglesAt((ix - 0, iy - 1)),
					grid.GetRectanglesAt((ix + 1, iy - 1)),

					grid.GetRectanglesAt((ix - 1, iy - 0)),
					grid.GetRectanglesAt((ix - 0, iy - 0)),
					grid.GetRectanglesAt((ix + 1, iy - 0)),

					grid.GetRectanglesAt((ix - 1, iy + 1)),
					grid.GetRectanglesAt((ix - 0, iy + 1)),
					grid.GetRectanglesAt((ix + 1, iy + 1)),
				};
				for(int i = 0; i < neighbourCells.Count; i++)
					for(int j = 0; j < neighbourCells[i].Length; j++)
						if(rects.Contains(neighbourCells[i][j]) == false)
							rects.Add(neighbourCells[i][j]);

				if(x0 == x1 && y0 == y1)
					break;

				e2 = 2 * err;

				if(e2 > dy)
				{
					err += dy;
					x0 += sx;
				}
				if(e2 < dx)
				{
					err += dx;
					y0 += sy;
				}
			}

			var result = new List<(float, float)>();
			for(int i = 0; i < rects.Count; i++)
			{
				var crossPoints = CrossPoints(rects[i]);
				for(int j = 0; j < crossPoints.Length; j++)
				{
					var (x, y) = crossPoints[j];
					if(float.IsNaN(x) == false && float.IsNaN(y) == false &&
						result.Contains((x, y)) == false)
						result.Add((x, y));
				}
			}

			return result.ToArray();
		}
		/// <summary>
		/// Calculates the cross points between the <see cref="Line"/> and a <paramref name="hitbox"/>,
		/// puts them in an <see cref="Array"/> and returns it.
		/// </summary>
		public (float, float)[] CrossPoints(Hitbox hitbox)
		{
			var result = new List<(float, float)>();
			for(int i = 0; i < hitbox.RectangleCount; i++)
				result.AddRange(CrossPoints(hitbox[i]));

			return result.ToArray();
		}
		/// <summary>
		/// Calculates the cross points between the <see cref="Line"/> and a <see cref="Rectangle"/>,
		/// puts them in an <see cref="Array"/> and returns it.
		/// </summary>
		public (float, float)[] CrossPoints(Rectangle rectangle)
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
			var result = new List<(float, float)>();
			var points = new List<(float, float)>()
			{ CrossPoint(up), CrossPoint(right), CrossPoint(down), CrossPoint(left) };

			for(int i = 0; i < points.Count; i++)
				if(float.IsNaN(points[i].Item1) == false && float.IsNaN(points[i].Item2) == false)
					result.Add(points[i]);

			return result.ToArray();
		}
		/// <summary>
		/// Returns the point where <see langword="this"/> and another <paramref name="line"/> cross.
		/// Returns (<see cref="float.NaN"/>, <see cref="float.NaN"/>) if there is no such point.
		/// </summary>
		public (float, float) CrossPoint(Line line)
		{
			var p = CrossPoint(A, B, line.A, line.B);
			return IsCrossing(p) && line.IsCrossing(p) ? p : (float.NaN, float.NaN);
		}

		/// <summary>
		/// Returns the closest point on the <see cref="Line"/> to a <paramref name="point"/>.
		/// </summary>
		public (float, float) ClosestPoint((float, float) point)
		{
			var AP = (point.Item1 - A.Item1, point.Item2 - A.Item2);
			var AB = (B.Item1 - A.Item1, B.Item2 - A.Item2);

			var magnitudeAB = LengthSquared(AB);
			var ABAPproduct = Dot(AP, AB);
			var distance = ABAPproduct / magnitudeAB;

			return distance < 0 ?
				A : distance > 1 ?
				B : (A.Item1 + AB.Item1 * distance, A.Item2 + AB.Item2 * distance);
		}

		/// <summary>
		/// Returns a text representation of this <see cref="Line"/> in the format:
		/// <see langword="A[x y] B[x y]"/>
		/// </summary>
		public override string ToString()
		{
			var (x1, y1) = A;
			var (x2, y2) = B;
			return $"{nameof(A)}[{x1} {y1}] {nameof(B)}[{x2} {y2}]";
		}

		#region Backend
		private const int MAX_ITERATIONS = 1000;

		private static (float, float) CrossPoint(
			(float, float) A, (float, float) B, (float, float) C, (float, float) D)
		{
			var a1 = B.Item2 - A.Item2;
			var b1 = A.Item1 - B.Item1;
			var c1 = a1 * A.Item1 + b1 * A.Item2;
			var a2 = D.Item2 - C.Item2;
			var b2 = C.Item1 - D.Item1;
			var c2 = a2 * C.Item1 + b2 * C.Item2;
			var determinant = a1 * b2 - a2 * b1;

			if(determinant == 0)
				return (float.NaN, float.NaN);

			var x = (b2 * c1 - b1 * c2) / determinant;
			var y = (a1 * c2 - a2 * c1) / determinant;
			return (x, y);
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
			if(rangeA > rangeB)
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
}
