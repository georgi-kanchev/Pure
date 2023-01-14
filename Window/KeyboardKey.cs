namespace Pure.Window
{
	/// <summary>
	/// Handles the physical keys on a keyboard.
	/// </summary>
	public static class KeyboardKey
	{
		public const int UNKNOWN = -1, A = 00, B = 01, C = 02, D = 03, E = 04, F = 05, G = 06,
			H = 07, I = 08, J = 09, K = 10, L = 11, M = 12, N = 13, O = 14, P = 15, Q = 16,
			R = 17, S = 18, T = 19, U = 20, V = 21, W = 22, X = 23, Y = 24, Z = 25,
			NUMBER_0 = 26, NUMBER_1 = 27, NUMBER_2 = 28,
			NUMBER_3 = 29, NUMBER_4 = 30, NUMBER_5 = 31,
			NUMBER_6 = 32, NUMBER_7 = 33, NUMBER_8 = 34, NUMBER_9 = 35,
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

		/// <summary>
		/// All currently held keys as text, in order.
		/// </summary>
		public static string Typed { get; internal set; } = "";

		public static int[] Pressed => pressed.ToArray();

		public static bool IsPressed(int key) => pressed.Contains(key);

		public static void OnPressed(int key, Action method)
		{
			Window.window.KeyPressed += (s, e) =>
			{
				if(key == (int)e.Code)
					method?.Invoke();
			};
		}
		public static void OnReleased(int key, Action method)
		{
			Window.window.KeyReleased += (s, e) =>
			{
				if(key == (int)e.Code)
					method?.Invoke();
			};
		}

		#region Backend
		private static readonly List<int> pressed = new();
		private static readonly Dictionary<int, (string, string)> symbols = new()
		{
			{ BRACKET_LEFT, ("[", "{") }, { BRACKET_RIGHT, ("]", "}") },
			{ SEMICOLON, (";", ":") }, { COMMA, (",", "<") }, { DOT, (".", ">") },
			{ QUOTE, ("'", "\"") }, { SLASH, ("/", "?") }, { BACKSLASH, ("\\", "|") },
			{ TILDE, ("`", "~") }, { EQUAL, ("=", "+") }, { HYPHEN, ("-", "_") },
			{ SPACE, (" ", " ") }, { ENTER, ("\n", "\n") },
			{ TAB, ("\t", "") }, { ADD, ("+", "+") }, { MINUS, ("-", "-") },
			{ ASTERISK, ("*", "*") }, { DIVIDE, ("/", "/") }
		};
		private static readonly string[] shiftNumbers = new string[10]
		{
			")", "!", "@", "#", "$", "%", "^", "&", "*", "("
		};

		static KeyboardKey()
		{
			Window.window.KeyPressed += (s, e) =>
			{
				var key = (int)e.Code;
				if(pressed.Contains(key) == false)
					pressed.Add(key);

				var symb = GetSymbol(key, IsPressed(SHIFT_LEFT) || IsPressed(SHIFT_RIGHT));
				if(Typed.Contains(symb) == false)
					Typed += symb;
			};
			Window.window.KeyReleased += (s, e) =>
			{
				var key = (int)e.Code;
				pressed.Remove(key);

				if(pressed.Count == 0)
				{
					Typed = "";
					return;
				}

				// shift released while holding special symbol, just like removing
				// lowercase and uppercase, shift + 1 = !, so releasing shift would
				// never removes the !
				if((key == SHIFT_LEFT || key == SHIFT_RIGHT) && Typed != "")
				{
					for(int i = 0; i < pressed.Count; i++)
					{
						// get symbol as if shift was pressed
						var symb = GetSymbol(pressed[i], true);
						Typed = Typed.Replace(symb, "");
					}
				}

				if(Typed.Length == 0)
					return;

				var symbol = GetSymbol(key, IsPressed(SHIFT_LEFT) || IsPressed(SHIFT_RIGHT));
				if(symbol == "")
					return;

				Typed = Typed.Replace(symbol.ToLower(), "");
				Typed = Typed.Replace(symbol.ToUpper(), "");

			};
		}

		internal static void CancelInput()
		{
			pressed.Clear();
			Typed = "";
		}

		private static bool IsBetween(int number, int a, int b)
		{
			return (int)a <= number && number <= (int)b;
		}
		private static string GetSymbol(int input, bool shift)
		{
			var i = input;

			if(IsBetween(i, A, Z))
			{
				var str = ((char)('A' + i)).ToString();
				return shift ? str : str.ToLower();
			}
			else if(IsBetween(i, NUMBER_0, NUMBER_9))
			{
				var n = i - NUMBER_0;
				return shift ? shiftNumbers[n] : ((char)('0' + n)).ToString();
			}
			else if(IsBetween(i, NUMPAD_0, NUMPAD_9))
			{
				var n = i - NUMPAD_0;
				return ((char)('0' + n)).ToString();
			}
			else if(symbols.ContainsKey(input))
				return shift ? symbols[input].Item2 : symbols[input].Item1;

			return "";
		}
		#endregion
	}
}
