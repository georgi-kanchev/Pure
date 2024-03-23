global using Pure.Engine.Utilities;
global using Pure.Engine.Tilemap;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Window;
global using static Pure.Editors.EditorUserInterface.Program;
global using static Pure.Tools.Tilemapper.TilemapperUserInterface;
global using Pure.Editors.EditorBase;

namespace Pure.Editors.EditorUserInterface;

public static class Program
{
    internal enum MenuType
    {
        Main,
        Add,
        AddList
    }

    internal static readonly Editor editor;

    internal static readonly BlockPack ui = new(), panels = new();
    internal static readonly Inspector inspector;
    internal static readonly Dictionary<MenuType, Menu> menus = new();

    internal static readonly Slider promptSlider;
    internal static readonly InputBox fileName;

    internal static Block? selected;

    public static void Run()
    {
        editor.OnUpdateEditor = () =>
        {
            editor.MapsEditor.Flush();

            IsInteractable = false;
            ui.Update();
            IsInteractable = true;
            panels.Update();
        };
        editor.OnUpdateUi = () =>
        {
            inspector.IsHidden = selected == null;
            if (inspector.IsHidden == false)
                inspector.Update();
        };
        editor.Run();
    }

#region Backend
    static Program()
    {
        editor = new(title: "Pure - User Interface Editor");

        var maps = editor.MapsUi;
        const int BACK = (int)Editor.LayerMapsUi.PromptBack;

        var (_, uh) = editor.MapsUi.Size;
        inspector = new(default) { Size = (16, uh) };
        inspector.AlignInside((1f, 0.5f));

        promptSlider = new() { Size = (15, 1) };
        promptSlider.OnDisplay(() => maps.SetSlider(promptSlider, BACK));

        fileName = new() { Size = (20, 1), IsSingleLine = true };
        fileName.OnDisplay(() => maps.SetInputBox(fileName, BACK));

        // submenus need higher update priority to not close upon parent menu opening them
        menus[MenuType.AddList] = new MenuAddList();
        menus[MenuType.Add] = new MenuAdd();
        menus[MenuType.Main] = new MenuMain();

        Mouse.Button.Left.OnRelease(() =>
        {
            for (var i = panels.Count - 1; i >= 0; i--)
                if (panels[i].IsOverlapping(editor.MousePositionWorld))
                    return;

            if (inspector.IsHovered == false && editor.Prompt.IsHidden)
                selected = null;
        });
    }

