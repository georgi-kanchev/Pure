namespace Pure.EditorUserInterface;

internal class MenuAddList : Menu
{
    public MenuAddList() : base(
        $"{nameof(List)}� ",
        $"  {Spans.Vertical}",
        $"  {Spans.Horizontal}",
        $"  {Spans.Dropdown}")
    {
        Size = (12, 4);

        OnItemInteraction(Interaction.Trigger, item =>
        {
            IsHidden = true;
            editUI.BlockCreate(10, Position, (Spans)(IndexOf(item) - 1));
        });
    }
}