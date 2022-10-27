namespace Purity.Graphics
{
	public static class Cell
	{
		public enum Shade : uint
		{
			Transparent,
			Opacity1, Opacity2, Opacity3, Opacity4, Opacity5,
			Opacity6, Opacity7, Opacity8, Opacity9, Opacity10,
			Opque,
		}
		public enum Letter : uint
		{
			A = 26, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
			a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, u, v, w, x, y, z,
		}
		public enum Number : uint
		{
			ZERO = 78, ONE, TWO, THREE, FOUR, FIVE, SIX, SEVEN, EIGHT, NINE,
			zero, one, two, three, four, five, six, seven, eight, nine,
			OneFourth, Half, ThreeFourths, Pi, GoldenRatio, Infinity
		}
		public enum Math : uint
		{
			Plus = 104, Minus, Asterisk, Add = 104, Subtract, Multiply, Divide, Equals = 108, DoesNotEqual, Approximately,
			LessThan, GreaterThan, LessThanOrEqualTo, GreaterThanOrEqualTo, Percent,

			ParenthesisLeft, ParenthesisRight, BracketLeft, BracketRight, BraceLeft, BraceRight,
			BracketRoundLeft = 116, BracketRoundRight, BracketSquareLeft, BracketSquareRight,
			BracketCurlyLeft, BracketCurlyRight, ChevronLeft = 111, ChevronRight, BracketAngleLeft = 111, BracketAngleRight,

			Carat = 122, PowerOf = 122, SquareRoot, Similar, Perpendicular, Parallel, Angle, AngleRight
		}
	}
}
