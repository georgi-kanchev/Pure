using System.Globalization;
using System.Reflection;

namespace Pure.Storage
{
	public class Storage
	{
		public object this[string id]
		{
			set
			{
				if(id == null)
					throw new ArgumentNullException(nameof(id));

				if(value == null)
				{
					if(data.ContainsKey(id)) // delete
						data.Remove(id);

					return;
				}

				var type = value.GetType();
				var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

				if(data.ContainsKey(id) == false)
					data[id] = new();

				for(int i = 0; i < props.Length; i++)
				{
					var prop = props[i];

					if(IsTypeSupported(prop.PropertyType) == false)
						continue;

					var val = prop.GetValue(value);

					if(val != null)
						data[id][prop.Name] = val;
				}
				for(int i = 0; i < fields.Length; i++)
				{
					var field = fields[i];
					if(IsTypeSupported(field.FieldType) == false || field.Name.Contains('<'))
						continue;

					var val = field.GetValue(value);

					if(val != null)
						data[id][field.Name] = val;
				}
			}
		}

		public Storage() { }
		public Storage(string path)
		{
			if(File.Exists(path) == false)
				return;

			var file = File.ReadAllText(path);

			if(file.Length == 0)
				return;

			var split = Trim(file).Split(OBJ, StringSplitOptions.RemoveEmptyEntries);

			data.Clear();
			for(int i = 0; i < split?.Length; i++)
			{
				var props = split[i].Split(OBJ_PROP, StringSplitOptions.RemoveEmptyEntries);
				var instanceName = props[0];

				for(int j = 1; j < props?.Length; j++)
					CacheProp(instanceName, props[j]);
			}
		}

		public bool HasID(string id)
		{
			return id != null && data.ContainsKey(id);
		}
		public void Save(string path, bool isDataFormatted = true)
		{
			var dir = Path.GetDirectoryName(path);
			if(dir != "" && Directory.Exists(dir) == false)
				return;

			var result = isDataFormatted ? FILE_HEADER + Environment.NewLine : "";
			var newLine = isDataFormatted ? Environment.NewLine : "";
			var space = isDataFormatted ? " " : "";
			var tab = isDataFormatted ? new string(' ', 4) : "";

			foreach(var kvp in data)
			{
				result += OBJ + space + kvp.Key;

				foreach(var kvp2 in kvp.Value)
				{
					var value = kvp2.Value;
					var type = value.GetType();

					result += newLine + tab + OBJ_PROP + space + kvp2.Key;

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

			File.WriteAllText(path, result);
		}
		public void Populate(object instance, string id)
		{
			if(instance == null || data.ContainsKey(id) == false)
				return;

			var type = instance.GetType();
			var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			for(int i = 0; i < props.Length; i++)
				PopulateMember(props[i], id, instance);
			for(int i = 0; i < fields.Length; i++)
				if(fields[i].Name.Contains('<') == false)
					PopulateMember(fields[i], id, instance);
		}

		#region Backend
		private const string SEP = "~", OBJ = SEP + "@", OBJ_PROP = SEP + "~", VALUE = SEP + "|",
			STRUCT = SEP + "&", STRUCT_PROP = SEP + "=", SPACE = SEP + "_", TAB = SEP + "__", NEW_LINE = SEP + "/";
		private const string FILE_HEADER = @$"Pure - Storage file
| - - - - - - - - - - - - -
| Map of symbols
| - - - - - - - - - - - - -
|	{SEP} Global separator
|
|	{OBJ} Object
|	{OBJ_PROP} Property
|	{STRUCT} Property containing sub properties (struct)
|	{STRUCT_PROP} Sub property
|	{VALUE} Value
|
|	{NEW_LINE} String new line
|	{SPACE} String space
|	{TAB} String tab
| - - - - - - - - - - - - -
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

					result += newLine + tab + tab + STRUCT_PROP + space + newLine;

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

					result += newLine + tab + tab + STRUCT_PROP + space;

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

			if(value is string str && str.Contains(STRUCT_PROP))
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

			if(valueType.IsArray || valueType == type.GetElementType())
			{
				if(valueType.IsArray == false)
				{
					var singleElementArray = Array.CreateInstance(valueType, 1);
					singleElementArray.SetValue(value, 0);
					prop?.SetValue(instance, singleElementArray);
					field?.SetValue(instance, singleElementArray);
					return;
				}

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
			var structProps = str.Split(STRUCT_PROP, StringSplitOptions.RemoveEmptyEntries);

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
				else if(structProp != null && structProp.GetSetMethod() != null)
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
