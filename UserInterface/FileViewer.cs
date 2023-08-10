using System.Diagnostics;

namespace Pure.UserInterface;

public class FileViewer : Element
{
    public List FilesAndFolders { get; }

    public string CurrentDirectory
    {
        get => dir;
        set
        {
            var defaultPath = DefaultPath;

            value ??= defaultPath;

            if (Directory.Exists(value) == false)
                value = defaultPath;

            dir = value;
            RecreateAllItems();
        }
    }
    public bool IsSelectingFolders { get; }

    public int CountFiles { get; private set; }
    public int CountFolders { get; private set; }

    public string[] SelectedPaths
    {
        get
        {
            var result = new List<string>();

            for (var i = 0; i < FilesAndFolders.Count; i++)
                if (FilesAndFolders[i].IsSelected)
                    result.Add(Path.Join(CurrentDirectory, FilesAndFolders[i].Text));

            return result.ToArray();
        }
    }

    public FileViewer((int x, int y) position, string directory = "", bool isSelectingFolders = false)
        : base(position)
    {
        FilesAndFolders = new(position, 0)
        {
            hasParent = true,
            itemSelectCallback = OnInternalItemSelect,
            itemTriggerCallback = OnInternalItemTrigger,
            itemDisplayCallback = OnInternalItemDisplay
        };

        IsSelectingFolders = isSelectingFolders;
        CurrentDirectory = directory;
        RecreateAllItems();
    }
    public FileViewer(byte[] bytes) : base(bytes) { }

    public bool IsFolder(Button item) => FilesAndFolders.IndexOf(item) < CountFolders;

    internal override void OnUpdate()
    {
        var (x, y) = Position;
        var (w, h) = Size;

        FilesAndFolders.Update();
        FilesAndFolders.size = (w, h);
        FilesAndFolders.position = (x, y);
        FilesAndFolders.itemSize = (w, 1);
    }

    protected virtual void OnItemDisplay(Button item) { }
    protected virtual void OnItemTrigger(Button item) { }
    protected virtual void OnItemSelect(Button item) { }

#region Backend
    private int clickedIndex;
    private readonly Stopwatch doubleClick = new();
    private string dir = "";
    private static string DefaultPath =>
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    // used in the UI class to receive callbacks
    internal Action<Button>? itemDisplayCallback;
    internal Action<Button>? itemTriggerCallback;
    internal Action<Button>? itemSelectCallback;

    private void RecreateAllItems()
    {
        var path = CurrentDirectory;
        string[] directories;
        string[] files;

        try
        {
            directories = Directory.GetDirectories(path);
            files = Directory.GetFiles(path);
        }
        catch (Exception)
        {
            CurrentDirectory = DefaultPath;
            path = CurrentDirectory;
            directories = Directory.GetDirectories(path);
            files = Directory.GetFiles(path);
        }

        FilesAndFolders.InternalClear();

        CountFolders = 0;
        CountFiles = 0;

        foreach (var directory in directories)
        {
            FilesAndFolders.InternalAdd();
            FilesAndFolders[^1].Text = $"{Path.GetFileName(directory)}";
        }

        CountFolders = directories.Length;

        if (IsSelectingFolders)
            return;

        foreach (var file in files)
        {
            FilesAndFolders.InternalAdd();
            FilesAndFolders[^1].Text = $"{Path.GetFileName(file)}";
        }

        CountFiles = files.Length;

        FilesAndFolders.Scroll.Slider.Progress = 0;
    }
    private void OnInternalItemTrigger(Button item)
    {
        OnItemTrigger(item);
        itemTriggerCallback?.Invoke(item);

        if (FilesAndFolders.IsSingleSelecting == false)
            return;

        if (IsFolder(item) != IsSelectingFolders)
            FilesAndFolders.Select(0);
    }
    private void OnInternalItemSelect(Button item)
    {
        var index = FilesAndFolders.IndexOf(item);

        var isFolder = IsFolder(item);

        if (isFolder != IsSelectingFolders)
            item.isSelected = false;

        if (isFolder == false)
            return;

        if (index == clickedIndex && doubleClick.Elapsed.TotalSeconds < 0.5f)
            CurrentDirectory = Path.Join(CurrentDirectory, item.Text);

        clickedIndex = index;
        doubleClick.Restart();
    }
    private void OnInternalItemDisplay(Button item)
    {
        OnItemDisplay(item);
        itemDisplayCallback?.Invoke(item);
    }
#endregion
}