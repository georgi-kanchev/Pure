namespace Pure.Engine.UserInterface;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using static Environment;

public class FileViewer : Block
{
    public enum Directory
    {
        Desktop, Programs, LocalApplicationData, Favorites, Recent,
        UserProfile, MyDocuments, MyMusic, MyVideos, MyPictures, Fonts
    }

    public Button User { get; private set; }
    public Button Back { get; private set; }
    public List HardDrives { get; private set; }
    public List FilesAndFolders { get; private set; }

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

            for (var i = 0; i < FilesAndFolders.Items.Count; i++)
                if (FilesAndFolders.Items[i].IsSelected)
                    result.Add(Path.Join(CurrentDirectory, FilesAndFolders.Items[i].Text));

            return result.ToArray();
        }
    }
    public string? FileFilter
    {
        get => fileFilter;
        set
        {
            fileFilter = value;
            Refresh();
        }
    }

    public FileViewer((int x, int y) position = default) : base(position)
    {
        Size = (16, 16);
        Init();
        IsSelectingFolders = isSelectingFolders;
    }
    public FileViewer(byte[] bytes) : base(bytes)
    {
        Init();
        var b = Decompress(bytes);
        IsSelectingFolders = GrabBool(b);
        FileFilter = GrabString(b);
    }
    public FileViewer(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public override string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public override byte[] ToBytes()
    {
        var result = Decompress(base.ToBytes()).ToList();
        PutBool(result, IsSelectingFolders);
        PutString(result, FileFilter ?? string.Empty);
        return Compress(result.ToArray());
    }

    public bool IsFolder(Button item)
    {
        return FilesAndFolders.Items.IndexOf(item) < CountFolders;
    }
    public static string GetPath(Directory directory)
    {
        switch (directory)
        {
            case Directory.Desktop: return GetFolderPath(SpecialFolder.Desktop);
            case Directory.Programs: return GetFolderPath(SpecialFolder.Programs);
            case Directory.LocalApplicationData:
            default: return GetFolderPath(SpecialFolder.LocalApplicationData);
            case Directory.Favorites: return GetFolderPath(SpecialFolder.Favorites);
            case Directory.Recent: return GetFolderPath(SpecialFolder.Recent);
            case Directory.UserProfile: return GetFolderPath(SpecialFolder.UserProfile);
            case Directory.MyDocuments: return GetFolderPath(SpecialFolder.MyDocuments);
            case Directory.MyMusic: return GetFolderPath(SpecialFolder.MyMusic);
            case Directory.MyVideos: return GetFolderPath(SpecialFolder.MyVideos);
            case Directory.MyPictures: return GetFolderPath(SpecialFolder.MyPictures);
            case Directory.Fonts: return GetFolderPath(SpecialFolder.Fonts);
        }
    }

    public FileViewer Duplicate()
    {
        return new(ToBytes());
    }

    public static implicit operator byte[](FileViewer button)
    {
        return button.ToBytes();
    }
    public static implicit operator FileViewer(byte[] bytes)
    {
        return new(bytes);
    }

#region Backend
    private string dir = "default";
    private FileSystemWatcher watcher;
    private static string DefaultPath
    {
        get => GetPath(Directory.LocalApplicationData);
    }

    private bool isSelectingFolders;
    private string? fileFilter;

    [MemberNotNull(nameof(User), nameof(Back), nameof(HardDrives), nameof(FilesAndFolders),
        nameof(watcher))]
    private void Init()
    {
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

        OnUpdate(OnUpdate);

        for (var i = 0; i < drives.Count; i++)
        {
            var drive = HardDrives.Items[i];
            drive.OnInteraction(Interaction.Scroll, ApplyScroll);
            drive.OnInteraction(Interaction.Select, () => CurrentDirectory = drive.Text);
            drive.text = drives[i];
        }
    }

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

        if (IsSelectingFolders)
            return;

        var filters = FileFilter?.Split(Path.DirectorySeparatorChar);
        foreach (var file in files)
            if (IsShowingFile(filters, file))
                CreateItem(file);

        CountFiles = files.Length;
        FilesAndFolders.Scroll.Slider.Progress = 0;
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
        });
    }

    internal override void ApplyScroll()
    {
        if (FilesAndFolders.IsHovered == false)
            FilesAndFolders.ApplyScroll();
    }
    internal void OnUpdate()
    {
        LimitSizeMin((3, 3 + HardDrives.Items.Count));

        if (FilesAndFolders is not { IsSingleSelecting: true, SelectedItems.Count: 1 })
            return;

        var selected = FilesAndFolders.SelectedItems;
        var index = FilesAndFolders.Items.IndexOf(selected[0]);
        if (IsSelectingFolders ^ (index < CountFolders))
            FilesAndFolders.Deselect();
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