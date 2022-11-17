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

					result += newLine + tab + INSTANCE_PROPERTY + space +
						kvp2.Key.PadRight(isDataFormatted ? PAD_SPACES + 8 : 0);

					if(type.IsArray)
					{
						var array = (Array)value;
						var elementType = type.GetElementType();

						if(elementType != null && IsStructType(elementType) == false)
							result += space + VALUE;

						for(int i = 0; i < array.Length; i++)
						{
							var curValue = array.GetValue(i);

							result += newLine + tab + tab;
							if(curValue != null)
								AddToString(ref result, curValue, false, true, isFormatted: isDataFormatted);
						}
						continue;
					}

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

			return;
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
		private const int PAD_SPACES = 16;
		private const string NUMBER = "~#", BOOL = "~;", CHAR = "~'", STRING = "~\"",
			INSTANCE = "~@", INSTANCE_PROPERTY = "~~", VALUE = "~|",
			STRUCT = "~&", STRUCT_PROPERTY = "~=",
			SPACE = "~_", TAB = "~__", NEW_LINE = "~/";
		private const string FILE_HEADER = @"Purity - Storage file
--------------------------
| Map of symbols
|
|	~ Global separator
|
|	@ Instance
|	~ Instance property name
|
|	& Struct
|	- Struct property name
|
|	| Property value
|
|	; Bool
|	# Number
|	' Char
|	"" String
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
			var valueSep = isInArray ? "" : space + VALUE + space;

			if(IsStructType(type) && isInStruct == false)
			{
				var props = type
					.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				var fields = type
					.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

				if(isInArray == false)
					result += newLine + tab + tab;

				result += STRUCT;

				for(int i = 0; i < props.Length; i++)
				{
					var prop = props[i];

					if(IsTypeSupported(prop.PropertyType) == false)
						continue;

					var propValue = prop.GetValue(value);

					result += newLine + tab + tab + space + space + STRUCT_PROPERTY + space;

					if(propValue != null)
					{
						result += prop.Name.PadRight(isFormatted ? PAD_SPACES + 2 : 0);
						AddToString(ref result, propValue, true, false, isFormatted: isFormatted);
					}
				}
				for(int i = 0; i < fields.Length; i++)
				{
					var field = fields[i];
					if(IsTypeSupported(field.FieldType) == false || field.Name.Contains('<'))
						continue;

					var fieldValue = field.GetValue(value);

					result += newLine + tab + tab + space + space + STRUCT_PROPERTY + space;

					if(fieldValue != null)
					{
						result += field.Name.PadRight(isFormatted ? PAD_SPACES + 2 : 0);
						AddToString(ref result, fieldValue, true, false, isFormatted: isFormatted);
					}
				}

				return;
			}

			if(value is bool) result += valueSep + BOOL + space + value;
			else if(value is char) result += valueSep + CHAR + space + value;
			else if(IsNumericType(value)) result += valueSep + NUMBER + space + value;
			else if(value is string) result += valueSep + STRING + space + value?.ToString()?
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
					result.SetValue(Convert.ChangeType(array[j], elementType), j);

				prop?.SetValue(instance, result);
				field?.SetValue(instance, result);
				return;
			}

			prop?.SetValue(instance, Convert.ChangeType(value, type));
			field?.SetValue(instance, Convert.ChangeType(value, type));
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
				var structValue = default(object);
				var structField = structType.GetField(structValues[0]);
				var structProp = structType.GetProperty(structValues[0]);

				if(valStr.StartsWith(NUMBER)) structValue = ParseValues(valStr, NUMBER)[0];
				else if(valStr.StartsWith(BOOL)) structValue = ParseValues(valStr, BOOL)[0];
				else if(valStr.StartsWith(CHAR)) structValue = ParseValues(valStr, CHAR)[0];
				else if(valStr.StartsWith(STRING)) structValue = valStr
						.Replace(STRING, "")
						.Replace(TAB, "\t")
						.Replace(NEW_LINE, Environment.NewLine)
						.Replace(SPACE, " ");
				;

				structField?.SetValue(structInstance, structValue);
				structProp?.SetValue(structInstance, structValue);
			}
			return structInstance;
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
					val += struc[i] + sep;
				}

				data[instanceName][structPropName] = val;
				return;
			}

			var values = prop.Split(VALUE, StringSplitOptions.RemoveEmptyEntries);
			if(values.Length < 2)
				return;

			var name = values[0];
			var valueStr = values[1];

			if(TryCacheInstanceValue<float>(NUMBER, instanceName, name, valueStr)) { }
			else if(TryCacheInstanceValue<bool>(BOOL, instanceName, name, valueStr)) { }
			else if(TryCacheInstanceValue<char>(CHAR, instanceName, name, valueStr)) { }
			else if(TryCacheInstanceValue<string>(STRING, instanceName, name, valueStr)) { }
		}
		private bool TryCacheInstanceValue<T>(string separator, string instanceName, string propName,
			string valueStr)
		{
			if(valueStr.Contains(separator) == false)
				return false;

			var finalValues = ParseValues(valueStr, separator);

			if(data.ContainsKey(instanceName) == false)
				data[instanceName] = new();

			var finalValue = finalValues.Length == 1 ? finalValues[0] : finalValues;
			if(finalValue != null)
				data[instanceName][propName] = finalValue;

			return true;
		}
		private static object[] ParseValues(string valueStr, string separator)
		{
			var isString = separator.Contains(STRING);
			var index = isString ? 1 : 0;
			var splitOptions = isString ?
				StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries;
			var allValues = valueStr.Split(separator, splitOptions);
			var finalValues = new object[allValues.Length - index];

			for(int k = index; k < allValues.Length; k++)
			{
				var value = allValues[k].Replace(separator, "");

				try
				{
					if(valueStr.StartsWith(NUMBER))
						finalValues[k - index] = float.Parse(value, CultureInfo.CurrentCulture);
					else if(valueStr.StartsWith(BOOL))
						finalValues[k - index] = bool.Parse(value);
					else if(valueStr.StartsWith(CHAR))
						finalValues[k - index] = char.Parse(value);
					else if(valueStr.StartsWith(STRING))
						finalValues[k - index] = value
							.Replace(TAB, "\t")
							.Replace(NEW_LINE, Environment.NewLine)
							.Replace(SPACE, " ");
				}
				catch(Exception) { continue; }
			}

			return finalValues;
		}
		private static bool IsTypeSupported(Type type)
		{
			return type.IsClass == false || type.IsArray || type == typeof(string);
		}
		private static string Trim(string text)
		{
			return text.Replace("\t", "").Replace(" ", "").Replace(Environment.NewLine, "");
		}
		private static bool IsNumericType(object obj)
		{
			switch(Type.GetTypeCode(obj.GetType()))
			{
				case TypeCode.Byte:
				case TypeCode.SByte:
				case TypeCode.UInt16:
				case TypeCode.UInt32:
				case TypeCode.UInt64:
				case TypeCode.Int16:
				case TypeCode.Int32:
				case TypeCode.Int64:
				case TypeCode.Decimal:
				case TypeCode.Double:
				case TypeCode.Single:
					return true;
				default:
					return false;
			}
		}
		private static bool IsStructType(Type type)
		{
			return type.IsValueType && type.IsEnum == false && type.IsPrimitive == false &&
				type.IsEquivalentTo(typeof(decimal)) == false;
		}
		#endregion
	}
}
