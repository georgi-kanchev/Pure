using System.Text;

namespace Pure.Engine.Execution;

using System.Text.RegularExpressions;
using System.Globalization;

[Script.DoNotSave]
public class Commands
{
    public bool IsDisabled { get; set; }
    public (char command, char value, char text, char array) Dividers { get; set; } = (';', ' ', '`', '|');

    public void Create(string commandName, Func<string?> onExecute)
    {
        if (string.IsNullOrWhiteSpace(commandName))
            return;

        commandName = commandName.Replace(Dividers.value, char.MinValue);
        commandName = commandName.Replace(Dividers.text, char.MinValue);
        commandName = commandName.Replace(Dividers.command, char.MinValue);
        commandName = commandName.Replace(Dividers.array, char.MinValue);
        commandName = Name(commandName, Naming.lower, NAME_PLACEHOLDER);

        commands[commandName] = onExecute;
    }
    public string[] Execute(string commandName)
    {
        if (IsDisabled)
            return [];

        var strings = AddPlaceholders(ref commandName, Dividers.text.ToString());
        var results = new List<string>();
        var cmds = commandName.Trim().Split(Dividers.command);

        foreach (var cmd in cmds)
        {
            var parts = cmd.Trim().Split();
            var name = Name(parts[0], Naming.lower, NAME_PLACEHOLDER);

            if (commands.TryGetValue(name, out var callback) == false)
                continue;

            parameterIndex = 0;
            parameters.Clear();

            for (var i = 1; i < parts.Length; i++)
            {
                var part = parts[i];
                if (part.StartsWith(STR_PLACEHOLDER)) // is text
                {
                    _ = int.TryParse(part.Replace(STR_PLACEHOLDER, string.Empty), out var paramIndex);
                    parameters.Add(strings[paramIndex]);
                    continue;
                }

                parameters.Add(part);
            }

            results.Add(callback.Invoke() ?? string.Empty);
        }

        return results.ToArray();
    }
    public T? GetNextValue<T>()
    {
        var value = GetNextValue(typeof(T));
        return value == default ? default : (T?)value;
    }
    public object? GetNextValue(Type type)
    {
        if (parameterIndex >= parameters.Count)
            return default;

        var str = parameters[parameterIndex];
        parameterIndex++;

        var obj = TextToObject(str, type);
        return obj is "" or null ? default : Convert.ChangeType(obj, type);
    }

#region Backend
    // ReSharper disable InconsistentNaming
    [Flags]
    private enum Naming
    {
        RaNDomCasE, lower = 1 << 0, UPPER = 1 << 1, camelCase = 1 << 2, PascalCase = 1 << 3, Sentence_case = 1 << 4,
        PiNgPoNg_CaSe = 1 << 5, pOnGpInG_cAsE = 1 << 6, Separated = 1 << 7
    }
    // ReSharper restore InconsistentNaming

    private const string STR_PLACEHOLDER = "—", NAME_PLACEHOLDER = "╌";
    private int parameterIndex;
    private readonly Dictionary<string, Func<string?>> commands = new();
    private readonly List<string> parameters = [];

    private object? TextToObject(string dataAsText, Type type)
    {
        if (type.IsArray && IsArray(dataAsText))
            return TextToArray(dataAsText, type);

        if (type.IsPrimitive || type == typeof(string))
            return TextToPrimitive(dataAsText, type);

        return default;
    }
    private static object TextToPrimitive(string dataAsText, Type type)
    {
        if (type == typeof(bool) && bool.TryParse(dataAsText, out _))
            return Convert.ToBoolean(dataAsText);
        if (type == typeof(char) && char.TryParse(dataAsText, out _)) return Convert.ToChar(dataAsText);

        var cultDecPoint = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        dataAsText = dataAsText.Replace(cultDecPoint, ".");
        decimal.TryParse(dataAsText, NumberStyles.Any, CultureInfo.InvariantCulture, out var number);

        if (type == typeof(sbyte)) return Wrap(number, sbyte.MinValue, sbyte.MaxValue);
        if (type == typeof(byte)) return Wrap(number, byte.MinValue, byte.MaxValue);
        if (type == typeof(short)) return Wrap(number, short.MinValue, short.MaxValue);
        if (type == typeof(ushort)) return Wrap(number, ushort.MinValue, ushort.MaxValue);
        if (type == typeof(int)) return Wrap(number, int.MinValue, int.MaxValue);
        if (type == typeof(uint)) return Wrap(number, uint.MinValue, uint.MaxValue);
        if (type == typeof(long)) return Wrap(number, long.MinValue, long.MaxValue);
        if (type == typeof(ulong)) return Wrap(number, ulong.MinValue, ulong.MaxValue);
        if (type == typeof(float)) return Wrap(number, float.MinValue, float.MaxValue);
        if (type == typeof(double)) return Wrap(number, double.MinValue, double.MaxValue);
        if (type == typeof(decimal)) return number;

