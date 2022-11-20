using System.Globalization;
using System.Reflection;

namespace Purity.Tools
{
	public class Storage
	{
		public void Save(string filePath, bool isDataFormatted = true)
		{
			var dir = Path.GetDirectoryName(filePath);
			if(dir != "" && Directory.Exists(dir) == false)
				return;

			var result = isDataFormatted ? FILE_HEADER + Environment.NewLine : "";
			var newLine = isDataFormatted ? Environment.NewLine : "";
			var space = isDataFormatted ? " " : "";
			var tab = isDataFormatted ? new string(' ', 4) : "";

			foreach(var kvp in data)
			{
				result += INSTANCE + space + kvp.Key;

				foreach(var kvp2 in kvp.Value)
				{
					var value = kvp2.Value;
					var type = value.GetType();

					result += newLine + tab + INSTANCE_PROPERTY + space + kvp2.Key;

					if(type.IsArray)
					{
						var array = (Array)value;
						var elementType = type.GetElementType();
						var isStruct = elementType != null && IsStructType(elementType);

						for(int i = 0; i < array.Length; i++)
						{
							var curValue = array.GetValue(i);

							result += newLine + tab + tab + (isStruct ? "" : VALUE);
							if(curValue != null)
								AddToString(ref result, curValue, false, true,
									isFormatted: isDataFormatted);
						}
						continue;
					}
					result += newLine + tab + tab;

					AddToString(ref result, value, isFormatted: isDataFormatted);
				}
			}

			File.WriteAllText(filePath, result);
		}
		public void Load(string filePath)
		{
			if(File.Exists(filePath) == false)
				return;

			var file = File.ReadAllText(filePath);

			if(file.Length == 0)
				return;

			var split = Trim(file).Split(INSTANCE, StringSplitOptions.RemoveEmptyEntries);

			data.Clear();
			for(int i = 0; i < split?.Length; i++)
			{
				var props = split[i].Split(INSTANCE_PROPERTY, StringSplitOptions.RemoveEmptyEntries);
				var instanceName = props[0];

				for(int j = 1; j < props?.Length; j++)
					CacheProp(instanceName, props[j]);
			}
		}

		public void Populate(string storageKey, object instance)
		{
			if(instance == null || data.ContainsKey(storageKey) == false)
				return;

			var type = instance.GetType();
			var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			for(int i = 0; i < props.Length; i++)
				PopulateMember(props[i], storageKey, instance);
			for(int i = 0; i < fields.Length; i++)
				if(fields[i].Name.Contains('<') == false)
					PopulateMember(fields[i], storageKey, instance);
		}
		public void Store(string storageKey, object instance)
		{
			if(instance == null)
				return;

			var type = instance.GetType();
			var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			if(data.ContainsKey(storageKey) == false)
				data[storageKey] = new();

			for(int i = 0; i < props.Length; i++)
			{
				var prop = props[i];

				if(IsTypeSupported(prop.PropertyType) == false)
					continue;

				var value = prop.GetValue(instance);

				if(value != null)
					data[storageKey][prop.Name] = value;
			}
			for(int i = 0; i < fields.Length; i++)
			{
				var field = fields[i];
				if(IsTypeSupported(field.FieldType) == false || field.Name.Contains('<'))
					continue;

				var value = field.GetValue(instance);

				if(value != null)
					data[storageKey][field.Name] = value;
			}

			return;
		}

		#region Backend
		private const string INSTANCE = "~@", INSTANCE_PROPERTY = "~~", VALUE = "~|",
			STRUCT = "~&", STRUCT_PROPERTY = "~=",
			SPACE = "~_", TAB = "~__", NEW_LINE = "~/";
		private const string FILE_HEADER = @"Purity - Storage file
--------------------------
| Map of symbols
|
|	~ Global separator
|
|	@ Object
|	~ Property
|	& Property containing sub properties
|	- Sub property
|	| Value
|
|	/ String new line
|	_ String space
|	__ String tab
--------------------------
";

		private readonly Dictionary<string, Dictionary<string, object>> data = new();

