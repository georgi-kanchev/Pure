namespace Pure.EditorUserInterface;

using Pure.Tilemap;
using Pure.Tracker;
using Pure.UserInterface;
using Pure.Utilities;
using Pure.Window;

public class RightClickMenu : List
{
	private enum Action
	{
		MenuExpand
	}
	private static readonly string[] rightClickMenuTexts = new string[]
	{
		"Add… ",
		$"  {nameof(Button)}",
		$"  {nameof(InputBox)}",
		$"  {nameof(Pages)}",
		$"  {nameof(Panel)}",
		$"  {nameof(Palette)}",
		$"  {nameof(Slider)}… ",
		"    Vertical",
		"    Horizontal",
		$"  {nameof(Pure.UserInterface.Scroll)}… ",
		"    Vertical",
		"    Horizontal",
		"    Numeric",
		$"  {nameof(List)}… ",
		"    Vertical",
		"    Horizontal",
		"    Dropdown",
	};

	private readonly Tilemap background, tilemap;
	private readonly RendererEdit edit;
	private readonly RendererUI ui;

	public RightClickMenu(Tilemap background, Tilemap tilemap, RendererUI ui, RendererEdit edit) :
		base((int.MaxValue, int.MaxValue), rightClickMenuTexts.Length, Types.Dropdown)
	{
		this.background = background;
		this.tilemap = tilemap;
		this.ui = ui;
		this.edit = edit;

		Tracker<Action>.When(Action.MenuExpand, () => Position = (int.MaxValue, int.MaxValue));
		Size = (15, 15);

		for (int i = 0; i < rightClickMenuTexts.Length; i++)
		{
			var item = this[i];
			if (item == null)
				continue;

			item.Text = rightClickMenuTexts[i];
		}
	}

	protected override void OnUpdate()
	{
		var (mouseX, mouseY) = tilemap.PointFrom(Mouse.CursorPosition, Window.Size);
		if (Mouse.IsButtonPressed(Mouse.Button.Right).Once("onRmb"))
		{
			Position = ((int)mouseX + 1, (int)mouseY + 1);
			IsExpanded = true;
		}
		base.OnUpdate();

		ItemMaximumSize = (Size.width - 1, 1);

		var scrollColor = Color.Gray;
		background.SetRectangle(Position, Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.66f)));
		tilemap.SetTile(Scroll.Up.Position, new(Tile.ARROW, GetColor(Scroll.Up, scrollColor), 3));
		tilemap.SetTile(Scroll.Slider.Handle.Position, new(Tile.SHAPE_CIRCLE, GetColor(Scroll, scrollColor)));
		tilemap.SetTile(Scroll.Down.Position, new(Tile.ARROW, GetColor(Scroll.Down, scrollColor), 1));

		Tracker<Action>.Track(Action.MenuExpand, IsExpanded == false);
	}
	protected override void OnItemUpdate(Button item)
	{
		item.IsDisabled = item.Text.EndsWith(" ");

		var color = item.IsDisabled ? Color.Gray : Color.Gray.ToBright();

		tilemap.SetTextLine(item.Position, item.Text, GetColor(item, color));

		var (itemX, itemY) = item.Position;
		var dropdownTile = new Tile(Tile.MATH_GREATER, GetColor(item, color), 1);
		if (IsExpanded == false)
			tilemap.SetTile((itemX + item.Size.width - 1, itemY), dropdownTile);
	}
	protected override void OnItemSelect(Button item)
	{
		edit.CreateElement(IndexOf(item), item.Position);
	}

	private static Color GetColor(Element element, Color baseColor)
	{
		if (element.IsDisabled) return baseColor;
		else if (element.IsPressedAndHeld) return baseColor.ToDark();
		else if (element.IsHovered) return baseColor.ToBright();

		return baseColor;
	}
}