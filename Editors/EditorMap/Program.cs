global using Pure.Engine.Tilemap;
global using Pure.Engine.UserInterface;
global using static Pure.Default.Tilemapper.TilemapperUserInterface;
global using Pure.Editors.EditorBase;
global using Pure.Engine.Window;
global using System.Diagnostics.CodeAnalysis;
global using Pure.Engine.Utilities;

namespace Pure.Editors.EditorMap;

public static class Program
{
    static Program()
    {
        var (mw, mh) = (50, 50);

        editor = new(title: "Pure - Map Editor", mapSize: (mw, mh), viewSize: (mw, mh));

        editor.MapsEditor.Clear();
        editor.MapsEditor.Add(new Tilemap((mw, mh)));
        editor.MapsEditor.View = (0, 0, mw, mh);

        CreateInspector();
        CreateMenu();
        CreatePrompts();
    }

    public static void Run()
    {
        editor.OnUpdateLate += UpdatePalette;
        editor.Run();
    }

#region Backend
    private static string? promptFilePath;
    private static (int w, int h) promptTileSize, promptTileGap;
    private static int promptTileFull;

    private static readonly Editor editor;
    private static Menu menu;
    private static List layers;
    private static InputBox rename, create, promptPair;
    private static FileViewer promptFileViewer;
    private static Stepper promptStepper;
    //private static Tilemap palette;

    [MemberNotNull(nameof(layers), nameof(create), nameof(rename))]
    private static void CreateInspector()
    {
        var (w, h) = (16, editor.MapsUi.Size.height);
        var inspector = new Panel()
        {
            Text = "",
            Size = (w, h),
            IsMovable = false,
            IsResizable = false
        };
        inspector.OnDisplay(() => editor.MapsUi.SetPanel(inspector));
        inspector.Align((1f, 0.5f));

        //========

        layers = new(itemCount: 1) { ItemSize = (13, 1), ItemGap = 1 };
        layers.OnDisplay(() => editor.MapsUi.SetList(layers));
        layers.OnItemDisplay(item => editor.MapsUi.SetListItem(layers, item));
        layers[0].Text = "Layer1";

        create = new() { Value = "", Placeholder = "Create…", IsSingleLine = true };
        create.OnSubmit(OnLayerCreate);
        create.OnDisplay(() => editor.MapsUi.SetInputBox(create));

        rename = new() { Value = "", Placeholder = "Rename…", IsSingleLine = true };
        rename.OnSubmit(OnLayersRename);
        rename.OnDisplay(() => editor.MapsUi.SetInputBox(rename));

        var remove = new Button() { Text = "Remove" };
        remove.OnInteraction(Interaction.Trigger, OnLayersRemove);
        remove.OnUpdate(() =>
        {
            remove.IsHidden = layers.ItemsSelected.Length == 0;
            remove.IsDisabled = remove.IsHidden;
        });
        remove.OnDisplay(() => editor.MapsUi.SetButton(remove, zOrder: 1));

        //========

        var inspectorItems = new Block?[]
        {
            layers, null, create, null, rename, null, remove
        };
        var layout = new Layout((inspector.Position.x + 1, inspector.Position.y + 1))
            { Size = (w - 2, h - 2) };
        layout.OnDisplaySegment((segment, i) => UpdateInspectorItem(i, inspectorItems, segment));

        layout.Cut(0, Side.Bottom, 0.7f);
        for (var i = 1; i < 6; i++)
            layout.Cut(i, Side.Bottom, 0.95f);
        layout.Cut(6, Side.Bottom, 0.85f);

        editor.Ui.Add(inspector, layout, create, rename, remove, layers);
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
            "  Collisions") { Size = (12, 7) };
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
            var (mx, my) = (0, 0); //Mouse.PixelToWorld(Mouse.CursorPosition);
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

        promptStepper = new() { Range = (0, int.MaxValue), Size = (20, 2) };
        promptStepper.OnDisplay(() => maps.SetStepper(promptStepper, BACK));

        promptPair = new()
        {
            Size = (20, 1),
            SymbolGroup = SymbolGroup.Digits | SymbolGroup.Space,
            Value = ""
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
    }

    private static void PromptTileSet()
    {
        editor.Prompt.Text = "Select Image File:";
        editor.Prompt.Open(promptFileViewer, i =>
        {
            editor.Prompt.Close();

            var paths = promptFileViewer.SelectedPaths;
            if (i != 0 || paths.Length == 0)
                return;

            promptFilePath = promptFileViewer.SelectedPaths[0];
            PromptTileSize();
        });
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

            var split = promptPair.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2)
                return;

            promptTileSize = ((int)split[0].ToNumber(), (int)split[1].ToNumber());
            PromptTileGap();
        });
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

            var split = promptPair.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2)
                return;

            promptTileGap = ((int)split[0].ToNumber(), (int)split[1].ToNumber());
            PromptTileFull();
        });
    }
    private static void PromptTileFull()
    {
        editor.Prompt.Text = "Provide Full Tile Id";
        editor.Prompt.Open(promptStepper, i =>
        {
            editor.Prompt.Close();

            if (i != 0)
                return;

            promptTileFull = (int)promptStepper.Value;
            PromptTileSetSuccessful();
        });
    }
    private static void PromptTileSetSuccessful()
    {
        //Window.LayerAdd(1, promptFilePath, promptTileSize, promptTileGap, promptTileFull);
        //Window.LayerAdd(3, promptFilePath, promptTileSize, promptTileGap, promptTileFull);
    }

    private static void OnLayerCreate()
    {
        var item = new Button { Text = create.Value };
        var size = editor.MapsEditor.Size;
        var isEmpty = editor.MapsEditor.Count == 0;
        size = isEmpty ? (50, 50) : size;

        layers.Add(item);
        editor.MapsEditor.Add(new Tilemap(size));
        create.Value = "";

        if (isEmpty)
            editor.MapsEditor.View = (0, 0, 50, 50);
    }
    private static void OnLayersRename()
    {
        var selected = layers.ItemsSelected;
        foreach (var item in selected)
            item.Text = rename.Value;
        rename.Value = "";
    }
    private static void OnLayersRemove()
    {
        var selected = layers.ItemsSelected;
        foreach (var item in selected)
        {
            var index = layers.IndexOf(item);
            editor.MapsEditor.Remove(editor.MapsEditor[index]);
            layers.Remove(item);
        }
    }

    private static void UpdateInspectorItem(
        int i,
        Block?[] inspectorItems,
        (int x, int y, int width, int height) segment)
    {
        if (i >= inspectorItems.Length)
            return;

        var items = inspectorItems[i];
        if (items == null)
            return;

        items.Position = (segment.x, segment.y);
        items.Size = (segment.width, segment.height);
    }

    private static void UpdatePalette()
    {
        //Window.LayerCurrent = 3;
    }
#endregion
}