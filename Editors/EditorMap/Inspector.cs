namespace Pure.Editors.EditorMap;

using static Keyboard.Key;
using static Tile;
using static Color;

internal class Inspector : Panel
{
    public List layers, layersVisibility;
    public Palette paletteColor;
    public Scroll paletteScrollV;
    public Scroll paletteScrollH;
    public Pages tools;

    public (bool mirror, bool flip) tileOrientation;
    public sbyte tileTurns;

    public Tile pickedTile;

    public Inspector(Editor editor, TilePalette tilePalette)
    {
        this.editor = editor;

        var (w, h) = (16, editor.MapsUi.Size.height);
        Text = string.Empty;
        Size = (w, h);
        IsMovable = false;
        IsResizable = false;
        AlignInside((1f, 0.5f));
        var (x, y) = Position;

        OnDisplay(() =>
        {
            editor.MapsUi.SetPanel(this);

            var tileLeft = new Tile(BOX_GRID_T_SHAPED, Blue);
            var tileMid = new Tile(BOX_GRID_STRAIGHT, Blue);
            var tileRight = new Tile(BOX_GRID_T_SHAPED, Blue, 2);

            SetLine(8, 18, 22, 26, 28);

            if (tools?.Current < 9 && tilePalette.layer.IsHovered && editor.Prompt.IsHidden)
            {
                var (mx, my) = tilePalette.mousePos;
                var index = (my, mx).ToIndex1D(tilePalette.map.Size);
                editor.MapsUi.Tilemaps[FRONT].SetText((X + 1, Y + 29), $"{index} ({mx} {my})");
            }

            void SetLine(params int[] ys)
            {
                foreach (var curY in ys)
                {
                    editor.MapsUi.Tilemaps[MIDDLE].SetLine((x, curY), (x + w, curY), null, tileMid);
                    editor.MapsUi.Tilemaps[MIDDLE].SetTile((x, curY), tileLeft);
                    editor.MapsUi.Tilemaps[MIDDLE].SetTile((x + w - 1, curY), tileRight);
                }
            }
        });

        var layerButtons = AddLayers();
        var autoButtons = AddAutoTiles(tilePalette);
        var paletteScroll = AddPaletteScrolls();
        var orientButtons = AddOrientations();
        AddTools(tilePalette);
        AddPaletteColor();

        editor.Ui.Blocks.AddRange(new Block[]
            { this, tools, paletteColor, layersVisibility, layers, paletteScrollV, paletteScrollH });
        editor.Ui.Blocks.AddRange(paletteScroll);
        editor.Ui.Blocks.AddRange(autoButtons);
        editor.Ui.Blocks.AddRange(layerButtons);
        editor.Ui.Blocks.AddRange(orientButtons);
    }

    public Tilemap? GetSelectedTilemap()
    {
        var drawLayer = layers.SelectedItems;
        var index = drawLayer.Count != 1 ? -1 : layers.Items.IndexOf(drawLayer[0]);
        return index == -1 ? null : editor.MapsEditor.Tilemaps[index];
    }

#region Backend
    private const int MIDDLE = (int)Editor.LayerMapsUi.Middle, FRONT = (int)Editor.LayerMapsUi.Front;
    private readonly Editor editor;

