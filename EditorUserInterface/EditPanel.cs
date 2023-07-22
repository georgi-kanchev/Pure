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
        IsHidden = true;
        IsResizable = false;
        IsMovable = false;
    }

    protected override void OnDisplay()
    {
        var onLmb = Mouse.IsButtonPressed(Mouse.Button.Left).Once("edit-panel-lmb");
        if (onLmb && IsHovered == false && IsHidden == false)
            IsHidden = true;

        if (Selected == null)
            return;

        var x = Selected.Position.x + Selected.Size.width / 2;
        var cx = CameraPosition.x + CameraSize.w / 2;

        Size = (10, CameraSize.h);
        Position = (x > cx ? CameraPosition.x : CameraPosition.x + CameraSize.w - Size.width,
            CameraPosition.y);

        if (IsHidden)
            return;

        var offset = (Size.width - Text.Length) / 2;
        offset = Math.Max(offset, 0);
        var textPos = (Position.x + offset, Position.y);
        const int CORNER = Tile.BOX_HOLLOW_CORNER;
        const int STRAIGHT = Tile.BOX_HOLLOW_STRAIGHT;

        var middle = tilemaps[(int)Layer.EditMiddle];
        var front = tilemaps[(int)Layer.EditFront];
        middle.SetRectangle(Position, Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.66f)));
        front.SetRectangle(Position, Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.66f)));
        front.SetBox(Position, Size, Tile.SHADE_TRANSPARENT, CORNER, STRAIGHT, Color.Yellow);
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

            var name = Selected.GetType().Name;
            if (Text == "Remove")
            {
                editUI.ElementRemove(Selected);
                editPanel.Position = (int.MaxValue, int.MaxValue);

                DisplayInfoText(name + " removed");
            }
            else if (Text == "To Top")
            {
                editUI.ElementToTop(Selected);
                DisplayInfoText(name + " surfaced");
            }
        }
    }

    private readonly EditButton remove = new((0, 0)) { Text = "Remove" },
        toTop = new((0, 0)) { Text = "To Top" };

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