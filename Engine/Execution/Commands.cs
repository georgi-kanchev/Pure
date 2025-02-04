namespace Pure.Engine.Execution;

using System.Text.RegularExpressions;
using System.Globalization;

[Script.DoNotSave]
public class Commands
{
    public bool IsDisabled { get; set; }
    public (char command, char value, char text, char array) Dividers { get; set; } = (';', ' ', '`', '|');

    public void Create(string command, Func<string?> onExecute)
    {
        if (string.IsNullOrWhiteSpace(command))
            return;

        command = command.Replace(Dividers.value, char.MinValue);
        command = command.Replace(Dividers.text, char.MinValue);
        command = command.Replace(Dividers.command, char.MinValue);
        command = command.Replace(Dividers.array, char.MinValue);

        commands[command] = onExecute;
    }
    public string[] Execute(string command)
    {
        if (IsDisabled)
            return [];

        var strings = AddPlaceholders(ref command, Dividers.text.ToString());
        var results = new List<string>();
        var cmds = command.Trim().Split(Dividers.command);

        foreach (var cmd in cmds)
        {
            var parts = cmd.Trim().Split();

            if (commands.ContainsKey(parts[0]) == false)
                continue;

            var callback = commands[parts[0]];

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
        return (T?)GetNextValue(typeof(T));
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
    private const string STR_PLACEHOLDER = "—";
    private int parameterIndex;
    private static readonly Dictionary<string, Func<string?>> commands = new();
    private static readonly List<string> parameters = [];

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
#endregion
}