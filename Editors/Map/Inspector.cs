namespace Pure.Editors.Map;

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

    public List pose;

    public Tile pickedTile;

    public Inspector(Editor editor, TilePalette tilePalette)
    {
        this.editor = editor;

        var (w, h) = (16, editor.MapsUi[0].Size.height);
        Text = string.Empty;
        Size = (w, h);
        IsMovable = false;
        IsResizable = false;
        AlignInside((1f, 0.5f));
        var (x, y) = Position;

        OnDisplay += () =>
        {
            editor.MapsUi.SetPanel(this);

            var tileLeft = new Tile(PIPE_GRID_T_SHAPED, Blue);
            var tileMid = new Tile(PIPE_GRID_STRAIGHT, Blue);
            var tileRight = new Tile(PIPE_GRID_T_SHAPED, Blue, Pose.Down);

            SetLine(14, 18, 23, 28);

            if (tools?.Current < 9 && tilePalette.layer.IsHovered && editor.Prompt.IsHidden)
            {
                var (mx, my) = tilePalette.mousePos;
                var index = (my, mx).ToIndex(tilePalette.map.Size);
                editor.MapsUi[FRONT].SetText((X + 1, Y + 29), $"{index} ({mx} {my})");
            }

            void SetLine(params int[] ys)
            {
                foreach (var curY in ys)
                {
                    editor.MapsUi[MIDDLE].SetLine((x, curY), (x + w, curY), tileMid);
                    editor.MapsUi[MIDDLE].SetTile((x, curY), tileLeft);
                    editor.MapsUi[MIDDLE].SetTile((x + w - 1, curY), tileRight);
                }
            }
        };

        var layerButtons = AddLayers();
        var paletteScroll = AddPaletteScrolls();
        AddTools(tilePalette);
        AddPaletteColor();

        editor.Ui.AddRange([this, tools, paletteColor, layersVisibility, layers, paletteScrollV, paletteScrollH]);
        editor.Ui.AddRange(paletteScroll);
        editor.Ui.AddRange(layerButtons);
        editor.Ui.Add(AddPose());
    }

    public TileMap? GetSelectedTilemap()
    {
        var drawLayer = layers.SelectedItems;
        var index = drawLayer.Count != 1 ? -1 : layers.Items.IndexOf(drawLayer[0]);
        return index == -1 ? null : editor.MapsEditor[index];
    }

