namespace Pure.Editors.Collision;

using Base;
using Tools.Tilemap;
using Engine.Collision;
using Engine.Utilities;
using Engine.Tilemap;
using Engine.UserInterface;
using Engine.Window;
using System.Diagnostics.CodeAnalysis;

public static class Program
{
    public static void Run()
    {
        Window.SetIconFromTile(editor.LayerUi, (Tile.SHAPE_SQUARE_BIG_HOLLOW, Color.Blue),
            (Tile.SHAPE_TRIANGLE_HOLLOW, Color.Red));

        editor.OnUpdateUi += () =>
        {
            var (vx, vy) = editor.MapsEditor.View.Position;
            solidPack.Position = (-vx, -vy);
            solidMap.Offset = CanDrawLayer ? (0, 0) : (-vx, -vy);
            linePack.Position = (-vx, -vy);

            if ((vx, vy) != prevViewPos)
                solidMap.Update(editor.MapsEditor.Tilemaps[currentLayer]);

            prevViewPos = (vx, vy);

            if (CanDrawLayer)
            {
                layer.DrawTilemap(map);
                solidMap.Update(map);
                layer.DrawRectangles(solidMap);
            }
            else
            {
                editor.LayerMap.DrawRectangles(solidMap);
                editor.LayerMap.DrawRectangles(solidPack);
                editor.LayerMap.DrawLines(linePack.ToBundle());
            }

            if (isDragging == false)
                return;

            var pos = CanDrawLayer ? MousePosPrompt : MousePos;
            var l = CanDrawLayer ? layer : editor.LayerMap;
            var (sx, sy) = (1f / l.AtlasTileSize.width, 1f / l.AtlasTileSize.height);
            solid.Size = Snap((pos.x - solid.Position.x, pos.y - solid.Position.y));
            solid.Position = Snap(solid.Position);

            if (solid.Size.width <= 0)
                solid.Size = (solid.Size.width - sx, solid.Size.height);
            if (solid.Size.height <= 0)
                solid.Size = (solid.Size.width, solid.Size.height - sy);

            if (CanDrawLayer)
                layer.DrawRectangles(solid);
            else if (CanEditGlobal)
                editor.LayerMap.DrawRectangles(solid);
            else if (CanEditLines)
            {
                var line = new Line(Snap(clickPos), Snap(pos), solid.Color);
                editor.LayerMap.DrawLines(line);
            }
        };
        editor.OnUpdateLate += () =>
        {
            if (CanDrawLayer)
                layer.Draw();
        };
        editor.Run();
    }

#region Backend
    private static (float x, float y) clickPos;
    private static (int width, int height) originalMapViewPos;
    private static readonly Editor editor;
    private static readonly List tools;
    private static readonly Layer layer = new();
    private static readonly Tilemap map = new((3, 3));
    private static readonly Panel promptPanel;
    private static readonly Palette palette;
    private static SolidPack solidPack = new();
    private static SolidMap solidMap = new();
    private static LinePack linePack = new();
    private static Menu menu;
    private static bool isDragging;
    private static Solid solid;
    private static string[] layers = ["Layer1"];
    private static int currentLayer;
    private static Tile currentTile;
    private static (int x, int y) prevViewPos;

    private static bool CanDrawLayer
    {
        get => editor.Prompt.IsHidden == false && editor.Prompt.Text.Contains("Tile");
    }
    private static bool CanEdit
    {
        get => editor.LayerMap.IsHovered &&
               editor.Prompt.IsHidden &&
               editor.MapPanel.IsHovered == false &&
               menu.IsHovered == false &&
               tools.IsHovered == false &&
               palette.IsHovered == false;
    }
    private static bool CanEditGlobal
    {
        get => CanEdit && tools.Items[0].IsSelected;
    }
    private static bool CanEditTile
    {
        get => CanEdit && tools.Items[1].IsSelected;
    }
    private static bool CanEditLines
    {
        get => CanEdit && tools.Items[2].IsSelected;
    }
    private static (float x, float y) MousePos
    {
        get => editor.LayerMap.PixelToPosition(Mouse.CursorPosition);
    }
    private static (float x, float y) MousePosPrompt
    {
        get => layer.PixelToPosition(Mouse.CursorPosition);
    }

