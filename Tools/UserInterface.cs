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
			var clicked = (0, 0);

			if(isPressed == false && wasPressed && pressedCell == hoveredIndices) // on click
				clicked = hoveredIndices;
			if(isPressed && wasPressed == false) // on press
				pressedCell = hoveredIndices;

			var xStep = (uint)(size.Item1 < 0 ? -1 : 1);
			var yStep = (uint)(size.Item2 < 0 ? -1 : 1);
			for(uint x = indices.Item1; x != indices.Item1 + size.Item1; x += xStep)
				for(uint y = indices.Item2; y != indices.Item2 + size.Item2; y += yStep)
				{
					var curIndices = (x, y);
					if(clicked == curIndices)
					{
						method?.Invoke(StateButton.Clicked);
						wasPressed = isPressed;
						return;
					}
					if(hoveredIndices == curIndices)
					{
						var isValidPress = isPressed && pressedCell == curIndices;
						method?.Invoke(isValidPress ? StateButton.Pressed : StateButton.Hovered);
						wasPressed = isPressed;
						return;
					}
				}

			method?.Invoke(result);
			wasPressed = isPressed;
		}

		#region Backend
		private static bool wasPressed;
		private static (int, int) pressedCell = (int.MaxValue, int.MaxValue);
		#endregion
	}
}
