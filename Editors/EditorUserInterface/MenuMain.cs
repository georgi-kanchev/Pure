namespace Pure.Editors.EditorUserInterface;

internal class MenuMain : Menu
{
    public static (int x, int y) clickPositionWorld;

    public MenuMain() : base(
        editor,
        "Add… ",
        " Block",
        "Blocks… ",
        " New",
        " Save",
        " Load",
        " Copy",
        " Paste")
    {
        IsHidden = true;
        Size = (7, 8);

        OnItemInteraction(Interaction.Trigger, item =>
        {
            IsHidden = true;

            var index = Items.IndexOf(item);
            if (index == 1)
                menus[MenuType.Add].Show(Position);
            else if (index == 3)
                editor.PromptConfirm(() =>
                {
                    selected = null;
                    ui.Blocks.Clear();
                    panels.Blocks.Clear();
                });
            else if (index == 4)
                editor.PromptFileSave(ui.ToBytes());
            else if (index == 5)
                editor.PromptFileLoad(Load);
            else if (index == 6)
                Window.Clipboard = Convert.ToBase64String(ui.ToBytes());
            else if (index == 7)
                editor.PromptBase64(() => Load(Convert.FromBase64String(editor.PromptInput.Value)));
        });

        Mouse.Button.Right.OnPress(() =>
        {
            var (x, y) = editor.MousePositionUi;
            var (wx, wy) = editor.MousePositionWorld;
            clickPositionWorld = ((int)wx, (int)wy);

            foreach (var kvp in menus)
                kvp.Value.IsHidden = true;

            Position = ((int)x + 1, (int)y + 1);
            IsHidden = false;
        });
    }
    private static void Load(byte[] bytes)
    {
        var loadedUi = new BlockPack(bytes);

        selected = null;
        ui.Blocks.Clear();
        panels.Blocks.Clear();
        foreach (var block in loadedUi.Blocks)
        {
            var bBytes = block.ToBytes();
            var span = Span.Vertical;

            if (block is List l)
                span = l.Span;

            BlockCreate(block.GetType().Name, (0, 0), span, bBytes);
        }
    }
}