namespace Pure.Editors.EditorMap;

internal class Inspector : Panel
{
    public readonly List layers, layersVisibility;
    public readonly Palette paletteColor;
    public readonly Scroll paletteScrollV, paletteScrollH;
    public readonly Pages tools;

    public Inspector(Editor editor, TilePalette tilePalette)
    {
        this.editor = editor;

        var (w, h) = (16, editor.MapsUi.Size.height);
        Text = "";
        Size = (w, h);
        IsMovable = false;
        IsResizable = false;

        OnDisplay(() => editor.MapsUi.SetPanel(this));
        Align((1f, 0.5f));

        //========

        layers = new(itemCount: 1) { ItemSize = (13, 1) };
        layers.OnDisplay(() => editor.MapsUi.SetList(layers));
        layers.OnItemDisplay(item => editor.MapsUi.SetListItem(layers, item));
        layers[0].Text = "Layer1";
        layers.Select(0);
        layersVisibility = new(itemCount: 1) { ItemSize = (1, 1) };
        layersVisibility.OnItemDisplay(item =>
        {
            var tile = item.IsSelected ?
                new Tile(Tile.ICON_EYE_OPENED, Color.White) :
                new(Tile.ICON_EYE_CLOSED, Color.Gray);
            editor.MapsUi.SetButtonIcon(item, tile, 1);
        });
        layersVisibility.Select(0);
        layersVisibility.OnItemInteraction(Interaction.Trigger, item =>
        {
            var index = layersVisibility.IndexOf(item);
            editor.MapsEditorVisible[index] = item.IsSelected;
        });

        var create = new Button();
        create.OnInteraction(Interaction.Trigger, LayerCreate);
        create.OnDisplay(() =>
            editor.MapsUi.SetButtonIcon(create, new(Tile.CURSOR_CROSSHAIR, Color.Gray), 1));

        var remove = new Button();
        remove.OnInteraction(Interaction.Trigger, () =>
            editor.PromptYesNo("Remove selected layers?", LayersRemove));
        remove.OnUpdate(() =>
        {
            remove.IsHidden = layers.Count < 2 || layers.ItemsSelected.Length == 0;
            remove.IsDisabled = remove.IsHidden;
        });
        remove.OnDisplay(() =>
            editor.MapsUi.SetButtonIcon(remove, new(Tile.ICON_DELETE, Color.Gray), 1));

        var flush = new Button();
        flush.OnInteraction(Interaction.Trigger, () =>
            editor.PromptYesNo("Flush selected layers?", () =>
            {
                for (var i = 0; i < layers.ItemsSelected.Length; i++)
                    editor.MapsEditor[i].Flush();
            }));
        flush.OnUpdate(() => ShowWhenLayerSelected(flush));
        flush.OnDisplay(() => editor.MapsUi.SetButtonIcon(flush,
            new(Tile.SHAPE_SQUARE_BIG_HOLLOW, Color.Gray, 1), 1));

        var rename = new InputBox { Value = "", Placeholder = "Rename…", IsSingleLine = true };
        rename.OnSubmit(() =>
        {
            LayersRename(rename.Value);
            rename.Value = "";
        });
        rename.OnUpdate(() => ShowWhenLayerSelected(rename));
        rename.OnDisplay(() => editor.MapsUi.SetInputBox(rename, 1));

        var up = new Button();
        up.OnInteraction(Interaction.Trigger, () => LayersMove(-1));
        up.OnUpdate(() => ShowWhenLayerSelected(up));
        up.OnDisplay(() => editor.MapsUi.SetButtonIcon(up, new(Tile.ARROW, Color.Gray, 3), 1));

        var down = new Button();
        down.OnInteraction(Interaction.Trigger, () => LayersMove(1));
        down.OnUpdate(() => ShowWhenLayerSelected(down));
        down.OnDisplay(() => editor.MapsUi.SetButtonIcon(down, new(Tile.ARROW, Color.Gray, 1), 1));

        //========

        tools = new(count: 13) { ItemWidth = 1, ItemGap = 0 };
        tools.OnItemDisplay(item =>
        {
            var hotkey = new[]
            {
                Keyboard.Key.G, Keyboard.Key.T, Keyboard.Key.R, Keyboard.Key.L, Keyboard.Key.E,
                Keyboard.Key.I, Keyboard.Key.S, Keyboard.Key.B, Keyboard.Key.A, Keyboard.Key.V,
                Keyboard.Key.H, Keyboard.Key.C, Keyboard.Key.P
            };
            var graphics = new[]
            {
                Tile.SHAPE_SQUARE_SMALL_HOLLOW, Tile.GAME_DICE_6, Tile.SHAPE_SQUARE,
                Tile.PUNCTUATION_SLASH, Tile.SHAPE_CIRCLE, Tile.SHAPE_CIRCLE_HOLLOW,
                Tile.ICON_GRID, Tile.ICON_FILL, Tile.ICON_LOOP, Tile.ICON_MIRROR,
                Tile.ICON_FLIP, Tile.ICON_PALETTE, Tile.ICON_PICK
            };
            var index = tools.IndexOf(item);
            var id = graphics[index];
            var (x, y) = item.Position;
            item.Hotkey = ((int)hotkey[index], false);
            editor.MapsUi.SetButtonIcon(item, new(id, item.IsSelected ? Color.Green : Color.Gray), 1);
            editor.MapsUi[(int)Editor.LayerMapsUi.Middle].SetTextLine(
                (x, y + 1), $"{hotkey[index]}", Color.Gray.ToDark(0.4f));
        });
        tools.OnItemInteraction(Interaction.Trigger, item =>
        {
            var logs = new[]
            {
                "Group of tiles", "Single random tile", "Rectangle of random tiles",
                "Line of random tiles", "Filled ellipse of random tiles",
                "Hollow ellipse of random tiles",
                "Replace all tiles of a kind with random tiles", "Fill with random tiles",
                "Rotate rectangle of tiles", "Mirror rectangle of tiles vertically",
                "Flip rectangle of tiles horizontally", "Color rectangle of tiles", "Pick a tile"
            };
            editor.Log(logs[tools.IndexOf(item)]);
        });

        paletteColor = new() { IsPickHidden = true };
        paletteColor.OnDisplay(() =>
        {
            editor.MapsUi.SetPalette(paletteColor, zOrder: 1);
            editor.MapsUi.SetSlider(paletteColor.Opacity, zOrder: 1);
            editor.MapsUi.SetPages(paletteColor.Brightness, zOrder: 1);
        });
        paletteColor.OnSampleDisplay((btn, color) =>
            editor.MapsUi[1].SetTile(btn.Position, new(Tile.SHADE_OPAQUE, color)));
        paletteColor.Brightness.OnItemDisplay(btn =>
            editor.MapsUi.SetPagesItem(paletteColor.Brightness, btn));

        paletteScrollH = new(isVertical: false) { Size = (14, 1) };
        paletteScrollV = new(isVertical: true) { Size = (1, 14) };
        paletteScrollH.OnDisplay(() => editor.MapsUi.SetScroll(paletteScrollH));
        paletteScrollV.OnDisplay(() => editor.MapsUi.SetScroll(paletteScrollV));

        //========

        var inspectorItems = new Block?[]
        {
            layers, create, null, null, up, down, null, rename, null,
            paletteScrollV, paletteScrollH, null, paletteColor, null,
            null, tools, remove, flush, null, layersVisibility
        };
        var layout = new Layout((Position.x + 1, Position.y + 1)) { Size = (w - 2, h - 2) };
        layout.OnDisplaySegment((segment, i) =>
            UpdateInspectorItem(i, inspectorItems, segment, tilePalette));

        layout.Cut(0, Side.Bottom, 0.65f); // layers
        layout.Cut(1, Side.Bottom, 0.95f); // create
        layout.Cut(1, Side.Right, 0.9f); // empty
        layout.Cut(3, Side.Right, 0.9f); // remove
        layout.Cut(4, Side.Right, 0.9f); // up
        layout.Cut(5, Side.Right, 0.9f); // down
        layout.Cut(2, Side.Top, 0.05f); // rename

        layout.Cut(2, Side.Bottom, 0.52f); // tileset
        layout.Cut(8, Side.Right, 0.05f); // scroll V
        layout.Cut(8, Side.Bottom, 0.05f); // scroll H
        layout.Cut(2, Side.Bottom, 0.2f); // empty

        layout.Cut(2, Side.Bottom, 0.3f); // colors
        layout.Cut(2, Side.Bottom, 0.6f); // tools text
        layout.Cut(11, Side.Bottom, 0.1f); // hover info
        layout.Cut(13, Side.Bottom, 0.7f); // tools

        layout.Cut(6, Side.Right, 0.1f); // remove
        layout.Cut(6, Side.Right, 0.1f); // flush
        layout.Cut(15, Side.Bottom, 0.5f); // empty
        layout.Cut(0, Side.Left, 0.1f); // layers visibility

        editor.Ui.Add(this, layout, create, up, down, rename, remove, flush,
            tools, paletteColor, paletteScrollV, paletteScrollH, layers, layersVisibility);
    }

