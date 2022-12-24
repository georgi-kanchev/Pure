namespace Purity.Input
{
	/// <summary>
	/// The physical keys on a keyboard.
	/// </summary>
	public enum Key
	{
#pragma warning disable CA1069
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
	}

	/// <summary>
	/// (Inherits <see cref="Device{T}"/>)<br></br><br></br>
	/// 
	/// Handles input from a physical keyboard.
	/// </summary>
	public class Keyboard : Device<Key>
	{
		public string TypedSymbols { get; private set; } = "";

		public override void Update()
		{
			TypedSymbols = "";
			base.Update();
		}
		protected override bool IsPressedRaw(Key input)
		{
			var key = (SFML.Window.Keyboard.Key)input;
			var pressed = SFML.Window.Keyboard.IsKeyPressed(key);
			var i = (int)input;
			var shift =
				SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.LShift) ||
				SFML.Window.Keyboard.IsKeyPressed(SFML.Window.Keyboard.Key.RShift);

			if(pressed == false)
				return false;

			if(IsBetween(i, Key.A, Key.Z))
			{
				var str = input.ToString();
				TypedSymbols += shift ? str : str.ToLower();
			}
			else if(IsBetween(i, Key.N0, Key.N9))
			{
				var n = i - (int)Key.N0;
				TypedSymbols += shift ? shiftNumbers[n] : ((char)('0' + n)).ToString();
			}
			else if(IsBetween(i, Key.Numpad0, Key.Numpad9))
			{
				var n = i - (int)Key.Numpad0;
				TypedSymbols += ((char)('0' + n)).ToString();
			}
			else if(symbols.ContainsKey(input))
				TypedSymbols += shift ? symbols[input].Item2 : symbols[input].Item1;

			return true;
		}

		#region Backend
		private readonly Dictionary<Key, (string, string)> symbols = new()
		{
			{ Key.BracketLeft, ("[", "{") }, { Key.BracketRight, ("]", "}") },
			{ Key.Semicolon, (";", ":") }, { Key.Comma, (",", "<") }, { Key.Dot, (".", ">") },
			{ Key.Quote, ("'", "\"") }, { Key.Slash, ("/", "?") }, { Key.Backslash, ("\\", "|") },
			{ Key.Tilde, ("`", "~") }, { Key.Equal, ("=", "+") }, { Key.Hyphen, ("-", "_") },
			{ Key.Space, (" ", " ") }, { Key.Enter, (Environment.NewLine, Environment.NewLine) },
			{ Key.Tab, ("\t", "") }, { Key.Add, ("+", "+") }, { Key.Minus, ("-", "-") },
			{ Key.Asterisk, ("*", "*") }, { Key.Divide, ("/", "/") }
		};
		private readonly string[] shiftNumbers = new string[10]
		{
			")", "!", "@", "#", "$", "%", "^", "&", "*", "("
		};

		private static bool IsBetween(int number, Key a, Key b)
		{
			return (int)a <= number && number <= (int)b;
		}
		#endregion
	}
}