namespace Pure.Storage;

using System;
using System.Collections;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Text;

/// <summary>
/// Provides a simple key-value storage system that can serialize and deserialize objects 
/// to and from text or binary files. Supported types include:<br></br>
/// A. Primitives and Strings<br></br>
/// B. Tuples of A<br></br>
/// C. Arrays of A and B<br></br>
/// D. Lists of A and B<br></br>
/// E. Dictionaries of A and B<br></br>
/// </summary>
public class Storage
{
    /// <summary>
    /// Gets or sets the divider used for each storage element (key, typeId, value).
    /// </summary>
    public string? Divider
    {
        get => sep;
        set => sep = string.IsNullOrEmpty(value) ? " " : value;
    }
    /// <summary>
    /// Gets or sets the divider used for tuples.
    /// </summary>
    public string? DividerTuple
    {
        get => sepTuple;
        set => sepTuple = string.IsNullOrEmpty(value) ? ";" : value;
    }
    /// <summary>
    /// Gets or sets the divider used for 1D collections.
    /// </summary>
    public string? DividerCollection1D
    {
        get => sep1D;
        set => sep1D = string.IsNullOrEmpty(value) ? "|" : value;
    }
    // 3D divider is two 2D ones
    /// <summary>
    /// Gets or sets the divider used for 2D collections.
    /// </summary>
    public string? DividerCollection2D
    {
        get => sep2D;
        set => sep2D = string.IsNullOrEmpty(value) ? $"|{Environment.NewLine}" : value;
    }
    /// <summary>
    /// Gets or sets the divider used for dictionaries.
    /// </summary>
    public string? DividerDictionary
    {
        get => sepDict;
        set => sepDict = string.IsNullOrEmpty(value) ? "/" : value;
    }
    public string? DividerText
    {
        get => sepStr;
        set => sepStr = string.IsNullOrEmpty(value) ? "`" : value;
    }

    public Storage()
    {
        Divider = default;
        DividerTuple = default;
        DividerCollection1D = default;
        DividerCollection2D = default;
        DividerDictionary = default;
        DividerText = default;
    }

    /// <summary>
    /// Sets the specified object instance with the given key and type identifier.
    /// </summary>
    /// <param name="key">The key used to store the object.</param>
    /// <param name="instance">The object instance to be stored.</param>
    /// <param name="typeId">The type identifier of the object instance.</param>
    public void Set(string key, object? instance, int typeId = default)
    {
        data[key] = (typeId, TextFromObject(typeId, instance));
    }
    /// <summary>
    /// Removes the object with the specified key from storage.
    /// </summary>
    /// <param name="key">The key of the object to be removed.</param>
    public void Remove(string key)
    {
        data.Remove(key);
    }

    /// <summary>
    /// Gets the type ID of the object instance with the specified key.
    /// </summary>
    /// <param name="key">The key of the object instance.</param>
    /// <returns>The type ID of the object instance.</returns>
    public int GetTypeId(string key)
    {
        return data.ContainsKey(key) ? data[key].typeId : default;
    }
    /// <summary>
    /// Gets the object instance with the specified key as text.
    /// </summary>
    /// <param name="key">The key of the object instance.</param>
    /// <returns>The object instance as text.</returns>
    public string? GetAsText(string key)
    {
        return data.ContainsKey(key) ? data[key].data : default;
    }
    /// <summary>
    /// Gets the object instance with the specified key as an object of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object instance.</typeparam>
    /// <param name="key">The key of the object instance.</param>
    /// <returns>The object instance as an object of type <typeparamref name="T"/>.</returns>
    public T? GetAsObject<T>(string key)
    {
        if (data.ContainsKey(key) == false)
            return default;

        var obj = TextToObject<T>(data[key].typeId, data[key].data);
        return obj is "" or null ? default : (T)obj;
    }

    /// <summary>
    /// Saves the storage data to a file.
    /// </summary>
    /// <param name="path">The path of the file.</param>
    /// <param name="isBinary">A value indicating whether the data should be saved as binary.</param>
    public void ToFile(string path, bool isBinary = false)
    {
        if (isBinary)
        {
            File.WriteAllBytes(path, ToBytes());
            return;
        }

        File.WriteAllText(path, ToText());
    }
    /// <summary>
    /// Loads the storage data from a file.
    /// </summary>
    /// <param name="path">The path to the file to load.</param>
    /// <param name="isBinary">Whether the file is binary or text-based. Defaults to false.</param>
    public void FromFile(string path, bool isBinary = false)
    {
        if (isBinary)
        {
            FromBytes(File.ReadAllBytes(path));
            return;
        }

        FromText(File.ReadAllText(path));
    }

