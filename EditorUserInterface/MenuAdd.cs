namespace Pure.EditorUserInterface;

using UserInterface;
using static Program;

public class MenuAdd : Menu
{
    public MenuAdd()
        : base(
            "Add… ",
            $"  {nameof(Button)}",
            $"  {nameof(InputBox)}",
            $"  {nameof(Pages)}",
            $"  {nameof(Panel)}",
            $"  {nameof(Palette)}",
            $"  {nameof(Slider)}",
            $"  {nameof(Pure.UserInterface.Scroll)}",
            $"  {nameof(Stepper)}",
            $"  {nameof(Layout)}",
            $"  {nameof(List)}",
            $"  {nameof(FileViewer)}",
            $"  FolderViewer")
    {
        Size = (13, 13);
    }

    protected override void OnItemTrigger(Button item)
    {
        IsHidden = true;

        var index = IndexOf(item);
        if (index == 10)
        {
            menus[MenuType.AddList].Show(Position);
            return;
        }

        editUI.ElementCreate(index, Position);
    }
}