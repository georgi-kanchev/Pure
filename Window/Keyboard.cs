using SFML.Window;

namespace Pure.Window;

/// <summary>
/// Handles keyboard input.
/// </summary>
public static class Keyboard
{
    /// <summary>
    /// The common keyboard keys.
    /// </summary>
    public enum Key
    {
        Unknown = -1,
        A = 00,
        B = 01,
        C = 02,
        D = 03,
        E = 04,
        F = 05,
        G = 06,
        H = 07,
        I = 08,
        J = 09,
        K = 10,
        L = 11,
        M = 12,
        N = 13,
        O = 14,
        P = 15,
        Q = 16,
        R = 17,
        S = 18,
        T = 19,
        U = 20,
        V = 21,
        W = 22,
        X = 23,
        Y = 24,
        Z = 25,
        Number0 = 26,
        Number1 = 27,
        Number2 = 28,
        Number3 = 29,
        Number4 = 30,
        Number5 = 31,
        Number6 = 32,
        Number7 = 33,
        Number8 = 34,
        Number9 = 35,
        Escape = 36,
        ControlLeft = 37,
        ShiftLeft = 38,
        AltLeft = 39,
        SystemLeft = 40,
        ControlRight = 41,
        ShiftRight = 42,
        AltRight = 43,
        SystemRight = 44,
        Menu = 45,
        BracketLeft = 46,
        BracketRight = 47,
        Semicolon = 48,
        Comma = 49,
        Dot = 50,
        Period = 50,
        Quote = 51,
        Slash = 52,
        Backslash = 53,
        Tilde = 54,
        Equal = 55,
        Hyphen = 56,
        Dash = 56,
        Space = 57,
        Enter = 58,
        Return = 58,
        Backspace = 59,
        Tab = 60,
        PageUp = 61,
        PageDown = 62,
        End = 63,
        Home = 64,
        Insert = 65,
        Delete = 66,
        Add = 67,
        Plus = 67,
        Subtract = 68,
        Minus = 68,
        Asterisk = 69,
        Multiply = 69,
        Divide = 70,
        ArrowLeft = 71,
        ArrowRight = 72,
        ArrowUp = 73,
        ArrowDown = 74,
        Numpad0 = 75,
        Numpad1 = 76,
        Numpad2 = 77,
        Numpad3 = 78,
        Numpad4 = 79,
        Numpad5 = 80,
        Numpad6 = 81,
        Numpad7 = 82,
        Numpad8 = 83,
        Numpad9 = 84,
        F1 = 85,
        F2 = 86,
        F3 = 87,
        F4 = 88,
        F5 = 89,
        F6 = 90,
        F7 = 91,
        F8 = 92,
        F9 = 93,
        F10 = 94,
        F11 = 95,
        F12 = 96,
        F13 = 97,
        F14 = 98,
        F15 = 99,
        Pause = 100
    }

    /// <summary>
    /// Gets the text representation of the latest key typed by the user.
    /// </summary>
    public static string KeyTyped
    {
        get;
        internal set;
    }
    /// <summary>
    /// Gets an array of currently pressed keys.
    /// </summary>
    public static Key[] KeysPressed
    {
        get => pressed.ToArray();
    }
    /// <summary>
    /// Gets an array of currently pressed key identifiers.
    /// </summary>
    public static int[] KeyIDsPressed
    {
        get
        {
            var pressed = KeysPressed;
            var result = new int[pressed.Length];
            for (var i = 0; i < pressed.Length; i++)
                result[i] = (int)pressed[i];

            return result;
        }
    }
    /// <param name="key">
    /// The key to check.</param>
    /// <returns>True if the key is currently pressed, otherwise false.</returns>
    public static bool IsKeyPressed(Key key)
    {
        return pressed.Contains(key);
    }

    public static void CancelInput()
    {
        pressed.Clear();
        KeyTyped = "";
    }

