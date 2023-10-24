namespace Pure.Editors.EditorMap;

public static class Program
{
    public static void Run()
    {
        var editor = new EditorBase.Editor(title: "Pure - Map Editor", mapSize: (50, 50));
        editor.Run();
    }
}