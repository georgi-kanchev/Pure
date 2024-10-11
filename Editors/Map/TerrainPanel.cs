using static Pure.Engine.Tilemap.Tile;
using static Pure.Engine.Utilities.Color;

namespace Pure.Editors.Map;

internal class TerrainPanel : Panel
{
    public MapGenerator generator = new();

    public TerrainPanel(Editor editor, Inspector inspector, TilePalette tilePalette)
    {
        this.editor = editor;
        this.inspector = inspector;

        var h = editor.MapsUi.Size.height;
        Text = string.Empty;
        Size = (15, h - 8);
        IsMovable = false;
        IsResizable = false;
        AlignInside((0f, 0.35f));

        var toggle = new Button((X, h - 5)) { Text = "Terrain Panel", Size = (13, 1) };
        toggle.OnDisplay(() => editor.MapsUi.SetButton(toggle, 0));

        OnDisplay(() =>
        {
            X = toggle.IsSelected && GetLayer() >= 0 ? 0 : -100;

            editor.MapsUi.SetPanel(this);

            var tileLeft = new Tile(BOX_GRID_T_SHAPED, Blue);
            var tileMid = new Tile(BOX_GRID_STRAIGHT, Blue);
            var tileRight = new Tile(BOX_GRID_T_SHAPED, Blue, 2);

            SetLine(17);

            void SetLine(params int[] ys)
            {
                foreach (var curY in ys)
                {
                    editor.MapsUi.Tilemaps[MIDDLE].SetLine(
                        (X, Y + curY), (X + Width - 1, Y + curY), null, tileMid);
                    editor.MapsUi.Tilemaps[MIDDLE].SetTile((X, Y + curY), tileLeft);
                    editor.MapsUi.Tilemaps[MIDDLE].SetTile((X + Width - 1, Y + curY), tileRight);
                }
            }
        });

        var autoButtons = AddAutoTiles(tilePalette);
        var terrainBlocks = AddTerrainBlocks(tilePalette);

        editor.Ui.Blocks.AddRange([this, toggle]);
        editor.Ui.Blocks.AddRange(autoButtons);
        editor.Ui.Blocks.AddRange(terrainBlocks);
    }

    public void UpdateUI()
    {
        noiseType.Select(noiseType.Items[(int)generator.Noise]);
        scale.Value = $"{generator.Scale}";
        seed.Value = $"{generator.Seed}";

        tiles.Items.Clear();
        foreach (var (depth, tile) in generator.Elevations.OrderBy(x => x.Key))
            tiles.Items.Add(new() { Text = $"{depth}|{tile.Id}" });

        TryAutoGenerate();
    }

#region Backend
    private const int MIDDLE = (int)Editor.LayerMapsUi.Middle, FRONT = (int)Editor.LayerMapsUi.Front;
    private int lastLayer = -1;
    private readonly Editor editor;
    private readonly Inspector inspector;
    private Button? generate, autoGenerate;
    private List tiles;
    private List noiseType;
    private InputBox scale, seed;

