using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Pure.UserInterface;

public class FileViewer : Element
{
    public Button Back { get; private set; }
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
            watcher.Path = value;

            Refresh();
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
        Size = (12, 8);
        Init();
        FilesAndFolders = new(position, 0)
        {
            isReadOnly = true,
            hasParent = true,
            itemSelectCallback = OnInternalItemSelect,
            itemTriggerCallback = OnInternalItemTrigger,
            itemDisplayCallback = OnInternalItemDisplay
        };
        IsSelectingFolders = isSelectingFolders;
        CurrentDirectory = directory;
    }
    public FileViewer(byte[] bytes) : base(bytes)
    {
        Init();
        FilesAndFolders = new(bytes)
        {
            isReadOnly = true,
            hasParent = true,
            itemSelectCallback = OnInternalItemSelect,
            itemTriggerCallback = OnInternalItemTrigger,
            itemDisplayCallback = OnInternalItemDisplay
        };
        IsSelectingFolders = GrabBool(bytes);
        CurrentDirectory = GrabString(bytes);
    }

    public bool IsFolder(Button item) => FilesAndFolders.IndexOf(item) < CountFolders;

    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        result.AddRange(FilesAndFolders.ToBytes());
        PutBool(result, IsSelectingFolders);
        PutString(result, CurrentDirectory);
        return result.ToArray();
    }

    internal override void OnInput()
    {
        TryScrollWhileHover(Back);
    }
    internal override void OnUpdate()
    {
        LimitSizeMin((3, 3));

        var (x, y) = Position;
        var (w, h) = Size;

        Back.Update();
        Back.size = (w, 1);
        Back.position = (x, y);

        FilesAndFolders.Update();
        FilesAndFolders.size = (w, h - 1);
        FilesAndFolders.position = (x, y + 1);
        FilesAndFolders.itemSize = (w, 1);
    }

    protected virtual void OnItemDisplay(Button item) { }
    protected virtual void OnItemTrigger(Button item) { }
    protected virtual void OnItemSelect(Button item) { }

#region Backend
    private int clickedIndex;
    private readonly Stopwatch doubleClick = new();
    private string dir = "";
    private readonly FileSystemWatcher watcher = new();
    private static string DefaultPath =>
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    // used in the UI class to receive callbacks
    internal Action<Button>? itemDisplayCallback;
    internal Action<Button>? itemTriggerCallback;
    internal Action<Button>? itemSelectCallback;

    [MemberNotNull(nameof(Back))]
    private void Init()
    {
        watcher.EnableRaisingEvents = true;
        watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName;
        watcher.Changed += (_, _) => Refresh();
        watcher.Deleted += (_, _) => Refresh();
        watcher.Created += (_, _) => Refresh();
        watcher.Renamed += (_, _) => Refresh();

        Back = new(position) { hasParent = true };
        Back.SubscribeToUserAction(UserAction.Trigger, () =>
            CurrentDirectory = Path.GetDirectoryName(CurrentDirectory) ?? DefaultPath);
    }

    private void Refresh()
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
            FilesAndFolders[^1].isTextReadonly = true;
        }

        CountFolders = directories.Length;

        if (IsSelectingFolders)
            return;

        foreach (var file in files)
        {
            FilesAndFolders.InternalAdd();
            FilesAndFolders[^1].Text = $"{Path.GetFileName(file)}";
            FilesAndFolders[^1].isTextReadonly = true;
        }

        CountFiles = files.Length;

        FilesAndFolders.Scroll.Slider.Progress = 0;
    }
    private void TryScrollWhileHover(Element element)
    {
        if (element.IsHovered && Input.Current.ScrollDelta != 0 && element.IsFocused &&
            FocusedPrevious == element)
            FilesAndFolders.Scroll.Slider.Progress -=
                Input.Current.ScrollDelta * FilesAndFolders.Scroll.Step;
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

        OnItemSelect(item);
        itemSelectCallback?.Invoke(item);

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