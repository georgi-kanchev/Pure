global using Pure.Editors.EditorBase;
global using Pure.Engine.Tilemap;
global using System.Diagnostics.CodeAnalysis;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Window;
global using Pure.Tools.Tilemapper;

namespace Pure.Editors.EditorCollision;

public static class Program
{
    public static void Run()
    {
        editor.Run();
    }

#region Backend
    private static readonly Editor editor;
    internal static Menu menu;

    static Program()
    {
        var (mw, mh) = (50, 50);

        editor = new(title: "Pure - Collision Editor", mapSize: (mw, mh), viewSize: (mw, mh));
        editor.MapsEditor.Clear();
        editor.MapsEditor.Add(new Tilemap((mw, mh)));
        editor.MapsEditor.ViewSize = (mw, mh);
        CreateMenu();

        editor.MapFileViewer.FilesAndFolders.OnItemInteraction(Interaction.DoubleTrigger, btn =>
        {
            if (editor.MapFileViewer.IsSelectingFolders || editor.MapFileViewer.IsFolder(btn))
                return;

            editor.Prompt.Close();
            editor.LoadMap();
        });

        var tools = new List(itemCount: 0)
        {
            Size = (8, 2),
            ItemSize = (8, 1),
            IsSingleSelecting = true
        };
        tools.Add(
            new Button { Text = "Per Tile" },
            new Button { Text = "Global" });
        tools.Select(0);
        tools.Align((1f, 0f));
        tools.OnDisplay(() => editor.MapsUi.SetList(tools, (int)Editor.LayerMapsUi.Front));
        tools.OnItemDisplay(
            item => editor.MapsUi.SetListItem(tools, item, (int)Editor.LayerMapsUi.Front));
        editor.Ui.Add(new Block[] { tools });
    }

    [MemberNotNull(nameof(menu))]
    private static void CreateMenu()
    {
        menu = new(editor,
                "Save… ",
                " Solids Map",
                " Solids Pack",
                "Load… ",
                " Solids Map",
                " Solids Pack",
                " Tileset",
                " Tilemap")
            { Size = (12, 8) };
        menu.OnItemInteraction(Interaction.Trigger, btn =>
        {
            menu.IsHidden = true;
            menu.IsDisabled = true;
            var index = menu.IndexOf(btn);

            if (index == 1)
            {
            }
            else if (index == 6) // load tileset
                editor.OpenTilesetPrompt(null, null);
            else if (index == 7) // load map
            {
                editor.Prompt.Text = "Select a Map File:";
                editor.MapFileViewer.IsSelectingFolders = false;
                editor.Prompt.Open(editor.MapFileViewer, i =>
                {
                    editor.Prompt.Close();

                    if (i != 0)
                        return;

                    editor.LoadMap();
                });
            }
        });

        Mouse.Button.Right.OnPress(() =>
        {
            if (editor.Prompt.IsHidden == false)
                return;

            var (mx, my) = editor.LayerUi.PixelToWorld(Mouse.CursorPosition);
            menu.IsHidden = false;
            menu.IsDisabled = false;
            menu.Position = ((int)mx + 1, (int)my + 1);
        });
#endregion
    }
}