    [MemberNotNull(nameof(layers)), MemberNotNull(nameof(layersVisibility))]
    private Block[] AddLayers()
    {
        var (x, y) = (X + 1, Y + 1);
        layersVisibility = new((x, y), 1) { ItemSize = (1, 1), Size = (1, 5) };
        layersVisibility.OnItemDisplay(item =>
        {
            var tile = item.IsSelected ?
                new Tile(ICON_EYE_OPENED, White) :
                new(ICON_EYE_CLOSED, Gray);
            editor.MapsUi.SetButtonIcon(item, tile, 1);
        });
        layersVisibility.Select(layersVisibility.Items[0]);
        layersVisibility.OnItemInteraction(Interaction.Trigger, item =>
            editor.MapsEditorVisible[layersVisibility.Items.IndexOf(item)] = item.IsSelected);

        layers = new((x + layersVisibility.Width, y), 1) { ItemSize = (13, 1), Size = (13, 5) };
        layers.OnUpdate(() => layersVisibility.Scroll.Slider.Progress = layers?.Scroll.Slider.Progress ?? 0);
        layers.OnDisplay(() => editor.MapsUi.SetList(layers));
        layers.OnItemDisplay(item => editor.MapsUi.SetListItem(layers, item));
        layers.Items[0].Text = "Layer1";
        layers.Select(layers.Items[0]);

        var rename = new InputBox((x, y + layers.Height))
        {
            Value = string.Empty, Placeholder = "Rename…", IsSingleLine = true, Size = (14, 1)
        };
        rename.OnSubmit(() =>
        {
            LayersRename(rename.Value);
            rename.Value = string.Empty;
        });
        rename.OnUpdate(() => ShowIfLayerSelected(rename));
        rename.OnDisplay(() => editor.MapsUi.SetInputBox(rename, 1));

        var create = new Button((x, rename.Y + rename.Height)) { Size = (1, 1) };
        create.OnInteraction(Interaction.Trigger, LayerCreate);
        create.OnDisplay(() => editor.MapsUi.SetButtonIcon(create, new(CURSOR_CROSSHAIR, Gray), 1));

        var remove = new Button((x + 13, create.Y)) { Size = (1, 1) };
        const string REMOVE = "Remove selected layers?";
        remove.OnInteraction(Interaction.Trigger, () => editor.PromptYesNo(REMOVE, LayersRemove));
        remove.OnUpdate(() => ShowIfMoreThan1LayerSelected(remove));
        remove.OnDisplay(() => editor.MapsUi.SetButtonIcon(remove, new(ICON_DELETE, Gray), 1));

        var flush = new Button((x + 11, create.Y)) { Size = (1, 1) };
        const string FLUSH = "Flush selected layers?";
        flush.OnInteraction(Interaction.Trigger, () => editor.PromptYesNo(FLUSH, () =>
        {
            for (var i = 0; i < layers.SelectedItems.Count; i++)
                editor.MapsEditor.Tilemaps[i].Flush();
        }));
        flush.OnUpdate(() => ShowIfLayerSelected(flush));
        flush.OnDisplay(() => editor.MapsUi.SetButtonIcon(flush, new(SHAPE_SQUARE_BIG_HOLLOW, Gray), 1));

        var up = new Button((create.X + 2, create.Y)) { Size = (1, 1) };
        up.OnInteraction(Interaction.Trigger, () => LayersMove(-1));
        up.OnUpdate(() => ShowIfMoreThan1LayerSelected(up));
        up.OnDisplay(() => editor.MapsUi.SetButtonIcon(up, new(ARROW, Gray, 3), 1));

        var down = new Button((up.X + 1, create.Y)) { Size = (1, 1) };
        down.OnInteraction(Interaction.Trigger, () => LayersMove(1));
        down.OnUpdate(() => ShowIfMoreThan1LayerSelected(down));
        down.OnDisplay(() => editor.MapsUi.SetButtonIcon(down, new(ARROW, Gray, 1), 1));

        return new Block[] { create, remove, flush, rename, up, down };

        void LayerCreate()
        {
            var item = new Button { Text = "NewLayer" };
            var size = editor.MapsEditor.Size;
            var isEmpty = editor.MapsEditor.Tilemaps.Count == 0;
            size = isEmpty ? (50, 50) : size;

            layers.Items.Add(item);
            layersVisibility.Items.Add(new() { IsSelected = true });
            editor.MapsEditor.Tilemaps.Add(new(size));
            RecreateMapVisibilities();

            if (isEmpty)
                editor.MapsEditor.View = new(editor.MapsEditor.View.Position, (50, 50));
        }

        void LayersRename(string name)
        {
            var selected = layers.SelectedItems;
            foreach (var item in selected)
                item.Text = name;
        }

        void LayersRemove()
        {
            var selected = layers.SelectedItems;
            foreach (var item in selected)
            {
                var index = layers.Items.IndexOf(item);
                editor.MapsEditor.Tilemaps.Remove(editor.MapsEditor.Tilemaps[index]);
                layers.Items.Remove(item);
                layersVisibility.Items.Remove(layersVisibility.Items[index]);
            }

            RecreateMapVisibilities();
        }

        void LayersMove(int direction)
        {
            var selected = layers.SelectedItems;
            var maps = new Tilemap[selected.Count];
            var vis = new Button[selected.Count];
            for (var i = 0; i < selected.Count; i++)
            {
                var index = layers.Items.IndexOf(selected[i]);
                maps[i] = editor.MapsEditor.Tilemaps[index];
                vis[i] = layersVisibility.Items[index];
            }

            layers.Items.Shift(direction, layers.SelectedItems.ToArray());
            layersVisibility.Items.Shift(direction, vis);
            editor.MapsEditor.Tilemaps.Shift(direction, maps);
            RecreateMapVisibilities();
        }

        void ShowIfLayerSelected(Block block)
        {
            block.IsHidden = layers.SelectedItems.Count == 0;
            block.IsDisabled = block.IsHidden;
        }

        void ShowIfMoreThan1LayerSelected(Block block)
        {
            block.IsHidden = layers.Count < 2 || layers.SelectedItems.Count == 0;
            block.IsDisabled = block.IsHidden;
        }

        void RecreateMapVisibilities()
        {
            editor.MapsEditorVisible.Clear();
            for (var i = 0; i < layersVisibility.Count; i++)
                editor.MapsEditorVisible.Add(layersVisibility.Items[i].IsSelected);
        }
    }
    [MemberNotNull(nameof(tools))]
    private void AddTools(TilePalette tilePalette)
    {
        tools = new((X + 1, 20), 14) { ItemWidth = 1, ItemGap = 0, Size = (14, 1) };
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
            var (itemX, itemY) = item.Position;
            item.Hotkey = ((int)hotkey[index], false);
            editor.MapsUi.SetButtonIcon(item, new(id, item.IsSelected ? Green : Gray), 1);
            editor.MapsUi.Tilemaps[MIDDLE].SetText((itemX, itemY + 1), $"{hotkey[index]}", Gray.ToDark(0.4f));
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
        tools.OnDisplay(() => editor.MapsUi.Tilemaps[FRONT].SetText((tools.X, tools.Y - 1), "Tool:"));
    }
    [MemberNotNull(nameof(paletteColor))]
    private void AddPaletteColor()
    {
        paletteColor = new((X + 2, 23)) { Pick = { IsHidden = true } };
        paletteColor.OnDisplay(() =>
        {
            editor.MapsUi.SetPalette(paletteColor, MIDDLE);

            var (px, py) = paletteColor.Position;
            var area = (px - 1, py, 1, 3);
            var tile = new Tile(FULL, paletteColor.SelectedColor);
            editor.MapsUi.Tilemaps[0].SetArea(area, null, new Tile(FULL, Black));
            editor.MapsUi.Tilemaps[MIDDLE].SetArea(area, null, new Tile(SHADE_5, Gray));
            editor.MapsUi.Tilemaps[FRONT].SetArea(area, null, tile);
        });
        paletteColor.OnSampleDisplay((btn, color) => editor.MapsUi.Tilemaps[MIDDLE].SetTile(btn.Position, new(SHADE_OPAQUE, color)));
    }
    [MemberNotNull(nameof(paletteScrollV)), MemberNotNull(nameof(paletteScrollH))]
    private Button[] AddPaletteScrolls()
    {
        paletteScrollV = new((X + 14, Y + 29)) { Size = (1, 14) };
        paletteScrollH = new((X + 1, Y + 43), false) { Size = (14, 1) };
        paletteScrollH.OnDisplay(() => editor.MapsUi.SetScroll(paletteScrollH));
        paletteScrollV.OnDisplay(() => editor.MapsUi.SetScroll(paletteScrollV));
        var paletteScroll = new Button((paletteScrollH.X, paletteScrollV.Y + 1)) { Size = (13, 13) };
        paletteScroll.OnInteraction(Interaction.Scroll, () => paletteScrollV.Slider.Move(Mouse.ScrollDelta));
        return new[] { paletteScroll };
    }
    private Block[] AddAutoTiles(TilePalette tilePalette)
    {
        var (x, y) = (X + 1, Y + 9);
        var auto = new List<Button>();

        var add = new Button { Size = (8, 1), Text = "Add Rule", Position = (x, y + 1) };
        add.OnInteraction(Interaction.Trigger, () =>
        {
            var layer = layers.Items.IndexOf(layers.SelectedItems[0]);
            var rule = new List<int>();
            foreach (var btn in auto)
            {
                rule.Add(btn.Text == "any" ? -1 : int.Parse(btn.Text));
                btn.Text = "any";
            }

            editor.MapsEditor.Tilemaps[layer].AddAutoTileRule(rule.ToArray(), pickedTile);
        });
        add.OnDisplay(() => editor.MapsUi.SetButton(add, FRONT));

        var apply = new Button((x, y + 2)) { Size = (11, 1), Text = "Apply Rules" };
        apply.OnInteraction(Interaction.Trigger, () =>
        {
            var layer = layers.Items.IndexOf(layers.SelectedItems[0]);
            editor.MapsEditor.Tilemaps[layer].SetAutoTiles(editor.MapsEditor.Tilemaps[layer]);
        });

        var clear = new Button((x + 13, y + 1)) { Size = (1, 1) };
        clear.OnInteraction(Interaction.Trigger, () =>
        {
            editor.PromptYesNo("Clear all auto tile rules?", () =>
            {
                var layer = layers.Items.IndexOf(layers.SelectedItems[0]);
                editor.MapsEditor.Tilemaps[layer].ClearAutoTileRules();
            });
        });

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

        OnDisplay(() =>
        {
            for (var i = 0; i < auto.Count; i++)
            {
                var curX = x + i % 3 * 5;
                var curY = y + 6 + i / 3;
                auto[i].Position = (curX, curY);
            }

            var empty = pickedTile == default;
            var pickText = empty ?
                $"Use pick tool{Environment.NewLine}on a map tile!" :
                $"Id{pickedTile.Id} Turns{pickedTile.Turns}{Environment.NewLine}" +
                $"Flip{(pickedTile.IsFlipped ? "+" : "-")} Mirror{(pickedTile.IsMirrored ? "+" : "-")}";
            editor.MapsUi.Tilemaps[FRONT].SetText((x, y), "Auto Tiles:");
            editor.MapsUi.Tilemaps[FRONT].SetText((x, y + 4), pickText, empty ? White : pickedTile.Tint);
            editor.MapsUi.SetButton(apply, FRONT);
            editor.MapsUi.SetButtonIcon(clear, new(ICON_DELETE, Gray), FRONT);

            var sameCount = 1;
            var prevId = "";
            foreach (var btn in auto)
            {
                sameCount += btn.Text == prevId ? 1 : 0;
                prevId = btn.Text;
                editor.MapsUi.SetButton(btn, FRONT);
            }

            add.IsDisabled = sameCount == 9 || pickedTile == default;
        });

        var result = new List<Block>();
        result.AddRange(auto);
        result.AddRange(new[] { add, apply, clear });
        return result.ToArray();
    }
    private Block[] AddOrientations()
    {
        var m = new Button((X + 2, Y + 27)) { Size = (1, 1) };
        m.OnDisplay(() =>
        {
            tileOrientation = (m.IsSelected, tileOrientation.flip);
            editor.MapsUi.Tilemaps[FRONT].SetText((m.X - 1, m.Y), "M");
            editor.MapsUi.SetButtonIcon(m, new(ICON_MIRROR, m.IsSelected ? Green : Gray), FRONT);
        });

        var f = new Button((X + 5, m.Y)) { Size = (1, 1) };
        f.OnDisplay(() =>
        {
            tileOrientation = (tileOrientation.mirror, f.IsSelected);
            editor.MapsUi.Tilemaps[FRONT].SetText((f.X - 1, f.Y), "F");
            editor.MapsUi.SetButtonIcon(f, new(ICON_FLIP, f.IsSelected ? Green : Gray), FRONT);
        });

        var turns = new Button((X + 10, m.Y)) { Size = (1, 1) };
        turns.OnInteraction(Interaction.Trigger, () => tileTurns++);
        turns.OnDisplay(() =>
        {
            var color = tileTurns % 4 == 0 ? Gray : Green;
            editor.MapsUi.Tilemaps[FRONT].SetTile((turns.X - 1, turns.Y), new(ARROW, White, tileTurns));
            editor.MapsUi.SetButtonIcon(turns, new(ICON_ROTATE, color), FRONT);
        });

        return new Block[] { m, f, turns };
    }
#endregion
}