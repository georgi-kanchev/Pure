namespace Purity.Tools
{
	public static class UserInterface
	{
		public enum StateButton { Default, Hovered, Pressed, Clicked }

		public static void ProcessButton((uint, uint) indices, (int, int) size,
			(int, int) hoveredIndices, bool isPressed, Action<StateButton> method)
		{
			if(size.Item1 == 0 || size.Item2 == 0)
				return;

			var result = StateButton.Default;

			var wasPressed = ButtonWasPressed(indices, size);
			ButtonPress(indices, size, isPressed);

			var isReleased = isPressed == false && wasPressed;
			var key = GetKey(indices, size);
			var hx = hoveredIndices.Item1;
			var hy = hoveredIndices.Item2;
			var x = indices.Item1;
			var y = indices.Item2;
			var w = size.Item1;
			var h = size.Item2;
			var isHoveredX = hx >= x && hx < x + w;
			var isHoveredY = hy >= y && hy < y + h;
			var isClicked = buttonsClicked.ContainsKey(key) && buttonsClicked[key];

			if(w < 0)
				isHoveredX = hx > x + w && hx <= x;
			if(h < 0)
				isHoveredY = hy > y + h && hy <= y;

			var isHovered = isHoveredX && isHoveredY;

			if(isHovered && isReleased && isClicked)
				result = StateButton.Clicked;
			else if(isHovered && isPressed)
			{
				if(isClicked)
					result = StateButton.Pressed;

				if(wasPressed == false)
					buttonsClicked[key] = true;
			}
			else if(isHovered)
				result = StateButton.Hovered;

			if(isReleased)
				buttonsClicked[key] = false;

			method?.Invoke(result);
		}

		#region Backend
		private static readonly Dictionary<string, bool> buttonsWerePressed = new();
		private static readonly Dictionary<string, bool> buttonsClicked = new();

		private static string GetKey((uint, uint) indices, (int, int) size)
		{
			return $"{indices.Item1} {indices.Item2} {size.Item1} {size.Item2}";
		}
		private static void ButtonPress((uint, uint) indices, (int, int) size, bool isPressed)
		{
			var key = GetKey(indices, size);
			buttonsWerePressed[key] = isPressed;
		}
		private static bool ButtonWasPressed((uint, uint) indices, (int, int) size)
		{
			var key = GetKey(indices, size);
			return buttonsWerePressed.ContainsKey(key) && buttonsWerePressed[key];
		}
		#endregion
	}
}
