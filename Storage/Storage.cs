namespace Pure.Storage;

using System;
using System.Globalization;
using System.Text;

public class Storage<TKey> where TKey : notnull
{
	public void Set(TKey key, object? instance, int typeId = default)
	{
		data[key] = (typeId, ObjectToString(typeId, instance));
	}
	public bool Remove(TKey key) => data.Remove(key);

	public int GetTypeID(TKey key) => data.ContainsKey(key) ? data[key].typeId : default;
	public string? GetAsText(TKey key) => data.ContainsKey(key) ? data[key].data : default;
	public T? GetAsObject<T>(TKey key)
	{
		if (data.ContainsKey(key) == false)
			return default;

		var obj = StringToObject<T>(data[key].typeId, data[key].data);
		return obj == null ? default : (T)obj;
	}

	public void SaveToFile(string filename)
	{

	}

	public void LoadFromFile(string filename)
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
	private Dictionary<TKey, (int typeId, string data)> data = new();

	private object? StringToObject<T>(int typeId, string dataAsText)
	{
		if (typeId != default)
			return OnTextToObject(typeId, dataAsText);

		var t = typeof(T);
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
	private string ObjectToString(int typeId, object? instance)
	{
		if (instance == null)
			return string.Empty;

		var type = instance.GetType();

		if (typeId == default)
		{
			if (type.IsPrimitive || type == typeof(string))
				return instance.ToString() ?? string.Empty;
			else if (type.IsArray && IsArrayOfPrimitives((Array)instance))
			{
				var arrayStr = new StringBuilder();
				var array = (Array)instance;
				var sep1D = ", ";
				var sep2D = Environment.NewLine;

				if (array.Rank == 1)
					for (int i = 0; i < array.GetLength(0); i++)
					{
						var sep = i == 0 ? string.Empty : sep1D;
						arrayStr.Append(sep + array.GetValue(i)?.ToString());
					}
				else if (array.Rank == 2)
					for (int i = 0; i < array.GetLength(0); i++)
					{
						arrayStr.Append(sep2D);

						for (int j = 0; j < array.GetLength(1); j++)
						{
							var sep = j == 0 ? string.Empty : sep1D;
							arrayStr.Append(sep + array.GetValue(i, j)?.ToString());
						}
					}
				else if (array.Rank == 3)
					for (int i = 0; i < array.GetLength(0); i++)
					{
						arrayStr.Append(sep2D);

						for (int j = 0; j < array.GetLength(1); j++)
						{
							arrayStr.Append(sep2D);

							for (int k = 0; k < array.GetLength(2); k++)
							{
								var sep = k == 0 ? string.Empty : sep1D;
								arrayStr.Append(sep + array.GetValue(i, j, k)?.ToString());
							}
						}
					}
				return arrayStr.ToString().Trim();
			}

			return string.Empty;
		}

		return OnObjectToText(typeId, instance);
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
	#endregion
}