    public static void OnKeyPress(Key key, Action<string> method)
    {
        if (pressedCallbacks.TryAdd(key, method) == false)
            pressedCallbacks[key] += method;
    }
    public static void OnKeyRelease(Key key, Action method)
    {
        if (releasedCallbacks.TryAdd(key, method) == false)
            releasedCallbacks[key] += method;
    }

#region Backend
    private static readonly Dictionary<Key, Action<string>> pressedCallbacks = new();
    private static readonly Dictionary<Key, Action> releasedCallbacks = new();
    private static readonly List<Key> pressed;
    private static readonly Dictionary<Key, (string, string)> symbols;
    private static readonly string[] shiftNumbers;
    static Keyboard()
    {
        KeyTyped = "";
        pressed = new();
        symbols = new()
        {
            { Key.BracketLeft, ("[", "{") }, { Key.BracketRight, ("]", "}") },
            { Key.Semicolon, (";", ":") }, { Key.Comma, (",", "<") }, { Key.Dot, (".", ">") },
            { Key.Quote, ("'", "\"") }, { Key.Slash, ("/", "?") }, { Key.Backslash, ("\\", "|") },
            { Key.Tilde, ("`", "~") }, { Key.Equal, ("=", "+") }, { Key.Hyphen, ("-", "_") },
            { Key.Space, (" ", " ") }, { Key.Enter, ("\n", "\n") },
            { Key.Tab, ("\t", "") }, { Key.Add, ("+", "+") }, { Key.Minus, ("-", "-") },
            { Key.Asterisk, ("*", "*") }, { Key.Divide, ("/", "/") }
        };
        shiftNumbers = new[]
        {
            ")", "!", "@", "#", "$", "%", "^", "&", "*", "("
        };
    }

    private static bool IsBetween(int number, int a, int b)
    {
        return a <= number && number <= b;
    }
    private static string GetSymbol(Key input, bool shift)
    {
        var i = input;

        if (IsBetween((int)i, (int)Key.A, (int)Key.Z))
        {
            var str = ((char)('A' + i)).ToString();
            return shift ? str : str.ToLower();
        }
        else if (IsBetween((int)i, (int)Key.Number0, (int)Key.Number9))
        {
            var n = i - Key.Number0;
            return shift ? shiftNumbers[n] : ((char)('0' + n)).ToString();
        }
        else if (IsBetween((int)i, (int)Key.Numpad0, (int)Key.Numpad9))
        {
            var n = i - Key.Numpad0;
            return ((char)('0' + n)).ToString();
        }
        else if (symbols.ContainsKey(input))
            return shift ? symbols[input].Item2 : symbols[input].Item1;

        return "";
    }

    internal static void OnKeyPress(object? s, KeyEventArgs e)
    {
        var key = (Key)e.Code;

        var onPress = pressed.Contains(key) == false;
        if (onPress)
            pressed.Add(key);

        var symbol = GetSymbol(key, IsKeyPressed(Key.ShiftLeft) || IsKeyPressed(Key.ShiftRight));
        if (KeyTyped.Contains(symbol) == false)
            KeyTyped += symbol;

        if (pressedCallbacks.TryGetValue(key, out var callback))
            callback.Invoke(symbol);
    }
    internal static void OnKeyRelease(object? s, KeyEventArgs e)
    {
        var key = (Key)e.Code;

        pressed.Remove(key);

        if (releasedCallbacks.TryGetValue(key, out var callback))
            callback.Invoke();

        if (pressed.Count == 0)
        {
            KeyTyped = "";
            return;
        }

        // shift released while holding special symbol, just like removing
        // lowercase and uppercase, shift + 1 = !, so releasing shift would
        // never removes the !
        if (key is Key.ShiftLeft or Key.ShiftRight && KeyTyped != "")
            foreach (var k in pressed)
                // get symbol as if shift was pressed
                KeyTyped = KeyTyped.Replace(GetSymbol(k, true), "");

        if (KeyTyped.Length == 0)
            return;

        var symbol = GetSymbol(key, IsKeyPressed(Key.ShiftLeft) || IsKeyPressed(Key.ShiftRight));
        if (symbol == "")
            return;

        KeyTyped = KeyTyped.Replace(symbol.ToLower(), "");
        KeyTyped = KeyTyped.Replace(symbol.ToUpper(), "");
    }
#endregion
}