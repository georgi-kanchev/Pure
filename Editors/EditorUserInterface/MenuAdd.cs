namespace Pure.Editors.EditorUserInterface;

internal class MenuAdd : Menu
{
    public MenuAdd() : base(
        editor,
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
        $"  {nameof(FileViewer)}")
    {
        Size = (12, 12);

        OnItemInteraction(Interaction.Trigger, item =>
        {
            IsHidden = true;

            var index = IndexOf(item);
            if (index == 10)
            {
                menus[MenuType.AddList].Show(Position);
                return;
            }

            BlockCreate(item.Text.Trim(), MenuMain.clickPositionWorld);
        });
    }
}