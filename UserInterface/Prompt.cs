namespace Pure.UserInterface;

public class Prompt
{
    public Element? Element
    {
        get => currentElement;
        set
        {
            currentElement = value;

            if (value == null)
                return;

            var text = Message ?? "";
            var lines = text.Split(Environment.NewLine).Length;
            var sz = Element.TilemapSize;
            var (x, y) = (sz.width / 2, sz.height / 2);
            var (w, h) = value.Size;
            value.position = (x - w / 2, y - h / 2 + lines);
        }
    }

    public (int x, int y) Position { get; private set; }
    public (int width, int height) Size { get; private set; }
    public string? Message { get; set; }
    public bool IsOpened { get; private set; }

    public void Open(int buttonCount = 2, Action<int>? onButtonTrigger = null)
    {
        if (IsOpened)
            return;

        var text = Message ?? "";

        IsOpened = true;

        promptLabel.Placeholder = text;

        promptButtons.Clear();
        for (var i = 0; i < buttonCount; i++)
        {
            var btn = new Button((0, 0))
            {
                hasParent = true,
                size = (1, 1),
            };
            var index = i;
            btn.OnUserAction(UserAction.Trigger, () => onButtonTrigger?.Invoke(index));
            promptButtons.Add(btn);
        }
    }
    public void Close()
    {
        if (IsOpened == false)
            return;

        IsOpened = false;

        if (Element == null)
            return;

        Element.hasParent = false;
        Element = null;
        promptButtons.Clear();
    }

    public bool IsOwning(Element? element)
    {
        return element != null && (promptButtons.Contains(element) ||
                                   promptPanel == element || promptLabel == element);
    }
    public int IndexOf(Button? button) => button == null ? -1 : promptButtons.IndexOf(button);

#region Backend
    private readonly List<Button> promptButtons = new();
    private readonly InputBox promptLabel = new((0, 0))
    {
        IsEditable = false,
        IsDisabled = true,
        Value = "",
        hasParent = true,
    };
    private readonly Panel promptPanel = new((0, 0))
    {
        IsResizable = false,
        IsMovable = false,
        IsRestricted = false,
        isTextReadonly = true,
        hasParent = true,
    };
    private Element? currentElement;

    internal void Update(UserInterface ui)
    {
        if (IsOpened == false)
            return;

        var sz = Element.TilemapSize;
        var (w, h) = (sz.width / 2, sz.height / 2);
        var (x, y) = (sz.width / 4, sz.height / 4 + sz.height / 2);
        var lines = promptLabel.Placeholder.Split(Environment.NewLine).Length;

        promptPanel.isDisabled = IsOpened == false;
        promptPanel.position = IsOpened ? (0, 0) : (int.MaxValue, int.MaxValue);
        promptPanel.size = sz;
        ui.UpdateElement(promptPanel);

        if (Element != null)
        {
            ui.UpdateElement(Element);
            w = Element.Size.width;
            h = Element.Size.height;
            x = Element.Position.x;
            y = Element.Position.y;
        }

        promptLabel.isDisabled = promptPanel.isDisabled;
        promptLabel.position = IsOpened ? (x, y - lines) : (int.MaxValue, int.MaxValue);
        promptLabel.size = (w, lines);
        ui.UpdateElement(promptLabel);

        Position = promptLabel.position;
        Size = (w, promptLabel.Size.height + h + 1);

        var btnXs = Distribute(promptButtons.Count, (x, x + w));
        for (var i = 0; i < promptButtons.Count; i++)
        {
            var btn = promptButtons[i];
            btn.position = ((int)btnXs[i], y + h);
            ui.UpdateElement(btn);
        }
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