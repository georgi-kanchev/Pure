namespace Purity.Input
{
	public enum Key
	{
#pragma warning disable CA1069 // Enums values should not be duplicated
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
#pragma warning restore CA1069 // Enums values should not be duplicated
	}

	public class Keyboard : Device<Key>
	{
		protected override bool IsPressed(Key input)
		{
			var key = (SFML.Window.Keyboard.Key)input;
			return SFML.Window.Keyboard.IsKeyPressed(key);
		}
	}
}