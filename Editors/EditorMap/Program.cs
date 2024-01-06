global using System.Diagnostics.CodeAnalysis;
global using Pure.Editors.EditorBase;
global using Pure.Engine.Tilemap;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Utilities;
global using Pure.Engine.Window;
global using static Pure.Tools.Tilemapper.TilemapperUserInterface;
global using System.Text;

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

        const int BACK = (int)Editor.LayerMapsUi.PromptBack;

        mapSaveName = new()
        {
            Size = (20, 1),
            Value = "cool.tmap",
            IsSingleLine = true
        };
        mapSaveName.OnDisplay(() => editor.MapsUi.SetInputBox(mapSaveName, BACK));

        editor.MapFileViewer.FilesAndFolders.OnItemInteraction(Interaction.DoubleTrigger, btn =>
        {
            if (editor.MapFileViewer.IsSelectingFolders || editor.MapFileViewer.IsFolder(btn))
                return;

            editor.Prompt.Close();
            var layers = editor.LoadMap();
            LoadLayers(layers);
        });

        CreateMenu();
    }

    [MemberNotNull(nameof(menu))]
    private static void CreateMenu()
    {
        menu = new(editor,
                "Save… ",
                " Tilemap",
                "Load… ",
                " Tileset",
                " Tilemap")
            { Size = (9, 5) };
        menu.OnItemInteraction(Interaction.Trigger, btn =>
        {
            menu.IsHidden = true;
            menu.IsDisabled = true;
            var index = menu.IndexOf(btn);

            if (index == 1) // save map
            {
                editor.Prompt.Text = "Select a Directory:";
                editor.MapFileViewer.IsSelectingFolders = true;
                editor.Prompt.Open(editor.MapFileViewer, i =>
                {
                    editor.Prompt.Close();

                    if (i != 0)
                        return;

                    editor.Prompt.Text = "Provide a File Name:";
                    editor.Prompt.Open(mapSaveName, btnIndex =>
                    {
                        editor.Prompt.Close();
                        if (btnIndex == 0)
                            Save(mapSaveName.Value);
                    });
                });
            }
            else if (index == 3) // load tileset
                editor.OpenTilesetPrompt(
                    onSuccess: (layer, map) =>
                    {
                        layer.Offset = (755, 340);
                        tilePalette.layer = layer;
                        tilePalette.map = map;
                    },
                    onFail: () => tilePalette.Create(tilePalette.layer.TilesetSize));
            else if (index == 4) // load map
            {
                editor.Prompt.Text = "Select a Map File:";
                editor.MapFileViewer.IsSelectingFolders = false;
                editor.Prompt.Open(editor.MapFileViewer, i =>
                {
                    editor.Prompt.Close();

                    if (i != 0)
                        return;

                    var layers = editor.LoadMap();
                    LoadLayers(layers);
                });
            }
        });

        Mouse.Button.Right.OnPress(() =>
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

    private static void Save(string name)
    {
        try
        {
            var selectedPaths = editor.MapFileViewer.SelectedPaths;
            var directory = selectedPaths.Length == 1 ?
                selectedPaths[0] :
                editor.MapFileViewer.CurrentDirectory;
            var layers = inspector.layers;
            var bytes = new List<byte>();
            var path = $"{directory}{Path.DirectorySeparatorChar}{name}";

            PutInt(bytes, layers.Count);
            for (var i = 0; i < layers.Count; i++)
                PutString(bytes, layers[i].Text);

            bytes.AddRange(editor.MapsEditor.ToBytes());

            File.WriteAllBytes(path, bytes.ToArray());
        }
        catch (Exception)
        {
            editor.PromptMessage("Could not save map!");
        }
    }
    private static void LoadLayers(string[] layers)
    {
        inspector.layers.Clear();
        foreach (var layer in layers)
            inspector.layers.Add(new Button { Text = layer });
    }

    private static void PutInt(List<byte> intoBytes, int value)
    {
        intoBytes.AddRange(BitConverter.GetBytes(value));
    }
    private static void PutString(List<byte> intoBytes, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        PutInt(intoBytes, bytes.Length);
        intoBytes.AddRange(bytes);
    }
#endregion
}