    static Program()
    {
        editor = new("Pure - Collision Editor");
        var (mw, mh) = editor.MapsEditor.Size;
        editor.MapsEditor.Tilemaps.Clear();
        editor.MapsEditor.Tilemaps.AddRange([new((mw, mh)), new((mw, mh))]);
        editor.MapsEditor.View = new(editor.MapsEditor.View.Position, (mw, mh));
        CreateMenu();

        const int MIDDLE = (int)Editor.LayerMapsUi.Middle;
        const int FRONT = (int)Editor.LayerMapsUi.Front;
        const int PROMPT_MIDDLE = (int)Editor.LayerMapsUi.PromptMiddle;
        const int PROMPT_BACK = (int)Editor.LayerMapsUi.PromptBack;
        tools = new(itemCount: 0)
        {
            Size = (10, 3),
            ItemSize = (10, 1),
            IsSingleSelecting = true
        };
        tools.Items.AddRange([
            new() { Text = "Line Pack" },
            new() { Text = "Solid Map" },
            new() { Text = "Solid Pack" }
        ]);
        tools.AlignInside((1f, 0f));
        tools.OnDisplay(() => editor.MapsUi.SetList(tools, FRONT));
        tools.OnUpdate(() => tools.IsHidden = editor.Prompt.IsHidden == false);
        tools.OnItemDisplay(item => editor.MapsUi.SetListItem(tools, item, FRONT));
        tools.OnItemInteraction(Interaction.Trigger, btn => menu.Items[5].Text = $"{btn.Text}… ");
        tools.Items[0].Interact(Interaction.Trigger);

        palette = new() { Pick = { IsHidden = true } };
        palette.OnDisplay(() =>
        {
            editor.MapsUi.SetPalette(palette, MIDDLE);
            editor.MapsUi.SetSlider(palette.Opacity, MIDDLE);
            editor.MapsUi.SetSlider(palette.Brightness, MIDDLE);
        });
        palette.AlignInside((0.8f, 0f));

        editor.Ui.Blocks.AddRange([tools, palette]);

        // override the default prompt buttons

        editor.OnPromptItemDisplay = item =>
        {
            MapperUserInterface.ThemePromptItems = CanDrawLayer ?
                [
                    new(Tile.ICON_TICK, Color.Green),
                    new(Tile.ARROW, Color.Gray, 3),
                    new(Tile.ARROW, Color.Gray, 1)
                ] :
                [];

            editor.MapsUi.SetPromptItem(editor.Prompt, item, PROMPT_MIDDLE);
        };

        promptPanel = new()
        {
            Size = (30, 30),
            IsResizable = false,
            IsMovable = false
        };
        promptPanel.OnDisplay(() => editor.MapsUi.SetPanel(promptPanel, PROMPT_BACK));

        layer.Size = map.Size;
        layer.Offset = (0f, 0f);

        SubscribeToClicks();
    }

