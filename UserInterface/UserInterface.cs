using System.Text;

namespace Pure.UserInterface;

public class UserInterface
{
    public int Count => elements.Count;

    public Element this[int index] => elements[index];

    public UserInterface() { }
    public UserInterface(byte[] bytes)
    {
        var offset = 0;
        var count = GetInt();

        for (var i = 0; i < count; i++)
        {
            var byteCount = GetInt();

            // elementType string gets saved first for each element
            var typeStrBytesLength = GetInt();
            var typeStr = Encoding.UTF8.GetString(GetBytes(bytes, typeStrBytesLength, ref offset));

            // return the offset to where it was so the element can get loaded properly
            offset -= typeStrBytesLength + 4;
            var bElement = GetBytes(bytes, byteCount, ref offset);

            if (typeStr == nameof(Button)) Add(new Button(bElement));
            else if (typeStr == nameof(InputBox)) Add(new InputBox(bElement));
            else if (typeStr == nameof(List)) Add(new List(bElement));
            else if (typeStr == nameof(Stepper)) Add(new Stepper(bElement));
            else if (typeStr == nameof(Pages)) Add(new Pages(bElement));
            else if (typeStr == nameof(Palette)) Add(new Palette(bElement));
            else if (typeStr == nameof(Panel)) Add(new Panel(bElement));
            else if (typeStr == nameof(Scroll)) Add(new Scroll(bElement));
            else if (typeStr == nameof(Slider)) Add(new Slider(bElement));
        }

        int GetInt() => BitConverter.ToInt32(GetBytes(bytes, 4, ref offset));
    }

    public void Add(Element element)
    {
        elements.Add(element);

        if (element is Layout layout)
            layout.ui = this;

        var userActionCount = Enum.GetNames(typeof(UserAction)).Length;
        for (var i = 0; i < userActionCount; i++)
        {
            var act = (UserAction)i;
            if (element is Button b)
                element.SubscribeToUserAction(act, () => OnUserActionButton(b, act));
            else if (element is InputBox u)
                element.SubscribeToUserAction(act, () => OnUserActionInputBox(u, act));
            else if (element is List l)
                element.SubscribeToUserAction(act, () => OnUserActionList(l, act));
            else if (element is Stepper n)
                element.SubscribeToUserAction(act, () => OnUserActionStepper(n, act));
            else if (element is Pages g)
                element.SubscribeToUserAction(act, () => OnUserActionPages(g, act));
            else if (element is Palette t)
                element.SubscribeToUserAction(act, () => OnUserActionPalette(t, act));
            else if (element is Panel p)
                element.SubscribeToUserAction(act, () => OnUserActionPanel(p, act));
            else if (element is Scroll r)
                element.SubscribeToUserAction(act, () => OnUserActionScroll(r, act));
            else if (element is Slider s)
                element.SubscribeToUserAction(act, () => OnUserActionSlider(s, act));
        }
    }
    public void Remove(Element element)
    {
        element.UnsubscribeAll();
        elements.Remove(element);
    }
    public void BringToTop(Element element)
    {
        Remove(element);
        Add(element);
    }

    public int IndexOf(Element? element) => element == null ? -1 : elements.IndexOf(element);
    public bool IsContaining(Element? element) => element != null && elements.Contains(element);

    public void Update()
    {
        foreach (var element in elements)
        {
            element.Update();

            if (element is Button b) OnUpdateButton(b);
            else if (element is InputBox u) OnUpdateInputBox(u);
            else if (element is List l) OnUpdateList(l);
            else if (element is Pages g) OnUpdatePages(g);
            else if (element is Palette t) OnUpdatePalette(t);
            else if (element is Panel p) OnUpdatePanel(p);
            else if (element is Stepper n) OnUpdateStepper(n);
            else if (element is Scroll r) OnUpdateScroll(r);
            else if (element is Slider s) OnUpdateSlider(s);
            else if (element is Layout o) OnUpdateLayout(o);

            if (element is Button button)
                button.dragCallback = d => OnDragButton(button, d);
            else if (element is InputBox input)
                input.dragCallback = d => OnDragInputBox(input, d);
            if (element is List list)
            {
                list.dragCallback = d => OnDragList(list, d);
                list.itemUpdateCallback = b => OnUpdateListItem(list, b);
                list.itemTriggerCallback = b => OnListItemTrigger(list, b);
            }
            else if (element is Pages pages)
            {
                pages.dragCallback = d => OnDragPages(pages, d);
                pages.pageUpdateCallback = b => OnUpdatePagesPage(pages, b);
            }
            else if (element is Palette palette)
            {
                palette.dragCallback = d => OnDragPalette(palette, d);
                palette.pageUpdateCallback = b => OnUpdatePalettePage(palette, b);
                palette.sampleUpdateCallback =
                    (b, c) => OnUpdatePaletteSample(palette, b, c);
                palette.pickCallback = p => OnPalettePick(palette, p);
            }
            else if (element is Panel panel)
            {
                panel.dragCallback = d => OnDragPanel(panel, d);
                panel.resizeCallback = d => OnPanelResize(panel, d);
            }
            else if (element is Stepper stepper)
                stepper.dragCallback = d => OnDragStepper(stepper, d);
            else if (element is Scroll scroll)
                scroll.dragCallback = d => OnDragScroll(scroll, d);
            else if (element is Slider slider)
                slider.dragCallback = d => OnDragSlider(slider, d);
            else if (element is Layout layout)
                layout.segmentUpdateCallback = (seg, i) => OnUpdateLayoutSegment(layout, seg, i);
        }
    }

