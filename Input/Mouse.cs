namespace Purity.Input
{
	/// <summary>
	/// The physical buttons on a mouse.
	/// </summary>
	public enum Button { Left, Right, Middle, Extra1, Extra2 }

	/// <summary>
	/// (Inherits <see cref="Device{T}"/>)<br></br><br></br>
	/// 
	/// Handles input from a physical mouse.
	/// </summary>
	public class Mouse : Device<Button>
	{
		/// <summary>
		/// The cursor position on the screen.
		/// </summary>
		public static (int, int) Position
		{
			get { var pos = SFML.Window.Mouse.GetPosition(); return (pos.X, pos.Y); }
			set => SFML.Window.Mouse.SetPosition(new(value.Item1, value.Item2));
		}

		protected override bool IsPressedRaw(Button input)
		{
			var btn = (SFML.Window.Mouse.Button)input;
			return SFML.Window.Mouse.IsButtonPressed(btn);
		}
	}
}
