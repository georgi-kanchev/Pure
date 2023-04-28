namespace Pure.Storage;

using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text;

public class Storage
{
	public string? SeparatorCollection1D
	{
		get => sep1D;
		set
		{
			if (string.IsNullOrEmpty(value))
				value = "¸";

			sep1D = value;
		}
	}
	// 3D separator is two 2D ones
	public string? SeparatorCollection2D
	{
		get => sep2D;
		set
		{
			if (string.IsNullOrEmpty(value))
				value = $"·{Environment.NewLine}";

			sep2D = value;
		}
	}
	public string? SeparatorTuple
	{
		get => sepTuple;
		set
		{
			if (string.IsNullOrEmpty(value))
				value = "‚";

			sepTuple = value;
		}
	}

	public Storage()
	{
		SeparatorCollection1D = default;
		SeparatorCollection2D = default;
		SeparatorTuple = default;
	}

	public void Set(string key, object? instance, int typeId = default)
	{
		if (key == null)
			return;

		data[key] = (typeId, ObjectToText(typeId, instance));
	}
	public void Remove(string key)
	{
		if (key != null)
			data.Remove(key);
	}

	public int GetTypeID(string key) => key != null && data.ContainsKey(key) ? data[key].typeId : default;
	public string? GetAsText(string key) => key != null && data.ContainsKey(key) ? data[key].data : default;
	public T? GetAsObject<T>(string key)
	{
		if (key == null || data.ContainsKey(key) == false)
			return default;

		var obj = TextToObject<T>(data[key].typeId, data[key].data);
		return obj == null ? default : (T)obj;
	}

	public void Save(string path)
	{

	}

	protected virtual object? OnTextToObject(int typeId, string dataAsText)
	{
		return default;
	}
	protected virtual string OnObjectToText(int typeId, object? instance)
	{
		return string.Empty;
	}

	#region Backend
	private static readonly HashSet<Type> ValueTupleTypes = new(new Type[]
	{
		typeof(ValueTuple<>),
		typeof(ValueTuple<,>),
		typeof(ValueTuple<,,>),
		typeof(ValueTuple<,,,>),
		typeof(ValueTuple<,,,,>),
		typeof(ValueTuple<,,,,,>),
		typeof(ValueTuple<,,,,,,>),
		typeof(ValueTuple<,,,,,,,>)
	});
	private string? sep1D, sep2D, sepTuple;
	private Dictionary<string, (int typeId, string data)> data = new();

	private object? TextToObject<T>(int typeId, string dataAsText)
	{
		if (typeId != default)
			return OnTextToObject(typeId, dataAsText); // ask user for parse

		var t = typeof(T);
		if ((t.IsArray || IsGenericList(t)) && IsArray(dataAsText))
			return TextToArrayOrList(dataAsText, t);

		if (IsPrimitiveTuple(t))
			return TextToTuple(dataAsText, t);

		if (t.IsPrimitive || IsPrimitiveGenTypes(t))
			return TextToPrimitive(dataAsText, t);

		return default;
	}
	private string ObjectToText(int typeId, object? instance)
	{
		if (instance == null)
			return string.Empty;

		var type = instance.GetType();

		if (typeId != default)
			return OnObjectToText(typeId, instance); // ask user for parse

		if (type.IsPrimitive || type == typeof(string))
			return instance.ToString() ?? string.Empty;

		if (type.IsArray && IsArrayOfPrimitives((Array)instance))
			return ArrayToText((Array)instance);

		if (IsPrimitiveTuple(type))
			return TupleToText(instance);

		return string.Empty;
	}

	private object? TextToTuple(string dataAsText, Type type)
	{
		var instance = Activator.CreateInstance(type);
		var items = dataAsText.Split(SeparatorTuple);
		var genTypes = type.GenericTypeArguments;
		var minLength = Math.Min(items.Length, genTypes.Length);
		for (int i = 0; i < minLength; i++)
		{
			var item = TextToPrimitive(items[i], genTypes[i]);

			var field = type.GetField($"Item{i + 1}");
			if (field == null)
				continue;

			field.SetValue(instance, item);
		}

		return instance;
	}
	private string TupleToText(object tuple)
	{
		var result = string.Empty;

		var items = GetTupleItems(tuple);
		for (int i = 0; i < items.Count; i++)
			result += (i != 0 ? SeparatorTuple : string.Empty) + $"{items[i]}";

		return result;
	}
	private object TextToPrimitive(string dataAsText, Type type)
	{
		var t = type;
		if (t == typeof(bool)) return Convert.ToBoolean(dataAsText);
		else if (t == typeof(char)) return Convert.ToChar(dataAsText);

		decimal.TryParse(dataAsText, NumberStyles.Any, CultureInfo.InvariantCulture, out var number);

		if (t == typeof(sbyte)) return Wrap<sbyte>(number, sbyte.MinValue, sbyte.MaxValue);
		else if (t == typeof(byte)) return Wrap<byte>(number, byte.MinValue, byte.MaxValue);
		else if (t == typeof(short)) return Wrap<short>(number, short.MinValue, short.MaxValue);
		else if (t == typeof(ushort)) return Wrap<ushort>(number, ushort.MinValue, ushort.MaxValue);
		else if (t == typeof(int)) return Wrap<int>(number, int.MinValue, int.MaxValue);
		else if (t == typeof(uint)) return Wrap<uint>(number, uint.MinValue, uint.MaxValue);
		else if (t == typeof(long)) return Wrap<long>(number, long.MinValue, long.MaxValue);
		else if (t == typeof(ulong)) return Wrap<ulong>(number, ulong.MinValue, ulong.MaxValue);
		else if (t == typeof(float)) return Wrap<float>(number, float.MinValue, float.MaxValue);
		else if (t == typeof(double)) return Wrap<double>(number, double.MinValue, double.MaxValue);
		else if (t == typeof(decimal)) return number;

