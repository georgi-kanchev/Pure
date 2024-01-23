using System.Text.RegularExpressions;

namespace Pure.Engine.Storage;

using System;
using System.Collections;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Text;

/// <summary>
/// Provides a simple key-value storage system that can serialize and deserialize objects 
/// to and from text/bytes. The supported types are:<br></br>
/// A. Primitives and Strings<br></br>
/// B. Tuples of A<br></br>
/// C. Arrays of A and B<br></br>
/// D. Lists of A and B<br></br>
/// E. Dictionaries of A and B<br></br>
/// </summary>
public class Storage
{
    public (string? common, string? text, string? tuple) Dividers
    {
        get => (sep, sepStr, sepTuple);
        set
        {
            sep = string.IsNullOrEmpty(value.common) ? " " : value.common;
            sepStr = string.IsNullOrEmpty(value.text) ? "`" : value.text;
            sepTuple = string.IsNullOrEmpty(value.tuple) ? ";" : value.tuple;
        }
    }
    public (string? oneD, string? twoD, string? dictionary) DividersCollection
    {
        get => (sep1D, sep2D, sepDict);
        set
        {
            sep1D = string.IsNullOrEmpty(value.oneD) ? "|" : value.oneD;
            sep2D = string.IsNullOrEmpty(value.twoD) ? $"|{Environment.NewLine}" : value.twoD;
            sepDict = string.IsNullOrEmpty(value.dictionary) ? "/" : value.dictionary;
        }
    }

    public Storage()
    {
        Dividers = default;
        DividersCollection = default;
    }
    public Storage(byte[] bytes)
    {
        LoadFromBytes(bytes);
    }
    public Storage(string dataAsText, bool isBase64 = false)
    {
        if (isBase64)
        {
            LoadFromBytes(Convert.FromBase64String(dataAsText));
            return;
        }

        Dividers = default;
        DividersCollection = default;
        LoadFromText(dataAsText);
    }

    public string ToText()
    {
        if (string.IsNullOrEmpty(Dividers.common))
            return string.Empty;

        var result = new StringBuilder();
        foreach (var kvp in data)
        {
            result.Append(kvp.Key.key);
            result.Append(Dividers.common);
            result.Append(kvp.Key.typeId);
            result.Append(Dividers.common);
            result.Append(kvp.Value);
            result.Append(Dividers.common + Environment.NewLine);
        }

        var length = Dividers.common.Length + Environment.NewLine.Length;
        result.Remove(result.Length - length, length);

        var placeholders = RemovePlaceholders(result.ToString());
        return placeholders;
    }
    public string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public byte[] ToBytes()
    {
        var result = new List<byte>();
        var text = ToText();

        PutString(result, Dividers.common ?? "");
        PutString(result, Dividers.text ?? "");
        PutString(result, Dividers.tuple ?? "");
        PutString(result, DividersCollection.oneD ?? "");
        PutString(result, DividersCollection.twoD ?? "");
        PutString(result, DividersCollection.dictionary ?? "");
        PutString(result, text);

        return Compress(result.ToArray());

        void PutString(List<byte> intoBytes, string value)
        {
            var b = Encoding.UTF8.GetBytes(value);
            intoBytes.AddRange(BitConverter.GetBytes(b.Length));
            intoBytes.AddRange(b);
        }
    }

    /// <summary>
    /// Sets the specified object instance with the given key and type identifier.
    /// </summary>
    /// <param name="key">The key used to store the object.</param>
    /// <param name="instance">The object instance to be stored.</param>
    /// <param name="typeId">The type identifier of the object instance.</param>
    public void Set(string key, object? instance, int typeId = default)
    {
        data[(key, typeId)] = AddPlaceholders(TextFromObject(instance, typeId));
    }
    /// <summary>
    /// Removes the object with the specified key from storage.
    /// </summary>
    /// <param name="key">The key of the object to be removed.</param>
    public void Remove(string key)
    {
        var keyToRemove = default((string, int));
        foreach (var kvp in data)
            if (kvp.Key.key == key)
            {
                keyToRemove = kvp.Key;
                break;
            }

        data.Remove(keyToRemove);
    }
    public bool IsContaining(string key)
    {
        foreach (var kvp in data)
            if (kvp.Key.key == key)
                return true;

        return false;
    }

