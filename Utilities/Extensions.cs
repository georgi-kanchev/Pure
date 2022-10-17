using System.Globalization;
using System.IO.Compression;
using System.Numerics;
using System.Text;

namespace Purity.Utilities
{
	/// <summary>
	/// Various methods that extend the primitive types, structs and collections.
	/// These serve as shortcuts for frequently used expressions/algorithms/calculations/systems.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// The type of rounding direction used by <see cref="Round"/>.
		/// </summary>
		public enum RoundWay { Closest, Up, Down }
		/// <summary>
		/// The prefered case when the number is in the middle (ends on '5' or on '.5') and the direction is <see cref="RoundWay.Closest"/>.
		/// This is used by <see cref="Round"/>.
		/// </summary>
		public enum RoundMiddle { TowardEven, AwayFromZero, TowardZero, TowardNegativeInfinity, TowardPositiveInfinity }
		/// <summary>
		/// The type of number animations used by <see cref="AnimateUnit"/>. Also known as 'easing functions'.
		/// </summary>
		public enum Animation
		{
			BendWeak, // Sine
			Bend, // Cubic
			BendStrong, // Quint
			Circle, // Circ
			Elastic, // Elastic
			Swing, // Back
			Bounce // Bounce
		}
		/// <summary>
		/// The type of number animation direction used by <see cref="AnimateUnit"/>.
		/// </summary>
		public enum AnimationCurve { Backward, Forward, BackwardThenForward }

		/// <summary>
		/// Returns true only the first time a <paramref name="condition"/> is <see langword="true"/>.
		/// This is reset whenever the <paramref name="condition"/> becomes <see langword="false"/>.
		/// This process can be repeated <paramref name="max"/> amount of times, always returns <see langword="false"/> after that.<br></br>
		/// A <paramref name="uniqueID"/> needs to be provided that describes each type of condition in order to separate/identify them.
		/// </summary>
		public static bool Once(this bool condition, string uniqueID, uint max = uint.MaxValue)
		{
			if(gates.ContainsKey(uniqueID) == false && condition == false)
				return false;
			else if(gates.ContainsKey(uniqueID) == false && condition == true)
			{
				gates[uniqueID] = true;
				gateEntries[uniqueID] = 1;
				return true;
			}
			else
			{
				if(gates[uniqueID] == true && condition == true)
					return false;
				else if(gates[uniqueID] == false && condition == true)
				{
					gates[uniqueID] = true;
					gateEntries[uniqueID]++;
					return true;
				}
				else if(gateEntries[uniqueID] < max)
					gates[uniqueID] = false;
			}
			return false;
		}
		/// <summary>
		/// Switches the values of two variables.
		/// </summary>
		public static void Swap<T>(ref T a, ref T b)
		{
			(b, a) = (a, b);
		}

		/// <summary>
		/// Randomly shuffles the contents of a <paramref name="list"/>.
		/// </summary>
		public static void Shuffle<T>(this IList<T> list)
		{
			var n = list.Count;
			while(n > 1)
			{
				n--;
				var k = new Random().Next(n + 1);
				(list[n], list[k]) = (list[k], list[n]);
			}
		}
		/// <summary>
		/// Picks randomly a single <typeparamref name="T"/> value out of a <paramref name="list"/> and returns it.
		/// </summary>
		public static T ChooseOne<T>(this IList<T> list)
		{
			return list[Random(0, list.Count - 1)];
		}
		/// <summary>
		/// Picks randomly a single <typeparamref name="T"/> value out of some <paramref name="choices"/> and returns it.
		/// </summary>
		public static T ChooseOneFrom<T>(this T choice, params T[] choices)
		{
			var list = choices == null ? new() : choices.ToList();
			list.Add(choice);
			return ChooseOne(list);
		}
		/// <summary>
		/// Calculates the average <see cref="float"/> out of a <paramref name="list"/> of <see cref="float"/>s and returns it.
		/// </summary>
		public static float Average(this IList<float> list)
		{
			var sum = 0f;
			for(int i = 0; i < list.Count; i++)
				sum += list[i];
			return sum / list.Count;
		}
		/// <summary>
		/// Calculates the average <see cref="float"/> out of a <paramref name="list"/> of <see cref="float"/>s and returns it.
		/// </summary>
		public static float AverageFrom(this float number, params float[] numbers)
		{
			var list = numbers == null ? new() : numbers.ToList();
			list.Add(number);
			return Average(list);
		}

