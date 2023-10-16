namespace Pure.Examples.ExamplesUserInterface;

public static class SlidersAndScrolls
{
    public static Block[] Create(TilemapPack maps)
    {
        var sliderH = new Slider((0, 0), isVertical: false) { Size = (10, 10) };
        var sliderV = new Slider((0, 0), isVertical: true) { Size = (10, 10) };
        var scrollH = new Scroll((0, 0), isVertical: false);
        var scrollV = new Scroll((0, 0), isVertical: true);

        sliderH.Align((0.1f, 0.1f));
        sliderV.Align((0.9f, 0.1f));
        scrollH.Align((0.9f, 0.9f));
        scrollV.Align((0.1f, 0.9f));

        sliderH.OnDisplay(() => SetSlider(maps, sliderH, zOrder: 0));
        sliderV.OnDisplay(() => SetSlider(maps, sliderV, zOrder: 0));
        scrollH.OnDisplay(() => SetScroll(maps, scrollH, zOrder: 0));
        scrollV.OnDisplay(() => SetScroll(maps, scrollV, zOrder: 0));

        return new Block[] { sliderH, sliderV, scrollH, scrollV };
    }
}