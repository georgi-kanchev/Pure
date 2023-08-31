namespace Pure.EditorUserInterface;

using UserInterface;
using Utilities;
using Window;
using static Program;

public class MenuMain : Menu
{
    public MenuMain() : base(
        "Element… ",
        "  Add",
        "-------- ",
        "Scene… ",
        "  Save",
        "  Load") =>
        Size = (8, 6);

    protected override void OnDisplay()
    {
        if (Mouse.IsButtonPressed(Mouse.Button.Right).Once("onRMB"))
        {
            var (x, y) = MousePosition;

            foreach (var kvp in menus)
                kvp.Value.IsHidden = true;

            Position = ((int)x + 1, (int)y + 1);
            IsHidden = false;
        }

        base.OnDisplay();
    }
    protected override void OnItemTrigger(Button item)
    {
        IsHidden = true;

        var index = IndexOf(item);
        if (index == 1)
            menus[MenuType.Add].Show(Position);
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