namespace Pure.EditorUserInterface;

using static Program;
using UserInterface;

public class MenuAddList : Menu
{
    public MenuAddList() : base(
        $"{nameof(List)}… ",
        $"  {Spans.Vertical}",
        $"  {Spans.Horizontal}",
        $"  {Spans.Dropdown}") =>
        Size = (12, 4);

    protected override void OnItemTrigger(Button item)
    {
        IsHidden = true;
        editUI.ElementCreate(10, Position, (Spans)(IndexOf(item) - 1));
    }
}