        return dataAsText;
    }
    private Array? TextToArray(string dataAsText, Type type)
    {
        var arrayType = type.GetElementType();

        if (arrayType == null)
            return default;

        var result = dataAsText.Split(Dividers.array);
        var resultArray = Array.CreateInstance(arrayType, result.Length);
        for (var i = 0; i < result.Length; i++)
        {
            var value = TextToPrimitive(result[i], arrayType);
            var item = value.GetType() != arrayType ? default : Convert.ChangeType(value, arrayType);
            resultArray.SetValue(item, i);
        }

        return resultArray;
    }

    private bool IsArray(string dataAsText)
    {
        return dataAsText.Contains(Dividers.array);
    }

    private static List<string> AddPlaceholders(ref string dataAsText, string dividerText)
    {
        var pattern = $"{Regex.Escape(dividerText)}(.*?){Regex.Escape(dividerText)}";
        var result = new List<string>();
        dataAsText = Regex.Replace(dataAsText, pattern, match =>
        {
            var replacedValue = STR_PLACEHOLDER + result.Count;
            result.Add(match.Groups[1].Value);
            return replacedValue;
        });
        return result;
    }
    private static T Wrap<T>(decimal value, T minValue, T maxValue) where T : struct, IComparable, IConvertible
    {
        if (typeof(T).IsPrimitive == false || typeof(T) == typeof(bool))
            throw new ArgumentException("Type must be a primitive numeric type.");

        var range = Convert.ToDouble(maxValue) - Convert.ToDouble(minValue);
        var wrappedValue = Convert.ToDouble(value);

        while (wrappedValue < Convert.ToDouble(minValue))
            wrappedValue += range;

        while (wrappedValue > Convert.ToDouble(maxValue))
            wrappedValue -= range;

        return (T)Convert.ChangeType(wrappedValue, typeof(T));
    }

