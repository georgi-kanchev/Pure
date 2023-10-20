namespace Pure.Editors.EditorUserInterface;

internal class MenuAddList : Menu
{
    public MenuAddList() : base(
        $"{nameof(List)}… ",
        $"  {Span.Vertical}",
        $"  {Span.Horizontal}",
        $"  {Span.Dropdown}")
    {
        Size = (12, 4);

        OnItemInteraction(Interaction.Trigger, item =>
        {
            IsHidden = true;
            editUI.BlockCreate(nameof(List), Position, (Span)(IndexOf(item) - 1));
        });
    }
}