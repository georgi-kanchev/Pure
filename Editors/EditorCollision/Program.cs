using Pure.Engine.Collision;
using Pure.Engine.Utilities;

namespace Pure.Editors.EditorCollision;

using System.Diagnostics.CodeAnalysis;
using EditorBase;
using Engine.Tilemap;
using Engine.UserInterface;
using Tools.Tilemapper;
using Engine.Window;

public static class Program
{
    public static void Run()
    {
        editor.OnUpdateUi += () =>
        {
            if (isDragging)
            {
                solid.Size = (MousePos.x - solid.Position.x, MousePos.y - solid.Position.y);
                solid.Color = new Color(0, 255, 0, 100);
                editor.LayerMap.DrawRectangles(solid);
            }

            editor.LayerMap.DrawRectangles(solidPack);
        };
        editor.Run();
    }

#region Backend
    private static readonly Editor editor;
    private static Menu menu;
    private static readonly List tools;
    private static Layer layerGlobal = new(), layerPerTile = new();
    private static bool isDragging;
    private static readonly SolidPack solidPack = new();
    private static Solid solid;

    private static bool CanEditGlobal
    {
        get => editor.LayerMap.IsHovered &&
               editor.Prompt.IsHidden &&
               editor.MapPanel.IsHovered == false &&
               menu.IsHovered == false &&
               tools.IsHovered == false &&
               tools[0].IsSelected;
    }
    private static (float x, float y) MousePos
    {
        get => editor.LayerMap.PixelToWorld(Mouse.CursorPosition);
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
            editor.LoadMap();
        });

        const int FRONT = (int)Editor.LayerMapsUi.Front;
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
        tools.OnItemDisplay(item => editor.MapsUi.SetListItem(tools, item, FRONT));
        editor.Ui.Add(new Block[] { tools });

        Mouse.Button.Left.OnPress(() =>
        {
            if (CanEditGlobal == false)
                return;

            isDragging = true;
            solid.Position = MousePos;
        });
        Mouse.Button.Left.OnRelease(() =>
        {
            if (CanEditGlobal == false || isDragging == false)
                return;

            isDragging = false;

            var (mx, my) = MousePos;
            if (solid.Size.width < 0)
                solid.Position = (mx, solid.Position.y);

            if (solid.Size.height < 0)
                solid.Position = (solid.Position.x, my);

            solid.Size = (Math.Abs(solid.Size.width), Math.Abs(solid.Size.height));
            solidPack.Add(solid);
        });
        Mouse.Button.Right.OnPress(() =>
        {
            if (CanEditGlobal == false)
                return;

            for (var i = 0; i < solidPack.Count; i++)
                if (solidPack[i].IsOverlapping(MousePos))
                {
                    solidPack.Remove(solidPack[i]);
                    i--;
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

            if (index == 1)
            {
            }
            else if (index == 6) // load tileset
                editor.OpenTilesetPrompt(null, null);
            else if (index == 7) // load map
            {
                editor.Prompt.Text = "Select Map File:";
                editor.MapFileViewer.IsSelectingFolders = false;
                editor.Prompt.Open(editor.MapFileViewer, i =>
                {
                    editor.Prompt.Close();

                    if (i != 0)
                        return;

                    editor.LoadMap();
                });
            }
        });
        menu.OnUpdate(() =>
        {
            menu.IsHidden = editor.Prompt.IsHidden == false;
            menu.Align((1f, 1f));
        });
    }
#endregion
}