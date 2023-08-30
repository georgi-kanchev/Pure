namespace Pure.UserInterface;

using System.Diagnostics.CodeAnalysis;

public class FileViewer : Element
{
    public Button Back { get; private set; }
    public List FilesAndFolders { get; }

    public string CurrentDirectory
    {
        get => dir;
        set
        {
            if (dir == value)
                return;

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
        IsSelectingFolders = GrabBool(bytes);
        CurrentDirectory = GrabString(bytes);

        var listLength = GrabInt(bytes);
        FilesAndFolders = new(bytes[^listLength..])
        {
            isReadOnly = true,
            hasParent = true,
            itemSelectCallback = OnInternalItemSelect,
            itemTriggerCallback = OnInternalItemTrigger,
            itemDisplayCallback = OnInternalItemDisplay
        };

        // try to refresh if the loaded directory doesn't exist so that
        // all items are up to date
        CurrentDirectory = dir;
    }

    public bool IsFolder(Button item) => FilesAndFolders.IndexOf(item) < CountFolders;

    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutBool(result, IsSelectingFolders);
        PutString(result, CurrentDirectory);

        var bList = FilesAndFolders.ToBytes();
        PutInt(result, bList.Length);
        result.AddRange(bList);

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
            CreateItem(directory);

        CountFolders = directories.Length;

        if (IsSelectingFolders)
            return;

        foreach (var file in files)
            CreateItem(file);

        CountFiles = files.Length;

        FilesAndFolders.Scroll.Slider.Progress = 0;
    }
    private void CreateItem(string path)
    {
        FilesAndFolders.InternalAdd();
        var item = FilesAndFolders[^1];
        item.Text = $"{Path.GetFileName(path)}";
        item.isTextReadonly = true;
        item.SubscribeToUserAction(UserAction.DoubleTrigger, () =>
            CurrentDirectory = Path.Join(CurrentDirectory, item.Text));
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
        var isFolder = IsFolder(item);

        if (isFolder != IsSelectingFolders)
            item.isSelected = false;

        OnItemSelect(item);
        itemSelectCallback?.Invoke(item);
    }
    private void OnInternalItemDisplay(Button item)
    {
        OnItemDisplay(item);
        itemDisplayCallback?.Invoke(item);
    }
#endregion
}