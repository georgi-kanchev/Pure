using System.Text;

namespace Pure.UserInterface;

public class UserInterface
{
	public int Count => elements.Count;
	public Prompt? Prompt { get; set; }

	public Element this[int index] => elements[index];

	public UserInterface() { }
	public UserInterface(byte[] bytes)
	{
		var offset = 0;
		var count = GetInt();

		for(var i = 0; i < count; i++)
		{
			var byteCount = GetInt();

			// elementType string gets saved first for each element
			var typeStrBytesLength = GetInt();
			var typeStr = Encoding.UTF8.GetString(GetBytes(bytes, typeStrBytesLength, ref offset));

			// return the offset to where it was so the element can get loaded properly
			offset -= typeStrBytesLength + 4;
			var bElement = GetBytes(bytes, byteCount, ref offset);

			if(typeStr == nameof(Button)) Add(new Button(bElement));
			else if(typeStr == nameof(InputBox)) Add(new InputBox(bElement));
			else if(typeStr == nameof(FileViewer)) Add(new FileViewer(bElement));
			else if(typeStr == nameof(List)) Add(new List(bElement));
			else if(typeStr == nameof(Stepper)) Add(new Stepper(bElement));
			else if(typeStr == nameof(Pages)) Add(new Pages(bElement));
			else if(typeStr == nameof(Palette)) Add(new Palette(bElement));
			else if(typeStr == nameof(Panel)) Add(new Panel(bElement));
			else if(typeStr == nameof(Scroll)) Add(new Scroll(bElement));
			else if(typeStr == nameof(Slider)) Add(new Slider(bElement));
			else if(typeStr == nameof(Layout)) Add(new Layout(bElement));
		}

		return;

		int GetInt() => BitConverter.ToInt32(GetBytes(bytes, 4, ref offset));
	}

	public void Add(params Element[] elements)
	{
		if(elements == null || elements.Length == 0)
			return;

		foreach(var element in elements)
		{
			this.elements.Add(element);

			if(element is Layout layout)
				layout.ui = this;

			foreach(var kvp in interactions)
				element.OnInteraction(kvp.Key, () => kvp.Value.Invoke(element));

			if(element is List list)
				foreach(var kvp in itemInteractions)
					list.OnItemInteraction(kvp.Key, item => kvp.Value.Invoke(list, item));
		}
	}
	public void Remove(params Element[] elements)
	{
		if(elements == null || elements.Length == 0)
			return;

		foreach(var element in elements)
		{
			if(element is Layout layout)
				layout.ui = null;

			this.elements.Remove(element);
		}
	}
	public void BringToTop(params Element[] elements)
	{
		if(elements == null || elements.Length == 0)
			return;

		foreach(var element in elements)
		{
			Remove(element);
			Add(element);
		}
	}

	public int IndexOf(Element? element) => element == null ? -1 : elements.IndexOf(element);
	public bool IsContaining(Element? element) => element != null && elements.Contains(element);

	public void Update()
	{
		foreach(var e in elements)
			UpdateElement(e);

		Prompt?.Update(this);
	}

	public byte[] ToBytes()
	{
		var result = new List<byte>();
		result.AddRange(BitConverter.GetBytes(elements.Count));

		foreach(var element in elements)
		{
			var bytes = element.ToBytes();
			result.AddRange(BitConverter.GetBytes(bytes.Length));
			result.AddRange(bytes);
		}

		return result.ToArray();
	}

	public void OnDisplay(Action<Element> method)
	{
		displays += method;
	}
	public void OnInteraction(Interaction interaction, Action<Element> method)
	{
		if(interactions.TryAdd(interaction, method) == false)
			interactions[interaction] += method;

		foreach(var element in elements)
			element.OnInteraction(interaction, () => method.Invoke(element));
	}
	public void OnItemDisplay(Action<List, Button> method)
	{
		itemDisplays += method;
	}
	public void OnItemInteraction(Interaction interaction, Action<List, Button> method)
	{
		if(itemInteractions.TryAdd(interaction, method) == false)
			itemInteractions[interaction] += method;

		foreach(var element in elements)
			if(element is List list)
				list.OnItemInteraction(interaction, item => method.Invoke(list, item));
	}
	public void OnItemInteraction(Interaction interaction, Action<Pages, Button> method)
	{
		if(pageInteractions.TryAdd(interaction, method) == false)
			pageInteractions[interaction] += method;

		foreach(var element in elements)
			if(element is Pages pages)
				pages.OnItemInteraction(interaction, item => method.Invoke(pages, item));
	}

