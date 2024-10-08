namespace Pure.Editors.Base;

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
            SymbolGroup = SymbolGroup.Decimals | SymbolGroup.Space,
            Value = string.Empty
        };
        pair.OnDisplay(() => maps.SetInputBox(pair, BACK));

        fileViewer = new()
        {
            FilesAndFolders = { IsSingleSelecting = true },
            Size = (21, 16),
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
    }

    public Action<Layer, Tilemap>? OnSuccess { get; set; }
    public Action? OnFail { get; set; }

    public void Open()
    {
        editor.Prompt.Text = "Select Image File:";
        editor.Prompt.Open(fileViewer, onButtonTrigger: i =>
        {
            if (i == 0)
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
        var paths = fileViewer.SelectedPaths;
        if (paths.Length == 0)
        {
            editor.PromptMessage("Could not load image!");
            return;
        }

        var path = fileViewer.SelectedPaths[0];
        editor.LayerGrid.AtlasPath = path;
        editor.LayerMap.AtlasPath = path;
        layer.AtlasPath = path;

        if (editor.LayerMap.AtlasPath == "default")
        {
            editor.LayerGrid.ToDefault();
            editor.LayerMap.ToDefault();
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
        editor.Prompt.Open(pair, onButtonTrigger: i =>
        {
            if (i == 0)
                PromptTileSizeAccept();
        });
    }
    private void PromptTileSizeAccept()
    {
        var split = pair.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 2)
        {
            editor.PromptMessage("Only 2 values allowed!");
            return;
        }

        var result = ((byte)split[0].ToNumber(), (byte)split[1].ToNumber());
        editor.LayerGrid.AtlasTileSize = result;
        editor.LayerMap.AtlasTileSize = result;
        layer.AtlasTileSize = result;
        PromptTileGap();
    }
    private void PromptTileGap()
    {
        editor.Prompt.Text = $"Enter Tile Gap{Environment.NewLine}" +
                             $"example: '1 1'";
        editor.Prompt.Open(pair, onButtonTrigger: i =>
        {
            if (i == 0)
                PromptTileGapAccept();
        });
    }
    private void PromptTileGapAccept()
    {
        var split = pair.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 2)
        {
            editor.PromptMessage("Only 2 values allowed!");
            return;
        }

        var result = ((byte)split[0].ToNumber(), (byte)split[1].ToNumber());
        editor.LayerGrid.AtlasTileGap = result;
        editor.LayerMap.AtlasTileGap = result;
        layer.AtlasTileGap = result;

        PromptTileFull();
    }
    private void PromptTileFull()
    {
        editor.Prompt.Text = "Provide Full Tile Id";
        editor.Prompt.Open(stepper, onButtonTrigger: i =>
        {
            if (i == 0)
                PromptTileFullAccept();
        });
    }
    private void PromptTileFullAccept()
    {
        var result = (int)stepper.Value;

        editor.LayerGrid.AtlasTileIdFull = result;
        editor.LayerMap.AtlasTileIdFull = result;
        layer.AtlasTileIdFull = result;

        var (tw, th) = layer.AtlasTileSize;
        var ratio = MathF.Max(tw / 8f, th / 8f);
        var zoom = 3.8f / ratio;
        layer.Zoom = zoom;
        map = new(layer.AtlasTileCount) { View = (0, 0, 10, 10) };

        editor.SetGrid();

        OnSuccess?.Invoke(layer, map);
    }
#endregion
}