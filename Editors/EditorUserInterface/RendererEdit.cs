namespace Pure.Editors.EditorUserInterface;

internal class RendererEdit : BlockPack
{
    public void BlockCreate(
        string typeName,
        (int x, int y) position,
        Span type = default,
        byte[]? bytes = default)
    {
        var block = default(Block);

        const int BACK = (int)Layer.UiBack;
        const int MIDDLE = (int)Layer.UiMiddle;

        var panel = new Panel { IsRestricted = false, SizeMinimum = (3, 3) };

        switch (typeName)
        {
            case nameof(Button):
            {
                block = bytes != null ? new(bytes) : new Button(position);
                block.OnDisplay(() => maps.SetButton((Button)block, BACK, true));
                break;
            }
            case nameof(InputBox):
            {
                block = bytes != null ? new(bytes) : new InputBox(position);
                block.OnDisplay(() => maps.SetInputBox((InputBox)block, BACK));
                break;
            }
            case nameof(Pages):
            {
                block = bytes != null ? new(bytes) : new Pages(position);
                var pages = (Pages)block;
                panel.SizeMinimum = (8, 3);
                block.OnDisplay(() => maps.SetPages(pages, BACK));
                pages.OnItemDisplay(item => maps.SetPagesItem(pages, item, MIDDLE));
                break;
            }
            case nameof(Panel):
            {
                block = bytes != null ? new(bytes) : new Panel(position);
                panel.SizeMinimum = (5, 5);
                block.OnDisplay(() => maps.SetPanel((Panel)block, BACK));
                break;
            }
            case nameof(Palette):
            {
                block = bytes != null ? new(bytes) : new Palette(position);
                var palette = (Palette)block;
                panel.SizeMinimum = (15, 5);
                block.OnDisplay(() => maps.SetPalette(palette, BACK));
                block.OnDisplay(() => maps.SetPages(palette.Brightness, BACK));
                palette.Brightness.OnItemDisplay(item =>
                    maps.SetPagesItem(palette.Brightness, item, MIDDLE));
                palette.OnColorSampleDisplay((btn, c) =>
                    maps[BACK].SetTile(btn.Position, new(Tile.SHADE_OPAQUE, (Color)c)));
                break;
            }
            case nameof(Slider):
            {
                block = bytes != null ? new(bytes) : new Slider(position);
                panel.SizeMinimum = (4, 3);
                block.OnDisplay(() => maps.SetSlider((Slider)block, BACK));
                break;
            }
            case nameof(Scroll):
            {
                block = bytes != null ? new(bytes) : new Scroll(position);
                panel.SizeMinimum = (3, 4);
                block.OnDisplay(() => maps.SetScroll((Scroll)block, BACK));
                break;
            }
            case nameof(Stepper):
            {
                block = bytes != null ? new(bytes) : new Stepper(position);
                panel.SizeMinimum = (6, 4);
                block.OnDisplay(() => maps.SetStepper((Stepper)block, BACK));
                break;
            }
            case nameof(Layout):
            {
                block = bytes != null ? new(bytes) : new Layout(position);
                var layout = (Layout)block;
                layout.OnDisplaySegment((s, i) => maps.SetLayoutSegment(s, i, true, BACK));
                break;
            }
            case nameof(List):
            {
                block = bytes != null ? new(bytes) : new List(position, 10, type);
                var list = (List)block;
                panel.SizeMinimum = (4, 4);
                list.OnDisplay(() => maps.SetList(list, BACK));
                list.OnItemDisplay(item => maps.SetListItem(list, item, MIDDLE));
                break;
            }
            case nameof(FileViewer):
            {
                block = bytes != null ? new(bytes) : new FileViewer(position);
                var fileViewer = (FileViewer)block;

                panel.SizeMinimum = (5, 5);
                block.OnDisplay(() => maps.SetFileViewer(fileViewer, BACK));
                fileViewer.FilesAndFolders.OnItemDisplay(item =>
                    maps.SetFileViewerItem(fileViewer, item, MIDDLE));
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
        Add(panel);

        DisplayInfoText("Added " + block.Text);
        Input.Focused = null;
    }
    public void BlockRemove(Block block)
    {
        var panel = this[ui.IndexOf(block)];
        Remove(panel);
        ui.Remove(block);

        if (Selected == block)
            Selected = null;
    }
    public void BlockToTop(Block block)
    {
        var panel = this[ui.IndexOf(block)];
        editUI.BringToTop(panel);
        ui.BringToTop(block);
        Selected = block;
    }

    public static void DrawGrid()
    {
        var tmapSz = maps.Size;
        var color = Color.Gray.ToDark(0.66f);
        const int LAYER = (int)Layer.Grid;
        for (var i = 0; i < tmapSz.width; i += 10)
            maps[LAYER].SetLine((i, 0), (i, tmapSz.height), new(Tile.SHADE_1, color));
        for (var i = 0; i < tmapSz.height; i += 10)
            maps[LAYER].SetLine((0, i), (tmapSz.width, i), new(Tile.SHADE_1, color, 1));

        for (var i = 0; i < tmapSz.height; i += 20)
            for (var j = 0; j < tmapSz.width; j += 20)
            {
                maps[LAYER].SetTile((j, i), new Tile(Tile.SHADE_OPAQUE, color));
                maps[LAYER].SetTextLine((j + 1, i + 1), $"{j}, {i}", color);
            }
    }

#region Backend
    private void OnPanelPress(Panel panel)
    {
        var notOverEditPanel = editPanel.IsHovered == false || editPanel.IsHidden;
        var isHoveringMenu = false;
        foreach (var kvp in menus)
            if (kvp.Value is { IsHovered: true, IsHidden: false })
            {
                isHoveringMenu = true;
                break;
            }

        if (prompt.IsHidden == false || notOverEditPanel == false || isHoveringMenu)
            return;

        Selected = ui[IndexOf(panel)];
        panel.IsHidden = false;
    }
    private void OnPanelDisplay(Panel panel)
    {
        var index = IndexOf(panel);
        var e = ui[index];

        e.Position = (panel.Position.x + 1, panel.Position.y + 1);
        e.Size = (panel.Size.width - 2, panel.Size.height - 2);

        var offset = (panel.Size.width - panel.Text.Length) / 2;
        offset = Math.Max(offset, 0);
        var textPos = (panel.Position.x + offset, panel.Position.y);
        const int CORNER = Tile.BOX_GRID_CORNER;
        const int STRAIGHT = Tile.BOX_GRID_STRAIGHT;

        if (Selected != e)
            return;

        var back = maps[(int)Layer.EditBack];
        var middle = maps[(int)Layer.EditMiddle];
        back.SetBox(panel.Position, panel.Size, Tile.SHADE_TRANSPARENT, CORNER, STRAIGHT, Color.Cyan);
        back.SetRectangle(textPos, (panel.Text.Length, 1), default);
        middle.SetTextLine(textPos, panel.Text, Color.Cyan);

        var (x, y) = (panel.Position.x, panel.Position.y - 1);
        var curX = 0;
        if (Selected.IsDisabled)
        {
            back.SetTile((x, y), new(Tile.LOWERCASE_X, Color.Red));
            curX++;
        }

        if (Selected.IsHidden == false)
            return;

        var pos = (x + curX, y);
        back.SetTile(pos, new(Tile.ICON_EYE_OPENED, Color.Red));
    }
    private static void OnPanelResize(Panel panel, (int width, int height) delta)
    {
        DisplayInfoText($"{panel.Text} {panel.Size.width - 2}x{panel.Size.height - 2}");
    }
    private static void OnDragPanel(Panel panel, (int width, int height) delta)
    {
        DisplayInfoText($"{panel.Text} {panel.Position.x + 1}, {panel.Position.y + 1}");
    }
#endregion
}