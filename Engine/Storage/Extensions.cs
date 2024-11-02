using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Xsl;

namespace Pure.Engine.Storage;

public static class Extensions
{
    public static string Compress(this string value)
    {
        return Convert.ToBase64String(Compress(Encoding.UTF8.GetBytes(value)));
    }
    public static string Decompress(this string compressedText)
    {
        return Encoding.UTF8.GetString(Decompress(Convert.FromBase64String(compressedText)));
    }
    public static byte[] Compress(this byte[] data)
    {
        var compressed = new List<byte>();
        for (var i = 0; i < data.Length; i++)
        {
            var count = (byte)1;
            while (i + 1 < data.Length && data[i] == data[i + 1] && count < 255)
            {
                count++;
                i++;
            }

            compressed.Add(count);
            compressed.Add(data[i]);
        }

        var frequencies = compressed.GroupBy(b => b).ToDictionary(g => g.Key, g => g.Count());
        var root = GetTree(frequencies);
        var codeTable = GetTable(root);
        var header = new List<byte> { (byte)frequencies.Count };
        foreach (var kvp in frequencies)
        {
            header.Add(kvp.Key);
            header.AddRange(BitConverter.GetBytes(kvp.Value));
        }

        var bitString = string.Join("", compressed.Select(b => codeTable[b]));
        var byteList = new List<byte>(header);

        for (var i = 0; i < bitString.Length; i += 8)
        {
            var byteStr = bitString.Substring(i, Math.Min(8, bitString.Length - i));
            byteList.Add(Convert.ToByte(byteStr, 2));
        }

        return byteList.ToArray();
    }
    public static byte[] Decompress(this byte[] compressedData)
    {
        var index = 0;
        var tableSize = (int)compressedData[index++];

        var frequencies = new Dictionary<byte, int>();
        for (var i = 0; i < tableSize; i++)
        {
            var key = compressedData[index++];
            var frequency = BitConverter.ToInt32(compressedData, index);
            index += 4;
            frequencies[key] = frequency;
        }

        var root = GetTree(frequencies);
        var decompressed = new List<byte>();

        var node = root;
        for (var i = index; i < compressedData.Length; i++)
        {
            var bits = Convert.ToString(compressedData[i], 2).PadLeft(8, '0');
            foreach (var bit in bits)
            {
                node = bit == '0' ? node.left : node.right;
                if (node!.IsLeaf == false)
                    continue;

                decompressed.Add(node.value);
                node = root;
            }
        }

        var result = new List<byte>();
        for (var i = 0; i < decompressed.Count; i += 2)
            for (var j = 0; j < decompressed[i]; j++)
                result.Add(decompressed[i + 1]);

        return result.ToArray();
    }

    public static byte[] ToBytes(this object? value)
    {
        // first 4 bytes indicates how many bytes the data is (max 2.15 GB)
        // then the amount of bytes of actual data

        if (value == null)
            return [0, 0, 0, 0]; // size of 0 bytes indicates null

        var type = value.GetType();

        if (value is bool valueBool)
            return [1, 0, 0, 0, (byte)(valueBool ? 1 : 0)];
        else if (value is byte valueByte)
            return [1, 0, 0, 0, valueByte];
        else if (value is sbyte valueSbyte)
            return [1, 0, 0, 0, Convert.ToByte(valueSbyte)];
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
        else if (IsTuple(type))
        {
            var result = new List<byte>();
            var tuple = GetTupleItems(value);
            foreach (var item in tuple)
                result.AddRange(ToBytes(item));

            return BitConverter.GetBytes(result.Count).Concat(result).ToArray();
        }
        else if (type.IsArray)
        {
            var result = new List<byte>();
            var array = ToJaggedArray((Array)value);
            for (var i = 0; i < array.Length; i++)
                result.AddRange(ToBytes(array.GetValue(i)));

            return BitConverter.GetBytes(result.Count).Concat(result).ToArray();
        }
        else if (IsList(type))
        {
            var result = new List<byte>();
            var list = (IList)value;
            foreach (var item in list)
                result.AddRange(ToBytes(item));

            return BitConverter.GetBytes(result.Count).Concat(result).ToArray();
        }
        else if (IsDictionary(type))
        {
            var result = new List<byte>();
            var dict = (IDictionary)value;
            foreach (var obj in dict)
            {
                var kvp = (DictionaryEntry)obj;
                result.AddRange(ToBytes(kvp.Key));
                result.AddRange(ToBytes(kvp.Value));
            }

            var size = BitConverter.GetBytes(result.Count);
            return size.Concat(result).ToArray();
        }
        else if (IsStruct(type) || type.IsClass)
        {
            var sorted = new SortedDictionary<uint, List<byte>>();
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Concat(type.GetFields(BindingFlags.Public | BindingFlags.Instance)).ToArray();
            var props = type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                .Concat(type.GetProperties(BindingFlags.Public | BindingFlags.Instance)).ToArray();

            var i = (uint)1;
            foreach (var field in fields)
            {
                var doNotSave = field.IsDefined(typeof(DoNotSave), false);
                var order = field.GetCustomAttribute<SaveAtOrder>(false)?.Value ?? i;

                // some fields are auto generated by a property, so extract the attributes
                // from said property (searching by name) and use them for the field
                if (field.Name.Contains(">k__BackingField"))
                {
                    var name = field.Name.Replace("<", "").Replace(">k__BackingField", "");
                    foreach (var prop in props)
                        if (prop.Name == name)
                        {
                            doNotSave = prop.IsDefined(typeof(DoNotSave), false);
                            order = prop.GetCustomAttribute<SaveAtOrder>(false)?.Value ?? i;
                        }
                }

                if (doNotSave)
                    continue;

                sorted.TryAdd(order, []);
                sorted[order].AddRange(ToBytes(field.GetValue(value)));
                i++;
            }

            var result = new List<byte>();
            foreach (var kvp in sorted)
                result.AddRange(kvp.Value);

            return BitConverter.GetBytes(result.Count).Concat(result).ToArray();
        }

        return [];
    }
    public static T? ToObject<T>(this byte[]? data)
    {
        if (data == null || data.Length == 0)
            return default;

        return (T?)ToObject(data, typeof(T), out _);
    }

