namespace Pure.Editors.EditorMap;

internal class TilesetPrompt
{
    public TilesetPrompt(Editor editor, TilePalette tilePalette)
    {
        this.editor = editor;
        this.tilePalette = tilePalette;

        const int BACK = (int)Editor.LayerMapsUi.PromptBack;
        const int MIDDLE = (int)Editor.LayerMapsUi.PromptMiddle;
        var maps = editor.MapsUi;

        stepper = new() { Range = (0, int.MaxValue), Size = (22, 2) };
        stepper.OnDisplay(() => maps.SetStepper(stepper, BACK));

        pair = new()
        {
            Size = (20, 1),
            SymbolGroup = SymbolGroup.Digits | SymbolGroup.Space,
            Value = "",
            IsSingleLine = true
        };
        pair.OnDisplay(() => maps.SetInputBox(pair, BACK));

        fileViewer = new()
        {
            FilesAndFolders = { IsSingleSelecting = true },
            Size = (21, 10)
        };
        fileViewer.OnDisplay(() => maps.SetFileViewer(fileViewer, BACK));
        fileViewer.FilesAndFolders.OnItemDisplay(btn =>
            maps.SetFileViewerItem(fileViewer, btn, MIDDLE));

        Keyboard.OnKeyPress(Keyboard.Key.Enter, asText =>
        {
            if (editor.Prompt.IsHidden)
                return;

            var text = editor.Prompt.Text;
            if (text.Contains("Image File"))
                PromptTilesetAccept();
            else if (text.Contains("Tile Size"))
                PromptTileSizeAccept();
            else if (text.Contains("Tile Gap"))
                PromptTileGapAccept();
            else if (text.Contains("Full Tile Id"))
                PromptTileFullAccept();
        });
    }

    public void Open()
    {
        editor.Prompt.Text = "Select Image File:";
        editor.Prompt.Open(fileViewer, i =>
        {
            editor.Prompt.Close();

            if (i != 0)
                return;

            PromptTilesetAccept();
        });
    }

#region Backend
    private readonly Editor editor;
    private readonly TilePalette tilePalette;
    private readonly InputBox pair;
    private readonly FileViewer fileViewer;
    private readonly Stepper stepper;

    private void PromptTilesetAccept()
    {
        editor.Prompt.Close();
        var paths = fileViewer.SelectedPaths;
        if (paths.Length == 0)
        {
            Program.PromptMessage("Could not load image!");
            return;
        }

        var path = fileViewer.SelectedPaths[0];
        editor.LayerMap.TilesetPath = path;
        tilePalette.layer.TilesetPath = path;

        if (editor.LayerMap.TilesetPath == "default")
        {
            tilePalette.Create(tilePalette.layer.TilesetSize);
            Program.PromptMessage("Could not load image!");
            return;
        }

        PromptTileSize();
    }
    private void PromptTileSize()
    {
        editor.Prompt.Text = $"Enter Tile Size{Environment.NewLine}" +
                             $"example: '16 16'";
        editor.Prompt.Open(pair, i =>
        {
            editor.Prompt.Close();

            if (i != 0)
                return;

            PromptTileSizeAccept();
        });
    }
    private void PromptTileSizeAccept()
    {
        editor.Prompt.Close();
        var split = pair.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 2)
        {
            Program.PromptMessage("Only 2 values allowed!");
            return;
        }

        var result = ((int)split[0].ToNumber(), (int)split[1].ToNumber());
        editor.LayerMap.TileSize = result;
        tilePalette.layer.TileSize = result;
        PromptTileGap();
    }
    private void PromptTileGap()
    {
        editor.Prompt.Text = $"Enter Tile Gap{Environment.NewLine}" +
                             $"example: '1 1'";
        editor.Prompt.Open(pair, i =>
        {
            editor.Prompt.Close();

            if (i != 0)
                return;

            PromptTileGapAccept();
        });
    }
    private void PromptTileGapAccept()
    {
        editor.Prompt.Close();
        var split = pair.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 2)
        {
            Program.PromptMessage("Only 2 values allowed!");
            return;
        }

        var result = ((int)split[0].ToNumber(), (int)split[1].ToNumber());
        editor.LayerMap.TileGap = result;
        tilePalette.layer.TileGap = result;

        tilePalette.Create(tilePalette.layer.TilesetSize);

        PromptTileFull();
    }
    private void PromptTileFull()
    {
        editor.Prompt.Text = "Provide Full Tile Id";
        editor.Prompt.Open(stepper, i =>
        {
            editor.Prompt.Close();

            if (i != 0)
                return;

            PromptTileFullAccept();
        });
    }
    private void PromptTileFullAccept()
    {
        editor.Prompt.Close();
        editor.LayerMap.TileIdFull = (int)stepper.Value;
        tilePalette.layer.TileIdFull = (int)stepper.Value;
    }
#endregion
}