    internal static void BlockCreate(
        string typeName,
        (int x, int y) position,
        Span type = default,
        byte[]? bytes = default)
    {
        var block = default(Block);
        var panel = new Panel { IsRestricted = false, SizeMinimum = (3, 3) };

        switch (typeName)
        {
            case nameof(Button):
            {
                block = bytes != null ? new(bytes) : new Button(position);
                block.OnDisplay(() =>
                    editor.MapsEditor.SetButton((Button)block, isDisplayingSelection: true));
                break;
            }
            case nameof(InputBox):
            {
                block = bytes != null ? new(bytes) : new InputBox(position);
                block.OnDisplay(() => editor.MapsEditor.SetInputBox((InputBox)block));
                break;
            }
            case nameof(Pages):
            {
                block = bytes != null ? new(bytes) : new Pages(position);
                var pages = (Pages)block;
                panel.SizeMinimum = (8, 3);
                block.OnDisplay(() => editor.MapsEditor.SetPages(pages));
                pages.OnItemDisplay(item => editor.MapsEditor.SetPagesItem(pages, item));
                break;
            }
            case nameof(Panel):
            {
                block = bytes != null ? new(bytes) : new Panel(position);
                panel.SizeMinimum = (5, 5);
                block.OnDisplay(() => editor.MapsEditor.SetPanel((Panel)block));
                break;
            }
            case nameof(Palette):
            {
                const int BACK = (int)Editor.LayerMapsEditor.Back;
                block = bytes != null ? new(bytes) : new Palette(position);
                var palette = (Palette)block;
                panel.SizeMinimum = (15, 5);

                block.OnDisplay(() => editor.MapsEditor.SetPalette(palette));
                block.OnDisplay(() => editor.MapsEditor.SetPages(palette.Brightness));

                palette.Brightness.OnItemDisplay(item =>
                    editor.MapsEditor.SetPagesItem(palette.Brightness, item));
                palette.OnSampleDisplay((btn, c) =>
                    editor.MapsEditor[BACK].SetTile(btn.Position, new(Tile.SHADE_OPAQUE, (Color)c)));
                break;
            }
            case nameof(Slider):
            {
                block = bytes != null ? new(bytes) : new Slider(position);
                panel.SizeMinimum = (4, 3);
                block.OnDisplay(() => editor.MapsEditor.SetSlider((Slider)block));
                break;
            }
            case nameof(Scroll):
            {
                block = bytes != null ? new(bytes) : new Scroll(position);
                panel.SizeMinimum = (3, 4);
                block.OnDisplay(() => editor.MapsEditor.SetScroll((Scroll)block));
                break;
            }
            case nameof(Stepper):
            {
                block = bytes != null ? new(bytes) : new Stepper(position);
                panel.SizeMinimum = (6, 4);
                block.OnDisplay(() => editor.MapsEditor.SetStepper((Stepper)block));
                break;
            }
            case nameof(Layout):
            {
                block = bytes != null ? new(bytes) : new Layout(position);
                var layout = (Layout)block;
                layout.OnDisplaySegment((s, i) => editor.MapsEditor.SetLayoutSegment(s, i, true));
                break;
            }
            case nameof(List):
            {
                block = bytes != null ? new(bytes) : new List(position, 10, type);
                var list = (List)block;
                panel.SizeMinimum = (4, 4);
                list.OnDisplay(() => editor.MapsEditor.SetList(list));
                list.OnItemDisplay(item => editor.MapsEditor.SetListItem(list, item));
                break;
            }
            case nameof(FileViewer):
            {
                block = bytes != null ? new(bytes) : new FileViewer(position);
                var fileViewer = (FileViewer)block;

                panel.SizeMinimum = (5, 5);
                block.OnDisplay(() => editor.MapsEditor.SetFileViewer(fileViewer));
                fileViewer.FilesAndFolders.OnItemDisplay(item =>
                    editor.MapsEditor.SetFileViewerItem(fileViewer, item));
                fileViewer.HardDrives.OnItemDisplay(item =>
                    editor.MapsEditor.SetFileViewerItem(fileViewer, item));
                break;
            }
        }

        if (block == null)
            return;

        panel.OnInteraction(Interaction.Press, () => OnPanelPress(panel));
        panel.OnDisplay(() => OnPanelDisplay(panel));
        panel.OnResize(delta => OnPanelResize(panel, delta));
        panel.OnDrag(delta => OnDragPanel(panel, delta));

        panel.Size = (block.Size.width + 2, block.Size.height + 2);
        panel.Text = block.Text;
        panel.Position = (block.Position.x - 1, block.Position.y - 1);

        ui.Add(block);
        panels.Add(panel);

        editor.Log("Added " + block.Text);
        Input.Focused = null;
    }
    internal static void BlockRemove(Block block)
    {
        var panel = panels[ui.IndexOf(block)];
        panels.Remove(panel);
        ui.Remove(block);

        if (selected == block)
            selected = null;
    }
    internal static void BlockToTop(Block block)
    {
        var panel = panels[ui.IndexOf(block)];
        panels.OrderToFront(panel);
        ui.OrderToFront(block);
        selected = block;
    }

    private static void OnPanelPress(Panel panel)
    {
        var notOverEditPanel = inspector.IsHovered == false || inspector.IsHidden;
        var isHoveringMenu = false;
        foreach (var kvp in menus)
            if (kvp.Value is { IsHovered: true, IsHidden: false })
            {
                isHoveringMenu = true;
                break;
            }

        if (editor.Prompt.IsHidden == false || notOverEditPanel == false || isHoveringMenu)
            return;

        selected = ui[panels.IndexOf(panel)];
        panel.IsHidden = false;
    }
    private static void OnPanelDisplay(Panel panel)
    {
        var index = panels.IndexOf(panel);
        var e = ui[index];

        e.Position = (panel.Position.x + 1, panel.Position.y + 1);
        e.Size = (panel.Size.width - 2, panel.Size.height - 2);

        var offset = (panel.Size.width - panel.Text.Length) / 2;
        offset = Math.Max(offset, 0);
        var textPos = (panel.Position.x + offset, panel.Position.y);
        const int CORNER = Tile.BOX_GRID_CORNER;
        const int STRAIGHT = Tile.BOX_GRID_STRAIGHT;

        if (selected != e)
            return;

        var back = editor.MapsEditor[(int)Editor.LayerMapsEditor.Back];
        back.SetBox(panel.Area, Tile.SHADE_TRANSPARENT, CORNER, STRAIGHT, Color.Cyan);
        back.SetArea((textPos.Item1, textPos.y, panel.Text.Length, 1), default);
        back.SetTextLine(textPos, panel.Text, Color.Cyan);

        var (x, y) = (panel.Position.x, panel.Position.y - 1);
        var curX = 0;
        if (selected.IsDisabled)
        {
            back.SetTile((x, y), new(Tile.LOWERCASE_X, Color.Red));
            curX++;
        }

        if (selected.IsHidden == false)
            return;

        var pos = (x + curX, y);
        back.SetTile(pos, new(Tile.ICON_EYE_OPENED, Color.Red));
    }
    private static void OnPanelResize(Panel panel, (int width, int height) delta)
    {
        editor.Log($"{panel.Text} {panel.Size.width - 2}x{panel.Size.height - 2}");
    }
    private static void OnDragPanel(Panel panel, (int width, int height) delta)
    {
        editor.Log($"{panel.Text} {panel.Position.x + 1}, {panel.Position.y + 1}");
    }
#endregion
}