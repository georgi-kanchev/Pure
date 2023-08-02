namespace Pure.UserInterface;

public class FileViewer : List
{
    public string? CurrentDirectory
    {
        get => dir;
        set
        {
            var defaultPath = DefaultPath;

            value ??= defaultPath;

            if (Directory.Exists(value) == false)
                value = defaultPath;

            dir = value;
        }
    }

    public int CountFiles { get; private set; }
    public int CountFolders { get; private set; }

    public FileViewer((int x, int y) position, string? path = null)
        : base(position)
    {
        CurrentDirectory = path;
        RecreateAllItems();
    }
    public FileViewer(byte[] bytes) : base(bytes) { }

    internal override void OnUpdate()
    {
        base.OnUpdate();
        ItemSize = (Size.width, 1);
    }

#region Backend
    private string? dir;
    private static string DefaultPath =>
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    private void RecreateAllItems()
    {
        var path = CurrentDirectory;
        if (path == null)
            return;

        var directories = Directory.GetDirectories(path);
        var files = Directory.GetFiles(path);

        Clear();
        foreach (var directory in directories)
        {
            Add();
            this[^1].Text = $"{Path.GetFileName(directory)}";
        }

        foreach (var file in files)
        {
            Add();
            this[^1].Text = $"{Path.GetFileName(file)}";
        }

        CountFolders = directories.Length;
        CountFiles = files.Length;
    }
#endregion
}