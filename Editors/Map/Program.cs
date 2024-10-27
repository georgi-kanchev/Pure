global using System.Diagnostics.CodeAnalysis;

global using Pure.Editors.Base;
global using Pure.Engine.Tilemap;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Utilities;
global using Pure.Engine.Window;
global using Pure.Tools.Tilemap;

global using System.Text;
using System.IO.Compression;

namespace Pure.Editors.Map;

public static class Program
{
    public static void Run()
    {
        Window.SetIconFromTile(editor.LayerUi, (Tile.ICON_SKY_STARS, Color.Red.ToDark()),
            (Tile.FULL, Color.Brown.ToBright()));

        editor.OnUpdateUi += tilePalette.TryDraw;
        editor.OnUpdateEditor += UpdateEditor;
        editor.OnUpdateLate += () => tilePalette.Update(inspector, terrainPanel);
        editor.Run();
    }

    #region Backend
    private static readonly Editor editor;
    private static readonly Inspector inspector;
    private static readonly TerrainPanel terrainPanel;
    private static readonly TilePalette tilePalette;

    internal static Menu menu;

    static Program()
    {
        var (mw, mh) = (50, 50);

        editor = new("Pure - Map Editor");
        editor.MapsEditor.Tilemaps.Clear();
        editor.MapsEditor.Tilemaps.Add(new((mw, mh)));
        editor.MapsEditor.View = new(editor.MapsEditor.View.Position, (mw, mh));
        editor.MapsEditorVisible.Clear();
        editor.MapsEditorVisible.Add(true);

        tilePalette = new(editor);
        inspector = new(editor, tilePalette);
        terrainPanel = new(editor, inspector, tilePalette);

        CreateMenu();
    }

    [MemberNotNull(nameof(menu))]
    private static void CreateMenu()
    {
        menu = new(editor,
            "Graphics… ",
            " Load",
            "Map… ",
            " Save",
            " Load",
            " Copy",
            " Paste")
        {
            Size = (9, 7),
            IsHidden = true
        };
        menu.OnItemInteraction(Interaction.Trigger, btn =>
        {
            menu.IsHidden = true;
            menu.IsDisabled = true;
            var index = menu.Items.IndexOf(btn);

            if (index == 1) // load tileset
                editor.PromptTileset((layer, map) =>
                    {
                        var zoomFactor = 3.8f / layer.Zoom;
                        layer.Offset = (198f * zoomFactor, 88f * zoomFactor);
                        tilePalette.layer = layer;
                        tilePalette.map = map;
                    },
                    () => tilePalette.Create(tilePalette.layer.AtlasTileCount));

            if (index == 3) // save map
                editor.PromptFileSave(Save());
            else if (index == 4) // load map
                editor.PromptLoadMap(LoadMap);
            else if (index == 5)
                Window.Clipboard = Convert.ToBase64String(Save());
            else if (index == 6)
                editor.PromptLoadMapBase64(LoadMap);
        });

        Mouse.Button.Right.OnPress(() =>
        {
            if (editor.Prompt.IsHidden == false || inspector.IsHovered || terrainPanel.IsHovered)
                return;

            var (mx, my) = editor.LayerUi.PixelToPosition(Mouse.CursorPosition);
            menu.IsHidden = false;
            menu.IsDisabled = false;
            menu.Position = ((int)mx + 1, (int)my + 1);
        });

        void LoadMap(string[] layers, MapGenerator? gen)
        {
            inspector.layers.Items.Clear();
            inspector.layersVisibility.Items.Clear();
            editor.MapsEditorVisible.Clear();
            foreach (var layer in layers)
            {
                inspector.layers.Items.Add(new() { Text = layer });
                inspector.layersVisibility.Items.Add(new() { IsSelected = true });
                editor.MapsEditorVisible.Add(true);
            }

            inspector.layers.Select(inspector.layers.Items[0]);

            if (gen == null)
                return;

            terrainPanel.generator = gen;
            terrainPanel.UpdateUI();
        }
    }

    private static byte[] Save()
    {
        try
        {
            var layers = inspector.layers;
            var bytes = Decompress(editor.MapsEditor.ToBytes()).ToList();

            // hijack the end of the file to save some extra info
            // should be ignored by the engine but not by the editor
            PutInt(bytes, layers.Items.Count);
            for (var i = 0; i < layers.Items.Count; i++)
                PutString(bytes, layers.Items[i].Text);

            bytes.AddRange(terrainPanel.generator.ToBytes());

            return Compress(bytes.ToArray());
        }
        catch (Exception)
        {
            editor.PromptMessage("Saving failed!");
            return [];
        }
    }

    private static void UpdateEditor()
    {
        editor.IsDisabledViewInteraction = inspector.IsHovered || terrainPanel.IsHovered;
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

    private static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
            stream.Write(data, 0, data.Length);

        return output.ToArray();
    }
    private static byte[] Decompress(byte[] data)
    {
        var input = new MemoryStream(data);
        var output = new MemoryStream();
        using (var stream = new DeflateStream(input, CompressionMode.Decompress))
            stream.CopyTo(output);

        return output.ToArray();
    }
    #endregion
}