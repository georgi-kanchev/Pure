using Pure.UserInterface;
using Pure.Utilities;

namespace Pure.EditorUserInterface;

using Pure.Window;

public class MenuMain : Menu
{
    public MenuMain()
        : base(
            "Element… ",
            "  Add",
            "  Edit",
            "-------- ",
            "Scene… ",
            "  Save",
            "  Load")
    {
        Size = (9, 7);
    }

    protected override void OnUpdate()
    {
        if (Mouse.IsButtonPressed(Mouse.Button.Right).Once("onRMB"))
        {
            var (x, y) = GetInputPosition();
            Position = ((int)x + 1, (int)y + 1);
            IsExpanded = true;
            Program.menus[Program.MenuType.Add].IsExpanded = false;
        }

        // disable edit if no elements are selected
        var hoveredStr = Program.Selected != null ? "" : " ";
        this[2].Text = "  Edit" + hoveredStr;

        base.OnUpdate();
    }
    protected override void OnItemTrigger(Button item)
    {
        var index = IndexOf(item);
        if (index == 1)
        {
            var menuAdd = Program.menus[Program.MenuType.Add];
            menuAdd.Position = Position;
            menuAdd.IsExpanded = true;
        }
        else if (index == 2 && Program.Selected != null)
        {
            var (x, _) = Program.Selected.Position;
            var (w, _) = Program.Selected.Size;
            var (tw, _) = TilemapSize;
            var pos = (x + w / 2 > tw / 2 ? 0 : tw - Program.editPanel.Size.width, 0);
            Program.editPanel.Position = pos;
        }
    }

    #region Backend
    private (float, float) GetInputPosition()
    {
        return Program.tilemaps[(int)Program.Layer.UiMiddle]
            .PointFrom(Mouse.CursorPosition, Window.Size);
    }
    private Element? GetHoveredElement()
    {
        var keys = Program.ui.Keys;
        for (var i = keys.Length - 1; i >= 0; i--)
        {
            if (Program.ui[keys[i]].IsOverlapping(GetInputPosition()))
                return Program.ui[keys[i]];
        }

        return null;
    }
    #endregion
}