    public static string? ToBase64(this string? value)
    {
        return string.IsNullOrEmpty(value) ? null : ToBase64(Encoding.UTF8.GetBytes(value));
    }
    public static string? ToBase64(this byte[]? data)
    {
        return data == null || data.Length == 0 ? null : Convert.ToBase64String(data);
    }
    public static string? ToText(this string? base64)
    {
        if (string.IsNullOrEmpty(base64))
            return null;

        try
        {
            var data = Convert.FromBase64String(base64);
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

#region Backend
    private class Node
    {
        public byte value;
        public int freq;
        public Node? left;
        public Node? right;
        public bool IsLeaf
        {
            get => left == null && right == null;
        }
    }

    private static object? ToObject(this byte[]? data, Type? expectedType, out byte[]? remaining)
    {
        remaining = null;
        if (data == null || data.Length < 4)
            return default;

        var size = BitConverter.ToInt32(data, 0);

        if (size == 0 || expectedType == null)
            return default;

        var bytes = data.AsSpan()[4..(4 + size)].ToArray();
        remaining = data.AsSpan()[(4 + bytes.Length)..].ToArray();

        if ((expectedType == typeof(bool) || expectedType == typeof(bool?)) && size == 1)
            return BitConverter.ToBoolean(bytes);
        else if ((expectedType == typeof(byte) || expectedType == typeof(byte?)) && size == 1)
            return bytes[0];
        else if ((expectedType == typeof(sbyte) || expectedType == typeof(sbyte?)) && size == 1)
            return bytes[0];
        else if ((expectedType == typeof(char) || expectedType == typeof(char?)) && size == 2)
            return BitConverter.ToChar(bytes);
        else if ((expectedType == typeof(short) || expectedType == typeof(short?)) && size == 2)
            return BitConverter.ToInt16(bytes);
        else if ((expectedType == typeof(ushort) || expectedType == typeof(ushort?)) && size == 2)
            return BitConverter.ToUInt16(bytes);
        else if ((expectedType == typeof(int) || expectedType == typeof(int?)) && size == 4)
            return BitConverter.ToInt32(bytes);
        else if ((expectedType == typeof(uint) || expectedType == typeof(uint?)) && size == 4)
            return BitConverter.ToUInt32(bytes);
        else if ((expectedType == typeof(float) || expectedType == typeof(float?)) && size == 4)
            return BitConverter.ToSingle(bytes);
        else if ((expectedType == typeof(long) || expectedType == typeof(long?)) && size == 8)
            return BitConverter.ToInt64(bytes);
        else if ((expectedType == typeof(ulong) || expectedType == typeof(ulong?)) && size == 8)
            return BitConverter.ToUInt64(bytes);
        else if ((expectedType == typeof(double) || expectedType == typeof(double?)) && size == 8)
            return BitConverter.ToDouble(bytes);
        else if ((expectedType == typeof(decimal) || expectedType == typeof(decimal?)) && size == 16)
        {
            var lo = BitConverter.ToInt32(bytes, 0);
            var mid = BitConverter.ToInt32(bytes, 4);
            var hi = BitConverter.ToInt32(bytes, 8);
            var flags = BitConverter.ToInt32(bytes, 12);
            return new decimal(lo, mid, hi, (flags & 0x80000000) != 0, (byte)((flags >> 16) & 255));
        }
        else if (expectedType == typeof(string))
            return Encoding.UTF8.GetString(bytes);
        else if (IsTuple(expectedType))
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
        else if (expectedType.IsArray)
        {
            var itemType = expectedType.GetElementType();
            var ranks = expectedType.GetArrayRank();
            if (itemType == null || ranks > 3)
                return default;

            if (ranks == 1)
            {
                var result1D = new List<object?>();

                while (bytes is { Length: > 0 })
                {
                    var obj = ToObject(bytes, itemType, out var left);
                    bytes = left;
                    result1D.Add(obj);
                }

                var instance = Array.CreateInstance(itemType, result1D.Count);
                for (var i = 0; i < result1D.Count; i++)
                    instance.SetValue(result1D[i], i);

                return instance;
            }

            var result = new List<object?>();
            var arrayType = Array.CreateInstance(itemType, new int[ranks - 1]);

            while (bytes is { Length: > 0 })
            {
                var obj = ToObject(bytes, arrayType.GetType(), out var left);
                bytes = left;
                result.Add(obj);
            }

            return ConvertToMultiDimensionalArray(result);
        }

        return default;
    }

    private static Dictionary<byte, string> GetTable(Node root)
    {
        var codeTable = new Dictionary<byte, string>();
        BuildCode(root, "", codeTable);
        return codeTable;

        void BuildCode(Node node, string code, Dictionary<byte, string> table)
        {
            if (node.IsLeaf)
                table[node.value] = code;

            if (node.left != null) BuildCode(node.left, code + "0", table);
            if (node.right != null) BuildCode(node.right, code + "1", table);
        }
    }
    private static Node GetTree(Dictionary<byte, int> frequencies)
    {
        var nodes = new List<Node>(frequencies.Select(f => new Node { value = f.Key, freq = f.Value }));
        while (nodes.Count > 1)
        {
            nodes = nodes.OrderBy(n => n.freq).ToList();
            var left = nodes[0];
            var right = nodes[1];
            var parent = new Node { left = left, right = right, freq = left.freq + right.freq };
            nodes.RemoveRange(0, 2);
            nodes.Add(parent);
        }

        return nodes[0];
    }

    private static bool IsPrimitive(Type? type)
    {
        return type is { IsPrimitive: true } || type == typeof(string);
    }
    private static bool IsTuple(Type? type)
    {
        if (type == null)
            return false;

        return type.Name.StartsWith(nameof(ValueTuple)) || type.Name.StartsWith(nameof(Tuple));
    }
    private static bool IsList(Type? type)
    {
        return type is { IsGenericType: true } && type.GetGenericTypeDefinition() == typeof(List<>);
    }
    private static bool IsDictionary(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }
    private static bool IsStruct(Type type)
    {
        return type is { IsValueType: true, IsEnum: false, IsPrimitive: false };
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
    private static Array ToJaggedArray(Array array)
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
                var jaggedArray = new object[length];
                for (var i = 0; i < length; i++)
                {
                    indices[currentDimension] = i;
                    jaggedArray[i] = ConvertToJagged(arr, currentDimension + 1, indices);
                }

                return jaggedArray;
            }
        }
    }

