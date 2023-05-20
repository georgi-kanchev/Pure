namespace Pure.EditorUserInterface;

using Pure.Tilemap;
using Pure.Tracker;
using Pure.UserInterface;
using Pure.Utilities;
using Pure.Window;

public partial class Program
{

	private static void Main()
	{
		Window.Create(Window.Mode.Windowed);

		var aspectRatio = Window.MonitorAspectRatio;
		var back = new Tilemap((aspectRatio.width * 3, aspectRatio.height * 3));
		var tilemap = new Tilemap(back.Size);
		var front = new Tilemap(back.Size);
		var ui = new Tilemap(back.Size);

		var rightClickMenuTexts = new string[]
		{
			"Add-", "-Button", "-Slider", "-Scroll", "-Scroll Numeric", "-Input", "-Pages", "-Panel",
		  	"-List Dropdown", "-List Vertical", "-List Horizontal", "-Color Palette"
		};
		var rightClickMenu = new RightClickMenu(ui, rightClickMenuTexts.Length) { Size = (17, 8) };

		for (int i = 0; i < rightClickMenuTexts.Length; i++)
			SetText(rightClickMenu, i, rightClickMenuTexts[i]);

		Tracker<string>.When("on-right-click-menu-collapse", () => rightClickMenu.Position = (int.MaxValue, int.MaxValue));

		while (Window.IsOpen)
		{
			Window.Activate(true);

			ui.Fill();

			var mousePos = ui.PointFrom(Mouse.CursorPosition, Window.Size);

			if (Mouse.IsButtonPressed(Mouse.Button.Right).Once("onRmb"))
			{
				rightClickMenu.Position = ((int)mousePos.x, (int)mousePos.y);
				rightClickMenu.IsExpanded = true;
			}
			Element.ApplyInput(
				Mouse.IsButtonPressed(Mouse.Button.Left),
				mousePos,
				Mouse.ScrollDelta,
				Keyboard.KeyIDsPressed,
				Keyboard.KeyTyped,
				back.Size);

			rightClickMenu.Update();
			Tracker<string>.Track("on-right-click-menu-collapse", rightClickMenu.IsExpanded == false);

			Mouse.CursorGraphics = (Mouse.Cursor)Element.MouseCursorResult;
			Window.DrawTiles(ui.ToBundle());

			Window.Activate(false);
		}
	}

	private static void SetText(List list, int index, string text)
	{
		var item = list[index];
		if (item == null)
			return;

		item.Text = text;
	}
}