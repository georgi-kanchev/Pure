namespace Pure.UserInterface;

public class Prompt
{
    public Element? Element
    {
        get => currentElement;
        set
        {
            currentElement = value;
            UpdateElementPosition();
        }
    }

    public (int x, int y) Position
    {
        get;
        private set;
    }
    public (int width, int height) Size
    {
        get;
        private set;
    }
    public string? Message
    {
        get;
        set;
    }
    public bool IsOpened
    {
        get;
        private set;
    }

    public void Open(int buttonCount = 2, Action<int>? onButtonTrigger = null)
    {
        if (IsOpened)
            return;

        IsOpened = true;
        UpdateElementPosition();

        buttons.Clear();
        for (var i = 0; i < buttonCount; i++)
        {
            var btn = new Button((0, 0))
            {
                hasParent = true,
                size = (1, 1),
            };
            var index = i;
            btn.OnInteraction(Interaction.Trigger, () => onButtonTrigger?.Invoke(index));
            buttons.Add(btn);
        }
    }
    public void Close()
    {
        if (IsOpened == false)
            return;

        IsOpened = false;
        buttons.Clear();
    }

    public bool IsOwning(Element? element)
    {
        return element != null && (buttons.Contains(element) || panel == element);
    }
    public int IndexOf(Button? button)
    {
        return button == null ? -1 : buttons.IndexOf(button);
    }

    public void OnDisplay(Action<Button[]> method)
    {
        display += method;
    }

#region Backend
    private readonly List<Button> buttons = new();
    private readonly Panel panel = new((0, 0))
    {
        IsResizable = false,
        IsMovable = false,
        IsRestricted = false,
        isTextReadonly = true,
        hasParent = true,
    };
    private Element? currentElement;

    private Action<Button[]>? display;

    internal void Update(UserInterface ui)
    {
        if (IsOpened == false)
            return;

        var sz = Element.TilemapSize;
        var lines = Message?.Split(Environment.NewLine).Length ?? 0;
        var (w, h) = (sz.width / 2, lines - 2);
        var (x, y) = (sz.width / 4, sz.height / 2);

        display?.Invoke(buttons.ToArray());

        panel.isDisabled = IsOpened == false;
        panel.position = IsOpened ? (0, 0) : (int.MaxValue, int.MaxValue);
        panel.size = sz;
        ui.UpdateElement(panel);

        if (Element != null)
        {
            ui.UpdateElement(Element);
            w = Element.Size.width;
            h = Element.Size.height;
            x = Element.Position.x;
            y = Element.Position.y;
        }

        Position = IsOpened ? (x, y - lines) : (int.MaxValue, int.MaxValue);
        Size = (w, lines + h + 1);

        var btnXs = Distribute(buttons.Count, (x, x + w));
        for (var i = 0; i < buttons.Count; i++)
        {
            var btn = buttons[i];
            btn.position = ((int)btnXs[i], y + h);
            ui.UpdateElement(btn);
        }
    }
    private void UpdateElementPosition()
    {
        if (currentElement == null)
            return;

        var text = Message ?? "";
        var lines = text.Split(Environment.NewLine).Length;
        currentElement.Align((0.5f, 0.5f));
        var (x, y) = currentElement.Position;
        currentElement.position = (x, y + lines / 2);
    }

    private static float[] Distribute(int amount, (float a, float b) range)
    {
        if (amount <= 0)
            return Array.Empty<float>();

        var result = new float[amount];
        var size = range.b - range.a;
        var spacing = size / (amount + 1);

        for (var i = 1; i <= amount; i++)
            result[i - 1] = range.a + i * spacing;

        return result;
    }
#endregion
}