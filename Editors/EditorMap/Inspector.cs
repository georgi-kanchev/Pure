namespace Pure.Editors.EditorMap;

using static Keyboard.Key;

using static Tile;

internal class Inspector : Panel
{
    public readonly List layers, layersVisibility;
    public readonly Palette paletteColor;
    public readonly Scroll paletteScrollV, paletteScrollH;
    public readonly Pages tools;

    public Tile pickedTile;

    public Inspector(Editor editor, TilePalette tilePalette)
    {
        this.editor = editor;

        var (w, h) = (16, editor.MapsUi.Size.height);
        Text = string.Empty;
        Size = (w, h);
        IsMovable = false;
        IsResizable = false;

        OnDisplay(() => editor.MapsUi.SetPanel(this));
        AlignInside((1f, 0.5f));

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
                new Tile(ICON_EYE_OPENED, Color.White) :
                new(ICON_EYE_CLOSED, Color.Gray);
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
            editor.MapsUi.SetButtonIcon(create, new(CURSOR_CROSSHAIR, Color.Gray), 1));

        var remove = new Button();
        remove.OnInteraction(Interaction.Trigger, () =>
            editor.PromptYesNo("Remove selected layers?", LayersRemove));
        remove.OnUpdate(() =>
        {
            remove.IsHidden = layers.Count < 2 || layers.ItemsSelected.Length == 0;
            remove.IsDisabled = remove.IsHidden;
        });
        remove.OnDisplay(() =>
            editor.MapsUi.SetButtonIcon(remove, new(ICON_DELETE, Color.Gray), 1));

        var flush = new Button();
        flush.OnInteraction(Interaction.Trigger, () =>
            editor.PromptYesNo("Flush selected layers?", () =>
            {
                for (var i = 0; i < layers.ItemsSelected.Length; i++)
                    editor.MapsEditor[i].Flush();
            }));
        flush.OnUpdate(() => ShowWhenLayerSelected(flush));
        flush.OnDisplay(() => editor.MapsUi.SetButtonIcon(flush,
            new(SHAPE_SQUARE_BIG_HOLLOW, Color.Gray, 1), 1));

        var rename = new InputBox { Value = string.Empty, Placeholder = "Rename…", IsSingleLine = true };
        rename.OnSubmit(() =>
        {
            LayersRename(rename.Value);
            rename.Value = string.Empty;
        });
        rename.OnUpdate(() => ShowWhenLayerSelected(rename));
        rename.OnDisplay(() => editor.MapsUi.SetInputBox(rename, 1));

        var up = new Button();
        up.OnInteraction(Interaction.Trigger, () => LayersMove(-1));
        up.OnUpdate(() => ShowWhenLayerSelected(up));
        up.OnDisplay(() => editor.MapsUi.SetButtonIcon(up, new(ARROW, Color.Gray, 3), 1));

        var down = new Button();
        down.OnInteraction(Interaction.Trigger, () => LayersMove(1));
        down.OnUpdate(() => ShowWhenLayerSelected(down));
        down.OnDisplay(() => editor.MapsUi.SetButtonIcon(down, new(ARROW, Color.Gray, 1), 1));

        //========

        tools = new(count: 14) { ItemWidth = 1, ItemGap = 0 };
        tools.OnItemDisplay(item =>
        {
            var hotkey = new[] { G, T, R, L, E, O, K, B, A, V, H, C, P, S };
            var graphics = new[]
            {
                SHAPE_SQUARE_SMALL_HOLLOW, GAME_DICE_6, SHAPE_SQUARE,
                PUNCTUATION_SLASH, SHAPE_CIRCLE, SHAPE_CIRCLE_HOLLOW,
                ICON_GRID, ICON_FILL, ICON_LOOP, ICON_MIRROR,
                ICON_FLIP, ICON_PALETTE, ICON_PICK, SHAPE_SQUARE_HOLLOW
            };
            var index = tools.IndexOf(item);
            var id = graphics[index];
            var (x, y) = item.Position;
            item.Hotkey = ((int)hotkey[index], false);
            editor.MapsUi.SetButtonIcon(item, new(id, item.IsSelected ? Color.Green : Color.Gray), 1);
            editor.MapsUi[(int)Editor.LayerMapsUi.Middle].SetText(
                (x, y + 1), $"{hotkey[index]}", Color.Gray.ToDark(0.4f));
        });
        tools.OnItemInteraction(Interaction.Trigger, item =>
        {
            var logs = new[]
            {
                "Group of tiles", "Single random tile", "Rectangle of random tiles",
                "Line of random tiles", "Filled ellipse of random tiles",
                "Hollow ellipse of random tiles", "Replace all tiles of a kind with random tiles",
                "Fill with random tiles", "Rotate rectangle of tiles", "Mirror rectangle of tiles vertically",
                "Flip rectangle of tiles horizontally", "Color rectangle of tiles", "Pick a tile",
                $"Select tiles:{Environment.NewLine}" +
                $"Ctrl = drag{Environment.NewLine}" +
                $"Ctrl+C/V/X = copy/paste/cut{Environment.NewLine}" +
                "Delete = clear"
            };
            editor.Log(logs[tools.IndexOf(item)]);
            tilePalette.start = tilePalette.end;
        });

