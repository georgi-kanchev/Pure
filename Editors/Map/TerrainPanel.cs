using static Pure.Engine.Tiles.Tile;
using static Pure.Engine.Utility.Color;

namespace Pure.Editors.Map;

internal class TerrainPanel : Panel
{
    public MapGenerator generator = new();

    public TerrainPanel(Editor editor, Inspector inspector, TilePalette tilePalette)
    {
        this.editor = editor;
        this.inspector = inspector;

        var h = editor.MapsUi[0].Size.height;
        Text = string.Empty;
        Size = (15, h - 8);
        IsMovable = false;
        IsResizable = false;
        AlignInside((0f, 0.35f));

        var toggle = new Button((X, h - 5)) { Text = "Terrain Panel", Size = (13, 1) };
        toggle.OnDisplay += () => editor.MapsUi.SetButton(toggle, 0);

        OnDisplay += () =>
        {
            X = toggle.IsSelected && GetLayer() >= 0 ? 0 : -100;

            editor.MapsUi.SetPanel(this);

            var tileLeft = new Tile(PIPE_SOLID_STRAIGHT, Gray);
            var tileMid = new Tile(PIPE_SOLID_STRAIGHT, Gray);
            var tileRight = new Tile(PIPE_SOLID_STRAIGHT, Gray, Pose.Down);

            SetLine(15);

            void SetLine(params int[] ys)
            {
                foreach (var curY in ys)
                {
                    editor.MapsUi[MIDDLE].SetLine((X, Y + curY), (X + Width - 1, Y + curY), tileMid);
                    editor.MapsUi[MIDDLE].SetTile((X, Y + curY), tileLeft);
                    editor.MapsUi[MIDDLE].SetTile((X + Width - 1, Y + curY), tileRight);
                }
            }
        };

        var autoButtons = AddAutoTiles(tilePalette);
        var terrainBlocks = AddTerrainBlocks(tilePalette);

        editor.Ui.AddRange([this, toggle]);
        editor.Ui.AddRange(autoButtons);
        editor.Ui.AddRange(terrainBlocks);
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
        var pages = new Pages((0, 0), 1) { Size = (9, 1), ItemWidth = 2 };
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
                autoTiles[pages.Current - 1].from3X3[index] = id;
            });
        }

        pages.OnInteraction(Interaction.Select, () =>
        {
            if (autoTiles.Count == 0)
                return;

            inspector.pickedTile = autoTiles[pages.Current - 1].to3X3;
            for (var i = 0; i < matchIds.Count; i++)
            {
                var id = autoTiles[pages.Current - 1].from3X3[i];
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
            // editor.MapsEditor[layer].SetAutoTiles(editor.MapsEditor[layer]);
        });
        remove.OnInteraction(Interaction.Trigger, () => { editor.PromptYesNo("Remove this autotile rule?", () => autoTiles.RemoveAt(pages.Current - 1)); });
        pages.OnItemDisplay += page => editor.MapsUi.SetPagesItem(pages, page, 1);

        OnDisplay += () =>
        {
            var layer = GetLayer();

            pages.Position = autoTiles.Count == 0 ? (-100, 0) : (X + 1, Y + 11);
            add.Position = (X + 13, Y + 11);
            remove.Position = autoTiles.Count == 0 ? (-100, 0) : (X + 12, Y + 11);
            apply.Position = autoTiles.Count == 0 ? (-100, 0) : (X + 1, Y + 12);

            if (pages.Count != autoTiles.Count || layer != lastLayer)
            {
                pages.Count = autoTiles.Count;
                pages.Interact(Interaction.Select);
            }

            if (autoTiles.Count > 0 && tilePalette.justPickedTile)
            {
                var curr = autoTiles[pages.Current - 1];
                curr.to3X3 = inspector.pickedTile;
                autoTiles[pages.Current - 1] = curr;
            }

            lastLayer = layer;

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
                $"Id: {pickedTile.Id}\n" +
                "Tint:\n" +
                $"Pose: {pickedTile.Pose}";

            editor.MapsUi[FRONT].SetText((X + 1, Y + 6), "To set center:", White);
            editor.MapsUi[FRONT].SetText((X + 1, Y + 7), pickText, Gray);
            editor.MapsUi[FRONT].SetText((X + 1, Y + 1), "Match Ids:", White);

            if (empty == false)
            {
                var colorArea = (X + 7, Y + 8, 7, 1);
                editor.MapsUi[MIDDLE].SetArea(colorArea, new Tile(SHADE_5, Gray));
                editor.MapsUi[FRONT].SetArea(colorArea, new Tile(FULL, pickedTile.Tint));
            }

            editor.MapsUi.SetButtonIcon(add, new(CURSOR_CROSSHAIR, Gray), FRONT);
            editor.MapsUi.SetButtonIcon(remove, new(ICON_TRASH, Gray), FRONT);
            editor.MapsUi.SetButton(apply, FRONT);
            editor.MapsUi.SetPages(pages, 1);

            foreach (var btn in matchIds)
                editor.MapsUi.SetButton(btn, FRONT);
        };

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
            Value = "10", SymbolLimit = 5, Size = (6, 1), SymbolGroup = SymbolGroup.Decimals
        };
        seed = new()
        {
            Value = "0", SymbolLimit = 5, Size = (6, 1), SymbolGroup = SymbolGroup.Decimals
        };
        generate = new() { Text = "Generate!", Size = (13, 3) };
        autoGenerate = new() { Text = "Auto Gen", Size = (13, 1) };
        tiles = new((0, 0), 0) { IsSingleSelecting = true, Size = (9, 7), ItemSize = (9, 1) };
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
            btn.OnDisplay += () =>
            {
                btn.Position = (X + 2 + x, Y + 22 + y);
                var color = btn.GetInteractionColor(Gray);
                var arrow = new Tile(ARROW_TAILLESS_ROUND, color, (Pose)(byte)index);
                var center = new Tile(SHAPE_CIRCLE, color);
                editor.MapsUi[FRONT].SetTile(btn.Position, index == 4 ? center : arrow);
            };

            result.Add(btn);

            void Trigger()
            {
                var off = (generator.Offset.x + x, generator.Offset.y + y);
                generator.Offset = x == 0 && y == 0 ? (0, 0) : off;
                TryAutoGenerate();
            }
        }

        noiseType.OnItemDisplay += item => editor.MapsUi.SetListItem(noiseType, item);
        noiseType.Edit([
            nameof(Noise.OpenSimplex2), nameof(Noise.OpenSimplex2S), nameof(Noise.Cellular),
            nameof(Noise.Perlin), nameof(Noise.ValueCubic), nameof(Noise.Value)
        ]);
        noiseType.Select(noiseType.Items[4]);

        noiseType.OnItemInteraction(Interaction.Select, btn =>
        {
            generator.Noise = (Noise)noiseType.Items.IndexOf(btn);
            TryAutoGenerate();
        });

        scale.OnInteraction(Interaction.Select, () =>
        {
            var success = float.TryParse(scale.Value, out var value);
            generator.Scale = success ? value : 10;
            TryAutoGenerate();
        });
        seed.OnInteraction(Interaction.Select, () =>
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

            var map = editor.MapsEditor[layer];
            map.Flush();
            generator.Apply(map);
        });
        tiles.OnItemDisplay += btn => editor.MapsUi.SetListItem(tiles, btn);

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

        OnDisplay += () =>
        {
            noiseType.Position = (X + 1, Y + 17);
            scale.Position = (X + 8, Y + 19);
            seed.Position = (X + 8, Y + 20);
            tiles.Position = (X + 5, Y + 25);
            add.Position = (X + 1, Y + 31);
            edit.Position = (X + 2, Y + 31);
            remove.Position = (X + 4, Y + 31);
            generate.Position = (X + 1, Y + 32);
            autoGenerate.Position = (X + 1, Y + 35);

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

            editor.MapsUi[FRONT].SetText((X + 1, Y + 19), "Scale", White);
            editor.MapsUi.SetInputBox(scale);
            editor.MapsUi[FRONT].SetText((X + 1, Y + 20), "Seed", White);
            editor.MapsUi.SetInputBox(seed);
            editor.MapsUi[FRONT].SetText((X + 5, Y + 22), $"{generator.Offset.x} {generator.Offset.y}", White);

            editor.MapsUi[FRONT].SetText((X + 5, Y + 24), "Height|Id", White);
            editor.MapsUi.SetList(tiles);

            if (tiles.SelectedItems.Count > 0)
                editor.MapsUi[FRONT].SetText((X + 1, Y + 26), $"use\n" +
                                                              $"pick\n" +
                                                              $"tool", Gray);

            editor.MapsUi.SetButtonIcon(add, new(CURSOR_CROSSHAIR, Gray), MIDDLE);
            editor.MapsUi.SetButtonIcon(edit, new(ICON_PEN, Gray), MIDDLE);
            editor.MapsUi.SetButtonIcon(remove, new(ICON_TRASH, Gray), MIDDLE);

            editor.MapsUi[FRONT].SetText((X + 1, Y + 16), "Noise Type", White);
            editor.MapsUi.SetList(noiseType);

            editor.MapsUi.SetButton(generate);
            editor.MapsUi.SetCheckbox(autoGenerate);
        };

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
    private List<(int[] from3X3, Tile to3X3)> GetAutoTiles()
    {
        return [];
        // var layer = GetLayer();
        // return layer == -1 ? new() : editor.MapsEditor.TileMaps[GetLayer()].AutoTiles;
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