    public byte[] ToBytes()
    {
        var result = new List<byte>();
        result.AddRange(BitConverter.GetBytes(elements.Count));

        foreach (var element in elements)
        {
            var bytes = element.ToBytes();
            result.AddRange(BitConverter.GetBytes(bytes.Length));
            result.AddRange(bytes);
        }

        return result.ToArray();
    }

    protected virtual void OnUserActionButton(Button button, UserAction userAction) { }
    protected virtual void OnUserActionInputBox(InputBox inputBox, UserAction userAction) { }
    protected virtual void OnUserActionList(List list, UserAction userAction) { }
    protected virtual void OnUserActionStepper(Stepper stepper,
        UserAction userAction) { }
    protected virtual void OnUserActionPages(Pages pages, UserAction userAction) { }
    protected virtual void OnUserActionPalette(Palette palette, UserAction userAction) { }
    protected virtual void OnUserActionPanel(Panel panel, UserAction userAction) { }
    protected virtual void OnUserActionScroll(Scroll scroll, UserAction userAction) { }
    protected virtual void OnUserActionSlider(Slider slider, UserAction userAction) { }
    protected virtual void OnUserActionLayout(Layout layout, UserAction userAction) { }

    protected virtual void OnDragButton(Button button, (int width, int height) delta) { }
    protected virtual void OnDragInputBox(InputBox inputBox, (int width, int height) delta) { }
    protected virtual void OnDragList(List list, (int width, int height) delta) { }
    protected virtual void OnDragStepper(Stepper stepper,
        (int width, int height) delta) { }
    protected virtual void OnDragPages(Pages pages, (int width, int height) delta) { }
    protected virtual void OnDragPalette(Palette palette, (int width, int height) delta) { }
    protected virtual void OnDragPanel(Panel panel, (int width, int height) delta) { }
    protected virtual void OnDragScroll(Scroll scroll, (int width, int height) delta) { }
    protected virtual void OnDragSlider(Slider slider, (int width, int height) delta) { }
    protected virtual void OnDragLayout(Layout layout, (int width, int height) delta) { }

    protected virtual void OnUpdateButton(Button button) { }
    protected virtual void OnUpdateInputBox(InputBox inputBox) { }
    protected virtual void OnUpdateList(List list) { }
    protected virtual void OnUpdateListItem(List list, Button item) { }
    protected virtual void OnUpdateStepper(Stepper stepper) { }
    protected virtual void OnUpdatePages(Pages pages) { }
    protected virtual void OnUpdatePagesPage(Pages pages, Button page) { }
    protected virtual void OnUpdatePalette(Palette palette) { }
    protected virtual void OnUpdatePalettePage(Palette palette, Button page) { }
    protected virtual void
        OnUpdatePaletteSample(Palette palette, Button sample, uint color) { }
    protected virtual void OnUpdatePanel(Panel panel) { }
    protected virtual void OnUpdateScroll(Scroll scroll) { }
    protected virtual void OnUpdateSlider(Slider slider) { }
    protected virtual void OnUpdateLayout(Layout layout) { }
    protected virtual void OnUpdateLayoutSegment(Layout layout,
        (int x, int y, int width, int height) segment, int index) { }

    protected virtual uint OnPalettePick(Palette palette, (float x, float y) position) => default;
    protected virtual void OnListItemTrigger(List list, Button item) { }
    protected virtual void OnPanelResize(Panel panel, (int width, int height) delta) { }

#region Backend
    private readonly List<Element> elements = new();

    private static byte[] GetBytes(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}