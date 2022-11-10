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

			// remove all tabs
			while(file.Contains('\t'))
				file = file.Replace("\t", "");

			var split = file.Split(SYMBOL_INSTANCE, StringSplitOptions.RemoveEmptyEntries);

			data.Clear();
			for(int i = 0; i < split?.Length; i++)
			{
				var props = split[i].Split(SymbolProperty, StringSplitOptions.RemoveEmptyEntries);

				// first prop should be the instance name
				var instanceName = props[0];

				for(int j = 1; j < props?.Length; j++)
				{
					var prop = props[j];
					var values = prop.Split(SYMBOL_VALUE, StringSplitOptions.RemoveEmptyEntries);
					var name = values[0];
					var value = values[1];
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
				var name = prop.Name;

				if(data[instanceName].ContainsKey(name) == false)
					continue;

				prop.SetValue(instance, data[instanceName][name]);
			}

			return true;
		}

		private const char SYMBOL_INSTANCE = '@';
		private const char SYMBOL_COLLECTION_ELEMENT = '`';
		private static string SymbolProperty => Environment.NewLine;
		private const char SYMBOL_VALUE = ' ';
		private readonly Dictionary<string, Dictionary<string, object>> data = new();
	}
}
