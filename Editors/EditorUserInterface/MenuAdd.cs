namespace Pure.Editors.EditorUserInterface;

internal class MenuAdd : Menu
{
    public MenuAdd() : base(
        "Add… ",
        $"  {nameof(Button)}",
        $"  {nameof(InputBox)}",
        $"  {nameof(Pages)}",
        $"  {nameof(Panel)}",
        $"  {nameof(Palette)}",
        $"  {nameof(Slider)}",
        $"  {nameof(Engine.UserInterface.Scroll)}",
        $"  {nameof(Stepper)}",
        $"  {nameof(Layout)}",
        $"  {nameof(List)}",
        $"  Viewer")
    {
        Size = (10, 12);

        OnItemInteraction(Interaction.Trigger, item =>
        {
            IsHidden = true;

            var index = IndexOf(item);
            if (index == 10)
            {
                menus[MenuType.AddList].Show(Position);
                return;
            }
            else if (index == 11)
            {
                menus[MenuType.AddViewer].Show(Position);
                return;
            }

            editUI.BlockCreate(index, Position);
        });
    }
}