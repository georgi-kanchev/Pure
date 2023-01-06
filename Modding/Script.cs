using MoonSharp.Interpreter;

namespace Pure.Modding
{
	public class Script
	{
		public Script(string luaCode)
		{
			script.DebuggerEnabled = false;
			script.DoString(luaCode);
		}

		public object? Call(string function, params object[] parameters)
		{
			var result = script.Call(script.Globals.Get(function), parameters);
			return GetPrimitive(result);
		}
		public void Add(string name, Delegate method)
		{
			script.Globals[name] = method;
		}

		#region Backend
		private readonly MoonSharp.Interpreter.Script script = new(CoreModules.None);

		private object? GetPrimitive(DynValue value)
		{
			switch(value.Type)
			{
				case DataType.Boolean: return value.Boolean;
				case DataType.Number: return (float)value.Number;
				case DataType.String: return value.String;
				case DataType.Tuple:
					{
						var array = value.Tuple;
						var result = new object?[array.Length];
						for(int i = 0; i < array.Length; i++)
							result[i] = GetPrimitive(array[i]);

						return result;
					}
				case DataType.Void: default: return default;
			}
		}
		#endregion
	}
}