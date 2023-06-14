using System.Text;

namespace Pure.UserInterface;

public class UserInterface
{
	public Element this[string key]
	{
		get => elements[key];
		set
		{
			elements[key] = value;

			var userActionCount = Enum.GetNames(typeof(UserAction)).Length;
			for (int i = 0; i < userActionCount; i++)
			{
				var act = (UserAction)i;
				if (value is Button b) value.SubscribeToUserAction(act, () => OnUserActionButton(key, b, act));
				else if (value is InputBox e) value.SubscribeToUserAction(act, () => OnUserActionInputBox(key, e, act));
				else if (value is List l) value.SubscribeToUserAction(act, () => OnUserActionList(key, l, act));
				else if (value is NumericScroll n) value.SubscribeToUserAction(act, () => OnUserActionNumericScroll(key, n, act));
				else if (value is Pages g) value.SubscribeToUserAction(act, () => OnUserActionPages(key, g, act));
				else if (value is Palette t) value.SubscribeToUserAction(act, () => OnUserActionPalette(key, t, act));
				else if (value is Panel p) value.SubscribeToUserAction(act, () => OnUserActionPanel(key, p, act));
				else if (value is Scroll r) value.SubscribeToUserAction(act, () => OnUserActionScroll(key, r, act));
				else if (value is Slider s) value.SubscribeToUserAction(act, () => OnUserActionSlider(key, s, act));
			}
		}
	}

	public UserInterface() { }
	public UserInterface(byte[] bytes)
	{
		var offset = 0;
		var count = BitConverter.ToInt32(GetBytes(bytes, 4, ref offset));

		for (int i = 0; i < count; i++)
		{
			var keyByteLength = BitConverter.ToInt32(GetBytes(bytes, 4, ref offset));
			var key = Encoding.UTF8.GetString(GetBytes(bytes, keyByteLength, ref offset));

			var byteCount = BitConverter.ToInt32(GetBytes(bytes, 4, ref offset));

			// elementType string gets saved first for each element
			var typeStrBytesLength = BitConverter.ToInt32(GetBytes(bytes, 4, ref offset));
			var typeStr = Encoding.UTF8.GetString(GetBytes(bytes, typeStrBytesLength, ref offset));

			// return the offset to where it was so the element can get loaded properly
			offset -= typeStrBytesLength + 4;
			var bElement = GetBytes(bytes, byteCount, ref offset);

			switch (typeStr)
			{
				case nameof(Button): this[key] = new Button(bElement); break;
				case nameof(InputBox): this[key] = new InputBox(bElement); break;
				case nameof(List): this[key] = new List(bElement); break;
				case nameof(NumericScroll): this[key] = new NumericScroll(bElement); break;
				case nameof(Pages): this[key] = new Pages(bElement); break;
				case nameof(Palette): this[key] = new Palette(bElement); break;
				case nameof(Panel): this[key] = new Panel(bElement); break;
				case nameof(Scroll): this[key] = new Scroll(bElement); break;
				case nameof(Slider): this[key] = new Slider(bElement); break;
			}
		}
	}

	public void Update()
	{
		foreach (var kvp in elements)
		{
			if (kvp.Value is List list)
			{
				list.itemUpdateCallback = (b) => OnUpdateListItem(kvp.Key, list, b);
				list.itemSelectCallback = (i) => OnListItemSelect(kvp.Key, list, i);
			}
			else if (kvp.Value is Pages pages)
				pages.pageUpdateCallback = (b) => OnUpdatePagesPage(kvp.Key, pages, b);
			else if (kvp.Value is Palette palette)
			{
				palette.pageUpdateCallback = (b) => OnUpdatePalettePage(kvp.Key, palette, b);
				palette.sampleUpdateCallback = (b, c) => OnUpdatePaletteSample(kvp.Key, palette, b, c);
				palette.pickCallback = (p) => OnPalettePick(kvp.Key, palette, p);
			}

			kvp.Value.Update();

			if (kvp.Value is Button b) OnUpdateButton(kvp.Key, b);
			else if (kvp.Value is InputBox i) OnUpdateInputBox(kvp.Key, i);
			else if (kvp.Value is List l) OnUpdateList(kvp.Key, l);
			else if (kvp.Value is NumericScroll n) OnUpdateNumericScroll(kvp.Key, n);
			else if (kvp.Value is Pages g) OnUpdatePages(kvp.Key, g);
			else if (kvp.Value is Palette t) OnUpdatePalette(kvp.Key, t);
			else if (kvp.Value is Panel p) OnUpdatePanel(kvp.Key, p);
			else if (kvp.Value is Scroll r) OnUpdateScroll(kvp.Key, r);
			else if (kvp.Value is Slider s) OnUpdateSlider(kvp.Key, s);
		}
	}

	public byte[] ToBytes()
	{
		var result = new List<byte>();
		result.AddRange(BitConverter.GetBytes(elements.Count));

		foreach (var kvp in elements)
		{
			var bKey = Encoding.UTF8.GetBytes(kvp.Key);
			result.AddRange(BitConverter.GetBytes(bKey.Length));
			result.AddRange(bKey);

			var bytes = kvp.Value.ToBytes();
			result.AddRange(BitConverter.GetBytes(bytes.Length));
			result.AddRange(bytes);
		}
		return result.ToArray();
	}

	protected virtual void OnUserActionButton(string key, Button button, UserAction userAction) { }
	protected virtual void OnUserActionInputBox(string key, InputBox inputBox, UserAction userAction) { }
	protected virtual void OnUserActionList(string key, List list, UserAction userAction) { }
	protected virtual void OnUserActionNumericScroll(string key, NumericScroll numericScroll, UserAction userAction) { }
	protected virtual void OnUserActionPages(string key, Pages pages, UserAction userAction) { }
	protected virtual void OnUserActionPalette(string key, Palette palette, UserAction userAction) { }
	protected virtual void OnUserActionPanel(string key, Panel panel, UserAction userAction) { }
	protected virtual void OnUserActionScroll(string key, Scroll scroll, UserAction userAction) { }
	protected virtual void OnUserActionSlider(string key, Slider slider, UserAction userAction) { }

	protected virtual void OnUpdateButton(string key, Button button) { }
	protected virtual void OnUpdateInputBox(string key, InputBox inputBox) { }
	protected virtual void OnUpdateList(string key, List list) { }
	protected virtual void OnUpdateListItem(string key, List list, Button item) { }
	protected virtual void OnUpdateNumericScroll(string key, NumericScroll numericScroll) { }
	protected virtual void OnUpdatePages(string key, Pages pages) { }
	protected virtual void OnUpdatePagesPage(string key, Pages pages, Button page) { }
	protected virtual void OnUpdatePalette(string key, Palette palette) { }
	protected virtual void OnUpdatePalettePage(string key, Palette palette, Button page) { }
	protected virtual void OnUpdatePaletteSample(string key, Palette palette, Button sample, uint color) { }
	protected virtual void OnUpdatePanel(string key, Panel panel) { }
	protected virtual void OnUpdateScroll(string key, Scroll scroll) { }
	protected virtual void OnUpdateSlider(string key, Slider slider) { }

	protected virtual uint OnPalettePick(string key, Palette palette, (float x, float y) position) => default;
	protected virtual void OnListItemSelect(string key, List list, Button item) { }

	#region Backend
	private readonly Dictionary<string, Element> elements = new();

	private byte[] GetBytes(byte[] fromBytes, int amount, ref int offset)
	{
		var result = fromBytes[offset..(offset + amount)];
		offset += amount;
		return result;
	}
	#endregion
}