    public Tilemap? GetSelectedTilemap()
    {
        var drawLayer = layers.ItemsSelected;
        var index = drawLayer.Length != 1 ? -1 : layers.IndexOf(drawLayer[0]);
        return index == -1 ? null : editor.MapsEditor[index];
    }

#region Backend
    private readonly Editor editor;

    private void LayerCreate()
    {
        var item = new Button { Text = "NewLayer" };
        var size = editor.MapsEditor.Size;
        var isEmpty = editor.MapsEditor.Count == 0;
        size = isEmpty ? (50, 50) : size;

        layers.Add(item);
        layersVisibility.Add(new Button { IsSelected = true });
        editor.MapsEditor.Add(new Tilemap(size));
        RecreateMapVisibilities();

        if (isEmpty)
            editor.MapsEditor.ViewSize = (50, 50);
    }
    private void LayersRename(string name)
    {
        var selected = layers.ItemsSelected;
        foreach (var item in selected)
            item.Text = name;
    }
    private void LayersRemove()
    {
        var selected = layers.ItemsSelected;
        foreach (var item in selected)
        {
            var index = layers.IndexOf(item);
            editor.MapsEditor.Remove(editor.MapsEditor[index]);
            layers.Remove(item);
            layersVisibility.Remove(layersVisibility[index]);
        }

        RecreateMapVisibilities();
    }
    private void LayersMove(int direction)
    {
        var selected = layers.ItemsSelected;
        var maps = new Tilemap[selected.Length];
        var vis = new Button[selected.Length];
        for (var i = 0; i < selected.Length; i++)
        {
            var index = layers.IndexOf(selected[i]);
            maps[i] = editor.MapsEditor[index];
            vis[i] = layersVisibility[index];
        }

        layers.Shift(direction, layers.ItemsSelected);
        layersVisibility.Shift(direction, vis);
        editor.MapsEditor.Shift(direction, maps);
        RecreateMapVisibilities();
    }

