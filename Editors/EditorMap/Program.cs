global using System.Diagnostics.CodeAnalysis;
global using Pure.Editors.EditorBase;
global using Pure.Engine.Tilemap;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Utilities;
global using Pure.Engine.Window;
global using static Pure.Default.Tilemapper.TilemapperUserInterface;

namespace Pure.Editors.EditorMap;

public static class Program
{
    public static void Run()
    {
        editor.OnUpdateUi += tilePalette.TryDraw;
        editor.OnUpdateEditor += UpdateEditor;
        editor.OnUpdateLate += () => tilePalette.Update(inspector);
        editor.Run();
    }

#region Backend
    private static readonly Editor editor;
    private static readonly Inspector inspector;
    private static readonly TilePalette tilePalette;
    private static readonly TilesetPrompt tilesetPrompt;
    private static readonly FileViewer mapSaveLoad;

    internal static Menu menu;

    static Program()
    {
        var (mw, mh) = (50, 50);

        editor = new(title: "Pure - Map Editor", mapSize: (mw, mh), viewSize: (mw, mh));
        editor.MapsEditor.Clear();
        editor.MapsEditor.Add(new Tilemap((mw, mh)));
        editor.MapsEditor.ViewSize = (mw, mh);

        tilePalette = new(editor);
        inspector = new(editor, tilePalette);
        tilesetPrompt = new(editor, tilePalette);

        const int BACK = (int)Editor.LayerMapsUi.PromptBack;
        const int MIDDLE = (int)Editor.LayerMapsUi.PromptMiddle;
        mapSaveLoad = new()
        {
            FilesAndFolders = { IsSingleSelecting = true },
            Size = (21, 10)
        };
        mapSaveLoad.OnDisplay(() => editor.MapsUi.SetFileViewer(mapSaveLoad, BACK));
        mapSaveLoad.FilesAndFolders.OnItemDisplay(btn =>
            editor.MapsUi.SetFileViewerItem(mapSaveLoad, btn, MIDDLE));
        mapSaveLoad.HardDrives.OnItemDisplay(btn =>
            editor.MapsUi.SetFileViewerItem(mapSaveLoad, btn, MIDDLE));
        mapSaveLoad.FilesAndFolders.OnItemInteraction(Interaction.DoubleTrigger, btn =>
        {
            if (mapSaveLoad.IsSelectingFolders == false && mapSaveLoad.IsFolder(btn) == false)
                Save(mapSaveLoad.FilesAndFolders.ItemsSelected);
        });

        CreateMenu();
    }

    [MemberNotNull(nameof(menu))]
    private static void CreateMenu()
    {
        menu = new(editor,
                "Save… ",
                "  Map",
                "Load… ",
                "  Tileset",
                "  Map")
            { Size = (9, 5) };
        menu.OnItemInteraction(Interaction.Trigger, btn =>
        {
            menu.IsHidden = true;
            menu.IsDisabled = true;
            var index = menu.IndexOf(btn);

            if (index == 1) // save map
            {
                editor.Prompt.Text = "Select a Directory:";
                mapSaveLoad.IsSelectingFolders = true;
                editor.Prompt.Open(mapSaveLoad, i =>
                {
                    editor.Prompt.Close();

                    if (i != 0)
                        return;

                    var selected = mapSaveLoad.FilesAndFolders.ItemsSelected;

                    if (selected.Length == 0)
                        selected = new[] { mapSaveLoad.Back };

                    Save(selected);
                });
            }
            else if (index == 3) // load tileset
                tilesetPrompt.Open();
            else if (index == 4) // load map
            {
                editor.Prompt.Text = "Select a Map File:";
                mapSaveLoad.IsSelectingFolders = false;
                editor.Prompt.Open(mapSaveLoad, i =>
                {
                    editor.Prompt.Close();

                    if (i != 0)
                        return;

                    Load(mapSaveLoad.FilesAndFolders.ItemsSelected);
                });
            }
        });

        Mouse.OnButtonPress(Mouse.Button.Right, () =>
        {
            var (mx, my) = editor.LayerUi.PixelToWorld(Mouse.CursorPosition);
            menu.IsHidden = false;
            menu.IsDisabled = false;
            menu.Position = ((int)mx + 1, (int)my + 1);
        });
    }

    private static void UpdateEditor()
    {
        editor.IsDisabledViewInteraction = inspector.IsHovered;
    }

    internal static void PromptMessage(string msg)
    {
        editor.Prompt.Text = msg;
        editor.Prompt.ButtonCount = 1;
        editor.Prompt.Open(onButtonTrigger: _ => editor.Prompt.Close());
        editor.Prompt.ButtonCount = 2;
    }

    private static void Save(Button[] files)
    {
        try
        {
            File.WriteAllBytes(files[0].Text, editor.MapsEditor.ToBytes());
        }
        catch (Exception e)
        {
            PromptMessage("Could not save map!");
        }
    }
    private static void Load(Button[] directories)
    {
        try
        {
            var map = new TilemapPack(File.ReadAllBytes(directories[0].Text));

            editor.MapsEditor.Clear();
            for (var i = 0; i < map.Count; i++)
                editor.MapsEditor.Add(map[i]);
        }
        catch (Exception e)
        {
            PromptMessage("Could not load map!");
        }
    }
#endregion
}