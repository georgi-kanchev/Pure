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
        stepper.OnDisplay += () => maps.SetStepper(stepper, BACK);

        fileViewer = new()
        {
            FilesAndFolders = { IsSingleSelecting = true },
            Size = (21, 16),
            FileFilter = ".png"
        };
        fileViewer.OnDisplay += () => maps.SetFileViewer(fileViewer, BACK);
        fileViewer.FilesAndFolders.OnItemDisplay += btn =>
            maps.SetFileViewerItem(fileViewer, btn, MIDDLE);
        fileViewer.HardDrives.OnItemDisplay += btn =>
            maps.SetFileViewerItem(fileViewer, btn, MIDDLE);
        fileViewer.FilesAndFolders.OnItemInteraction(Interaction.DoubleTrigger, btn =>
        {
            if (fileViewer.IsFolder(btn) == false)
                PromptTilesetAccept();
        });
    }

    public Action<LayerTiles, TileMap>? OnSuccess { get; set; }
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
    private readonly LayerTiles layerTiles = new((1, 1));
    private TileMap? map;
    private readonly Editor editor;
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
        editor.LayerTiles.AtlasPath = path;
        editor.LayerTilesMap.AtlasPath = path;
        layerTiles.AtlasPath = path;

        if (editor.LayerTilesMap.AtlasPath == "default")
        {
            editor.LayerTiles.ToDefault();
            editor.LayerTilesMap.ToDefault();
            OnFail?.Invoke();
            editor.PromptMessage("Could not load image!");
            return;
        }

        PromptTileSize();
    }
    private void PromptTileSize()
    {
        editor.Prompt.Text = "";
        stepper.Text = "Provide Tile Size";
        editor.Prompt.Open(stepper, onButtonTrigger: i =>
        {
            if (i == 0)
                PromptTileSizeAccept();
        });
    }
    private void PromptTileSizeAccept()
    {
        var result = (byte)stepper.Value;
        editor.LayerTiles.AtlasTileSize = result;
        editor.LayerTilesMap.AtlasTileSize = result;
        layerTiles.AtlasTileSize = result;
        PromptTileGap();
    }
    private void PromptTileGap()
    {
        editor.Prompt.Text = "";
        stepper.Text = "Provide Tile Gap";
        editor.Prompt.Open(stepper, onButtonTrigger: i =>
        {
            if (i == 0)
                PromptTileGapAccept();
        });
    }
    private void PromptTileGapAccept()
    {
        var result = (byte)stepper.Value;
        editor.LayerTiles.AtlasTileGap = result;
        editor.LayerTilesMap.AtlasTileGap = result;
        layerTiles.AtlasTileGap = result;

        PromptTileFull();
    }
    private void PromptTileFull()
    {
        editor.Prompt.Text = "";
        stepper.Text = "Provide Full Tile Id";
        editor.Prompt.Open(stepper, onButtonTrigger: i =>
        {
            if (i == 0)
                PromptTileFullAccept();
        });
    }
    private void PromptTileFullAccept()
    {
        var result = (ushort)stepper.Value;

        editor.LayerTiles.AtlasTileIdFull = result;
        editor.LayerTilesMap.AtlasTileIdFull = result;
        layerTiles.AtlasTileIdFull = result;

        var pixels = 10 * layerTiles.AtlasTileSize;
        var scaleFactor = (float)pixels / 80; // 10x10 map size * 8x8 tile size
        var newZoomLevel = 3.8f / scaleFactor;

        layerTiles.Zoom = newZoomLevel;
        map = new(layerTiles.AtlasTileCount) { View = (0, 0, 10, 10) };

        editor.SetGrid();

        OnSuccess?.Invoke(layerTiles, map);
    }
#endregion
}