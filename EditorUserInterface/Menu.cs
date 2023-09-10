namespace Pure.EditorUserInterface;

using Tilemap;

using UserInterface;

using Utilities;

using Window;

using static Program;

public abstract class Menu : List
{
	protected Menu(params string[] options) :
		base((int.MaxValue, int.MaxValue), options.Length)
	{
		Size = (15, 15);
		ItemSize = (Size.width, 1);

		for(var i = 0; i < options.Length; i++)
			this[i].Text = options[i];
	}

	public void Show((int x, int y) position)
	{
		Position = position;
		IsHidden = false;

		if(Position.x + Size.width > TilemapSize.width)
			Position = (TilemapSize.width - Size.width, Position.y);
		if(Position.y + Size.height > TilemapSize.height)
			Position = (Position.x, TilemapSize.height - Size.height);
	}

	public override void Update()
	{
		IsDisabled = IsHidden;
		base.Update();
	}
	protected override void OnInput()
	{
		base.OnInput();

		if(Mouse.ScrollDelta != 0 || Mouse.IsButtonPressed(Mouse.Button.Middle))
			IsHidden = true;

		var key = "on-lmb-deselect" + GetHashCode();
		var onLmbRelease = (Mouse.IsButtonPressed(Mouse.Button.Left) == false).Once(key);
		if(onLmbRelease && IsHovered == false)
			IsHidden = true;
	}
	protected override void OnDisplay()
	{
		var scrollColor = Color.Gray;
		var middle = tilemaps[(int)Layer.EditMiddle];
		var front = tilemaps[(int)Layer.EditFront];

		SetClear(Layer.EditBack, this);
		SetClear(Layer.EditFront, this);

		middle.SetRectangle(Position, Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.66f)));

		if(Scroll.IsHidden)
			return;

		front.SetTile(Scroll.Increase.Position,
			new(Tile.ARROW, GetColor(Scroll.Increase, scrollColor), 3));
		front.SetTile(Scroll.Slider.Handle.Position,
			new(Tile.SHAPE_CIRCLE, GetColor(Scroll.Slider, scrollColor)));
		front.SetTile(Scroll.Decrease.Position,
			new(Tile.ARROW, GetColor(Scroll.Decrease, scrollColor), 1));
	}
	protected override void OnItemDisplay(Button item)
	{
		Disable(item, item.Text.EndsWith(" "));

		var color = item.IsDisabled ? Color.Gray : Color.Gray.ToBright();
		var front = tilemaps[(int)Layer.EditFront];

		front.SetTextLine(item.Position, item.Text, GetColor(item, color));
	}

	#region Backend
	private static Color GetColor(Element element, Color baseColor)
	{
		if(element.IsDisabled) return baseColor;
		else if(element.IsPressedAndHeld) return baseColor.ToDark();
		else if(element.IsHovered) return baseColor.ToBright();

		return baseColor;
	}
	private static void SetClear(Layer layer, Element element)
	{
		tilemaps[(int)layer].SetBox(element.Position, element.Size, 0, 0, 0, 0);
	}
	#endregion
}