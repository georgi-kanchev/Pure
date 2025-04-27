using System.Collections;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using static System.Reflection.BindingFlags;
using static System.Convert;

namespace Pure.Engine.Utility;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class SaveAtOrder(uint order) : Attribute
{
    public uint Value { get; } = order;
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class Space(uint newLineAmount = 1) : Attribute
{
    public uint NewLineAmount { get; } = newLineAmount;
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class Comment(string text) : Attribute
{
    public string Text { get; } = text;
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
public class DoNotSave : Attribute;

public static class Storage
{
    public static string Compress(this string value)
    {
        return ToBase64String(Compress(Encoding.UTF8.GetBytes(value)));
    }
    public static byte[] Compress(this byte[]? data)
    {
        try
        {
            if (data == null || data.Length == 0)
                return [];

            using var memoryStream = new MemoryStream();
            using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal, true))
            {
                gzipStream.Write(data, 0, data.Length);
            }

            return memoryStream.ToArray();
        }
        catch (Exception)
        {
            return data ?? [];
        }
    }
    public static string Decompress(this string compressedText)
    {
        return Encoding.UTF8.GetString(Decompress(FromBase64String(compressedText)));
    }
    public static byte[] Decompress(this byte[]? compressedData)
    {
        try
        {
            if (compressedData == null || compressedData.Length == 0)
                return [];

            using var compressedStream = new MemoryStream(compressedData);
            using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            gzipStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }
        catch (Exception)
        {
            return compressedData ?? [];
        }
    }

    public static byte[] ToDataAsBytes(this object? value)
    {
        // first 4 bytes indicates how many bytes the data is (max 2.15 GB)
        // then the amount of bytes of actual data

        // max size bytes indicates null, 0 may be valid for 0 item collections
        if (value == null)
            return BitConverter.GetBytes(int.MaxValue);

        var type = value.GetType();
        var result = new List<byte>();

        if (value is bool valueBool)
            return [1, 0, 0, 0, (byte)(valueBool ? 1 : 0)];
        else if (value is byte valueByte)
            return [1, 0, 0, 0, valueByte];
        else if (value is sbyte valueSbyte)
            return [1, 0, 0, 0, ToByte(valueSbyte)];
        else if (value is char valueChar)
            return new byte[] { 2, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueChar)).ToArray();
        else if (value is short valueShort)
            return new byte[] { 2, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueShort)).ToArray();
        else if (value is ushort valueUshort)
            return new byte[] { 2, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueUshort)).ToArray();
        else if (value is int valueInt)
            return new byte[] { 4, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueInt)).ToArray();
        else if (value is uint valueUint)
            return new byte[] { 4, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueUint)).ToArray();
        else if (value is float valueFl)
            return new byte[] { 4, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueFl)).ToArray();
        else if (value is long valueLong)
            return new byte[] { 8, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueLong)).ToArray();
        else if (value is ulong valueUlong)
            return new byte[] { 8, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueUlong)).ToArray();
        else if (value is double valueDb)
            return new byte[] { 8, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueDb)).ToArray();
        else if (value is decimal valueDec)
        {
            var bits = decimal.GetBits(valueDec);
            var bytes = new byte[16];
            for (var i = 0; i < bits.Length; i++)
                Array.Copy(BitConverter.GetBytes(bits[i]), 0, bytes, i * 4, 4);

            return new byte[] { 16, 0, 0, 0 }.Concat(bytes).ToArray();
        }
        else if (value is string valueStr)
        {
            var data = Encoding.UTF8.GetBytes(valueStr);
            return BitConverter.GetBytes(data.Length).Concat(data).ToArray();
        }
        else if (type.IsEnum)
            return ToDataAsBytes(ChangeType(value, Enum.GetUnderlyingType(type)));
        else if (IsTuple(type))
        {
            var tuple = GetTupleItems(value);
            foreach (var item in tuple)
                result.AddRange(ToDataAsBytes(item));

            return BitConverter.GetBytes(result.Count).Concat(result).ToArray();
        }
        else if (type.IsArray)
        {
            var array = ToJagged((Array)value);
            for (var i = 0; i < array.Length; i++)
                result.AddRange(ToDataAsBytes(array.GetValue(i)));

            return BitConverter.GetBytes(result.Count).Concat(result).ToArray();
        }
        else if (IsList(type))
        {
            var list = (IList)value;
            foreach (var item in list)
                result.AddRange(ToDataAsBytes(item));

            return BitConverter.GetBytes(result.Count).Concat(result).ToArray();
        }
        else if (IsDictionary(type))
        {
            var dict = (IDictionary)value;
            foreach (var obj in dict)
            {
                var kvp = (DictionaryEntry)obj;
                result.AddRange(ToDataAsBytes(kvp.Key));
                result.AddRange(ToDataAsBytes(kvp.Value));
            }

            var size = BitConverter.GetBytes(result.Count);
            return size.Concat(result).ToArray();
        }
        else if (IsInstance(type))
            return InstanceToBytes(value, type);

        return [];
    }
    public static byte[] ToDataAsBytes(this Type staticClass)
    {
        return IsInstance(staticClass) ? InstanceToBytes(null, staticClass) : [];
    }
    public static string ToDataAsText(this object? value)
    {
        return ToData(value, 0);
    }
    public static string ToDataAsText(this Type staticClass)
    {
        return IsInstance(staticClass) ? InstanceToData(null, staticClass, 0) : "";
    }

    public static object? ToObject(this string? data)
    {
        return data == null ? default : ToDictionary(data);
    }
    public static T? ToObject<T>(this byte[]? data)
    {
        try
        {
            var type = typeof(T);
            var obj = ToObject(data, type, out _);

            if (type == obj?.GetType())
                return (T?)obj;

            if (obj != null && type.IsArray && type.GetArrayRank() > 1) // expects regular array
                obj = ToArray(obj);

            return (T?)obj;
        }
        catch (Exception)
        {
            return default;
        }
    }
    public static T? ToObject<T>(this string? data)
    {
        try
        {
            var obj = ToObject(data, typeof(T));
            return obj == default ? default : (T?)obj;
        }
        catch (Exception)
        {
            return default;
        }
    }
    public static void ToStatic<T>(this byte[]? data)
    {
        try
        {
            ToObject(data, typeof(T), out _, true);
        }
        catch (Exception)
        {
        }
    }
    public static void ToStatic<T>(this string? data)
    {
        try
        {
            ToObject(data, typeof(T), true);
        }
        catch (Exception)
        {
        }
    }
    public static void ToStatic(this string data, Type staticClass)
    {
        try
        {
            ToObject(data, staticClass, true);
        }
        catch (Exception)
        {
        }
    }

    public static string? ToBase64(this string? value)
    {
        return string.IsNullOrEmpty(value) ? null : ToBase64(Encoding.UTF8.GetBytes(value));
    }
    public static string? ToBase64(this byte[]? data)
    {
        return data == null || data.Length == 0 ? null : ToBase64String(data);
    }
    public static byte[]? ToBase64Bytes(this string? base64)
    {
        return string.IsNullOrWhiteSpace(base64) ? null : FromBase64String(base64);
    }

    public static string? ToTextFromBase64(this string? base64)
    {
        if (string.IsNullOrEmpty(base64))
            return null;

        try
        {
            var data = FromBase64String(base64);
            return Encoding.UTF8.GetString(data);
        }
        catch (Exception)
        {
            return null;
        }
    }
    public static string? ToTextFromBase64Bytes(this byte[]? dataBase64)
    {
        return dataBase64 == null || dataBase64.Length == 0 ? null : Encoding.UTF8.GetString(dataBase64);
    }

    public static T? ToPrimitive<T>(this string data) where T : struct, IComparable, IConvertible
    {
        var value = TextToPrimitive(typeof(T), data);
        return value == null ? null : (T)value;
    }

