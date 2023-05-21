namespace Pure.EditorUserInterface;

using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;

public class RightClickMenu : List
{
	private readonly Tilemap background, tilemap;

	public RightClickMenu(Tilemap background, Tilemap tilemap, int count) :
		base((int.MaxValue, int.MaxValue), count, Types.Dropdown)
	{
		this.background = background;
		this.tilemap = tilemap;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		ItemMaximumSize = (Size.width - 1, 1);

		var scrollColor = Color.Gray;
		background.SetRectangle(Position, Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.66f)));
		tilemap.SetTile(Scroll.Up.Position, new(Tile.ARROW, GetColor(Scroll.Up, scrollColor), 3));
		tilemap.SetTile(Scroll.Slider.Handle.Position, new(Tile.SHAPE_CIRCLE, GetColor(Scroll, scrollColor)));
		tilemap.SetTile(Scroll.Down.Position, new(Tile.ARROW, GetColor(Scroll.Down, scrollColor), 1));
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

	private static Color GetColor(Element element, Color baseColor)
	{
		if (element.IsDisabled) return baseColor;
		else if (element.IsPressedAndHeld) return baseColor.ToDark();
		else if (element.IsHovered) return baseColor.ToBright();

		return baseColor;
	}
}