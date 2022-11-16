using System.Globalization;
using System.Reflection;

namespace Purity.Tools
{
	public class Storage
	{
		//public bool Save(string filePath)
		//{
		//	return false;
		//}
		public bool Load(string filePath)
		{
			if(File.Exists(filePath) == false)
				return false;

			var file = File.ReadAllText(filePath);

			if(file.Length == 0)
				return false;

			var split = file.Split(INSTANCE, StringSplitOptions.RemoveEmptyEntries);

			data.Clear();
			for(int i = 0; i < split?.Length; i++)
			{
				var props = split[i].Split(INSTANCE_PROPERTY, StringSplitOptions.RemoveEmptyEntries);
				var instanceName = Trim(props[0]);

				for(int j = 1; j < props?.Length; j++)
					CacheProp(instanceName, props[j]);
			}
			return true;
		}

		public bool Populate(object instance, string instanceName)
		{
			if(instance == null || data.ContainsKey(instanceName) == false)
				return false;

			var type = instance.GetType();
			var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			for(int i = 0; i < props.Length; i++)
				PopulateMember(props[i], instanceName, instance);
			for(int i = 0; i < fields.Length; i++)
				if(fields[i].IsSpecialName == false)
					PopulateMember(fields[i], instanceName, instance);

			return true;
		}

		#region Backend
		private const string NUMBER = "~#", BOOL = "~;", CHAR = "~'", STRING = "~\"",
			INSTANCE = "~@", INSTANCE_PROPERTY = "~~", VALUE = "~|",
			STRUCT = "~&", STRUCT_PROPERTY = "~=",
			SPACE = "~_", TAB = "~__", NEW_LINE = "~/";
		private readonly Dictionary<string, Dictionary<string, object>> data = new();

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
				var structPropName = Trim(struc[0]);

				var val = "";
				for(int i = 1; i < struc.Length; i++)
				{
					var sep = i < struc.Length - 1 ? STRUCT : "";
					val += Trim(struc[i]) + sep;
				}

				data[instanceName][structPropName] = val;
				return;
			}

			var values = prop.Split(VALUE, StringSplitOptions.RemoveEmptyEntries);
			if(values.Length < 2)
				return;

			var name = Trim(values[0]);
			var valueStr = Trim(values[1]);

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
		private static string Trim(string text)
		{
			return text.Replace("\t", "").Replace(" ", "").Replace(Environment.NewLine, "");
		}
		#endregion
	}
}