    public void FromText(string text)
    {
        strings = GetStrings(ref text);
        var split = text.Split(Divider);
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

            data[key] = (typeId, s.Trim());
            index = 0;
        }
    }
    public string ToText()
    {
        if (string.IsNullOrEmpty(Divider))
            return string.Empty;

        var result = new StringBuilder();
        foreach (var kvp in data)
        {
            result.Append(kvp.Key);
            result.Append(Divider);
            result.Append(kvp.Value.typeId);
            result.Append(Divider);
            result.Append(kvp.Value.data);
            result.Append(Divider + Environment.NewLine);
        }

        var length = Divider.Length + Environment.NewLine.Length;
        result.Remove(result.Length - length, length);

        return result.ToString();
    }

    public void FromBytes(byte[] bytes)
    {
        var text = Decompress(Convert.ToBase64String(Decompress(bytes)));
        FromText(text);
    }
    public byte[] ToBytes()
    {
        var base64 = Compress(ToText());
        var bytes = Compress(Convert.FromBase64String(base64));
        return bytes;
    }

    /// <summary>
    /// Called when an object of a given type is required to be turned into text data.
    /// Subclasses should override this method to implement their own behavior.
    /// </summary>
    /// <param name="typeId">The identifier of the object's type.</param>
    /// <param name="dataAsText">The object data as text.</param>
    /// <returns>The deserialized object.</returns>
    protected virtual object? OnObjectFromText(int typeId, string dataAsText)
    {
        return default;
    }
    /// <summary>
    /// Called when text data is required to be turned into an object of a given type.
    /// Subclasses should override this method to implement their own behavior.
    /// </summary>
    /// <param name="typeId">The identifier of the object's type.</param>
    /// <param name="instance">The object instance to serialize.</param>
    /// <returns>The serialized object data as text.</returns>
    protected virtual string OnObjectToText(int typeId, object? instance)
    {
        return string.Empty;
    }