#region Backend
    private const char ESCAPE = '"', TAB = '\t';
    private const string GENERATED_FIELD = ">k__BackingField", DIV = ": ", COMMENT = "//", ENUM_FLAG = " | ",
        NEW_LINE = "\n", TUPLE_ITEM = "Item", DICT_KEY = "key", DICT_VALUE = "value", INFINITY = "Infinity";

    private static object? ToObject(byte[]? data, Type? expectedType, out byte[]? remaining, bool isStatic = false)
    {
        remaining = null;
        if (data == null || data.Length < 4 || expectedType == null)
            return default;

        var size = BitConverter.ToInt32(data, 0);

        if (size == int.MaxValue) // this indicates null
        {
            remaining = data.AsSpan()[4..].ToArray();
            return default;
        }

        var bytes = data.AsSpan()[4..(4 + size)].ToArray();
        remaining = data.AsSpan()[(4 + bytes.Length)..].ToArray();

        if ((expectedType == typeof(bool) || expectedType == typeof(bool?)) && size == 1)
            return BitConverter.ToBoolean(bytes);
        if ((expectedType == typeof(byte) || expectedType == typeof(byte?)) && size == 1)
            return bytes[0];
        if ((expectedType == typeof(sbyte) || expectedType == typeof(sbyte?)) && size == 1)
            return bytes[0];
        if ((expectedType == typeof(char) || expectedType == typeof(char?)) && size == 2)
            return BitConverter.ToChar(bytes);
        if ((expectedType == typeof(short) || expectedType == typeof(short?)) && size == 2)
            return BitConverter.ToInt16(bytes);
        if ((expectedType == typeof(ushort) || expectedType == typeof(ushort?)) && size == 2)
            return BitConverter.ToUInt16(bytes);
        if ((expectedType == typeof(int) || expectedType == typeof(int?)) && size == 4)
            return BitConverter.ToInt32(bytes);
        if ((expectedType == typeof(uint) || expectedType == typeof(uint?)) && size == 4)
            return BitConverter.ToUInt32(bytes);
        if ((expectedType == typeof(float) || expectedType == typeof(float?)) && size == 4)
            return BitConverter.ToSingle(bytes);
        if ((expectedType == typeof(long) || expectedType == typeof(long?)) && size == 8)
            return BitConverter.ToInt64(bytes);
        if ((expectedType == typeof(ulong) || expectedType == typeof(ulong?)) && size == 8)
            return BitConverter.ToUInt64(bytes);
        if ((expectedType == typeof(double) || expectedType == typeof(double?)) && size == 8)
            return BitConverter.ToDouble(bytes);
        if ((expectedType == typeof(decimal) || expectedType == typeof(decimal?)) && size == 16)
        {
            var lo = BitConverter.ToInt32(bytes, 0);
            var mid = BitConverter.ToInt32(bytes, 4);
            var hi = BitConverter.ToInt32(bytes, 8);
            var flags = BitConverter.ToInt32(bytes, 12);
            return new decimal(lo, mid, hi, (flags & 0x80000000) != 0, (byte)((flags >> 16) & 255));
        }

        if (expectedType == typeof(string))
            return Encoding.UTF8.GetString(bytes);

        if (expectedType.IsEnum)
        {
            var enumType = Enum.GetUnderlyingType(expectedType);
            var obj = ToObject(data, enumType, out _);
            return obj == null ? null : Enum.ToObject(expectedType, obj);
        }

        if (IsTuple(expectedType))
        {
            var fields = GetTupleFields(expectedType);
            var instance = Activator.CreateInstance(expectedType);
            foreach (var field in fields)
            {
                var obj = ToObject(bytes, field.FieldType, out var left);
                bytes = left;
                field.SetValue(instance, obj);
            }

            return instance;
        }

        if (expectedType.IsArray)
        {
            // turn to jagged array, only working with int[][][], not int[,,]
            // since we may be multiple recursion levels in and not know the "final" type
            var expectingRegularArray = expectedType.GetArrayRank() > 1;
            if (expectingRegularArray)
                expectedType = RegularToJaggedArrayType(expectedType);

            var itemType = expectedType.GetElementType();
            if (itemType == null)
                return default;

            var result1D = new List<object?>();
            while (bytes is { Length: > 0 })
            {
                var obj = ToObject(bytes, itemType, out var left);
                bytes = left;
                result1D.Add(obj);
            }

            var jagged = Array.CreateInstance(itemType, result1D.Count);
            for (var i = 0; i < result1D.Count; i++)
                jagged.SetValue(result1D[i], i);

            return expectingRegularArray ? ToArray(jagged) : jagged;
        }

        if (IsList(expectedType))
        {
            // keep it straightforward, parse as an array & make it a list
            // that supports List<List<List<>>> too
            var itemType = expectedType.GetGenericArguments()[0];
            var result = ToObject(data, itemType.MakeArrayType(), out _) as Array;
            var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType)) as IList;

            if (result == null || list == null)
                return list;

            foreach (var item in result)
                list.Add(item);

            return list;
        }

        if (IsDictionary(expectedType))
        {
            var dict = Activator.CreateInstance(expectedType) as IDictionary;
            var keyType = expectedType.GetGenericArguments()[0];
            var valueType = expectedType.GetGenericArguments()[1];
            while (bytes is { Length: > 0 })
            {
                var key = ToObject(bytes, keyType, out var leftKey);
                bytes = leftKey;
                var value = ToObject(bytes, valueType, out var leftValue);
                bytes = leftValue;

                if (key != null)
                    dict![key] = value;
            }

            return dict;
        }

        if (IsStruct(expectedType) || expectedType.IsClass)
        {
            var sorted = GetFieldsInOrder(expectedType, isStatic);
            var instance = isStatic ? null : CreateInstance(expectedType);

            foreach (var (_, fields) in sorted)
                foreach (var (field, space, comment) in fields)
                {
                    var obj = ToObject(bytes, field.FieldType, out var left);
                    bytes = left;
                    field.SetValue(instance, obj);
                }

            return instance;
        }

        return default;
    }
    private static object? ToObject(string? data, Type? expectedType, bool isStatic = false)
    {
        if (expectedType == null || string.IsNullOrWhiteSpace(data))
            return default;

        var nullable = Nullable.GetUnderlyingType(expectedType);
        if (nullable != null)
            expectedType = nullable;

        if (expectedType.IsPrimitive)
            return TextToPrimitive(expectedType, data);
        else if (expectedType.IsEnum)
            return ParseEnum(data, expectedType);

        var values = ReadValues(data, expectedType);

        if (expectedType == typeof(string))
        {
            var result = "";
            var lines = data.Replace("\r", "").Split(NEW_LINE);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                line = line.StartsWith(ESCAPE) && line.EndsWith(ESCAPE) ? line[1..^1] : line;
                result += $"{(i == 0 ? "" : NEW_LINE)}{line}";
            }

            return result;
        }
        else if (IsTuple(expectedType))
        {
            var fields = GetTupleFields(expectedType);
            var instance = Activator.CreateInstance(expectedType);

            for (var i = 0; i < fields.Count; i++)
                if (values.TryGetValue(fields[i].Name, out var value))
                    fields[i].SetValue(instance, ToObject(value, fields[i].FieldType));

            return instance;
        }
        else if (expectedType.IsArray)
        {
            var isJagged = GetJaggedArrayDimensionCount(expectedType) > 1;
            if (isJagged == false)
                expectedType = RegularToJaggedArrayType(expectedType);

            var itemType = expectedType.GetElementType()!;
            var array = Array.CreateInstance(itemType, values.Count);
            for (var i = 0; i < values.Count; i++)
                if (values.TryGetValue(i.ToString(), out var value))
                    array.SetValue(ToObject(value, itemType), i);

            return isJagged ? array : ToArray(array);
        }
        else if (IsList(expectedType))
        {
            // keep it straightforward, parse as an array & make it a list
            // that supports List<List<>> too
            var jaggedType = NestedListToJaggedArrayType(expectedType);
            var itemType = GetJaggedArrayElementType(jaggedType);
            var result = ToObject(data, jaggedType) as Array;
            var list = Activator.CreateInstance(expectedType) as IList;

            if (result == null || list == null)
                return list;

            foreach (var item in result)
            {
                var elementType = list.GetType().GetGenericArguments()[0];
                if (item is Array arr && IsList(elementType))
                {
                    var dim = Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType!)) as IList;
                    for (var i = 0; i < arr.Length; i++)
                        dim?.Add(arr.GetValue(i));
                    list.Add(dim);
                    continue;
                }

                if (elementType.IsArray && elementType.GetArrayRank() > 1)
                    continue;

                list.Add(item);
            }

            return list;
        }
        else if (IsDictionary(expectedType))
        {
            var dict = (Activator.CreateInstance(expectedType) as IDictionary)!;
            var keyType = expectedType.GetGenericArguments()[0];
            var valueType = expectedType.GetGenericArguments()[1];
            var key = "";

            foreach (var (name, value) in values)
                if (name.StartsWith(DICT_KEY))
                    key = value;
                else if (name.StartsWith(DICT_VALUE))
                    dict[ToObject(key, keyType)!] = ToObject(value, valueType);

            return dict;
        }
        else if ((IsStruct(expectedType) || expectedType.IsClass) && IsDelegate(expectedType) == false)
        {
            var sorted = GetFieldsInOrder(expectedType, isStatic);
            var instance = isStatic ? null : CreateInstance(expectedType);

            foreach (var (_, fields) in sorted)
                foreach (var (field, space, comment) in fields)
                {
                    var name = field.Name.Replace("<", "").Replace(GENERATED_FIELD, "");
                    if (values.TryGetValue(name, out var value))
                        field.SetValue(instance, ToObject(value, field.FieldType));
                }

            return instance;
        }

        return default;
    }
    private static string ToData(object? value, int depth, FieldInfo? tupleField = null)
    {
        if (value == null)
            return "";

        var type = value.GetType();
        var result = "";

        if (type.IsPrimitive)
            return $"{value}".Replace("∞", "Infinity");
        else if (value is string str)
        {
            var lines = str.Replace("\r", "").Split(NEW_LINE);
            for (var i = 0; i < lines.Length; i++)
                result += $"{(lines.Length > 1 ? $"{NEW_LINE}{Tab(depth)}" : "")}{ESCAPE}{lines[i]}{ESCAPE}";

            result = depth == 0 ? result.Trim() : result;
        }
        else if (type.IsEnum)
        {
            var obj = ToObject(value.ToDataAsBytes(), Enum.GetUnderlyingType(type), out _);
            var names = Enum.ToObject(type, obj ?? 0).ToString()?.Replace(", ", ENUM_FLAG) ?? "";
            result = names;
        }
        else if (IsTuple(type))
        {
            var items = GetTupleItems(value);
            var att = tupleField?.GetCustomAttribute<TupleElementNamesAttribute>();

            var count = att?.TransformNames.Count ?? items.Count;
            for (var i = 0; i < count; i++)
            {
                var name = att?.TransformNames == null ? $"{TUPLE_ITEM}{i + 1}" : $"{att.TransformNames[i]}";
                result += $"{NEW_LINE}{Tab(depth)}{name}{DIV}{ToData(items[i], depth + 1)}";
            }

            result = depth == 0 ? result.Trim() : result;
        }
        else if (IsList(type))
        {
            var jaggedArrayType = NestedListToJaggedArrayType(type);
            result = ToData(ToObject(value.ToDataAsBytes(), jaggedArrayType, out _), depth);
        }
        else if (type.IsArray)
        {
            var array = ToJagged((Array)value);
            for (var i = 0; i < array.Length; i++)
            {
                var item = array.GetValue(i);
                var newDepth = depth + (IsIndented(item) ? 1 : 0);
                result += $"{NEW_LINE}{Tab(depth)}{i}{DIV}{ToData(item, newDepth)}";
            }

            result = depth == 0 ? result.Trim() : result;
        }
        else if (IsDictionary(type))
        {
            var dict = (IDictionary)value;
            var i = 0;
            foreach (DictionaryEntry kvp in dict)
            {
                var keyDepth = depth + (IsIndented(kvp.Key) ? 1 : 0);
                var valueDepth = depth + (IsIndented(kvp.Value) ? 1 : 0);
                result += $"{NEW_LINE}{Tab(depth)}{DICT_KEY}{i}{DIV}{ToData(kvp.Key, keyDepth)}";
                result += $"{NEW_LINE}{Tab(depth)}{DICT_VALUE}{i}{DIV}{ToData(kvp.Value, valueDepth)}";
                result = depth == 0 ? result.Trim() : result;
                i++;
            }

            return result;
        }
        else if (IsInstance(type))
            result = InstanceToData(value, type, depth);

        return result;
    }
    private static byte[] InstanceToBytes(object? value, Type type)
    {
        if (IsStruct(type) == false && type.IsClass == false)
            return [];

        var sorted = GetFieldsInOrder(type, value == null);
        if (sorted.Count == 0)
            return BitConverter.GetBytes(int.MaxValue);

        var result = new List<byte>();
        foreach (var (_, fields) in sorted)
            foreach (var (field, space, comment) in fields)
                result.AddRange(ToDataAsBytes(field.GetValue(value)));

        return BitConverter.GetBytes(result.Count).Concat(result).ToArray();
    }
    private static string InstanceToData(object? value, Type type, int depth)
    {
        var sorted = GetFieldsInOrder(type, value == null);
        var result = "";

        foreach (var (_, fields) in sorted)
            foreach (var (field, space, comment) in fields)
            {
                var fieldValue = field.GetValue(value);
                var fieldName = $"{field.Name.Replace("<", "").Replace(GENERATED_FIELD, "")}";
                var newDepth = depth + (IsIndented(fieldValue) ? 1 : 0);
                var sp = new string('\n', (int)space);
                var com = "";

                if (string.IsNullOrWhiteSpace(comment) == false)
                {
                    var comments = comment.Replace("\r", "").Split(NEW_LINE);
                    foreach (var c in comments)
                        com += $"{COMMENT}{c}\n";
                }

                result += $"{NEW_LINE}{sp}{com}{Tab(depth)}{fieldName}{DIV}{ToData(fieldValue, newDepth, field)}";
            }

        result = depth == 0 ? result.Trim() : result;
        return result;
    }
    private static object? TextToPrimitive(Type type, string? text)
    {
        if (type == typeof(string)) return text;
        if (type.IsEnum) return ParseEnum(text ?? "", type);
        if (type.IsPrimitive == false || string.IsNullOrWhiteSpace(text)) return null;

        var cultDecPoint = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        text = text.Replace(cultDecPoint, ".").ToLower();

        if (type == typeof(bool))
        {
            if (text.Contains('+') || text.Contains('y') || text.Contains('v') || text.Contains('1')) return true;
            if (text.Contains('-') || text.Contains('n') || text.Contains('x') || text.Contains('0')) return false;
            if (bool.TryParse(text, out var b)) return b;
        }

        var valid = decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var number);
        var isPosInf = text.Contains(INFINITY);
        var isNegInf = text.Contains("-" + INFINITY);
        var isNaN = text.Contains(nameof(float.NaN).ToLower()) || valid == false;
        var isPi = text.Contains(nameof(float.Pi).ToLower());
        var isMin = text.Contains("min");
        var isMax = text.Contains("max");
        var isTau = text.Contains(nameof(float.Tau).ToLower());
        var isE = text.Contains(nameof(float.E).ToLower());
        var isEpsilon = text.Contains(nameof(float.Epsilon).ToLower());

        if (type == typeof(sbyte))
        {
            if (isMin) return sbyte.MinValue;
            else if (isMax) return sbyte.MaxValue;
            return Wrap(number, sbyte.MinValue, sbyte.MaxValue);
        }

        if (type == typeof(byte))
        {
            if (isMin) return byte.MinValue;
            else if (isMax) return byte.MaxValue;
            return Wrap(number, byte.MinValue, byte.MaxValue);
        }

        if (type == typeof(short))
        {
            if (isMin) return short.MinValue;
            else if (isMax) return short.MaxValue;
            return Wrap(number, short.MinValue, short.MaxValue);
        }

        if (type == typeof(ushort))
        {
            if (isMin) return ushort.MinValue;
            else if (isMax) return ushort.MaxValue;
            return Wrap(number, ushort.MinValue, ushort.MaxValue);
        }

        if (type == typeof(int))
        {
            if (isMin) return int.MinValue;
            else if (isMax) return int.MaxValue;
            return Wrap(number, int.MinValue, int.MaxValue);
        }

        if (type == typeof(uint))
        {
            if (isMin) return uint.MinValue;
            else if (isMax) return uint.MaxValue;
            return Wrap(number, uint.MinValue, uint.MaxValue);
        }

        if (type == typeof(long))
        {
            if (isMin) return long.MinValue;
            else if (isMax) return long.MaxValue;
            return Wrap(number, long.MinValue, long.MaxValue);
        }

        if (type == typeof(ulong))
        {
            if (isMin) return ulong.MinValue;
            else if (isMax) return ulong.MaxValue;
            return Wrap(number, ulong.MinValue, ulong.MaxValue);
        }

        if (type == typeof(float))
        {
            if (isPosInf) return float.PositiveInfinity;
            else if (isNegInf) return float.NegativeInfinity;
            else if (isMax) return float.MaxValue;
            else if (isMin) return float.MinValue;
            else if (isPi) return float.Pi;
            else if (isEpsilon) return float.Epsilon;
            else if (isTau) return float.Tau;
            else if (isE) return float.E;
            else if (isNaN) return float.NaN;
            return Wrap(number, float.MinValue, float.MaxValue);
        }

        if (type == typeof(double))
        {
            if (isPosInf) return double.PositiveInfinity;
            else if (isNegInf) return double.NegativeInfinity;
            else if (isMax) return double.MaxValue;
            else if (isMin) return double.MinValue;
            else if (isPi) return double.Pi;
            else if (isEpsilon) return double.Epsilon;
            else if (isTau) return double.Tau;
            else if (isE) return double.E;
            else if (isNaN) return double.NaN;
            return Wrap(number, double.MinValue, double.MaxValue);
        }

        if (type == typeof(decimal))
        {
            if (isMin) return decimal.MinValue;
            else if (isMax) return decimal.MaxValue;
            return number;
        }

        if (type != typeof(char) || !char.TryParse(text, out var c))
            return null;

        if (isMin) return char.MinValue;
        else if (isMax) return char.MaxValue;
        return c;

        T Wrap<T>(decimal value, T minValue, T maxValue) where T : struct, IComparable, IConvertible
        {
            if (typeof(T).IsPrimitive == false || typeof(T) == typeof(bool))
                throw default!;

            var range = ToDouble(maxValue) - ToDouble(minValue);
            var wrappedValue = ToDouble(value);

            while (wrappedValue < ToDouble(minValue))
                wrappedValue += range;

            while (wrappedValue > ToDouble(maxValue))
                wrappedValue -= range;

            return (T)ChangeType(wrappedValue, typeof(T));
        }
    }
    private static object ToDictionary(string data)
    {
        var values = ReadValues(data, null);
        if (values.Count == 0)
            return ToObject<string>(values.Count == 1 ? values.First().Value : data) ?? "";

        var result = new Dictionary<string, object?>();
        foreach (var (key, value) in values)
            result[key] = ToDictionary(value);

        return result;
    }

    private static bool IsTuple(Type? type)
    {
        if (type == null || type.IsGenericType == false)
            return false;

        var genTypeDef = type.GetGenericTypeDefinition();
        return genTypeDef == typeof(Tuple<>) ||
               genTypeDef == typeof(Tuple<,>) ||
               genTypeDef == typeof(Tuple<,,>) ||
               genTypeDef == typeof(Tuple<,,,>) ||
               genTypeDef == typeof(Tuple<,,,,>) ||
               genTypeDef == typeof(Tuple<,,,,,>) ||
               genTypeDef == typeof(Tuple<,,,,,,>) ||
               genTypeDef == typeof(Tuple<,,,,,,,>) || // Max elements in a Tuple
               genTypeDef == typeof(ValueTuple<>) ||
               genTypeDef == typeof(ValueTuple<,>) ||
               genTypeDef == typeof(ValueTuple<,,>) ||
               genTypeDef == typeof(ValueTuple<,,,>) ||
               genTypeDef == typeof(ValueTuple<,,,,>) ||
               genTypeDef == typeof(ValueTuple<,,,,,>) ||
               genTypeDef == typeof(ValueTuple<,,,,,,>) ||
               genTypeDef == typeof(ValueTuple<,,,,,,,>); // Max elements in a ValueTuple
    }
    private static bool IsList(Type? type)
    {
        return type is { IsGenericType: true } && type.GetGenericTypeDefinition() == typeof(List<>);
    }
    private static bool IsDictionary(Type? type)
    {
        return type is { IsGenericType: true } &&
               (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
                type.GetGenericTypeDefinition() == typeof(SortedDictionary<,>));
    }
    private static bool IsStruct(Type type)
    {
        return type is { IsValueType: true, IsEnum: false, IsPrimitive: false };
    }
    private static bool IsDelegate(Type type)
    {
        return typeof(Delegate).IsAssignableFrom(type) && type != typeof(Delegate);
    }
    private static bool IsInstance(Type type)
    {
        return (IsStruct(type) || type.IsClass) && IsDelegate(type) == false;
    }

    private static object CreateInstance(Type type)
    {
        var instance = type.GetConstructor(Instance | Public | NonPublic, null, Type.EmptyTypes, null)?.Invoke([]);
#pragma warning disable SYSLIB0050
        return instance ?? FormatterServices.GetUninitializedObject(type);
#pragma warning restore SYSLIB0050;
    }
    private static string Tab(int count)
    {
        return new(TAB, count);
    }
    private static bool IsIndented(object? value)
    {
        if (value is string str)
            return str.Contains('\n');

        return value?.GetType() is { IsEnum: false, IsPrimitive: false };
    }
    private static object? ParseEnum(string data, Type expectedType)
    {
        data = data.Replace(", ", ENUM_FLAG);

        var enumType = Enum.GetUnderlyingType(expectedType);
        var value = TextToPrimitive(enumType, data);

        if (value == null)
            return default;

        if (data.IsNumber())
            return Enum.ToObject(expectedType, value);

        var names = Enum.GetNames(expectedType).ToList();
        var values = Enum.GetValues(expectedType);

        if (expectedType.IsDefined(typeof(FlagsAttribute)))
        {
            var result = Activator.CreateInstance(expectedType)!;

            foreach (var name in names)
            {
                var hasValue = false;
                var flag = (ulong)0;
                var split = data.Split(ENUM_FLAG);

                foreach (var s in split)
                    if (s.Trim() == name)
                    {
                        hasValue = true;
                        flag = (ulong)ChangeType(values.GetValue(names.IndexOf(s)), typeof(ulong))!;
                        break;
                    }

                if (hasValue == false)
                    continue;

                if (enumType == typeof(int)) result = (int)ChangeType(result, typeof(int)) | (int)flag;
                else if (enumType == typeof(sbyte)) result = (sbyte)((sbyte)ChangeType(result, typeof(sbyte)) | (sbyte)flag);
                else if (enumType == typeof(byte)) result = (byte)((byte)ChangeType(result, typeof(byte)) | (byte)flag);
                else if (enumType == typeof(short)) result = (short)((short)ChangeType(result, typeof(short)) | (short)flag);
                else if (enumType == typeof(ushort)) result = (ushort)((ushort)ChangeType(result, typeof(ushort)) | (ushort)flag);
                else if (enumType == typeof(uint)) result = (uint)ChangeType(result, typeof(uint)) | (uint)flag;
                else if (enumType == typeof(long)) result = (long)ChangeType(result, typeof(long)) | (long)flag;
                else if (enumType == typeof(ulong)) result = (ulong)ChangeType(result, typeof(ulong)) | flag;
            }

            return Enum.ToObject(expectedType, result);
        }

        // perhaps the enum value is a name, not value (eg Month.January, not 0 or 1)
        var index = names.IndexOf(data);
        return index == -1 ? value : values.GetValue(index);
    }
    private static Dictionary<string, string> ReadValues(string data, Type? expectedType)
    {
        data = data.Replace("\r", "");
        var lines = data.Split(NEW_LINE).ToList();
        var lns = lines.ToList();
        var result = new Dictionary<string, string>();
        while (lns.Count > 0)
        {
            var index = lns[0].IndexOf(DIV, StringComparison.Ordinal) + 1;
            var name = index == 0 ? "" : lns[0][..(index - 1)].Trim();
            var value = index >= lns[0].Length ? "" : lns[0][index..].Trim();

            if (value.StartsWith(COMMENT) || string.IsNullOrWhiteSpace(name))
            {
                lns.RemoveAt(0);
                continue;
            }

            // custom tuple names are a clusterfuck to follow since they don't live in the type itself
            // but rather in its parent declaration, that may be a class field, a local variable,
            // an array element etc
            // so they are discarded upon parsing for simplicity - they don't follow names but rather the order
            if (IsTuple(expectedType))
                name = $"{TUPLE_ITEM}{lines.Count - lns.Count + 1}";

            if (value == "")
            {
                lns.RemoveAt(0);
                var initialIndent = GetIndentation(lns[0]);
                while (lns.Count > 0 && GetIndentation(lns[0]) >= initialIndent)
                {
                    value += $"{NEW_LINE}{lns[0]}";
                    lns.RemoveAt(0);
                }

                result[name] = value[1..];
                continue;
            }

            result[name] = value;
            lns.RemoveAt(0);
        }

        return result;
    }
    private static int GetIndentation(string text)
    {
        var result = 0;
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != TAB)
                break;
            result++;
        }

        return result;
    }

    private static List<object?> GetTupleItems(object? tuple)
    {
        return tuple == null ? [] : GetTupleFields(tuple.GetType()).Select(f => f.GetValue(tuple)).ToList();
    }
    private static List<FieldInfo> GetTupleFields(Type tupleType)
    {
        var items = new List<FieldInfo>();

        FieldInfo? field;
        var nth = 1;
        while ((field = tupleType.GetRuntimeField($"{TUPLE_ITEM}{nth}")) != null)
        {
            nth++;
            items.Add(field);
        }

        return items;
    }
    private static SortedDictionary<uint, List<(FieldInfo field, uint space, string comment)>> GetFieldsInOrder(Type type, bool isStatic)
    {
        if (HasFlagDoNotSave(type))
            return [];

        var result = new SortedDictionary<uint, List<(FieldInfo field, uint space, string comment)>>();
        var flags = NonPublic | Public | (isStatic ? Static : Instance);
        var props = type.GetProperties(flags).ToList();
        var fields = type.GetFields(flags).ToList();
        var currOrder = (uint)1;
        var handledTypes = new List<Type> { type };

        // all properties on the parent type have their generated fields living in that parent type
        // so when getting the child properties, they don't have them, this below adds them
        for (var i = 0; i < props.Count; i++)
        {
            var parentType = props[i].DeclaringType;
            if (parentType == null || parentType == type || handledTypes.Contains(parentType))
                continue;

            var fds = parentType.GetFields(flags);
            foreach (var field in fds)
                if (field.Name.Contains(GENERATED_FIELD))
                    fields.Add(field);

            handledTypes.Add(parentType);
        }

        foreach (var field in fields)
        {
            var isConst = field is { IsLiteral: true, IsStatic: true };
            if (IsDelegate(field.FieldType) || isConst)
                continue;

            var doNotSave = HasFlagDoNotSave(field);
            var order = field.GetCustomAttribute<SaveAtOrder>(false)?.Value ?? currOrder;
            var space = field.GetCustomAttribute<Space>(false)?.NewLineAmount ?? 0;
            var comment = field.GetCustomAttribute<Comment>(false)?.Text ?? "";

            // some fields are auto generated by a property, so extract the attributes
            // from said property (searching by name) and use them for the field
            if (field.Name.Contains(GENERATED_FIELD))
            {
                var name = field.Name.Replace("<", "").Replace(GENERATED_FIELD, "");
                foreach (var prop in props)
                    if (prop.Name == name)
                    {
                        doNotSave = HasFlagDoNotSave(prop);
                        order = prop.GetCustomAttribute<SaveAtOrder>(false)?.Value ?? currOrder;
                        space = prop.GetCustomAttribute<Space>(false)?.NewLineAmount ?? 0;
                        comment = prop.GetCustomAttribute<Comment>(false)?.Text ?? "";
                    }
            }

            if (doNotSave)
                continue;

            result.TryAdd(order, []);
            result[order].Add((field, space, comment));
            currOrder++;
        }

        return result;
    }
    private static bool HasFlagDoNotSave(MemberInfo memberInfo)
    {
        var attributes = memberInfo.GetCustomAttributes();

        foreach (var attribute in attributes)
            if (attribute.GetType().Name == nameof(DoNotSave))
                return true;

        return false;
    }

    private static Array ToJagged(Array array)
    {
        return ConvertToJagged(array, 0, new int[array.Rank]);

        Array ConvertToJagged(Array arr, int currentDimension, int[] indices)
        {
            var length = arr.GetLength(currentDimension);
            var elementType = arr.GetType().GetElementType();

            if (currentDimension == arr.Rank - 1)
            {
                var jaggedArray = Array.CreateInstance(elementType!, length);
                for (var i = 0; i < length; i++)
                {
                    indices[currentDimension] = i;
                    jaggedArray.SetValue(arr.GetValue(indices), i);
                }

                return jaggedArray;
            }
            else
            {
                var innerType = CreateJaggedArrayType(elementType!, arr.Rank - currentDimension - 1);
                var jaggedArray = Array.CreateInstance(innerType, length);

                for (var i = 0; i < length; i++)
                {
                    indices[currentDimension] = i;
                    jaggedArray.SetValue(ConvertToJagged(arr, currentDimension + 1, indices), i);
                }

                return jaggedArray;
            }
        }
    }
    private static Array ToArray(object jaggedArray)
    {
        var elementType = GetJaggedArrayElementType(jaggedArray.GetType());
        var dimensions = GetJaggedArrayDimensions(jaggedArray);
        var flatList = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType!)) as IList;
        var regularArray = Array.CreateInstance(elementType!, dimensions);
        var indices = new int[dimensions.Length];
        var flatIndex = 0;
        var totalSize = 1;

        Flatten(jaggedArray);

        foreach (var dim in dimensions)
            totalSize *= dim;

        for (var i = 0; i < totalSize; i++)
        {
            var value = flatIndex < flatList!.Count ? flatList[flatIndex++] : default;
            regularArray.SetValue(value, indices);

            for (var d = dimensions.Length - 1; d >= 0; d--)
            {
                indices[d]++;
                if (indices[d] < dimensions[d])
                    break;
                indices[d] = 0;
            }
        }

        return regularArray;

        void Flatten(object? array)
        {
            if (array is Array arr)
                foreach (var item in arr)
                    Flatten(item);
            else if (array != null)
                flatList!.Add(array);
        }
    }
    private static int[] GetJaggedArrayDimensions(object jaggedArray)
    {
        var result = new List<int>();
        DetermineDimensions(jaggedArray, 0, result);
        return result.ToArray();

        void DetermineDimensions(object array, int level, List<int> dimensions)
        {
            if (array is not Array arr)
                return;

            if (dimensions.Count <= level)
                dimensions.Add(arr.Length);
            else
                dimensions[level] = Math.Max(dimensions[level], arr.Length);

            foreach (var item in arr)
                DetermineDimensions(item, level + 1, dimensions);
        }
    }
    private static int GetJaggedArrayDimensionCount(Type jaggedArrayType)
    {
        var dimensions = 0;
        var currentType = jaggedArrayType;

        while (currentType is { IsArray: true })
        {
            dimensions++;
            currentType = currentType.GetElementType();
        }

        return dimensions;
    }
    private static Type RegularToJaggedArrayType(Type type)
    {
        var rank = type.GetArrayRank();
        var jaggedArrayType = type.GetElementType()!;

        for (var i = 0; i < rank; i++)
            jaggedArrayType = jaggedArrayType.MakeArrayType();

        return jaggedArrayType;
    }
    private static Type JaggedToRegularArrayType(Type type)
    {
        var rank = 0;
        while (type.IsArray)
        {
            type = type.GetElementType()!;
            rank++;
        }

        return rank > 0 ? type.MakeArrayType(rank) : type;
    }
    private static Type CreateJaggedArrayType(Type baseType, int dimensions)
    {
        var arrayType = baseType;
        for (var i = 0; i < dimensions; i++)
            arrayType = arrayType.MakeArrayType();
        return arrayType;
    }
    private static Type NestedListToJaggedArrayType(Type nestedListType)
    {
        var depth = 0;
        var currentType = nestedListType;

        while (IsList(currentType))
        {
            depth++;
            currentType = currentType.GetGenericArguments()[0];
        }

        var jaggedArrayType = currentType;
        for (var i = 0; i < depth; i++)
            jaggedArrayType = jaggedArrayType.MakeArrayType();

        return jaggedArrayType;
    }
    private static Type? GetJaggedArrayElementType(Type jaggedArrayType)
    {
        var elementType = jaggedArrayType.GetElementType();
        while (elementType is { IsArray: true })
            elementType = elementType.GetElementType();
        return elementType;
    }
#endregion
}