    private static void SubscribeToClicks()
    {
        Mouse.Button.Left.OnPress(() =>
        {
            if (CanEditLines || CanEditGlobal)
            {
                isDragging = true;
                clickPos = MousePos;
                solid.Position = MousePos;
                solid.Color = palette.SelectedColor;
                return;
            }

            if (CanDrawLayer)
            {
                isDragging = true;
                solid.Position = MousePosPrompt;
                solid.Color = palette.SelectedColor;
                return;
            }

            if (CanEditTile == false)
                return;

            var (x, y) = MousePos;
            x += editor.MapsEditor.View.X;
            y += editor.MapsEditor.View.Y;
            currentTile = editor.MapsEditor.Tilemaps[currentLayer].TileAt(((int)x, (int)y));

            layer.AtlasTileGap = editor.LayerMap.AtlasTileGap;
            layer.AtlasPath = editor.LayerMap.AtlasPath;
            layer.AtlasTileSize = editor.LayerMap.AtlasTileSize;
            layer.AtlasTileIdFull = editor.LayerMap.AtlasTileIdFull;
            var (tw, th) = layer.AtlasTileSize;
            var ratio = MathF.Max(tw / 8f, th / 8f);
            var zoom = 28f / ratio;
            layer.Zoom = zoom;

            UpdateMap();
            promptPanel.Text = layers[currentLayer];
            editor.Prompt.Text = "Edit Tile Solids";
            editor.Prompt.Open(promptPanel, btnNo: -1, autoClose: false, btnCount: 3,
                onButtonTrigger: i =>
                {
                    if (i == 0)
                    {
                        editor.Prompt.Close();
                        var (vx, vy) = editor.MapsEditor.View.Position;
                        solidMap.Offset = (-vx, -vy);
                        solidMap.Update(editor.MapsEditor.Tilemaps[currentLayer]);
                        return;
                    }

                    if (i == 1)
                    {
                        OffsetLayer(1);
                        UpdateMap();
                    }
                    else if (i == 2)
                    {
                        OffsetLayer(-1);
                        UpdateMap();
                    }
                });
        });
        Mouse.Button.Left.OnRelease(() =>
        {
            if (isDragging == false)
                return;

            isDragging = false;
            var (mx, my) = CanDrawLayer ? MousePosPrompt : MousePos;
            if (solid.Size.width < 0)
                solid.Position = (mx, solid.Position.y);

            if (solid.Size.height < 0)
                solid.Position = (solid.Position.x, my);

            solid.Size = Snap((Math.Abs(solid.Size.width), Math.Abs(solid.Size.height)));

            var (vx, vy) = editor.MapsEditor.View.Position;
            var curSolid = solid;
            if (CanEditGlobal)
            {
                curSolid.Position = (curSolid.Position.x + vx, curSolid.Position.y + vy);
                if (curSolid.Size is { width: > 0, height: > 0 })
                    solidPack.Add(curSolid);
            }
            else if (CanDrawLayer && layer.IsHovered)
            {
                curSolid.Position = (solid.Position.x - 1, solid.Position.y - 1);
                if (curSolid.Size is { width: > 0, height: > 0 })
                    solidMap.AddSolids(currentTile, curSolid);
            }
            else if (CanEditLines)
            {
                var a = Snap((clickPos.x + vx, clickPos.y + vy));
                var b = Snap((mx + vx, my + vy));
                linePack.Add(new Line(a, b, solid.Color));
            }
        });
        Mouse.Button.Right.OnPress(() =>
        {
            if (CanEditLines)
            {
                for (var i = 0; i < linePack.Count; i++)
                {
                    var line = linePack[i];
                    var closestPoint = (Point)line.ClosestPoint(MousePos);
                    if (closestPoint.Distance(MousePos) > 1)
                        continue;

                    linePack.RemoveAt(i);
                    i--;
                }

                return;
            }

            if (CanEditGlobal)
            {
                for (var i = 0; i < solidPack.Count; i++)
                    if (solidPack[i].IsOverlapping(MousePos))
                    {
                        solidPack.RemoveAt(i);
                        i--;
                    }

                return;
            }

            if (CanDrawLayer == false)
                return;

            var rects = solidMap.SolidsIn(currentTile);
            foreach (var r in rects)
            {
                var rect = r;
                rect.Position = (rect.Position.x + 1, rect.Position.y + 1);

                if (rect.IsOverlapping(MousePosPrompt) == false)
                    continue;

                solidMap.RemoveSolids(currentTile, r);
            }
        });
    }

