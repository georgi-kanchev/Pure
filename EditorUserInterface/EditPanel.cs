using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;
using Pure.Window;
using static Pure.EditorUserInterface.Program;

namespace Pure.EditorUserInterface;

public class EditPanel : Panel
{
    public EditPanel((int x, int y) position) : base(position)
    {
        Text = "Edit";
    }

    protected override void OnUpdate()
    {
        Size = (10, TilemapSize.height);

        var onLmb = Mouse.IsButtonPressed(Mouse.Button.Left).Once("edit-panel-lmb");
        if (onLmb && IsHovered == false)
            Position = (int.MaxValue, int.MaxValue);

        var offset = (Size.width - Text.Length) / 2;
        offset = Math.Max(offset, 0);
        var textPos = (Position.x + offset, Position.y);
        const int corner = Tile.BORDER_HOLLOW_CORNER;
        const int straight = Tile.BORDER_HOLLOW_STRAIGHT;

        var front = tilemaps[(int)Layer.EditFront];
        front.SetBorder(Position, Size, corner, straight, Color.Yellow);
        front.SetTextLine(textPos, Text, Color.Yellow);

        var size = (Size.width - 2, 1);
        var (bottomX, bottomY) = (Position.x + 1, Position.y + Size.height - 2);
        var (topX, topY) = (Position.x + 1, Position.y + 1);
        UpdateButton(remove, (bottomX, bottomY), size);
        UpdateButton(toTop, (topX, topY), size);
    }

    #region Backend
    private class EditButton : Button
    {
        public EditButton((int x, int y) position) : base(position) { }

        protected override void OnUserAction(UserAction userAction)
        {
            if (userAction != UserAction.Trigger || Selected == null)
                return;

            if (Text == "Remove")
            {
                editUI.ElementRemove(Selected);
                editPanel.Position = (int.MaxValue, int.MaxValue);
            }
            else if (Text == "To Top")
                editUI.ElementToTop(Selected);
        }
    }

    private readonly EditButton remove = new((0, 0)) { Text = "Remove" },
        toTop = new((0, 0)) { Text = "To Top" },
        pin = new((0, 0)) { Text = "Pin" };

    private static void UpdateButton(Element btn, (int x, int y) position, (int w, int h) size)
    {
        var front = tilemaps[(int)Layer.EditFront];
        btn.Position = position;
        btn.Size = size;
        btn.Update();
        front.SetTextRectangle(btn.Position, btn.Size, btn.Text, GetColor(btn, Color.Yellow.ToDark()),
            alignment: Tilemap.Tilemap.Alignment.Center);
    }

    private static Color GetColor(Element element, Color baseColor)
    {
        if (element.IsPressedAndHeld) return baseColor.ToDark();
        else if (element.IsHovered) return baseColor.ToBright();

        return baseColor;
    }
    #endregion
}