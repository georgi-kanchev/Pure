using SFML.Window;

namespace Pure.Window;

/// <summary>
/// Handles keyboard input.
/// </summary>
public static class Keyboard
{
	/// <summary>
	/// Provides a set of constants representing keyboard keys.
	/// </summary>
	public static class Key
	{
		public const int UNKNOWN = -1, A = 00, B = 01, C = 02, D = 03, E = 04, F = 05, G = 06,
			H = 07, I = 08, J = 09, K = 10, L = 11, M = 12, N = 13, O = 14, P = 15, Q = 16,
			R = 17, S = 18, T = 19, U = 20, V = 21, W = 22, X = 23, Y = 24, Z = 25,
			NUMBER_0 = 26, NUMBER_1 = 27, NUMBER_2 = 28, NUMBER_3 = 29, NUMBER_4 = 30, NUMBER_5 = 31,
			NUMBER_6 = 32, NUMBER_7 = 33, NUMBER_8 = 34, NUMBER_9 = 35,
			ESCAPE = 36, CONTROL_LEFT = 37, SHIFT_LEFT = 38, ALT_LEFT = 39, SYSTEM_LEFT = 40,
			CONTROL_RIGHT = 41, SHIFT_RIGHT = 42, ALT_RIGHT = 43, SYSTEM_RIGHT = 44, MENU = 45,
			BRACKET_LEFT = 46, BRACKET_RIGHT = 47, SEMICOLON = 48, COMMA = 49, DOT = 50, PERIOD = 50,
			QUOTE = 51, SLASH = 52, BACKSLASH = 53, TILDE = 54, EQUAL = 55, HYPHEN = 56, DASH = 56,
			SPACE = 57, ENTER = 58, RETURN = 58, BACKSPACE = 59, TAB = 60, PAGE_UP = 61, PAGE_DOWN = 62,
			END = 63, HOME = 64, INSERT = 65, DELETE = 66, ADD = 67, PLUS = 67, SUBTRACT = 68,
			MINUS = 68, ASTERISK = 69, MULTIPLY = 69, DIVIDE = 70,
			ARROW_LEFT = 71, ARROW_RIGHT = 72, ARROW_UP = 73, ARROW_DOWN = 74,
			NUMPAD_0 = 75, NUMPAD_1 = 76, NUMPAD_2 = 77, NUMPAD_3 = 78, NUMPAD_4 = 79, NUMPAD_5 = 80,
			NUMPAD_6 = 81, NUMPAD_7 = 82, NUMPAD_8 = 83, NUMPAD_9 = 84,
			F1 = 85, F2 = 86, F3 = 87, F4 = 88, F5 = 89, F6 = 90, F7 = 91, F8 = 92, F9 = 93,
			F10 = 94, F11 = 95, F12 = 96, F13 = 97, F14 = 98, F15 = 99,
			PAUSE = 100;
	}

	/// <summary>
	/// Gets the latest keys typed by the user, in order.
	/// </summary>
	public static string KeyTyped { get; internal set; } = "";
	/// <summary>
	/// Gets an array of currently pressed keys.
	/// </summary>
	public static int[] KeysPressed => pressed.ToArray();
	/// <param name="key">
	/// The key to check.</param>
	/// <returns>True if the key is currently pressed, otherwise false.</returns>
	public static bool IsKeyPressed(int key) => pressed.Contains(key);

	#region Backend
	private static readonly List<int> pressed = new();
	private static readonly Dictionary<int, (string, string)> symbols = new()
		{
			{ Key.BRACKET_LEFT, ("[", "{") }, { Key.BRACKET_RIGHT, ("]", "}") },
			{ Key.SEMICOLON, (";", ":") }, { Key.COMMA, (",", "<") }, { Key.DOT, (".", ">") },
			{ Key.QUOTE, ("'", "\"") }, { Key.SLASH, ("/", "?") }, { Key.BACKSLASH, ("\\", "|") },
			{ Key.TILDE, ("`", "~") }, { Key.EQUAL, ("=", "+") }, { Key.HYPHEN, ("-", "_") },
			{ Key.SPACE, (" ", " ") }, { Key.ENTER, ("\n", "\n") },
			{ Key.TAB, ("\t", "") }, { Key.ADD, ("+", "+") }, { Key.MINUS, ("-", "-") },
			{ Key.ASTERISK, ("*", "*") }, { Key.DIVIDE, ("/", "/") }
		};
	private static readonly string[] shiftNumbers = new string[10]
	{
		")", "!", "@", "#", "$", "%", "^", "&", "*", "("
	};

	internal static void CancelInput()
	{
		pressed.Clear();
		KeyTyped = "";
	}

	private static bool IsBetween(int number, int a, int b)
	{
		return (int)a <= number && number <= (int)b;
	}
	private static string GetSymbol(int input, bool shift)
	{
		var i = input;

		if (IsBetween(i, Key.A, Key.Z))
		{
			var str = ((char)('A' + i)).ToString();
			return shift ? str : str.ToLower();
		}
		else if (IsBetween(i, Key.NUMBER_0, Key.NUMBER_9))
		{
			var n = i - Key.NUMBER_0;
			return shift ? shiftNumbers[n] : ((char)('0' + n)).ToString();
		}
		else if (IsBetween(i, Key.NUMPAD_0, Key.NUMPAD_9))
		{
			var n = i - Key.NUMPAD_0;
			return ((char)('0' + n)).ToString();
		}
		else if (symbols.ContainsKey(input))
			return shift ? symbols[input].Item2 : symbols[input].Item1;

		return "";
	}

	internal static void OnKeyPressed(object? s, KeyEventArgs e)
	{
		var key = (int)e.Code;

		if (pressed.Contains(key) == false)
			pressed.Add(key);

		var symb = GetSymbol(key, IsKeyPressed(Key.SHIFT_LEFT) || IsKeyPressed(Key.SHIFT_RIGHT));
		if (KeyTyped.Contains(symb) == false)
			KeyTyped += symb;
	}
	internal static void OnKeyReleased(object? s, KeyEventArgs e)
	{
		var key = (int)e.Code;

		pressed.Remove(key);

		if (pressed.Count == 0)
		{
			KeyTyped = "";
			return;
		}

		// shift released while holding special symbol, just like removing
		// lowercase and uppercase, shift + 1 = !, so releasing shift would
		// never removes the !
		if ((key == Key.SHIFT_LEFT || key == Key.SHIFT_RIGHT) && KeyTyped != "")
		{
			for (int i = 0; i < pressed.Count; i++)
			{
				// get symbol as if shift was pressed
				var symb = GetSymbol(pressed[i], true);
				KeyTyped = KeyTyped.Replace(symb, "");
			}
		}

		if (KeyTyped.Length == 0)
			return;

		var symbol = GetSymbol(key, IsKeyPressed(Key.SHIFT_LEFT) || IsKeyPressed(Key.SHIFT_RIGHT));
		if (symbol == "")
			return;

		KeyTyped = KeyTyped.Replace(symbol.ToLower(), "");
		KeyTyped = KeyTyped.Replace(symbol.ToUpper(), "");
	}
	#endregion
}
