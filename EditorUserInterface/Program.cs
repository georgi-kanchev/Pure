namespace Pure.EditorUserInterface;

using Pure.Tilemap;
using Pure.Tracker;
using Pure.UserInterface;
using Pure.Utilities;
using Pure.Window;

public partial class Program
{
	enum Layer
	{
		Scene_Back, Scene_Middle, Scene_Front,
		UI_Back, UI_Middle, UI_Front,
		Count
	}

	private static void Main()
	{
		Window.Create(Window.Mode.Windowed);

		var aspectRatio = Window.MonitorAspectRatio;
		var tilemaps = new TilemapManager((int)Layer.Count, (aspectRatio.width * 3, aspectRatio.height * 3));

		const int ON_MENU_EXPAND = 0;
		var rightClickMenuTexts = new string[]
		{
			"Add ",
			"  Button",
			"  Input Box",
			"  Pages",
			"  Panel",
			"  Color Palette",
			"  Slider ",
			"    Vertical",
			"    Horizontal",
			"  Scroll ",
			"    Vertical",
			"    Horizontal",
			"    Numeric",
			"  List ",
			"    Vertical",
			"    Horizontal",
			"    Dropdown",
		};
		var rightClickMenu = new RightClickMenu(
			tilemaps[(int)Layer.UI_Back],
			tilemaps[(int)Layer.UI_Middle], rightClickMenuTexts.Length)
		{ Size = (16, 12) };

		for (int i = 0; i < rightClickMenuTexts.Length; i++)
			SetText(rightClickMenu, i, rightClickMenuTexts[i]);

		Tracker<int>.When(ON_MENU_EXPAND, () => rightClickMenu.Position = (int.MaxValue, int.MaxValue));

		while (Window.IsOpen)
		{
			Window.Activate(true);

			tilemaps.Fill();

			var mousePos = tilemaps.PointFrom(Mouse.CursorPosition, Window.Size);

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
				tilemaps.Size);

			rightClickMenu.Update();
			Tracker<int>.Track(ON_MENU_EXPAND, rightClickMenu.IsExpanded == false);

			Mouse.CursorGraphics = (Mouse.Cursor)Element.MouseCursorResult;

			for (int i = 0; i < tilemaps.Count; i++)
				Window.DrawTiles(tilemaps[i].ToBundle());

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