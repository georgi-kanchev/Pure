namespace Pure.EditorUserInterface;

using Tilemap;
using UserInterface;
using Utilities;
using static Program;

public class RendererUI : UserInterface
{
    protected override void OnDisplayButton(Button button)
    {
        var e = button;
        var color = e.IsSelected ? Color.Green : Color.White;
        var (x, y) = e.Position;
        var middle = tilemaps[(int)Layer.UiMiddle];
        var offX = (e.Size.width - e.Text.Length) / 2;
        var offY = e.Size.height / 2;
        offX = Math.Max(offX, 0);

        SetClear(Layer.UiMiddle, e);
        SetClear(Layer.UiFront, e);

        SetBackground(Layer.UiBack, e, Color.Gray.ToDark());

        middle.SetTextLine((x + offX, y + offY), e.Text, color, e.Size.width);
    }
    protected override void OnDisplayInputBox(InputBox inputBox)
    {
        var e = inputBox;
        var middle = tilemaps[(int)Layer.UiMiddle];

        SetClear(Layer.UiMiddle, e);
        SetClear(Layer.UiFront, e);

        SetBackground(Layer.UiBack, e, Color.Gray.ToDark());

        middle.SetTextRectangle(e.Position, e.Size, e.Text, isWordWrapping: false);

        if (string.IsNullOrWhiteSpace(e.Text))
            middle.SetTextRectangle(e.Position, e.Size, e.Placeholder, Color.Gray.ToBright(), false);
    }
    protected override void OnDisplaySlider(Slider slider)
    {
        var e = slider;
        var middle = tilemaps[(int)Layer.UiMiddle];
        var isHorizontal = e.IsVertical == false;

        SetClear(Layer.UiMiddle, e);
        SetClear(Layer.UiFront, e);

        SetBackground(Layer.UiBack, e, Color.Gray.ToDark());

        middle.SetBar(e.Handle.Position, Tile.BAR_DEFAULT_EDGE, Tile.BAR_DEFAULT_STRAIGHT,
            Color.White, isHorizontal ? e.Size.height : e.Size.width, isHorizontal);
    }
    protected override void OnDisplayList(List list)
    {
        var front = tilemaps[(int)Layer.UiFront];

        SetClear(Layer.UiMiddle, list);
        SetClear(Layer.UiFront, list);
        SetBackground(Layer.UiBack, list, Color.Gray.ToDark());

        OnDisplayScroll(list.Scroll);

        var dropdownTile = new Tile(Tile.MATH_GREATER, Color.White, 1);
        if (list.IsExpanded == false)
            front.SetTile((list.Position.x + list.Size.width - 1, list.Position.y), dropdownTile);
    }
    protected override void OnDisplayListItem(List list, Button item)
    {
        var color = item.IsSelected ? Color.Green : Color.White;
        var front = tilemaps[(int)Layer.UiFront];

        SetBackground(Layer.UiMiddle, item, Color.Gray);
        front.SetTextLine(item.Position, item.Text, color, item.Size.width);
    }
    protected override void OnDisplayPanel(Panel panel)
    {
        var e = panel;
        var offset = (e.Size.width - e.Text.Length) / 2;
        var back = tilemaps[(int)Layer.UiBack];
        var middle = tilemaps[(int)Layer.UiMiddle];
        offset = Math.Max(offset, 0);

        SetClear(Layer.UiMiddle, e);
        SetClear(Layer.UiFront, e);

        SetBackground(Layer.UiBack, e, Color.Gray.ToDark());

        back.SetRectangle((e.Position.x + 1, e.Position.y), (e.Size.width - 2, 1),
            new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.2f)));
        middle.SetBox(e.Position, e.Size, Tile.SHADE_TRANSPARENT, Tile.BOX_GRID_CORNER,
            Tile.BOX_GRID_STRAIGHT, Color.Gray);
        middle.SetTextLine((e.Position.x + offset, e.Position.y), e.Text, Color.White, e.Size.width);
    }
    protected override void OnDisplayPages(Pages pages)
    {
        var middle = tilemaps[(int)Layer.UiMiddle];

        SetClear(Layer.UiMiddle, pages);
        SetClear(Layer.UiFront, pages);

        SetBackground(Layer.UiBack, pages, Color.Gray.ToDark());

        middle.SetTile(pages.First.Position, new(Tile.MATH_MUCH_LESS, Color.Gray));
        middle.SetTile(pages.Previous.Position, new(Tile.MATH_LESS, Color.Gray));
        middle.SetTile(pages.Next.Position, new(Tile.MATH_GREATER, Color.Gray));
        middle.SetTile(pages.Last.Position, new(Tile.MATH_MUCH_GREATER, Color.Gray));
    }
    protected override void OnDisplayPagesPage(Pages pages, Button page)
    {
        var color = page.IsSelected ? Color.Green : Color.White;
        tilemaps[(int)Layer.UiMiddle].SetTextLine(page.Position, page.Text, color);
    }
    protected override void OnDisplayPalette(Palette palette)
    {
        var e = palette;
        var alpha = e.Opacity;
        var middle = tilemaps[(int)Layer.UiMiddle];
        var first = e.Brightness.First;
        var previous = e.Brightness.Previous;
        var next = e.Brightness.Next;
        var last = e.Brightness.Last;

        OnDisplaySlider(alpha);

        middle.SetTile(first.Position, new(Tile.MATH_MUCH_LESS, Color.Gray));
        middle.SetTile(previous.Position, new(Tile.MATH_LESS, Color.Gray));
        middle.SetTile(next.Position, new(Tile.MATH_GREATER, Color.Gray));
        middle.SetTile(last.Position, new(Tile.MATH_MUCH_GREATER, Color.Gray));

        middle.SetTile(e.Pick.Position, new(Tile.MATH_PLUS, Color.Gray));
    }
    protected override void OnDisplayPalettePage(Palette palette, Button page)
    {
        OnDisplayPagesPage(palette.Brightness, page); // display the same kind of pages
    }
    protected override void OnUpdatePaletteSample(Palette palette, Button sample, uint color)
    {
        tilemaps[(int)Layer.UiMiddle].SetTile(sample.Position, new(Tile.SHADE_OPAQUE, color));
    }
    protected override void OnDisplayStepper(Stepper stepper)
    {
        var e = stepper;
        var middle = tilemaps[(int)Layer.UiMiddle];
        var text = MathF.Round(e.Value, 2).Precision() == 0 ? $"{e.Value}" : $"{e.Value:F2}";

        SetClear(Layer.UiMiddle, e);
        SetClear(Layer.UiFront, e);

        SetBackground(Layer.UiBack, stepper, Color.Gray.ToDark());

        middle.SetTile(e.Decrease.Position, new(Tile.ARROW, Color.Gray, 1));
        middle.SetTile(e.Increase.Position, new(Tile.ARROW, Color.Gray, 3));
        middle.SetTextLine((e.Position.x + 2, e.Position.y), e.Text);
        middle.SetTextLine((e.Position.x + 2, e.Position.y + 1), text);
    }
    protected override void OnDisplayScroll(Scroll scroll)
    {
        var scrollUpAng = (sbyte)(scroll.IsVertical ? 3 : 0);
        var scrollDownAng = (sbyte)(scroll.IsVertical ? 1 : 2);
        var scrollColor = Color.Gray;
        var middle = tilemaps[(int)Layer.UiMiddle];
        var inc = scroll.Increase;
        var dec = scroll.Decrease;

        SetClear(Layer.UiMiddle, scroll);
        SetClear(Layer.UiFront, scroll);

        SetBackground(Layer.UiBack, scroll, Color.Gray.ToDark());

        var e = scroll.Slider;
        var isHorizontal = e.IsVertical == false;
        middle.SetBar(e.Handle.Position, Tile.BAR_DEFAULT_EDGE, Tile.BAR_DEFAULT_STRAIGHT,
            Color.White, isHorizontal ? e.Size.height : e.Size.width, isHorizontal);

        middle.SetRectangle(inc.Position, inc.Size, new(Tile.ARROW, scrollColor, scrollUpAng));
        middle.SetRectangle(dec.Position, dec.Size, new(Tile.ARROW, scrollColor, scrollDownAng));
    }
    protected override void OnDisplayFileViewer(FileViewer fileViewer)
    {
        var e = fileViewer;
        var back = tilemaps[(int)Layer.UiBack];
        var front = tilemaps[(int)Layer.UiFront];
        back.SetRectangle(e.Position, e.Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark()));
        OnDisplayScroll(e.FilesAndFolders.Scroll);

        var color = Color.Gray;
        var (x, y) = e.Back.Position;
        front.SetTile((x, y), new(Tile.ICON_BACK, color));
        front.SetTextLine((x + 1, y), e.CurrentDirectory, color, -e.Back.Size.width + 2);
    }
    protected override void OnDisplayFileViewerItem(FileViewer fileViewer, Button item)
    {
        var front = tilemaps[(int)Layer.UiFront];
        var color = item.IsSelected ? Color.Green : Color.Gray.ToBright();
        var (x, y) = item.Position;
        var icon = fileViewer.IsFolder(item)
            ? new Tile(Tile.ICON_FOLDER, Color.Yellow)
            : new(Tile.ICON_FILE, Color.Gray.ToBright());

        icon = item.Text == ".." ? Tile.ICON_BACK : icon;

        front.SetTile((x, y), icon);
        front.SetTextLine((x + 1, y), item.Text, color, item.Size.width - 1);
    }
    protected override void OnDisplayLayoutSegment(Layout layout,
        (int x, int y, int width, int height) segment, int index)
    {
        var color = new Color(
            (byte)(50, 200).Random(seed / (index + 0)),
            (byte)(50, 200).Random(seed / (index + 1)),
            (byte)(50, 200).Random(seed / (index + 2)));
        var pos = (segment.x, segment.y);
        var size = (segment.width, segment.height);
        var back = tilemaps[(int)Layer.UiBack];
        var middle = tilemaps[(int)Layer.UiMiddle];
        var tile = new Tile(Tile.SHADE_OPAQUE, color);

        back.SetBox(pos, size, tile, Tile.BOX_CORNER_ROUND, Tile.SHADE_OPAQUE, color);
        middle.SetTextLine((pos.x + size.width / 2, pos.y + size.height / 2), $"{index}");
    }

#region Backend
    private readonly float seed = (-9999999, 9999999).Random();

    private static void SetBackground(Layer layer, Element element, Color color)
    {
        var tile = new Tile(Tile.SHADE_OPAQUE, color);
        var map = tilemaps[(int)layer];
        var pos = element.Position;
        var size = element.Size;

        map.SetBox(pos, size, tile, Tile.BOX_CORNER_ROUND, Tile.SHADE_OPAQUE, color);
    }
    private static void SetClear(Layer layer, Element element)
    {
        tilemaps[(int)layer].SetBox(element.Position, element.Size, 0, 0, 0, 0);
    }
#endregion
}