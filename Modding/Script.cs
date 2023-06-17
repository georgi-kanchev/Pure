namespace Pure.Modding;

using MoonSharp.Interpreter;

/// <summary>
/// Wrapper for executing Lua scripts.
/// </summary>
public class Script
{
	/// <summary>
	/// Initializes a new script instance with the specified Lua code.
	/// </summary>
	/// <param name="luaCode">The Lua code to execute.</param>
	public Script(string luaCode)
	{
		script.DebuggerEnabled = false;
		script.DoString(luaCode);
	}

	/// <param name="path">
	/// The path to the Lua code file.</param>
	/// <returns>A new script instance with the loaded Lua code.</returns>
	public static Script Load(string path) => new Script(File.ReadAllText(path));

	/// <summary>
	/// Calls a Lua function with the specified function name 
	/// and parameters.
	/// </summary>
	/// <param name="functionName">The name of the function to call.</param>
	/// <param name="parameters">The parameters to pass to the function.</param>
	/// <returns>The return value of the function.</returns>
	public object? Call(string functionName, params object[] parameters)
	{
		var result = script.Call(script.Globals.Get(functionName), parameters);
		return GetPrimitive(result);
	}
	/// <summary>
	/// Adds a C# method with the specified name to the Lua environment.
	/// </summary>
	/// <param name="methodName">The name of the method.</param>
	/// <param name="method">The method to add.</param>
	public void Add(string methodName, Delegate method)
	{
		script.Globals[methodName] = method;
	}

	#region Backend
	private readonly MoonSharp.Interpreter.Script script = new(CoreModules.None);

	private object? GetPrimitive(DynValue value)
	{
		switch (value.Type)
		{
			case DataType.Boolean: return value.Boolean;
			case DataType.Number: return (float)value.Number;
			case DataType.String: return value.String;
			case DataType.Tuple:
				{
					var array = value.Tuple;
					var result = new object?[array.Length];
					for (var i = 0; i < array.Length; i++)
						result[i] = GetPrimitive(array[i]);

					return result;
				}
			case DataType.Void: default: return default;
		}
	}
	#endregion
}
