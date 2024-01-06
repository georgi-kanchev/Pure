namespace Pure.Editors.EditorUserInterface;

internal class MenuMain : Menu
{
    public static (int x, int y) clickPositionWorld;

    public MenuMain() : base(
        editor,
        "Block… ",
        "  Add",
        "------ ",
        "Scene… ",
        "  New",
        "  Save",
        "  Load")
    {
        Size = (6, 7);

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
                OpenPromptFolder();
            else if (index == 6)
                OpenPromptFile();
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

#region Backend
    private static void OpenPromptFile()
    {
        var prompt = editor.Prompt;
        saveLoad.IsSelectingFolders = false;
        prompt.Text = "Load from file:";
        prompt.Open(saveLoad, btnIndex =>
        {
            prompt.Close();

            if (btnIndex != 0)
                return;

            var file = saveLoad.SelectedPaths[0];
            var bytes = File.ReadAllBytes(file);
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
    }
    private static void OpenPromptFolder()
    {
        var prompt = editor.Prompt;
        saveLoad.IsSelectingFolders = true;
        prompt.Text = "Save to directory:";
        prompt.Open(saveLoad, btnIndex =>
        {
            prompt.Close();

            if (btnIndex == 0)
                OpenPromptFileName();
        });
    }
    private static void OpenPromptFileName()
    {
        var prompt = editor.Prompt;
        var directory = saveLoad.SelectedPaths.Length == 0 ?
            saveLoad.CurrentDirectory :
            saveLoad.SelectedPaths[0];
        prompt.Text = "File name:";
        prompt.Open(fileName, btnIndex =>
        {
            prompt.Close();

            if (btnIndex != 0)
                return;

            var bytes = ui.ToBytes();
            File.WriteAllBytes(Path.Join(directory, fileName.Value), bytes);
        });
    }
#endregion
}