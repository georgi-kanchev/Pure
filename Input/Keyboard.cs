namespace Pure.Input
{
	/// <summary>
	/// The physical keys on a keyboard.
	/// </summary>
	public enum Key
	{
#pragma warning disable CA1069
#pragma warning disable CS1591
		Unknown = -1,
		A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
		N0, N1, N2, N3, N4, N5, N6, N7, N8, N9,
		Escape, ControlLeft, ShiftLeft, AltLeft, SystemLeft, ControlRight, ShiftRight, AltRight,
		SystemRight, Menu, BracketLeft, BracketRight, Semicolon, Comma, Dot, Period = 50,
		Quote, Slash, Backslash, Tilde, Equal, Hyphen, Dash = 56, Space, Enter, Return = 58,
		Backspace, Tab, PageUp, PageDown, End, Home, Insert, Delete, Add, Plus = 67, Subtract,
		Minus = 68, Asterisk, Multiply = 69, Divide, ArrowLeft, ArrowRight, ArrowUp, ArrowDown,
		Numpad0, Numpad1, Numpad2, Numpad3, Numpad4, Numpad5, Numpad6, Numpad7, Numpad8, Numpad9,
		F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, Pause,
#pragma warning restore CA1069
#pragma warning restore CS1591
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
		public static Key[] Pressed => i.Pressed;
		/// <summary>
		/// A collection of each newly pressed <see cref="Key"/>.
		/// </summary>
		public static Key[] JustPressed => i.JustPressed;
		/// <summary>
		/// A collection of each newly no longer pressed <see cref="Key"/>.
		/// </summary>
		public static Key[] JustReleased => i.JustReleased;

		/// <summary>
		/// Triggers events and provides each <see cref="Key"/> to the collections
		/// accordingly.
		/// </summary>
		public static void Update() => i.Update();

		/// <summary>
		/// Checks whether an <paramref name="input"/> is pressed and returns a result.
		/// </summary>
		public static bool IsPressed(Key input) => i.IsPressed(input);
		/// <summary>
		/// Checks whether this is the very moment an <paramref name="input"/> is pressed
		/// and returns a result.
		/// </summary>
		public static bool IsJustPressed(Key input) => i.IsJustPressed(input);
		/// <summary>
		/// Checks whether this is the very moment an <paramref name="input"/> is no
		/// longer pressed and returns a result.
		/// </summary>
		public static bool IsJustReleased(Key input) => i.IsJustReleased(input);

		/// <summary>
		/// Subscribes a <paramref name="method"/> to an <paramref name="input"/> press.
		/// </summary>
		public static void OnPressed(Key input, Action method) => i.OnPressed(input, method);
		/// <summary>
		/// Subscribes a <paramref name="method"/> to an <paramref name="input"/> release.
		/// </summary>
		public static void OnReleased(Key input, Action method) => i.OnReleased(input, method);
		/// <summary>
		/// Subscribes a <paramref name="method"/> to an <paramref name="input"/> hold.
		/// </summary>
		public static void WhilePressed(Key input, Action method) => i.WhilePressed(input, method);

		#region Backend
		private static readonly KeyboardInstance i = new();
		#endregion
	}

	#region Backend
	internal class KeyboardInstance : Device<Key>
	{
		protected override bool IsPressedRaw(Key input)
		{
			var key = (SFML.Window.Keyboard.Key)input;
			return SFML.Window.Keyboard.IsKeyPressed(key);
		}
		protected override void OnPressed(Key input)
		{
			Keyboard.TypedSymbols += GetSymbol(input);
		}
		protected override void OnReleased(Key input)
		{
			if(Keyboard.TypedSymbols.Length == 0)
				return;

			Keyboard.TypedSymbols = Keyboard.TypedSymbols.Replace(GetSymbol(input), "");
		}

		private static readonly Dictionary<Key, (string, string)> symbols = new()
		{
			{ Key.BracketLeft, ("[", "{") }, { Key.BracketRight, ("]", "}") },
			{ Key.Semicolon, (";", ":") }, { Key.Comma, (",", "<") }, { Key.Dot, (".", ">") },
			{ Key.Quote, ("'", "\"") }, { Key.Slash, ("/", "?") }, { Key.Backslash, ("\\", "|") },
			{ Key.Tilde, ("`", "~") }, { Key.Equal, ("=", "+") }, { Key.Hyphen, ("-", "_") },
			{ Key.Space, (" ", " ") }, { Key.Enter, ("\n", "\n") },
			{ Key.Tab, ("\t", "") }, { Key.Add, ("+", "+") }, { Key.Minus, ("-", "-") },
			{ Key.Asterisk, ("*", "*") }, { Key.Divide, ("/", "/") }
		};
		private static readonly string[] shiftNumbers = new string[10]
		{
			")", "!", "@", "#", "$", "%", "^", "&", "*", "("
		};

		private static bool IsBetween(int number, Key a, Key b)
		{
			return (int)a <= number && number <= (int)b;
		}
		private static string GetSymbol(Key input)
		{
			var i = (int)input;
			var shift =
				SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.LShift) ||
				SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.RShift);

			if(IsBetween(i, Key.A, Key.Z))
			{
				var str = input.ToString();
				return shift ? str : str.ToLower();
			}
			else if(IsBetween(i, Key.N0, Key.N9))
			{
				var n = i - (int)Key.N0;
				return shift ? shiftNumbers[n] : ((char)('0' + n)).ToString();
			}
			else if(IsBetween(i, Key.Numpad0, Key.Numpad9))
			{
				var n = i - (int)Key.Numpad0;
				return ((char)('0' + n)).ToString();
			}
			else if(symbols.ContainsKey(input))
				return shift ? symbols[input].Item2 : symbols[input].Item1;

			return "";
		}
	}
	#endregion
}