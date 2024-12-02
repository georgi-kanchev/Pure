namespace Pure.Engine.Window;

/// <summary>
/// Handles keyboard input.
/// </summary>
[DoNotSave]
public static class Keyboard
{
    /// <summary>
    /// The common keyboard keys.
    /// </summary>
    public enum Key
    {
        Unknown = -1,
        A = 00, B = 01, C = 02, D = 03, E = 04, F = 05, G = 06, H = 07, I = 08, J = 09, K = 10, L = 11,
        M = 12, N = 13, O = 14, P = 15, Q = 16, R = 17, S = 18, T = 19, U = 20, V = 21, W = 22, X = 23,
        Y = 24, Z = 25,
        Number0 = 26, Number1 = 27, Number2 = 28, Number3 = 29, Number4 = 30, Number5 = 31,
        Number6 = 32, Number7 = 33, Number8 = 34, Number9 = 35,
        Escape = 36, ControlLeft = 37, ShiftLeft = 38, AltLeft = 39, SystemLeft = 40, ControlRight = 41,
        ShiftRight = 42, AltRight = 43, SystemRight = 44, Menu = 45, BracketLeft = 46, BracketRight = 47,
        Semicolon = 48, Comma = 49, Dot = 50, Period = 50, Quote = 51, Slash = 52, Backslash = 53,
        Tilde = 54, Equal = 55, Hyphen = 56, Dash = 56, Space = 57, Enter = 58, Return = 58,
        Backspace = 59, Tab = 60, PageUp = 61, PageDown = 62, End = 63, Home = 64, Insert = 65,
        Delete = 66, Add = 67, Plus = 67, Subtract = 68, Minus = 68, Asterisk = 69, Multiply = 69,
        Divide = 70, ArrowLeft = 71, ArrowRight = 72, ArrowUp = 73, ArrowDown = 74,
        Numpad0 = 75, Numpad1 = 76, Numpad2 = 77, Numpad3 = 78, Numpad4 = 79, Numpad5 = 80, Numpad6 = 81,
        Numpad7 = 82, Numpad8 = 83, Numpad9 = 84,
        F1 = 85, F2 = 86, F3 = 87, F4 = 88, F5 = 89, F6 = 90, F7 = 91, F8 = 92, F9 = 93,
        F10 = 94, F11 = 95, F12 = 96, F13 = 97, F14 = 98, F15 = 99,
        Pause = 100
    }

    /// <summary>
    /// Gets the text representation of the latest key typed by the user.
    /// </summary>
    public static string KeyTyped { get; internal set; } = "";
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
    public static int[] KeyIdsPressed
    {
        get
        {
            var press = pressed;
            var result = new int[press.Count];
            for (var i = 0; i < press.Count; i++)
                result[i] = (int)press[i];

            return result;
        }
    }

    public static void SimulatePress(this Key key)
    {
        simulatedPresses.Add(key);

        if (pressed.Contains(key) == false)
            OnPress(null, new(new() { Code = (SFML.Window.Keyboard.Key)key }));
    }
    public static void CancelInput()
    {
        pressed.Clear();
        KeyTyped = string.Empty;
    }
    public static string ToText(this Key key, bool shift)
    {
        var i = key;

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
        else if (symbols.ContainsKey(key))
            return shift ? symbols[key].Item2 : symbols[key].Item1;

        return string.Empty;
    }

    /// <param name="key">
    /// The key to check.</param>
    /// <returns>True if the key is currently pressed, otherwise false.</returns>
    public static bool IsPressed(this Key key)
    {
        return pressed.Contains(key);
    }
    public static bool IsJustPressed(this Key key)
    {
        return IsPressed(key) && prevPressed.Contains(key) == false;
    }
    public static bool IsJustReleased(this Key key)
    {
        return IsPressed(key) == false && prevPressed.Contains(key);
    }
    public static bool IsJustPressedAndHeld(this Key key)
    {
        return IsJustPressed(key) || (IsPressed(key) && isJustHeld);
    }

    public static bool IsAnyPressed()
    {
        return pressed.Count > 0;
    }
    public static bool IsAnyJustPressed()
    {
        return pressed.Count > prevPressed.Count;
    }
    public static bool IsAnyJustReleased()
    {
        return pressed.Count < prevPressed.Count;
    }
    public static bool IsAnyJustPressedAndHeld()
    {
        return IsAnyJustPressed() || isJustHeld;
    }

    public static void OnPress(this Key key, Action method)
    {
        if (onPress.TryAdd(key, method) == false)
            onPress[key] += method;
    }
    public static void OnRelease(this Key key, Action method)
    {
        if (onRelease.TryAdd(key, method) == false)
            onRelease[key] += method;
    }
    public static void OnPressAndHold(this Key key, Action method)
    {
        if (onHold.TryAdd(key, method) == false)
            onHold[key] += method;
    }

