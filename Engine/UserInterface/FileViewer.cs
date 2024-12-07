using static System.Environment;

namespace Pure.Engine.UserInterface;

public class FileViewer : Block
{
    public enum Directory
    {
        Desktop, Programs, LocalApplicationData, Favorites, Recent,
        UserProfile, MyDocuments, MyMusic, MyVideos, MyPictures, Fonts
    }

    [DoNotSave]
    public Button User { get; }
    [DoNotSave]
    public Button Back { get; }
    [DoNotSave]
    public List HardDrives { get; }
    [DoNotSave]
    public List FilesAndFolders { get; }

    public string? CurrentDirectory
    {
        get => dir;
        set
        {
            var defaultPath = DefaultPath;
            value ??= defaultPath;

            if (System.IO.Directory.Exists(value) == false)
                value = defaultPath;

            if (dir == value)
                return;

            dir = value;
            watcher.Path = value;

            Refresh();
        }
    }
    public bool IsSelectingFolders
    {
        get => isSelectingFolders;
        set
        {
            if (value == isSelectingFolders)
                return;

            isSelectingFolders = value;

            Refresh();
        }
    }

    public int CountFiles { get; private set; }
    public int CountFolders { get; private set; }

    public string[] SelectedPaths
    {
        get
        {
            var result = new List<string>();
            var selectedItems = FilesAndFolders.SelectedItems;

            foreach (var item in selectedItems)
                result.Add(Path.Join(CurrentDirectory, item.Text));

            return result.ToArray();
        }
    }
    public string? FileFilter
    {
        get => fileFilter;
        set
        {
            if (fileFilter == value)
                return;

            fileFilter = value;
            Refresh();
        }
    }

    public FileViewer() : this((0, 0))
    {
    }
    public FileViewer((int x, int y) position) : base(position)
    {
        Size = (16, 16);
        watcher = new(DefaultPath)
        {
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName
        };
        watcher.Changed += (_, _) => Refresh();
        watcher.Deleted += (_, _) => Refresh();
        watcher.Created += (_, _) => Refresh();
        watcher.Renamed += (_, _) => Refresh();

        var drives = GetHardDrives();
        HardDrives = new(position, drives.Count)
        {
            isReadOnly = true,
            hasParent = true,
            wasMaskSet = true,
            IsSingleSelecting = true
        };
        FilesAndFolders = new(position, 0)
        {
            isReadOnly = true,
            hasParent = true,
            wasMaskSet = true
        };
        FilesAndFolders.OnInteraction(Interaction.Select, () => Interact(Interaction.Select));

        Back = new(position) { hasParent = true, wasMaskSet = true, isTextReadonly = true };
        Back.OnInteraction(Interaction.Scroll, ApplyScroll);
        Back.OnInteraction(Interaction.Trigger, () =>
            CurrentDirectory = Path.GetDirectoryName(CurrentDirectory) ?? DefaultPath);

        User = new(position)
        {
            hasParent = true,
            wasMaskSet = true,
            isTextReadonly = true,
            text = GetPath(Directory.UserProfile)
        };
        User.OnInteraction(Interaction.Scroll, ApplyScroll);
        User.OnInteraction(Interaction.Trigger, () => CurrentDirectory = User.text);

        CurrentDirectory = dir;

        OnUpdate += OnRefresh;

        for (var i = 0; i < drives.Count; i++)
        {
            var drive = HardDrives.Items[i];
            drive.OnInteraction(Interaction.Scroll, ApplyScroll);
            drive.OnInteraction(Interaction.Select, () => CurrentDirectory = drive.Text);
            drive.text = drives[i];
        }

        IsSelectingFolders = isSelectingFolders;
    }

    public bool IsFolder(Button item)
    {
        return FilesAndFolders.Items.IndexOf(item) < CountFolders;
    }
    public static string GetPath(Directory directory)
    {
        return directory switch
        {
            Directory.Desktop => GetFolderPath(SpecialFolder.Desktop),
            Directory.Programs => GetFolderPath(SpecialFolder.Programs),
            Directory.LocalApplicationData => GetFolderPath(SpecialFolder.LocalApplicationData),
            Directory.Favorites => GetFolderPath(SpecialFolder.Favorites),
            Directory.Recent => GetFolderPath(SpecialFolder.Recent),
            Directory.UserProfile => GetFolderPath(SpecialFolder.UserProfile),
            Directory.MyDocuments => GetFolderPath(SpecialFolder.MyDocuments),
            Directory.MyMusic => GetFolderPath(SpecialFolder.MyMusic),
            Directory.MyVideos => GetFolderPath(SpecialFolder.MyVideos),
            Directory.MyPictures => GetFolderPath(SpecialFolder.MyPictures),
            Directory.Fonts => GetFolderPath(SpecialFolder.Fonts),
            _ => GetFolderPath(SpecialFolder.LocalApplicationData)
        };
    }

#region Backend
    private string dir = "default";
    [DoNotSave]
    private readonly FileSystemWatcher watcher;
    private static string DefaultPath
    {
        get => GetPath(Directory.LocalApplicationData);
    }

