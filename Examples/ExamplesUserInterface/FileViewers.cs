namespace Pure.Examples.ExamplesUserInterface;

public static class FileViewers
{
    public static Block[] Create(TilemapPack maps)
    {
        var folderViewer = new FileViewer((25, 2))
        {
            Text = "Select Folders:",
            Size = (22, 10),
            IsSelectingFolders = true
        };

        var fileViewer = new FileViewer((1, 2))
        {
            Text = "Select Files:",
            Size = (22, 10)
        };
        fileViewer.OnDisplay(() =>
        {
            DisplayExtra(maps, fileViewer);
            maps.SetFileViewer(fileViewer);
        });
        fileViewer.FilesAndFolders.OnItemDisplay(item =>
            maps.SetFileViewerItem(fileViewer, item));

        //================

        folderViewer.OnDisplay(() =>
        {
            DisplayExtra(maps, folderViewer);
            maps.SetFileViewer(folderViewer);
        });
        folderViewer.FilesAndFolders.OnItemDisplay(item =>
            maps.SetFileViewerItem(folderViewer, item));

        return new Block[] { fileViewer, folderViewer };
    }

    private static void DisplayExtra(TilemapPack maps, FileViewer fileViewer)
    {
        var e = fileViewer;
        var selected = e.SelectedPaths;
        var paths = "";

        foreach (var path in selected)
            paths += $"{Environment.NewLine}{Environment.NewLine}{path}";

        maps[0].SetTextLine((fileViewer.Position.x, fileViewer.Position.y - 1), fileViewer.Text);
        maps[0].SetTextRectangle(
            position: (e.Position.x, e.Position.y + e.Size.height - 1),
            size: (e.Size.width, 20),
            paths);
    }
}