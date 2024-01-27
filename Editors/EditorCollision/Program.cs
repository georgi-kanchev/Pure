namespace Pure.Editors.EditorCollision;

using EditorBase;
using Tools.Tilemapper;
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
        editor.OnUpdateUi += () =>
        {
            if (prevViewPos != editor.MapsEditor.ViewPosition)
                UpdateViewOffsets();

            prevViewPos = editor.MapsEditor.ViewPosition;

            if (CanDrawLayer)
            {
                layer.DrawTilemap(map);
                solidMap.Update(map);
                layer.DrawRectangles(solidMap);
            }
            else
            {
                editor.LayerMap.DrawRectangles(solidPack);
                editor.LayerMap.DrawRectangles(solidMap);
            }

            if (isDragging == false)
                return;

            var pos = CanDrawLayer ? MousePosPrompt : MousePos;
            solid.Size = Snap((pos.x - solid.Position.x, pos.y - solid.Position.y));

            if (CanDrawLayer)
                layer.DrawRectangles(solid);
            else
                editor.LayerMap.DrawRectangles(solid);
        };
        editor.OnUpdateLate += () =>
        {
            if (CanDrawLayer)
                layer.Draw();
        };
        editor.Run();
    }

#region Backend
    private static (int x, int y) prevViewPos;
    private static (int width, int height) originalMapViewPos;
    private static readonly Editor editor;
    private static readonly List tools;
    private static readonly Layer layer = new();
    private static readonly Tilemap map = new((3, 3));
    private static readonly FileViewer saveLoad = new();
    private static readonly Panel promptPanel;
    private static SolidPack solidPack = new();
    private static SolidMap solidMap = new();
    private static Menu menu;
    private static bool isDragging;
    private static Solid solid;
    private static string[] layers = { "Layer1" };
    private static int currentLayer;
    private static Tile currentTile;

    private static bool CanDrawLayer
    {
        get => editor.Prompt is { IsHidden: false, ButtonCount: 3 };
    }
    private static bool CanEditGlobal
    {
        get => editor.LayerMap.IsHovered &&
               editor.Prompt.IsHidden &&
               editor.MapPanel.IsHovered == false &&
               menu.IsHovered == false &&
               tools.IsHovered == false &&
               tools[0].IsSelected;
    }
    private static bool CanEditTile
    {
        get => editor.LayerMap.IsHovered &&
               editor.Prompt.IsHidden &&
               editor.MapPanel.IsHovered == false &&
               menu.IsHovered == false &&
               tools.IsHovered == false &&
               tools[1].IsSelected;
    }
    private static (float x, float y) MousePos
    {
        get => editor.LayerMap.PixelToWorld(Mouse.CursorPosition);
    }
    private static (float x, float y) MousePosPrompt
    {
        get => layer.PixelToWorld(Mouse.CursorPosition);
    }

    static Program()
    {
        var (mw, mh) = (50, 50);

        editor = new(title: "Pure - Collision Editor", mapSize: (mw, mh), viewSize: (mw, mh));
        editor.MapsEditor.Clear();
        editor.MapsEditor.Add(new Tilemap((mw, mh)), new Tilemap((mw, mh)));
        editor.MapsEditor.ViewSize = (mw, mh);
        CreateMenu();

        editor.MapFileViewer.FilesAndFolders.OnItemInteraction(Interaction.DoubleTrigger, btn =>
        {
            if (editor.MapFileViewer.IsSelectingFolders || editor.MapFileViewer.IsFolder(btn))
                return;

            editor.Prompt.Close();
            layers = editor.LoadMap();
        });

        const int FRONT = (int)Editor.LayerMapsUi.Front;
        const int PROMPT_MIDDLE = (int)Editor.LayerMapsUi.PromptMiddle;
        const int PROMPT_BACK = (int)Editor.LayerMapsUi.PromptBack;
        tools = new(itemCount: 0)
        {
            Size = (8, 2),
            ItemSize = (8, 1),
            IsSingleSelecting = true
        };
        tools.Add(
            new Button { Text = "Per Tile" },
            new Button { Text = "Global" });
        tools.Select(0);
        tools.Align((1f, 0f));
        tools.OnDisplay(() => editor.MapsUi.SetList(tools, FRONT));
        tools.OnUpdate(() => tools.IsHidden = editor.Prompt.IsHidden == false);
        tools.OnItemDisplay(item => editor.MapsUi.SetListItem(tools, item, FRONT));
        editor.Ui.Add(new Block[] { tools });

        // override the default prompt buttons
        editor.OnPromptItemDisplay = item =>
        {
            var tiles = new Tile[]
            {
                new(Tile.ICON_TICK, Color.Green),
                new(Tile.ARROW, Color.Gray, 3),
                new(Tile.ARROW, Color.Gray, 1)
            };

            editor.MapsUi.SetPromptItem(
                editor.Prompt,
                item,
                zOrder: PROMPT_MIDDLE,
                CanDrawLayer ? tiles : null);
        };

        promptPanel = new()
        {
            Size = (30, 30),
            IsResizable = false,
            IsMovable = false
        };
        promptPanel.OnDisplay(() => editor.MapsUi.SetPanel(promptPanel, PROMPT_BACK));

        layer.TilemapSize = map.Size;
        layer.Offset = (0f, -10f);

        const int BACK = (int)Editor.LayerMapsUi.PromptBack;
        const int MIDDLE = (int)Editor.LayerMapsUi.PromptMiddle;
        saveLoad = new()
        {
            FilesAndFolders = { IsSingleSelecting = true },
            Size = (21, 16)
        };
        saveLoad.OnDisplay(() => editor.MapsUi.SetFileViewer(saveLoad, BACK));
        saveLoad.FilesAndFolders.OnItemDisplay(btn =>
            editor.MapsUi.SetFileViewerItem(saveLoad, btn, MIDDLE));
        saveLoad.HardDrives.OnItemDisplay(btn =>
            editor.MapsUi.SetFileViewerItem(saveLoad, btn, MIDDLE));
        saveLoad.FilesAndFolders.OnItemInteraction(Interaction.DoubleTrigger, item =>
        {
            if (saveLoad.IsFolder(item) == false)
                editor.Prompt.TriggerButton(0);
        });

        SubscribeToClicks();
    }

    private static void SubscribeToClicks()
    {
        Mouse.Button.Left.OnPress(() =>
        {
            if (CanEditGlobal)
            {
                isDragging = true;
                solid.Position = Snap(MousePos);
                solid.Color = new Color((uint)Color.Green) { A = 100 };
                return;
            }

            if (CanDrawLayer)
            {
                isDragging = true;
                solid.Position = Snap(MousePosPrompt);
                solid.Color = new Color((uint)Color.Red) { A = 100 };
                return;
            }

            if (CanEditTile == false)
                return;

            var (x, y) = MousePos;
            x += editor.MapsEditor.ViewPosition.x;
            y += editor.MapsEditor.ViewPosition.y;
            editor.Prompt.ButtonCount = 3;
            currentTile = editor.MapsEditor[currentLayer].TileAt(((int)x, (int)y));

            layer.TileGap = editor.LayerMap.TileGap;
            layer.TilesetPath = editor.LayerMap.TilesetPath;
            layer.TileSize = editor.LayerMap.TileSize;
            layer.TileIdFull = editor.LayerMap.TileIdFull;
            var (tw, th) = layer.TileSize;
            var ratio = MathF.Max(tw / 8f, th / 8f);
            var zoom = 28f / ratio;
            layer.Zoom = zoom;

            UpdateMap();
            promptPanel.Text = layers[currentLayer];
            editor.Prompt.Text = "Edit Tile Solids";
            editor.Prompt.Open(promptPanel, i =>
            {
                if (i == 0)
                {
                    editor.Prompt.Close();
                    solidMap.Update(editor.MapsEditor[currentLayer]);
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
                solid.Position = Snap((mx, solid.Position.y));

            if (solid.Size.height < 0)
                solid.Position = Snap((solid.Position.x, my));

            solid.Size = Snap((Math.Abs(solid.Size.width), Math.Abs(solid.Size.height)));

            if (CanEditGlobal)
            {
                var curSolid = solid;
                curSolid.Position = Snap(curSolid.Position);
                curSolid.Size = Snap(curSolid.Size);
                if (curSolid.Size is { width: > 0, height: > 0 })
                    solidPack.Add(curSolid);
            }
            else if (CanDrawLayer)
            {
                var curSolid = solid;
                curSolid.Position = Snap((solid.Position.x - 1, solid.Position.y - 1));
                curSolid.Size = Snap(curSolid.Size);
                if (curSolid.Size is { width: > 0, height: > 0 })
                    solidMap.SolidsAdd(currentTile, curSolid);
            }
        });
        Mouse.Button.Right.OnPress(() =>
        {
            if (CanEditGlobal)
            {
                for (var i = 0; i < solidPack.Count; i++)
                    if (solidPack[i].IsOverlapping(MousePos))
                    {
                        solidPack.Remove(solidPack[i]);
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

                solidMap.SolidsRemove(currentTile, r);
            }
        });
    }

    [MemberNotNull(nameof(menu))]
    private static void CreateMenu()
    {
        menu = new(editor,
            "Save… ",
            " Solids Global",
            " Solids Map",
            "Load… ",
            " Solids Global",
            " Solids Map",
            " Tileset",
            " Tilemap")
        {
            Size = (14, 8),
            IsHidingOnClick = false
        };
        menu.OnItemInteraction(Interaction.Trigger, btn =>
        {
            var index = menu.IndexOf(btn);

            if (index is 1 or 2) // save solids
            {
                saveLoad.IsSelectingFolders = true;
                editor.Prompt.Text = "Select a Directory:";
                editor.Prompt.ButtonCount = 2;
                editor.Prompt.Open(saveLoad, i =>
                {
                    editor.Prompt.Close();

                    if (i != 0)
                        return;

                    editor.Prompt.Text = "Provide a File Name:";
                    editor.Prompt.Open(editor.PromptInput, btnIndex =>
                    {
                        editor.Prompt.Close();
                        if (btnIndex != 0)
                            return;

                        var paths = saveLoad.SelectedPaths;
                        var directory = paths.Length == 1 ? paths[0] : saveLoad.CurrentDirectory;
                        var path = Path.Combine($"{directory}", editor.PromptInput.Value);

                        // ignore editor offset, keep original tilemap's view
                        var prevMapOffset = solidMap.Offset;
                        var prevGlobalOffset = solidPack.Offset;

                        solidPack.Offset = originalMapViewPos;
                        solidMap.Offset = originalMapViewPos;

                        var data = index == 1 ? solidPack.ToBytes() : solidMap.ToBytes();
                        File.WriteAllBytes(path, data);

                        solidPack.Offset = prevGlobalOffset;
                        solidMap.Offset = prevMapOffset;
                    });
                });
            }
            else if (index is 4 or 5) // load solids
            {
                saveLoad.IsSelectingFolders = false;
                editor.Prompt.Text = "Select Solids File:";
                editor.Prompt.ButtonCount = 2;
                editor.Prompt.Open(saveLoad, i =>
                {
                    editor.Prompt.Close();

                    if (i != 0)
                        return;

                    var selectedPaths = saveLoad.SelectedPaths;
                    var file = selectedPaths.Length == 1 ? selectedPaths[0] : saveLoad.CurrentDirectory;
                    var bytes = File.ReadAllBytes($"{file}");

                    if (index == 4)
                        solidPack = new(bytes);
                    else
                    {
                        solidMap = new(bytes);
                        UpdateViewOffsets();
                    }
                });
            }
            else if (index == 6) // load tileset
                editor.OpenTilesetPrompt(null, null);
            else if (index == 7) // load map
            {
                editor.Prompt.Text = "Select Map File:";
                editor.MapFileViewer.IsSelectingFolders = false;
                editor.Prompt.ButtonCount = 2;
                editor.Prompt.Open(editor.MapFileViewer, i =>
                {
                    editor.Prompt.Close();

                    if (i != 0)
                        return;

                    layers = editor.LoadMap();
                    originalMapViewPos = editor.MapsEditor.ViewPosition;
                    solidMap.Update(editor.MapsEditor[currentLayer]);
                });
            }
        });
        menu.OnUpdate(() =>
        {
            menu.IsHidden = editor.Prompt.IsHidden == false;
            menu.Align((1f, 1f));
        });
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

    private static void UpdateViewOffsets()
    {
        var (x, y) = editor.MapsEditor.ViewPosition;
        solidPack.Offset = (-x, -y);
        solidMap.Offset = (-x, -y);
        solidMap.Update(editor.MapsEditor[currentLayer]);
    }

    private static (float x, float y) Snap((float x, float y) pair)
    {
        var l = CanDrawLayer ? layer : editor.LayerMap;
        var (sx, sy) = (1f / l.TileSize.width, 1f / l.TileSize.height);
        return (pair.x.Snap(sx), pair.y.Snap(sy));
    }
#endregion
}