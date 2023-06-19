using Pure.Utilities;
using Pure.Window;
using static Pure.EditorUserInterface.Program;

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
            $"  {nameof(Stepper)}",
            $"  {nameof(List)}")
    {
        Size = (10, 10);
    }

    protected override void OnItemTrigger(Button item)
    {
        IsHidden = true;
        editUI.ElementCreate(IndexOf(item), Position);
    }
}