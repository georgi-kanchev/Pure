namespace Pure.Editors.UserInterface;

internal class MenuAdd : Menu
{
    public MenuAdd() : base(
        editor,
        "Add… ",
        $" {nameof(Button)}",
        $" {nameof(InputBox)}",
        $" {nameof(Pages)}",
        $" {nameof(Panel)}",
        $" {nameof(Palette)}",
        $" {nameof(Slider)}",
        $" {nameof(Scroll)}",
        $" {nameof(Stepper)}",
        $" {nameof(Layout)}",
        $" {nameof(List)}",
        $" {nameof(FileViewer)}")
    {
        IsHidden = true;
        Size = (11, 12);

        OnItemInteraction(Interaction.Trigger, item =>
        {
            IsHidden = true;

            var index = Items.IndexOf(item);
            if (index == 10)
            {
                menus[MenuType.AddList].Show(Position);
                return;
            }

            BlockCreate(item.Text.Trim(), MenuMain.clickPositionWorld);
        });
    }
}