    /// <summary>
    /// Gets the type ID of the object instance with the specified key.
    /// </summary>
    /// <param name="key">The key of the object instance.</param>
    /// <returns>The type ID of the object instance.</returns>
    public int GetTypeId(string key)
    {
        foreach (var kvp in data)
            if (kvp.Key.key == key)
                return kvp.Key.typeId;

        return default;
    }
    /// <summary>
    /// Gets the object instance with the specified key as text.
    /// </summary>
    /// <param name="key">The key of the object instance.</param>
    /// <returns>The object instance as text.</returns>
    public string? GetText(string key)
    {
        foreach (var kvp in data)
            if (kvp.Key.key == key)
                return kvp.Value;

        return default;
    }
    /// <summary>
    /// Gets the object instance with the specified key as an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object instance.</typeparam>
    /// <param name="key">The key of the object instance.</param>
    /// <returns>The object instance as an object of type <typeparamref name="T"/>.</returns>
    public T? GetObject<T>(string key)
    {
        foreach (var kvp in data)
            if (kvp.Key.key == key)
                return TextToObject<T>(kvp.Value, kvp.Key.typeId);

        return default;
    }

    public string ObjectToText(object? instance, int typeId = default)
    {
        // no need deal with placeholders since we would never use them & the result
        // goes straight back to the user, unlike in the storage's case where it's kept
        //
        // see brother method for more clarification
        return TextFromObject(instance, typeId);
    }
    public T? ObjectFromText<T>(string? dataAsText, int typeId = default)
    {
        // this method and their brother are a bit nomadic in this class, they have
        // nothing to do with the storage & are purely for converting objects
        // from and to text, they can't be static because i said so
        //
        // to encapsulate this data's strings, we hijack the placeholders of
        // the storage and then leave no tracks behind us while escaping with
        // our precious result
        //
        // this is needed since strings may contain separator values

        var prevCount = strings.Count;
        dataAsText = AddPlaceholders(dataAsText ?? "");
        var result = TextToObject<T>(dataAsText, typeId);

        if (prevCount != strings.Count)
            strings.RemoveRange(prevCount, strings.Count - prevCount);

        return result;
    }

    public void OnObjectFromText(int typeId, Func<string, object> method)
    {
        onObjectFromText[typeId] = method;
    }
    public void OnObjectToText(int typeId, Func<object, string> method)
    {
        onObjectToText[typeId] = method;
    }

    public Storage Copy()
    {
        return new(ToBytes());
    }

    public static implicit operator Storage(byte[] bytes)
    {
        return new(bytes);
    }
    public static implicit operator byte[](Storage storage)
    {
        return storage.ToBytes();
    }

#region Backend
    private readonly Dictionary<int, Func<string, object>> onObjectFromText = new();
    private readonly Dictionary<int, Func<object, string>> onObjectToText = new();
    private readonly Dictionary<(string key, int typeId), string?> data = new();
    private const string STR_PLACEHOLDER = "—";
    private string? sep1D, sep2D, sepTuple, sepDict, sep, sepStr;
    private readonly List<string> strings = new();

    private void LoadFromBytes(byte[] bytes)
    {
        var b = Decompress(bytes);
        var offset = 0;

        Dividers = (GrabString(), GrabString(), GrabString());
        DividersCollection = (GrabString(), GrabString(), GrabString());

        var text = GrabString();
        LoadFromText(text);

        string GrabString()
        {
            var textBytesLength = BitConverter.ToInt32(GetBytesFrom(b, 4, ref offset));
            var bText = GetBytesFrom(b, textBytesLength, ref offset);
            return Encoding.UTF8.GetString(bText);
        }
    }
    private void LoadFromText(string text)
    {
        try
        {
            var str = AddPlaceholders(text);
            var split = str.Split(Dividers.common);
            var index = 0;
            var key = string.Empty;
            var typeId = 0;

            foreach (var s in split)
            {
                var curIndex = index;
                index++;
                if (curIndex == 0)
                {
                    key = s.Trim();
                    continue;
                }
                else if (curIndex == 1)
                {
                    typeId = int.Parse(s.Trim());
                    continue;
                }

                data[(key, typeId)] = s.Trim();
                index = 0;
            }
        }
        catch (Exception)
        {
            throw new ArgumentException("Failed to process data! " +
                                        "Make sure the data is valid and " +
                                        "export/import dividers are the same.");
        }
    }

