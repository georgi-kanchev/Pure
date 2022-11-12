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
			var split = file.Split(SeparatorProperty + SeparatorProperty, StringSplitOptions.RemoveEmptyEntries);

			data.Clear();
			for(int i = 0; i < split?.Length; i++)
			{
				var props = split[i].Split(SeparatorProperty, StringSplitOptions.RemoveEmptyEntries);
				var instanceName = props[0];
				data[instanceName] = new();

				for(int j = 1; j < props?.Length; j++)
				{
					var prop = props[j];
					var values = prop.Split(SEPARATOR_VALUE, StringSplitOptions.RemoveEmptyEntries);
					var name = values[0].Replace("\t", "");
					var valueStr = values[1];

					if(ProcessValue<decimal>(SEPARATOR_NUMBER)) { }
					else if(ProcessValue<bool>(SEPARATOR_BOOL)) { }
					else if(ProcessValue<char>(SEPARATOR_CHAR)) { }
					else if(ProcessValue<string>(SEPARATOR_STRING)) { }
					else if(ProcessValue<string>(SEPARATOR_INSTANCE_REF)) { }

					bool ProcessValue<T>(string separator)
					{
						if(valueStr.Contains(separator) == false)
							return false;

						var isString = separator == SEPARATOR_STRING;
						var index = isString ? 1 : 0;
						var splitOptions = isString ?
							StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries;
						var allValues = valueStr.Split(separator, splitOptions);
						var finalValues = new object[allValues.Length - index];

						for(int i = index; i < allValues?.Length; i++)
						{
							if(valueStr.StartsWith(SEPARATOR_NUMBER) &&
								decimal.TryParse(allValues[i].Replace(separator, ""), out var number))
								finalValues[i - index] = number;
							else if(valueStr.StartsWith(SEPARATOR_BOOL) &&
								bool.TryParse(allValues[i].Replace(separator, ""), out var boolean))
								finalValues[i - index] = boolean;
							else if(valueStr.StartsWith(SEPARATOR_CHAR) &&
								char.TryParse(allValues[i].Replace(separator, ""), out var ch))
								finalValues[i - index] = ch;
							else if(valueStr.StartsWith(SEPARATOR_STRING))
								finalValues[i - index] = allValues[i].Replace(separator, "");
							else if(valueStr.StartsWith(SEPARATOR_INSTANCE_REF))
								finalValues[i - index] = allValues[i];
						}

						data[instanceName][name] = finalValues.Length == 1 ? finalValues[0] : finalValues;
						return true;
					}
				}
			}
			return true;
		}

		public bool Populate(object instance, string instanceName)
		{
			if(instance == null || data.ContainsKey(instanceName) == false)
				return false;

			var props = instance.GetType().GetProperties();
			for(int i = 0; i < props.Length; i++)
			{
				var prop = props[i];
				var type = prop.PropertyType;
				var name = prop.Name;

				if(data[instanceName].ContainsKey(name) == false)
					continue;

				var value = data[instanceName][name];
				var valueType = value.GetType();

				if(valueType.IsArray)
				{
					var array = (object[])value;
					var elementType = type.GetElementType();
					var result = Array.CreateInstance(elementType, array.Length);

					for(int j = 0; j < array?.Length; j++)
						result.SetValue(Convert.ChangeType(array[j], elementType), j);

					prop.SetValue(instance, result);
					continue;
				}

				prop.SetValue(instance, Convert.ChangeType(value, type));
			}

			return true;
		}

		private const string SEPARATOR_NUMBER = "~#";
		private const string SEPARATOR_BOOL = "~;";
		private const string SEPARATOR_CHAR = "~'";
		private const string SEPARATOR_STRING = "~\"";
		private const string SEPARATOR_INSTANCE_REF = "~@";
		private static string SeparatorProperty => Environment.NewLine;
		private const string SEPARATOR_VALUE = "~|";
		private readonly Dictionary<string, Dictionary<string, object>> data = new();
	}
}