	// protected virtual void OnUserActionButton(Button button, Interaction userAction) { }
	// protected virtual void OnUserActionInputBox(InputBox inputBox, Interaction userAction) { }
	// protected virtual void OnUserActionList(List list, Interaction userAction) { }
	// protected virtual void OnUserActionStepper(Stepper stepper, Interaction userAction) { }
	// protected virtual void OnUserActionPages(Pages pages, Interaction userAction) { }
	// protected virtual void OnUserActionPalette(Palette palette, Interaction userAction) { }
	// protected virtual void OnUserActionPanel(Panel panel, Interaction userAction) { }
	// protected virtual void OnUserActionScroll(Scroll scroll, Interaction userAction) { }
	// protected virtual void OnUserActionSlider(Slider slider, Interaction userAction) { }
	// protected virtual void OnUserActionLayout(Layout layout, Interaction userAction) { }
	// protected virtual void OnUserActionFileViewer(FileViewer fileViewer, Interaction userAction) { }
	// 
	// protected virtual void OnDragButton(Button button, (int width, int height) delta) { }
	// protected virtual void OnDragInputBox(InputBox inputBox, (int width, int height) delta) { }
	// protected virtual void OnDragList(List list, (int width, int height) delta) { }
	// protected virtual void OnDragStepper(Stepper stepper, (int width, int height) delta) { }
	// protected virtual void OnDragPages(Pages pages, (int width, int height) delta) { }
	// protected virtual void OnDragPalette(Palette palette, (int width, int height) delta) { }
	// protected virtual void OnDragPanel(Panel panel, (int width, int height) delta) { }
	// protected virtual void OnDragScroll(Scroll scroll, (int width, int height) delta) { }
	// protected virtual void OnDragSlider(Slider slider, (int width, int height) delta) { }
	// protected virtual void OnDragLayout(Layout layout, (int width, int height) delta) { }
	// protected virtual void OnDragFileViewer(FileViewer fileViewer, (int width, int height) delta) { }
	// 
	// protected virtual void OnDisplayButton(Button button) { }
	// protected virtual void OnDisplayInputBox(InputBox inputBox) { }
	// protected virtual void OnDisplayList(List list) { }
	// protected virtual void OnDisplayListItem(List list, Button item) { }
	// protected virtual void OnDisplayStepper(Stepper stepper) { }
	// protected virtual void OnDisplayPages(Pages pages) { }
	// protected virtual void OnDisplayPagesPage(Pages pages, Button page) { }
	// protected virtual void OnDisplayPalette(Palette palette) { }
	// protected virtual void OnDisplayPalettePage(Palette palette, Button page) { }
	// protected virtual void OnUpdatePaletteSample(Palette palette, Button sample, uint color) { }
	// protected virtual void OnDisplayPanel(Panel panel) { }
	// protected virtual void OnDisplayScroll(Scroll scroll) { }
	// protected virtual void OnDisplaySlider(Slider slider) { }
	// protected virtual void OnDisplayFileViewer(FileViewer fileViewer) { }
	// protected virtual void OnDisplayFileViewerItem(FileViewer fileViewer, Button item) { }
	// protected virtual void OnDisplayLayout(Layout layout) { }
	// protected virtual void OnDisplayLayoutSegment(Layout layout, (int x, int y, int width, int height) segment, int index) { }

	// protected virtual uint OnPalettePick(Palette palette, (float x, float y) position) => default;
	// protected virtual void OnListItemTrigger(List list, Button item) { }
	// protected virtual void OnListItemSelect(List list, Button item) { }
	// protected virtual void OnFileViewerItemTrigger(FileViewer fileViewer, Button item) { }
	// protected virtual void OnFileViewerItemSelect(FileViewer fileViewer, Button item) { }
	// protected virtual void OnPanelResize(Panel panel, (int width, int height) delta) { }

	#region Backend
	private readonly List<Element> elements = new();

	private Action<Element>? displays;
	private readonly Dictionary<Interaction, Action<Element>> interactions = new();
	private readonly Dictionary<Interaction, Action<List, Button>> itemInteractions = new();
	private readonly Dictionary<Interaction, Action<Pages, Button>> pageInteractions = new();
	private Action<List, Button>? itemDisplays;

	internal void UpdateElement(Element e)
	{
		//if(e is Button button)
		//{
		//	button.display = () => OnDisplayButton(button);
		//	button.drag = d => OnDragButton(button, d);
		//}
		//else if(e is InputBox input)
		//{
		//	input.display = () => OnDisplayInputBox(input);
		//	input.drag = d => OnDragInputBox(input, d);
		//}
		//else if(e is FileViewer fileViewer)
		//{
		//	fileViewer.display += () => OnDisplayFileViewer(fileViewer);
		//	fileViewer.drag += d => OnDragFileViewer(fileViewer, d);
		//}
		//else if(e is List list)
		//{
		//	list.display = () => OnDisplayList(list);
		//	list.drag = d => OnDragList(list, d);
		//}
		//else if(e is Pages pages)
		//{
		//	pages.display = () => OnDisplayPages(pages);
		//	pages.pageDisplayCallback = b => OnDisplayPagesPage(pages, b);
		//	pages.drag = d => OnDragPages(pages, d);
		//}
		//else if(e is Palette palette)
		//{
		//	palette.display = () => OnDisplayPalette(palette);
		//	palette.pageDisplayCallback = b => OnDisplayPalettePage(palette, b);
		//	palette.sampleDisplayCallback = (b, c) => OnUpdatePaletteSample(palette, b, c);
		//	palette.pickCallback = p => OnPalettePick(palette, p);
		//	palette.drag = d => OnDragPalette(palette, d);
		//}
		//else if(e is Panel panel)
		//{
		//	panel.display = () => OnDisplayPanel(panel);
		//	panel.drag = d => OnDragPanel(panel, d);
		//	panel.resizeCallback = d => OnPanelResize(panel, d);
		//}
		//else if(e is Scroll scroll)
		//{
		//	scroll.display = () => OnDisplayScroll(scroll);
		//	scroll.drag = d => OnDragScroll(scroll, d);
		//}
		//else if(e is Stepper stepper)
		//{
		//	stepper.display = () => OnDisplayStepper(stepper);
		//	stepper.drag = d => OnDragStepper(stepper, d);
		//}
		//else if(e is Slider slider)
		//{
		//	slider.display = () => OnDisplaySlider(slider);
		//	slider.drag = d => OnDragSlider(slider, d);
		//}
		//else if(e is Layout layout)
		//{
		//	layout.display = () => OnDisplayLayout(layout);
		//	layout.segmentUpdateCallback = (seg, i) => OnDisplayLayoutSegment(layout, seg, i);
		//}

		e.Update();
	}

	private static byte[] GetBytes(byte[] fromBytes, int amount, ref int offset)
	{
		var result = fromBytes[offset..(offset + amount)];
		offset += amount;
		return result;
	}
	#endregion
}