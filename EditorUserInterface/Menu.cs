using Pure.Window;

namespace Pure.EditorUserInterface;

using Tilemap;
using UserInterface;
using Utilities;

public abstract class Menu : List
{
    protected Menu(params string[] options) :
        base((int.MaxValue, int.MaxValue), options.Length)
    {
        Size = (15, 15);

        for (var i = 0; i < options.Length; i++)
            this[i].Text = options[i];
    }

    protected override void OnDisplay()
    {
        if (Mouse.ScrollDelta != 0 || Mouse.IsButtonPressed(Mouse.Button.Middle))
            IsHidden = true;

        if (Position.x + Size.width > TilemapSize.width)
            Position = (TilemapSize.width - Size.width, Position.y);
        if (Position.y + Size.height > TilemapSize.height)
            Position = (Position.x, TilemapSize.height - Size.height);

        var key = "on-lmb-deselect" + GetHashCode();
        var onLmbRelease = (Mouse.IsButtonPressed(Mouse.Button.Left) == false).Once(key);
        if (onLmbRelease && IsHovered == false)
            IsHidden = true;

        ItemMaximumSize = (Size.width - 1, 1);

        var scrollColor = Color.Gray;
        var middle = Program.tilemaps[(int)Program.Layer.EditMiddle];
        var front = Program.tilemaps[(int)Program.Layer.EditFront];

        middle.SetRectangle(Position, Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.66f)));
        front.SetTile(Scroll.Increase.Position,
            new(Tile.ARROW, GetColor(Scroll.Increase, scrollColor), 3));
        front.SetTile(Scroll.Slider.Handle.Position,
            new(Tile.SHAPE_CIRCLE, GetColor(Scroll.Slider, scrollColor)));
        front.SetTile(Scroll.Decrease.Position,
            new(Tile.ARROW, GetColor(Scroll.Decrease, scrollColor), 1));

        if (IsHidden)
            Position = (int.MaxValue, int.MaxValue);
    }
    protected override void OnItemDisplay(Button item)
    {
        if (IsHidden)
            return;

        item.IsDisabled = item.Text.EndsWith(" ");

        var color = item.IsDisabled ? Color.Gray : Color.Gray.ToBright();
        var front = Program.tilemaps[(int)Program.Layer.EditFront];

        front.SetTextLine(item.Position, item.Text, GetColor(item, color));
    }

#region Backend
    private static Color GetColor(Element element, Color baseColor)
    {
        if (element.IsDisabled) return baseColor;
        else if (element.IsPressedAndHeld) return baseColor.ToDark();
        else if (element.IsHovered) return baseColor.ToBright();

        return baseColor;
    }
#endregion
}