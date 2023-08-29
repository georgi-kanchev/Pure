namespace Pure.EditorUserInterface;

using static Program;
using UserInterface;

public class MenuAddList : Menu
{
    public MenuAddList() : base(
        $"{nameof(List)}… ",
        $"  {Types.Vertical}",
        $"  {Types.Horizontal}",
        $"  {Types.Dropdown}") =>
        Size = (12, 4);

    protected override void OnItemTrigger(Button item)
    {
        IsHidden = true;
        editUI.ElementCreate(10, Position, (Types)(IndexOf(item) - 1));
    }
}