        paletteColor = new() { Pick = { IsHidden = true } };
        paletteColor.OnDisplay(() =>
        {
            editor.MapsUi.SetPalette(paletteColor, 1);
            editor.MapsUi.SetSlider(paletteColor.Opacity, 1);
            editor.MapsUi.SetPages(paletteColor.Brightness, 1);

            var (px, py, pw, ph) = paletteColor.Area;
            var tile = new Tile(FULL, paletteColor.SelectedColor);
            editor.MapsUi[(int)Editor.LayerMapsUi.Front].SetArea((px, py + ph, pw, 1), null, tile);
        });
        paletteColor.OnSampleDisplay((btn, color) =>
            editor.MapsUi[1].SetTile(btn.Position, new(SHADE_OPAQUE, color)));
        paletteColor.Brightness.OnItemDisplay(btn =>
            editor.MapsUi.SetPagesItem(paletteColor.Brightness, btn));

        paletteScrollH = new(vertical: false) { Size = (14, 1) };
        paletteScrollV = new(vertical: true) { Size = (1, 14) };
        paletteScrollH.OnDisplay(() => editor.MapsUi.SetScroll(paletteScrollH));
        paletteScrollV.OnDisplay(() => editor.MapsUi.SetScroll(paletteScrollV));

        //========

        const int FRONT = (int)Editor.LayerMapsUi.Front;
        var autoTiles = new Panel { IsResizable = false, IsMovable = false, Size = (w, 5) };
        var autoAdd = new Button { Size = (8, 1), Text = "Add Rule" };
        var autoApply = new Button { Size = (11, 1), Text = "Apply Rules" };
        var autoClear = new Button { Size = (1, 1) };
        var auto = new List<Button>();
        for (var i = 0; i < 9; i++)
        {
            var btn = new Button { Text = "any", Size = (4, 1) };
            auto.Add(btn);
            btn.OnInteraction(Interaction.Trigger, () =>
            {
                var selected = tilePalette.GetSelectedTiles().Flatten();
                if (selected.Length == 1)
                    btn.Text = btn.Text == "any" ? $"{selected[0].Id}" : "any";
            });
        }

        autoAdd.OnInteraction(Interaction.Trigger, () =>
        {
            var layer = layers.IndexOf(layers.ItemsSelected[0]);
            var rule = new List<int>();
            foreach (var btn in auto)
            {
                rule.Add(btn.Text == "any" ? -1 : int.Parse(btn.Text));
                btn.Text = "any";
            }

            editor.MapsEditor[layer].AddAutoTileRule(rule.ToArray(), pickedTile);
        });
        autoClear.OnInteraction(Interaction.Trigger, () =>
        {
            editor.PromptYesNo("Clear all auto tile rules?", () =>
            {
                var layer = layers.IndexOf(layers.ItemsSelected[0]);
                editor.MapsEditor[layer].ClearAutoTileRules();
            });
        });
        autoApply.OnInteraction(Interaction.Trigger, () =>
        {
            var layer = layers.IndexOf(layers.ItemsSelected[0]);
            editor.MapsEditor[layer].SetAutoTiles(editor.MapsEditor[layer]);
        });
        autoAdd.OnDisplay(() => editor.MapsUi.SetButton(autoAdd, FRONT));
        autoTiles.OnDisplay(() =>
        {
            autoAdd.Position = (autoTiles.Position.x, autoTiles.Position.y + 1);
            autoApply.Position = (autoTiles.Position.x, autoTiles.Position.y + 2);
            autoClear.Position = (autoTiles.Position.x + 13, autoTiles.Position.y + 1);

            for (var i = 0; i < auto.Count; i++)
            {
                var x = autoTiles.Position.x + i % 3 * 5;
                var y = autoTiles.Position.y + 5 + i / 3;
                auto[i].Position = (x, y);
            }

            var empty = pickedTile == default;
            var pickText = empty ? $"Use pick tool{Environment.NewLine}on a map tile!" :
                $"Id{pickedTile.Id} Turns{pickedTile.Turns}{Environment.NewLine}" +
                $"Flip{(pickedTile.IsFlipped ? "+" : "-")} Mirror{(pickedTile.IsMirrored ? "+" : "-")}";
            editor.MapsUi[FRONT].SetText((autoTiles.Position.x, autoTiles.Position.y + 3),
                pickText, empty ? Color.White : pickedTile.Tint);
            editor.MapsUi.SetButton(autoApply, FRONT);
            editor.MapsUi.SetButtonIcon(autoClear, new(ICON_DELETE, Color.Gray), FRONT);

            var sameCount = 1;
            var prevId = "";
            foreach (var btn in auto)
            {
                sameCount += btn.Text == prevId ? 1 : 0;
                prevId = btn.Text;
                editor.MapsUi.SetButton(btn, FRONT);
            }

            autoAdd.IsDisabled = sameCount == 9 || pickedTile == default;
        });

