using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace Pure.Utilities
{
	/// <summary>
	/// Various methods that extend the primitive types, structs and collections.
	/// These serve as shortcuts for frequently used expressions/algorithms/calculations/systems.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// The type of number animations used by <see cref="Animate"/>. Also known as 'easing functions'.
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
		/// The type of number animation direction used by <see cref="Animate"/>.
		/// </summary>
		public enum AnimationCurve { Backward, Forward, BackwardThenForward }

		static Extensions()
		{
			holdFrequency.Start();
			holdDelay.Start();
		}

		/// <summary>
		/// Returns <see langword="true"/> only the first time a <paramref name="condition"/> is <see langword="true"/>.
		/// This is reset whenever the <paramref name="condition"/> becomes <see langword="false"/>.
		/// This process can be repeated <paramref name="max"/> amount of times, always returns <see langword="false"/> after that.
		/// A <paramref name="uniqueID"/> needs to be provided that describes each type of condition in order to separate/identify them.
		/// <br></br><br></br>
		/// # Useful for triggering continuous checks only once, rather than every update.
		/// </summary>
		public static bool Once(this bool condition, string uniqueID, uint max = uint.MaxValue)
		{
			if(gates.ContainsKey(uniqueID) == false && condition == false)
				return false;
			else if(gates.ContainsKey(uniqueID) == false && condition)
			{
				gates[uniqueID] = new Gate() { value = true, entries = 1 };
				return true;
			}
			else
			{
				if(gates[uniqueID].value && condition)
					return false;
				else if(gates[uniqueID].value == false && condition)
				{
					gates[uniqueID].value = true;
					gates[uniqueID].entries++;
					return true;
				}
				else if(gates[uniqueID].entries < max)
					gates[uniqueID].value = false;
			}
			return false;
		}
		/// <summary>
		/// Returns <see langword="true"/> the first time a <paramref name="condition"/> is <see langword="true"/>.
		/// Also returns <see langword="true"/> after a <paramref name="delay"/> in seconds every <paramref name="frequency"/> seconds.
		/// Returns <see langword="false"/> otherwise.
		/// A <paramref name="uniqueID"/> needs to be provided that describes each type of condition in order to separate/identify them.
		/// <br></br><br></br>
		/// # Useful for turning a continuous input condition into the familiar "press and hold" key trigger.
		/// </summary>
		public static bool PressAndHold(this bool condition, string uniqueID, float delay = 0.5f, float frequency = 0.06f)
		{
			if(condition.Once(uniqueID))
			{
				holdDelay.Restart();
				return true;
			}

			if(condition &&
				holdDelay.Elapsed.TotalSeconds > delay &&
				holdFrequency.Elapsed.TotalSeconds > frequency)
			{
				holdFrequency.Restart();
				return true;
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
		/// Picks randomly a single <typeparamref name="T"/> value out of <paramref name="list"/> and returns it.
		/// </summary>
		public static T ChooseOne<T>(this IList<T> list)
		{
			return list[Random(0, list.Count - 1)];
		}
		/// <summary>
		/// Picks randomly a single <typeparamref name="T"/> value out of <paramref name="choice"/> and
		/// <paramref name="choices"/> and returns it.
		/// </summary>
		public static T ChooseOneFrom<T>(this T choice, params T[] choices)
		{
			var list = choices == null ? new() : choices.ToList();
			list.Add(choice);
			return ChooseOne(list);
		}
		/// <summary>
		/// Calculates the average number out of a <paramref name="list"/> of numbers and returns it.
		/// </summary>
		public static float Average(this IList<float> list)
		{
			var sum = 0f;
			for(int i = 0; i < list.Count; i++)
				sum += list[i];
			return sum / list.Count;
		}
		/// <summary>
		/// Calculates the average number out of <paramref name="numbers"/> and returns it.
		/// </summary>
		public static float AverageFrom(this float number, params float[] numbers)
		{
			var list = numbers == null ? new() : numbers.ToList();
			list.Add(number);
			return Average(list);
		}

		/// <summary>
		/// Returns whether <paramref name="text"/> represents a valid number.
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
		/// Adds <paramref name="text"/> to <paramref name="text"/> a certain amount of
		/// <paramref name="times"/> and returns it.
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
		/// Tries to convert <paramref name="text"/> to a number and returns the result (<see cref="float.NaN"/> if unsuccessful).
		/// This takes into account the system's default decimal symbol.
		/// </summary>
		public static float ToNumber(this string text)
		{
			var result = 0.0f;
			text = text.Replace(',', '.');
			var parsed = float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out result);

			return parsed ? result : float.NaN;
		}

		/// <summary>
		/// Snaps a <paramref name="number"/> to an <paramref name="interval"/> and returns it.
		/// </summary>
		public static float Snap(this float number, float interval)
		{
			if(interval == default)
				return number;

			// this prevents -0
			var value = number - (number < 0 ? interval : 0);
			value -= number % interval;
			return value;
		}
		/// <summary>
		/// Wraps a <paramref name="number"/> around the range[0 to <paramref name="targetNumber"/>]
		/// and returns it.<br></br><br></br>
		/// # Useful for keeping an angle in the range[0 to 360] degrees.
		/// </summary>
		public static float Wrap(this float number, float targetNumber)
		{
			return ((number % targetNumber) + targetNumber) % targetNumber;
		}
		/// <summary>
		/// Wraps a <paramref name="number"/> around the range[0 to <paramref name="targetNumber"/>]
		/// and returns it.<br></br><br></br>
		/// # Useful for keeping an angle in the range[0 to 360] degrees.
		/// </summary>
		public static int Wrap(this int number, int targetNumber)
		{
			return ((number % targetNumber) + targetNumber) % targetNumber;
		}
		/// <summary>
		/// Transforms a <paramref name="unit"/>[0 to 1] to an animated progress acording to <paramref name="animation"/>
		/// and <paramref name="curve"/>. The animation <paramref name="isRepeated"/> optionally
		/// if the provided progress <paramref name="unit"/> is outside of its range[0 to 1].<br></br><br></br>
		/// # Also known as easing and interpolating functions.
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
		/// Restricts a <paramref name="number"/> in the inclusive range[<paramref name="rangeA"/> to
		/// <paramref name="rangeB"/>] with a certain type of
		/// <paramref name="limitation"/>. When the limit <paramref name="isOverflowing"/> <paramref name="rangeB"/>
		/// is not inclusive since <paramref name="rangeA"/> = <paramref name="rangeB"/>.
		/// Example for this is the range[0 to 10], which means (0 = 10), Therefore the range [0 - 11] should be provided.<br></br><br></br>
		/// # Also known as Clamp.
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
		/// Restricts a <paramref name="number"/> in the inclusive range[<paramref name="rangeA"/> to
		/// <paramref name="rangeB"/>] with a certain type of
		/// <paramref name="limitation"/>. When the limit <paramref name="isOverflowing"/> <paramref name="rangeB"/>
		/// is not inclusive since <paramref name="rangeA"/> = <paramref name="rangeB"/>.
		/// Example for this is the range[0 to 10], which means (0 = 10), Therefore the range [0 - 11] should be provided.<br></br><br></br>
		/// # Also known as Clamp.
		/// </summary>
		public static int Limit(this int number, int rangeA, int rangeB, bool isOverflowing = false)
		{
			return (int)Limit((float)number, rangeA, rangeB, isOverflowing);
		}
		/// <summary>
		/// Ensures a <paramref name="number"/> is <paramref name="isSigned"/> and returns the result.
		/// </summary>
		public static float Sign(this float number, bool isSigned)
		{
			return isSigned ? -MathF.Abs(number) : MathF.Abs(number);
		}
		/// <summary>
		/// Ensures a <paramref name="number"/> is <paramref name="signed"/> and returns the result.
		/// </summary>
		public static int Sign(this int number, bool signed)
			=> (int)Sign((float)number, signed);
		/// <summary>
		/// Calculates the precision of a <paramref name="number"/> (amount of digits after the decimal symbol) and returns it.
		/// </summary>
		public static int Precision(this float number)
		{
			var cultDecPoint = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
			var split = number.ToString().Split(cultDecPoint);
			return split.Length > 1 ? split[1].Length : 0;
		}
		/// <summary>
		/// Returns whether <paramref name="number"/> is in range[<paramref name="rangeA"/> to <paramref name="rangeB"/>].
		/// The range boundaries may be <paramref name="inclusiveA"/> or <paramref name="inclusiveB"/>.
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
		/// Returns whether <paramref name="number"/> is in range[<paramref name="rangeA"/> to <paramref name="rangeB"/>].
		/// The range boundaries may be <paramref name="inclusiveA"/> or <paramref name="inclusiveB"/>.
		/// </summary>
		public static bool IsBetween(this int number, int rangeA, int rangeB, bool inclusiveA = false, bool inclusiveB = false)
			=> IsBetween((float)number, rangeA, rangeB, inclusiveA, inclusiveB);
		/// <summary>
		/// Returns whether <paramref name="number"/> is within <paramref name="range"/> of <paramref name="targetNumber"/>.
		/// </summary>
		public static bool IsWithin(this float number, float targetNumber, float range)
		{
			return IsBetween(number, targetNumber - range, targetNumber + range, true, true);
		}
		/// <summary>
		/// Returns whether <paramref name="number"/> is within <paramref name="range"/> of <paramref name="targetNumber"/>.
		/// </summary>
		public static bool IsWithin(this int number, int targetNumber, int range)
		{
			return IsBetween(number, targetNumber - range, targetNumber + range, true, true);
		}
		/// <summary>
		/// Moves a <paramref name="number"/> with <paramref name="speed"/> according to
		/// <paramref name="deltaTime"/>. The result is then returned.<br></br><br></br>
		/// # See <see cref="Time.Delta"/> for more info.
		/// </summary>
		public static float Move(this float number, float speed, float deltaTime = 1)
		{
			return number + speed * deltaTime;
		}
		/// <summary>
		/// Moves a <paramref name="number"/> towards a <paramref name="targetNumber"/> with <paramref name="speed"/>
		/// according to <paramref name="deltaTime"/>. The calculation ensures to stop exactly at the
		/// <paramref name="targetNumber"/>. The result is then returned.<br></br><br></br>
		/// # See <see cref="Time.Delta"/> for more info.
		/// </summary>
		public static float MoveTo(this float number, float targetNumber, float speed, float deltaTime = 1)
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
		/// Projects a <paramref name="number"/> in the range[<paramref name="number"/>(0) to
		/// <paramref name="targetNumber"/>(1)] according to <paramref name="unit"/>[0 to 1]
		/// and returns the result.<br></br><br></br>
		/// # Also known as Linear Interpolation - Lerp.<br></br>
		/// # Similar to <see cref="Map"/>
		/// </summary>
		public static float ToTarget(this float number, float targetNumber, float unit)
		{
			return unit.Map(0, 1, number, targetNumber);
		}
		/// <summary>
		/// Maps a <paramref name="number"/> from one range[<paramref name="a1"/> to <paramref name="a2"/>]
		/// to another [<paramref name="b1"/> to <paramref name="b2"/>] and returns it.<br></br>
		/// The <paramref name="b1"/> value is returned if the result is <see cref="float.NaN"/>,
		/// <see cref="float.NegativeInfinity"/> or <see cref="float.PositiveInfinity"/>.
		/// </summary>
		public static float Map(this float number, float a1, float a2, float b1, float b2)
		{
			var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
			return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
		}
		/// <summary>
		/// Maps a <paramref name="number"/> from one range[<paramref name="a1"/> to <paramref name="a2"/>]
		/// to another [<paramref name="b1"/> to <paramref name="b2"/>] and returns it.<br></br>
		/// The <paramref name="b1"/> value is returned if the result is <see cref="float.NaN"/>,
		/// <see cref="float.NegativeInfinity"/> or <see cref="float.PositiveInfinity"/>.
		/// </summary>
		public static int Map(this int number, int a1, int a2, int b1, int b2) =>
			(int)Map((float)number, a1, a2, b1, b2);
		/// <summary>
		/// Generates a random number in the inclusive range[<paramref name="rangeA"/> to <paramref name="rangeB"/>] with
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
		/// Generates a random number in the inclusive range[<paramref name="rangeA"/> to <paramref name="rangeB"/>] with
		/// <paramref name="precision"/> and an optional <paramref name="seed"/>. Then returns the result.
		/// </summary>
		public static int Random(this int rangeA, int rangeB, float seed = float.NaN)
		{
			return (int)Random(rangeA, rangeB, 0, seed);
		}
		/// <summary>
		/// Returns <see langword="true"/> only a certain <paramref name="percent"/> of the calls, <see langword="false"/> otherwise.
		/// </summary>
		public static bool HasChance(this float percent)
		{
			percent = percent.Limit(0, 100);
			var n = Random(1f, 100f); // should not roll 0 so it doesn't return true with 0% (outside of roll)
			return n <= percent;
		}
		/// <summary>
		/// Returns <see langword="true"/> only a certain <paramref name="percent"/> of the calls, <see langword="false"/> otherwise.
		/// </summary>
		public static bool HasChance(this int percent)
		{
			return HasChance((float)percent);
		}

		#region Backend
		private class Gate
		{
			public int entries;
			public bool value;
		}
		private static readonly Stopwatch holdFrequency = new(), holdDelay = new();

		private static readonly Dictionary<string, Gate> gates = new();
		#endregion
	}
}
