namespace Pure.EditorUserInterface;

using Pure.Tilemap;
using Pure.Tracker;
using Pure.UserInterface;
using Pure.Utilities;
using Pure.Window;

public class Program
{
	class RightClickMenu : List
	{
		private readonly Tilemap tilemap;

		public RightClickMenu(Tilemap tilemap) : base((int.MaxValue, int.MaxValue), 10, Types.Dropdown) => this.tilemap = tilemap;

		protected override void OnUpdate()
		{
			base.OnUpdate();

			ItemMaximumSize = (Size.width - 1, 1);

			var scrollColor = Color.Gray.ToBright();
			tilemap.SetTile(ScrollUp.Position, new(Tile.ARROW, GetColor(ScrollUp, scrollColor), 3));
			tilemap.SetTile(Scroll.Handle.Position, new(Tile.SHAPE_CIRCLE, GetColor(Scroll, scrollColor)));
			tilemap.SetTile(ScrollDown.Position, new(Tile.ARROW, GetColor(ScrollDown, scrollColor), 1));
		}
		protected override void OnItemUpdate(Button item)
		{
			var color = Color.Gray;

			tilemap.SetTextLine(item.Position, item.Text, GetColor(item, color));

			var (itemX, itemY) = item.Position;
			var dropdownTile = new Tile(Tile.MATH_GREATER, GetColor(item, color), 1);
			if(IsExpanded == false)
				tilemap.SetTile((itemX + item.Size.width - 1, itemY), dropdownTile);
		}
	}

	private static void Main()
	{
		Window.Create(Window.Mode.Windowed);

		var aspectRatio = Window.MonitorAspectRatio;
		var back = new Tilemap((aspectRatio.width * 3, aspectRatio.height * 3));
		var tilemap = new Tilemap(back.Size);
		var front = new Tilemap(back.Size);
		var ui = new Tilemap(back.Size);
		var rightClickMenu = new RightClickMenu(ui);

		SetText(rightClickMenu, 0, "Add Button");

		Tracker<string>.When("on-right-click-menu-collapse", () => rightClickMenu.Position = (int.MaxValue, int.MaxValue));

		while(Window.IsOpen)
		{
			Window.Activate(true);

			ui.Fill();

			var mousePos = ui.PointFrom(Mouse.CursorPosition, Window.Size);

			if(Mouse.IsButtonPressed(Mouse.Button.Right).Once("onRmb"))
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

	private static Color GetColor(Element element, Color baseColor)
	{
		if(element.IsPressedAndHeld) return baseColor.ToDark();
		else if(element.IsHovered) return baseColor.ToBright();

		return baseColor;
	}
	private static void SetText(List list, int index, string text)
	{
		var item = list[index];
		if(item == null)
			return;

		item.Text = text;
	}
}