#region Backend
    private const int MIDDLE = (int)Editor.LayerMapsUi.Middle, FRONT = (int)Editor.LayerMapsUi.Front;
    private readonly Editor editor;

    [MemberNotNull(nameof(layers)), MemberNotNull(nameof(layersVisibility))]
    private Block[] AddLayers()
    {
        var (x, y) = (X + 1, Y + 1);
        layersVisibility = new((x, y), 1) { ItemSize = (1, 1), Size = (1, 11) };
        layersVisibility.OnItemDisplay += item =>
        {
            var tile = item.IsSelected ?
                new Tile(ICON_EYE_OPENED, White) :
                new(ICON_EYE_CLOSED, Gray);
            editor.MapsUi.SetButtonIcon(item, tile, 1);
        };
        layersVisibility.Select(layersVisibility.Items[0]);
        layersVisibility.OnItemInteraction(Interaction.Trigger, item =>
            editor.MapsEditorVisible[layersVisibility.Items.IndexOf(item)] = item.IsSelected);

        layers = new((x + layersVisibility.Width, y), 1) { ItemSize = (13, 1), Size = (13, 11) };
        layers.OnUpdate += () => layersVisibility.Scroll.Slider.Progress = layers?.Scroll.Slider.Progress ?? 0;
        layers.OnDisplay += () => editor.MapsUi.SetList(layers);
        layers.OnItemDisplay += item => editor.MapsUi.SetListItem(layers, item);
        layers.Items[0].Text = "Layer1";
        layers.Select(layers.Items[0]);

        var rename = new InputBox((x, y + layers.Height))
        {
            Value = string.Empty, Placeholder = "Rename…", Size = (14, 1)
        };
        rename.OnInteraction(Interaction.Trigger, () =>
        {
            if (string.IsNullOrEmpty(rename.Value))
                return;

            LayersRename(rename.Value);
            rename.Value = string.Empty;
        });
        rename.OnUpdate += () => ShowIfLayerSelected(rename);
        rename.OnDisplay += () => editor.MapsUi.SetInputBox(rename, 1);

        var create = new Button((x, rename.Y + rename.Height)) { Size = (1, 1) };
        create.OnInteraction(Interaction.Trigger, LayerCreate);
        create.OnDisplay += () => editor.MapsUi.SetButtonIcon(create, new(CURSOR_CROSSHAIR, Gray), 1);

        var remove = new Button((x + 13, create.Y)) { Size = (1, 1) };
        const string REMOVE = "Remove selected layers?";
        remove.OnInteraction(Interaction.Trigger, () => editor.PromptYesNo(REMOVE, LayersRemove));
        remove.OnUpdate += () => ShowIfMoreThan1LayerSelected(remove);
        remove.OnDisplay += () => editor.MapsUi.SetButtonIcon(remove, new(ICON_DELETE, Gray), 1);

        var flush = new Button((x + 11, create.Y)) { Size = (1, 1) };
        const string FLUSH = "Flush selected layers?";
        flush.OnInteraction(Interaction.Trigger, () => editor.PromptYesNo(FLUSH, () =>
        {
            for (var i = 0; i < layers.SelectedItems.Count; i++)
                editor.MapsEditor[i].Flush();
        }));
        flush.OnUpdate += () => ShowIfLayerSelected(flush);
        flush.OnDisplay += () => editor.MapsUi.SetButtonIcon(flush, new(SHAPE_SQUARE_BIG_HOLLOW, Gray), 1);

        var up = new Button((create.X + 2, create.Y)) { Size = (1, 1) };
        up.OnInteraction(Interaction.Trigger, () => LayersMove(-1));
        up.OnUpdate += () => ShowIfMoreThan1LayerSelected(up);
        up.OnDisplay += () => editor.MapsUi.SetButtonIcon(up, new(ARROW, Gray, Pose.Left), 1);

        var down = new Button((up.X + 1, create.Y)) { Size = (1, 1) };
        down.OnInteraction(Interaction.Trigger, () => LayersMove(1));
        down.OnUpdate += () => ShowIfMoreThan1LayerSelected(down);
        down.OnDisplay += () => editor.MapsUi.SetButtonIcon(down, new(ARROW, Gray, Pose.Right), 1);

        return [create, remove, flush, rename, up, down];

        void LayerCreate()
        {
            var item = new Button { Text = "NewLayer" };
            var size = editor.MapsEditor[0].Size;
            var isEmpty = editor.MapsEditor.Count == 0;
            size = isEmpty ? (50, 50) : size;

            layers.Items.Add(item);
            layersVisibility.Items.Add(new() { IsSelected = true });
            editor.MapsEditor.Add(new(size));
            RecreateMapVisibilities();

            if (isEmpty)
                editor.MapsEditor.ForEach(map => map.View = new(editor.MapsEditor[0].View.Position, (50, 50)));
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
                editor.MapsEditor.Remove(editor.MapsEditor[index]);
                layers.Items.Remove(item);
                layersVisibility.Items.Remove(layersVisibility.Items[index]);
            }

            RecreateMapVisibilities();
        }

        void LayersMove(int direction)
        {
            var selected = layers.SelectedItems;
            var maps = new TileMap[selected.Count];
            var vis = new Button[selected.Count];
            for (var i = 0; i < selected.Count; i++)
            {
                var index = layers.Items.IndexOf(selected[i]);
                maps[i] = editor.MapsEditor[index];
                vis[i] = layersVisibility.Items[index];
            }

            layers.Items.Shift(direction, layers.SelectedItems.ToArray());
            layersVisibility.Items.Shift(direction, vis);
            editor.MapsEditor.Shift(direction, maps);
            RecreateMapVisibilities();
        }

        void ShowIfLayerSelected(Block block)
        {
            block.IsHidden = layers.SelectedItems.Count == 0;
            block.IsDisabled = block.IsHidden;
        }

        void ShowIfMoreThan1LayerSelected(Block block)
        {
            block.IsHidden = layers.Items.Count < 2 || layers.SelectedItems.Count == 0;
            block.IsDisabled = block.IsHidden;
        }

        void RecreateMapVisibilities()
        {
            editor.MapsEditorVisible.Clear();
            for (var i = 0; i < layersVisibility.Items.Count; i++)
                editor.MapsEditorVisible.Add(layersVisibility.Items[i].IsSelected);
        }
    }
    [MemberNotNull(nameof(tools))]
    private void AddTools(TilePalette tilePalette)
    {
        tools = new((X + 1, 16), 14) { ItemWidth = 1, ItemGap = 0, Size = (14, 1) };
        tools.OnItemDisplay += item =>
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
            editor.MapsUi[MIDDLE].SetText((itemX, itemY + 1), $"{hotkey[index]}", Gray.ToDark(0.4f));
        };
        tools.OnItemInteraction(Interaction.Trigger, item =>
        {
            var logs = new[]
            {
                "Group of tiles", "Single random tile", "Rectangle of random tiles",
                "Line of random tiles", "Filled ellipse of random tiles",
                "Hollow ellipse of random tiles", "Replace all tiles of a kind with random tiles",
                "Fill with random tiles", "Rotate rectangle of tiles", "Mirror rectangle of tiles vertically",
                "Flip rectangle of tiles horizontally", "Color rectangle of tiles", "Pick a tile",
                $"Select tiles:\n" +
                $"Ctrl = drag\n" +
                $"Ctrl+C/V/X = copy/paste/cut\n" +
                "Delete = clear"
            };
            editor.Log(logs[tools.IndexOf(item)]);
            tilePalette.start = tilePalette.end;
        });
        tools.OnDisplay += () => editor.MapsUi[FRONT].SetText((tools.X, tools.Y - 1), "Tool:");
    }
    [MemberNotNull(nameof(paletteColor))]
    private void AddPaletteColor()
    {
        paletteColor = new((X + 2, 25)) { Pick = { IsHidden = true } };
        paletteColor.OnDisplay += () =>
        {
            editor.MapsUi[FRONT].SetText((X + 1, 24), "Tint:");
            editor.MapsUi.SetPalette(paletteColor, MIDDLE);

            var (px, py) = paletteColor.Position;
            var area = (px - 1, py, 1, 3);
            var tile = new Tile(FULL, paletteColor.SelectedColor);
            editor.MapsUi[0].SetArea(area, new Tile(FULL, Black));
            editor.MapsUi[MIDDLE].SetArea(area, new Tile(SHADE_5, Gray));
            editor.MapsUi[FRONT].SetArea(area, tile);
        };
    }
    [MemberNotNull(nameof(paletteScrollV)), MemberNotNull(nameof(paletteScrollH))]
    private Button[] AddPaletteScrolls()
    {
        paletteScrollV = new((X + 14, Y + 29)) { Size = (1, 14) };
        paletteScrollH = new((X + 1, Y + 43), false) { Size = (14, 1) };
        paletteScrollH.OnDisplay += () => editor.MapsUi.SetScroll(paletteScrollH);
        paletteScrollV.OnDisplay += () => editor.MapsUi.SetScroll(paletteScrollV);
        var paletteScroll = new Button((paletteScrollH.X, paletteScrollV.Y + 1)) { Size = (13, 13) };
        paletteScroll.OnInteraction(Interaction.Scroll, () => paletteScrollV.Slider.Move(Mouse.ScrollDelta));
        return [paletteScroll];
    }
    [MemberNotNull(nameof(pose))]
    private List AddPose()
    {
        pose = new((X + 1, 20), 8, Span.Dropdown) { Size = (14, 8), ItemSize = (14, 1) };
        pose.Edit([
            nameof(Pose.Default), nameof(Pose.Right), nameof(Pose.Down), nameof(Pose.Left),
            nameof(Pose.Flip), nameof(Pose.FlipRight), nameof(Pose.FlipDown), nameof(Pose.FlipLeft)
        ]);

        OnDisplay += () =>
        {
            var disabled = tools.Current >= 9;

            if (disabled == false)
                editor.MapsUi[FRONT].SetText((X + 1, 19), "Pose:");

            pose.IsHidden = disabled;
            pose.IsDisabled = disabled;
        };

        pose.OnDisplay += () => editor.MapsUi.SetList(pose, MIDDLE);
        pose.OnItemDisplay += item => editor.MapsUi.SetListItem(pose, item, FRONT);

        return pose;
    }
#endregion
}