namespace Pure.Editors.EditorUserInterface;

internal class MenuMain : Menu
{
    public MenuMain() : base(
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
                Selected = null;
                ui.Clear();
                editUI.Clear();
            }
            else if (index == 5)
                OpenPromptFolder();
            else if (index == 6)
                OpenPromptFile();
        });
    }

    public override void Update()
    {
        base.Update();

        if (Mouse.IsButtonPressed(Mouse.Button.Right).Once("onRMB") == false)
            return;

        var (x, y) = MousePosition;

        foreach (var kvp in menus)
            kvp.Value.IsHidden = true;

        Position = ((int)x + 1, (int)y + 1);
        IsHidden = false;
    }

#region Backend
    private static void OpenPromptFile()
    {
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

            Selected = null;
            editUI.Clear();
            ui.Clear();
            for (var i = 0; i < loadedUi.Count; i++)
            {
                var block = loadedUi[i];
                var bBytes = block.ToBytes();
                var span = Span.Vertical;

                if (block is List l)
                    span = l.Span;

                editUI.BlockCreate(block.GetType().Name, (0, 0), span, bBytes);
            }
        });
        var (camX, camY) = CameraPosition;
        saveLoad.Position = (saveLoad.Position.x + camX, saveLoad.Position.y + camY);
    }
    private static void OpenPromptFolder()
    {
        saveLoad.IsSelectingFolders = true;
        prompt.Text = "Save to directory:";
        prompt.Open(saveLoad, btnIndex =>
        {
            prompt.Close();

            if (btnIndex == 0)
                OpenPromptFileName();
        });
        var (camX, camY) = CameraPosition;
        saveLoad.Position = (saveLoad.Position.x + camX, saveLoad.Position.y + camY);
    }
    private static void OpenPromptFileName()
    {
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
        var (camX, camY) = CameraPosition;
        fileName.Position = (fileName.Position.x + camX, fileName.Position.y + camY);
    }
#endregion
}