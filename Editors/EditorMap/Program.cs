global using Pure.Engine.Tilemap;
global using Pure.Engine.UserInterface;
global using static Pure.Default.RendererUserInterface.Default;
global using Pure.Editors.EditorBase;
global using Pure.Engine.Window;

namespace Pure.Editors.EditorMap;

public static class Program
{
    internal static readonly Editor editor;
    internal static readonly Panel inspector;
    internal static readonly Layout layout;
    internal static readonly List layers;
    internal static readonly InputBox rename, create;
    internal static readonly Button remove;
    internal static readonly Menu menu;

    static Program()
    {
        var (mw, mh) = (50, 50);

        editor = new(title: "Pure - Map Editor", mapSize: (mw, mh), viewSize: (mw, mh));

        editor.MapsEditor.Clear();
        editor.MapsEditor.Add(new Tilemap((mw, mh)));
        editor.MapsEditor.View = (0, 0, mw, mh);

        var (w, h) = (16, editor.MapsUi.Size.height);
        inspector = new()
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

        remove = new() { Text = "Remove" };
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
        layout = new((inspector.Position.x + 1, inspector.Position.y + 1)) { Size = (w - 2, h - 2) };
        layout.OnDisplaySegment((segment, i) => UpdateInspectorItem(i, inspectorItems, segment));

        layout.Cut(0, Side.Bottom, 0.7f);
        for (var i = 1; i < 6; i++)
            layout.Cut(i, Side.Bottom, 0.95f);
        layout.Cut(6, Side.Bottom, 0.85f);

        editor.Ui.Add(inspector, layout, create, rename, remove, layers);

        menu = new(editor,
            "Save… ",
            "  Map",
            "  Collisions",
            "Load… ",
            "  Tileset",
            "  Map",
            "  Collisions");
        menu.OnItemInteraction(Interaction.Trigger, btn =>
        {
            var index = menu.IndexOf(btn);
            if (index == 4)
            {
                //Window.
            }
        });
    }

    public static void Run()
    {
        editor.OnUpdateLate += UpdatePalette;
        editor.Run();
    }

#region Backend
    private static readonly Tilemap palette;

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
        Window.LayerCurrent = 3;
    }
#endregion
}