global using System.Diagnostics.CodeAnalysis;
global using Pure.Editors.Base;
global using Pure.Engine.Tilemap;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Utilities;
global using Pure.Engine.Window;
global using static Pure.Tools.Tilemapper.TilemapperUI;
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
        editor.OnUpdateLate += () => tilePalette.Update(inspector);
        editor.Run();
    }

#region Backend
    private static readonly Editor editor;
    private static readonly Inspector inspector;
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
                editor.PromptTileset(
                    (layer, map) =>
                    {
                        layer.Offset = (755, 340);
                        tilePalette.layer = layer;
                        tilePalette.map = map;
                    },
                    () => tilePalette.Create(tilePalette.layer.AtlasTileCount));

            if (index == 3) // save map
                editor.PromptFileSave(Save());
            else if (index == 4) // load map
                editor.PromptLoadMap(InitializeLayers);
            else if (index == 5)
                Window.Clipboard = Convert.ToBase64String(Save());
            else if (index == 6)
                editor.PromptLoadMapBase64(InitializeLayers);
        });

        Mouse.Button.Right.OnPress(() =>
        {
            if (editor.Prompt.IsHidden == false || inspector.IsHovered)
                return;

            var (mx, my) = editor.LayerUi.PixelToPosition(Mouse.CursorPosition);
            menu.IsHidden = false;
            menu.IsDisabled = false;
            menu.Position = ((int)mx + 1, (int)my + 1);
        });

        void InitializeLayers(string[] layers)
        {
            inspector.layers.Items.Clear();
            inspector.layersVisibility.Items.Clear();
            editor.MapsEditorVisible.Clear();
            foreach (var layer in layers)
            {
                inspector.layers.Items.Add(new() { Text = layer });
                inspector.layersVisibility.Items.Add(new());
                editor.MapsEditorVisible.Add(true);
            }
        }
    }

    private static byte[] Save()
    {
        try
        {
            var layers = inspector.layers;
            var bytes = new List<byte>();

            var maps = Decompress(editor.MapsEditor.ToBytes());
            bytes.AddRange(maps);

            // hijack the end of the file to save some extra info
            // should be ignored by the engine but not by the editor
            PutInt(bytes, layers.Count);
            for (var i = 0; i < layers.Count; i++)
                PutString(bytes, layers.Items[i].Text);

            return Compress(bytes.ToArray());
        }
        catch (Exception)
        {
            editor.PromptMessage("Saving failed!");
            return Array.Empty<byte>();
        }
    }

    private static void UpdateEditor()
    {
        editor.IsDisabledViewInteraction = inspector.IsHovered;
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
        {
            stream.Write(data, 0, data.Length);
        }

        return output.ToArray();
    }
    private static byte[] Decompress(byte[] data)
    {
        var input = new MemoryStream(data);
        var output = new MemoryStream();
        using (var stream = new DeflateStream(input, CompressionMode.Decompress))
        {
            stream.CopyTo(output);
        }

        return output.ToArray();
    }
#endregion
}