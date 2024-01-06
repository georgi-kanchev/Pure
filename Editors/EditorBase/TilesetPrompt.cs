namespace Pure.Editors.EditorBase;

internal class TilesetPrompt
{
    public TilesetPrompt(Editor editor)
    {
        this.editor = editor;

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
            Size = (21, 10),
            FileFilter = ".png"
        };
        fileViewer.OnDisplay(() => maps.SetFileViewer(fileViewer, BACK));
        fileViewer.FilesAndFolders.OnItemDisplay(btn =>
            maps.SetFileViewerItem(fileViewer, btn, MIDDLE));
        fileViewer.HardDrives.OnItemDisplay(btn =>
            maps.SetFileViewerItem(fileViewer, btn, MIDDLE));
        fileViewer.FilesAndFolders.OnItemInteraction(Interaction.DoubleTrigger, btn =>
        {
            if (fileViewer.IsFolder(btn) == false)
                PromptTilesetAccept();
        });

        Keyboard.Key.Enter.OnPress(() =>
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

    public Action<Layer, Tilemap>? OnSuccess { get; set; }
    public Action? OnFail { get; set; }

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
    private readonly Layer layer = new((1, 1));
    private Tilemap? map;
    private readonly Editor editor;
    private readonly InputBox pair;
    private readonly FileViewer fileViewer;
    private readonly Stepper stepper;

    private void PromptTilesetAccept()
    {
        editor.Prompt.Close();
        var paths = fileViewer.SelectedPaths;
        if (paths.Length == 0)
        {
            editor.PromptMessage("Could not load image!");
            return;
        }

        var path = fileViewer.SelectedPaths[0];
        editor.LayerGrid.TilesetPath = path;
        editor.LayerMap.TilesetPath = path;
        layer.TilesetPath = path;

        if (editor.LayerMap.TilesetPath == "default")
        {
            editor.LayerGrid.ResetToDefaults();
            editor.LayerMap.ResetToDefaults();
            OnFail?.Invoke();
            editor.PromptMessage("Could not load image!");
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
            editor.PromptMessage("Only 2 values allowed!");
            return;
        }

        var result = ((int)split[0].ToNumber(), (int)split[1].ToNumber());
        editor.LayerGrid.TileSize = result;
        editor.LayerMap.TileSize = result;
        layer.TileSize = result;
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
            editor.PromptMessage("Only 2 values allowed!");
            return;
        }

        var result = ((int)split[0].ToNumber(), (int)split[1].ToNumber());
        editor.LayerGrid.TileGap = result;
        editor.LayerMap.TileGap = result;
        layer.TileGap = result;

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
        var result = (int)stepper.Value;

        editor.Prompt.Close();
        editor.LayerGrid.TileIdFull = result;
        editor.LayerMap.TileIdFull = result;
        layer.TileIdFull = result;

        var (tw, th) = layer.TileSize;
        var ratio = MathF.Max(tw / 8f, th / 8f);
        var zoom = 3.8f / ratio;
        layer.Zoom = zoom;
        map = new(layer.TilesetSize) { ViewSize = (10, 10) };

        editor.SetGrid();

        OnSuccess?.Invoke(layer, map);
    }
#endregion
}