    private string RemovePlaceholders(string dataAsText)
    {
        return Regex.Replace(dataAsText, "—(\\d+)", match =>
        {
            var index = int.Parse(match.Groups[1].Value);
            return index >= 0 && index < strings.Count ?
                $"{Dividers.text}{strings[index]}{Dividers.text}" :
                match.Value;
        });
    }
    private string AddPlaceholders(string dataAsText)
    {
        return Regex.Replace(dataAsText, "`([^`]+)`", match =>
        {
            var replacedValue = "—" + strings.Count;
            strings.Add(match.Groups[1].Value);
            return replacedValue;
        });
    }

    private string TextFromObject(object? instance, int typeId)
    {
        if (instance == null)
            return string.Empty;

        var t = instance.GetType();

        if (typeId != default)
        {
            onObjectToText.TryGetValue(typeId, out var value);
            return value?.Invoke(instance) ?? string.Empty; // ask user for parse
        }

        if (t.IsPrimitive || t == typeof(string))
            return TextFromPrimitiveOrTuple(instance);
        else if (IsPrimitiveTuple(t))
            return TextFromTuple(instance);
        else if (IsGenericList(t) || (t.IsArray && IsArrayOfPrimitives((Array)instance)))
            return TextFromArrayOrList(instance);
        else if (IsPrimitiveOrTupleDictionary(t))
            return TextFromDictionary((IDictionary)instance);

        return string.Empty;
    }
    private T? TextToObject<T>(string? dataAsText, int typeId)
    {
        if (string.IsNullOrWhiteSpace(dataAsText))
            return default;

        var result = default(object);

        if (typeId != default)
        {
            onObjectFromText.TryGetValue(typeId, out var value);
            result = value?.Invoke(dataAsText); // ask user for parse
        }

        var t = typeof(T);
        if ((t.IsArray || IsGenericList(t)) && IsArray(dataAsText))
            result = TextToArrayOrList(dataAsText, t);
        else if (IsPrimitiveTuple(t))
            result = TextToTuple(dataAsText, t);
        else if (IsPrimitiveOrTupleDictionary(t))
            result = TextToDictionary(dataAsText, t);
        else if (t.IsPrimitive || t == typeof(string))
            result = TextToPrimitive(dataAsText, t);

        return result is "" or null ? default : (T)result;
    }

    private string TextFromDictionary(IDictionary dict)
    {
        if (string.IsNullOrEmpty(DividersCollection.dictionary))
            return string.Empty;

        var result = new StringBuilder();
        foreach (var obj in dict)
        {
            var kvp = (DictionaryEntry)obj;
            var key = TextFromPrimitiveOrTuple(kvp.Key);
            var value = TextFromPrimitiveOrTuple(kvp.Value);

            result.Append(DividersCollection.dictionary);
            result.Append(key);
            result.Append(DividersCollection.dictionary);
            result.Append(value);
        }

        result.Remove(0, DividersCollection.dictionary.Length);
        return result.ToString();
    }
    private object? TextToDictionary(string dataAsText, Type type)
    {
        var keyType = type.GenericTypeArguments[0];
        var valueType = type.GenericTypeArguments[1];
        var dictType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        var instance = Activator.CreateInstance(dictType);
        if (instance == null)
            return default;

        var dict = (IDictionary)instance;

        var kvpStrings = dataAsText.Split(DividersCollection.dictionary);
        var lastKey = default(object);
        for (var i = 0; i < kvpStrings.Length; i++)
        {
            var isKey = i % 2 == 0;
            var curType = isKey ? keyType : valueType;
            var cur = TextToPrimitiveOrTuple(kvpStrings[i], curType);

            if (lastKey != null && isKey == false)
                dict[lastKey] = cur;

            if (isKey)
                lastKey = cur;
        }

        return dict;
    }

