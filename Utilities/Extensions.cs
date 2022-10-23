using System.Globalization; // precision point and parse
using System.IO.Compression; // string compression/decompression
using System.Text; // string compression/decompression

namespace Purity.Utilities
{
	/// <summary>
	/// Various methods that extend the primitive types, structs and collections.
	/// These serve as shortcuts for frequently used expressions/algorithms/calculations/systems.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// The type of number animations used by <see cref="AnimateUnit"/>. Also known as 'easing functions'.
		/// </summary>
		public enum Animation
		{
			Line, // Linear, lerp
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

		public static float Snap(this float number, float intervalSize)
		{
			if(intervalSize == default)
				return number;

			// this prevents -0
			var value = number - (number < 0 ? intervalSize : 0);
			value -= number % intervalSize;
			return value;
		}
		/// <summary>
		/// Wraps a <paramref name="number"/> around 0 to <paramref name="range"/> and returns it.
		/// </summary>
		public static float Wrap(this float number, float range)
		{
			return ((number % range) + range) % range;
		}
		/// <summary>
		/// Transforms a <paramref name="unit"/> ranged [0-1] to an animated progress acording to an <paramref name="animation"/>
		/// and a <paramref name="curve"/>. The animation <paramref name="isRepeated"/> optionally
		/// (if the provided progress is outside of the range [0-1]). These are also known as easing and interpolating functions.
		/// </summary>
		public static float Animate(this float unit, Animation animation, AnimationCurve curve, bool isRepeated = false)
		{
			var x = unit.Limit(0, 1, isRepeated);
			switch(animation)
			{
				case Animation.Line:
					{
						return curve == AnimationCurve.Backward ? 1f - unit :
							curve == AnimationCurve.Forward ? unit :
							unit < 0.5f ? unit.Map(0, 0.5f, 1f, 0) : unit.Map(0.5f, 1f, 0, 1f);
					}
				case Animation.BendWeak:
					{
						return curve == AnimationCurve.Backward ? 1 - MathF.Cos(x * MathF.PI / 2) :
							curve == AnimationCurve.Forward ? 1 - MathF.Sin(x * MathF.PI / 2) :
							-(MathF.Cos(MathF.PI * x) - 1) / 2;
					}
				case Animation.Bend:
					{
						return curve == AnimationCurve.Backward ? x * x * x :
							curve == AnimationCurve.Forward ? 1 - MathF.Pow(1 - x, 3) :
							(x < 0.5 ? 4 * x * x * x : 1 - MathF.Pow(-2 * x + 2, 3) / 2);
					}
				case Animation.BendStrong:
					{
						return curve == AnimationCurve.Backward ? x * x * x * x :
							curve == AnimationCurve.Forward ? 1 - MathF.Pow(1 - x, 5) :
							(x < 0.5 ? 16 * x * x * x * x * x : 1 - MathF.Pow(-2 * x + 2, 5) / 2);
					}
				case Animation.Circle:
					{
						return curve == AnimationCurve.Backward ? 1 - MathF.Sqrt(1 - MathF.Pow(x, 2)) :
							curve == AnimationCurve.Forward ? MathF.Sqrt(1 - MathF.Pow(x - 1, 2)) :
							(x < 0.5 ? (1 - MathF.Sqrt(1 - MathF.Pow(2 * x, 2))) / 2 : (MathF.Sqrt(1 - MathF.Pow(-2 * x + 2, 2)) + 1) / 2);
					}
				case Animation.Elastic:
					{
						return curve == AnimationCurve.Backward ?
							(x == 0 ? 0 : x == 1 ? 1 : -MathF.Pow(2, 10 * x - 10) * MathF.Sin((x * 10 - 10.75f) * ((2 * MathF.PI) / 3))) :
							curve == AnimationCurve.Forward ?
							(x == 0 ? 0 : x == 1 ? 1 : MathF.Pow(2, -10 * x) * MathF.Sin((x * 10 - 0.75f) * (2 * MathF.PI) / 3) + 1) :
							(x == 0 ? 0 : x == 1 ? 1 : x < 0.5f ? -(MathF.Pow(2, 20 * x - 10) * MathF.Sin((20f * x - 11.125f) *
							(2 * MathF.PI) / 4.5f)) / 2 :
							(MathF.Pow(2, -20 * x + 10) * MathF.Sin((20 * x - 11.125f) * (2 * MathF.PI) / 4.5f)) / 2 + 1);
					}
				case Animation.Swing:
					{
						return curve == AnimationCurve.Backward ? 2.70158f * x * x * x - 1.70158f * x * x :
							curve == AnimationCurve.Forward ? 1 + 2.70158f * MathF.Pow(x - 1, 3) + 1.70158f * MathF.Pow(x - 1, 2) :
							(x < 0.5 ? (MathF.Pow(2 * x, 2) * ((2.59491f + 1) * 2 * x - 2.59491f)) / 2 :
							(MathF.Pow(2 * x - 2, 2) * ((2.59491f + 1) * (x * 2 - 2) + 2.59491f) + 2) / 2);
					}
				case Animation.Bounce:
					{
						return curve == AnimationCurve.Backward ? 1 - EaseOutBounce(1 - x) :
							curve == AnimationCurve.Forward ? EaseOutBounce(x) :
							(x < 0.5f ? (1 - EaseOutBounce(1 - 2 * x)) / 2 : (1 + EaseOutBounce(2 * x - 1)) / 2);

						float EaseOutBounce(float x)
						{
							return x < 1 / 2.75f ? 7.5625f * x * x : x < 2 / 2.75f ? 7.5625f * (x -= 1.5f / 2.75f) * x + 0.75f :
								x < 2.5f / 2.75f ? 7.5625f * (x -= 2.25f / 2.75f) * x + 0.9375f : 7.5625f * (x -= 2.625f / 2.75f) * x + 0.984375f;
						}
					}
				default: return default;
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
				(rangeA, rangeB) = (rangeB, rangeA);

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
		/// Returns whether <paramref name="number"/> is in range [<paramref name="rangeA"/> - <paramref name="rangeB"/>].
		/// The ranges may be <paramref name="inclusiveA"/> or <paramref name="inclusiveB"/>.
		/// </summary>
		public static bool IsBetween(this float number, float rangeA, float rangeB, bool inclusiveA = false, bool inclusiveB = false)
		{
			if(rangeA > rangeB)
				(rangeA, rangeB) = (rangeB, rangeA);

			var l = inclusiveA ? rangeA <= number : rangeA < number;
			var u = inclusiveB ? rangeB >= number : rangeB > number;
			return l && u;
		}
		/// <summary>
		/// Moves a <paramref name="number"/> with <paramref name="speed"/>. The result is then returned.
		/// </summary>
		public static float Move(this float number, float speed, float deltaTime = 1)
		{
			return number + speed * deltaTime;
		}
		/// <summary>
		/// Moves a <paramref name="number"/> toward a <paramref name="targetNumber"/> with <paramref name="speed"/>.
		/// The calculation ensures to stop exactly at the <paramref name="targetNumber"/>. The result is then returned.
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
		public static float ToTarget(this float number, float targetNumber, float unit)
		{
			return unit.Map(0, 1, number, targetNumber);
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
		/// Generates a random <see cref="float"/> number in the inclusive range [<paramref name="rangeA"/> - <paramref name="rangeB"/>] with
		/// <paramref name="precision"/> and an optional <paramref name="seed"/>. Then returns the result.
		/// </summary>
		public static float Random(this float rangeA, float rangeB, float precision = 0, float seed = float.NaN)
		{
			if(rangeA > rangeB)
				(rangeA, rangeB) = (rangeB, rangeA);

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
		#region Backend
		private static readonly Dictionary<string, int> gateEntries = new();
		private static readonly Dictionary<string, bool> gates = new();
		#endregion
	}
}
