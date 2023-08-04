using System.Diagnostics;

namespace Pure.UserInterface;

public class FileViewer : List
{
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

            for (var i = 1; i < Count; i++)
                if (this[i].IsSelected)
                    result.Add(Path.Join(CurrentDirectory, this[i].Text));

            return result.ToArray();
        }
    }

    public FileViewer((int x, int y) position, string path = "", bool isSelectingFolders = false)
        : base(position)
    {
        IsSelectingFolders = isSelectingFolders;
        CurrentDirectory = path;
        RecreateAllItems();
    }
    public FileViewer(byte[] bytes) : base(bytes) { }

    public bool IsFolder(Button item) => IndexOf(item) <= CountFolders;

    internal override void OnUpdate()
    {
        base.OnUpdate();
        itemSize = (Size.width, 1);
    }

    protected override void OnItemSelect(Button item)
    {
        if (IsSingleSelecting == false)
            return;

        if (IsFolder(item) != IsSelectingFolders)
            Select(0);
    }
    protected override void OnItemTrigger(Button item)
    {
        var index = IndexOf(item);
        if (index == 0)
        {
            CurrentDirectory = $"{Directory.GetParent(CurrentDirectory)}";
            return;
        }

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

#region Backend
    private int clickedIndex;
    private readonly Stopwatch doubleClick = new();
    private string dir = "";
    private static string DefaultPath =>
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

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

        InternalClear();

        CountFolders = 0;
        CountFiles = 0;

        InternalAdd();
        this[^1].Text = "..";
        foreach (var directory in directories)
        {
            InternalAdd();
            this[^1].Text = $"{Path.GetFileName(directory)}";
        }

        if (IsSelectingFolders)
        {
            CountFiles = 0;
            CountFolders = directories.Length;
            return;
        }

        foreach (var file in files)
        {
            InternalAdd();
            this[^1].Text = $"{Path.GetFileName(file)}";
        }

        CountFolders = directories.Length;
        CountFiles = files.Length;

        Scroll.Slider.Progress = 0;
    }
#endregion
}