    private string TextFromTuple(object tuple)
    {
        var result = string.Empty;

        var items = GetTupleItems(tuple);
        for (var i = 0; i < items.Count; i++)
            result += (i != 0 ? Dividers.tuple : string.Empty) + TextFromPrimitiveOrTuple(items[i]);

        return result;
    }
    private object? TextToTuple(string dataAsText, Type type)
    {
        var instance = Activator.CreateInstance(type);
        var items = dataAsText.Split(Dividers.tuple);
        var genTypes = type.GenericTypeArguments;
        var minLength = Math.Min(items.Length, genTypes.Length);
        for (var i = 0; i < minLength; i++)
        {
            var item = TextToPrimitive(items[i], genTypes[i]);
            var field = type.GetField($"Item{i + 1}");

            if (field == null)
                continue;

            // this makes the same flexible primitive cast but in tuples,
            // tries to satisfy the tuple's field
            var areNumber = IsNumber(genTypes[i]) && IsNumber(item.GetType());
            if (field.FieldType != item.GetType() && areNumber == false) // diff type or not numbers
                continue;

            field.SetValue(instance, item);
        }

        return instance;
    }

    private string TextFromArrayOrList(object collection)
    {
        if (string.IsNullOrEmpty(DividersCollection.oneD) ||
            string.IsNullOrEmpty(DividersCollection.twoD))
            return string.Empty;

        var result = new StringBuilder();
        var isArray = collection is Array;

        if (isArray == false) // is list
        {
            var list = (IList)collection;
            for (var i = 0; i < list.Count; i++)
            {
                var separator = i == 0 ? string.Empty : DividersCollection.oneD;
                result.Append(separator + TextFromPrimitiveOrTuple(list[i]));
            }

            return result.ToString();
        }

        var array = (Array)collection;
        if (array.Rank == 1)
        {
            for (var i = 0; i < array.GetLength(0); i++)
            {
                var separator = i == 0 ? string.Empty : DividersCollection.oneD;
                result.Append(separator + TextFromPrimitiveOrTuple(array.GetValue(i)));
            }
        }
        else if (array.Rank == 2)
        {
            for (var i = 0; i < array.GetLength(0); i++)
            {
                result.Append(DividersCollection.twoD);

                for (var j = 0; j < array.GetLength(1); j++)
                {
                    var separator = j == 0 ? string.Empty : DividersCollection.oneD;
                    result.Append(separator + TextFromPrimitiveOrTuple(array.GetValue(i, j)));
                }
            }

            result.Remove(0, DividersCollection.twoD.Length);
        }
        else if (array.Rank == 3)
        {
            for (var i = 0; i < array.GetLength(0); i++)
            {
                result.Append(DividersCollection.twoD);

                for (var j = 0; j < array.GetLength(1); j++)
                {
                    result.Append(DividersCollection.twoD);

                    for (var k = 0; k < array.GetLength(2); k++)
                    {
                        var separator = k == 0 ? string.Empty : DividersCollection.oneD;
                        result.Append(separator + TextFromPrimitiveOrTuple(array.GetValue(i, j, k)));
                    }
                }
            }

            result.Remove(0, DividersCollection.twoD.Length * 2);
        }

        return result.ToString();
    }
    private object TextToPrimitive(string dataAsText, Type type)
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

        if (dataAsText.StartsWith(STR_PLACEHOLDER) == false)
            return dataAsText;

