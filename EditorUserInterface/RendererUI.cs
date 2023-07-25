using static Pure.EditorUserInterface.Program;

namespace Pure.EditorUserInterface;

using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;

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

        SetBackground(Layer.UiBack, e);
        SetBackground(Layer.UiMiddle, e);
        middle.SetTextLine((x + offX, y + offY), e.Text, color, e.Size.width);
    }
    protected override void OnDisplayInputBox(InputBox inputBox)
    {
        var e = inputBox;
        var middle = tilemaps[(int)Layer.UiMiddle];

        SetBackground(Layer.UiBack, e);
        SetBackground(Layer.UiMiddle, e);

        middle.SetTextRectangle(e.Position, e.Size, e.Text, isWordWrapping: false);

        if (string.IsNullOrWhiteSpace(e.Text))
            middle.SetTextRectangle(e.Position, e.Size, e.Placeholder, Color.Gray.ToBright(), false);
    }
    protected override void OnDisplaySlider(Slider slider)
    {
        var e = slider;
        var middle = tilemaps[(int)Layer.UiMiddle];

        SetBackground(Layer.UiBack, e);
        middle.SetBar(e.Handle.Position, Tile.BAR_DEFAULT_EDGE, Tile.BAR_DEFAULT_STRAIGHT,
            Color.White, e.Size.height, true);
    }
    protected override void OnDisplayList(List list)
    {
        SetBackground(Layer.UiBack, list);
        SetBackground(Layer.UiMiddle, list);
        OnDisplayScroll(list.Scroll);
    }
    protected override void OnDisplayListItem(List list, Button item)
    {
        var color = item.IsSelected ? Color.Green : Color.White;
        var middle = tilemaps[(int)Layer.UiMiddle];

        middle.SetTextLine(item.Position, item.Text, color, item.Size.width);

        var (itemX, itemY) = item.Position;
        var dropdownTile = new Tile(Tile.MATH_GREATER, color, 1);
        if (list.IsExpanded == false)
            middle.SetTile((itemX + item.Size.width - 1, itemY), dropdownTile);
    }
    protected override void OnDisplayPanel(Panel panel)
    {
        var e = panel;
        var offset = (e.Size.width - e.Text.Length) / 2;
        var back = tilemaps[(int)Layer.UiBack];
        var middle = tilemaps[(int)Layer.UiMiddle];
        offset = Math.Max(offset, 0);

        SetBackground(Layer.UiBack, e);
        SetBackground(Layer.UiMiddle, e);

        back.SetRectangle((e.Position.x + 1, e.Position.y), (e.Size.width - 2, 1),
            new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.2f)));
        middle.SetBox(e.Position, e.Size, Tile.SHADE_TRANSPARENT, Tile.BOX_GRID_CORNER,
            Tile.BOX_GRID_STRAIGHT, Color.Gray);
        middle.SetTextLine((e.Position.x + offset, e.Position.y), e.Text, Color.White, e.Size.width);
    }
    protected override void OnDisplayPages(Pages pages)
    {
        var middle = tilemaps[(int)Layer.UiMiddle];

        SetBackground(Layer.UiBack, pages);
        SetBackground(Layer.UiMiddle, pages);

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

        SetBackground(Layer.UiBack, stepper);
        SetBackground(Layer.UiMiddle, stepper);

        middle.SetTile(e.Decrease.Position, new(Tile.ARROW, Color.Gray, 1));
        middle.SetTile(e.Increase.Position, new(Tile.ARROW, Color.Gray, 3));
        middle.SetTextLine((e.Position.x + 2, e.Position.y), e.Text);
        middle.SetTextLine((e.Position.x + 2, e.Position.y + 1), $"{e.Value}");
    }
    protected override void OnDisplayScroll(Scroll scroll)
    {
        var scrollUpAng = (sbyte)(scroll.IsVertical ? 3 : 0);
        var scrollDownAng = (sbyte)(scroll.IsVertical ? 1 : 2);
        var scrollColor = Color.Gray;
        var middle = tilemaps[(int)Layer.UiMiddle];

        SetBackground(Layer.UiBack, scroll);
        SetBackground(Layer.UiMiddle, scroll);

        middle.SetTile(scroll.Increase.Position, new(Tile.ARROW, scrollColor, scrollUpAng));
        middle.SetTile(scroll.Slider.Handle.Position, new(Tile.SHAPE_CIRCLE, scrollColor));
        middle.SetTile(scroll.Decrease.Position, new(Tile.ARROW, scrollColor, scrollDownAng));
    }
    protected override void OnDisplayLayoutSegment(Layout layout,
        (int x, int y, int width, int height) segment, int index)
    {
        var colors = new uint[]
        {
            Color.Red, Color.Blue, Color.Green, Color.Yellow, Color.Gray,
            Color.Orange, Color.Cyan, Color.Black, Color.Azure, Color.Brown,
            Color.Magenta, Color.Purple, Color.Pink, Color.Violet
        };
        var pos = (segment.x, segment.y);
        var size = (segment.width, segment.height);
        var middle = tilemaps[(int)Layer.UiMiddle];

        var tile = new Tile(Tile.SHADE_OPAQUE, colors[index]);

        middle.SetBox(pos, size, tile, Tile.BOX_CORNER_ROUND, Tile.SHADE_OPAQUE, colors[index]);
    }

#region Backend
    private static void SetBackground(Layer layer, Element element)
    {
        var color = Color.Gray.ToDark();
        var tile = new Tile(Tile.SHADE_OPAQUE, color);
        var middle = tilemaps[(int)layer];
        var pos = element.Position;
        var size = element.Size;

        middle.SetBox(pos, size, tile, Tile.BOX_CORNER_ROUND, Tile.SHADE_OPAQUE, color);
    }
#endregion
}