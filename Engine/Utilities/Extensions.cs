namespace Pure.Engine.Utilities;

using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

using static Alignment;

/// <summary>
/// The type of number animations used by <see cref="Extensions.Animate"/>.
/// Also known as 'easing functions'.
/// </summary>
public enum Animation
{
    /// <summary>
    /// Represents a linear/lerp animation, characterized by a constant rate of change.
    /// </summary>
    Line,
    /// <summary>
    /// Corresponds to a sine easing function, creating a gentle bending effect.
    /// </summary>
    BendWeak,
    /// <summary>
    /// Indicates a cubic easing function, resulting in a moderate bending motion.
    /// </summary>
    Bend,
    /// <summary>
    /// Represents a quintic easing function, producing a strong bending effect.
    /// </summary>
    BendStrong,
    /// <summary>
    /// Refers to a circular easing function, often denoted as Circ, creating a circular motion.
    /// </summary>
    Circle,
    /// <summary>
    /// Describes an elastic easing function, simulating an elastic or rubber-band-like motion.
    /// </summary>
    Elastic,
    /// <summary>
    /// Represents a back easing function, generating a swinging or backward motion.
    /// </summary>
    Swing,
    /// <summary>
    /// Represents a bounce easing function, creating a bouncing effect.
    /// </summary>
    Bounce
}

/// <summary>
/// The type of number animation direction used by <see cref="Extensions.Animate"/>.
/// </summary>
public enum AnimationCurve
{
    /// <summary>
    /// Eases in for a gradual start.
    /// </summary>
    In,
    /// <summary>
    /// Eases out for a gradual slowdown or stop.
    /// </summary>
    Out,
    /// <summary>
    /// Eases in first and then eases out, combining characteristics of both
    /// <see cref="In"/> and <see cref="Out"/>.
    /// </summary>
    InOut
}

/// <summary>
/// Specifies a 9-directional alignment.
/// </summary>
public enum Alignment
{
    TopLeft, Top, TopRight,
    Left, Center, Right,
    BottomLeft, Bottom, BottomRight
}

