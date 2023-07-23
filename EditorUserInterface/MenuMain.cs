using Pure.UserInterface;
using Pure.Utilities;
using static Pure.EditorUserInterface.Program;

namespace Pure.EditorUserInterface;

using Window;

public class MenuMain : Menu
{
    public MenuMain()
        : base(
            "Element… ",
            "  Add",
            "-------- ",
            "Scene… ",
            "  Save",
            "  Load")
    {
        Size = (8, 6);
    }

    protected override void OnDisplay()
    {
        if (Mouse.IsButtonPressed(Mouse.Button.Right).Once("onRMB"))
        {
            var (x, y) = MousePosition;
            Position = ((int)x + 1, (int)y + 1);
            IsHidden = false;
            menus[MenuType.Add].IsHidden = true;
        }

        base.OnDisplay();
    }
    protected override void OnItemTrigger(Button item)
    {
        IsHidden = true;

        var index = IndexOf(item);
        if (index == 1)
        {
            var menuAdd = menus[MenuType.Add];
            menuAdd.Position = Position;
            menuAdd.IsHidden = false;
        }
        else if (index == 3)
        {
            // save
        }
        else if (index == 4)
        {
            // load
        }
    }
}