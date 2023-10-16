namespace Pure.Engine.Commands;

using System.Globalization;

public class CommandManager
{
    public bool IsDisabled
    {
        get;
        set;
    }

    public char Divider
    {
        get;
        set;
    } = ';';
    public char DividerValue
    {
        get;
        set;
    } = ' ';
    public char DividerText
    {
        get;
        set;
    } = '`';
    public char DividerArray
    {
        get;
        set;
    } = '|';

    public void Add(string commandName, Func<string?> onExecute)
    {
        if (string.IsNullOrWhiteSpace(commandName))
            return;

        commandName = commandName.Replace(DividerValue, char.MinValue);
        commandName = commandName.Replace(DividerText, char.MinValue);
        commandName = commandName.Replace(Divider, char.MinValue);
        commandName = commandName.Replace(DividerArray, char.MinValue);

        commands[commandName] = onExecute;
    }
    public string[] Execute(string command)
    {
        if (IsDisabled)
            return Array.Empty<string>();

        var strings = GetStrings(ref command);
        var results = new List<string>();
        var cmds = command.Trim().Split(Divider);

        foreach (var cmd in cmds)
        {
            var parts = cmd.Trim().Split();
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
        if (parameterIndex >= parameters.Count)
            return default;

        var str = parameters[parameterIndex];
        parameterIndex++;

        var obj = TextToObject<T>(str);
        return obj is "" or null ? default : (T)obj;
    }

#region Backend
    private const string STR_PLACEHOLDER = "—";
    private static int parameterIndex;
    private static readonly Dictionary<string, Func<string?>> commands = new();
    private static readonly List<string> parameters = new();

    private object? TextToObject<T>(string dataAsText)
    {
        var t = typeof(T);
        if (t.IsArray && IsArray(dataAsText))
            return TextToArray(dataAsText, t);

        if (t.IsPrimitive || t == typeof(string))
            return TextToPrimitive(dataAsText, t);

        return default;
    }

    private static object TextToPrimitive(string dataAsText, Type type)
    {
        if (type == typeof(bool) && bool.TryParse(dataAsText, out _))
            return Convert.ToBoolean(dataAsText);
        if (type == typeof(char) && char.TryParse(dataAsText, out _)) return Convert.ToChar(dataAsText);

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

        var result = dataAsText.Split(DividerArray);
        var resultArray = Array.CreateInstance(arrayType, result.Length);
        for (var i = 0; i < result.Length; i++)
        {
            var value = TextToPrimitive(result[i], arrayType);
            var item = value.GetType() != arrayType ? default : Convert.ChangeType(value, arrayType);
            resultArray.SetValue(item, i);
        }

        return resultArray;
    }

    private static T Wrap<T>(decimal value, T minValue, T maxValue)
        where T : struct, IComparable, IConvertible
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
        return dataAsText.Contains(DividerArray);
    }

    private List<string> GetStrings(ref string input)
    {
        var result = new List<string>();
        var count = 0;

        while (input.Contains(DividerText))
        {
            var openIndex = input.IndexOf(DividerText, 0);
            if (openIndex == -1)
                break;

            var closeIndex = input.IndexOf(DividerText, openIndex + 1);

            if (closeIndex == -1)
                closeIndex = openIndex + 1;

            var extractedString = input.Substring(openIndex + 1, closeIndex - openIndex - 1);
            result.Add(extractedString);

            var placeholder = STR_PLACEHOLDER + count;
            input = string.Concat(input.AsSpan(0, openIndex), placeholder, input.AsSpan(closeIndex + 1));

            count++;
        }

        return result;
    }
#endregion
}