		/// <summary>
		/// Returns whether <paramref name="text"/> can be cast to a <see cref="float"/>.
		/// </summary>
		public static bool IsNumber(this string text)
		{
			return float.IsNaN(ToNumber(text)) == false;
		}
		/// <summary>
		/// Returns whether <paramref name="text"/> contains only letters.
		/// </summary>
		public static bool IsLetters(this string text)
		{
			for(int i = 0; i < text.Length; i++)
			{
				var isLetter = (text[i] >= 'A' && text[i] <= 'Z') || (text[i] >= 'a' && text[i] <= 'z');
				if(isLetter == false)
					return false;
			}
			return true;
		}
		/// <summary>
		/// Puts <paramref name="text"/> to the right with a set amount of <paramref name="spaces"/>
		/// if they are more than the <paramref name="text"/>'s length.<br></br>
		/// </summary>
		public static string Pad(this string text, int spaces)
		{
			return string.Format("{0," + spaces + "}", text);
		}
		/// <summary>
		/// Adds <paramref name="text"/> to itself a certain amount of <paramref name="times"/> and returns it.
		/// </summary>
		public static string Repeat(this string text, int times)
		{
			var result = "";
			times = times.Limit(0, 999999);
			for(int i = 0; i < times; i++)
				result = $"{result}{text}";
			return result;
		}
		/// <summary>
		/// Encrypts and compresses a <paramref name="text"/> and returns the result.
		/// The <paramref name="text"/> can be retrieved back with <see cref="Decompress(string)"/>
		/// </summary>
		public static string Compress(this string text)
		{
			byte[] compressedBytes;

			using(var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
			{
				using var compressedStream = new MemoryStream();
				using(var compressorStream = new DeflateStream(compressedStream, CompressionLevel.Fastest, true))
				{
					uncompressedStream.CopyTo(compressorStream);
				}

				compressedBytes = compressedStream.ToArray();
			}

			return Convert.ToBase64String(compressedBytes);
		}
		/// <summary>
		/// Decrypts and decompresses a <paramref name="compressedText"/> and returns it. See <see cref="Compress(string)"/>.
		/// </summary>
		public static string Decompress(this string compressedText)
		{
			byte[] decompressedBytes;

			var compressedStream = new MemoryStream(Convert.FromBase64String(compressedText));

			using(var decompressorStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
			{
				using var decompressedStream = new MemoryStream();
				decompressorStream.CopyTo(decompressedStream);

				decompressedBytes = decompressedStream.ToArray();
			}

			return Encoding.UTF8.GetString(decompressedBytes);
		}
		/// <summary>
		/// Tries to convert <paramref name="text"/> to a <see cref="float"/> and returns the result (<see cref="float.NaN"/> if unsuccessful).
		/// This also takes into account the system's default decimal symbol.
		/// </summary>
		public static float ToNumber(this string text)
		{
			var result = 0.0f;
			text = text.Replace(',', '.');
			var parsed = float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result);

			return parsed ? result : float.NaN;
		}

		/// <summary>
		/// Wraps a <paramref name="number"/> around 0 to <paramref name="range"/> and returns it.
		/// </summary>
		public static float Wrap(this float number, float range)
		{
			return ((number % range) + range) % range;
		}
		/// <summary>
		/// Transforms a <paramref name="progress"/> ranged [0-1] to an animated progress acording to an <paramref name="animation"/>
		/// and a <paramref name="curve"/>. The animation might be <paramref name="isRepeated"/> (if the provided progress is outside of the range [0-1].
		/// This is also known as easing functions.
		/// </summary>
		public static float Animate(this float progress, Animation animation, AnimationCurve curve, bool isRepeated = false)
		{
			var result = 0f;
			var x = progress.Limit(0, 1, isRepeated);
			switch(animation)
			{
				case Animation.BendWeak:
					{
						result = curve == AnimationCurve.Backward ? 1 - MathF.Cos(x * MathF.PI / 2) :
							curve == AnimationCurve.Forward ? 1 - MathF.Sin(x * MathF.PI / 2) :
							-(MathF.Cos(MathF.PI * x) - 1) / 2;
						break;
					}
				case Animation.Bend:
					{
						result = curve == AnimationCurve.Backward ? x * x * x :
							curve == AnimationCurve.Forward ? 1 - MathF.Pow(1 - x, 3) :
							(x < 0.5 ? 4 * x * x * x : 1 - MathF.Pow(-2 * x + 2, 3) / 2);
						break;
					}
				case Animation.BendStrong:
					{
						result = curve == AnimationCurve.Backward ? x * x * x * x :
							curve == AnimationCurve.Forward ? 1 - MathF.Pow(1 - x, 5) :
							(x < 0.5 ? 16 * x * x * x * x * x : 1 - MathF.Pow(-2 * x + 2, 5) / 2);
						break;
					}
				case Animation.Circle:
					{
						result = curve == AnimationCurve.Backward ? 1 - MathF.Sqrt(1 - MathF.Pow(x, 2)) :
							curve == AnimationCurve.Forward ? MathF.Sqrt(1 - MathF.Pow(x - 1, 2)) :
							(x < 0.5 ? (1 - MathF.Sqrt(1 - MathF.Pow(2 * x, 2))) / 2 : (MathF.Sqrt(1 - MathF.Pow(-2 * x + 2, 2)) + 1) / 2);
						break;
					}
				case Animation.Elastic:
					{
						result = curve == AnimationCurve.Backward ?
							(x == 0 ? 0 : x == 1 ? 1 : -MathF.Pow(2, 10 * x - 10) * MathF.Sin((x * 10 - 10.75f) * ((2 * MathF.PI) / 3))) :
							curve == AnimationCurve.Forward ?
							(x == 0 ? 0 : x == 1 ? 1 : MathF.Pow(2, -10 * x) * MathF.Sin((x * 10 - 0.75f) * (2 * MathF.PI) / 3) + 1) :
							(x == 0 ? 0 : x == 1 ? 1 : x < 0.5f ? -(MathF.Pow(2, 20 * x - 10) * MathF.Sin((20f * x - 11.125f) *
							(2 * MathF.PI) / 4.5f)) / 2 :
							(MathF.Pow(2, -20 * x + 10) * MathF.Sin((20 * x - 11.125f) * (2 * MathF.PI) / 4.5f)) / 2 + 1);
						break;
					}
				case Animation.Swing:
					{
						result = curve == AnimationCurve.Backward ? 2.70158f * x * x * x - 1.70158f * x * x :
							curve == AnimationCurve.Forward ? 1 + 2.70158f * MathF.Pow(x - 1, 3) + 1.70158f * MathF.Pow(x - 1, 2) :
							(x < 0.5 ? (MathF.Pow(2 * x, 2) * ((2.59491f + 1) * 2 * x - 2.59491f)) / 2 :
							(MathF.Pow(2 * x - 2, 2) * ((2.59491f + 1) * (x * 2 - 2) + 2.59491f) + 2) / 2);
						break;
					}
				case Animation.Bounce:
					{
						result = curve == AnimationCurve.Backward ? 1 - easeOutBounce(1 - x) :
							curve == AnimationCurve.Forward ? easeOutBounce(x) :
							(x < 0.5f ? (1 - easeOutBounce(1 - 2 * x)) / 2 : (1 + easeOutBounce(2 * x - 1)) / 2);
						break;
					}
			}
			return result;

			float easeOutBounce(float x)
			{
				return x < 1 / 2.75f ? 7.5625f * x * x : x < 2 / 2.75f ? 7.5625f * (x -= 1.5f / 2.75f) * x + 0.75f :
					x < 2.5f / 2.75f ? 7.5625f * (x -= 2.25f / 2.75f) * x + 0.9375f : 7.5625f * (x -= 2.625f / 2.75f) * x + 0.984375f;
			}
		}
		/// <summary>
		/// Restricts a <paramref name="number"/> in the inclusive range [<paramref name="rangeA"/> - <paramref name="rangeB"/>] with a certain type of
		/// <paramref name="limitation"/> and returns it. Also known as Clamping.<br></br><br></br>
		/// - Note when using <see cref="Limitation.Overflow"/>: <paramref name="rangeB"/> is not inclusive since <paramref name="rangeA"/> = <paramref name="rangeB"/>.
		/// <br></br>
		/// - Example for this: Range [0 - 10], (0 = 10). So <paramref name="number"/> = -1 would result in 9. Putting the range [0 - 11] would give the "real" inclusive
		/// [0 - 10] range.<br></br> Therefore <paramref name="number"/> = <paramref name="rangeB"/> would result in <paramref name="rangeA"/> but not vice versa.
		/// </summary>
		public static float Limit(this float number, float rangeA, float rangeB, bool isOverflowing = false)
		{
			if(rangeA > rangeB)
				Swap(ref rangeA, ref rangeB);

			if(isOverflowing)
			{
				var d = rangeB - rangeA;
				return ((number - rangeA) % d + d) % d + rangeA;
			}
			else
			{
				if(number < rangeA)
					return rangeA;
				else if(number > rangeB)
					return rangeB;
				return number;
			}
		}
		/// <summary>
		/// Ensures a <paramref name="number"/> is <paramref name="isSigned"/> and returns the result.
		/// </summary>
		public static float Sign(this float number, bool isSigned)
		{
			return isSigned ? -MathF.Abs(number) : MathF.Abs(number);
		}
		/// <summary>
		/// Calculates <paramref name="number"/>'s precision (amount of digits after the decimal point) and returns it.
		/// </summary>
		public static int Precision(this float number)
		{
			var cultDecPoint = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			var split = number.ToString().Split(cultDecPoint);
			return split.Length > 1 ? split[1].Length : 0;
		}
		/// <summary>
		/// Rounds a <paramref name="number"/> <paramref name="toward"/> a chosen way and <paramref name="precision"/> then returns the result.
		/// May take into account a certain <paramref name="priority"/>.
		/// </summary>
		public static float Round(this float number, float precision = 0, RoundWay toward = RoundWay.Closest,
			RoundMiddle priority = RoundMiddle.AwayFromZero)
		{
			precision = (int)precision.Limit(0, 5);

			if(toward == RoundWay.Down || toward == RoundWay.Up)
			{
				var numStr = number.ToString();
				var prec = Precision(number);
				if(prec > 0 && prec > precision)
				{
					var digit = toward == RoundWay.Down ? "1" : "9";
					numStr = numStr.Remove(numStr.Length - 1);
					numStr = $"{numStr}{digit}";
					number = numStr.ToNumber();
				}
			}

			return MathF.Round(number, (int)precision, (MidpointRounding)priority);
		}
		/// <summary>
		/// Returns whether <paramref name="number"/> is in range [<paramref name="rangeA"/> - <paramref name="rangeB"/>].
		/// The ranges may be <paramref name="inclusiveA"/> or <paramref name="inclusiveB"/>.
		/// </summary>
		public static bool IsBetween(this float number, float rangeA, float rangeB, bool inclusiveA = false, bool inclusiveB = false)
		{
			if(rangeA > rangeB)
				Swap(ref rangeA, ref rangeB);
			var l = inclusiveA ? rangeA <= number : rangeA < number;
			var u = inclusiveB ? rangeB >= number : rangeB > number;
			return l && u;
		}
		/// <summary>
		/// Moves a <paramref name="number"/> in the direction of <paramref name="speed"/>. May be <paramref name="fpsDependent"/>
		/// (see <see cref="Time.Delta"/> for info). The result is then returned.
		/// </summary>
		public static float Move(this float number, float speed, float deltaTime = 1)
		{
			return number + speed * deltaTime;
		}
		/// <summary>
		/// Moves a <paramref name="number"/> toward a <paramref name="targetNumber"/> with <paramref name="speed"/>. May be
		/// <paramref name="fpsDependent"/> (see <see cref="Time.Delta"/> for info).
		/// The calculation ensures not to pass the <paramref name="targetNumber"/>. The result is then returned.
		/// </summary>
		public static float MoveToTarget(this float number, float targetNumber, float speed, float deltaTime = 1)
		{
			var goingPos = number < targetNumber;
			var result = Move(number, goingPos ? Sign(speed, false) : Sign(speed, true), deltaTime);

			if(goingPos && result > targetNumber)
				return targetNumber;
			else if(goingPos == false && result < targetNumber)
				return targetNumber;
			return result;
		}
		/// <summary>
		/// Maps a <paramref name="number"/> from one range to another ([<paramref name="a1"/> - <paramref name="a2"/>] to
		/// [<paramref name="b1"/> - <paramref name="b2"/>]) and returns it.<br></br>
		/// The <paramref name="b1"/> value is returned if the result is <see cref="float.NaN"/>,
		/// <see cref="float.NegativeInfinity"/> or <see cref="float.PositiveInfinity"/>.<br></br>
		/// - Example: 50 mapped from [0 - 100] and [0 - 1] results to 0.5<br></br>
		/// - Example: 25 mapped from [30 - 20] and [1 - 5] results to 3
		/// </summary>
		public static float Map(this float number, float a1, float a2, float b1, float b2)
		{
			var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
			return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
		}
		/// <summary>
		/// Rotates a 360 degrees <paramref name="angle"/> toward a <paramref name="targetAngle"/> with <paramref name="speed"/>
		/// taking the closest direction. May be <paramref name="fpsDependent"/> (see <see cref="Time.Delta"/> for info).
		/// The calculation ensures not to pass the <paramref name="targetAngle"/>. The result is then returned.
		/// </summary>
		public static float MoveToAngle(this float angle, float targetAngle, float speed, float deltaTime = 1)
		{
			angle = Wrap(angle, 360);
			targetAngle = Wrap(targetAngle, 360);
			speed = Math.Abs(speed);
			var difference = angle - targetAngle;

			// stops the rotation with an else when close enough
			// prevents the rotation from staying behind after the stop
			var checkedSpeed = speed;
			checkedSpeed *= deltaTime;
			if(Math.Abs(difference) < checkedSpeed) angle = targetAngle;
			else if(difference >= 0 && difference < 180) angle = Move(angle, -speed, deltaTime);
			else if(difference >= -180 && difference < 0) angle = Move(angle, speed, deltaTime);
			else if(difference >= -360 && difference < -180) angle = Move(angle, -speed, deltaTime);
			else if(difference >= 180 && difference < 360) angle = Move(angle, speed, deltaTime);

			// detects speed greater than possible
			// prevents jiggle when passing 0-360 & 360-0 | simple to fix yet took me half a day
			if(Math.Abs(difference) > 360 - checkedSpeed) angle = targetAngle;

			return angle;
		}
		/// <summary>
		/// Generates a random <see cref="float"/> number in the inclusive range [<paramref name="rangeA"/> - <paramref name="rangeB"/>] with
		/// <paramref name="precision"/> and an optional <paramref name="seed"/>. Then returns the result.
		/// </summary>
		public static float Random(this float rangeA, float rangeB, float precision = 0, float seed = float.NaN)
		{
			if(rangeA > rangeB)
				Swap(ref rangeA, ref rangeB);

			precision = (int)precision.Limit(0, 5);
			precision = MathF.Pow(10, precision);

			rangeA *= precision;
			rangeB *= precision;

			var s = new Random(float.IsNaN(seed) ? Guid.NewGuid().GetHashCode() : (int)seed);
			var randInt = s.Next((int)rangeA, (int)rangeB + 1).Limit((int)rangeA, (int)rangeB);

			return randInt / (precision);
		}
		/// <summary>
		/// Returns true only <paramref name="percent"/>% / returns false (100 - <paramref name="percent"/>)% of the times.
		/// </summary>
		public static bool HasChance(this float percent)
		{
			percent = percent.Limit(0, 100);
			var n = Random(1f, 100f); // should not roll 0 so it doesn't return true with 0% (outside of roll)
			return n <= percent;
		}
		/// <summary>
		/// Converts a 360 degrees <paramref name="angle"/> into a normalized <see cref="Vector2"/> direction then returns the result.
		/// </summary>
		public static (float, float) ToDirection(this float angle)
		{
			//Angle to Radians : (Math.PI / 180) * angle
			//Radians to Vector2 : Vector2.x = cos(angle) ; Vector2.y = sin(angle)

			var rad = MathF.PI / 180 * angle;
			var dir = new Vector2(MathF.Cos(rad), MathF.Sin(rad));

			return (dir.X, dir.Y);
		}
		/// <summary>
		/// Converts <paramref name="radians"/> to a 360 degrees angle and returns the result.
		/// </summary>
		public static float ToDegrees(this float radians)
		{
			return radians * (180f / MathF.PI);
		}
		/// <summary>
		/// Converts a 360 <paramref name="degrees"/> angle into radians and returns the result.
		/// </summary>
		public static float ToRadians(this float degrees)
		{
			return (MathF.PI / 180f) * degrees;
		}

		/// <summary>
		/// Calculates a reflected normalized <paramref name="direction"/> <see cref="Vector2"/> as if it was to bounce off of a
		/// <paramref name="surfaceNormal"/> (the direction the surface is facing) and returns it.
		/// </summary>
		public static Vector2 Reflect(this Vector2 direction, Vector2 surfaceNormal)
		{
			return Vector2.Reflect(direction, surfaceNormal);
		}
		/// <summary>
		/// Normalizes a <paramref name="direction"/> <see cref="Vector2"/>. Or in other words: sets the length (magnitude) of the
		/// <paramref name="direction"/> <see cref="Vector2"/> to 1. Then the result is returned.
		/// </summary>
		public static Vector2 Normalize(this Vector2 direction)
		{
			return Vector2.Normalize(direction);
		}
		/// <summary>
		/// Calculates the distance between a <paramref name="point"/> and a <paramref name="targetPoint"/> then returns it.
		/// </summary>
		public static float Distance(this Vector2 point, Vector2 targetPoint)
		{
			return Vector2.Distance(point, targetPoint);
		}
		/// <summary>
		/// Returns whether this <paramref name="vector"/> is invalid.
		/// </summary>
		public static bool IsNaN(this Vector2 vector)
		{
			return float.IsNaN(vector.X) || float.IsNaN(vector.Y);
		}
		/// <summary>
		/// Returns an invalid <see cref="Vector2"/>.
		/// </summary>
		public static Vector2 NaN(this Vector2 vector)
		{
			return new(float.NaN, float.NaN);
		}
		/// <summary>
		/// Converts a directional <see cref="Vector2"/> into a 360 degrees angle and returns the result.
		/// </summary>
		public static float ToAngle(this Vector2 direction)
		{
			//Vector2 to Radians: atan2(Vector2.y, Vector2.x)
			//Radians to Angle: radians * (180 / Math.PI)

			var rad = MathF.Atan2(direction.Y, direction.X);
			var result = rad * (180f / MathF.PI);
			return result;
		}
		/// <summary>
		/// Calculates the 360 degrees angle between two <see cref="Vector2"/> points and returns it.
		/// </summary>
		public static float Angle(this Vector2 point, Vector2 targetPoint)
		{
			return ToAngle(targetPoint - point).Wrap(360);
		}
		/// <summary>
		/// Snaps a <paramref name="point"/> to the closest grid cell according to <paramref name="gridSize"/> and returns the result.
		/// </summary>
		public static Vector2 ToGrid(this Vector2 point, Vector2 gridSize)
		{
			if(gridSize == default)
				return point;

			// this prevents -0 cells
			point.X -= point.X < 0 ? gridSize.X : 0;
			point.Y -= point.Y < 0 ? gridSize.Y : 0;

			point.X -= point.X % gridSize.X;
			point.Y -= point.Y % gridSize.Y;
			return point;
		}
		/// <summary>
		/// Calculates the direction between <paramref name="point"/> and <paramref name="targetPoint"/>. The result may be
		/// <paramref name="normalized"/> (see <see cref="Normalize"/> for info). Then it is returned.
		/// </summary>
		public static Vector2 Direction(this Vector2 point, Vector2 targetPoint, bool normalized = true)
		{
			return normalized ? Vector2.Normalize(targetPoint - point) : targetPoint - point;
		}
		/// <summary>
		/// Moves a <paramref name="point"/> in <paramref name="direction"/> with <paramref name="speed"/>. May be <paramref name="fpsDependent"/>
		/// (see <see cref="Time.Delta"/> for info). The result is then returned.
		/// </summary>
		public static Vector2 MoveInDirection(this Vector2 point, Vector2 direction, float speed, float deltaTime = 1)
		{
			point.X += direction.X * speed * deltaTime;
			point.Y += direction.Y * speed * deltaTime;
			return new(point.X, point.Y);
		}
		/// <summary>
		/// Moves a <paramref name="point"/> at a 360 degrees <paramref name="angle"/> with <paramref name="speed"/>. May be
		/// <paramref name="fpsDependent"/> (see <see cref="Time.Delta"/> for info). The result is then returned.
		/// </summary>
		public static Vector2 MoveAtAngle(this Vector2 point, float angle, float speed, float deltaTime = 1)
		{
			var dir = angle.Wrap(360).ToDirection();
			var result = MoveInDirection(point, Vector2.Normalize(new(dir.Item1, dir.Item2)), speed, deltaTime);
			return result;
		}
		/// <summary>
		/// Moves a <paramref name="point"/> toward <paramref name="targetPoint"/> with <paramref name="speed"/>. May be
		/// <paramref name="fpsDependent"/> (see <see cref="Time.Delta"/> for info). The calculation ensures not to pass the
		/// <paramref name="targetPoint"/>. The result is then returned.
		/// </summary>
		public static Vector2 MoveToTarget(this Vector2 point, Vector2 targetPoint, float speed, float deltaTime = 1)
		{
			var result = point.MoveAtAngle(point.Angle(targetPoint), speed, deltaTime);

			speed *= deltaTime;
			return Vector2.Distance(result, targetPoint) < speed * 1.1f ? targetPoint : result;
		}
		/// <summary>
		/// Calculates the <see cref="Vector2"/> point that is a certain <paramref name="percent"/> between <paramref name="point"/> and
		/// <paramref name="targetPoint"/> then returns the result. Also known as Lerping (linear interpolation).
		/// </summary>
		public static Vector2 PercentToTarget(this Vector2 point, Vector2 targetPoint, Vector2 percent)
		{
			point.X = percent.X.Map(0, 100, point.X, targetPoint.X);
			point.Y = percent.Y.Map(0, 100, point.Y, targetPoint.Y);
			return point;
		}
		public static (int, int) ToCoords(this int index, int width, int height)
		{
			index = index.Limit(0, width * height - 1);
			return (index % width, index / width);
		}

		/// <summary>
		/// Wraps a <paramref name="number"/> around 0 to <paramref name="range"/> and returns it.
		/// </summary>
		public static int Wrap(this int number, int range)
		{
			return ((number % range) + range) % range;
		}
		/// <summary>
		/// Generates a random <see cref="int"/> number in the inclusive range [<paramref name="rangeA"/> - <paramref name="rangeB"/>] with an
		/// optional <paramref name="seed"/>. Then returns the result.
		/// </summary>
		public static int Random(this int rangeA, int rangeB, float seed = float.NaN)
		{
			return (int)Random(rangeA, rangeB, 0, seed);
		}
		/// <summary>
		/// Returns true only <paramref name="percent"/>% / returns false (100 - <paramref name="percent"/>)% of the times.
		/// </summary>
		public static bool HasChance(this int percent)
		{
			return HasChance((float)percent);
		}
		/// <summary>
		/// Restricts a <paramref name="number"/> in the inclusive range [<paramref name="rangeA"/> - <paramref name="rangeB"/>] with a certain type of
		/// <paramref name="limitation"/> and returns it. Also known as Clamping.<br></br><br></br>
		/// - Note when using <see cref="Limitation.Overflow"/>: <paramref name="rangeB"/> is not inclusive since <paramref name="rangeA"/> = <paramref name="rangeB"/>.
		/// <br></br>
		/// - Example for this: Range [0 - 10], (0 = 10). So <paramref name="number"/> = -1 would result in 9. Putting the range [0 - 11] would give the "real" inclusive
		/// [0 - 10] range.<br></br> Therefore <paramref name="number"/> = <paramref name="rangeB"/> would result in <paramref name="rangeA"/> but not vice versa.
		/// </summary>
		public static int Limit(this int number, int rangeA, int rangeB, bool isOverflowing = false)
		{
			return (int)Limit((float)number, rangeA, rangeB, isOverflowing);
		}
		/// <summary>
		/// Ensures a <paramref name="number"/> is <paramref name="signed"/> and returns the result.
		/// </summary>
		public static int Sign(this int number, bool signed)
			=> (int)Sign((float)number, signed);
		/// <summary>
		/// Returns whether <paramref name="number"/> is in range [<paramref name="rangeA"/> - <paramref name="rangeB"/>].
		/// The ranges may be <paramref name="inclusiveA"/> or <paramref name="inclusiveB"/>.
		/// </summary>
		public static bool IsBetween(this int number, int rangeA, int rangeB, bool inclusiveA = false, bool inclusiveB = false)
			=> IsBetween((float)number, rangeA, rangeB, inclusiveA, inclusiveB);
		/// <summary>
		/// Maps a <paramref name="number"/> from [<paramref name="A1"/> - <paramref name="B1"/>] to
		/// [<paramref name="B1"/> - <paramref name="B2"/>] and returns it. Similar to Lerping (linear interpolation).<br></br>
		/// - Example: 50 mapped from [0 - 100] and [0 - 1] results to 0.5<br></br>
		/// - Example: 25 mapped from [30 - 20] and [1 - 5] results to 3
		/// </summary>
		public static int Map(this int number, int a1, int a2, int b1, int b2) =>
			(int)Map((float)number, a1, a2, b1, b2);

		public static int ToIndex(this (int, int) coords, int width, int height)
		{
			return coords.Item1.Limit(0, width - 1) * width + coords.Item2.Limit(0, height - 1);
		}

		#region Backend
		private static readonly Dictionary<string, int> gateEntries = new();
		private static readonly Dictionary<string, bool> gates = new();
		#endregion
	}
}
