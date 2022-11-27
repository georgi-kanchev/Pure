namespace Purity.Input
{
	public static class Mouse
	{
		[Flags]
		public enum Button
		{
			None = 0,
			Left = 1,
			Right = 2,
			Middle = 4,
			Extra1 = 8,
			Extra2 = 16
		}

		public static Button ButtonsPressed { get; private set; }

		public static void Update()
		{
			ButtonsPressed = Button.None;

			var btnCount = (int)SFML.Window.Mouse.Button.ButtonCount;
			for(int i = 0; i < btnCount; i++)
				if(SFML.Window.Mouse.IsButtonPressed((SFML.Window.Mouse.Button)i))
					ButtonsPressed = Add(ButtonsPressed, (Button)Math.Pow(2, i));
		}

		#region Backend
		private static Button Add(Button a, Button b) => a | b;
		#endregion
	}
}
