namespace Pure.Examples.Systems.UserInterface;

using Pure.UserInterface;
using Pure.Utilities;
using Tilemap;
using static Utility;

public static class SlidersAndScrolls
{
    public static Element[] Create(TilemapManager maps)
    {
        var sliderH = new Slider((0, 0), isVertical: false) { Size = (10, 10) };
        var sliderV = new Slider((0, 0), isVertical: true) { Size = (10, 10) };
        var scrollH = new Scroll((0, 0), isVertical: false);
        var scrollV = new Scroll((0, 0), isVertical: true);

        sliderH.Align((0.1f, 0.1f));
        sliderV.Align((0.9f, 0.1f));
        scrollH.Align((0.9f, 0.9f));
        scrollV.Align((0.1f, 0.9f));

        sliderH.OnDisplay(() => DisplaySlider(maps, sliderH));
        sliderV.OnDisplay(() => DisplaySlider(maps, sliderV));
        scrollH.OnDisplay(() => DisplayScroll(maps, scrollH));
        scrollV.OnDisplay(() => DisplayScroll(maps, scrollV));

        return new Element[] { sliderH, sliderV, scrollH, scrollV };
    }

    public static void DisplaySlider(TilemapManager maps, Slider slider)
    {
        var e = slider;
        var (w, h) = e.Size;
        var text = e.IsVertical ? $"{e.Progress:F2}" : $"{e.Progress * 100f:F0}%";
        var isHandle = e.Handle.IsPressedAndHeld;
        var color = GetColor(isHandle ? e.Handle : e, Color.Gray.ToBright());

        SetBackground(maps[0], e);
        maps[1].SetBar(e.Handle.Position,
            tileIdEdge: Tile.BAR_DEFAULT_EDGE,
            tileId: Tile.BAR_DEFAULT_STRAIGHT,
            color,
            size: e.Size.height,
            isVertical: e.IsVertical == false);
        maps[2].SetTextLine(
            position: (e.Position.x + w / 2 - text.Length / 2, e.Position.y + h / 2),
            text);
    }
    public static void DisplayScroll(TilemapManager maps, Scroll scroll)
    {
        var e = scroll;
        var scrollUpAng = (sbyte)(e.IsVertical ? 3 : 0);
        var scrollDownAng = (sbyte)(e.IsVertical ? 1 : 2);
        var scrollColor = Color.Gray.ToBright();
        var isHandle = e.Slider.Handle.IsPressedAndHeld;

        SetBackground(maps[0], e, 0.4f);
        maps[1].SetTile(e.Increase.Position,
            new(Tile.ARROW, GetColor(e.Increase, scrollColor), scrollUpAng));
        maps[1].SetTile(e.Slider.Handle.Position,
            new(Tile.SHAPE_CIRCLE, GetColor(isHandle ? e.Slider.Handle : e.Slider, scrollColor)));
        maps[1].SetTile(e.Decrease.Position,
            new(Tile.ARROW, GetColor(e.Decrease, scrollColor), scrollDownAng));
    }
}