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
        var count = BitConverter.ToInt32(GetBytes(bytes, 4, ref offset));

        for (var i = 0; i < count; i++)
        {
            var byteCount = BitConverter.ToInt32(GetBytes(bytes, 4, ref offset));

            // elementType string gets saved first for each element
            var typeStrBytesLength = BitConverter.ToInt32(GetBytes(bytes, 4, ref offset));
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
    }

    public int Add(Element element)
    {
        var e = element;
        elements.Add(e);

        var userActionCount = Enum.GetNames(typeof(UserAction)).Length;

        for (var i = 0; i < userActionCount; i++)
        {
            var act = (UserAction)i;
            if (e is Button b) e.SubscribeToUserAction(act, () => OnUserActionButton(b, act));
            else if (e is InputBox u)
                e.SubscribeToUserAction(act, () => OnUserActionInputBox(u, act));
            else if (e is List l) e.SubscribeToUserAction(act, () => OnUserActionList(l, act));
            else if (e is Stepper n)
                e.SubscribeToUserAction(act, () => OnUserActionStepper(n, act));
            else if (e is Pages g) e.SubscribeToUserAction(act, () => OnUserActionPages(g, act));
            else if (e is Palette t) e.SubscribeToUserAction(act, () => OnUserActionPalette(t, act));
            else if (e is Panel p) e.SubscribeToUserAction(act, () => OnUserActionPanel(p, act));
            else if (e is Scroll r) e.SubscribeToUserAction(act, () => OnUserActionScroll(r, act));
            else if (e is Slider s) e.SubscribeToUserAction(act, () => OnUserActionSlider(s, act));
        }

        return elements.Count - 1;
    }
    public void BringToTop(Element element)
    {
        Remove(element);
        Add(element);
    }
    public int IndexOf(Element element) => elements.IndexOf(element);
    public bool IsContaining(Element element) => elements.Contains(element);
    public void Remove(Element element)
    {
        element.UnsubscribeAll();
        var contains = elements.Contains(element);
        elements.Remove(element);
    }

    public void Update()
    {
        foreach (var element in elements)
        {
            if (element is List list)
            {
                list.itemUpdateCallback = (b) => OnUpdateListItem(list, b);
                list.itemTriggerCallback = (b) => OnListItemTrigger(list, b);
            }
            else if (element is Pages pages)
                pages.pageUpdateCallback = (b) => OnUpdatePagesPage(pages, b);
            else if (element is Palette palette)
            {
                palette.pageUpdateCallback = (b) => OnUpdatePalettePage(palette, b);
                palette.sampleUpdateCallback =
                    (b, c) => OnUpdatePaletteSample(palette, b, c);
                palette.pickCallback = (p) => OnPalettePick(palette, p);
            }

            element.Update();

            if (element is Button b) OnUpdateButton(b);
            else if (element is InputBox u) OnUpdateInputBox(u);
            else if (element is List l) OnUpdateList(l);
            else if (element is Stepper n) OnUpdateStepper(n);
            else if (element is Pages g) OnUpdatePages(g);
            else if (element is Palette t) OnUpdatePalette(t);
            else if (element is Panel p) OnUpdatePanel(p);
            else if (element is Scroll r) OnUpdateScroll(r);
            else if (element is Slider s) OnUpdateSlider(s);
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

    protected virtual uint OnPalettePick(Palette palette, (float x, float y) position) => default;
    protected virtual void OnListItemTrigger(List list, Button item) { }

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