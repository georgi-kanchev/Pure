namespace Pure.Examples.Systems.UserInterface;

using Pure.UserInterface;
using Pure.Utilities;
using Tilemap;
using static Utility;

public static class FileViewers
{
    public static Element[] Create(TilemapManager maps)
    {
        var fileViewer = new FileViewer((1, 2))
        {
            Text = "Select Files:",
            Size = (22, 10)
        };
        var folderViewer = new FileViewer((25, 2))
        {
            Text = "Select Folders:",
            Size = (22, 10),
            IsSelectingFolders = true
        };

        fileViewer.OnDisplay(() => DisplayFileViewer(maps, fileViewer));
        fileViewer.FilesAndFolders.OnItemDisplay((item) => DisplayFileViewerItem(item, maps, fileViewer));
        folderViewer.OnDisplay(() => DisplayFileViewer(maps, folderViewer));
        folderViewer.FilesAndFolders.OnItemDisplay((item) => DisplayFileViewerItem(item, maps, folderViewer));

        return new Element[] { fileViewer, folderViewer };
    }

    private static void DisplayFileViewerItem(Button item, TilemapManager maps, FileViewer fileViewer)
    {
        var color = item.IsSelected ? Color.Green : Color.Gray.ToBright();
        var (x, y) = item.Position;
        var icon = fileViewer.IsFolder(item) ? new Tile(Tile.ICON_FOLDER, GetColor(item, Color.Yellow)) : new(Tile.ICON_FILE, GetColor(item, Color.Gray.ToBright()));

        icon = item.Text == ".." ? Tile.ICON_BACK : icon;

        maps[2].SetTile((x, y), icon);
        maps[2].SetTextLine((x + 1, y), item.Text, GetColor(item, color), item.Size.width - 1);
    }

    private static void DisplayFileViewer(TilemapManager maps, FileViewer fileViewer)
    {
        var e = fileViewer;
        SetBackground(maps[0], e);

        maps[0].SetTextLine((e.Position.x, e.Position.y - 1), e.Text);
        if (e.FilesAndFolders.Scroll.IsHidden == false)
            SlidersAndScrolls.DisplayScroll(maps, e.FilesAndFolders.Scroll);

        var color = GetColor(e.Back, Color.Gray);
        var (x, y) = e.Back.Position;
        maps[2].SetTile((x, y), new(Tile.ICON_BACK, color));
        maps[2].SetTextLine((x + 1, y), e.CurrentDirectory, color, -e.Back.Size.width + 2);

        var selected = e.SelectedPaths;
        var paths = "";
        foreach (var path in selected)
            paths += $"{Environment.NewLine}{Environment.NewLine}{path}";

        maps[0].SetTextRectangle((e.Position.x, e.Position.y + e.Size.height - 1), (e.Size.width, 20), paths);
    }
}