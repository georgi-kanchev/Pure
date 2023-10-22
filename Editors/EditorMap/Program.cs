global using Pure.Engine.Utilities;
global using Pure.Engine.Tilemap;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Window;
global using static Pure.Default.RendererUserInterface.Default;

namespace Pure.Editors.EditorMap;

public static class Program
{
    public static void Run()
    {
        var editor = new EditorBase.Editor(
            title: "Pure - Map Editor",
            scaleUi: 5,
            mapSize: (100, 100));

        editor.Run();
    }
}