namespace Pure.UserInterface
{
	public class Scroll : Slider
	{
		public Scroll((int, int) position, int size = 5, bool isVertical = true)
			: base(position, size, isVertical) { }
	}
}
