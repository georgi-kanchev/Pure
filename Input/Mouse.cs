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

		public static Button Pressed { get; private set; }
		public static Button JustPressed { get; private set; }
		public static Button JustReleased { get; private set; }

		public static void Update()
		{
			var btnCount = (int)SFML.Window.Mouse.Button.ButtonCount;

			JustPressed = Button.None;
			JustReleased = Button.None;
			prevPressed = Pressed;
			Pressed = Button.None;

			for(int i = 0; i < btnCount; i++)
			{
				var curBtn = (Button)Math.Pow(2, i);
				if(SFML.Window.Mouse.IsButtonPressed((SFML.Window.Mouse.Button)i))
					Pressed |= curBtn;

				var isPr = Pressed.HasFlag(curBtn);
				var wasPr = prevPressed.HasFlag(curBtn);

				if(wasPr && isPr == false)
					JustReleased |= curBtn;
				else if(wasPr == false && isPr)
					JustPressed |= curBtn;
			}
		}
		public static bool ArePressed(Button buttons) => Pressed.HasFlag(buttons);
		public static bool AreJustPressed(Button buttons) => JustPressed.HasFlag(buttons);
		public static bool AreJustReleased(Button buttons) => JustReleased.HasFlag(buttons);

		#region Backend
		private static Button prevPressed;
		#endregion
	}
}
