using static Pure.EditorUserInterface.Program;

namespace Pure.EditorUserInterface;

using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;

public class RendererUI : UserInterface
{
    protected override void OnUpdateButton(Button button)
    {
        var e = button;
        var offX = (e.Size.width - e.Text.Length) / 2;
        var offY = e.Size.height / 2;
        offX = Math.Max(offX, 0);
        var (x, y) = e.Position;

        SetBackground(e);
        tilemaps[(int)Layer.UiMiddle]
            .SetTextLine((x + offX, y + offY), e.Text, Color.White, e.Size.width);
    }
    protected override void OnUpdateInputBox(InputBox inputBox)
    {
        var e = inputBox;
        SetBackground(e);
        tilemaps[(int)Layer.UiMiddle]
            .SetTextRectangle(e.Position, e.Size, e.Text, isWordWrapping: false);

        if (string.IsNullOrWhiteSpace(e.Text) && e.CursorIndices == 0)
            tilemaps[(int)Layer.UiMiddle]
                .SetTextRectangle(e.Position, e.Size, e.Placeholder, Color.Gray.ToBright(), false);
    }
    protected override void OnUpdateSlider(Slider slider)
    {
        var e = slider;
        tilemaps[(int)Layer.UiMiddle].SetBar(e.Position, Tile.BAR_BIG_EDGE, Tile.BAR_BIG_STRAIGHT,
            Color.Gray,
            e.Size.width);
        tilemaps[(int)Layer.UiMiddle].SetTile(e.Handle.Position, new(Tile.SHADE_OPAQUE, Color.White));
    }
    protected override void OnUpdateList(List list)
    {
        SetBackground(list);
        OnUpdateScroll(list.Scroll);
    }
    protected override void OnUpdateListItem(List list, Button item)
    {
        var color = item.IsSelected ? Color.Green : Color.Gray;

        tilemaps[(int)Layer.UiMiddle].SetTextLine(item.Position, item.Text, color, item.Size.width);

        var (itemX, itemY) = item.Position;
        var dropdownTile = new Tile(Tile.MATH_GREATER, color, 1);
        if (list.IsExpanded == false)
            tilemaps[(int)Layer.UiMiddle].SetTile((itemX + item.Size.width - 1, itemY), dropdownTile);
    }
    protected override void OnUpdatePanel(Panel panel)
    {
        var e = panel;
        var offset = (e.Size.width - e.Text.Length) / 2;
        offset = Math.Max(offset, 0);
        SetBackground(e);
        tilemaps[(int)Layer.UiMiddle].SetBox(e.Position, e.Size, Tile.SHADE_TRANSPARENT,
            Tile.BOX_GRID_CORNER, Tile.BOX_GRID_STRAIGHT, Color.Gray);
        tilemaps[(int)Layer.UiBack].SetRectangle((e.Position.x + 1, e.Position.y), (e.Size.width - 2, 1),
            new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.2f)));
        tilemaps[(int)Layer.UiMiddle].SetTextLine((e.Position.x + offset, e.Position.y), e.Text,
            Color.White, e.Size.width);
    }
    protected override void OnUpdatePages(Pages pages)
    {
        SetBackground(pages);
        tilemaps[(int)Layer.UiMiddle]
            .SetTile(pages.First.Position, new(Tile.MATH_MUCH_LESS, Color.Gray));
        tilemaps[(int)Layer.UiMiddle].SetTile(pages.Previous.Position, new(Tile.MATH_LESS, Color.Gray));
        tilemaps[(int)Layer.UiMiddle].SetTile(pages.Next.Position, new(Tile.MATH_GREATER, Color.Gray));
        tilemaps[(int)Layer.UiMiddle]
            .SetTile(pages.Last.Position, new(Tile.MATH_MUCH_GREATER, Color.Gray));
    }
    protected override void OnUpdatePagesPage(Pages pages, Button page)
    {
        var color = page.IsSelected ? Color.Green : Color.Gray;
        tilemaps[(int)Layer.UiMiddle].SetTextLine(page.Position, page.Text, color);
    }
    protected override void OnUpdatePalette(Palette palette)
    {
        var e = palette;
        var tile = new Tile(Tile.SHADE_OPAQUE, Color.Gray.ToBright());
        var alpha = e.Opacity;
        tilemaps[(int)Layer.UiMiddle].SetBar(alpha.Position, Tile.BAR_BIG_EDGE, Tile.BAR_BIG_STRAIGHT,
            Color.Gray,
            alpha.Size.width);
        tilemaps[(int)Layer.UiMiddle].SetTile(alpha.Handle.Position, tile);

        var first = e.Brightness.First;
        var previous = e.Brightness.Previous;
        var next = e.Brightness.Next;
        var last = e.Brightness.Last;
        tilemaps[(int)Layer.UiMiddle].SetTile(first.Position, new(Tile.MATH_MUCH_LESS, Color.Gray));
        tilemaps[(int)Layer.UiMiddle].SetTile(previous.Position, new(Tile.MATH_LESS, Color.Gray));
        tilemaps[(int)Layer.UiMiddle].SetTile(next.Position, new(Tile.MATH_GREATER, Color.Gray));
        tilemaps[(int)Layer.UiMiddle].SetTile(last.Position, new(Tile.MATH_MUCH_GREATER, Color.Gray));

        tilemaps[(int)Layer.UiMiddle].SetTile(e.Pick.Position, new(Tile.MATH_PLUS, Color.Gray));
    }
    protected override void OnUpdatePalettePage(Palette palette, Button page)
    {
        OnUpdatePagesPage(palette.Brightness, page); // display the same kind of pages
    }
    protected override void OnUpdatePaletteSample(Palette palette, Button sample, uint color)
    {
        tilemaps[(int)Layer.UiMiddle].SetTile(sample.Position, new(Tile.SHADE_OPAQUE, color));
    }
    protected override void OnUpdateStepper(Stepper stepper)
    {
        var e = stepper;
        SetBackground(stepper);
        tilemaps[(int)Layer.UiMiddle].SetTile(e.Down.Position, new(Tile.ARROW, Color.Gray, 1));
        tilemaps[(int)Layer.UiMiddle].SetTextLine((e.Position.x, e.Position.y + 1), $"{e.Value}");
        tilemaps[(int)Layer.UiMiddle].SetTile(e.Up.Position, new(Tile.ARROW, Color.Gray, 3));
    }
    protected override void OnUpdateScroll(Scroll scroll)
    {
        var scrollUpAng = (sbyte)(scroll.IsVertical ? 3 : 0);
        var scrollDownAng = (sbyte)(scroll.IsVertical ? 1 : 2);
        var scrollColor = Color.Gray.ToBright();
        SetBackground(scroll);
        tilemaps[(int)Layer.UiMiddle]
            .SetTile(scroll.Up.Position, new(Tile.ARROW, scrollColor, scrollUpAng));
        tilemaps[(int)Layer.UiMiddle]
            .SetTile(scroll.Slider.Handle.Position, new(Tile.SHAPE_CIRCLE, scrollColor));
        tilemaps[(int)Layer.UiMiddle]
            .SetTile(scroll.Down.Position, new(Tile.ARROW, scrollColor, scrollDownAng));
    }

#region Backend
    private static void SetBackground(Element element)
    {
        var tile = new Tile(Tile.SHADE_OPAQUE, Color.Gray.ToDark());
        tilemaps[(int)Layer.UiBack].SetRectangle(element.Position, element.Size, tile);
    }
#endregion
}