    public static Array ConvertToMultiDimensionalArray(IList jaggedList)
    {
        var dimensions = GetDimensions(jaggedList);
        var elementType = jaggedList.Cast<object>().SelectMany(GetInnermostElements).First().GetType();
        var multiDimArray = Array.CreateInstance(elementType, dimensions.ToArray());

        Populate(multiDimArray, jaggedList, []);

        return multiDimArray;

        List<int> GetDimensions(IList list)
        {
            var dims = new List<int>();
            while (list is { } currentList)
            {
                dims.Add(currentList.Count);
                list = currentList.Cast<object>().FirstOrDefault(item => item is IList) as IList;
                if (list == null)
                    break;
            }

            return dims;
        }
        IEnumerable<object> GetInnermostElements(object item)
        {
            if (item is IList list)
                foreach (var subItem in list)
                {
                    foreach (var innermost in GetInnermostElements(subItem))
                        yield return innermost;
                }
            else
                yield return item;
        }
        void Populate(Array multiDimArray2, IList sourceList, int[] indices)
        {
            for (var i = 0; i < sourceList.Count; i++)
            {
                var currentIndices = indices.Append(i).ToArray();

                if (sourceList[i] is IList nestedList)
                    Populate(multiDimArray2, nestedList, currentIndices);
                else
                    multiDimArray2.SetValue(sourceList[i], currentIndices);
            }
        }
    }
#endregion
}