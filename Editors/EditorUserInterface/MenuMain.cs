namespace Pure.Editors.EditorUserInterface;

internal class MenuMain : Menu
{
    public static (int x, int y) clickPositionWorld;

    public MenuMain() : base(
        editor,
        "Block… ",
        "  Add",
        "------ ",
        "Blocks… ",
        "  New",
        "  Save",
        "  Load")
    {
        IsHidden = true;
        Size = (7, 7);

        OnItemInteraction(Interaction.Trigger, item =>
        {
            IsHidden = true;

            var index = IndexOf(item);
            if (index == 1)
                menus[MenuType.Add].Show(Position);
            else if (index == 4)
            {
                selected = null;
                ui.Clear();
                panels.Clear();
            }
            else if (index == 5)
                editor.PromptFileSave(ui.ToBytes());
            else if (index == 6)
                editor.PromptFileLoad(bytes =>
                {
                    var loadedUi = new BlockPack(bytes);

                    selected = null;
                    ui.Clear();
                    panels.Clear();
                    for (var i = 0; i < loadedUi.Count; i++)
                    {
                        var block = loadedUi[i];
                        var bBytes = block.ToBytes();
                        var span = Span.Vertical;

                        if (block is List l)
                            span = l.Span;

                        BlockCreate(block.GetType().Name, (0, 0), span, bBytes);
                    }
                });
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
}