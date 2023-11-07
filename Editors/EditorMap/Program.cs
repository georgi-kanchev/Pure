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
        editor.OnUpdateEditor += UpdateEditor;
        editor.OnUpdateLate += UpdatePalette;
        editor.Run();
    }

#region Backend
    private static readonly Editor editor;
    private static Panel inspector;
    private static Menu menu;
    private static List layers;
    private static InputBox promptPair;
    private static FileViewer promptFileViewer;
    private static Stepper promptStepper;

    private static Palette paletteColor;
    private static Tilemap paletteMap;
    private static Scroll paletteScrollV, paletteScrollH;
    private static readonly Layer paletteLayer;
    private static (int x, int y) paletteMousePos;

    static Program()
    {
        var (mw, mh) = (50, 50);

        editor = new(title: "Pure - Map Editor", mapSize: (mw, mh), viewSize: (mw, mh));

        editor.MapsEditor.Clear();
        editor.MapsEditor.Add(new Tilemap((mw, mh)));
        editor.MapsEditor.ViewSize = (mw, mh);

        CreatePalette((26, 26));
        CreateInspector();
        CreateMenu();
        CreatePrompts();

        paletteLayer = new(paletteMap.ViewSize) { Zoom = 3.8f, Offset = (755, 340) };
    }

    [MemberNotNull(nameof(paletteMap))]
    private static void CreatePalette((int width, int height) size)
    {
        paletteMap = new(size) { ViewSize = (10, 10) };

        for (var i = 0; i < size.height; i++)
            for (var j = 0; j < size.width; j++)
                paletteMap.SetTile((j, i), new Indices(i, j).ToIndex(size.width));
    }

    [MemberNotNull(nameof(inspector), nameof(layers), nameof(paletteScrollH),
        nameof(paletteScrollV), nameof(paletteColor))]
    private static void CreateInspector()
    {
        var (w, h) = (16, editor.MapsUi.Size.height);
        inspector = new Panel
        {
            Text = "",
            Size = (w, h),
            IsMovable = false,
            IsResizable = false
        };
        inspector.OnDisplay(() => editor.MapsUi.SetPanel(inspector));
        inspector.Align((1f, 0.5f));

        //========

        layers = new(itemCount: 1) { ItemSize = (13, 1) };
        layers.OnDisplay(() => editor.MapsUi.SetList(layers));
        layers.OnItemDisplay(item => editor.MapsUi.SetListItem(layers, item));
        layers[0].Text = "Layer1";

        var create = new Button();
        create.OnInteraction(Interaction.Trigger, LayerCreate);
        create.OnDisplay(() =>
            editor.MapsUi.SetButtonIcon(create, new(Tile.CURSOR_CROSSHAIR, Color.Gray), 1));

        var remove = new Button();
        remove.OnInteraction(Interaction.Trigger, LayersRemove);
        remove.OnUpdate(() => ShowWhenLayerSelected(remove));
        remove.OnDisplay(() =>
            editor.MapsUi.SetButtonIcon(remove, new(Tile.PUNCTUATION_DASH, Color.Gray), 1));

        var rename = new InputBox() { Value = "", Placeholder = "Rename…", IsSingleLine = true };
        rename.OnSubmit(() =>
        {
            LayersRename(rename.Value);
            rename.Value = "";
        });
        rename.OnUpdate(() => ShowWhenLayerSelected(rename));
        rename.OnDisplay(() => editor.MapsUi.SetInputBox(rename));

        var up = new Button();
        up.OnInteraction(Interaction.Trigger, LayersUp);
        up.OnUpdate(() => ShowWhenLayerSelected(up));
        up.OnDisplay(() => editor.MapsUi.SetButtonIcon(up, new(Tile.ARROW, Color.Gray, 3), 1));

        var down = new Button();
        down.OnInteraction(Interaction.Trigger, LayersDown);
        down.OnUpdate(() => ShowWhenLayerSelected(down));
        down.OnDisplay(() => editor.MapsUi.SetButtonIcon(down, new(Tile.ARROW, Color.Gray, 1), 1));

        //========

        paletteColor = new();
        paletteColor.OnDisplay(() =>
        {
            editor.MapsUi.SetPalette(paletteColor, zOrder: 1);
            editor.MapsUi.SetSlider(paletteColor.Opacity, zOrder: 1);
            editor.MapsUi.SetPages(paletteColor.Brightness, zOrder: 1);
        });
        paletteColor.OnColorSampleDisplay((btn, color) =>
        {
            editor.MapsUi[1].SetTile(btn.Position, new Tile(Tile.SHADE_OPAQUE, color));
        });
        paletteColor.Brightness.OnItemDisplay(btn =>
            editor.MapsUi.SetPagesItem(paletteColor.Brightness, btn));

        paletteScrollH = new(isVertical: false) { Size = (14, 1) };
        paletteScrollV = new(isVertical: true) { Size = (1, 14) };
        paletteScrollH.OnDisplay(() => editor.MapsUi.SetScroll(paletteScrollH));
        paletteScrollV.OnDisplay(() => editor.MapsUi.SetScroll(paletteScrollV));

        //========

        var inspectorItems = new Block?[]
        {
            layers, create, null, remove, up, down, null, rename,
            null, paletteScrollV, paletteScrollH, null, paletteColor
        };
        var layout = new Layout((inspector.Position.x + 1, inspector.Position.y + 1))
            { Size = (w - 2, h - 2) };
        layout.OnDisplaySegment((segment, i) => UpdateInspectorItem(i, inspectorItems, segment));

        layout.Cut(0, Side.Bottom, 0.85f);
        layout.Cut(1, Side.Bottom, 0.95f);
        layout.Cut(1, Side.Right, 0.9f);
        layout.Cut(3, Side.Right, 0.9f);
        layout.Cut(4, Side.Right, 0.9f);
        layout.Cut(5, Side.Right, 0.9f);
        layout.Cut(2, Side.Top, 0.05f);

        layout.Cut(2, Side.Bottom, 0.4f);
        layout.Cut(8, Side.Right, 0.05f);
        layout.Cut(8, Side.Bottom, 0.05f);

        layout.Cut(2, Side.Bottom, 0.02f);
        layout.Cut(2, Side.Bottom, 0.15f);

        editor.Ui.Add(inspector, layout, create, up, down, rename, remove,
            paletteColor, paletteScrollV, paletteScrollH, layers);
    }
    [MemberNotNull(nameof(menu))]
    private static void CreateMenu()
    {
        menu = new(editor,
                "Save… ",
                "  Map",
                "  Collisions",
                "Load… ",
                "  Tileset",
                "  Map",
                "  Collisions")
            { Size = (12, 7) };
        menu.OnItemInteraction(Interaction.Trigger, btn =>
        {
            menu.IsHidden = true;
            menu.IsDisabled = true;
            var index = menu.IndexOf(btn);
            if (index == 4)
                PromptTileSet();
        });

        Mouse.OnButtonPress(Mouse.Button.Right, () =>
        {
            var (mx, my) = editor.LayerUi.PixelToWorld(Mouse.CursorPosition);
            menu.IsHidden = false;
            menu.IsDisabled = false;
            menu.Position = ((int)mx + 1, (int)my + 1);
        });
    }
    [MemberNotNull(nameof(promptPair), nameof(promptFileViewer), nameof(promptStepper))]
    private static void CreatePrompts()
    {
        const int BACK = (int)Editor.LayerMapsUi.PromptBack;
        const int MIDDLE = (int)Editor.LayerMapsUi.PromptMiddle;
        var maps = editor.MapsUi;

        promptStepper = new() { Range = (0, int.MaxValue), Size = (22, 2) };
        promptStepper.OnDisplay(() => maps.SetStepper(promptStepper, BACK));

        promptPair = new()
        {
            Size = (20, 1),
            SymbolGroup = SymbolGroup.Digits | SymbolGroup.Space,
            Value = "",
            IsSingleLine = true
        };
        promptPair.OnDisplay(() => maps.SetInputBox(promptPair, BACK));

        promptFileViewer = new()
        {
            FilesAndFolders = { IsSingleSelecting = true },
            Size = (21, 10)
        };
        promptFileViewer.OnDisplay(() => maps.SetFileViewer(promptFileViewer, BACK));
        promptFileViewer.FilesAndFolders.OnItemDisplay(btn =>
            maps.SetFileViewerItem(promptFileViewer, btn, MIDDLE));

        Keyboard.OnKeyPress(Keyboard.Key.Enter, asText =>
        {
            if (editor.Prompt.IsHidden)
                return;

            var text = editor.Prompt.Text;
            if (text.Contains("Image File"))
                PromptTilesetAccept();
            else if (text.Contains("Tile Size"))
                PromptTileSizeAccept();
            else if (text.Contains("Tile Gap"))
                PromptTileGapAccept();
            else if (text.Contains("Full Tile Id"))
                PromptTileFullAccept();
        });
    }

    private static void PromptMessage(string msg)
    {
        editor.Prompt.Text = msg;
        editor.Prompt.ButtonCount = 1;
        editor.Prompt.Open(onButtonTrigger: _ => editor.Prompt.Close());
        editor.Prompt.ButtonCount = 2;
    }
    private static void PromptTileSet()
    {
        editor.Prompt.Text = "Select Image File:";
        editor.Prompt.Open(promptFileViewer, i =>
        {
            editor.Prompt.Close();

            if (i != 0)
                return;

            PromptTilesetAccept();
        });
    }
    private static void PromptTilesetAccept()
    {
        editor.Prompt.Close();
        var paths = promptFileViewer.SelectedPaths;
        if (paths.Length == 0)
        {
            PromptMessage("Could not load image!");
            return;
        }

        var path = promptFileViewer.SelectedPaths[0];
        editor.LayerMap.TilesetPath = path;
        paletteLayer.TilesetPath = path;

        if (editor.LayerMap.TilesetPath == "default")
        {
            CreatePalette(paletteLayer.TilesetSize);
            PromptMessage("Could not load image!");
            return;
        }

        PromptTileSize();
    }
    private static void PromptTileSize()
    {
        editor.Prompt.Text = $"Enter Tile Size{Environment.NewLine}" +
                             $"example: '16 16'";
        editor.Prompt.Open(promptPair, i =>
        {
            editor.Prompt.Close();

            if (i != 0)
                return;

            PromptTileSizeAccept();
        });
    }
    private static void PromptTileSizeAccept()
    {
        editor.Prompt.Close();
        var split = promptPair.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 2)
        {
            PromptMessage("Only 2 values allowed!");
            return;
        }

        var result = ((int)split[0].ToNumber(), (int)split[1].ToNumber());
        editor.LayerMap.TileSize = result;
        paletteLayer.TileSize = result;
        PromptTileGap();
    }
    private static void PromptTileGap()
    {
        editor.Prompt.Text = $"Enter Tile Gap{Environment.NewLine}" +
                             $"example: '1 1'";
        editor.Prompt.Open(promptPair, i =>
        {
            editor.Prompt.Close();

            if (i != 0)
                return;

            PromptTileGapAccept();
        });
    }
    private static void PromptTileGapAccept()
    {
        editor.Prompt.Close();
        var split = promptPair.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        if (split.Length != 2)
        {
            PromptMessage("Only 2 values allowed!");
            return;
        }

        var result = ((int)split[0].ToNumber(), (int)split[1].ToNumber());
        editor.LayerMap.TileGap = result;
        paletteLayer.TileGap = result;

        CreatePalette(paletteLayer.TilesetSize);

        PromptTileFull();
    }
    private static void PromptTileFull()
    {
        editor.Prompt.Text = "Provide Full Tile Id";
        editor.Prompt.Open(promptStepper, i =>
        {
            editor.Prompt.Close();

            if (i != 0)
                return;

            PromptTileFullAccept();
        });
    }
    private static void PromptTileFullAccept()
    {
        editor.Prompt.Close();
        editor.LayerMap.TileIdFull = (int)promptStepper.Value;
        paletteLayer.TileIdFull = (int)promptStepper.Value;
    }

    private static void LayerCreate()
    {
        var item = new Button { Text = "NewLayer" };
        var size = editor.MapsEditor.Size;
        var isEmpty = editor.MapsEditor.Count == 0;
        size = isEmpty ? (50, 50) : size;

        layers.Add(item);
        editor.MapsEditor.Add(new Tilemap(size));

        if (isEmpty)
            editor.MapsEditor.ViewSize = (50, 50);
    }
    private static void LayersRename(string name)
    {
        var selected = layers.ItemsSelected;
        foreach (var item in selected)
            item.Text = name;
    }
    private static void LayersRemove()
    {
        var selected = layers.ItemsSelected;
        foreach (var item in selected)
        {
            var index = layers.IndexOf(item);
            editor.MapsEditor.Remove(editor.MapsEditor[index]);
            layers.Remove(item);
        }
    }
    private static void LayersUp()
    {
        layers.Shift(-1, layers.ItemsSelected);
    }
    private static void LayersDown()
    {
        layers.Shift(1, layers.ItemsSelected);
    }

    private static void UpdateInspectorItem(
        int i,
        Block?[] inspectorItems,
        (int x, int y, int width, int height) segment)
    {
        //editor.MapsUi.SetLayoutSegment(segment, i, true, 5);

        if (i >= inspectorItems.Length)
            return;

        if (i == 11 && Mouse.IsHovering(paletteLayer) && editor.Prompt.IsHidden)
        {
            var (mx, my) = paletteMousePos;
            var index = new Indices(my, mx).ToIndex(paletteMap.Size.width);
            editor.MapsUi[(int)Editor.LayerMapsUi.Front].SetTextRectangle(
                position: (segment.x, segment.y),
                size: (segment.width, segment.height),
                text: $"{index} ({mx} {my})");
            return;
        }

        var items = inspectorItems[i];
        if (items == null)
            return;

        items.Position = (segment.x, segment.y);
        items.Size = (segment.width, segment.height);
    }

    private static void UpdateEditor()
    {
        editor.IsDisabledViewInteraction = inspector.IsHovered;
    }
    private static void UpdatePalette()
    {
        if (editor.Prompt.IsHidden == false)
            return;

        paletteLayer.Clear();

        var (tw, th) = paletteLayer.TilesetSize;
        paletteMap.ViewSize = (Math.Min(10, tw), Math.Min(10, th));
        paletteLayer.TilemapSize = paletteMap.ViewSize;
        var (mw, mh) = paletteMap.Size;
        var (vw, vh) = paletteMap.ViewSize;
        paletteScrollH.Step = 1f / (mw - vw);
        paletteScrollV.Step = 1f / (mh - vh);
        var w = (int)MathF.Round(paletteScrollH.Slider.Progress * (mw - vw));
        var h = (int)MathF.Round(paletteScrollV.Slider.Progress * (mh - vh));
        paletteMap.ViewPosition = (w, h);

        var (mx, my) = paletteLayer.PixelToWorld(Mouse.CursorPosition);
        paletteMousePos = ((int)mx + w, (int)my + h);

        var view = paletteMap.ViewUpdate();
        paletteLayer.DrawTilemap(view);

        var isHovering = Mouse.IsHovering(paletteLayer);
        if (isHovering)
            paletteLayer.DrawTile(
                position: ((int)mx, (int)my),
                tile: new Tile(paletteLayer.TileIdFull, new Color(50, 100, 255, 150)));

        Window.DrawLayer(paletteLayer);
    }

    private static void ShowWhenLayerSelected(Block block)
    {
        block.IsHidden = layers.ItemsSelected.Length == 0;
        block.IsDisabled = block.IsHidden;
    }
#endregion
}