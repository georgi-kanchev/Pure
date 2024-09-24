namespace Pure.Editors.EditorUserInterface;

internal class MenuAddList : Menu
{
    public MenuAddList() : base(
        editor,
        $"{nameof(List)}… ",
        $" {Span.Vertical}",
        $" {Span.Horizontal}",
        $" {Span.Dropdown}")
    {
        IsHidden = true;
        Size = (11, 4);

        OnItemInteraction(Interaction.Trigger, item =>
        {
            IsHidden = true;
            BlockCreate(nameof(List), MenuMain.clickPositionWorld, (Span)(Items.IndexOf(item) - 1));
        });
    }
}