        //========

        var inspectorItems = new Block?[]
        {
            layers, create, autoTiles, null, up, down, null, rename, null,
            paletteScrollV, paletteScrollH, null, paletteColor, null,
            null, tools, remove, flush, null, layersVisibility
        };
        var layout = new Layout((Position.x + 1, Position.y + 1)) { Size = (w - 2, h - 2) };
        layout.OnDisplaySegment((segment, i) =>
            UpdateInspectorItem(i, inspectorItems, segment, tilePalette));

        layout.Cut(0, Side.Bottom, 0.8f); // layers
        layout.Cut(1, Side.Bottom, 0.95f); // create
        layout.Cut(1, Side.Right, 0.9f); // empty
        layout.Cut(3, Side.Right, 0.9f); // remove
        layout.Cut(4, Side.Right, 0.9f); // up
        layout.Cut(5, Side.Right, 0.9f); // down
        layout.Cut(2, Side.Top, 0.05f); // rename

        layout.Cut(2, Side.Bottom, 0.4f); // tileset
        layout.Cut(8, Side.Right, 0.05f); // scroll V
        layout.Cut(8, Side.Bottom, 0.05f); // scroll H
        layout.Cut(2, Side.Bottom, 0.1f); // empty

        layout.Cut(2, Side.Bottom, 0.15f); // colors
        layout.Cut(2, Side.Bottom, 0.25f); // tools text
        layout.Cut(11, Side.Bottom, 0.1f); // hover info
        layout.Cut(13, Side.Bottom, 0.6f); // tools

        layout.Cut(6, Side.Right, 0.1f); // remove
        layout.Cut(6, Side.Right, 0.1f); // flush
        layout.Cut(15, Side.Bottom, 0.5f); // empty
        layout.Cut(0, Side.Left, 0.1f); // layers visibility

        layout.Cut(2, Side.Top, 0.1f); // empty above auto tile

        editor.Ui.Add(this, layout, create, up, down, rename, remove, flush,
            autoTiles, autoAdd, autoClear, autoApply,
            tools, paletteColor, paletteScrollV, paletteScrollH, layers, layersVisibility);

        foreach (var btn in auto)
            editor.Ui.Add(btn);
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
            editor.MapsEditor.View = new(editor.MapsEditor.View.Position, (50, 50));
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

    private void UpdateInspectorItem(int i, Block?[] inspectorItems, (int x, int y, int width, int height) segment, TilePalette palette)
    {
        //editor.MapsUi.SetLayoutSegment(segment, i, true, 5);

        if (i == 13)
        {
            editor.MapsUi[(int)Editor.LayerMapsUi.Front]
                .SetText((segment.x, segment.y), "Tool:");
            return;
        }

        if (i >= inspectorItems.Length)
            return;

        var tool = tools.Current;
        if (tool < 9 && i == 14 && palette.layer.IsHovered && editor.Prompt.IsHidden)
        {
            var (mx, my) = palette.mousePos;
            var index = (my, mx).ToIndex1D(palette.map.Size);
            var text = $"{index} ({mx} {my})".Constrain((segment.width, segment.height));
            editor.MapsUi[(int)Editor.LayerMapsUi.Front].SetText((segment.x, segment.y), text);
            return;
        }

        var item = inspectorItems[i];
        if (item == null)
            return;

        item.Position = (segment.x, segment.y);
        item.Size = (segment.width, segment.height);
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