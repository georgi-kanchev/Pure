namespace Pure.Input
{
	/// <summary>
	/// The physical keys on a keyboard.
	/// </summary>
	public static class Key
	{
		public const int UNKNOWN = -1, A = 0, B = 1, C = 2, D = 3, E = 4, F = 5, G = 6, H = 7, I = 8, J = 9, K = 10, L = 11,
			M = 12, N = 13, O = 14, P = 15, Q = 16, R = 17, S = 18, T = 19, U = 20, V = 21, W = 22, X = 23, Y = 24, Z = 25,
			N0 = 26, N1 = 27, N2 = 28,
			N3 = 29, N4 = 30, N5 = 31,
			N6 = 32, N7 = 33, N8 = 34, N9 = 35,
			ESCAPE = 36, CONTROL_LEFT = 37, SHIFT_LEFT = 38, ALT_LEFT = 39, SYSTEM_LEFT = 40,
			CONTROL_RIGHT = 41, SHIFT_RIGHT = 42, ALT_RIGHT = 43, SYSTEM_RIGHT = 44, MENU = 45,
			BRACKET_LEFT = 46, BRACKET_RIGHT = 47, SEMICOLON = 48, COMMA = 49, DOT = 50, PERIOD = 50,
			QUOTE = 51, SLASH = 52, BACKSLASH = 53, TILDE = 54, EQUAL = 55, HYPHEN = 56, DASH = 56,
			SPACE = 57, ENTER = 58, RETURN = 58, BACKSPACE = 59, TAB = 60, PAGE_UP = 61, PAGE_DOWN = 62,
			END = 63, HOME = 64, INSERT = 65, DELETE = 66, ADD = 67, PLUS = 67, SUBTRACT = 68,
			MINUS = 68, ASTERISK = 69, MULTIPLY = 69, DIVIDE = 70,
			ARROW_LEFT = 71,
			ARROW_RIGHT = 72,
			ARROW_UP = 73,
			ARROW_DOWN = 74,
			NUMPAD_0 = 75,
			NUMPAD_1 = 76, NUMPAD_2 = 77, NUMPAD_3 = 78,
			NUMPAD_4 = 79, NUMPAD_5 = 80, NUMPAD_6 = 81,
			NUMPAD_7 = 82, NUMPAD_8 = 83, NUMPAD_9 = 84,
			F1 = 85, F2 = 86, F3 = 87,
			F4 = 88, F5 = 89, F6 = 90,
			F7 = 91, F8 = 92, F9 = 93,
			F10 = 94, F11 = 95, F12 = 96,
			F13 = 97, F14 = 98, F15 = 99,
			PAUSE = 100;
		internal const int COUNT = 101;
	}

	/// <summary>
	/// Handles input from a physical keyboard.
	/// </summary>
	public static class Keyboard
	{
		public static string TypedSymbols { get; internal set; } = "";

		/// <summary>
		/// A collection of each currently pressed <see cref="Key"/>.
		/// </summary>
		public static int[] Pressed => i.Pressed;
		/// <summary>
		/// A collection of each newly pressed <see cref="Key"/>.
		/// </summary>
		public static int[] JustPressed => i.JustPressed;
		/// <summary>
		/// A collection of each newly no longer pressed <see cref="Key"/>.
		/// </summary>
		public static int[] JustReleased => i.JustReleased;

		/// <summary>
		/// Triggers events and provides each <see cref="Key"/> to the collections
		/// accordingly.
		/// </summary>
		public static void Update() => i.Update();

		/// <summary>
		/// Checks whether a <paramref name="key"/> is pressed and returns a result.
		/// </summary>
		public static bool IsPressed(int key) => i.IsPressed(key);
		/// <summary>
		/// Checks whether this is the very moment a <paramref name="key"/> is pressed
		/// and returns a result.
		/// </summary>
		public static bool IsJustPressed(int key) => i.IsJustPressed(key);
		/// <summary>
		/// Checks whether this is the very moment a <paramref name="key"/> is no
		/// longer pressed and returns a result.
		/// </summary>
		public static bool IsJustReleased(int key) => i.IsJustReleased(key);

		/// <summary>
		/// Subscribes a <paramref name="method"/> to a <paramref name="key"/> press.
		/// </summary>
		public static void OnPressed(int key, Action method) => i.OnPressed(key, method);
		/// <summary>
		/// Subscribes a <paramref name="method"/> to a <paramref name="key"/> release.
		/// </summary>
		public static void OnReleased(int key, Action method) => i.OnReleased(key, method);
		/// <summary>
		/// Subscribes a <paramref name="method"/> to a <paramref name="key"/> hold.
		/// </summary>
		public static void WhilePressed(int key, Action method) => i.WhilePressed(key, method);

		#region Backend
		private static readonly KeyboardInstance i = new();
		#endregion
	}

	#region Backend
	internal class KeyboardInstance : Device
	{
		protected override int GetInputCount() => Key.COUNT;
		protected override bool IsPressedRaw(int input)
		{
			var key = (SFML.Window.Keyboard.Key)input;
			return SFML.Window.Keyboard.IsKeyPressed(key);
		}
		protected override void OnPressed(int input)
		{
			Keyboard.TypedSymbols += GetSymbol(input);
		}
		protected override void OnReleased(int input)
		{
			if(Keyboard.TypedSymbols.Length == 0)
				return;

			var symbol = GetSymbol(input);
			if(symbol == "")
				return;

			Keyboard.TypedSymbols = Keyboard.TypedSymbols.Replace(symbol.ToLower(), "");
			Keyboard.TypedSymbols = Keyboard.TypedSymbols.Replace(symbol.ToUpper(), "");
		}

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

		private static bool IsBetween(int number, int a, int b)
		{
			return (int)a <= number && number <= (int)b;
		}
		private static string GetSymbol(int input)
		{
			var i = (int)input;
			var shift =
				SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.LShift) ||
				SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.RShift);

			if(IsBetween(i, Key.A, Key.Z))
			{
				var str = ((char)('A' + i)).ToString();
				return shift ? str : str.ToLower();
			}
			else if(IsBetween(i, Key.N0, Key.N9))
			{
				var n = i - (int)Key.N0;
				return shift ? shiftNumbers[n] : ((char)('0' + n)).ToString();
			}
			else if(IsBetween(i, Key.NUMPAD_0, Key.NUMPAD_9))
			{
				var n = i - (int)Key.NUMPAD_0;
				return ((char)('0' + n)).ToString();
			}
			else if(symbols.ContainsKey(input))
				return shift ? symbols[input].Item2 : symbols[input].Item1;

			return "";
		}
	}
	#endregion
}