    private Block[] AddAutoTiles(TilePalette tilePalette)
    {
        var matchIds = new List<Button>();
        var add = new Button { Size = (1, 1) };
        var remove = new Button { Size = (1, 1) };
        var apply = new Button { Size = (13, 3), Text = "Apply Rules" };
        var pages = new Pages(count: 1) { Size = (6, 1), ItemWidth = 2 };
        var autoTiles = GetAutoTiles();

        for (var i = 0; i < 9; i++)
        {
            var btn = new Button { Text = "any", Size = (3, 1) };
            var index = i;
            matchIds.Add(btn);
            btn.OnInteraction(Interaction.Trigger, () =>
            {
                var selected = tilePalette.GetSelectedTiles().Flatten();

                if (selected.Length == 1)
                    btn.Text = btn.Text == "any" ? $"{selected[0].Id}" : "any";

                if (autoTiles.Count <= 0)
                    return;

                var id = btn.Text == "any" ? -1 : int.Parse(btn.Text);
                autoTiles[pages.Current - 1].matchIds[index] = id;
            });
        }

        pages.OnInteraction(Interaction.Select, () =>
        {
            if (autoTiles.Count == 0)
                return;

            inspector.pickedTile = autoTiles[pages.Current - 1].replacedCenter;
            for (var i = 0; i < matchIds.Count; i++)
            {
                var id = autoTiles[pages.Current - 1].matchIds[i];
                matchIds[i].Text = id < 0 ? "any" : $"{id}";
            }
        });

        add.OnInteraction(Interaction.Trigger, () =>
        {
            var rule = new List<int>();
            foreach (var btn in matchIds)
                rule.Add(btn.Text == "any" ? -1 : int.Parse(btn.Text));

            autoTiles.Add((rule.ToArray(), inspector.pickedTile));
        });
        apply.OnInteraction(Interaction.Trigger, () =>
        {
            var layer = GetLayer();
            editor.MapsEditor.Tilemaps[layer].SetAutoTiles(editor.MapsEditor.Tilemaps[layer]);
        });
        remove.OnInteraction(Interaction.Trigger, () =>
        {
            editor.PromptYesNo("Remove this autotile rule?", () => autoTiles.RemoveAt(pages.Current - 1));
        });
        pages.OnItemDisplay(page => editor.MapsUi.SetPagesItem(pages, page));

        OnDisplay(() =>
        {
            var layer = GetLayer();

            add.Position = (X + 13, Y + 13);
            apply.Position = autoTiles.Count == 0 ? (-100, 0) : (X + 1, Y + 14);
            remove.Position = autoTiles.Count == 0 ? (-100, 0) : (X + 12, Y + 13);

            if (pages.Count != autoTiles.Count || layer != lastLayer)
            {
                pages.Count = autoTiles.Count;
                pages.Interact(Interaction.Select);
            }

            if (autoTiles.Count > 0 && tilePalette.justPickedTile)
            {
                var curr = autoTiles[pages.Current - 1];
                curr.replacedCenter = inspector.pickedTile;
                autoTiles[pages.Current - 1] = curr;
            }

            lastLayer = layer;

            pages.Position = autoTiles.Count == 0 ? (-100, 0) : (X + 1, Y + 13);

            remove.IsDisabled = autoTiles.Count == 0;

            for (var i = 0; i < matchIds.Count; i++)
            {
                var curX = X + 1 + i % 3 * 5;
                var curY = Y + 2 + i / 3;
                matchIds[i].Position = (curX, curY);
            }

            var pickedTile = inspector.pickedTile;
            var empty = pickedTile == default;
            var pickText = empty ?
                "use pick tool" :
                $"Id: {pickedTile.Id}{Environment.NewLine}" +
                $"Turns: {pickedTile.Turns}{Environment.NewLine}" +
                $"Flip: {(pickedTile.IsFlipped ? "yes" : "no")}{Environment.NewLine}" +
                $"Mirror: {(pickedTile.IsMirrored ? "yes" : "no")}";

            editor.MapsUi.Tilemaps[FRONT].SetText((X + 1, Y + 6),
                $"To change the{Environment.NewLine}center to:", White);
            editor.MapsUi.Tilemaps[FRONT].SetText((X + 1, Y + 8), pickText, Gray);
            editor.MapsUi.Tilemaps[FRONT].SetText((X + 1, Y + 1), "Match Ids:", White);

            if (empty == false)
            {
                var colorArea = (X + 12, Y + 9, 2, 2);
                editor.MapsUi.Tilemaps[MIDDLE].SetArea(colorArea, null, new Tile(SHADE_5, Gray));
                editor.MapsUi.Tilemaps[FRONT].SetArea(colorArea, null, new Tile(FULL, pickedTile.Tint));
            }

            editor.MapsUi.SetButtonIcon(add, new(CURSOR_CROSSHAIR, Gray), FRONT);
            editor.MapsUi.SetButtonIcon(remove, new(ICON_TRASH, Gray), FRONT);
            editor.MapsUi.SetButton(apply, FRONT);
            editor.MapsUi.SetPages(pages);

            foreach (var btn in matchIds)
                editor.MapsUi.SetButton(btn, FRONT);
        });

        var result = new List<Block>();
        result.AddRange(matchIds);
        result.AddRange([add, apply, remove, pages]);
        return result.ToArray();
    }
    [MemberNotNull(nameof(tiles), nameof(noiseType), nameof(scale), nameof(seed))]
    private Block[] AddTerrainBlocks(TilePalette tilePalette)
    {
        var result = new List<Block>();
        noiseType = new((0, 0), 6, Span.Dropdown) { Size = (13, 6), ItemSize = (13, 1) };
        scale = new()
        {
            Value = "10", MaximumSymbolCount = 5, Size = (6, 1), SymbolGroup = SymbolGroup.Decimals
        };
        seed = new()
        {
            Value = "0", MaximumSymbolCount = 5, Size = (6, 1), SymbolGroup = SymbolGroup.Decimals
        };
        generate = new() { Text = "Generate!", Size = (13, 3) };
        autoGenerate = new() { IsSelected = true, Text = "Auto Generate", Size = (13, 1) };
        tiles = new(itemCount: 0) { IsSingleSelecting = true, Size = (9, 4), ItemSize = (9, 1) };
        var add = new Button { Size = (1, 1) };
        var edit = new Button { Size = (1, 1) };
        var remove = new Button { Size = (1, 1) };

        for (var i = 0; i < 5; i++)
        {
            var offsets = new (int x, int y)[] { (1, 0), (0, 1), (-1, 0), (0, -1), (0, 0) };
            var btn = new Button { Size = (1, 1) };
            var (x, y) = offsets[i];
            var index = i;

            if (x != 0 && y != 0)
                btn.OnInteraction(Interaction.PressAndHold, Trigger);

            btn.OnInteraction(Interaction.Trigger, Trigger);
            btn.OnInteraction(Interaction.PressAndHold, Trigger);
            btn.OnDisplay(() =>
            {
                btn.Position = (X + 2 + x, Y + 25 + y);
                var color = btn.GetInteractionColor(Gray);
                var arrow = new Tile(ARROW_TAILLESS_ROUND, color, (sbyte)index);
                var center = new Tile(SHAPE_CIRCLE, color);
                editor.MapsUi.Tilemaps[FRONT].SetTile(btn.Position, index == 4 ? center : arrow);
            });

            result.Add(btn);

            void Trigger()
            {
                var off = (generator.Offset.x + x, generator.Offset.y + y);
                generator.Offset = x == 0 && y == 0 ? (0, 0) : off;
                TryAutoGenerate();
            }
        }

        noiseType.OnItemDisplay(item => editor.MapsUi.SetListItem(noiseType, item));
        noiseType.Edit(nameof(Noise.OpenSimplex2), nameof(Noise.OpenSimplex2S), nameof(Noise.Cellular),
            nameof(Noise.Perlin), nameof(Noise.ValueCubic), nameof(Noise.Value));
        noiseType.Select(noiseType.Items[4]);

        noiseType.OnItemInteraction(Interaction.Select, btn =>
        {
            generator.Noise = (Noise)noiseType.Items.IndexOf(btn);
            TryAutoGenerate();
        });

        scale.OnInteraction(Interaction.Trigger, () =>
        {
            var success = float.TryParse(scale.Text, out var value);
            generator.Scale = success ? value : 10;
            TryAutoGenerate();
        });
        seed.OnInteraction(Interaction.Trigger, () =>
        {
            var success = int.TryParse(seed.Text, out var value);
            generator.Seed = success ? value : 0;
            TryAutoGenerate();
        });
        generate.OnInteraction(Interaction.Trigger, () =>
        {
            var layer = GetLayer();
            if (layer == -1)
                return;

            var map = editor.MapsEditor.Tilemaps[layer];
            map.Flush();
            generator.Apply(map);
        });
        tiles.OnItemDisplay(btn => editor.MapsUi.SetListItem(tiles, btn));

        add.OnInteraction(Interaction.Trigger, () =>
        {
            generator.Elevations[GetFreeDepth()] = inspector.pickedTile;
            UpdateUI();
        });
        edit.OnInteraction(Interaction.Trigger, () =>
        {
            foreach (var btn in tiles.SelectedItems)
            {
                var depth = byte.Parse(btn.Text.Split("|")[0]);
                var tile = generator.Elevations[depth];

                editor.PromptInput.SymbolGroup = SymbolGroup.Integers;
                editor.PromptInput.Value = "";
                editor.Prompt.Text = "Set terrain tile height 0…255:";
                editor.Prompt.Open(editor.PromptInput, onButtonTrigger: i =>
                {
                    if (i != 0)
                        return;

                    byte.TryParse(editor.PromptInput.Value, out var newDepth);
                    newDepth = GetFreeDepth(newDepth);
                    generator.Elevations.Remove(depth);
                    generator.Elevations[newDepth] = tile;
                    UpdateUI();
                });
            }
        });
        remove.OnInteraction(Interaction.Trigger, () =>
        {
            editor.PromptYesNo("Remove this terrain tile height?", () =>
            {
                foreach (var btn in tiles.SelectedItems)
                {
                    var depth = byte.Parse(btn.Text.Split("|")[0]);
                    generator.Elevations.Remove(depth);
                }

                UpdateUI();
            });
        });

        OnDisplay(() =>
        {
            noiseType.Position = (X + 1, Y + 19);
            scale.Position = (X + 8, Y + 21);
            seed.Position = (X + 8, Y + 22);
            tiles.Position = (X + 5, Y + 26);
            add.Position = (X + 5, Y + 30);
            edit.Position = (X + 7, Y + 30);
            remove.Position = (X + 13, Y + 30);
            autoGenerate.Position = (X + 1, Y + 32);
            generate.Position = (X + 1, Y + 33);

            if (tilePalette.justPickedTile)
            {
                var selected = tiles.SelectedItems;
                foreach (var btn in selected)
                {
                    var depth = byte.Parse(btn.Text.Split("|")[0]);
                    generator.Elevations[depth] = inspector.pickedTile;
                }

                if (selected.Count > 0)
                    UpdateUI();
            }

            editor.MapsUi.Tilemaps[FRONT].SetText((X + 1, Y + 21), "Scale", White);
            editor.MapsUi.SetInputBox(scale);
            editor.MapsUi.Tilemaps[FRONT].SetText((X + 1, Y + 22), "Seed", White);
            editor.MapsUi.SetInputBox(seed);
            editor.MapsUi.Tilemaps[FRONT].SetText((X + 1, Y + 23),
                $"Offset {generator.Offset.x} {generator.Offset.y}", White);

            editor.MapsUi.Tilemaps[FRONT].SetText((X + 5, Y + 25), "Height|Id", White);
            editor.MapsUi.SetList(tiles);

            if (tiles.SelectedItems.Count > 0)
                editor.MapsUi.Tilemaps[FRONT].SetText((X + 1, Y + 28), $"use{Environment.NewLine}" +
                                                                       $"pick{Environment.NewLine}" +
                                                                       $"tool", Gray);

            editor.MapsUi.SetButtonIcon(add, new(CURSOR_CROSSHAIR, Gray), MIDDLE);
            editor.MapsUi.SetButtonIcon(edit, new(ICON_PEN, Gray), MIDDLE);
            editor.MapsUi.SetButtonIcon(remove, new(ICON_TRASH, Gray), MIDDLE);

            editor.MapsUi.Tilemaps[FRONT].SetText((X + 1, Y + 18), "Noise Type", White);
            editor.MapsUi.SetList(noiseType);

            editor.MapsUi.SetButton(generate, MIDDLE);
            editor.MapsUi.SetButton(autoGenerate, MIDDLE);
        });

        result.AddRange([add, edit, remove, scale, seed, tiles, autoGenerate, generate, noiseType]);
        return result.ToArray();
    }

    private void TryAutoGenerate()
    {
        if (autoGenerate is { IsSelected: true })
            generate?.Interact(Interaction.Trigger);
    }

    private int GetLayer()
    {
        var selected = inspector.layers.SelectedItems;
        return selected.Count != 1 ? -1 : inspector.layers.Items.IndexOf(selected[0]);
    }
    private List<(int[] matchIds, Tile replacedCenter)> GetAutoTiles()
    {
        var layer = GetLayer();
        return layer == -1 ? new() : editor.MapsEditor.Tilemaps[GetLayer()].AutoTiles;
    }
    private byte GetFreeDepth(byte starting = 0)
    {
        var freeDepth = starting;
        foreach (var (depth, _) in generator.Elevations)
        {
            if (generator.Elevations.ContainsKey(freeDepth) == false)
                break;

            freeDepth = (byte)(depth + 1);
        }

        return freeDepth;
    }
#endregion
}