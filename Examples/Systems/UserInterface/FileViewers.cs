namespace Pure.Examples.Systems.UserInterface;

using Pure.UserInterface;
using Utilities;
using Tilemap;
using static Utility;

public static class FileViewers
{
    public static Element[] Create(TilemapManager maps)
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
        fileViewer.OnDisplay(() => DisplayFileViewer(maps, fileViewer, zOrder: 0));
        fileViewer.FilesAndFolders.OnItemDisplay(item =>
            DisplayFileViewerItem(item, maps, fileViewer, zOrder: 1));

        //================

        folderViewer.OnDisplay(() => DisplayFileViewer(maps, folderViewer, zOrder: 0));
        folderViewer.FilesAndFolders.OnItemDisplay(item =>
            DisplayFileViewerItem(item, maps, folderViewer, zOrder: 1));

        return new Element[] { fileViewer, folderViewer };
    }

    private static void DisplayFileViewerItem(
        Button item,
        TilemapManager maps,
        FileViewer fileViewer,
        int zOrder)
    {
        var color = item.IsSelected ? Color.Green : Color.Gray.ToBright();
        var (x, y) = item.Position;
        var icon = fileViewer.IsFolder(item) ?
            new Tile(Tile.ICON_FOLDER, GetColor(item, Color.Yellow)) :
            new(Tile.ICON_FILE, GetColor(item, Color.Gray.ToBright()));

        maps[zOrder].SetTile((x, y), icon);
        maps[zOrder].SetTextLine(
            position: (x + 1, y),
            item.Text,
            GetColor(item, color),
            maxLength: item.Size.width - 1);
    }

    private static void DisplayFileViewer(TilemapManager maps, FileViewer fileViewer, int zOrder)
    {
        var e = fileViewer;
        var color = GetColor(e.Back, Color.Gray);
        var (x, y) = e.Back.Position;
        var selected = e.SelectedPaths;
        var paths = "";

        SetBackground(maps[0], e);
        maps[zOrder].SetTextLine((e.Position.x, e.Position.y - 1), e.Text);

        if (e.FilesAndFolders.Scroll.IsHidden == false)
            SlidersAndScrolls.DisplayScroll(maps, e.FilesAndFolders.Scroll, zOrder);

        maps[zOrder + 2].SetTile((x, y), new(Tile.ICON_BACK, color));
        maps[zOrder + 2].SetTextLine(
            position: (x + 1, y),
            text: e.CurrentDirectory,
            color,
            maxLength: -e.Back.Size.width + 1);

        foreach (var path in selected)
            paths += $"{Environment.NewLine}{Environment.NewLine}{path}";

        maps[zOrder].SetTextRectangle(
            position: (e.Position.x, e.Position.y + e.Size.height - 1),
            size: (e.Size.width, 20),
            paths);
    }
}