using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using static Pure.Engine.Utility.Alignment;

namespace Pure.Engine.Utility;

/// <summary>
/// Specifies a 9-directional alignment.
/// </summary>
public enum Alignment { TopLeft, Top, TopRight, Left, Center, Right, BottomLeft, Bottom, BottomRight }

public static class Text
{
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
            var num = new StringBuilder();
            while (i < mathExpression.Length &&
                   (char.IsDigit(mathExpression[i]) || mathExpression[i] == '.'))
            {
                num.Append(mathExpression[i]);
                i++;
            }

            i--;
            return num.ToString().ToNumber();
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
    /// Attempts to convert a string to a single-precision floating point number.
    /// </summary>
    /// <param name="text">The string to convert.</param>
    /// <returns>The single-precision floating point number represented by the input string. 
    /// If the input string is not a valid number, returns <see cref="float.NaN"/>.</returns>
    public static float ToNumber(this string text)
    {
        var cultDecPoint = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        text = text.Replace(cultDecPoint, ".");
        var parsed = float.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var result);
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
            if (OperatingSystem.IsWindows())
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (OperatingSystem.IsLinux())
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
    public static string Mask(this string text, float symbolProgress)
    {
        symbolProgress = Math.Clamp(symbolProgress, 0, 1);
        return text.Remove((int)(text.Length * symbolProgress));
    }
    public static string PadLeftAndRight(this string text, int length, char paddingCharacter = ' ')
    {
        var spaces = length - text.Length;
        var padLeft = spaces / 2 + text.Length;
        return text.PadLeft(padLeft, paddingCharacter).PadRight(length, paddingCharacter);
    }
    public static string Constrain(this string text, (int width, int height) size, bool wordWrap = true, Alignment alignment = TopLeft, float scrollProgress = 0f, float symbolProgress = 1f, char tintBrush = '#')
    {
        if (string.IsNullOrEmpty(text) || size.width <= 0 || size.height <= 0)
            return string.Empty;

        const char SPACE = ' ';
        var result = text;
        var colorTags = GetColorTags(result, tintBrush);
        result = RemoveColorTags(result, tintBrush);
        result = result.Remove((int)(result.Length * Math.Clamp(symbolProgress, 0, 1)));
        result = ApplyColorTags(result, colorTags);

        var lineList = result.TrimEnd().Replace("\r", "").Split("\n").ToList();
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
                var colors = GetColorTags(lineList[i], tintBrush);
                if (colors.Count > 0)
                    lastHiddenTag = colors[^1].tag;
            }

        var sb = new StringBuilder();
        for (var i = start; i < end; i++)
        {
            var nl = i == start ? lastHiddenTag : "\n";
            sb.Append(nl + TryAlignHorizontally(lineList[i]));
        }

        return sb.ToString();

        string TryAlignHorizontally(string line)
        {
            var tags = GetColorTags(line, tintBrush);
            line = RemoveColorTags(line, tintBrush);
            if (alignment is TopRight or Right or BottomRight)
                line = line.PadLeft(size.width, SPACE);
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
                var colors = GetColorTags(line, tintBrush);
                line = RemoveColorTags(line, tintBrush);
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
                    var lineLengthNoSpaces = lineList[i].Length - lineList[i].Count(SPACE.ToString());
                    lineList[i] = ApplyColorTags(lineList[i], colors);
                    for (var j = 0; j < colors.Count; j++)
                        colors[j] = (colors[j].index - lineLengthNoSpaces, colors[j].tag);

                    lineList[i + 1] = ApplyColorTags(lineList[i + 1], colors);
                }
            }

            int GetSafeNewLineIndex(string line, uint endLineIndex)
            {
                for (var i = (int)endLineIndex; i >= 0; i--)
                    if (line[i] == SPACE && i <= size.width)
                        return i;
                return default;
            }
        }
    }
    public static string Shorten(this string text, int maxLength, string indicator = "…", char tintBrush = '#')
    {
        // BUG does not account for tintBrush

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
    public static bool StartsWith(this string text, params string[] values)
    {
        for (var i = 0; i < values.Length; i++)
            if (text.StartsWith(values[i]))
                return true;

        return false;
    }

#region Backend
    private static List<(int index, string tag)> GetColorTags(string input, char tintBrush)
    {
        input = input.Replace(" ", "").Replace("\r", "").Replace("\n", "n");
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
    private static string RemoveColorTags(string input, char tintBrush)
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
    private static string ApplyColorTags(string input, List<(int index, string tag)> storedTags)
    {
        var realIndex = 0;
        var builder = new StringBuilder(input);
        for (var i = 0; i < builder.Length; i++)
        {
            if (builder[i] == ' ')
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
#endregion
}