        _ = int.TryParse(dataAsText.Replace(STR_PLACEHOLDER, string.Empty), out var paramIndex);
        return strings[paramIndex];
    }
    private IList? TextToArrayOrList(string dataAsText, Type type)
    {
        var arrayType = type.GetElementType();
        var isList = IsGenericList(type);

        if (isList)
            arrayType = type.GenericTypeArguments[0];
        if (arrayType == null)
            return default;

        var resultArrays2D = new List<string[]>();
        var resultArrays1D = new List<string[]>();
        var sep3D = DividersCollection.twoD + DividersCollection.twoD;
        var array3D = dataAsText.Split(sep3D);

        foreach (var str3D in array3D)
        {
            var array2D = str3D.Split(DividersCollection.twoD);
            resultArrays2D.Add(array2D);
            foreach (var str2D in array2D)
            {
                var array1D = str2D.Split(DividersCollection.oneD);
                resultArrays1D.Add(array1D);
            }
        }

        var length3D = array3D.Length;
        var length2D = resultArrays2D[0].Length;
        var length1D = resultArrays1D[0].Length;
        var resultArray3D = Array.CreateInstance(arrayType, length3D, length2D, length1D);
        var resultArray2D = Array.CreateInstance(arrayType, length2D, length1D);
        var resultArray1D = Array.CreateInstance(arrayType, length1D);
        for (var i = 0; i < array3D.Length; i++)
        {
            var array2D = array3D[i].Split(DividersCollection.twoD);
            for (var j = 0; j < array2D.Length; j++)
            {
                var array1D = array2D[j].Split(DividersCollection.oneD);
                for (var k = 0; k < array1D.Length; k++)
                {
                    var item = Convert.ChangeType(TextToPrimitiveOrTuple(array1D[k], arrayType),
                        arrayType);
                    resultArray1D.SetValue(item, k);
                    resultArray2D.SetValue(item, j, k);
                    resultArray3D.SetValue(item, i, j, k);
                }
            }
        }

        if (isList)
        {
            var genType = typeof(List<>).MakeGenericType(arrayType);
            var instance = Activator.CreateInstance(genType);
            if (instance == null)
                return default;

            var list = (IList)instance;
            for (var i = 0; i < resultArray1D.Length; i++)
                list.Add(resultArray1D.GetValue(i));

            return list;
        }

        var dimensions = type.Name.Split(",").Length;
        if (dimensions == 3) return resultArray3D;
        else if (dimensions == 2) return resultArray2D;
        else if (dimensions == 1) return resultArray1D;

        return default;
    }

    private string TextFromPrimitiveOrTuple(object? value)
    {
        if (value == null)
            return string.Empty;

        if (IsPrimitiveTuple(value.GetType()))
            return TextFromTuple(value);

        var primitive = value is string str ?
            $"{Dividers.text}{str}{Dividers.text}" :
            $"{value}";

        return primitive;
    }
    private object? TextToPrimitiveOrTuple(string dataAsText, Type type)
    {
        return IsPrimitiveTuple(type) ?
            TextToTuple(dataAsText, type) :
            TextToPrimitive(dataAsText, type);
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
    private static bool IsArrayOfPrimitives(Array array)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        var elementType = array.GetType().GetElementType();

        if (elementType == null)
            throw new ArgumentException("Array must have an element type.");

        return elementType.IsPrimitive || elementType == typeof(string) || IsPrimitiveTuple(elementType);
    }
    private bool IsArray(string dataAsText)
    {
        return DividersCollection is { oneD: not null, twoD: not null } &&
               (dataAsText.Contains(DividersCollection.oneD) ||
                dataAsText.Contains(DividersCollection.twoD));
    }
    private static bool IsGenericList(Type type)
    {
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(List<>) &&
               (type.GenericTypeArguments[0].IsPrimitive ||
                IsPrimitiveTuple(type.GenericTypeArguments[0]));
    }
    private static bool IsPrimitiveTuple(Type type)
    {
        var name = type.Name;
        var isTuple = name.StartsWith(nameof(ValueTuple)) || name.StartsWith(nameof(Tuple));
        return isTuple && IsPrimitiveGenTypes(type);
    }
    private static bool IsPrimitiveGenTypes(Type type)
    {
        var gen = type.GenericTypeArguments;

        if (gen.Length == 0)
            return false;

        foreach (var t in gen)
            if (t.IsPrimitive == false && t != typeof(string))
                return false;

        return true;
    }
    private static bool IsPrimitiveOrTupleDictionary(Type type)
    {
        var isDict = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        var types = type.GenericTypeArguments;
        foreach (var t in types)
            if (IsPrimitiveTuple(t) == false && t.IsPrimitive == false && t != typeof(string))
                return false;

        return isDict;
    }
    private static bool IsNumber(Type t)
    {
        return t == typeof(sbyte) ||
               t == typeof(byte) ||
               t == typeof(short) ||
               t == typeof(ushort) ||
               t == typeof(int) ||
               t == typeof(uint) ||
               t == typeof(long) ||
               t == typeof(ulong) ||
               t == typeof(float) ||
               t == typeof(double) ||
               t == typeof(decimal);
    }

    private static List<object?> GetTupleItems(object tuple)
    {
        return GetTupleFields(tuple.GetType()).Select(f => f.GetValue(tuple)).ToList();
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

    private static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
            stream.Write(data, 0, data.Length);

        return output.ToArray();
    }
    private static byte[] Decompress(byte[] data)
    {
        var input = new MemoryStream(data);
        var output = new MemoryStream();
        using (var stream = new DeflateStream(input, CompressionMode.Decompress))
            stream.CopyTo(output);

        return output.ToArray();
    }
    private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}