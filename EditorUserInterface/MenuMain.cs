using Pure.UserInterface;
using Pure.Utilities;

namespace Pure.EditorUserInterface;

using Pure.Window;
using Pure.Tracker;

public class MenuMain : Menu
{
    public MenuMain(Tilemap.Tilemap background, Tilemap.Tilemap tilemap, MenuAdd menuAdd, RendererUI ui)
        : base(background, tilemap,
            "Element… ",
            "  Add",
            "  Edit",
            "  Pin Contained",
            "-------- ",
            "Scene… ",
            "  Save",
            "  Load")
    {
        this.menuAdd = menuAdd;
        this.ui = ui;

        Size = (16, 7);
    }

    protected override void OnUpdate()
    {
        if (Mouse.IsButtonPressed(Mouse.Button.Right).Once("onRMB"))
        {
            var (x, y) = GetInputPosition();
            Position = ((int)x + 1, (int)y + 1);
            IsExpanded = true;
            menuAdd.IsExpanded = false;

            var hoveredElement = GetHoveredElement();
            selectedElement = hoveredElement;

            // disable edit if not hovering any elements upon bringing the menu
            var hoveredStr = hoveredElement != null ? "" : " ";
            this[2].Text = "  Edit" + hoveredStr;
            this[3].Text = "  Pin Contained" + hoveredStr;
        }

        base.OnUpdate();
    }
    protected override void OnItemTrigger(Button item)
    {
        if (IndexOf(item) == 1)
        {
            menuAdd.Position = Position;
            menuAdd.IsExpanded = true;
        }
    }

    #region Backend
    private readonly MenuAdd menuAdd;
    private readonly RendererUI ui;
    private Element? selectedElement;

    private (float, float) GetInputPosition()
    {
        return tilemap.PointFrom(Mouse.CursorPosition, Window.Size);
    }
    private Element? GetHoveredElement()
    {
        var keys = ui.Keys;
        for (var i = keys.Length - 1; i >= 0; i--)
        {
            if (ui[keys[i]].IsOverlapping(GetInputPosition()))
                return ui[keys[i]];
        }

        return null;
    }
    #endregion
}