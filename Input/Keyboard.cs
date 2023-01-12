namespace Pure.Input
{
	/// <summary>
	/// The physical keys on a keyboard.
	/// </summary>
	public static class Key
	{
		public const int VcUndefined = 0,
			VcEscape = 1,
			Vc1 = 2,
			Vc2 = 3,
			Vc3 = 4,
			Vc4 = 5,
			Vc5 = 6,
			Vc6 = 7,
			Vc7 = 8,
			Vc8 = 9,
			Vc9 = 10,
			Vc0 = 11,
			VcMinus = 12,
			VcEquals = 13,
			VcBackspace = 14,
			VcTab = 15,
			VcQ = 16,
			VcW = 17,
			VcE = 18,
			VcR = 19,
			VcT = 20,
			VcY = 21,
			VcU = 22,
			VcI = 23,
			VcO = 24,
			VcP = 25,
			VcOpenBracket = 26,
			VcCloseBracket = 27,
			VcEnter = 28,
			VcLeftControl = 29,
			VcA = 30,
			VcS = 31,
			VcD = 32,
			VcF = 33,
			VcG = 34,
			VcH = 35,
			VcJ = 36,
			VcK = 37,
			VcL = 38,
			VcSemicolon = 39,
			VcQuote = 40,
			VcBackquote = 41,
			VcLeftShift = 42,
			VcBackSlash = 43,
			VcZ = 44,
			VcX = 45,
			VcC = 46,
			VcV = 47,
			VcB = 48,
			VcN = 49,
			VcM = 50,
			VcComma = 51,
			VcPeriod = 52,
			VcSlash = 53,
			VcRightShift = 54,
			VcNumPadMultiply = 55,
			VcLeftAlt = 56,
			VcSpace = 57,
			VcCapsLock = 58,
			VcF1 = 59,
			VcF2 = 60,
			VcF3 = 61,
			VcF4 = 62,
			VcF5 = 63,
			VcF6 = 64,
			VcF7 = 65,
			VcF8 = 66,
			VcF9 = 67,
			VcF10 = 68,
			VcNumLock = 69,
			VcScrollLock = 70,
			VcNumPad7 = 71,
			VcNumPad8 = 72,
			VcNumPad9 = 73,
			VcNumPadSubtract = 74,
			VcNumPad4 = 75,
			VcNumPad5 = 76,
			VcNumPad6 = 77,
			VcNumPadAdd = 78,
			VcNumPad1 = 79,
			VcNumPad2 = 80,
			VcNumPad3 = 81,
			VcNumPad0 = 82,
			VcNumPadSeparator = 83,
			VcF11 = 87,
			VcF12 = 88,
			VcF13 = 91,
			VcF14 = 92,
			VcF15 = 93,
			VcF16 = 99,
			VcF17 = 100,
			VcF18 = 101,
			VcF19 = 102,
			VcF20 = 103,
			VcF21 = 104,
			VcF22 = 105,
			VcF23 = 106,
			VcF24 = 107,
			VcKatakana = 112,
			VcUnderscore = 115,
			VcFurigana = 119,
			VcKanji = 121,
			VcHiragana = 123,
			VcYen = 125,
			VcNumPadComma = 126,
			VcNumPadEquals = 3597,
			VcNumPadEnter = 3612,
			VcRightControl = 3613,
			VcNumPadDivide = 3637,
			VcPrintScreen = 3639,
			VcRightAlt = 3640,
			VcPause = 3653,
			VcLesserGreater = 3654,
			VcHome = 3655,
			VcPageUp = 3657,
			VcEnd = 3663,
			VcPageDown = 3665,
			VcInsert = 3666,
			VcDelete = 3667,
			VcLeftMeta = 3675,
			VcRightMeta = 3676,
			VcContextMenu = 3677,
			VcMediaPrevious = 57360,
			VcMediaNext = 57369,
			VcVolumeMute = 57376,
			VcAppCalculator = 57377,
			VcMediaPlay = 57378,
			VcMediaStop = 57380,
			VcMediaEject = 57388,
			VcVolumeDown = 57390,
			VcVolumeUp = 57392,
			VcBrowserHome = 57394,
			VcAppMusic = 57404,
			VcUp = 57416,
			VcLeft = 57419,
			VcClear = 57420,
			VcRight = 57421,
			VcDown = 57424,
			VcPower = 57438,
			VcSleep = 57439,
			VcWake = 57443,
			VcAppPictures = 57444,
			VcBrowserSearch = 57445,
			VcBrowserFavorites = 57446,
			VcBrowserRefresh = 57447,
			VcBrowserStop = 57448,
			VcBrowserForward = 57449,
			VcBrowserBack = 57450,
			VcAppMail = 57452,
			VcMediaSelect = 57453,
			VcNumPadHome = 60999,
			VcNumPadUp = 61000,
			VcNumPadPageUp = 61001,
			VcNumPadLeft = 61003,
			VcNumPadClear = 61004,
			VcNumPadRight = 61005,
			VcNumPadEnd = 61007,
			VcNumPadDown = 61008,
			VcNumPadPageDown = 61009,
			VcNumPadInsert = 61010,
			VcNumPadDelete = 61011,
			VcSunOpen = 65396,
			VcSunHelp = 65397,
			VcSunProps = 65398,
			VcSunFront = 65399,
			VcSunStop = 65400,
			VcSunAgain = 65401,
			VcSunUndo = 65402,
			VcSunCut = 65403,
			VcSunCopy = 65404,
			VcSunInsert = 65405,
			VcSunFind = 65406,
			CharUndefined = 65535;


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
		/// <summary>
		/// The result of each currently held <see cref="Key"/> as text.
		/// </summary>
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

		static Keyboard()
		{
			Device.input.KeyPressed += (s, e) =>
			{
				TypedSymbols += ((char)e.Data.KeyChar).ToString();
				i.justPressed.Add(e.Data.RawCode);
				Device.Trigger(e.Data.RawCode, i.pressedEvents);
			};
			Device.input.KeyReleased += (s, e) =>
			{
				if(TypedSymbols.Length == 0)
					return;

				var symbol = ((char)e.Data.KeyChar).ToString();
				if(symbol == "")
					return;

				TypedSymbols = TypedSymbols.Replace(symbol.ToLower(), "");
				TypedSymbols = TypedSymbols.Replace(symbol.ToUpper(), "");

				i.justReleased.Add(e.Data.RawCode);
				Device.Trigger(e.Data.RawCode, i.releasedEvents);
			};
		}

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