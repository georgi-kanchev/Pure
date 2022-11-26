namespace Purity.Tools
{
	public static class UserInterface
	{
		public enum StateButton { Default, Hovered, Pressed, Clicked }
		public enum StateInputBox { Default, Focused, Hovered, Clicked, Changed }

		public static void ProcessButton((int, int) indices, (int, int) size,
			(int, int) hoveredIndices, bool hasInput, Action<StateButton> method)
		{
			if(size.Item1 == 0 || size.Item2 == 0)
				return;

			Interact(indices, size, hoveredIndices, hasInput,
				out var resultIsHovered,
				out var resultIsClicked,
				out var resultIsPressed);

			if(resultIsHovered)
				method?.Invoke(StateButton.Hovered);
			else if(resultIsClicked)
				method?.Invoke(StateButton.Clicked);
			else if(resultIsPressed)
				method?.Invoke(StateButton.Pressed);
			else
				method?.Invoke(StateButton.Default);
		}
		public static void ProcessInputBox((int, int) indices, (int, int) size,
			(int, int) hoveredIndices, bool hasInput, char symbol, Action<StateInputBox> method)
		{
			if(size.Item1 == 0 || size.Item2 == 0)
				return;

			var key = GetKey(indices, size);
			Interact(indices, size, hoveredIndices, hasInput,
				out var resultIsHovered,
				out var resultIsClicked,
				out var resultIsPressed);

			if(resultIsClicked)
			{
				method?.Invoke(StateInputBox.Clicked);

				if(inputBoxFocused != key)
					method?.Invoke(StateInputBox.Focused);

				inputBoxFocused = key;
			}
		}

		#region Backend
		private static string? inputBoxFocused;
		private static readonly Dictionary<string, bool> rectsWerePressed = new();
		private static readonly Dictionary<string, bool> rectClicked = new();

		private static string GetKey((int, int) indices, (int, int) size)
		{
			return $"{indices.Item1} {indices.Item2} {size.Item1} {size.Item2}";
		}
		private static void ButtonPress((int, int) indices, (int, int) size, bool hasInput)
		{
			var key = GetKey(indices, size);
			rectsWerePressed[key] = hasInput;
		}
		private static bool ButtonWasPressed((int, int) indices, (int, int) size)
		{
			var key = GetKey(indices, size);
			return rectsWerePressed.ContainsKey(key) && rectsWerePressed[key];
		}
		private static void Interact((int, int) indices, (int, int) size,
			(int, int) hoveredIndices, bool hasInput,
			out bool isHovered, out bool isClicked, out bool isPressed)
		{
			isHovered = false;
			isClicked = false;
			isPressed = false;

			var wasPressed = ButtonWasPressed(indices, size);
			ButtonPress(indices, size, hasInput);

			var isReleased = hasInput == false && wasPressed;
			var key = GetKey(indices, size);
			var _isClicked = rectClicked.ContainsKey(key) && rectClicked[key];
			var hx = hoveredIndices.Item1;
			var hy = hoveredIndices.Item2;
			var x = indices.Item1;
			var y = indices.Item2;
			var w = size.Item1;
			var h = size.Item2;
			var isHoveredX = hx >= x && hx < x + w;
			var isHoveredY = hy >= y && hy < y + h;
			if(w < 0)
				isHoveredX = hx > x + w && hx <= x;
			if(h < 0)
				isHoveredY = hy > y + h && hy <= y;

			var _isHovered = isHoveredX && isHoveredY;

			if(_isHovered && isReleased && _isClicked)
				isClicked = true;
			else if(_isHovered && hasInput)
			{
				if(_isClicked)
					isPressed = true;

				if(wasPressed == false)
					rectClicked[key] = true;
			}
			else if(_isHovered)
				isHovered = true;

			if(isReleased)
				rectClicked[key] = false;
		}
		#endregion
	}
}
