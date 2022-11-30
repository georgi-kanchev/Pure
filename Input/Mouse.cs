namespace Purity.Input
{
	public enum Button { Left, Right, Middle, Extra1, Extra2 }

	public class Mouse : Device<Button>
	{
		protected override bool IsPressed(Button input)
		{
			var btn = (SFML.Window.Mouse.Button)input;
			return SFML.Window.Mouse.IsButtonPressed(btn);
		}
	}
}