    private void UpdateInspectorItem(
        int i,
        Block?[] inspectorItems,
        (int x, int y, int width, int height) segment,
        TilePalette palette)
    {
        //editor.MapsUi.SetLayoutSegment(segment, i, true, 5);

        if (i == 13)
        {
            editor.MapsUi[(int)Editor.LayerMapsUi.Front].SetTextLine(
                position: (segment.x, segment.y),
                text: "Tool:");
            return;
        }

        if (i >= inspectorItems.Length)
            return;

        if (i == 14 && palette.layer.IsHovered && editor.Prompt.IsHidden)
        {
            var (mx, my) = palette.mousePos;
            var index = new Indices(my, mx).ToIndex(palette.map.Size.width);
            editor.MapsUi[(int)Editor.LayerMapsUi.Front].SetTextRectangle(
                position: (segment.x, segment.y),
                size: (segment.width, segment.height),
                text: $"{index} ({mx} {my})");
            return;
        }

        var item = inspectorItems[i];
        if (item == null)
            return;

        item.Position = (segment.x, segment.y);
        item.Size = (segment.width, segment.height);

        if (i == 19)
        {
            var list = (List)item;
        }
    }

    private void ShowWhenLayerSelected(Block block)
    {
        block.IsHidden = layers.ItemsSelected.Length == 0;
        block.IsDisabled = block.IsHidden;
    }
    private void RecreateMapVisibilities()
    {
        editor.MapsEditorVisible.Clear();
        for (var i = 0; i < layersVisibility.Count; i++)
            editor.MapsEditorVisible.Add(layersVisibility[i].IsSelected);
    }
#endregion
}