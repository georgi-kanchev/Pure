using System.Collections;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
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

    public static byte[] ToData(this object? value)
    {
        // first 4 bytes indicates how many bytes the data is (max 2.15 GB)
        // then the amount of bytes of actual data

        // max size bytes indicates null, 0 may be valid for 0 item collections
        if (value == null)
            return BitConverter.GetBytes(int.MaxValue);

        var type = value.GetType();

        if (value is bool valueBool)
            return [1, 0, 0, 0, (byte)(valueBool ? 1 : 0)];
        if (value is byte valueByte)
            return [1, 0, 0, 0, valueByte];
        if (value is sbyte valueSbyte)
            return [1, 0, 0, 0, ToByte(valueSbyte)];
        if (value is char valueChar)
            return new byte[] { 2, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueChar)).ToArray();
        if (value is short valueShort)
            return new byte[] { 2, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueShort)).ToArray();
        if (value is ushort valueUshort)
            return new byte[] { 2, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueUshort)).ToArray();
        if (value is int valueInt)
            return new byte[] { 4, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueInt)).ToArray();
        if (value is uint valueUint)
            return new byte[] { 4, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueUint)).ToArray();
        if (value is float valueFl)
            return new byte[] { 4, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueFl)).ToArray();
        if (value is long valueLong)
            return new byte[] { 8, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueLong)).ToArray();
        if (value is ulong valueUlong)
            return new byte[] { 8, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueUlong)).ToArray();
        if (value is double valueDb)
            return new byte[] { 8, 0, 0, 0 }.Concat(BitConverter.GetBytes(valueDb)).ToArray();
        if (value is decimal valueDec)
        {
            var bits = decimal.GetBits(valueDec);
            var bytes = new byte[16];
            for (var i = 0; i < bits.Length; i++)
                Array.Copy(BitConverter.GetBytes(bits[i]), 0, bytes, i * 4, 4);

            return new byte[] { 16, 0, 0, 0 }.Concat(bytes).ToArray();
        }

        if (value is string valueStr)
        {
            var data = Encoding.UTF8.GetBytes(valueStr);
            return BitConverter.GetBytes(data.Length).Concat(data).ToArray();
        }

        if (type.IsEnum)
            return ToData(ChangeType(value, Enum.GetUnderlyingType(type)));
        if (IsTuple(type))
        {
            var result = new List<byte>();
            var tuple = GetTupleItems(value);
            foreach (var item in tuple)
                result.AddRange(ToData(item));

            return BitConverter.GetBytes(result.Count).Concat(result).ToArray();
        }

        if (type.IsArray)
        {
            var result = new List<byte>();
            var array = ToJagged((Array)value);
            for (var i = 0; i < array.Length; i++)
                result.AddRange(ToData(array.GetValue(i)));

            return BitConverter.GetBytes(result.Count).Concat(result).ToArray();
        }

        if (IsList(type))
        {
            var result = new List<byte>();
            var list = (IList)value;
            foreach (var item in list)
                result.AddRange(ToData(item));

            return BitConverter.GetBytes(result.Count).Concat(result).ToArray();
        }

        if (IsDictionary(type))
        {
            var result = new List<byte>();
            var dict = (IDictionary)value;
            foreach (var obj in dict)
            {
                var kvp = (DictionaryEntry)obj;
                result.AddRange(ToData(kvp.Key));
                result.AddRange(ToData(kvp.Value));
            }

            var size = BitConverter.GetBytes(result.Count);
            return size.Concat(result).ToArray();
        }

        if (IsStruct(type) || type.IsClass)
        {
            var sorted = GetFieldsInOrder(type);

            if (sorted.Count == 0)
                return BitConverter.GetBytes(int.MaxValue);

            var result = new List<byte>();
            foreach (var (_, fields) in sorted)
                foreach (var (field, space, comment) in fields)
                    result.AddRange(ToData(field.GetValue(value)));

            return BitConverter.GetBytes(result.Count).Concat(result).ToArray();
        }

        return [];
    }
    public static T? ToObject<T>(this byte[]? data)
    {
        if (data == null || data.Length == 0)
            return default;

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
    public static T? ToObject<T>(this string tsvText)
    {
        var obj = ToObject(tsvText, typeof(T));
        return obj == default ? default : (T?)obj;
    }
    public static string?[,] ToTable(this string tsvText)
    {
        var placeholders = new List<string>();
        var removedDoubleQuotes = tsvText.Replace("\"\"", QUOTE_PLACEHOLDER);
        var escaped = AddPlaceholders(removedDoubleQuotes, placeholders, "\"\"");

        var rows = escaped.Replace("\r", "").Split("\n").ToList();
        var numCols = 1;

        for (var i = 0; i < rows.Count; i++)
        {
            var columns = rows[i].Split('\t').Length;
            if (numCols < columns)
                numCols = columns;
        }

        for (var i = 0; i < rows.Count; i++)
        {
            if (rows[i].StartsWith(@"\\") == false)
                continue;

            rows.Remove(rows[i]);
            i--;
        }

        var result = new string?[rows.Count, numCols];

        for (var i = 0; i < rows.Count; i++)
        {
            var cols = rows[i].Split('\t');
            for (var j = 0; j < numCols; j++)
            {
                var value = j >= cols.Length ? null : FilterPlaceholders(cols[j], placeholders);
                value = value != null ? value.Replace(QUOTE_PLACEHOLDER, "\"") : value;
                result[i, j] = value;
            }
        }

        return result;
    }
    public static void ToStatic(this string tsvText, Type staticClass)
    {
        var table = ToTable(tsvText);
        TableToInstance(table, staticClass);
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

    public static string? ToText(this string? base64)
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
    public static string? ToText(this byte[]? dataBase64)
    {
        return dataBase64 == null || dataBase64.Length == 0 ? null : Encoding.UTF8.GetString(dataBase64);
    }

    public static string ToTSV(this object? value)
    {
        if (value == null)
            return "";

        var type = value.GetType();

        if (type.IsPrimitive || type == typeof(string))
            return $"{value}";
        if (IsTuple(type))
            return ToTSV(GetTupleItems(value).ToArray());
        if (type.IsEnum)
        {
            var enumType = Enum.GetUnderlyingType(type);
            var obj = ToObject(value.ToData(), enumType, out _);
            return Enum.ToObject(type, obj ?? 0).ToString()?.Replace(",", "") ?? "";
            // works with both flags & regular enums
        }

        if (IsList(type))
        {
            var jaggedArrayType = NestedListToJaggedArrayType(type);
            return ToTSV(ToObject(value.ToData(), jaggedArrayType, out _));
        }

        if (type.IsArray)
        {
            if (type.GetArrayRank() == 1)
            {
                var jaggedDims = GetJaggedArrayDimensionCount(type);

                if (jaggedDims > 2)
                    return $"{value}";

                // this hack works around a specific case, for example for (float, string[])[]
                // the parser thinks of the parent [] as jagged, which is kinda true
                // it still is an [] inside an [] but not in the pure jagged sense [][]
                // so we cast the inner [] to TSV which is a string rather than the current []
                // this happens BEFORE we turn the "jagged"[] to a multi-D[] (which it is not), just in time
                // so that the serializer sees some nested & escaped structure rather than a [,]
                // and happens to work & hopefully doesn't break anything else :D
                if (IsKindaJagged(value) && jaggedDims == 1)
                {
                    var arr = value as Array;
                    for (var i = 0; i < arr?.Length; i++)
                        if (arr.GetValue(i) is Array a)
                            arr.SetValue(a.ToTSV(), i);
                }

                value = ToArray(value);
                type = value.GetType();
            }

            var rank = type.GetArrayRank();
            var itemType = type.GetElementType();
            var array = (Array)value;

            if (itemType != null && IsList(itemType) && rank == 1)
            {
                var jaggedArrayType = NestedListToJaggedArrayType(itemType);
                return ToTSV(ToObject(value.ToData(), jaggedArrayType.MakeArrayType(), out _));
            }

            if (rank == 1)
            {
                var result = new string[1, array.Length];
                for (var i = 0; i < array.Length; i++)
                    result[0, i] = ToTSV(array.GetValue(i));
                return TableToTSV(result);
            }

            if (rank == 2)
            {
                if (itemType == typeof(string))
                    return TableToTSV((string[,])value);

                var result = new string[array.GetLength(0), array.GetLength(1)];
                for (var i = 0; i < array.GetLength(0); i++)
                    for (var j = 0; j < array.GetLength(1); j++)
                        result[i, j] = ToTSV(array.GetValue(i, j));

                return TableToTSV(result);
            }
        }
        else if (IsDictionary(type))
        {
            var dict = (IDictionary)value;
            var result = new string[dict.Count, 2];
            var i = 0;
            foreach (var obj in dict)
            {
                var kvp = (DictionaryEntry)obj;
                result[i, 0] = ToTSV(kvp.Key);
                result[i, 1] = ToTSV(kvp.Value);
                i++;
            }

            return TableToTSV(result);
        }
        else if ((IsStruct(type) || type.IsClass) && IsDelegate(type) == false)
            return TableToTSV(InstanceToTable(value, type));

        return $"{value}";
    }
    public static string ToTSV(this Type staticClass)
    {
        return TableToTSV(InstanceToTable(null, staticClass));
    }

    public static string ToDataAsText(this object? value, string divider = "|", string escape = "`", string multiline = "+")
    {
        return ToData("", value, divider, escape, multiline);
    }
    public static T? ToObj<T>(this string data, string divider = "|", string escape = "`", string multiline = "+")
    {
        var obj = ToObj(data, typeof(T), divider, escape, multiline);
        return obj == default ? default : (T?)obj;
    }

    public static T? ToPrimitive<T>(this string primitiveAsText) where T : struct, IComparable, IConvertible
    {
        var value = TextToPrimitive(typeof(T), primitiveAsText);
        return value == null ? null : (T)value;
    }

#region Backend
    private const string STRING_PLACEHOLDER = "—", QUOTE_PLACEHOLDER = "❝", GENERATED_FIELD = ">k__BackingField";

    private static object? ToObject(byte[]? data, Type? expectedType, out byte[]? remaining)
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
            var sorted = GetFieldsInOrder(expectedType);
            var instance = CreateInstance(expectedType);

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
    private static object? ToObject(string? tsvText, Type? expectedType)
    {
        if (expectedType == null || tsvText == null)
            return default;

        var nullable = Nullable.GetUnderlyingType(expectedType);
        if (nullable != null)
            expectedType = nullable;

        if (expectedType.IsPrimitive || expectedType == typeof(string))
            return TextToPrimitive(expectedType, tsvText);

        if (expectedType.IsEnum)
        {
            var enumType = Enum.GetUnderlyingType(expectedType);
            var value = TextToPrimitive(enumType, tsvText);

            if (value == null)
                return default;

            if (tsvText.IsNumber())
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
                    var split = tsvText.Split();

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
            var index = names.IndexOf(tsvText);
            return index == -1 ? value : values.GetValue(index);
        }

        var table = ToTable(tsvText);

        if (IsTuple(expectedType))
        {
            var fields = GetTupleFields(expectedType);
            var instance = Activator.CreateInstance(expectedType);
            var i = 0;
            foreach (var field in fields)
            {
                field.SetValue(instance, ToObject(table[0, i], field.FieldType));
                i++;
            }

            return instance;
        }

        if (expectedType.IsArray)
        {
            if (table.Length == 1 && table[0, 0] == "")
                return Array.CreateInstance(expectedType.GetElementType()!, 0);

            var isJagged = GetJaggedArrayDimensionCount(expectedType) > 1;
            if (isJagged)
                expectedType = JaggedToRegularArrayType(expectedType);

            var ranks = expectedType.GetArrayRank();

            var itemType = expectedType.GetElementType();
            if (itemType == null)
                return default;

            if (ranks == 1)
            {
                var result = Array.CreateInstance(itemType, table.Length);
                for (var i = 0; i < table.Length; i++)
                    result.SetValue(ToObject(table[0, i], itemType), i);

                return result;
            }

            if (ranks == 2)
            {
                var result = Array.CreateInstance(itemType, table.GetLength(0), table.GetLength(1));
                for (var i = 0; i < table.GetLength(0); i++)
                    for (var j = 0; j < table.GetLength(1); j++)
                        result.SetValue(ToObject(table[i, j], itemType), i, j);

                return isJagged ? ToJagged(result) : result;
            }
            // no more than 2 dimensions are supported

            return default;
        }

        if (IsList(expectedType))
        {
            // keep it straightforward, parse as an array & make it a list
            // that supports List<List<>> too
            var jaggedType = NestedListToJaggedArrayType(expectedType);
            var itemType = GetJaggedArrayElementType(jaggedType);
            var result = ToObject(tsvText, jaggedType) as Array;
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

        if (IsDictionary(expectedType))
        {
            var dict = (Activator.CreateInstance(expectedType) as IDictionary)!;
            var keyType = expectedType.GetGenericArguments()[0];
            var valueType = expectedType.GetGenericArguments()[1];

            for (var i = 0; i < table.GetLength(0); i++)
            {
                var key = ToObject(table[i, 0], keyType)!;
                dict[key] = ToObject(table[i, 1], valueType);
            }

            return dict;
        }

        if ((IsStruct(expectedType) || expectedType.IsClass) && IsDelegate(expectedType) == false)
            return TableToInstance(table, expectedType);

        return default;
    }

    private static bool IsTuple(Type type)
    {
        if (type.IsGenericType == false)
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
    private static bool IsDictionary(Type type)
    {
        return type.IsGenericType &&
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

    private static List<object?> GetTupleItems(object? tuple)
    {
        return tuple == null ? [] : GetTupleFields(tuple.GetType()).Select(f => f.GetValue(tuple)).ToList();
    }
    private static List<FieldInfo> GetTupleFields(Type tupleType)
    {
        var items = new List<FieldInfo>();

        FieldInfo? field;
        var nth = 1;
        while ((field = tupleType.GetRuntimeField($"Item{nth}")) != null)
        {
            nth++;
            items.Add(field);
        }

        return items;
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
    private static bool IsKindaJagged(object jaggedArray)
    {
        return jaggedArray is Array arr && Determine(arr);

        bool Determine(Array array)
        {
            foreach (var item in array)
            {
                if (item is not Array a)
                    continue;

                if (Determine(a) == false)
                    return true;
            }

            return false;
        }
    }

    private static string[,] InstanceToTable(object? value, Type type)
    {
        var sorted = GetFieldsInOrder(type);
        var list = new List<List<string>>();
        var maxWidth = 2;

        foreach (var (_, fields) in sorted)
            foreach (var (field, space, comment) in fields)
            {
                for (var i = 0; i < space; i++)
                {
                    list.Add([]);
                    list[^1].AddRange([" ", " "]);
                }

                if (string.IsNullOrWhiteSpace(comment) == false)
                {
                    var commentLines = comment.Replace("\r", "").Split("\n");
                    for (var i = 0; i < commentLines.Length; i++)
                    {
                        var com = commentLines[i].Split("\t");
                        var com1 = com.Length == 1 ? "\t " : com[1];

                        for (var j = 2; j < com.Length; j++)
                            com1 += $"\t{com[j]}";

                        list.Add([]);
                        list[^1].AddRange([$@"\\ {com[0]}", com1]);
                    }
                }

                var fieldName = $"{field.Name.Replace("<", "").Replace(GENERATED_FIELD, "")}";
                var fieldType = field.FieldType;
                var fieldValue = field.GetValue(value);

                list.Add([]);
                list[^1].Add(fieldName);

                if (IsTuple(fieldType))
                {
                    var items = GetTupleItems(fieldValue!);
                    for (var i = 0; i < items.Count; i++)
                        list[^1].Add(ToTSV(items[i]));

                    maxWidth = maxWidth < list[^1].Count ? list[^1].Count : maxWidth;
                    continue;
                }

                if (IsList(fieldType))
                {
                    var asList = (fieldValue as IList)!;
                    for (var i = 0; i < asList.Count; i++)
                        list[^1].Add(ToTSV(asList[i]));

                    maxWidth = maxWidth < list[^1].Count ? list[^1].Count : maxWidth;
                    continue;
                }

                if (fieldType.IsArray && fieldValue is Array arr)
                {
                    var rank = fieldType.GetArrayRank();
                    if (rank == 1)
                        for (var i = 0; i < arr.Length; i++)
                            list[^1].Add(ToTSV(arr.GetValue(i)));
                    else
                    {
                        var jagged = ToJagged(arr);
                        for (var i = 0; i < jagged.Length; i++)
                            list[^1].Add(ToTSV(jagged.GetValue(i)));
                    }

                    maxWidth = maxWidth < list[^1].Count ? list[^1].Count : maxWidth;
                    continue;
                }

                if (IsDictionary(fieldType))
                {
                    var asDict = (fieldValue as IDictionary)!;

                    foreach (DictionaryEntry obj in asDict)
                    {
                        list[^1].Add(ToTSV(obj.Key));
                        list[^1].Add(ToTSV(obj.Value));
                    }

                    maxWidth = maxWidth < list[^1].Count ? list[^1].Count : maxWidth;
                    continue;
                }

                list[^1].Add(ToTSV(fieldValue));
            }

        var result = new string[list.Count, maxWidth];
        for (var i = 0; i < list.Count; i++)
            for (var j = 0; j < list[i].Count; j++)
                result[i, j] = list[i][j];

        return result;
    }
    private static object? TableToInstance(string?[,] table, Type expectedType)
    {
        var sorted = GetFieldsInOrder(expectedType);
        var isStatic = expectedType is { IsAbstract: true, IsSealed: true };
        var instance = isStatic ? null : CreateInstance(expectedType);

        foreach (var (_, fields) in sorted)
            foreach (var (field, space, comment) in fields)
            {
                var i = -1;
                for (var j = 0; j < table.GetLength(0); j++)
                    if (table[j, 0] == field.Name.Replace("<", "").Replace(GENERATED_FIELD, ""))
                    {
                        i = j;
                        break;
                    }

                if (i == -1)
                    continue;

                var type = field.FieldType;
                var isArray = type.IsArray && type.GetArrayRank() == 1;
                var isList = IsList(type);
                var isDict = IsDictionary(type);
                var width = table.GetLength(1) - 1;

                // find the array length since the width of the table may be bigger than the array
                // the table width gets the length of the biggest array + 1 (for the field name)
                if (isList || isArray || isDict)
                    for (var j = 1; j < table.GetLength(1); j++)
                        if (table[i, j] == null)
                        {
                            width = j - 1;
                            break;
                        }

                if (IsTuple(type))
                {
                    var tupleFields = GetTupleFields(type);
                    var newTuple = Activator.CreateInstance(type);
                    var tupleIndex = 1;
                    foreach (var tupleField in tupleFields)
                    {
                        tupleField.SetValue(newTuple, ToObject(table[i, tupleIndex], tupleField.FieldType));
                        tupleIndex++;
                    }

                    field.SetValue(instance, newTuple);
                    continue;
                }

                if (isList)
                {
                    var elementType = type.GetGenericArguments()[0];
                    var list = Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IList;
                    for (var j = 0; j < width; j++)
                        if (table[i, j + 1] != null)
                            list?.Add(ToObject(table[i, j + 1], elementType));

                    field.SetValue(instance, ToObject(list.ToTSV(), field.FieldType));
                    continue;
                }

                if (isArray)
                {
                    var elementType = type.GetElementType()!;
                    var arr = Array.CreateInstance(elementType, width);
                    for (var j = 0; j < width; j++)
                        if (table[i, j + 1] != null)
                            arr.SetValue(ToObject(table[i, j + 1], elementType), j);

                    field.SetValue(instance, ToObject(arr.ToTSV(), field.FieldType));
                    continue;
                }

                if (isDict)
                {
                    var dict = (Activator.CreateInstance(type) as IDictionary)!;
                    var keyType = type.GetGenericArguments()[0];
                    var valueType = type.GetGenericArguments()[1];

                    for (var j = 0; j < width; j += 2)
                    {
                        var key = ToObject(table[i, j + 1], keyType)!;
                        dict[key] = ToObject(table[i, j + 2], valueType);
                    }

                    field.SetValue(instance, ToObject(dict.ToTSV(), field.FieldType));
                    continue;
                }

                field.SetValue(instance, ToObject(table[i, 1], field.FieldType));
            }

        return instance;
    }
    private static string TableToTSV(string[,] array)
    {
        var numRows = array.GetLength(0);
        var numCols = array.GetLength(1);
        var sb = new StringBuilder();

        for (var i = 0; i < numRows; i++)
        {
            for (var j = 0; j < numCols; j++)
            {
                if (array[i, j] == null)
                    break;

                var value = $"{array[i, j]}".Replace("\"", "\"\"");
                if (value.Contains('\t') || value.Contains('\n') || value.Contains("\"\""))
                    value = $"\"{value}\"";

                sb.Append(value);

                if (j < numCols - 1 && array[i, j + 1] != null)
                    sb.Append('\t');
            }

            if (i < numRows - 1)
                sb.AppendLine();
        }

        return sb.ToString().Replace("\r", "");
    }
    private static object? TextToPrimitive(Type type, string? text)
    {
        if (type == typeof(string))
            return text;

        if (type.IsPrimitive == false || string.IsNullOrWhiteSpace(text))
            return null;

        if (type == typeof(bool) && bool.TryParse(text, out var b))
            return b;

        var cultDecPoint = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        text = text.Replace(cultDecPoint, ".");
        decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var number);

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

        if (type == typeof(char) && char.TryParse(text, out var c))
            return c;

        return null;
    }
    private static object CreateInstance(Type type)
    {
        // creating an instance without needing a parameterless constructor such as with
        // var instance = Activator.CreateInstance(expectedType);
#pragma warning disable SYSLIB0050
        var instance = FormatterServices.GetUninitializedObject(type);
#pragma warning restore SYSLIB0050
        var emptyConstructor = type.GetConstructor(Instance | Public | NonPublic,
            null, Type.EmptyTypes, null);

        // in case there is a parameterless constructor, call it manually
        emptyConstructor?.Invoke([]);

        return instance;
    }

    private static string ToData(string front, object? value, string divider, string escape, string multiline)
    {
        if (value == null)
            return "";

        var result = "";
        var type = value.GetType();

        if (type.IsPrimitive || type == typeof(string))
        {
            var v = $"{value}";
            var esc = type == typeof(string) ? escape : "";

            v = v.Replace("\r", "");

            if (v.Contains('\n') == false)
                return $"{front}{esc}{v}{esc}";

            var split = v.Split("\n");
            var textLines = $"{front}{escape}{split[0]}{escape}";

            if (front.EndsWith(divider))
                front = $"{front[..^1]}{multiline}";

            foreach (var line in split.Skip(1))
                textLines += $"\n{front}{escape}{line}{escape}";

            return textLines;
        }

        if (type.IsEnum)
        {
            var enumType = Enum.GetUnderlyingType(type);
            var obj = ToObject(value.ToData(), enumType, out _);
            result = Enum.ToObject(type, obj ?? 0).ToString()?.Replace(",", "") ?? "";
            return $"{front}{result}";
            // works with both flags & regular enums
        }

        if (IsTuple(type))
            return GetTupleData(front, value, null, divider, escape, multiline);

        if (IsList(type))
        {
            var jaggedArrayType = NestedListToJaggedArrayType(type);
            return ToData("", ToObject(value.ToData(), jaggedArrayType, out _), divider, escape, multiline);
        }

        if (type.IsArray)
        {
            var array = (Array)value;

            array = ToJagged(array);

            var prevFront = front;
            for (var i = 0; i < array.Length; i++)
            {
                var nl = i < array.Length - 1 ? "\n" : "";
                front = $"{prevFront}{i}{divider}";
                result += $"{ToData(front, array.GetValue(i), divider, escape, multiline)}{nl}";
            }

            return result;
        }
        else if (IsDictionary(type))
        {
            var dict = (IDictionary)value;
            var i = 0;

            foreach (DictionaryEntry kvp in dict)
            {
                var nl = i < dict.Count - 1 ? "\n" : "";
                result += $"{ToData(front, kvp.Key, divider, escape, multiline)}\n";
                result += $"{ToData($" {divider}{front}", kvp.Value, divider, escape, multiline)}{nl}";
                i++;
            }

            return result;
        }
        else if ((IsStruct(type) || type.IsClass) && IsDelegate(type) == false)
        {
            var sorted = GetFieldsInOrder(type);

            front += " " + divider;

            foreach (var (depth, fields) in sorted)
                foreach (var (field, space, comment) in fields)
                {
                    var fieldName = $"{field.Name.Replace("<", "").Replace(GENERATED_FIELD, "")}";
                    var fieldValue = field.GetValue(value);

                    result += $"{divider}{fieldName}\n";

                    // necessary to not lose the field info names when passed recursively by value only
                    if (IsTuple(field.FieldType))
                    {
                        result += $"{GetTupleData(front, fieldValue, field, divider, escape, multiline)}\n\n";
                        continue;
                    }

                    result += $"{ToData(front, fieldValue, divider, escape, multiline)}\n\n";
                }

            result = result[..^2]; // removing the \n\n at the end

            return result;
        }

        return $"{value}";
    }
    private static string GetTupleData(string front, object? value, FieldInfo? field, string divider, string escape, string multiline)
    {
        var result = "";
        var items = GetTupleItems(value);
        var att = field?.GetCustomAttribute<TupleElementNamesAttribute>();

        if (att == null && value != null)
        {
            var fields = GetTupleFields(value.GetType());
            for (var i = 0; i < fields.Count; i++)
            {
                var nl = i < fields.Count - 1 ? "\n" : "";
                result += $"{ToData($"{front}{fields[i].Name}{divider}", items[i], divider, escape, multiline)}{nl}";
            }

            return result;
        }

        for (var i = 0; i < att?.TransformNames.Count; i++)
        {
            var nl = i < att.TransformNames.Count - 1 ? "\n" : "";
            result += $"{ToData($"{front}{att.TransformNames[i]}{divider}", items[i], divider, escape, multiline)}{nl}";
        }

        return result;
    }
    private static object? ToObj(string? data, Type? expectedType, string divider, string escape, string multiline)
    {
        if (expectedType == null || data == null)
            return default;

        data = data.Replace("\r", "");

        var nullable = Nullable.GetUnderlyingType(expectedType);
        if (nullable != null)
            expectedType = nullable;

        var isStr = expectedType == typeof(string);
        if (expectedType.IsPrimitive || isStr)
        {
            if (isStr == false)
                return TextToPrimitive(expectedType, data);

            var split = data.Split("\n");
            var res = split[0];

            if (res.StartsWith(escape) == false || res.EndsWith(escape) == false)
                return nullable;

            res = res[1..^1];

            for (var i = 1; i < split.Length; i++)
            {
                var line = split[i];
                if (line.StartsWith(escape) && line.EndsWith(escape))
                    line = line[1..^1];

                res += $"\n{line}";
            }

            return res;
        }

        if (expectedType.IsEnum)
        {
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
                    var split = data.Split();

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

        var strings = new List<string>();
        data = AddPlaceholders(data, strings, escape);

        var lines = data.Split("\n");

        if (expectedType.IsArray)
        {
            if (data == "")
                return Array.CreateInstance(expectedType.GetElementType()!, 0);

            var isJagged = GetJaggedArrayDimensionCount(expectedType) > 1;
            if (isJagged)
                expectedType = JaggedToRegularArrayType(expectedType);

            var ranks = expectedType.GetArrayRank();

            var itemType = expectedType.GetElementType();
            var isString = itemType == typeof(string);

            if (itemType == null)
                return default;

            var dimensions = GetDimensions(lines, divider);
            var lineIndex = 0;

            if (ranks == 1)
            {
                var result = Array.CreateInstance(itemType, dimensions[0]);
                for (var i = 0; i < result.Length; i++)
                {
                    var v = GetValue(lineIndex, isString, out var off);
                    result.SetValue(ToObj(v, itemType, divider, escape, multiline), i);
                    lineIndex += off + 1;
                }

                return result;
            }

            if (ranks == 2)
            {
                var result = Array.CreateInstance(itemType, dimensions[0], dimensions[1]);
                for (var i = 0; i < result.GetLength(0); i++)
                    for (var j = 0; j < result.GetLength(1); j++)
                    {
                        var off = 0;
                        if (lineIndex < lines.Length)
                        {
                            var v = GetValue(lineIndex, isString, out off);
                            result.SetValue(ToObj(v, itemType, divider, escape, multiline), i, j);
                        }

                        lineIndex += off + 1;
                    }

                return isJagged ? ToJagged(result) : result;
            }

            if (ranks == 3)
            {
            }

            return default;
        }

        if (IsList(expectedType))
        {
            var jaggedType = NestedListToJaggedArrayType(expectedType);
            var itemType = GetJaggedArrayElementType(jaggedType);
            var list = Activator.CreateInstance(expectedType) as IList;
        }

        if (IsDictionary(expectedType))
        {
            var dict = (Activator.CreateInstance(expectedType) as IDictionary)!;
            var keyType = expectedType.GetGenericArguments()[0];
            var valueType = expectedType.GetGenericArguments()[1];

            for (var i = 0; i < lines.Length; i++)
            {
                var key = ToObj(GetValue(i, keyType == typeof(string), out _), keyType, divider, escape, multiline)!;
                dict[key] = ToObj(GetValue(i, valueType == typeof(string), out _), valueType, divider, escape, multiline);
            }

            return dict;
        }

        if ((IsStruct(expectedType) || expectedType.IsClass) && IsDelegate(expectedType) == false)
        {
            var sorted = GetFieldsInOrder(expectedType);
            var isStatic = expectedType is { IsAbstract: true, IsSealed: true };
            var instance = isStatic ? null : CreateInstance(expectedType);

            foreach (var (_, fields) in sorted)
                foreach (var (field, space, comment) in fields)
                {
                    var index = -1;
                    for (var i = 0; i < lines.Length; i++)
                        if (lines[i] == $"{divider}{field.Name.Replace("<", "").Replace(GENERATED_FIELD, "")}")
                        {
                            index = i + 1;
                            break;
                        }

                    if (index == -1)
                        continue;

                    var type = field.FieldType;
                    var isString = type == typeof(string);

                    if (type.IsPrimitive || isString || type.IsEnum)
                    {
                        var str = GetValue(index, isString, out _);
                        field.SetValue(instance, ToObj(str, field.FieldType, divider, escape, multiline));
                        continue;
                    }

                    if (IsTuple(type))
                    {
                        var tupleFields = GetTupleFields(type);
                        var newTuple = Activator.CreateInstance(type);
                        for (var i = 0; i < tupleFields.Count; i++)
                        {
                            var tupleField = tupleFields[i];
                            var t = tupleField.FieldType;
                            var str = GetValue(index + i, t == typeof(string), out _);
                            tupleField.SetValue(newTuple, ToObj(str, t, divider, escape, multiline));
                        }

                        field.SetValue(instance, newTuple);
                        continue;
                    }
                }

            return instance;
        }

        return default;

        string GetValue(int lineIndex, bool isString, out int indexOffset)
        {
            var line = lines[lineIndex].Split(divider)[^1];
            indexOffset = 0;

            if (isString)
                line = $"{escape}{line}{escape}";
            else
                return line;

            indexOffset = -1;
            for (var i = 0; i < lines.Length; i++) // not while for better safety
            {
                lineIndex++;
                indexOffset++;

                var l = lineIndex <= lines.Length - 1 ? lines[lineIndex] : "";
                if (l.Contains(multiline) == false)
                {
                    line = FilterPlaceholders(line, strings);
                    break;
                }

                l = l.Split(multiline)[^1];
                var newLine = line == "" ? "" : "\n";
                line += $"{newLine}{escape}{l}{escape}";
            }

            return line;
        }
    }
    private static int[] GetDimensions(string[] lines, string divider)
    {
        var (maxI, maxJ, maxK) = (0, 0, 0);

        foreach (var line in lines)
        {
            var parts = line.Split(divider);

            if (parts.Length == 2)
            {
                var i = ToObj<int>(parts[0]);
                maxI = maxI < i ? i : maxI;
            }
            else if (parts.Length == 3)
            {
                var i = ToObj<int>(parts[0]);
                var j = ToObj<int>(parts[1]);
                maxI = maxI < i ? i : maxI;
                maxJ = maxJ < j ? j : maxJ;
            }
            else if (parts.Length == 4)
            {
                var i = ToObj<int>(parts[0]);
                var j = ToObj<int>(parts[1]);
                var k = ToObj<int>(parts[2]);
                maxI = maxI < i ? i : maxI;
                maxJ = maxJ < j ? j : maxJ;
                maxK = maxK < k ? k : maxK;
            }
        }

        return [maxI + 1, maxJ + 1, maxK + 1];
    }

    private static SortedDictionary<uint, List<(FieldInfo field, uint space, string comment)>> GetFieldsInOrder(Type type)
    {
        if (HasFlagDoNotSave(type))
            return [];

        var result = new SortedDictionary<uint, List<(FieldInfo field, uint space, string comment)>>();
        var props = type.GetProperties(NonPublic | Public | Instance | Static);
        var fields = type.GetFields(NonPublic | Public | Instance | Static);

        var i = (uint)1;
        foreach (var field in fields)
        {
            var isConst = field is { IsLiteral: true, IsStatic: true };
            if (IsDelegate(field.FieldType) || isConst)
                continue;

            var doNotSave = HasFlagDoNotSave(field);
            var order = field.GetCustomAttribute<SaveAtOrder>(false)?.Value ?? i;
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
                        order = prop.GetCustomAttribute<SaveAtOrder>(false)?.Value ?? i;
                        space = prop.GetCustomAttribute<Space>(false)?.NewLineAmount ?? 0;
                        comment = prop.GetCustomAttribute<Comment>(false)?.Text ?? "";
                    }
            }

            if (doNotSave)
                continue;

            result.TryAdd(order, []);
            result[order].Add((field, space, comment));
            i++;
        }

        return result;
    }
    private static T Wrap<T>(decimal value, T minValue, T maxValue) where T : struct, IComparable, IConvertible
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

    private static string FilterPlaceholders(string data, List<string> strings)
    {
        return Regex.Replace(data, STRING_PLACEHOLDER + "(\\d+)", match =>
        {
            var index = int.Parse(match.Groups[1].Value);
            return index >= 0 && index < strings.Count ?
                $"{strings[index]}" :
                match.Value;
        });
    }
    private static string AddPlaceholders(string dataAsText, List<string> strings, string escape)
    {
        var pattern = $"{Regex.Escape(escape)}(.+?){Regex.Escape(escape)}";

        return Regex.Replace(dataAsText, pattern, match =>
        {
            var replacedValue = STRING_PLACEHOLDER + strings.Count;
            var value = match.Groups[1].Value;

            if (strings.Contains(value))
                return STRING_PLACEHOLDER + strings.IndexOf(value);

            strings.Add(value);
            return replacedValue;
        });
    }
    private static bool HasFlagDoNotSave(MemberInfo memberInfo)
    {
        var attributes = memberInfo.GetCustomAttributes();

        foreach (var attribute in attributes)
            if (attribute.GetType().Name == nameof(DoNotSave))
                return true;

        return false;
    }
#endregion
}