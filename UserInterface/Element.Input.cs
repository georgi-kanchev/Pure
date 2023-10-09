namespace Pure.UserInterface;

public abstract partial class Element
{
    /// <summary>
    /// Represents user input for received by the user interface.
    /// </summary>
    protected class Input
    {
        /// <summary>
        /// The last input received by the user interface.
        /// </summary>
        public static Input Current
        {
            get;
        } = new();
        /// <summary>
        /// Indicates whether user input has been cancelled.
        /// </summary>
        public static bool IsCanceled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the input was pressed during the previous update.
        /// </summary>
        public bool WasPressed
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets a value indicating whether the input is currently pressed.
        /// </summary>
        public bool IsPressed
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets a value indicating whether the input has just been pressed.
        /// </summary>
        public bool IsJustPressed
        {
            get => WasPressed == false && IsPressed;
        }
        /// <summary>
        /// Gets a value indicating whether the input has just been released.
        /// </summary>
        public bool IsJustReleased
        {
            get => WasPressed && IsPressed == false;
        }
        /// <summary>
        /// Gets a value indicating whether the input has just been held.
        /// </summary>
        public bool IsJustHeld
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the current position of the input.
        /// </summary>
        public (float x, float y) Position
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets the previous position of the input.
        /// </summary>
        public (float x, float y) PositionPrevious
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets the most recent typed text.
        /// </summary>
        public string? Typed
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets the previous typed text.
        /// </summary>
        public string? TypedPrevious
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets the scroll delta of the input.
        /// </summary>
        public int ScrollDelta
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets an array of currently pressed keys.
        /// </summary>
        public Key[]? PressedKeys
        {
            get => pressedKeys.ToArray();
            internal set
            {
                pressedKeys.Clear();

                if (value != null && value.Length != 0)
                    pressedKeys.AddRange(value);
            }
        }

        /// <param name="key">
        /// The key to check.</param>
        /// <returns>True if the specified key is pressed, false otherwise.</returns>
        public bool IsKeyPressed(Key key)
        {
            return pressedKeys.Contains(key);
        }
        /// <param name="key">
        /// The key to check.</param>
        /// <returns>True if the specified key has just been 
        /// pressed, false otherwise.</returns>
        public bool IsKeyJustPressed(Key key)
        {
            return IsKeyPressed(key) && prevPressedKeys.Contains(key) == false;
        }
        /// <param name="key">
        /// The key to check.</param>
        /// <returns>True if the specified key has just been 
        /// released, false otherwise.</returns>
        public bool IsKeyJustReleased(Key key)
        {
            return IsKeyPressed(key) == false && prevPressedKeys.Contains(key);
        }

#region Backend
        internal readonly List<Key> pressedKeys = new(), prevPressedKeys = new();
#endregion
    }
}