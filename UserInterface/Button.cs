namespace Pure.UserInterface
{
	public class Button : UserInterface
	{
		public Button((int, int) position, (int, int) size) : base(position, size) { }

		public bool IsTriggered(Action<Button>? custom)
		{
			if(Size.Item1 == 0 || Size.Item2 == 0)
				return false;

			var t = TryTrigger();

			custom?.Invoke(this);
			return t;
		}
	}
}
