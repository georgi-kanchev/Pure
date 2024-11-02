using System.Collections;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

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
    public static bool IsBase64(this string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length % 4 != 0)
            return false;

        return new Regex(@"^[a-zA-Z0-9\+/]*={0,2}$", RegexOptions.None).IsMatch(value);
    }
    public static byte[] ToBytes(this object? value)
    {
        if (value == null)
            return [255, 255, 255];

        var type = value.GetType();

        if (value is bool valueBool) return BitConverter.GetBytes(valueBool);
        else if (value is byte valueByte) return [valueByte];
        else if (value is sbyte valueSbyte) return [Convert.ToByte(valueSbyte)];
        else if (value is char valueChar) return BitConverter.GetBytes(valueChar);
        else if (value is decimal valueDec)
        {
            var bits = decimal.GetBits(valueDec);
            var bytes = new byte[16];
            for (var i = 0; i < bits.Length; i++)
                Array.Copy(BitConverter.GetBytes(bits[i]), 0, bytes, i * 4, 4);

            return bytes;
        }
        else if (value is double valueDb) return BitConverter.GetBytes(valueDb);
        else if (value is float valueFl) return BitConverter.GetBytes(valueFl);
        else if (value is int valueInt) return BitConverter.GetBytes(valueInt);
        else if (value is uint valueUint) return BitConverter.GetBytes(valueUint);
        else if (value is nint valueNint) return BitConverter.GetBytes(valueNint);
        else if (value is nuint valueNuint) return BitConverter.GetBytes(valueNuint);
        else if (value is long valueLong) return BitConverter.GetBytes(valueLong);
        else if (value is ulong valueUlong) return BitConverter.GetBytes(valueUlong);
        else if (value is short valueShort) return BitConverter.GetBytes(valueShort);
        else if (value is ushort valueUshort) return BitConverter.GetBytes(valueUshort);
        else if (value is string valueStr)
        {
            var data = IsBase64(valueStr) ?
                Convert.FromBase64String(valueStr) :
                Encoding.UTF8.GetBytes(valueStr);

            var l = data.Length; // 24-bit uint (a maximum of 16.78 MB string)
            var size = new[] { (byte)(l & 255), (byte)((l >> 8) & 255), (byte)((l >> 16) & 255) };
            return size.Concat(data).ToArray();
        }
        else if (IsTuple(type))
        {
            var result = new List<byte>();
            var tuple = GetTupleItems(value);
            foreach (var item in tuple)
                result.AddRange(ToBytes(item));

            var size = BitConverter.GetBytes(result.Count);
            return size.Concat(result).ToArray();
        }
        else if (IsArray(type))
        {
            var result = new List<byte>();
            var array = ToJaggedArray((Array)value);
            for (var i = 0; i < array.Length; i++)
                result.AddRange(ToBytes(array.GetValue(i)));

            var size = BitConverter.GetBytes(result.Count);
            return size.Concat(result).ToArray();
        }
        else if (IsList(type))
        {
            var result = new List<byte>();
            var list = (IList)value;
            foreach (var item in list)
                result.AddRange(ToBytes(item));

            var size = BitConverter.GetBytes(result.Count);
            return size.Concat(result).ToArray();
        }
        else if (IsDictionary(type))
        {
            var result = new List<byte>();
            var dict = (IDictionary)value;
            var isValueString = type.GetGenericArguments()[1] == typeof(string);
            foreach (var obj in dict)
            {
                var kvp = (DictionaryEntry)obj;
                result.AddRange(ToBytes(kvp.Key)); // key can't be null
                result.AddRange(ToBytes(isValueString && kvp.Value == null ? "" : kvp.Value));
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

            var i = (uint)0;
            foreach (var field in fields)
            {
                var doNotSave = field.IsDefined(typeof(DoNotSave), false);
                var order = field.GetCustomAttribute<SaveAtOrder>(false)?.Value ?? i;

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

            var size = BitConverter.GetBytes(result.Count);
            return size.Concat(result).ToArray();
        }

        return [];
    }
    public static T? ToObject<T>(this byte[] bytes)
    {
        return default;
    }
    public static string ToBase64(this byte[] data)
    {
        return Convert.ToBase64String(data);
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
    private static bool IsArray(Type? type)
    {
        return type is { IsArray: true };
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
#endregion
}