    public static void OnPressAny(Action<Key> method)
    {
        onPressAny += method;
    }
    public static void OnReleaseAny(Action<Key> method)
    {
        onReleaseAny += method;
    }
    public static void OnPressAndHoldAny(Action<Key> method)
    {
        onHoldAny += method;
    }

#region Backend
    private static readonly List<Key> simulatedPresses = [], prevSimulatedPressed = [];

    private static Action<Key>? onPressAny, onReleaseAny, onHoldAny;
    private static readonly Dictionary<Key, Action> onPress = new(), onRelease = new(), onHold = new();
    private static readonly List<Key> pressed = [], prevPressed = [];
    private static readonly Dictionary<Key, (string, string)> symbols = new()
    {
        { Key.BracketLeft, ("[", "{") }, { Key.BracketRight, ("]", "}") },
        { Key.Semicolon, (";", ":") }, { Key.Comma, (",", "<") }, { Key.Dot, (".", ">") },
        { Key.Quote, ("'", "\"") }, { Key.Slash, ("/", "?") }, { Key.Backslash, ("\\", "|") },
        { Key.Tilde, ("`", "~") }, { Key.Equal, ("=", "+") }, { Key.Hyphen, ("-", "_") },
        { Key.Space, (" ", " ") }, { Key.Enter, ("\n", "\n") },
        { Key.Tab, ("\t", string.Empty) }, { Key.Add, ("+", "+") }, { Key.Minus, ("-", "-") },
        { Key.Asterisk, ("*", "*") }, { Key.Divide, ("/", "/") }
    };
    private static readonly string[] shiftNumbers = [")", "!", "@", "#", "$", "%", "^", "&", "*", "("];

    private const float HOLD_DELAY = 0.5f, HOLD_INTERVAL = 0.1f;
    private static readonly Stopwatch hold = new(), holdTrigger = new();
    private static bool isJustHeld;

    internal static void Update()
    {
        if (IsAnyJustPressed())
            hold.Restart();

        isJustHeld = false;
        if (hold.Elapsed.TotalSeconds > HOLD_DELAY &&
            holdTrigger.Elapsed.TotalSeconds > HOLD_INTERVAL)
        {
            holdTrigger.Restart();
            isJustHeld = true;
        }

        if (IsAnyJustPressedAndHeld())
        {
            onHoldAny?.Invoke(pressed[^1]);

            foreach (var key in pressed)
                if (IsJustPressedAndHeld(key) && onHold.TryGetValue(key, out var callback))
                    callback.Invoke();
        }

        prevPressed.Clear();
        prevPressed.AddRange(pressed);

        foreach (var key in prevSimulatedPressed)
            if (simulatedPresses.Contains(key) == false)
                OnRelease(null, new(new() { Code = (SFML.Window.Keyboard.Key)key }));

        prevSimulatedPressed.Clear();
        prevSimulatedPressed.AddRange(simulatedPresses);
        simulatedPresses.Clear();
    }

    private static bool IsBetween(int number, int a, int b)
    {
        return a <= number && number <= b;
    }

    internal static void OnPress(object? s, KeyEventArgs e)
    {
        var key = (Key)e.Code;

        hold.Restart();
        holdTrigger.Restart();

        var press = pressed.Contains(key) == false;
        if (press)
            pressed.Add(key);

        var symbol = key.ToText(IsPressed(Key.ShiftLeft) || IsPressed(Key.ShiftRight));
        if (KeyTyped.Contains(symbol) == false)
            KeyTyped += symbol;

        if (onPress.TryGetValue(key, out var callback))
            callback.Invoke();

        onPressAny?.Invoke(key);
    }
    internal static void OnRelease(object? s, KeyEventArgs e)
    {
        var key = (Key)e.Code;

        pressed.Remove(key);

        if (onRelease.TryGetValue(key, out var callback))
            callback.Invoke();

        onReleaseAny?.Invoke(key);

        if (pressed.Count == 0)
        {
            KeyTyped = string.Empty;
            return;
        }

        // shift released while holding special symbol, just like removing
        // lowercase and uppercase, shift + 1 = !, so releasing shift would
        // never removes the !
        if (key is Key.ShiftLeft or Key.ShiftRight && KeyTyped != string.Empty)
            foreach (var k in pressed)
                KeyTyped = KeyTyped.Replace(k.ToText(true), string.Empty);
        // get symbol as if shift was pressed

        if (KeyTyped.Length == 0)
            return;

        var symbol = key.ToText(IsPressed(Key.ShiftLeft) || IsPressed(Key.ShiftRight));
        if (symbol == string.Empty)
            return;

        KeyTyped = KeyTyped.Replace(symbol.ToLower(), string.Empty);
        KeyTyped = KeyTyped.Replace(symbol.ToUpper(), string.Empty);
    }
    internal static void OnType(object? s, TextEventArgs e)
    {
        // a hack for caps lock support
        if (e.Unicode[0] is >= 'A' and <= 'Z')
            KeyTyped = KeyTyped.ToUpper();
    }
#endregion
}