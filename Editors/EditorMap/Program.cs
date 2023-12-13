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
    private static readonly InputBox mapSaveName;

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

        mapSaveName = new()
        {
            Size = (20, 1),
            Value = "cool.tmap",
            IsSingleLine = true
        };
        mapSaveName.OnDisplay(() => editor.MapsUi.SetInputBox(mapSaveName, BACK));

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
            {
                editor.Prompt.Close();
                Load(mapSaveLoad.SelectedPaths);
            }
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

                    editor.Prompt.Text = "Provide a File Name:";
                    editor.Prompt.Open(mapSaveName, btnIndex =>
                    {
                        editor.Prompt.Close();
                        if (btnIndex == 0)
                            Save(mapSaveLoad.SelectedPaths, mapSaveName.Value);
                    });
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

                    Load(mapSaveLoad.SelectedPaths);
                });
            }
        });

        Mouse.OnButtonPress(Mouse.Button.Right, () =>
        {
            if (editor.Prompt.IsHidden == false || inspector.IsHovered)
                return;

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

    private static void Save(string[] selectedPaths, string name)
    {
        try
        {
            File.WriteAllBytes($"{selectedPaths[0]}{Path.DirectorySeparatorChar}{name}",
                editor.MapsEditor.ToBytes());
        }
        catch (Exception e)
        {
            editor.PromptMessage("Could not save map!");
        }
    }
    private static void Load(string[] selectedPaths)
    {
        try
        {
            var bytes = File.ReadAllBytes(selectedPaths[0]);
            var maps = new TilemapPack(bytes);

            editor.MapsEditor.Clear();
            for (var i = 0; i < maps.Count; i++)
                editor.MapsEditor.Add(maps[i]);
        }
        catch (Exception e)
        {
            editor.PromptMessage("Could not load map!");
        }
    }
#endregion
}