    private bool isSelectingFolders;
    private string? fileFilter;

    private void Refresh()
    {
        var path = CurrentDirectory ?? DefaultPath;
        string[] directories;
        string[] files;

        try
        {
            directories = System.IO.Directory.GetDirectories(path);
            files = System.IO.Directory.GetFiles(path);
        }
        catch (Exception)
        {
            CurrentDirectory = DefaultPath;
            path = CurrentDirectory;
            directories = System.IO.Directory.GetDirectories(path);
            files = System.IO.Directory.GetFiles(path);
        }

        FilesAndFolders.Items.Clear();

        CountFolders = 0;
        CountFiles = 0;

        foreach (var directory in directories)
            CreateItem(directory);

        CountFolders = directories.Length;

        Back.text = path;

        if (IsSelectingFolders == false)
        {
            var filters = FileFilter?.Split(Path.DirectorySeparatorChar);
            foreach (var file in files)
                if (IsShowingFile(filters, file))
                    CreateItem(file);

            CountFiles = files.Length;
            FilesAndFolders.Scroll.Slider.Progress = 0;
        }

        Interact(Interaction.Select);
    }
    private void CreateItem(string path)
    {
        var item = new Button
        {
            Text = $"{Path.GetFileName(path)}",
            isTextReadonly = true
        };
        FilesAndFolders.Items.Add(item);

        item.OnInteraction(Interaction.DoubleTrigger, () =>
        {
            if (IsFolder(item))
                CurrentDirectory = Path.Join(CurrentDirectory, item.Text);
        });
        item.OnInteraction(Interaction.Select, () =>
        {
            if (IsSelectingFolders == false && IsFolder(item))
                FilesAndFolders.Select(item, false);

            Interact(Interaction.Select);
        });
    }

    internal override void ApplyScroll()
    {
        if (FilesAndFolders.IsHovered == false)
            FilesAndFolders.ApplyScroll();
    }
    internal void OnRefresh()
    {
        LimitSizeMin((3, 3 + HardDrives.Items.Count));

        if (FilesAndFolders is not { IsSingleSelecting: true, SelectedItems.Count: 1 })
            return;

        var selected = FilesAndFolders.SelectedItems;
        var index = FilesAndFolders.Items.IndexOf(selected[0]);
        if ((IsSelectingFolders ^ (index < CountFolders)) == false)
            return;

        FilesAndFolders.Deselect();
        Interact(Interaction.Select);
    }
    internal override void OnChildrenUpdate()
    {
        var (x, y) = Position;
        var (w, h) = Size;
        var hds = HardDrives.Items.Count;

        HardDrives.size = (w, hds);
        HardDrives.position = (x, y);
        HardDrives.itemSize = (w, 1);
        HardDrives.mask = mask;
        HardDrives.Update();

        User.size = (w, 1);
        User.position = (x, y + hds);
        User.mask = mask;
        User.Update();

        Back.size = (w, 1);
        Back.position = (x, y + hds + 1);
        Back.mask = mask;
        Back.Update();

        FilesAndFolders.size = (w, h - hds - 2);
        FilesAndFolders.position = (x, y + hds + 2);
        FilesAndFolders.itemSize = (w, 1);
        FilesAndFolders.mask = mask;
        FilesAndFolders.Update();
    }

    private static bool IsShowingFile(string[]? filters, string filePath)
    {
        if (filters == null || filters.Length == 0)
            return true;

        foreach (var filter in filters)
            if (string.IsNullOrWhiteSpace(filter) == false &&
                filePath.EndsWith(filter) == false)
                return false;

        return true;
    }

    private static List<string> GetHardDrives()
    {
        var result = new List<string>();
        var drives = DriveInfo.GetDrives();

        if (OperatingSystem.IsWindows())
        {
            foreach (var drive in drives)
                if (drive.DriveType == DriveType.Fixed)
                    result.Add(drive.Name);
        }
        else if (OperatingSystem.IsLinux())
        {
            var sep = Path.DirectorySeparatorChar.ToString();
            var uuids = System.IO.Directory.GetFiles($"{sep}dev{sep}disk{sep}by-uuid{sep}");

            foreach (var drive in drives)
            {
                if (drive.Name == sep)
                    result.Add(drive.Name);

                foreach (var id in uuids)
                    if (drive.RootDirectory.Name == Path.GetFileName(id))
                        result.Add(drive.Name);
            }
        }

        return result;
    }
#endregion
}