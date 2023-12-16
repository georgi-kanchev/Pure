global using System.Diagnostics.CodeAnalysis;
global using Pure.Editors.EditorBase;
global using Pure.Engine.Tilemap;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Utilities;
global using Pure.Engine.Window;
global using static Pure.Tools.Tilemapper.TilemapperUserInterface;

namespace Pure.Editors.EditorMap;

using System.Text;
using Tools.TiledLoader;

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
            if (mapSaveLoad.IsSelectingFolders || mapSaveLoad.IsFolder(btn))
                return;

            editor.Prompt.Close();
            Load(mapSaveLoad);
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
                            Save(mapSaveLoad, mapSaveName.Value);
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

                    Load(mapSaveLoad);
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

    private static void Save(FileViewer fileViewer, string name)
    {
        try
        {
            var selectedPaths = fileViewer.SelectedPaths;
            var directory = selectedPaths.Length == 1 ? selectedPaths[0] : fileViewer.CurrentDirectory;
            var layers = inspector.layers;
            var bytes = new List<byte>();
            var path = $"{directory}{Path.DirectorySeparatorChar}{name}";

            PutInt(bytes, layers.Count);
            for (var i = 0; i < layers.Count; i++)
                PutString(bytes, layers[i].Text);

            bytes.AddRange(editor.MapsEditor.ToBytes());

            File.WriteAllBytes(path, bytes.ToArray());
        }
        catch (Exception e)
        {
            editor.PromptMessage("Could not save map!");
        }
    }
    private static void Load(FileViewer fileViewer)
    {
        try
        {
            var selectedPaths = fileViewer.SelectedPaths;
            var file = selectedPaths.Length == 1 ? selectedPaths[0] : fileViewer.CurrentDirectory;
            var maps = default(TilemapPack);

            if (Path.GetExtension(file) == ".tmx" && file != null)
            {
                maps = TiledLoader.Load(file, out var layers);
                inspector.layers.Clear();
                foreach (var layer in layers)
                    inspector.layers.Add(new Button { Text = $"{layer}" });
            }
            else
            {
                var bytes = File.ReadAllBytes($"{file}");
                var byteOffset = 0;

                var layerCount = GrabInt(bytes, ref byteOffset);
                inspector.layers.Clear();
                for (var i = 0; i < layerCount; i++)
                {
                    var layerName = GrabString(bytes, ref byteOffset);
                    inspector.layers.Add(new Button { Text = layerName });
                }

                maps = new(bytes[byteOffset..]);
            }

            editor.MapsEditor.Clear();
            for (var i = 0; i < maps.Count; i++)
                editor.MapsEditor.Add(maps[i]);
        }
        catch (Exception e)
        {
            editor.PromptMessage("Could not load map!");
        }
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
    private static string GrabString(byte[] fromBytes, ref int byteOffset)
    {
        var textBytesLength = GrabInt(fromBytes, ref byteOffset);
        var bText = GetBytes(fromBytes, textBytesLength, ref byteOffset);
        return Encoding.UTF8.GetString(bText);
    }
    private static int GrabInt(byte[] fromBytes, ref int byteOffset)
    {
        return BitConverter.ToInt32(GetBytes(fromBytes, 4, ref byteOffset));
    }
    private static byte[] GetBytes(byte[] fromBytes, int amount, ref int byteOffset)
    {
        var result = fromBytes[byteOffset..(byteOffset + amount)];
        byteOffset += amount;
        return result;
    }
#endregion
}