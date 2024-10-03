namespace Pure.Editors.Map;

using static Tile;
using static Color;

internal class TerrainPanel : Panel
{
    public TerrainPanel(Editor editor, Inspector inspector, TilePalette tilePalette)
    {
        this.editor = editor;
        this.inspector = inspector;

        Text = string.Empty;
        Size = (15, editor.MapsUi.Size.height - 8);
        IsMovable = false;
        IsResizable = false;
        AlignInside((0f, 0.35f));

        var toggle = new Button((X, Y + Height + 1)) { Text = "Terrain Panel", Size = (13, 1) };
        toggle.OnDisplay(() => editor.MapsUi.SetButton(toggle, 0, true));

        OnDisplay(() =>
        {
            X = toggle.IsSelected ? 0 : -100;

            editor.MapsUi.SetPanel(this);

            var tileLeft = new Tile(BOX_GRID_T_SHAPED, Blue);
            var tileMid = new Tile(BOX_GRID_STRAIGHT, Blue);
            var tileRight = new Tile(BOX_GRID_T_SHAPED, Blue, 2);

            SetLine(16, 19);

            void SetLine(params int[] ys)
            {
                foreach (var curY in ys)
                {
                    editor.MapsUi.Tilemaps[MIDDLE].SetLine((X, curY), (X + Width - 1, curY), null, tileMid);
                    editor.MapsUi.Tilemaps[MIDDLE].SetTile((X, curY), tileLeft);
                    editor.MapsUi.Tilemaps[MIDDLE].SetTile((X + Width - 1, curY), tileRight);
                }
            }
        });

        var autoButtons = AddAutoTiles(tilePalette);
        var terrainBlocks = AddTerrainBlocks();

        editor.Ui.Blocks.AddRange(new Block[] { this, toggle });
        editor.Ui.Blocks.AddRange(autoButtons);
        editor.Ui.Blocks.AddRange(terrainBlocks);
    }

#region Backend
    private const int MIDDLE = (int)Editor.LayerMapsUi.Middle, FRONT = (int)Editor.LayerMapsUi.Front;
    private readonly Editor editor;
    private readonly Inspector inspector;

    private Block[] AddAutoTiles(TilePalette tilePalette)
    {
        var matchIds = new List<Button>();

        var add = new Button { Size = (13, 1), Text = "Add This Rule" };
        add.OnInteraction(Interaction.Trigger, () =>
        {
            var layer = inspector.layers.Items.IndexOf(inspector.layers.SelectedItems[0]);
            var rule = new List<int>();
            foreach (var btn in matchIds)
            {
                rule.Add(btn.Text == "any" ? -1 : int.Parse(btn.Text));
                btn.Text = "any";
            }

            editor.MapsEditor.Tilemaps[layer].AutoTiles.Add((rule.ToArray(), inspector.pickedTile));
        });

        var apply = new Button { Size = (13, 1) };
        apply.OnInteraction(Interaction.Trigger, () =>
        {
            var layer = inspector.layers.Items.IndexOf(inspector.layers.SelectedItems[0]);
            editor.MapsEditor.Tilemaps[layer].SetAutoTiles(editor.MapsEditor.Tilemaps[layer]);
        });

        var clear = new Button { Size = (13, 1) };
        clear.OnInteraction(Interaction.Trigger, () =>
        {
            var layer = inspector.layers.Items.IndexOf(inspector.layers.SelectedItems[0]);
            var autoCount = editor.MapsEditor.Tilemaps[layer].AutoTiles.Count;
            editor.PromptYesNo($"Clear all {autoCount} auto tile rules?", () =>
            {
                editor.MapsEditor.Tilemaps[layer].AutoTiles.Clear();
            });
        });

        for (var i = 0; i < 9; i++)
        {
            var btn = new Button { Text = "any", Size = (3, 1) };
            matchIds.Add(btn);
            btn.OnInteraction(Interaction.Trigger, () =>
            {
                var selected = tilePalette.GetSelectedTiles().Flatten();
                if (selected.Length == 1)
                    btn.Text = btn.Text == "any" ? $"{selected[0].Id}" : "any";
            });
        }

        OnDisplay(() =>
        {
            add.Position = (X + 1, Y + 13);
            apply.Position = (X + 1, Y + 15);
            clear.Position = (X + 1, Y + 16);

            var layer = inspector.layers.Items.IndexOf(inspector.layers.SelectedItems[0]);
            var autoCount = editor.MapsEditor.Tilemaps[layer].AutoTiles.Count;
            apply.Text = $"Set {autoCount} Rules";
            clear.Text = $"Cut {autoCount} Rules";
            clear.IsDisabled = autoCount == 0;

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

            //editor.MapsUi.Tilemaps[FRONT].SetText((X + 1, Y + 1), "Auto Tiles:");
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

            editor.MapsUi.SetButton(add, FRONT);
            editor.MapsUi.SetButton(clear, FRONT);
            editor.MapsUi.SetButton(apply, FRONT);

            var sameCount = 1;
            var prevId = "";
            foreach (var btn in matchIds)
            {
                sameCount += btn.Text == prevId ? 1 : 0;
                prevId = btn.Text;
                editor.MapsUi.SetButton(btn, FRONT);
            }

            add.IsDisabled = sameCount == 9 || pickedTile == default;
        });

        var result = new List<Block>();
        result.AddRange(matchIds);
        result.AddRange(new[] { add, apply, clear });
        return result.ToArray();
    }
    private Block[] AddTerrainBlocks()
    {
        var noiseType = new List((0, 0), 6, Span.Dropdown) { Size = (13, 6), ItemSize = (13, 1) };
        noiseType.OnItemDisplay(item => editor.MapsUi.SetListItem(noiseType, item));
        noiseType.Edit(
            nameof(NoiseType.OpenSimplex2),
            nameof(NoiseType.OpenSimplex2S),
            nameof(NoiseType.Cellular),
            nameof(NoiseType.Perlin),
            nameof(NoiseType.ValueCubic),
            nameof(NoiseType.Value));
        noiseType.Select(noiseType.Items[4]);

        var scale = new InputBox { Size = (7, 1), SymbolGroup = SymbolGroup.Digits };

        OnDisplay(() =>
        {
            new Point().ToNoise(NoiseType.Cellular);
            noiseType.Position = (X + 1, 21);
            scale.Position = (X + 7, 23);

            editor.MapsUi.Tilemaps[FRONT].SetText((X + 1, Y + 18), "Noise Type:", White);
            editor.MapsUi.SetList(noiseType);
            editor.MapsUi.Tilemaps[FRONT].SetText((X + 1, Y + 21), "Scale:", White);
            editor.MapsUi.SetInputBox(scale);
        });

        return new Block[] { noiseType, scale };
    }
#endregion
}