    [MemberNotNull(nameof(menu))]
    private static void CreateMenu()
    {
        menu = new(editor,
            "Graphics… ",
            " Load",
            "Tilemap… ",
            " Load",
            " Paste",
            "Collisions… ",
            " New",
            " Save",
            " Load",
            " Copy",
            " Paste")
        {
            Size = (11, 11),
            IsHidingOnClick = false
        };
        menu.OnItemInteraction(Interaction.Trigger, btn =>
        {
            var index = menu.Items.IndexOf(btn);
            var selection = tools.Items.IndexOf(tools.SelectedItems[0]);

            if (index == 1) // load tileset
                editor.PromptTileset(null, null);
            else if (index == 3) // load tilemap
                editor.PromptLoadMap((resultLayers, _) =>
                {
                    layers = resultLayers;
                    originalMapViewPos = editor.MapsEditor.View.Position;
                    solidMap.Update(editor.MapsEditor.Tilemaps[currentLayer]);
                });
            else if (index == 4) // paste tilemap
                editor.PromptLoadMapBase64((resultLayers, _) =>
                {
                    layers = resultLayers;
                    originalMapViewPos = editor.MapsEditor.View.Position;
                    solidMap.Update(editor.MapsEditor.Tilemaps[currentLayer]);
                });
            else if (index == 6) // new
                editor.PromptConfirm(() =>
                {
                    if (selection == 0)
                        solidPack = new();
                    else if (selection == 1)
                        solidMap = new();
                    else if (selection == 2)
                        linePack = new();
                });
            else if (index == 7) // save
                editor.PromptFileSave(Save());
            else if (index == 8) // load
                editor.PromptFileLoad(Load);
            else if (index == 9) // copy
                Window.Clipboard = Convert.ToBase64String(Save());
            else if (index == 10) // paste
                editor.PromptBase64(() => Load(Convert.FromBase64String(editor.PromptInput.Value)));
        });
        menu.OnUpdate(() =>
        {
            menu.IsHidden = editor.Prompt.IsHidden == false;
            menu.AlignInside((1f, 1f));
        });
    }
    private static void Load(byte[] bytes)
    {
        try
        {
            var selection = tools.Items.IndexOf(tools.SelectedItems[0]);

            if (selection == 0)
                solidPack = new(bytes);
            else if (selection == 1)
                solidMap = new(bytes);
            else if (selection == 2)
                linePack = new(bytes);
        }
        catch (Exception)
        {
            editor.PromptMessage("Loading failed!");
        }
    }
    private static byte[] Save()
    {
        try
        {
            var selection = tools.Items.IndexOf(tools.SelectedItems[0]);

            // ignore editor offset, keep original tilemap's view
            var prevMapOffset = solidMap.Offset;
            var prevSolidOffset = solidPack.Position;
            var prevLineOffset = linePack.Position;

            solidPack.Position = originalMapViewPos;
            solidMap.Offset = originalMapViewPos;
            linePack.Position = originalMapViewPos;

            var data = Array.Empty<byte>();

            if (selection == 0)
                data = solidPack.ToBytes();
            else if (selection == 1)
                data = solidMap.ToBytes();
            else if (selection == 2)
                data = linePack.ToBytes();

            solidPack.Position = prevSolidOffset;
            solidMap.Offset = prevMapOffset;
            linePack.Position = prevLineOffset;

            return data;
        }
        catch (Exception)
        {
            editor.PromptMessage("Saving failed!");
            return [];
        }
    }
    private static void UpdateMap()
    {
        map.Flush();
        map.SetTile((1, 1), currentTile);
    }
    private static void OffsetLayer(int offset)
    {
        if (layers.Length == 0)
            return;

        currentLayer = (currentLayer + offset).Limit((0, layers.Length - 1), true);
    }

    private static (float x, float y) Snap((float x, float y) pair)
    {
        var l = CanDrawLayer ? layer : editor.LayerMap;
        var (sx, sy) = (1f / l.AtlasTileSize.width, 1f / l.AtlasTileSize.height);
        return (pair.x.Snap(sx), pair.y.Snap(sy));
    }
#endregion
}