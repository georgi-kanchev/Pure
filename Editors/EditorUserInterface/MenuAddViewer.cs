namespace Pure.EditorUserInterface;

internal class MenuAddViewer : Menu
{
    public MenuAddViewer() : base(
        "Viewer… ",
        "  Files",
        "  Folders")
    {
        Size = (9, 3);

        OnItemInteraction(Interaction.Trigger, item =>
        {
            IsHidden = true;
            editUI.BlockCreate(10 + IndexOf(item), Position);
        });
    }
}