#region Backend
    private const string STR_PLACEHOLDER = "—";
    private string? sep1D, sep2D, sepTuple, sepDict, sep, sepStr;
    private readonly Dictionary<string, (int typeId, string data)> data = new();
    private List<string>? strings;

    private string TextFromObject(int typeId, object? instance)
    {
        if (instance == null)
            return string.Empty;

        var t = instance.GetType();

        if (typeId != default)
            return OnObjectToText(typeId, instance); // ask user for parse

        if (t.IsPrimitive || t == typeof(string))
            return TextFromPrimitiveOrTuple(instance);

        if (IsGenericList(t) || (t.IsArray && IsArrayOfPrimitives((Array)instance)))
            return TextFromArrayOrList(instance);

        if (IsPrimitiveOrTupleDictionary(t))
            return TextFromDictionary((IDictionary)instance);

        if (IsPrimitiveTuple(t))
            return TextFromTuple(instance);

        return string.Empty;
    }
    private object? TextToObject<T>(int typeId, string dataAsText)
    {
        if (typeId != default)
            return OnObjectFromText(typeId, dataAsText); // ask user for parse

        var t = typeof(T);
        if ((t.IsArray || IsGenericList(t)) && IsArray(dataAsText))
            return TextToArrayOrList(dataAsText, t);

        if (t.IsPrimitive || t == typeof(string))
            return TextToPrimitive(dataAsText, t);

        if (IsPrimitiveOrTupleDictionary(t))
            return TextToDictionary(dataAsText, t);

        if (IsPrimitiveTuple(t))
            return TextToTuple(dataAsText, t);

        return default;
    }

    private string TextFromDictionary(IDictionary dict)
    {
        if (string.IsNullOrEmpty(DividerDictionary))
            return string.Empty;

        var result = new StringBuilder();
        foreach (var obj in dict)
        {
            var kvp = (DictionaryEntry)obj;
            var key = TextFromPrimitiveOrTuple(kvp.Key);
            var value = TextFromPrimitiveOrTuple(kvp.Value);

            result.Append(DividerDictionary);
            result.Append(key);
            result.Append(DividerDictionary);
            result.Append(value);
        }

        result.Remove(0, DividerDictionary.Length);
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

        var kvpStrings = dataAsText.Split(DividerDictionary);
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
            result += (i != 0 ? DividerTuple : string.Empty) + $"{items[i]}";

        return result;
    }
    private object? TextToTuple(string dataAsText, Type type)
    {
        var instance = Activator.CreateInstance(type);
        var items = dataAsText.Split(DividerTuple);
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
        if (string.IsNullOrEmpty(DividerCollection1D) ||
            string.IsNullOrEmpty(DividerCollection2D))
            return string.Empty;

        var result = new StringBuilder();
        var isArray = collection is Array;

        if (isArray == false) // is list
        {
            var list = (IList)collection;
            for (var i = 0; i < list.Count; i++)
            {
                var sep = i == 0 ? string.Empty : DividerCollection1D;
                result.Append(sep + TextFromPrimitiveOrTuple(list[i]));
            }

            return result.ToString();
        }

        var array = (Array)collection;
        if (array.Rank == 1)
            for (var i = 0; i < array.GetLength(0); i++)
            {
                var sep = i == 0 ? string.Empty : DividerCollection1D;
                result.Append(sep + TextFromPrimitiveOrTuple(array.GetValue(i)));
            }
        else if (array.Rank == 2)
        {
            for (var i = 0; i < array.GetLength(0); i++)
            {
                result.Append(DividerCollection2D);

                for (var j = 0; j < array.GetLength(1); j++)
                {
                    var sep = j == 0 ? string.Empty : DividerCollection1D;
                    result.Append(sep + TextFromPrimitiveOrTuple(array.GetValue(i, j)));
                }
            }

            result.Remove(0, DividerCollection2D.Length);
        }
        else if (array.Rank == 3)
        {
            for (var i = 0; i < array.GetLength(0); i++)
            {
                result.Append(DividerCollection2D);

                for (var j = 0; j < array.GetLength(1); j++)
                {
                    result.Append(DividerCollection2D);

                    for (var k = 0; k < array.GetLength(2); k++)
                    {
                        var sep = k == 0 ? string.Empty : DividerCollection1D;
                        result.Append(sep + TextFromPrimitiveOrTuple(array.GetValue(i, j, k)));
                    }
                }
            }

            result.Remove(0, DividerCollection2D.Length * 2);
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

        if (strings == null || dataAsText.StartsWith(STR_PLACEHOLDER) == false)
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
        var sep3D = DividerCollection2D + DividerCollection2D;
        var array3D = dataAsText.Split(sep3D);

        foreach (var str3D in array3D)
        {
            var array2D = str3D.Split(DividerCollection2D);
            resultArrays2D.Add(array2D);
            foreach (var str2D in array2D)
            {
                var array1D = str2D.Split(DividerCollection1D);
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
            var array2D = array3D[i].Split(DividerCollection2D);
            for (var j = 0; j < array2D.Length; j++)
            {
                var array1D = array2D[j].Split(DividerCollection1D);
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

        var primitive = value is string str ? $"{DividerText}{str}{DividerText}" : $"{value}";
        return primitive;
    }
    private object? TextToPrimitiveOrTuple(string dataAsText, Type type)
    {
        return IsPrimitiveTuple(type) ? TextToTuple(dataAsText, type) : TextToPrimitive(dataAsText, type);
    }

    private static T Wrap<T>(decimal value, T minValue, T maxValue)
        where T : struct, IComparable, IConvertible
    {
        if (typeof(T).IsPrimitive == false || typeof(T) == typeof(bool))
        {
            throw new ArgumentException("Type must be a primitive numeric type.");
        }

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
        return DividerCollection1D != null && DividerCollection2D != null &&
               (dataAsText.Contains(DividerCollection1D) ||
                dataAsText.Contains(DividerCollection2D));
    }
    private static bool IsGenericList(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) &&
               (type.GenericTypeArguments[0].IsPrimitive ||
                IsPrimitiveTuple(type.GenericTypeArguments[0]));
    }
    private static bool IsPrimitiveTuple(Type type)
    {
        return IsPrimitiveGenTypes(type);
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
        //var isDict = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        var types = type.GenericTypeArguments;
        foreach (var t in types)
            if (IsPrimitiveTuple(t) == false && t.IsPrimitive == false && t != typeof(string))
                return false;

        return true;
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

    private static string Compress(string text)
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
    private static string Decompress(string compressedText)
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

    private List<string> GetStrings(ref string input)
    {
        var result = new List<string>();

        if (DividerText == null)
            return result;

        var count = 0;

        while (input.Contains(DividerText))
        {
            var openIndex = input.IndexOf(DividerText, 0, StringComparison.Ordinal);
            if (openIndex == -1)
                break;

            var closeIndex = input.IndexOf(DividerText, openIndex + 1, StringComparison.Ordinal);

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