    private static string Name(string input, Naming naming, string divider = "")
    {
        divider ??= "";

        if (naming == Naming.RaNDomCasE)
        {
            var result = new StringBuilder();
            for (var i = 0; i < input.Length; i++)
            {
                var rand = Random((0f, 1f)) < 0.5f;
                var symbol = input[i].ToString();
                result.Append(rand ? symbol.ToLower() : symbol.ToUpper());
            }

            return result.ToString();
        }

        var (detectedNaming, detectedDivider) = GetNaming(input);
        var words = string.IsNullOrEmpty(detectedDivider) ? [input] : input.Split(detectedDivider);

        if (words.Length == 1 && divider != "" && IsOneOf(detectedNaming, Naming.camelCase, Naming.PascalCase))
            words = AddDivCamelPascal(words[0], divider).Split(divider);

        for (var i = 0; i < words.Length; i++)
        {
            if (naming.HasFlag(Naming.lower))
                words[i] = words[i].ToLower();

            if (naming.HasFlag(Naming.UPPER))
                words[i] = words[i].ToUpper();

            if (naming.HasFlag(Naming.camelCase))
                words[i] = i == 0 ? words[i].ToLower() : Capitalize(words[i]);

            if (naming.HasFlag(Naming.PascalCase))
                words[i] = Capitalize(words[i]);

            if (naming.HasFlag(Naming.Sentence_case))
                words[i] = i == 0 ? Capitalize(words[i]) : words[i].ToLower();

            if (naming.HasFlag(Naming.PiNgPoNg_CaSe))
            {
                var result = new StringBuilder();
                var isUpper = true;
                foreach (var c in words[i])
                {
                    result.Append(isUpper ? char.ToUpper(c) : char.ToLower(c));
                    isUpper = !isUpper;
                }

                words[i] = result.ToString();
            }

            if (naming.HasFlag(Naming.pOnGpInG_cAsE) == false)
                continue;

            var res = new StringBuilder();
            var isLower = true;
            foreach (var c in words[i])
            {
                res.Append(isLower ? char.ToLower(c) : char.ToUpper(c));
                isLower = !isLower;
            }

            words[i] = res.ToString();
        }

        return ToString(words, divider);

        string Capitalize(string word)
        {
            return char.ToUpper(word[0]) + word[1..].ToLower();
        }

        string AddDivCamelPascal(string text, string div)
        {
            var result = new StringBuilder();

            for (var i = 0; i < text.Length; i++)
            {
                if (i > 0 && i <= text.Length - 1 && char.IsUpper(text[i]) && (i == text.Length - 1 || char.IsLower(text[i + 1])))
                    result.Append(div);

                result.Append(text[i]);
            }

            return result.ToString();
        }
    }
    private static (Naming naming, string divider) GetNaming(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return (Naming.RaNDomCasE, "");

        var detectedNaming = Naming.RaNDomCasE;
        var divider = "";
        var match = Regex.Match(input, "[^a-zA-Z0-9]");
        var words = new[] { input };

        if (match.Success)
        {
            divider = match.Value[0].ToString();
            detectedNaming |= Naming.Separated;
            words = input.Split(divider);
        }

        var inputNoDivider = string.IsNullOrWhiteSpace(divider) ? input : input.Replace(divider, "");
        if (inputNoDivider.All(char.IsLower))
        {
            detectedNaming |= Naming.lower;
            return (detectedNaming, divider);
        }

        if (inputNoDivider.All(char.IsUpper))
        {
            detectedNaming |= Naming.UPPER;
            return (detectedNaming, divider);
        }

        if (words.Length == 1)
        {
            if (char.IsLower(input[0]) && input.Skip(1).Any(char.IsUpper)) detectedNaming |= Naming.camelCase;
            if (char.IsUpper(input[0]) && input.Skip(1).Any(char.IsUpper)) detectedNaming |= Naming.PascalCase;
            return (detectedNaming, divider);
        }

        var soFarCamel = words[0].All(char.IsLower);
        var soFarPascal = char.IsUpper(words[0][0]) && words[0].Skip(1).All(char.IsLower);
        var soFarSentence = soFarPascal;
        var soFarPing = IsPing(words[0]);
        var soFarPong = IsPong(words[0]);

        foreach (var word in words.Skip(1))
        {
            if (word.All(char.IsLower) == false)
                soFarSentence = false;

            if (char.IsUpper(word[0]) == false || word.Skip(1).All(char.IsLower) == false)
            {
                soFarCamel = false;
                soFarPascal = false;
            }

            if (IsPing(word) == false)
                soFarPing = false;

            if (IsPong(word) == false)
                soFarPong = false;

            if (soFarCamel == false && soFarPascal == false && soFarSentence == false && soFarPing == false && soFarPong == false)
                break;
        }

        if (soFarCamel) detectedNaming |= Naming.camelCase;
        if (soFarPascal) detectedNaming |= Naming.PascalCase;
        if (soFarSentence) detectedNaming |= Naming.Sentence_case;
        if (soFarPing) detectedNaming |= Naming.PiNgPoNg_CaSe;
        if (soFarPong) detectedNaming |= Naming.pOnGpInG_cAsE;

        return (detectedNaming, divider);

        bool IsPing(string str)
        {
            var isUpper = true;
            for (var i = 0; i < str.Length; i++)
            {
                if (isUpper && char.IsUpper(str[i]) == false) return false;
                if (isUpper == false && char.IsLower(str[i]) == false) return false;
                isUpper = !isUpper;
            }

            return true;
        }

        bool IsPong(string str)
        {
            var isLower = true;
            for (var i = 0; i < str.Length; i++)
            {
                if (isLower && char.IsLower(str[i]) == false) return false;
                if (isLower == false && char.IsUpper(str[i]) == false) return false;
                isLower = !isLower;
            }

            return true;
        }
    }
    private static string ToString<T>(IList<T> collection, string divider)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < collection.Count; i++)
        {
            var sep = i != 0 ? divider : string.Empty;
            sb.Append(sep).Append(collection[i]);
        }

        return sb.ToString();
    }
    private static float Random((float a, float b) range, float seed = float.NaN)
    {
        var (a, b) = range;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (a == b)
            return a;

        if (a > b)
            (a, b) = (b, a);

        var r = b - a;

        long intSeed = float.IsNaN(seed) ? Guid.NewGuid().GetHashCode() : BitConverter.SingleToInt32Bits(seed);
        intSeed = (1103515245 * intSeed + 12345) % 2147483648;
        var normalized = (intSeed & 0x7FFFFFFF) / (float)2147483648;
        return a + normalized * r;
    }
    private static bool IsOneOf<T>(T value, params T[] values)
    {
        for (var i = 0; i < values?.Length; i++)
            if (EqualityComparer<T>.Default.Equals(value, values[i]))
                return true;

        return false;
    }
#endregion
}