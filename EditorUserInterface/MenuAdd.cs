namespace Pure.EditorUserInterface;

using UserInterface;

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
            $"  {nameof(List)}")
    {
        Size = (10, 9);
    }

    protected override void OnItemTrigger(Button item)
    {
        Program.editUI.ElementCreate(IndexOf(item), Position);
    }

    #region Backend
    #endregion
}