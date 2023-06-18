using Pure.Window;

namespace Pure.EditorUserInterface;

using Pure.Tilemap;
using Pure.Tracker;
using Pure.UserInterface;
using Pure.Utilities;

public abstract class Menu : List
{
    protected Menu(params string[] options) :
        base((int.MaxValue, int.MaxValue), options.Length, Types.Dropdown)
    {
        Size = (15, 15);

        for (var i = 0; i < options.Length; i++)
            this[i].Text = options[i];
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();

        ItemMaximumSize = (Size.width - 1, 1);

        var scrollColor = Color.Gray;
        var middle = Program.tilemaps[(int)Program.Layer.EditMiddle];
        var front = Program.tilemaps[(int)Program.Layer.EditFront];
        middle.SetRectangle(Position, Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.66f)));
        front.SetTile(Scroll.Up.Position, new(Tile.ARROW, GetColor(Scroll.Up, scrollColor), 3));
        front.SetTile(Scroll.Slider.Handle.Position,
            new(Tile.SHAPE_CIRCLE, GetColor(Scroll, scrollColor)));
        front.SetTile(Scroll.Down.Position, new(Tile.ARROW, GetColor(Scroll.Down, scrollColor), 1));

        if (IsExpanded == false)
            Position = (int.MaxValue, int.MaxValue);
    }
    protected override void OnItemUpdate(Button item)
    {
        item.IsDisabled = item.Text.EndsWith(" ");

        var color = item.IsDisabled ? Color.Gray : Color.Gray.ToBright();
        var front = Program.tilemaps[(int)Program.Layer.EditFront];

        front.SetTextLine(item.Position, item.Text, GetColor(item, color));

        var (itemX, itemY) = item.Position;
        var dropdownTile = new Tile(Tile.MATH_GREATER, GetColor(item, color), 1);
        if (IsExpanded == false)
            front.SetTile((itemX + item.Size.width - 1, itemY), dropdownTile);
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