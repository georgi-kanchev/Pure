namespace Pure.Utilities;

using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

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
		if (gates.ContainsKey(uniqueID) == false && condition == false)
			return false;
		else if (gates.ContainsKey(uniqueID) == false && condition)
		{
			gates[uniqueID] = new Gate() { value = true, entries = 1 };
			return true;
		}
		else
		{
			if (gates[uniqueID].value && condition)
				return false;
			else if (gates[uniqueID].value == false && condition)
			{
				gates[uniqueID].value = true;
				gates[uniqueID].entries++;
				return true;
			}
			else if (gates[uniqueID].entries < max)
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
		if (condition.Once(uniqueID))
		{
			holdDelay.Restart();
			return true;
		}

		if (condition &&
			holdDelay.Elapsed.TotalSeconds > delay &&
			holdFrequency.Elapsed.TotalSeconds > frequency)
		{
			holdFrequency.Restart();
			return true;
		}

		return false;
	}

	/// <summary>
	/// Randomly shuffles the contents of a <paramref name="collection"/>.
	/// </summary>
	public static void Shuffle<T>(this IList<T> collection)
	{
		var rand = new Random();
		for (int i = collection.Count - 1; i > 0; i--)
		{
			int j = rand.Next(i + 1);
			var temp = collection[i];
			collection[i] = collection[j];
			collection[j] = temp;
		}
	}
	/// <summary>
	/// Picks randomly a single <typeparamref name="T"/> value out of <paramref name="collection"/> and returns it.
	/// </summary>
	public static T ChooseOne<T>(this IList<T> collection)
	{
		return collection[Random(0, collection.Count - 1)];
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
	/// Calculates the average number out of a <paramref name="collection"/> of numbers and returns it.
	/// </summary>
	public static float Average(this IList<float> collection)
	{
		var sum = 0f;
		for (int i = 0; i < collection.Count; i++)
			sum += collection[i];
		return sum / collection.Count;
	}
	/// <summary>
	/// Iterates over a <paramref name="collection"/>, calls <see cref="object.ToString"/>
	/// on each element and adds the returned <see cref="string"/> to the result, alongside
	/// a <paramref name="separator"/>. The result is then returned.
	/// </summary>
	public static string ToString<T>(this IList<T> collection, string separator)
	{
		var sb = new StringBuilder();
		for (int i = 0; i < collection.Count; i++)
		{
			var sep = i != 0 ? separator : "";
			sb.Append(sep).Append(collection[i]);
		}

		return sb.ToString();
	}
	/// <summary>
	/// Shifts all elements in a <paramref name="collection"/> by an <paramref name="offset"/>.
	/// Elements are wrapped to the back or front (according to the <paramref name="offset"/>)
	/// when they get out of range.
	/// </summary>
	public static void Shift<T>(this IList<T> collection, int offset)
	{
		if (offset == default)
			return;

		if (offset < 0)
		{
			offset = Math.Abs(offset);
			for (int j = 0; j < offset; j++)
			{
				var temp = new T[collection.Count];
				for (int i = 0; i < collection.Count - 1; i++)
					temp[i] = collection[i + 1];
				temp[temp.Length - 1] = collection[0];

				for (int i = 0; i < temp.Length; i++)
					collection[i] = temp[i];
			}
			return;
		}

		offset = Math.Abs(offset);
		for (int j = 0; j < offset; j++)
		{
			var tempp = new T[collection.Count];
			for (int i = 1; i < collection.Count; i++)
				tempp[i] = collection[i - 1];
			tempp[0] = collection[tempp.Length - 1];

			for (int i = 0; i < tempp.Length; i++)
				collection[i] = tempp[i];
		}
	}
	/// <summary>
	/// Returns the common elements between a <paramref name="collection"/> and a 
	/// <paramref name="targetCollection"/>.
	/// </summary>
	public static T[] Intersect<T>(this IList<T> collection, IList<T> targetCollection)
	{
		var set1 = new HashSet<T>(collection);
		var set2 = new HashSet<T>(targetCollection);
		set1.IntersectWith(set2);
		return set1.ToArray();
	}
	/// <summary>
	/// Takes a section from a <paramref name="collection"/> specified by
	/// <paramref name="start"/> and <paramref name="end"/>. Those indices are wrapped if
	/// out of range. The result is then returned.
	/// </summary>
	public static T[] Take<T>(this IList<T> collection, int start, int end)
	{
		start = start.Wrap(collection.Count);
		end = end.Wrap(collection.Count);

		if (start > end)
			(start, end) = (end, start);

		var length = end - start;
		var result = new T[length];
		Array.Copy(collection.ToArray(), start, result, 0, length);
		return result;
	}
	/// <summary>
	/// Reverses the order of elements in a <paramref name="collection"/>.
	/// </summary>
	public static void Reverse<T>(this IList<T> collection)
	{
		var left = 0;
		var right = collection.Count - 1;
		for (int i = 0; i < collection.Count / 2; i++)
		{
			var temp = collection[left];
			collection[left] = collection[right];
			collection[right] = temp;
			left++;
			right--;
		}
	}
	/// <summary>
	/// Returns whether a <paramref name="collection"/> contains duplicate elements.
	/// </summary>
	public static bool HasDuplicates<T>(this IList<T> collection)
	{
		var set = new HashSet<T>();
		for (int i = 0; i < collection.Count; i++)
			if (set.Add(collection[i]) == false)
				return true;

		return false;
	}

	/// <summary>
	/// Iterates over a <paramref name="collection"/>, calls <see cref="object.ToString"/>
	/// on each element and adds the returned <see cref="string"/> to the result, alongside
	/// a <paramref name="separator"/>. The result is then returned.
	/// </summary>
	public static string ToString<T>(this T[,] matrix, string separatorColumn, string separatorRow)
	{
		var (m, n) = (matrix.GetLength(0), matrix.GetLength(1));
		var result = new StringBuilder();

		for (int i = 0; i < m; i++)
		{
			for (int j = 0; j < n; j++)
			{
				result.Append(matrix[i, j]);

				if (j < n - 1)
					result.Append(separatorColumn);
			}
			result.Append(separatorRow);
		}

		return result.ToString();
	}
	/// <summary>
	/// Rotates a <paramref name="matrix"/> in a <paramref name="direction"/>.
	/// A positive <paramref name="direction"/> rotates clockwise and a
	/// negative one rotates counter-clockwise. A <paramref name="direction"/> of 0
	/// results in return of the original <paramref name="matrix"/>.
	/// The result is stored in a new matrix (since the width may be different from the height).
	/// The result is then returned.
	/// </summary>
	public static T[,] Rotate<T>(this T[,] matrix, int direction)
	{
		var dir = Math.Abs(direction).Wrap(4);
		if (dir == 0)
			return matrix;

		var (m, n) = (matrix.GetLength(0), matrix.GetLength(1));
		var rotated = new T[n, m];

		if (direction > 0)
		{
			for (int i = 0; i < n; i++)
				for (int j = 0; j < m; j++)
					rotated[i, j] = matrix[m - j - 1, i];

			direction--;
			return Rotate(rotated, direction);
		}

		for (int i = 0; i < n; i++)
			for (int j = 0; j < m; j++)
				rotated[i, j] = matrix[j, n - i - 1];

		direction++;
		return Rotate(rotated, direction);
	}
	/// <summary>
	/// Flips a <paramref name="matrix"/> horizontally.
	/// </summary>
	public static void FlipHorizontally<T>(this T[,] matrix)
	{
		var rows = matrix.GetLength(0);
		var cols = matrix.GetLength(1);

		for (int i = 0; i < rows; i++)
			for (int j = 0; j < cols / 2; j++)
			{
				T temp = matrix[i, j];
				matrix[i, j] = matrix[i, cols - j - 1];
				matrix[i, cols - j - 1] = temp;
			}
	}
	/// <summary>
	/// Flips a <paramref name="matrix"/> vertically.
	/// </summary>
	public static void FlipVertically<T>(this T[,] matrix)
	{
		int rows = matrix.GetLength(0);
		int cols = matrix.GetLength(1);

		for (int i = 0; i < rows / 2; i++)
			for (int j = 0; j < cols; j++)
			{
				T temp = matrix[i, j];
				matrix[i, j] = matrix[rows - i - 1, j];
				matrix[rows - i - 1, j] = temp;
			}
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
		for (int i = 0; i < text.Length; i++)
		{
			var isLetter = (text[i] >= 'A' && text[i] <= 'Z') || (text[i] >= 'a' && text[i] <= 'z');
			if (isLetter == false)
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
		var sb = new StringBuilder();
		times = times.Limit(0, 999_999);
		for (int i = 0; i < times; i++)
			sb.Append(text);
		return sb.ToString();
	}
	/// <summary>
	/// Encrypts and compresses a <paramref name="text"/> and returns the result.
	/// The <paramref name="text"/> can be retrieved back with <see cref="Decompress(string)"/>
	/// </summary>
	public static string Compress(this string text)
	{
		byte[] compressedBytes;

		using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
		{
			using var compressedStream = new MemoryStream();
			using (var compressorStream = new DeflateStream(compressedStream, CompressionLevel.Fastest, true))
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

		using (var decompressorStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
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
		text = text.Replace(',', '.');
		var parsed = float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture,
			out var result);

		return parsed ? result : float.NaN;
	}

	public static void OpenWebPage(this string url)
	{
		try { Process.Start(url); }
		catch
		{
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				url = url.Replace("&", "^&");
				Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				Process.Start("xdg-open", url);
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				Process.Start("open", url);
			else
				Console.WriteLine($"Could not load URL '{url}'.");
		}
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
	/// Snaps a <paramref name="number"/> to an <paramref name="interval"/> and returns it.
	/// </summary>
	public static float Snap(this float number, float interval)
	{
		if (interval == default)
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
		switch (animation)
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
		if (rangeA > rangeB)
			(rangeA, rangeB) = (rangeB, rangeA);

		if (isOverflowing)
		{
			var d = rangeB - rangeA;
			return ((number - rangeA) % d + d) % d + rangeA;
		}
		else
		{
			if (number < rangeA)
				return rangeA;
			else if (number > rangeB)
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
		if (rangeA > rangeB)
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

		if (goingPos && result > targetNumber)
			return targetNumber;
		else if (goingPos == false && result < targetNumber)
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
		if (rangeA > rangeB)
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

	static Extensions()
	{
		holdFrequency.Start();
		holdDelay.Start();
	}
	#endregion
}
