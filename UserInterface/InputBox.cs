namespace Purity.UserInterface
{
	public class InputBox : UserInterface
	{
		public string Text { get; set; } = "";

		public InputBox((int, int) position, (int, int) size, string text) : base(position, size)
		{
			Text = text;
		}

		public void Update(Action<InputBox>? result)
		{
			if(Size.Item1 == 0 || Size.Item2 == 0)
				return;

			TryTrigger();
			result?.Invoke(this);
		}
	}
}