		return dataAsText;
	}
	private string ArrayToText(Array array)
	{
		if (string.IsNullOrEmpty(SeparatorCollection1D) ||
			string.IsNullOrEmpty(SeparatorCollection2D))
			return string.Empty;

		var arrayStr = new StringBuilder();

		if (array.Rank == 1)
			for (int i = 0; i < array.GetLength(0); i++)
			{
				var sep = i == 0 ? string.Empty : SeparatorCollection1D;
				arrayStr.Append(sep + array.GetValue(i)?.ToString());
			}
		else if (array.Rank == 2)
		{
			for (int i = 0; i < array.GetLength(0); i++)
			{
				arrayStr.Append(SeparatorCollection2D);

				for (int j = 0; j < array.GetLength(1); j++)
				{
					var sep = j == 0 ? string.Empty : SeparatorCollection1D;
					arrayStr.Append(sep + array.GetValue(i, j)?.ToString());
				}
			}
			arrayStr.Remove(0, SeparatorCollection2D.Length);
		}
		else if (array.Rank == 3)
		{
			for (int i = 0; i < array.GetLength(0); i++)
			{
				arrayStr.Append(SeparatorCollection2D);

				for (int j = 0; j < array.GetLength(1); j++)
				{
					arrayStr.Append(SeparatorCollection2D);

					for (int k = 0; k < array.GetLength(2); k++)
					{
						var sep = k == 0 ? string.Empty : SeparatorCollection1D;
						arrayStr.Append(sep + array.GetValue(i, j, k)?.ToString());
					}
				}
			}
			arrayStr.Remove(0, SeparatorCollection2D.Length * 2);
		}
		return arrayStr.ToString();
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
		var sep3D = SeparatorCollection2D + SeparatorCollection2D;
		var array3D = dataAsText.Split(sep3D);

		for (int i = 0; i < array3D.Length; i++)
		{
			var array2D = array3D[i].Split(SeparatorCollection2D);
			resultArrays2D.Add(array2D);
			for (int j = 0; j < array2D.Length; j++)
			{
				var array1D = array2D[j].Split(SeparatorCollection1D);
				resultArrays1D.Add(array1D);
			}
		}
		var length3D = array3D.Length;
		var length2D = resultArrays2D[0].Length;
		var length1D = resultArrays1D[0].Length;
		var resultArray3D = Array.CreateInstance(arrayType, length3D, length2D, length1D);
		var resultArray2D = Array.CreateInstance(arrayType, length2D, length1D);
		var resultArray1D = Array.CreateInstance(arrayType, length1D);
		for (int i = 0; i < array3D.Length; i++)
		{
			var array2D = array3D[i].Split(SeparatorCollection2D);
			for (int j = 0; j < array2D.Length; j++)
			{
				var array1D = array2D[j].Split(SeparatorCollection1D);
				for (int k = 0; k < array1D.Length; k++)
				{
					var item = Convert.ChangeType(TextToPrimitive(array1D[k], arrayType), arrayType);
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
			for (int i = 0; i < resultArray1D.Length; i++)
				list.Add(resultArray1D.GetValue(i));

			return list;
		}

		var dimensions = type.Name.Split(",").Length;
		if (dimensions == 3) return resultArray3D;
		else if (dimensions == 2) return resultArray2D;
		else if (dimensions == 1) return resultArray1D;

		return default;
	}

	private static T Wrap<T>(decimal value, T minValue, T maxValue) where T : struct, IComparable, IConvertible
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

		return elementType.IsPrimitive || elementType == typeof(string);
	}
	private bool IsArray(string dataAsText)
	{
		return SeparatorCollection1D != null && SeparatorCollection2D != null &&
			(dataAsText.Contains(SeparatorCollection1D) ||
			dataAsText.Contains(SeparatorCollection2D));
	}
	private static bool IsGenericList(Type type)
	{
		return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) &&
			type.GenericTypeArguments[0].IsPrimitive;
	}
	private static bool IsPrimitiveTuple(Type type) => IsPrimitiveGenTypes(type);
	private static bool IsPrimitiveGenTypes(Type type)
	{
		var gen = type.GenericTypeArguments;
		for (int i = 0; i < gen.Length; i++)
			if (gen[i].IsPrimitive == false && gen[i] != typeof(string))
				return false;

		return true;
	}

	private static List<object?> GetTupleItems(object tuple)
	{
		return GetTupleFields(tuple.GetType()).Select(f => f.GetValue(tuple)).ToList();
	}
	private static List<Type> GetTupleTypes(Type tupleType)
	{
		return GetTupleFields(tupleType).Select(f => f.FieldType).ToList();
	}
	private static List<FieldInfo> GetTupleFields(Type tupleType)
	{
		var items = new List<FieldInfo>();

		FieldInfo? field;
		int nth = 1;
		while ((field = tupleType.GetRuntimeField($"Item{nth}")) != null)
		{
			nth++;
			items.Add(field);
		}

		return items;
	}
	#endregion
}