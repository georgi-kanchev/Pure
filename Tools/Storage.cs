using System.Globalization;
using System.Reflection;

namespace Purity.Tools
{
	public class Storage
	{
		public bool Save(string filePath)
		{
			return false;
		}
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
				{
					var prop = props[j];
					var isStruct = prop.Contains(STRUCT);

					if(isStruct)
					{
						ProcessStruct();
						continue;
					}

					var values = prop.Split(VALUE, StringSplitOptions.RemoveEmptyEntries);
					if(values.Length < 2)
						continue;

					var name = Trim(values[0]);
					var valueStr = Trim(values[1]);

					if(ProcessInstanceValue<float>(NUMBER)) { }
					else if(ProcessInstanceValue<bool>(BOOL)) { }
					else if(ProcessInstanceValue<char>(CHAR)) { }
					else if(ProcessInstanceValue<string>(STRING)) { }

					bool ProcessInstanceValue<T>(string separator)
					{
						if(valueStr.Contains(separator) == false)
							return false;

						var isString = separator.Contains(STRING);
						var index = isString ? 1 : 0;
						var splitOptions = isString ?
							StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries;
						var allValues = valueStr.Split(separator, splitOptions);
						var finalValues = new object[allValues.Length - index];

						for(int k = index; k < allValues?.Length; k++)
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

						if(data.ContainsKey(instanceName) == false)
							data[instanceName] = new();

						var finalValue = finalValues.Length == 1 ? finalValues[0] : finalValues;
						if(finalValue != null)
							data[instanceName][name] = finalValue;

						return true;
					}
					void ProcessStruct()
					{
						var struc = prop.Split(STRUCT, StringSplitOptions.RemoveEmptyEntries);
						if(struc.Length < 2)
							return;

						var structPropName = Trim(struc[0]);
						var structValueStr = Trim(struc[1]);
						var structProps = structValueStr
							.Split(STRUCT_PROPERTY, StringSplitOptions.RemoveEmptyEntries);

						for(int k = 0; k < structProps?.Length; k++)
						{
							var structProp = structProps[k].Split(VALUE, StringSplitOptions.RemoveEmptyEntries);
						}

					}
				}
			}
			return true;

			string Trim(string text) => text.Replace("\t", "").Replace(" ", "").Replace(Environment.NewLine, "");
		}

		public bool Populate(object instance, string instanceName)
		{
			if(instance == null || data.ContainsKey(instanceName) == false)
				return false;

			var type = instance.GetType();
			var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			for(int i = 0; i < props.Length; i++)
				ProcessMember(props[i]);
			for(int i = 0; i < fields.Length; i++)
				if(fields[i].IsSpecialName == false)
					ProcessMember(fields[i]);

			return true;

			void ProcessMember(MemberInfo memberInfo)
			{
				var prop = memberInfo is PropertyInfo p ? p : null;
				var field = memberInfo is FieldInfo f ? f : null;
				var type = prop?.PropertyType ?? field?.FieldType;
				var name = memberInfo.Name;

				if(data[instanceName].ContainsKey(name) == false || type == null)
					return;

				var value = data[instanceName][name];
				var valueType = value.GetType();

				if(valueType.IsArray)
				{
					var array = (object[])value;
					var elementType = type.GetElementType();
					if(elementType == null)
						return;

					var result = Array.CreateInstance(elementType, array.Length);

					for(int j = 0; j < array?.Length; j++)
						result.SetValue(Convert.ChangeType(array[j], elementType), j);

					prop?.SetValue(instance, result);
					field?.SetValue(instance, result);

					return;
				}

				prop?.SetValue(instance, Convert.ChangeType(value, type));
				field?.SetValue(instance, Convert.ChangeType(value, type));
			}
		}

		private const string NUMBER = "~#", BOOL = "~;", CHAR = "~'", STRING = "~\"",
			INSTANCE = "~@", INSTANCE_PROPERTY = "~~", VALUE = "~|",
			STRUCT = "~&", STRUCT_PROPERTY = "~-",
			SPACE = "~_", TAB = "~__", NEW_LINE = "~/";
		private readonly Dictionary<string, Dictionary<string, object>> data = new();
	}
}
