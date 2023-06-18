using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;
using Pure.Window;

namespace Pure.EditorUserInterface;

public class EditPanel : Panel
{
    public EditPanel((int x, int y) position, TilemapManager tilemaps) : base(position)
    {
        Text = "Edit Element";
    }

    protected override void OnUpdate()
    {
        var onLmb = Mouse.IsButtonPressed(Mouse.Button.Left).Once("edit-panel-lmb");
        if (onLmb && IsHovered == false)
            Position = (int.MaxValue, int.MaxValue);

        var offset = (Size.width - Text.Length) / 2;
        offset = Math.Max(offset, 0);
        var textPos = (Position.x + offset, Position.y);
        const int corner = Tile.BORDER_GRID_CORNER;
        const int straight = Tile.BORDER_GRID_STRAIGHT;

        var back = Program.tilemaps[(int)Program.Layer.EditFront];
        back.SetBorder(Position, Size, corner, straight, Color.Cyan);
        back.SetTextLine(textPos, Text, Color.Cyan);
    }

    #region Backend
    #endregion
}