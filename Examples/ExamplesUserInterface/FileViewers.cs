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
        fileViewer.OnDisplay(() => SetFileViewer(maps, fileViewer, zOrder: 0));
        fileViewer.FilesAndFolders.OnItemDisplay(item =>
            SetFileViewerItem(maps, item, fileViewer, zOrder: 1));

        //================

        folderViewer.OnDisplay(() => SetFileViewer(maps, folderViewer, zOrder: 0));
        folderViewer.FilesAndFolders.OnItemDisplay(item =>
            SetFileViewerItem(maps, item, folderViewer, zOrder: 1));

        return new Block[] { fileViewer, folderViewer };
    }
}