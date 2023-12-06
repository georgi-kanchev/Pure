using System.Runtime.InteropServices;

namespace Pure.Engine.UserInterface;

using System.Diagnostics.CodeAnalysis;
using static Environment;

public class FileViewer : Block
{
    public enum Directory
    {
        Desktop,
        Programs,
        LocalApplicationData,
        Favorites,
        Recent,
        UserProfile,
        MyDocuments,
        MyMusic,
        MyVideos,
        MyPictures,
        Fonts
    }

    public Button Back { get; private set; }
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

            for (var i = 0; i < FilesAndFolders.Count; i++)
                if (FilesAndFolders[i].IsSelected)
                    result.Add(Path.Join(CurrentDirectory, FilesAndFolders[i].Text));

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
        Size = (12, 8);
        Init();
        IsSelectingFolders = isSelectingFolders;
    }
    public FileViewer(byte[] bytes) : base(bytes)
    {
        Init();
        IsSelectingFolders = GrabBool(bytes);
        FileFilter = GrabString(bytes);
    }

    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutBool(result, IsSelectingFolders);
        PutString(result, FileFilter ?? "");
        return result.ToArray();
    }

    public bool IsFolder(Button item)
    {
        return FilesAndFolders.IndexOf(item) < CountFolders;
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

#region Backend
    private string dir = "default";
    private FileSystemWatcher watcher;
    private static string DefaultPath
    {
        get => GetPath(Directory.LocalApplicationData);
    }

    private bool isSelectingFolders;
    private string? fileFilter;

    [MemberNotNull(nameof(Back), nameof(FilesAndFolders), nameof(watcher))]
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

        FilesAndFolders = new(position, 0)
        {
            isReadOnly = true,
            hasParent = true,
        };

        Back = new(position) { hasParent = true };
        Back.OnInteraction(Interaction.Scroll, ApplyScroll);
        Back.OnInteraction(Interaction.Trigger, () =>
            CurrentDirectory = Path.GetDirectoryName(CurrentDirectory) ?? DefaultPath);

        CurrentDirectory = dir;

        OnUpdate(OnUpdate);
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

        FilesAndFolders.InternalClear();

        CountFolders = 0;
        CountFiles = 0;

        foreach (var directory in directories)
            CreateItem(directory);

        CountFolders = directories.Length;

        if (IsSelectingFolders)
            return;

        var filters = FileFilter?.Split(Path.DirectorySeparatorChar);
        foreach (var file in files)
            if (IsShowingFile(filters, file))
                CreateItem(file);

        CountFiles = files.Length;
        Back.Text = path;
        FilesAndFolders.Scroll.Slider.Progress = 0;

        var drives = GetHardDrives();
    }
    private void CreateItem(string path)
    {
        var item = new Button
        {
            Text = $"{Path.GetFileName(path)}",
            isTextReadonly = true
        };
        FilesAndFolders.InternalAdd(item);

        item.OnInteraction(Interaction.DoubleTrigger, () =>
        {
            if (IsFolder(item))
                CurrentDirectory = Path.Join(CurrentDirectory, item.Text);
        });

        item.OnInteraction(Interaction.Select, () =>
        {
            if (IsSelectingFolders ^ IsFolder(item) || item.IsSelected)
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
        LimitSizeMin((3, 3));

        if (FilesAndFolders is not { IsSingleSelecting: true, ItemsSelected.Length: 1 })
            return;

        var selected = FilesAndFolders.ItemsSelected;
        var index = FilesAndFolders.IndexOf(selected[0]);
        if (IsSelectingFolders ^ index < CountFolders)
            FilesAndFolders.Deselect();
    }
    internal override void OnChildrenUpdate()
    {
        var (x, y) = Position;
        var (w, h) = Size;

        Back.size = (w, 1);
        Back.position = (x, y);
        Back.Update();

        FilesAndFolders.size = (w, h - 1);
        FilesAndFolders.position = (x, y + 1);
        FilesAndFolders.itemSize = (w, 1);
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

    private static List<(string path, float totalGb, float percentFilled)> GetHardDrives()
    {
        var result = new List<(string path, float totalGb, float percentFilled)>();
        var drives = DriveInfo.GetDrives();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            foreach (var drive in drives)
                if (drive.DriveType == DriveType.Fixed)
                    AddDrive(drive);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var sep = Path.DirectorySeparatorChar.ToString();
            var uuids = System.IO.Directory.GetFiles($"{sep}dev{sep}disk{sep}by-uuid{sep}");

            foreach (var drive in drives)
            {
                if (drive.Name == sep)
                    AddDrive(drive);

                foreach (var id in uuids)
                    if (drive.RootDirectory.Name == Path.GetFileName(id))
                        AddDrive(drive);
            }
        }

        return result;

        void AddDrive(DriveInfo drive)
        {
            var total = drive.TotalSize / MathF.Pow(1024, 3);
            var free = drive.AvailableFreeSpace / MathF.Pow(1024, 3);
            result.Add((drive.Name, total, Map(free, (0f, total), (100f, 0f))));
        }
    }

    private static float Map(float number, (float a, float b) range, (float a, float b) targetRange)
    {
        var value = (number - range.a) / (range.b - range.a) * (targetRange.b - targetRange.a) +
                    targetRange.a;
        return float.IsNaN(value) || float.IsInfinity(value) ? targetRange.a : value;
    }
#endregion
}