namespace Pure.UserInterface
{
	public class Input
	{
		public bool IsPressed { get; set; }
		public bool IsReleased => IsPressed == false && wasPressed;
		public bool IsJustPressed => wasPressed == false && IsPressed;
		public bool IsJustReleased => wasPressed && IsPressed == false;

		public (float, float) Position { get; set; }
		public string? TypedSymbols { get; set; }

		public int[]? PressedKeys
		{
			get => pressedKeys.ToArray();
			set
			{
				pressedKeys.Clear();

				if(value != null && value.Length != 0)
					pressedKeys.AddRange(value);
			}
		}

		public bool IsKeyPressed(int key)
		{
			return pressedKeys.Contains(key);
		}
		public bool IsKeyJustPressed(int key)
		{
			return IsKeyPressed(key) && prevPressedKeys.Contains(key) == false;
		}
		public bool IsKeyJustReleased(int key)
		{
			return IsKeyPressed(key) == false && prevPressedKeys.Contains(key);
		}

		#region Backend
		internal readonly List<int> pressedKeys = new(), prevPressedKeys = new();
		internal bool wasPressed;

		internal (float, float) prevPosition;
		internal string? prevTypedSymbols;
		#endregion
	}
}
