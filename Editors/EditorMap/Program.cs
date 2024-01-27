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

    internal static Menu menu;

    static Program()
    {
        var (mw, mh) = (50, 50);

        editor = new(title: "Pure - Map Editor");
        editor.MapsEditor.Clear();
        editor.MapsEditor.Add(new Tilemap((mw, mh)));
        editor.MapsEditor.ViewSize = (mw, mh);

        tilePalette = new(editor);
        inspector = new(editor, tilePalette);

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
        {
            Size = (9, 5),
            IsHidden = true
        };
        menu.OnItemInteraction(Interaction.Trigger, btn =>
        {
            menu.IsHidden = true;
            menu.IsDisabled = true;
            var index = menu.IndexOf(btn);

            if (index == 1) // save map
            {
                var layers = inspector.layers;
                var bytes = new List<byte>();

                PutInt(bytes, layers.Count);
                for (var i = 0; i < layers.Count; i++)
                    PutString(bytes, layers[i].Text);

                bytes.AddRange(editor.MapsEditor.ToBytes());
                editor.PromptFileSave(bytes.ToArray());
            }
            else if (index == 3) // load tileset
                editor.PromptTileset(
                    onSuccess: (layer, map) =>
                    {
                        layer.Offset = (755, 340);
                        tilePalette.layer = layer;
                        tilePalette.map = map;
                    },
                    onFail: () => tilePalette.Create(tilePalette.layer.TilesetSize));
            else if (index == 4) // load map
                editor.PromptLoadMap(layers =>
                {
                    inspector.layers.Clear();
                    foreach (var layer in layers)
                        inspector.layers.Add(new Button { Text = layer });
                });
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