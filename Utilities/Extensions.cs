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
    public enum AnimationCurve
    {
        Backward,
        Forward,
        BackwardThenForward
    }

    /// <summary>
    /// Returns true only the first time a condition is true.
    /// This is reset whenever the condition becomes false.
    /// This process can be repeated maximum amount of times, always returns false after that.
    /// A uniqueID needs to be provided that describes each type of
    /// condition in order to separate/identify them.
    /// Useful for triggering continuous checks only once, rather than every update.
    /// </summary>
    /// <param name="condition">The bool value to check for.</param>
    /// <param name="uniqueID">The uniqueID to associate the check with.</param>
    /// <param name="maximum">The maximum number of entries allowed.</param>
    /// <returns>True if the condition is true and the uniqueID has not been checked before,
    /// or if the condition is true and the number of entries is less than the maximum allowed. False otherwise.</returns>
    public static bool Once(this bool condition, string uniqueID, uint maximum = uint.MaxValue)
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
            else if (gates[uniqueID].entries < maximum)
                gates[uniqueID].value = false;
        }

        return false;
    }
    /// <summary>
    /// Returns true the first time a condition is true.
    /// Also returns true after a delay in seconds every frequency seconds.
    /// Returns false otherwise.
    /// A uniqueID needs to be provided that describes each type of condition in order to separate/identify them.
    /// Useful for turning a continuous input condition into the familiar "press and hold" key trigger.
    /// </summary>
    /// <param name="condition">The bool value to check for.</param>
    /// <param name="uniqueID">The unique ID to associate the check with.</param>
    /// <param name="delay">The delay in seconds before the condition is considered held.</param>
    /// <param name="frequency">The frequency in seconds at which the result is true while held.</param>
    public static bool PressAndHold(this bool condition, string uniqueID, float delay = 0.5f,
        float frequency = 0.06f)
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
    /// Randomly shuffles the elements in the given collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to shuffle.</param>
    public static void Shuffle<T>(this IList<T> collection)
    {
        var rand = new Random();
        for (var i = collection.Count - 1; i > 0; i--)
        {
            var j = rand.Next(i + 1);
            (collection[j], collection[i]) = (collection[i], collection[j]);
        }
    }
    /// <typeparam name="T">
    /// The type of objects in the collection.</typeparam>
    /// <param name="collection">The collection to choose from.</param>
    /// <returns>A randomly selected value from the collection.</returns>
    public static T ChooseOne<T>(this IList<T> collection)
    {
        return collection[Random((0, collection.Count - 1))];
    }
    /// <typeparam name="T">
    /// The type of objects in the choices.</typeparam>
    /// <param name="choice">The first choice to include in the selection.</param>
    /// <param name="choices">Additional choices to include in the selection.</param>
    /// <returns>A randomly selected <typeparamref name="T"/> value from the given choices.</returns>
    public static T ChooseOneFrom<T>(this T choice, params T[]? choices)
    {
        var list = choices == null ? new() : choices.ToList();
        list.Add(choice);
        return ChooseOne(list);
    }
    /// <summary>
    /// Calculates the average number out of a collection of numbers and returns it.
    /// </summary>
    /// <param name="collection">The collection of numbers to calculate the average of.</param>
    /// <returns>The average of the numbers in the collection.</returns>
    public static float Average(this IList<float> collection)
    {
        var sum = 0f;
        foreach (var n in collection)
            sum += n;

        return sum / collection.Count;
    }
    /// <summary>
    /// Iterates over a collection, calls <see cref="object.ToString"/>
    /// on each element and adds the returned string to the result, alongside
    /// a separator. The result is then returned.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection of elements to iterate over.</param>
    /// <param name="separator">The separator string to use between elements.</param>
    /// <returns>A string that represents the collection as a sequence of elements separated 
    /// by the specified separator string.</returns>
    public static string ToString<T>(this IList<T> collection, string separator)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < collection.Count; i++)
        {
            var sep = i != 0 ? separator : "";
            sb.Append(sep).Append(collection[i]);
        }

        return sb.ToString();
    }
    /// <summary>
    /// Shifts the elements of the given collection by the specified offset.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to be shifted.</param>
    /// <param name="offset">The number of positions by which to shift the elements. Positive values 
    /// shift elements to the right, negative values shift elements to the left.</param>
    /// <remarks>
    /// If the offset is greater than the size of the collection, the method will wrap 
    /// around the collection.
    /// </remarks>
    public static void Shift<T>(this IList<T> collection, int offset)
    {
        if (offset == default)
            return;

        if (offset < 0)
        {
            offset = Math.Abs(offset);
            for (var j = 0; j < offset; j++)
            {
                var temp = new T[collection.Count];
                for (var i = 0; i < collection.Count - 1; i++)
                    temp[i] = collection[i + 1];
                temp[^1] = collection[0];

                for (var i = 0; i < temp.Length; i++)
                    collection[i] = temp[i];
            }

            return;
        }

        for (var j = 0; j < offset; j++)
        {
            var tmp = new T[collection.Count];
            for (var i = 1; i < collection.Count; i++)
                tmp[i] = collection[i - 1];
            tmp[0] = collection[tmp.Length - 1];

            for (var i = 0; i < tmp.Length; i++)
                collection[i] = tmp[i];
        }
    }
    /// <summary>
    /// Computes the intersection between the elements of two collections.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collections.</typeparam>
    /// <param name="collection">The first collection.</param>
    /// <param name="targetCollection">The second collection.</param>
    /// <returns>An array containing the common elements of both collections.</returns>
    public static T[] Intersect<T>(this IList<T> collection, IList<T> targetCollection)
    {
        var set1 = new HashSet<T>(collection);
        var set2 = new HashSet<T>(targetCollection);
        set1.IntersectWith(set2);
        return set1.ToArray();
    }
    /// <summary>
    /// Returns a subsequence of elements from a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to take elements from.</param>
    /// <param name="start">The index of the first element to take.</param>
    /// <param name="end">The index of the last element to take.</param>
    /// <returns>An array containing the elements between the start and end indices, inclusive.</returns>
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
    /// Reverses the order of the elements in a collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to reverse.</param>
    public static void Reverse<T>(this IList<T> collection)
    {
        var left = 0;
        var right = collection.Count - 1;
        for (var i = 0; i < collection.Count / 2; i++)
        {
            (collection[right], collection[left]) = (collection[left], collection[right]);
            left++;
            right--;
        }
    }
    /// <typeparam name="T">
    /// The type of elements in the collection.</typeparam>
    /// <param name="collection">The collection to check for duplicates.</param>
    /// <returns>True if the collection contains at least one duplicate element, 
    /// false otherwise.</returns>
    public static bool HasDuplicates<T>(this IList<T> collection)
    {
        var set = new HashSet<T>();
        foreach (var t in collection)
            if (set.Add(t) == false)
                return true;

        return false;
    }

    /// <typeparam name="T">
    /// The type of the elements in the matrix.</typeparam>
    /// <param name="matrix">The two-dimensional array to convert to a string.</param>
    /// <param name="separatorColumn">The string used to separate columns in the resulting string.</param>
    /// <param name="separatorRow">The string used to separate rows in the resulting string.</param>
    /// <returns>A string representation of the two-dimensional array  with specified separators.</returns>
    public static string ToString<T>(this T[,] matrix, string separatorColumn, string separatorRow)
    {
        var (m, n) = (matrix.GetLength(0), matrix.GetLength(1));
        var result = new StringBuilder();

        for (var i = 0; i < m; i++)
        {
            for (var j = 0; j < n; j++)
            {
                result.Append(matrix[i, j]);

                if (j < n - 1)
                    result.Append(separatorColumn);
            }

            result.Append(separatorRow);
        }

        result.Remove(result.Length - 1, 1);
        return result.ToString();
    }
    /// <typeparam name="T">
    /// The type of the elements in the matrix.</typeparam>
    /// <param name="matrix">The two-dimensional array to rotate.</param>
    /// <param name="direction">The direction and number of 90-degree turns to rotate the array. 
    /// Positive values rotate clockwise, negative values rotate counterclockwise.</param>
    /// <returns>A new two-dimensional array that is rotated clockwise or counterclockwise.</returns>
    public static T[,] Rotate<T>(this T[,] matrix, int direction)
    {
        var dir = Math.Abs(direction).Wrap(4);
        if (dir == 0)
            return matrix;

        var (m, n) = (matrix.GetLength(0), matrix.GetLength(1));
        var rotated = new T[n, m];

        if (direction > 0)
        {
            for (var i = 0; i < n; i++)
                for (var j = 0; j < m; j++)
                    rotated[i, j] = matrix[m - j - 1, i];

            direction--;
            return Rotate(rotated, direction);
        }

        for (var i = 0; i < n; i++)
            for (var j = 0; j < m; j++)
                rotated[i, j] = matrix[j, n - i - 1];

        direction++;
        return Rotate(rotated, direction);
    }
    /// <summary>
    /// Flips a two-dimensional array horizontally and/or vertically.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the matrix.</typeparam>
    /// <param name="matrix">The two-dimensional array to flip.</param>
    /// <param name="flips">A tuple indicating the flip direction for the horizontal and vertical axes. The first element indicates whether to flip horizontally, and the second element indicates whether to flip vertically.</param>
    public static void Flip<T>(this T[,] matrix,
        (bool isFlippedHorizontally, bool isFlippedVertically) flips)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);

        if (flips.isFlippedHorizontally)
        {
            for (var i = 0; i < rows; i++)
                for (var j = 0; j < cols / 2; j++)
                {
                    (matrix[i, cols - j - 1], matrix[i, j]) = (matrix[i, j], matrix[i, cols - j - 1]);
                }
        }

        if (flips.isFlippedVertically)
        {
            for (var i = 0; i < rows / 2; i++)
                for (var j = 0; j < cols; j++)
                {
                    (matrix[rows - i - 1, j], matrix[i, j]) = (matrix[i, j], matrix[rows - i - 1, j]);
                }
        }
    }

    /// <param name="text">
    /// The input string to check.</param>
    /// <returns>True if the input string represents a number; otherwise, false.</returns>
    public static bool IsNumber(this string text)
    {
        return float.IsNaN(ToNumber(text)) == false;
    }
    /// <param name="text">
    /// The input string to check.</param>
    /// <returns>True if the input string consists of a valid number only; otherwise, false.</returns>
    public static bool IsLetters(this string text)
    {
        foreach (var c in text)
        {
            var isLetter = c is >= 'A' and <= 'Z' or >= 'a' and <= 'z';
            if (isLetter == false)
                return false;
        }

        return true;
    }
    /// <summary>
    /// Returns a new string that repeats the input string a specified number of times.
    /// </summary>
    /// <param name="text">The input string to repeat.</param>
    /// <param name="times">The number of times to repeat the input string.</param>
    /// <returns>A new string that consists of the input string repeated a specified number of times.</returns>
    public static string Repeat(this string text, int times)
    {
        var sb = new StringBuilder();
        times = times.Limit((0, 999_999));
        for (var i = 0; i < times; i++)
            sb.Append(text);
        return sb.ToString();
    }
    /// <summary>
    /// Compresses a string using Deflate compression algorithm and returns the compressed string as a Base64-encoded string.
    /// </summary>
    /// <param name="text">The input string to compress.</param>
    /// <returns>The compressed input string as a Base64-encoded string.</returns>
    public static string Compress(this string text)
    {
        byte[] compressedBytes;

        using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
        {
            using var compressedStream = new MemoryStream();
            using (var compressorStream =
                   new DeflateStream(compressedStream, CompressionLevel.Fastest, true))
            {
                uncompressedStream.CopyTo(compressorStream);
            }

            compressedBytes = compressedStream.ToArray();
        }

        return Convert.ToBase64String(compressedBytes);
    }
    /// <summary>
    /// Decompresses a Base64-encoded string that was compressed using Deflate compression algorithm and returns the original uncompressed string.
    /// </summary>
    /// <param name="compressedText">The Base64-encoded compressed string to decompress.</param>
    /// <returns>The original uncompressed string.</returns>
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
    /// Attempts to convert a string to a single-precision floating point number.
    /// </summary>
    /// <param name="text">The string to convert.</param>
    /// <returns>The single-precision floating point number represented by the input string. 
    /// If the input string is not a valid number, returns <see cref="float.NaN"/>.</returns>
    public static float ToNumber(this string text)
    {
        text = text.Replace(',', '.');
        var parsed = float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture,
            out var result);

        return parsed ? result : float.NaN;
    }
    /// <summary>
    /// Opens a web page in the default browser.
    /// </summary>
    /// <param name="url">The URL of the web page to open.</param>
    public static void OpenUrl(this string url)
    {
        try
        {
            Process.Start(url);
        }
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
    /// <param name="text">
    /// The string to search for occurrences of target.</param>
    /// <param name="target">The substring to count within text.</param>
    /// <returns>The number of times target appears 
    /// within text.</returns>
    public static int Count(this string text, string target)
    {
        return string.IsNullOrEmpty(target) ? 0 : text.Split(target).Length - 1;
    }

    /// <summary>
    /// Calculates the average of the given numbers, 
    /// including the specified number.
    /// </summary>
    /// <param name="number">The number to be included in the average calculation.</param>
    /// <param name="numbers">Additional numbers to be included in the average calculation.</param>
    /// <returns>The average of the given numbers, 
    /// including the specified number.</returns>
    public static float AverageFrom(this float number, params float[]? numbers)
    {
        var list = numbers == null ? new() : numbers.ToList();
        list.Add(number);
        return Average(list);
    }
    /// <summary>
    /// Rounds the given number to the nearest multiple of the specified interval.
    /// </summary>
    /// <param name="number">The number to be rounded.</param>
    /// <param name="interval">The interval to which the number should be rounded.</param>
    /// <returns>The nearest multiple of the specified interval.</returns>
    public static float Snap(this float number, float interval)
    {
        if (Math.Abs(interval) < 0.001f)
            return number;

        // this prevents -0
        var value = number - (number < 0 ? interval : 0);
        value -= number % interval;
        return value;
    }
    /// <summary>
    /// Wraps a value within a range of 0 to target number inclusive. Useful for
    /// wrapping an angle in the range [0-359].
    /// </summary>
    /// <param name="number">The value to wrap.</param>
    /// <param name="targetNumber">The upper range of the wrap.</param>
    /// <returns>The wrapped value within the specified range.</returns>
    public static float Wrap(this float number, float targetNumber)
    {
        return ((number % targetNumber) + targetNumber) % targetNumber;
    }
    /// <summary>
    /// Wraps a value within a range of 0 to target number inclusive. Useful for
    /// wrapping an angle in the range [0-359].
    /// </summary>
    /// <param name="number">The value to wrap.</param>
    /// <param name="targetNumber">The upper range of the wrap.</param>
    /// <returns>The wrapped value within the specified range.</returns>
    public static int Wrap(this int number, int targetNumber)
    {
        return ((number % targetNumber) + targetNumber) % targetNumber;
    }
    /// <summary>
    /// Animate the specified value using the given animation and curve.
    /// </summary>
    /// <param name="unit">The unit value to animate (ranged 0 to 1).</param>
    /// <param name="animation">The animation to apply.</param>
    /// <param name="curve">The animation curve to use.</param>
    /// <param name="isRepeated">True if the animation should be repeated; false otherwise.</param>
    /// <returns>The animated value based on the given parameters.</returns>
    public static float Animate(this float unit, Animation animation, AnimationCurve curve,
        bool isRepeated = false)
    {
        var x = unit.Limit((0, 1), isRepeated);
        switch (animation)
        {
            case Animation.Line:
            {
                return curve == AnimationCurve.Backward ? 1f - unit :
                    curve == AnimationCurve.Forward ? unit :
                    unit < 0.5f ? unit.Map((0, 0.5f), (1f, 0)) : unit.Map((0.5f, 1f), (0, 1f));
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
                    (x < 0.5
                        ? (1 - MathF.Sqrt(1 - MathF.Pow(2 * x, 2))) / 2
                        : (MathF.Sqrt(1 - MathF.Pow(-2 * x + 2, 2)) + 1) / 2);
            }
            case Animation.Elastic:
            {
                return curve == AnimationCurve.Backward
                    ? x == 0 ? 0 :
                    Math.Abs((int)(x - 1)) < 0.001f ? 1 :
                    -MathF.Pow(2, 10 * x - 10) *
                    MathF.Sin((x * 10 - 10.75f) *
                              (2 * MathF.PI / 3))
                    : curve == AnimationCurve.Forward
                        ? x == 0 ? 0 :
                        Math.Abs((int)(x - 1)) < 0.001f ? 1 :
                        MathF.Pow(2, -10 * x) *
                        MathF.Sin((x * 10 - 0.75f) * (2 * MathF.PI) / 3) + 1
                        : x == 0
                            ? 0
                            : Math.Abs((int)(x - 1)) < 0.001f
                                ? 1
                                : x < 0.5f
                                    ? -(MathF.Pow(2, 20 * x - 10) * MathF.Sin((20f * x - 11.125f) *
                                        (2 * MathF.PI) / 4.5f)) / 2
                                    : MathF.Pow(2, -20 * x + 10) *
                                    MathF.Sin((20 * x - 11.125f) * (2 * MathF.PI) / 4.5f) / 2 + 1;
            }
            case Animation.Swing:
            {
                return curve == AnimationCurve.Backward ? 2.70158f * x * x * x - 1.70158f * x * x :
                    curve == AnimationCurve.Forward ? 1 + 2.70158f * MathF.Pow(x - 1, 3) +
                                                      1.70158f * MathF.Pow(x - 1, 2) :
                    (x < 0.5
                        ? (MathF.Pow(2 * x, 2) * ((2.59491f + 1) * 2 * x - 2.59491f)) / 2
                        : (MathF.Pow(2 * x - 2, 2) * ((2.59491f + 1) * (x * 2 - 2) + 2.59491f) + 2) / 2);
            }
            case Animation.Bounce:
            {
                return curve == AnimationCurve.Backward ? 1 - EaseOutBounce(1 - x) :
                    curve == AnimationCurve.Forward ? EaseOutBounce(x) :
                    (x < 0.5f ? (1 - EaseOutBounce(1 - 2 * x)) / 2 : (1 + EaseOutBounce(2 * x - 1)) / 2);

                static float EaseOutBounce(float x)
                {
                    return x < 1 / 2.75f ? 7.5625f * x * x :
                        x < 2 / 2.75f ? 7.5625f * (x -= 1.5f / 2.75f) * x + 0.75f :
                        x < 2.5f / 2.75f ? 7.5625f * (x -= 2.25f / 2.75f) * x + 0.9375f :
                        7.5625f * (x -= 2.625f / 2.75f) * x + 0.984375f;
                }
            }
            default: return default;
        }
    }
    /// <summary>
    /// Limits a float number to a specified range.
    /// </summary>
    /// <param name="number">The number to limit.</param>
    /// <param name="range">The range value.</param>
    /// <param name="isOverflowing">Indicates whether the range is treated as circular, 
    /// allowing overflow.</param>
    /// <returns>The limited float number.</returns>
    public static float Limit(this float number, (float a, float b) range, bool isOverflowing = false)
    {
        if (range.a > range.b)
            (range.a, range.b) = (range.b, range.a);

        if (isOverflowing)
        {
            var d = range.b - range.a;
            return ((number - range.a) % d + d) % d + range.a;
        }
        else
        {
            if (number < range.a)
                return range.a;
            else if (number > range.b)
                return range.b;
            return number;
        }
    }
    /// <summary>
    /// Limits an int number to a specified range.
    /// </summary>
    /// <param name="number">The number to limit.</param>
    /// <param name="range">The range value.</param>
    /// <param name="isOverflowing">Indicates whether the range is treated as circular, 
    /// allowing overflow.</param>
    /// <returns>The limited int number.</returns>
    public static int Limit(this int number, (int a, int b) range, bool isOverflowing = false)
    {
        return (int)Limit((float)number, range, isOverflowing);
    }
    /// <param name="number">
    /// The number whose sign to adjust.</param>
    /// <param name="isSigned">Indicates whether the sign of the number 
    /// should be negative.</param>
    /// <returns>The absolute value of the float number 
    /// with the specified sign.</returns>
    public static float Sign(this float number, bool isSigned)
    {
        return isSigned ? -MathF.Abs(number) : MathF.Abs(number);
    }
    /// <param name="number">
    /// The number whose sign to adjust.</param>
    /// <param name="isSigned">Indicates whether the sign of the number 
    /// should be negative.</param>
    /// <returns>The absolute value of the int number 
    /// with the specified sign.</returns>
    public static int Sign(this int number, bool isSigned) => (int)Sign((float)number, isSigned);
    /// <param name="number">
    /// The float number to check.</param>
    /// <returns>The number of decimal places of the given float number.</returns>
    public static int Precision(this float number)
    {
        var cultDecPoint = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        var split = number.ToString(CultureInfo.CurrentCulture).Split(cultDecPoint);
        return split.Length > 1 ? split[1].Length : 0;
    }
    /// <param name="number">
    /// The float number to check.</param>
    /// <param name="range">The two range values.</param>
    /// <param name="inclusiveA">If true, the lower bound is included in the range.</param>
    /// <param name="inclusiveB">If true, the upper bound is included in the range.</param>
    /// <returns>True if the given float number is within the given range, 
    /// false otherwise.</returns>
    public static bool IsBetween(this float number, (float a, float b) range, bool inclusiveA = false,
        bool inclusiveB = false)
    {
        if (range.a > range.b)
            (range.a, range.b) = (range.b, range.a);

        var l = inclusiveA ? range.a <= number : range.a < number;
        var u = inclusiveB ? range.b >= number : range.b > number;
        return l && u;
    }
    /// <param name="number">
    /// The int number to check.</param>
    /// <param name="range">The two range values.</param>
    /// <param name="inclusiveA">If true, the lower bound is included in the range.</param>
    /// <param name="inclusiveB">If true, the upper bound is included in the range.</param>
    /// <returns>True if the given int number is within the given range, 
    /// false otherwise.</returns>
    public static bool IsBetween(this int number, (int a, int b) range, bool inclusiveA = false,
        bool inclusiveB = false) =>
        IsBetween((float)number, range, inclusiveA, inclusiveB);
    /// <summary>
    /// Checks whether the given float number is within the range defined by a 
    /// targetNumber and a range value.
    /// </summary>
    /// <param name="number">The float number to check.</param>
    /// <param name="targetNumber">The target number defining the center of the range.</param>
    /// <param name="range">The range value defining the size of the range.</param>
    /// <returns>True if the given float number is within the range defined by the target number and 
    /// the range value, false otherwise.</returns>
    public static bool IsWithin(this float number, float targetNumber, float range)
    {
        return IsBetween(number, (targetNumber - range, targetNumber + range), true, true);
    }
    /// <summary>
    /// Checks whether the given int number is within the range defined by a 
    /// targetNumber and a range value.
    /// </summary>
    /// <param name="number">The int number to check.</param>
    /// <param name="targetNumber">The target number defining the center of the range.</param>
    /// <param name="range">The range value defining the size of the range.</param>
    /// <returns>True if the given int number is within the range defined by the target number and 
    /// the range value, false otherwise.</returns>
    public static bool IsWithin(this int number, int targetNumber, int range)
    {
        return IsBetween(number, (targetNumber - range, targetNumber + range), true, true);
    }
    /// <summary>
    /// Moves the given float number by a certain speed 
    /// over a certain time.
    /// </summary>
    /// <param name="number">The float number to move.</param>
    /// <param name="speed">The speed at which to move the float number.</param>
    /// <param name="deltaTime">The time elapsed since the last move operation.</param>
    /// <returns>The new value of the moved float number.</returns>
    public static float Move(this float number, float speed, float deltaTime = 1)
    {
        return number + speed * deltaTime;
    }
    /// <summary>
    /// Moves a float number towards a targetNumber 
    /// by a given speed.
    /// </summary>
    /// <param name="number">The current number.</param>
    /// <param name="targetNumber">The target number to move towards.</param>
    /// <param name="speed">The speed of movement.</param>
    /// <param name="deltaTime">The time step for the movement.</param>
    /// <returns>The new number after the movement.</returns>
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
    /// Maps a float number from one range of values to another range of values.
    /// </summary>
    /// <param name="number">The number to map.</param>
    /// <param name="range">The first input range.</param>
    /// <param name="targetRange">The second input range.</param>
    /// <returns>The mapped number.</returns>
    public static float Map(this float number, (float a, float b) range, (float a, float b) targetRange)
    {
        var value = (number - range.a) / (range.b - range.a) * (targetRange.b - targetRange.a) +
                    targetRange.a;
        return float.IsNaN(value) || float.IsInfinity(value) ? targetRange.a : value;
    }
    /// <summary>
    /// Maps a int number from one range of values to another range of values.
    /// </summary>
    /// <param name="number">The number to map.</param>
    /// <param name="range">The first input range.</param>
    /// <param name="targetRange">The second input range.</param>
    /// <returns>The mapped number.</returns>
    public static int Map(this int number, (int a, int b) range, (int a, int b) targetRange) =>
        (int)Map((float)number, range, targetRange);
    /// <summary>
    /// Returns a random float value between the given inclusive range of values.
    /// </summary>
    /// <param name="range">The two range values.</param>
    /// <param name="precision">The precision of the generated random value (default is 0).</param>
    /// <param name="seed">The seed to use for the random generator (default is NaN, 
    /// meaning randomly chosen).</param>
    /// <returns>A random float value between the specified range of values.</returns>
    public static float Random(this (float a, float b) range, float precision = 0,
        float seed = float.NaN)
    {
        if (range.a > range.b)
            (range.a, range.b) = (range.b, range.a);

        precision = (int)precision.Limit((0, 5));
        precision = MathF.Pow(10, precision);

        range.a *= precision;
        range.b *= precision;

        var s = new Random(float.IsNaN(seed) ? Guid.NewGuid().GetHashCode() : (int)seed);
        var randInt = s.Next((int)range.a, (int)range.b + 1).Limit(((int)range.a, (int)range.b + 1));

        return randInt / (precision);
    }
    /// <summary>
    /// Returns a random int value between the given inclusive range of values.
    /// </summary>
    /// <param name="range">The two range values.</param>
    /// <param name="seed">The seed to use for the random generator (default is NaN, 
    /// meaning randomly chosen).</param>
    /// <returns>A random int value between the specified range of values.</returns>
    public static int Random(this (int a, int b) range, float seed = float.NaN)
    {
        return (int)Random(range, 0, seed);
    }
    /// <summary>
    /// Determines if a random float value between 1 and 100 is less than or equal to the 
    /// given percentage value.
    /// </summary>
    /// <param name="percent">The percentage value to check (between 0 and 100).</param>
    /// <param name="seed">The seed to use for the random generator (default is NaN, 
    /// meaning randomly chosen).</param>
    /// <returns>True if the random float value is less than or equal to the percentage value, 
    /// false otherwise.</returns>
    public static bool HasChance(this float percent, float seed = float.NaN)
    {
        percent = percent.Limit((0, 100));
        var n = Random((1f, 100f),
            seed); // should not roll 0 so it doesn't return true with 0% (outside of roll)
        return n <= percent;
    }
    /// <summary>
    /// Determines if a random int value between 1 and 100 is less than or equal to the 
    /// given percentage value.
    /// </summary>
    /// <param name="percent">The percentage value to check (between 0 and 100).</param>
    /// <param name="seed">The seed to use for the random generator (default is NaN, 
    /// meaning randomly chosen).</param>
    /// <returns>True if the random int value is less than or equal to the percentage value, 
    /// false otherwise.</returns>
    public static bool HasChance(this int percent, float seed = float.NaN)
    {
        return HasChance((float)percent, seed);
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