namespace Pure.EditorUserInterface;

using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;

public class RendererUI : UserInterface
{
    protected override void OnUpdateButton(string key, Button button)
    {
        var e = button;
        var offX = (e.Size.width - e.Text.Length) / 2;
        var offY = e.Size.height / 2;
        offX = Math.Max(offX, 0);
        var (x, y) = e.Position;
        Program.tilemaps[0].SetRectangle(e.Position, e.Size, new(Tile.SHADE_OPAQUE, Color.Gray));
        Program.tilemaps[1].SetTextLine((x + offX, y + offY), e.Text, Color.White, e.Size.width);
    }
    protected override void OnUpdateInputBox(string key, InputBox inputBox)
    {
        var e = inputBox;
        Program.tilemaps[0].SetRectangle(e.Position, e.Size, new(Tile.SHADE_OPAQUE, Color.Gray));
        Program.tilemaps[1].SetTextRectangle(e.Position, e.Size, e.Text, isWordWrapping: false);

        if (string.IsNullOrWhiteSpace(e.Text) && e.CursorIndex == 0)
            Program.tilemaps[1]
                .SetTextRectangle(e.Position, e.Size, e.Placeholder, Color.Gray.ToBright(), false);
    }
    protected override void OnUpdateSlider(string key, Slider slider)
    {
        var e = slider;
        Program.tilemaps[1].SetBar(e.Position, Tile.BAR_BIG_EDGE, Tile.BAR_BIG_STRAIGHT, Color.Gray,
            e.Size.width);
        Program.tilemaps[1].SetTile(e.Handle.Position, new(Tile.SHADE_OPAQUE, Color.White));
    }
    protected override void OnUpdateList(string key, List list)
    {
        OnUpdateScroll(key, list.Scroll);
    }
    protected override void OnUpdateListItem(string key, List list, Button item)
    {
        var color = item.IsSelected ? Color.Green : Color.Gray;

        Program.tilemaps[1].SetTextLine(item.Position, item.Text, color, item.Size.width);

        var (itemX, itemY) = item.Position;
        var dropdownTile = new Tile(Tile.MATH_GREATER, color, 1);
        if (list.IsExpanded == false)
            Program.tilemaps[1].SetTile((itemX + item.Size.width - 1, itemY), dropdownTile);
    }
    protected override void OnUpdatePanel(string key, Panel panel)
    {
        var e = panel;
        var offset = (e.Size.width - e.Text.Length) / 2;
        offset = Math.Max(offset, 0);
        Program.tilemaps[0]
            .SetRectangle(e.Position, e.Size, new(Tile.SHADE_OPAQUE, Color.Gray.ToDark()));
        Program.tilemaps[2].SetBorder(e.Position, e.Size, Tile.BORDER_GRID_CORNER,
            Tile.BORDER_GRID_STRAIGHT,
            Color.Gray);
        Program.tilemaps[0].SetRectangle((e.Position.x + 1, e.Position.y), (e.Size.width - 2, 1),
            new(Tile.SHADE_OPAQUE, Color.Gray.ToDark(0.2f)));
        Program.tilemaps[2].SetTextLine((e.Position.x + offset, e.Position.y), e.Text, Color.White,
            e.Size.width);
    }
    protected override void OnUpdatePages(string key, Pages pages)
    {
        Program.tilemaps[1].SetTile(pages.First.Position, new(Tile.MATH_MUCH_LESS, Color.Gray));
        Program.tilemaps[1].SetTile(pages.Previous.Position, new(Tile.MATH_LESS, Color.Gray));
        Program.tilemaps[1].SetTile(pages.Next.Position, new(Tile.MATH_GREATER, Color.Gray));
        Program.tilemaps[1].SetTile(pages.Last.Position, new(Tile.MATH_MUCH_GREATER, Color.Gray));
    }
    protected override void OnUpdatePagesPage(string key, Pages pages, Button page)
    {
        var color = page.IsSelected ? Color.Green : Color.Gray;
        Program.tilemaps[1].SetTextLine(page.Position, page.Text, color);
    }
    protected override void OnUpdatePalette(string key, Palette palette)
    {
        var e = palette;
        var (x, y) = e.Position;
        var tile = new Tile(Tile.SHADE_OPAQUE, Color.Gray.ToBright());
        var alpha = e.Opacity;
        Program.tilemaps[1].SetBar(alpha.Position, Tile.BAR_BIG_EDGE, Tile.BAR_BIG_STRAIGHT, Color.Gray,
            alpha.Size.width);
        Program.tilemaps[1].SetTile(alpha.Handle.Position, tile);

        var first = e.Brightness.First;
        var previous = e.Brightness.Previous;
        var next = e.Brightness.Next;
        var last = e.Brightness.Last;
        Program.tilemaps[1].SetTile(first.Position, new(Tile.MATH_MUCH_LESS, Color.Gray));
        Program.tilemaps[1].SetTile(previous.Position, new(Tile.MATH_LESS, Color.Gray));
        Program.tilemaps[1].SetTile(next.Position, new(Tile.MATH_GREATER, Color.Gray));
        Program.tilemaps[1].SetTile(last.Position, new(Tile.MATH_MUCH_GREATER, Color.Gray));

        Program.tilemaps[1].SetTile(e.Pick.Position, new(Tile.MATH_PLUS, Color.Gray));
    }
    protected override void OnUpdatePalettePage(string key, Palette palette, Button page)
    {
        OnUpdatePagesPage(key, palette.Brightness, page); // display the same kind of pages
    }
    protected override void OnUpdatePaletteSample(string key, Palette palette, Button sample, uint color)
    {
        Program.tilemaps[1].SetTile(sample.Position, new(Tile.SHADE_OPAQUE, color));
    }
    protected override void OnUpdateNumericScroll(string key, NumericScroll numericScroll)
    {
        var e = numericScroll;
        Program.tilemaps[1].SetTile(e.Down.Position, new(Tile.ARROW, Color.Gray, 1));
        Program.tilemaps[1].SetTextLine((e.Position.x, e.Position.y + 1), $"{e.Value}");
        Program.tilemaps[1].SetTile(e.Up.Position, new(Tile.ARROW, Color.Gray, 3));
    }
    protected override void OnUpdateScroll(string key, Scroll scroll)
    {
        var scrollUpAng = (sbyte)(scroll.IsVertical ? 3 : 0);
        var scrollDownAng = (sbyte)(scroll.IsVertical ? 1 : 2);
        var scrollColor = Color.Gray.ToBright();
        Program.tilemaps[1].SetTile(scroll.Up.Position, new(Tile.ARROW, scrollColor, scrollUpAng));
        Program.tilemaps[1].SetTile(scroll.Slider.Handle.Position, new(Tile.SHAPE_CIRCLE, scrollColor));
        Program.tilemaps[1].SetTile(scroll.Down.Position, new(Tile.ARROW, scrollColor, scrollDownAng));
    }
}