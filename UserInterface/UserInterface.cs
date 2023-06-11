namespace Pure.UserInterface;

public class UserInterface<T> where T : notnull
{
	public Element this[T key]
	{
		get => elements[key];
		set
		{
			elements[key] = value;

			var userActionCount = Enum.GetNames(typeof(UserAction)).Length;
			for(int i = 0; i < userActionCount; i++)
			{
				if(value is Button b) value.SubscribeToUserAction((UserAction)i, () => OnUserActionButton(key, b, (UserAction)i));
				else if(value is InputBox e) value.SubscribeToUserAction((UserAction)i, () => OnUserActionInputBox(key, e, (UserAction)i));
				else if(value is List l) value.SubscribeToUserAction((UserAction)i, () => OnUserActionList(key, l, (UserAction)i));
				else if(value is NumericScroll n) value.SubscribeToUserAction((UserAction)i, () => OnUserActionNumericScroll(key, n, (UserAction)i));
				else if(value is Pages g) value.SubscribeToUserAction((UserAction)i, () => OnUserActionPages(key, g, (UserAction)i));
				else if(value is Palette t) value.SubscribeToUserAction((UserAction)i, () => OnUserActionPalette(key, t, (UserAction)i));
				else if(value is Panel p) value.SubscribeToUserAction((UserAction)i, () => OnUserActionPanel(key, p, (UserAction)i));
				else if(value is Scroll r) value.SubscribeToUserAction((UserAction)i, () => OnUserActionScroll(key, r, (UserAction)i));
				else if(value is Slider s) value.SubscribeToUserAction((UserAction)i, () => OnUserActionSlider(key, s, (UserAction)i));
			}
		}
	}

	public void Update()
	{
		foreach(var kvp in elements)
		{
			if(kvp.Value is List list)
				list.itemUpdateCallback = (b) => OnUpdateListItem(kvp.Key, list, b);
			else if(kvp.Value is Pages pages)
				pages.pageUpdateCallback = (b) => OnUpdatePagesPage(kvp.Key, pages, b);
			else if(kvp.Value is Palette palette)
			{
				palette.pageUpdateCallback = (b) => OnUpdatePalettePage(kvp.Key, palette, b);
				palette.sampleUpdateCallback = (b, c) => OnUpdatePaletteSample(kvp.Key, palette, b, c);
				palette.pickCallback = (p) => OnPalettePick(kvp.Key, palette, p);
			}

			kvp.Value.Update();

			if(kvp.Value is Button b) OnUpdateButton(kvp.Key, b);
			else if(kvp.Value is InputBox i) OnUpdateInputBox(kvp.Key, i);
			else if(kvp.Value is List l) OnUpdateList(kvp.Key, l);
			else if(kvp.Value is NumericScroll n) OnUpdateNumericScroll(kvp.Key, n);
			else if(kvp.Value is Pages g) OnUpdatePages(kvp.Key, g);
			else if(kvp.Value is Palette t) OnUpdatePalette(kvp.Key, t);
			else if(kvp.Value is Panel p) OnUpdatePanel(kvp.Key, p);
			else if(kvp.Value is Scroll r) OnUpdateScroll(kvp.Key, r);
			else if(kvp.Value is Slider s) OnUpdateSlider(kvp.Key, s);
		}
	}

	protected virtual void OnUserActionButton(T key, Button button, UserAction userAction) { }
	protected virtual void OnUserActionInputBox(T key, InputBox inputBox, UserAction userAction) { }
	protected virtual void OnUserActionList(T key, List list, UserAction userAction) { }
	protected virtual void OnUserActionNumericScroll(T key, NumericScroll numericScroll, UserAction userAction) { }
	protected virtual void OnUserActionPages(T key, Pages pages, UserAction userAction) { }
	protected virtual void OnUserActionPalette(T key, Palette palette, UserAction userAction) { }
	protected virtual void OnUserActionPanel(T key, Panel panel, UserAction userAction) { }
	protected virtual void OnUserActionScroll(T key, Scroll scroll, UserAction userAction) { }
	protected virtual void OnUserActionSlider(T key, Slider slider, UserAction userAction) { }

	protected virtual void OnUpdateButton(T key, Button button) { }
	protected virtual void OnUpdateInputBox(T key, InputBox inputBox) { }
	protected virtual void OnUpdateList(T key, List list) { }
	protected virtual void OnUpdateListItem(T key, List list, Button item) { }
	protected virtual void OnUpdateNumericScroll(T key, NumericScroll numericScroll) { }
	protected virtual void OnUpdatePages(T key, Pages pages) { }
	protected virtual void OnUpdatePagesPage(T key, Pages pages, Button page) { }
	protected virtual void OnUpdatePalette(T key, Palette palette) { }
	protected virtual void OnUpdatePalettePage(T key, Palette palette, Button page) { }
	protected virtual void OnUpdatePaletteSample(T key, Palette palette, Button sample, uint color) { }
	protected virtual void OnUpdatePanel(T key, Panel panel) { }
	protected virtual void OnUpdateScroll(T key, Scroll scroll) { }
	protected virtual void OnUpdateSlider(T key, Slider slider) { }

	protected virtual uint OnPalettePick(T key, Palette palette, (float x, float y) position) => default;

	#region Backend
	private readonly Dictionary<T, Element> elements = new();
	#endregion
}
