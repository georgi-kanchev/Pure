global using System.Diagnostics.CodeAnalysis;
global using Pure.Editors.Base;
global using Pure.Engine.Tilemap;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Utilities;
global using Pure.Engine.Window;
global using Pure.Tools.Tilemap;
global using System.Text;

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

            if (inspector.layers.Items.Count > 0)
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
            // hijack the end of the file to save some extra info
            // should be ignored by the engine but not by the editor

            var bytes = editor.MapsEditor.ToBytes().Compress().ToList();
            var layers = inspector.layers.Items;
            var layerNames = new List<string>();

            foreach (var layer in layers)
                layerNames.Add(layer.Text);

            var layersBytes = layerNames.ToArray().ToBytes();
            bytes.AddRange(layersBytes);

            var generatorBytes = terrainPanel.generator.ToBytes();
            bytes.AddRange(generatorBytes);

            bytes.AddRange(BitConverter.GetBytes(layersBytes.Length));
            bytes.AddRange(BitConverter.GetBytes(generatorBytes.Length));

            // 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

            return bytes.ToArray();
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
#endregion
}