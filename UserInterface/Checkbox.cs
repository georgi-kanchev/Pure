namespace Pure.UserInterface;

public class Checkbox : Button
{
	public bool IsChecked { get; set; }

	public Checkbox((int, int) position) : base(position, (1, 1)) { }

	protected override void OnEvent(UserAction when)
	{
		if (when == UserAction.Trigger)
			IsChecked = IsChecked == false;
	}
}
