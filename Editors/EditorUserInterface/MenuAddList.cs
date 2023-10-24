namespace Pure.Editors.EditorUserInterface;

internal class MenuAddList : Menu
{
    public MenuAddList() : base(
        editor,
        $"{nameof(List)}… ",
        $"  {Span.Vertical}",
        $"  {Span.Horizontal}",
        $"  {Span.Dropdown}")
    {
        Size = (12, 4);

        OnItemInteraction(Interaction.Trigger, item =>
        {
            IsHidden = true;
            BlockCreate(nameof(List), MenuMain.clickPositionWorld, (Span)(IndexOf(item) - 1));
        });
    }
}