		private void AddToString(ref string result, object value, bool isInStruct = false, bool isInArray = false,
			bool isFormatted = true)
		{
			var type = value.GetType();

			var space = isFormatted ? " " : "";
			var newLine = isFormatted ? Environment.NewLine : "";
			var tab = isFormatted ? new string(' ', 4) : "";
			var valueSep = isInArray ? "" : VALUE;

			if(IsStructType(type) && isInStruct == false)
			{
				var props = type
					.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				var fields = type
					.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

				result += STRUCT;

				for(int i = 0; i < props.Length; i++)
				{
					var prop = props[i];

					if(IsTypeSupported(prop.PropertyType) == false)
						continue;

					var propValue = prop.GetValue(value);

					result += newLine + tab + tab + STRUCT_PROPERTY + space + newLine;

					if(propValue != null)
					{
						result += prop.Name + space + newLine + tab + tab + tab;
						AddToString(ref result, propValue, true, false, isFormatted: isFormatted);
					}
				}
				for(int i = 0; i < fields.Length; i++)
				{
					var field = fields[i];
					if(IsTypeSupported(field.FieldType) == false || field.Name.Contains('<'))
						continue;

					var fieldValue = field.GetValue(value);

					result += newLine + tab + tab + STRUCT_PROPERTY + space;

					if(fieldValue != null)
					{
						result += field.Name + space + newLine + tab + tab + tab;
						AddToString(ref result, fieldValue, true, false, isFormatted: isFormatted);
					}
				}

				return;
			}

			result += valueSep + space + value?.ToString()?
				.Replace("\t", TAB)
				.Replace(Environment.NewLine, NEW_LINE)
				.Replace(" ", SPACE);
		}
		private void PopulateMember(MemberInfo memberInfo, string instanceName, object instance)
		{
			var prop = memberInfo is PropertyInfo p ? p : null;
			var field = memberInfo is FieldInfo f ? f : null;
			var type = prop?.PropertyType ?? field?.FieldType;
			var name = memberInfo.Name;

			if(data[instanceName].ContainsKey(name) == false || type == null)
				return;

			var value = data[instanceName][name];
			var valueType = value.GetType();

			if(value is string str && str.Contains(STRUCT_PROPERTY))
			{
				if(str.Contains(STRUCT))
				{
					var elementType = type.GetElementType();
					if(elementType == null)
						return;

					var structs = str.Split(STRUCT, StringSplitOptions.RemoveEmptyEntries);
					var result = Array.CreateInstance(elementType, structs.Length);

					for(int i = 0; i < structs.Length; i++)
					{
						var currStruct = ParseStruct(elementType, structs[i]);
						result.SetValue(currStruct, i);
					}

					prop?.SetValue(instance, result);
					field?.SetValue(instance, result);
					return;
				}

				var structInstance = ParseStruct(type, str);
				prop?.SetValue(instance, structInstance);
				field?.SetValue(instance, structInstance);
				return;
			}

			if(valueType.IsArray)
			{
				var array = (object[])value;
				var elementType = type.GetElementType();
				if(elementType == null)
					return;

				var result = Array.CreateInstance(elementType, array.Length);

				for(int j = 0; j < array.Length; j++)
				{
					var arrayValue = TryParseValue(elementType, array[j]);
					result.SetValue(arrayValue, j);
				}

				prop?.SetValue(instance, result);
				field?.SetValue(instance, result);
				return;
			}

			var v = TryParseValue(type, value);
			prop?.SetValue(instance, v);
			field?.SetValue(instance, v);
		}
		private static object? ParseStruct(Type structType, string str)
		{
			var structInstance = Activator.CreateInstance(structType);
			var structProps = str.Split(STRUCT_PROPERTY, StringSplitOptions.RemoveEmptyEntries);

			for(int i = 0; i < structProps.Length; i++)
			{
				var structValues = structProps[i].Split(VALUE, StringSplitOptions.RemoveEmptyEntries);
				if(structValues.Length < 2)
					continue;
				var valStr = structValues[1];
				var structField = structType.GetField(structValues[0]);
				var structProp = structType.GetProperty(structValues[0]);

				if(structField != null)
				{
					var fieldValue = TryParseValue(structField.FieldType, valStr);
					structField?.SetValue(structInstance, fieldValue);
				}
				else if(structProp != null)
				{
					var propValue = TryParseValue(structProp.PropertyType, valStr);
					structProp?.SetValue(structInstance, propValue);
				}
			}
			return structInstance;
		}
		private static object? TryParseValue(Type type, object value)
		{
			if(type == typeof(string))
				return value;
			else if(type == typeof(bool))
				return Convert.ChangeType(Convert.ToBoolean(value, CultureInfo.CurrentCulture), type);
			else if(type == typeof(char))
				return Convert.ChangeType(Convert.ToChar(value, CultureInfo.CurrentCulture), type);
			else if(IsNumericType(type))
				return Convert.ChangeType(Convert.ToSingle(value, CultureInfo.CurrentCulture), type);

			return default;
		}
		private void CacheProp(string instanceName, string prop)
		{
			if(prop.Contains(STRUCT))
			{
				var struc = prop.Split(STRUCT, StringSplitOptions.RemoveEmptyEntries);
				var structPropName = struc[0];

				var val = "";
				for(int i = 1; i < struc.Length; i++)
				{
					var sep = i < struc.Length - 1 ? STRUCT : "";
					val += struc[i]
						.Replace(TAB, "\t")
						.Replace(NEW_LINE, Environment.NewLine)
						.Replace(SPACE, " ") +
						sep;
				}

				data[instanceName][structPropName] = val;
				return;
			}

			var values = prop.Split(VALUE, StringSplitOptions.RemoveEmptyEntries);
			if(values.Length < 2)
				return;

			var name = values[0];
			var finalValues = new string[values.Length - 1];
			for(int i = 1; i < values.Length; i++)
				finalValues[i - 1] = values[i]
					.Replace(TAB, "\t")
					.Replace(NEW_LINE, Environment.NewLine)
					.Replace(SPACE, " ");

			if(data.ContainsKey(instanceName) == false)
				data[instanceName] = new();

			object finalValue = finalValues.Length == 1 ? finalValues[0] : finalValues;
			if(finalValue != null)
				data[instanceName][name] = finalValue;
		}
		private static bool IsTypeSupported(Type type)
		{
			return type.IsClass == false || type.IsArray || type == typeof(string);
		}
		private static string Trim(string text)
		{
			return text.Replace("\t", "").Replace(" ", "").Replace(Environment.NewLine, "");
		}
		private static bool IsNumericType(Type type)
		{
			return Type.GetTypeCode(type) switch
			{
				TypeCode.Byte or
				TypeCode.SByte or
				TypeCode.UInt16 or
				TypeCode.UInt32 or
				TypeCode.UInt64 or
				TypeCode.Int16 or
				TypeCode.Int32 or
				TypeCode.Int64 or
				TypeCode.Decimal or
				TypeCode.Double or
				TypeCode.Single => true,
				_ => false,
			};
		}
		private static bool IsStructType(Type type)
		{
			return type.IsValueType && type.IsEnum == false && type.IsPrimitive == false &&
				type.IsEquivalentTo(typeof(decimal)) == false;
		}
		#endregion
	}
}
