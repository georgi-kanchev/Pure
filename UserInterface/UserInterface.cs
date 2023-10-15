using System.Text;

namespace Pure.UserInterface;

public class UserInterface
{
    public int Count
    {
        get => elements.Count;
    }
    public Prompt? Prompt
    {
        get;
        set;
    }

    public Element this[int index]
    {
        get => elements[index];
    }

    public UserInterface()
    {
    }
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
            else if (typeStr == nameof(FileViewer)) Add(new FileViewer(bElement));
            else if (typeStr == nameof(List)) Add(new List(bElement));
            else if (typeStr == nameof(Stepper)) Add(new Stepper(bElement));
            else if (typeStr == nameof(Pages)) Add(new Pages(bElement));
            else if (typeStr == nameof(Palette)) Add(new Palette(bElement));
            else if (typeStr == nameof(Panel)) Add(new Panel(bElement));
            else if (typeStr == nameof(Scroll)) Add(new Scroll(bElement));
            else if (typeStr == nameof(Slider)) Add(new Slider(bElement));
            else if (typeStr == nameof(Layout)) Add(new Layout(bElement));
        }

        return;

        int GetInt()
        {
            return BitConverter.ToInt32(GetBytes(bytes, 4, ref offset));
        }
    }

    public void Add(params Element[]? elements)
    {
        if (elements == null || elements.Length == 0)
            return;

        foreach (var element in elements)
            this.elements.Add(element);
    }
    public void Remove(params Element[]? elements)
    {
        if (elements == null || elements.Length == 0)
            return;

        foreach (var element in elements)
            this.elements.Remove(element);
    }
    public void BringToTop(params Element[]? elements)
    {
        if (elements == null || elements.Length == 0)
            return;

        foreach (var element in elements)
        {
            Remove(element);
            Add(element);
        }
    }

    public int IndexOf(Element? element)
    {
        return element == null ? -1 : elements.IndexOf(element);
    }
    public bool IsContaining(Element? element)
    {
        return element != null && elements.Contains(element);
    }

    public void Update()
    {
        foreach (var e in elements)
            e.Update();

        Prompt?.Update(this);
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