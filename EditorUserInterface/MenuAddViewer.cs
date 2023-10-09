namespace Pure.EditorUserInterface;

using static Program;
using Pure.UserInterface;

public class MenuAddViewer : Menu
{
    public MenuAddViewer()
        : base(
            "Viewer… ",
            "  Files",
            "  Folders")
    {
        Size = (9, 3);
    }

    protected override void OnItemTrigger(Button item)
    {
        IsHidden = true;
        editUI.ElementCreate(10 + IndexOf(item), Position);
    }
}