/// <summary>
/// Various methods that extend the primitive types, structs and collections.
/// These serve as shortcuts for frequently used expressions/algorithms/calculations/systems.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Returns true only the first time a condition is true.
    /// This is reset whenever the condition becomes false.
    /// This process can be repeated maximum amount of times, always returns false after that.
    /// A uniqueID needs to be provided that describes each type of
    /// condition in order to separate/identify them.
    /// Useful for triggering continuous checks only once, rather than every update.
    /// </summary>
    /// <param name="condition">The bool value to check for.</param>
    /// <param name="uniqueId">The uniqueID to associate the check with.</param>
    /// <param name="maximum">The maximum number of entries allowed.</param>
    /// <returns>True if the condition is true and the uniqueID has not been checked before,
    /// or if the condition is true and the number of entries is less than the maximum allowed. False otherwise.</returns>
    public static bool Once(this bool condition, string uniqueId, uint maximum = uint.MaxValue)
    {
        if (gates.ContainsKey(uniqueId) == false && condition == false)
            return false;
        else if (gates.ContainsKey(uniqueId) == false && condition)
        {
            gates[uniqueId] = new() { value = true, entries = 1 };
            return true;
        }
        else
        {
            if (gates[uniqueId].value && condition)
                return false;
            else if (gates[uniqueId].value == false && condition)
            {
                gates[uniqueId].value = true;
                gates[uniqueId].entries++;
                return true;
            }
            else if (gates[uniqueId].entries < maximum)
                gates[uniqueId].value = false;
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
    /// <param name="uniqueId">The unique ID to associate the check with.</param>
    /// <param name="delay">The delay in seconds before the condition is considered held.</param>
    /// <param name="frequency">The frequency in seconds at which the result is true while held.</param>
    public static bool PressAndHold(this bool condition, string uniqueId, float delay = 0.5f, float frequency = 0.06f)
    {
        if (condition.Once(uniqueId))
        {
            holdDelay.Restart();
            return true;
        }

        if (condition == false ||
            holdDelay.Elapsed.TotalSeconds <= delay ||
            holdFrequency.Elapsed.TotalSeconds <= frequency)
            return false;

        holdFrequency.Restart();
        return true;
    }

    public static T[] Flatten<T>(this T[,] matrix)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);
        var result = new T[rows * cols];

        for (var i = 0; i < rows; i++)
            for (var j = 0; j < cols; j++)
                result[i * cols + j] = matrix[i, j];

        return result;
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
    public static T? ChooseOne<T>(this IList<T> collection, float seed = float.NaN)
    {
        return collection.Count == 0 ? default : collection[Random((0, collection.Count - 1), seed)];
    }
    public static T? ChooseOneFrom<T>(this T choice, float seed = float.NaN, params T[]? choices)
    {
        var list = choices == null ? new() : choices.ToList();
        list.Add(choice);
        return ChooseOne(list, seed);
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
    public static float Average(this IList<int> collection)
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
            var sep = i != 0 ? separator : string.Empty;
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
    public static void Shift<T>(this IList<T> collection, int offset, params int[]? affectedIndexes)
    {
        if (affectedIndexes == null || affectedIndexes.Length == 0 || offset == 0)
            return;

        if (collection is List<T> list)
        {
            var results = new List<int>();
            var indexList = affectedIndexes.ToList();
            var prevTargetIndex = -1;
            var max = list.Count - 1;

            indexList.Sort();

            if (offset > 0)
                indexList.Reverse();

            foreach (var currIndex in indexList)
            {
                if (currIndex < 0 || currIndex >= list.Count)
                    continue;

                var item = list[currIndex];
                var targetIndex = Math.Clamp(currIndex + offset, 0, max);

                // prevent items order change
                if (currIndex > 0 &&
                    currIndex < max &&
                    indexList.Contains(currIndex + (offset > 0 ? 1 : -1)))
                    continue;

                // prevent overshooting of multiple items which would change the order
                var isOvershooting = (targetIndex == 0 && prevTargetIndex == 0) ||
                                     (targetIndex == max && prevTargetIndex == max) ||
                                     results.Contains(targetIndex);
                var i = indexList.IndexOf(currIndex);
                var result = isOvershooting ? offset < 0 ? i : max - i : targetIndex;

                list.RemoveAt(currIndex);
                list.Insert(result, item);
                prevTargetIndex = targetIndex;
                results.Add(result);
            }

            return;
        }

        // if not a list then convert it
        var tempList = collection.ToList();
        Shift(tempList, offset, affectedIndexes);

        for (var i = 0; i < tempList.Count; i++)
            collection[i] = tempList[i];
    }
    public static void Shift<T>(this IList<T> collection, int offset, params T[]? affectedItems)
    {
        if (affectedItems == null || affectedItems.Length == 0 || offset == 0)
            return;

        var affectedIndexes = new int[affectedItems.Length];
        for (var i = 0; i < affectedItems.Length; i++)
            affectedIndexes[i] = collection.IndexOf(affectedItems[i]);

        Shift(collection, offset, affectedIndexes);
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

    public static string ToString<T>(this T[,] matrix, (string horizontal, string vertical) separator)
    {
        var (m, n) = (matrix.GetLength(0), matrix.GetLength(1));
        var result = new StringBuilder();

        for (var i = 0; i < m; i++)
        {
            for (var j = 0; j < n; j++)
            {
                result.Append(matrix[i, j]);

                if (j < n - 1)
                    result.Append(separator.horizontal);
            }

            result.Append(separator.vertical);
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
    public static void Flip<T>(this T[,] matrix, bool mirror, bool flip)
    {
        var rows = matrix.GetLength(0);
        var cols = matrix.GetLength(1);

        if (mirror)
            for (var i = 0; i < rows; i++)
                for (var j = 0; j < cols / 2; j++)
                    (matrix[i, cols - j - 1], matrix[i, j]) = (matrix[i, j], matrix[i, cols - j - 1]);

        if (flip == false)
            return;

        for (var i = 0; i < rows / 2; i++)
            for (var j = 0; j < cols; j++)
                (matrix[rows - i - 1, j], matrix[i, j]) = (matrix[i, j], matrix[rows - i - 1, j]);
    }

    public static bool IsSurroundedBy(this string input, string text)
    {
        var isInvalid = string.IsNullOrEmpty(input) || string.IsNullOrEmpty(text);
        return isInvalid == false && input.StartsWith(text) && input.EndsWith(text);
    }
    public static float Calculate(this string mathExpression)
    {
        mathExpression = mathExpression.Replace(' ', char.MinValue);

        var values = new Stack<float>();
        var operators = new Stack<char>();
        var bracketCountOpen = 0;
        var bracketCountClose = 0;

        for (var i = 0; i < mathExpression.Length; i++)
        {
            var c = mathExpression[i];

            if (char.IsDigit(c) || c == '.')
                values.Push(GetNumber(ref i));
            else if (c == '(')
            {
                operators.Push(c);
                bracketCountOpen++;
            }
            else if (c == ')')
            {
                bracketCountClose++;
                while (operators.Count > 0 && operators.Peek() != '(')
                    if (Process())
                        return float.NaN;

                if (operators.Count > 0)
                    operators.Pop(); // Pop the '('
            }
            else if (IsOperator(c))
            {
                while (operators.Count > 0 && Priority(operators.Peek()) >= Priority(c))
                    if (Process())
                        return float.NaN;

                operators.Push(c);
            }

            if (bracketCountClose > bracketCountOpen)
                return float.NaN;
        }

        if (bracketCountOpen != bracketCountClose)
            return float.NaN;

        while (operators.Count > 0)
            if (Process())
                return float.NaN;

        return values.Count == 0 ? float.NaN : values.Pop();

        bool Process()
        {
            if (values.Count < 2 || operators.Count < 1)
                return true;

            var val2 = values.Pop();
            var val1 = values.Pop();
            var op = operators.Pop();
            values.Push(ApplyOperator(val1, val2, op));
            return false;
        }

        bool IsOperator(char c)
        {
            return c is '+' or '-' or '*' or '/' or '^' or '%';
        }

        int Priority(char op)
        {
            if (op is '+' or '-') return 1;
            if (op is '*' or '/' or '%') return 2;
            if (op is '^') return 3;
            return 0;
        }

        float ApplyOperator(float val1, float val2, char op)
        {
            if (op == '+') return val1 + val2;
            if (op == '-') return val1 - val2;
            if (op == '*') return val1 * val2;
            if (op == '/') return val2 != 0 ? val1 / val2 : float.NaN;
            if (op == '%') return val2 != 0 ? val1 % val2 : float.NaN;
            if (op == '^') return MathF.Pow(val1, val2);
            return float.NaN;
        }

        float GetNumber(ref int i)
        {
            var num = string.Empty;
            while (i < mathExpression.Length &&
                   (char.IsDigit(mathExpression[i]) || mathExpression[i] == '.'))
            {
                num += mathExpression[i];
                i++;
            }

            i--;

            return num.ToNumber();
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
                uncompressedStream.CopyTo(compressorStream);

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
    public static void OpenAsUrl(this string url)
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
    public static string EnsureUnique(this string value, params string[]? values)
    {
        return EnsureUnique(values, value);
    }
    public static string EnsureUnique(this string[]? values, string value)
    {
        if (values == null || values.Contains(value) == false)
            return value;

        var baseName = Regex.Replace(value, @"(\d+)$", string.Empty);
        var number = 1;

        while (values.Any(n => Regex.IsMatch(n, $"^{baseName}{number}$")))
            number++;

        return $"{baseName}{number}";
    }
    public static string Mask(this string text, float symbolProgress)
    {
        symbolProgress = Math.Clamp(symbolProgress, 0, 1);
        return text.Remove((int)(text.Length * symbolProgress));
    }
    public static string PadLeftAndRight(string text, int length)
    {
        var spaces = length - text.Length;
        var padLeft = spaces / 2 + text.Length;
        return text.PadLeft(padLeft).PadRight(length);
    }
    public static string Constrain(this string text, (int width, int height) size, bool wordWrap = true, Alignment alignment = TopLeft, float scrollProgress = 0f, float symbolProgress = 1f, char tintBrush = '#')
    {
        if (string.IsNullOrEmpty(text) || size.width <= 0 || size.height <= 0)
            return string.Empty;

        var result = text;

        var colorTags = GetColorTags(result);
        result = RemoveColorTags(result);
        result = result.Remove((int)(result.Length * Math.Clamp(symbolProgress, 0, 1)));
        result = ApplyColorTags(result, colorTags);

        var lineList = result.TrimEnd().Split(Environment.NewLine).ToList();

        TryWordWrap();
        TryAlignVertically();

        var start = 0;
        var end = size.height;
        var e = lineList.Count - end;
        var scrollValue = (int)Math.Round(scrollProgress * e);

        start = e > 0 ? scrollValue : 0;
        end = Math.Min(end, lineList.Count);
        end += e > 0 ? scrollValue : 0;

        var lastHiddenTag = string.Empty;
        if (start != 0) // there is scrolling which might have cropped the last tag, so find it
            for (var i = start; i >= 0; i--)
            {
                var colors = GetColorTags(lineList[i]);
                if (colors.Count > 0)
                    lastHiddenTag = colors[^1].tag;
            }

        result = string.Empty;
        for (var i = start; i < end; i++)
        {
            var nl = i == start ? lastHiddenTag : Environment.NewLine;
            result += nl + TryAlignHorizontally(lineList[i]);
        }

        return result;

        string TryAlignHorizontally(string line)
        {
            var tags = GetColorTags(line);
            line = RemoveColorTags(line);
            if (alignment is TopRight or Right or BottomRight)
                line = line.PadLeft(size.width);
            else if (alignment is Top or Center or Bottom)
                line = PadLeftAndRight(line, size.width);
            line = ApplyColorTags(line, tags);
            return line;
        }

        void TryAlignVertically()
        {
            var yDiff = size.height - lineList.Count;

            if (alignment is Left or Center or Right)
                for (var i = 0; i < yDiff / 2; i++)
                    lineList.Insert(0, string.Empty);
            else if (alignment is BottomLeft or Bottom or BottomRight)
                for (var i = 0; i < yDiff; i++)
                    lineList.Insert(0, string.Empty);
        }

        void TryWordWrap()
        {
            for (var i = 0; i < lineList.Count; i++)
            {
                var line = lineList[i];
                var colors = GetColorTags(line);
                line = RemoveColorTags(line);

                if (line.Length <= size.width) // line is valid length
                    continue;

                var lastLineIndex = size.width - 1;
                var newLineIndex = wordWrap ?
                    GetSafeNewLineIndex(line, (uint)lastLineIndex) :
                    lastLineIndex;

                // end of line? can't word wrap, proceed to symbol wrap
                if (newLineIndex == 0)
                {
                    lineList[i] = line[..size.width];
                    lineList.Insert(i + 1, line[size.width..line.Length]);
                    ApplyNewLineToColors();
                    continue;
                }

                // otherwise wordwrap
                var endIndex = newLineIndex + (wordWrap ? 0 : 1);
                lineList[i] = line[..endIndex].TrimStart();
                lineList.Insert(i + 1, line[(newLineIndex + 1)..line.Length]);
                ApplyNewLineToColors();

                void ApplyNewLineToColors()
                {
                    var lineLengthNoSpaces = lineList[i].Length - lineList[i].Count(" ");
                    lineList[i] = ApplyColorTags(lineList[i], colors);

                    for (var j = 0; j < colors.Count; j++)
                        colors[j] = (colors[j].index - lineLengthNoSpaces, colors[j].tag);

                    lineList[i + 1] = ApplyColorTags(lineList[i + 1], colors);
                }
            }

            int GetSafeNewLineIndex(string line, uint endLineIndex)
            {
                for (var i = (int)endLineIndex; i >= 0; i--)
                    if (line[i] == ' ' && i <= size.width)
                        return i;

                return default;
            }
        }

        List<(int index, string tag)> GetColorTags(string input)
        {
            input = input.Replace(" ", string.Empty);
            input = input.Replace(Environment.NewLine, string.Empty);
            var matches = Regex.Matches(input, $"{tintBrush.ToString()}([0-9a-fA-F]+){tintBrush.ToString()}");
            var output = new List<(int index, string tag)>();

            var offset = 0;
            foreach (Match match in matches)
            {
                output.Add((match.Index - offset, $"{tintBrush}{match.Groups[1].Value}{tintBrush}"));
                offset += match.Length;
            }

            return output;
        }

        string RemoveColorTags(string input)
        {
            var matches = Regex.Matches(input, $"{tintBrush.ToString()}([0-9a-fA-F]+){tintBrush.ToString()}");
            var offset = 0;
            var builder = new StringBuilder(input);
            foreach (Match match in matches)
            {
                builder.Remove(match.Index - offset, match.Length);
                offset += match.Length;
            }

            return builder.ToString();
        }

        string ApplyColorTags(string input, List<(int index, string tag)> storedTags)
        {
            var realIndex = 0;
            var builder = new StringBuilder(input);
            for (var i = 0; i < builder.Length; i++)
            {
                if (char.IsWhiteSpace(builder[i]))
                    continue;

                // multiple tags on the same index?
                while (storedTags.Count > 0 && storedTags[0].index == realIndex)
                {
                    builder.Insert(i, storedTags[0].tag);
                    i += storedTags[0].tag.Length;
                    storedTags.RemoveAt(0);
                }

                if (storedTags.Count == 0)
                    return builder.ToString();

                realIndex++;
            }

            return builder.ToString();
        }
    }
    public static string Shorten(this string text, int maxLength, string indicator = "…")
    {
        if (maxLength == 0)
            return string.Empty;

        var abs = Math.Abs(maxLength);
        var index = abs - indicator.Length;

        if (maxLength > 0 && text.Length > maxLength)
            text = text[..Math.Max(index, 0)] + indicator;
        else if (maxLength < 0 && text.Length > abs)
            text = indicator + text[^index..];

        return text;
    }

    [SuppressMessage("ReSharper", "FormatStringProblem")]
    public static string PadZeros(this float number, int amountOfZeros)
    {
        if (amountOfZeros == 0)
            return $"{number}";

        var format = amountOfZeros < 0 ? new('0', Math.Abs(amountOfZeros)) : "F" + amountOfZeros;
        return string.Format("{0:" + format + "}", number);
    }
    [SuppressMessage("ReSharper", "FormatStringProblem")]
    public static string PadZeros(this int number, int amountOfZeros)
    {
        return string.Format("{0:D" + Math.Abs(amountOfZeros) + "}", number);
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
        if (float.IsNaN(interval) || float.IsInfinity(number) || Math.Abs(interval) < 0.001f)
            return number;

        var remainder = number % interval;
        var halfway = interval / 2f;

        return remainder < halfway ?
            number - remainder :
            number + (interval - remainder);
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
        return (number % targetNumber + targetNumber) % targetNumber;
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
        return (number % targetNumber + targetNumber) % targetNumber;
    }
    /// <summary>
    /// Animate the specified value using the given animation and curve.
    /// </summary>
    /// <param name="unit">The unit value to animate (ranged 0 to 1).</param>
    /// <param name="animation">The animation to apply.</param>
    /// <param name="curve">The animation curve to use.</param>
    /// <param name="repeat">True if the animation should be repeated; false otherwise.</param>
    /// <returns>The animated value based on the given parameters.</returns>
    public static float Animate(this float unit, Animation animation, AnimationCurve curve, bool repeat = false)
    {
        var x = unit.Limit((0, 1), repeat);
        switch (animation)
        {
            case Animation.Line:
            {
                return curve == AnimationCurve.In ? 1f - unit :
                    curve == AnimationCurve.Out ? unit :
                    unit < 0.5f ? unit.Map((0, 0.5f), (1f, 0)) : unit.Map((0.5f, 1f), (0, 1f));
            }
            case Animation.BendWeak:
            {
                return curve == AnimationCurve.In ? 1 - MathF.Cos(x * MathF.PI / 2) :
                    curve == AnimationCurve.Out ? 1 - MathF.Sin(x * MathF.PI / 2) :
                    -(MathF.Cos(MathF.PI * x) - 1) / 2;
            }
            case Animation.Bend:
            {
                return curve == AnimationCurve.In ? x * x * x :
                    curve == AnimationCurve.Out ? 1 - MathF.Pow(1 - x, 3) :
                    x < 0.5 ? 4 * x * x * x : 1 - MathF.Pow(-2 * x + 2, 3) / 2;
            }
            case Animation.BendStrong:
            {
                return curve == AnimationCurve.In ? x * x * x * x :
                    curve == AnimationCurve.Out ? 1 - MathF.Pow(1 - x, 5) :
                    x < 0.5 ? 16 * x * x * x * x * x : 1 - MathF.Pow(-2 * x + 2, 5) / 2;
            }
            case Animation.Circle:
            {
                return curve == AnimationCurve.In ? 1 - MathF.Sqrt(1 - MathF.Pow(x, 2)) :
                    curve == AnimationCurve.Out ? MathF.Sqrt(1 - MathF.Pow(x - 1, 2)) :
                    x < 0.5 ? (1 - MathF.Sqrt(1 - MathF.Pow(2 * x, 2))) / 2 :
                    (MathF.Sqrt(1 - MathF.Pow(-2 * x + 2, 2)) + 1) / 2;
            }
            case Animation.Elastic:
            {
                return curve == AnimationCurve.In ? x == 0 ? 0 :
                    Math.Abs((int)(x - 1)) < 0.001f ? 1 :
                    -MathF.Pow(2, 10 * x - 10) *
                    MathF.Sin((x * 10 - 10.75f) *
                              (2 * MathF.PI / 3))
                    : curve == AnimationCurve.Out ? x == 0 ? 0 :
                    Math.Abs((int)(x - 1)) < 0.001f ? 1 :
                    MathF.Pow(2, -10 * x) *
                    MathF.Sin((x * 10 - 0.75f) * (2 * MathF.PI) / 3) +
                    1
                    : x == 0 ? 0
                    : Math.Abs((int)(x - 1)) < 0.001f ? 1
                    : x < 0.5f ? -(MathF.Pow(2, 20 * x - 10) *
                                   MathF.Sin((20f * x - 11.125f) *
                                             (2 * MathF.PI) /
                                             4.5f)) /
                                 2
                    : MathF.Pow(2, -20 * x + 10) *
                      MathF.Sin((20 * x - 11.125f) * (2 * MathF.PI) / 4.5f) /
                      2 +
                      1;
            }
            case Animation.Swing:
            {
                return curve == AnimationCurve.In ? 2.70158f * x * x * x - 1.70158f * x * x :
                    curve == AnimationCurve.Out ? 1 +
                                                  2.70158f * MathF.Pow(x - 1, 3) +
                                                  1.70158f * MathF.Pow(x - 1, 2) :
                    x < 0.5 ? MathF.Pow(2 * x, 2) * ((2.59491f + 1) * 2 * x - 2.59491f) / 2 :
                    (MathF.Pow(2 * x - 2, 2) * ((2.59491f + 1) * (x * 2 - 2) + 2.59491f) + 2) / 2;
            }
            case Animation.Bounce:
            {
                return curve == AnimationCurve.In ? 1 - EaseOutBounce(1 - x) :
                    curve == AnimationCurve.Out ? EaseOutBounce(x) :
                    x < 0.5f ? (1 - EaseOutBounce(1 - 2 * x)) / 2 : (1 + EaseOutBounce(2 * x - 1)) / 2;

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
    /// <param name="overflow">Indicates whether the range is treated as circular, 
    /// allowing overflow.</param>
    /// <returns>The limited float number.</returns>
    public static float Limit(this float number, (float a, float b) range, bool overflow = false)
    {
        if (range.a > range.b)
            (range.a, range.b) = (range.b, range.a);

        if (overflow)
        {
            var d = range.b - range.a;
            return d == 0 ? range.a : ((number - range.a) % d + d) % d + range.a;
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
    /// <param name="overflow">Indicates whether the range is treated as circular, 
    /// allowing overflow.</param>
    /// <returns>The limited int number.</returns>
    public static int Limit(this int number, (int a, int b) range, bool overflow = false)
    {
        return (int)Limit((float)number, range, overflow);
    }
    /// <param name="amount">
    /// The number of values to distribute.</param>
    /// <param name="range">The range of values (inclusive). The order is maintained.</param>
    /// <returns>An array of evenly distributed numbers across the specified range.</returns>
    public static float[] Distribute(this int amount, (float a, float b) range)
    {
        if (amount <= 0)
            return Array.Empty<float>();

        var result = new float[amount];
        var size = range.b - range.a;
        var spacing = size / (amount + 1);

        for (var i = 1; i <= amount; i++)
            result[i - 1] = range.a + i * spacing;

        return result;
    }
    /// <param name="number">
    /// The number whose sign to adjust.</param>
    /// <param name="sign">Indicates whether the sign of the number 
    /// should be negative.</param>
    /// <returns>The absolute value of the float number 
    /// with the specified sign.</returns>
    public static float Sign(this float number, bool sign)
    {
        return sign ? -MathF.Abs(number) : MathF.Abs(number);
    }
    /// <param name="number">
    /// The number whose sign to adjust.</param>
    /// <param name="sign">Indicates whether the sign of the number 
    /// should be negative.</param>
    /// <returns>The absolute value of the int number 
    /// with the specified sign.</returns>
    public static int Sign(this int number, bool sign)
    {
        return (int)Sign((float)number, sign);
    }
    /// <param name="number">
    /// The float number to check.</param>
    /// <returns>The number of decimal places of the given float number.</returns>
    public static int Precision(this float number)
    {
        var cultDecPoint = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        var split = number.ToString(CultureInfo.CurrentCulture).Split(cultDecPoint);
        return split.Length > 1 ? split[1].Length : 0;
    }
    public static bool IsBetween(this float number, (float a, float b) range, (bool a, bool b) inclusive = default)
    {
        if (range.a > range.b)
            (range.a, range.b) = (range.b, range.a);

        var l = inclusive.a ? range.a <= number : range.a < number;
        var u = inclusive.b ? range.b >= number : range.b > number;
        return l && u;
    }
    public static bool IsBetween(this int number, (int a, int b) range, (bool a, bool b) inclusive = default)
    {
        return IsBetween((float)number, range, inclusive);
    }
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
        return IsBetween(number, (targetNumber - range, targetNumber + range), (true, true));
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
        return IsBetween(number, (targetNumber - range, targetNumber + range), (true, true));
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
    /// <param name="rangeIn">The first input range.</param>
    /// <param name="rangeOut">The second input range.</param>
    /// <returns>The mapped number.</returns>
    public static float Map(this float number, (float a, float b) rangeIn, (float a, float b) rangeOut)
    {
        if (Math.Abs(rangeIn.a - rangeIn.b) < 0.001f)
            return (rangeOut.a + rangeOut.b) / 2f;

        var target = rangeOut;
        var value = (number - rangeIn.a) / (rangeIn.b - rangeIn.a) * (target.b - target.a) + target.a;
        return float.IsNaN(value) || float.IsInfinity(value) ? rangeOut.a : value;
    }
    /// <summary>
    /// Maps an int number from one range of values to another range of values.
    /// </summary>
    /// <param name="number">The number to map.</param>
    /// <param name="range">The first input range.</param>
    /// <param name="targetRange">The second input range.</param>
    /// <returns>The mapped number.</returns>
    public static int Map(this int number, (int a, int b) range, (int a, int b) targetRange)
    {
        return (int)Map((float)number, range, targetRange);
    }
    /// <summary>
    /// Returns a random float value between the given inclusive range of values.
    /// </summary>
    /// <param name="range">The two range values.</param>
    /// <param name="precision">The precision of the generated random value (default is 0).</param>
    /// <param name="seed">The seed to use for the random generator (default is NaN, 
    /// meaning randomly chosen).</param>
    /// <returns>A random float value between the specified range of values.</returns>
    public static float Random(this (float a, float b) range, float precision = 0, float seed = float.NaN)
    {
        if (range.a > range.b)
            (range.a, range.b) = (range.b, range.a);

        precision = (int)Limit(precision, (0f, 5f));
        precision = MathF.Pow(10, precision);

        range.a *= precision;
        range.b *= precision;

        var s = float.IsNaN(seed) ? Guid.NewGuid().GetHashCode() : (int)seed;
        var random = new Random(s);
        var randInt = random.Next((int)range.a, Limit((int)range.b, ((int)range.a, (int)range.b)) + 1);
        return randInt / precision;
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
        // should not roll 0 so it doesn't return true with 0% (outside of roll)
        var n = Random((1f, 100f), 0f, seed);
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

    public static int ToSeed(this int number, params int[] parameters)
    {
        var seed = 2654435769L;
        Seed(number);
        foreach (var p in parameters)
            seed = Seed(p);

        return (int)seed;

        long Seed(int a)
        {
            seed ^= a;
            seed = (seed ^ (seed >> 16)) * 2246822519L;
            seed = (seed ^ (seed >> 13)) * 3266489917L;
            seed ^= seed >> 16;
            return (int)seed;
        }
    }

    public static int ToIndex1D(this (int x, int y) indexes, (int width, int height) size)
    {
        var result = indexes.x * size.width + indexes.y;
        return Math.Clamp(result, 0, size.width * size.height);
    }
    public static (int x, int y) ToIndex2D(this int index, (int width, int height) size)
    {
        index = Math.Clamp(index, 0, size.width * size.height);
        return (index % size.width, index / size.width);
    }

    #region Backend
    private class Gate
    {
        public int entries;
        public bool value;
    }

    private static readonly Stopwatch holdFrequency = new(), holdDelay = new();
    private static readonly Dictionary<string, Gate> gates = new();
    private static readonly Dictionary<int, Random> randomCache = new();

    static Extensions()
    {
        holdFrequency.Start();
        holdDelay.Start();
    }
    #endregion
}