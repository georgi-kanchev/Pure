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