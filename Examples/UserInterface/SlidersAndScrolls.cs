namespace Pure.Examples.UserInterface;

public static class SlidersAndScrolls
{
	public static Block[] Create(List<TileMap> maps)
	{
		Window.Title = "Pure - Sliders & Scrolls Example";

		var sliderH = new Slider((0, 0), false);
		var sliderV = new Slider((0, 0), true);
		var scrollH = new Scroll((0, 0), false) { Ratio = 0.5f };
		var scrollV = new Scroll((0, 0), true) { Ratio = 0.2f };

		sliderH.AlignInside((0.1f, 0.1f));
		sliderV.AlignInside((0.9f, 0.1f));
		scrollH.AlignInside((0.9f, 0.9f));
		scrollV.AlignInside((0.1f, 0.9f));

		sliderH.OnDisplay += () => maps.SetSlider(sliderH);
		sliderV.OnDisplay += () => maps.SetSlider(sliderV);
		scrollH.OnDisplay += () => maps.SetScroll(scrollH);
		scrollV.OnDisplay += () => maps.SetScroll(scrollV);

		return [sliderH, sliderV, scrollH, scrollV];
	}
}