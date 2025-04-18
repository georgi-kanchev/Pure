namespace Pure.Engine.UserInterface;

[DoNotSave]
public class Prompt : Block
{
    public Action<Button>? OnItemDisplay { get; set; }

    public int LastChoiceIndex { get; private set; }

    public Prompt()
    {
        hasParent = true;
        IsHidden = true;
    }

    public void Open(Block? block = null, bool autoClose = true, int btnCount = 2, int btnYes = default, int btnNo = 1, Action<int>? onButtonTrigger = null)
    {
        currentBlock = block;
        UpdateBlockPosition();

        btnCount = Math.Max(btnCount, 1);
        IsHidden = false;

        buttons.Clear();
        for (var i = 0; i < btnCount; i++)
        {
            var btn = new Button((-1, -1))
            {
                hasParent = true,
                size = (1, 1)
            };
            var index = i;
            btn.OnInteraction(Interaction.Trigger, () =>
            {
                LastChoiceIndex = index;

                if (autoClose)
                    Close();

                onButtonTrigger?.Invoke(index);
                Interact(Interaction.Select);
            });
            if (i == btnYes)
                btn.Hotkey = ((int)Key.Enter, false);
            if (i == btnNo)
                btn.Hotkey = ((int)Key.Escape, false);

            buttons.Add(btn);
        }
    }
    public void Close()
    {
        currentBlock = null;
        IsHidden = true;
    }
    public void TriggerButton(int index)
    {
        if (index < 0 || index >= buttons.Count || IsDisabled)
            return;

        buttons[index].Interact(Interaction.Trigger);
    }

    public int IndexOf(Button? button)
    {
        return button == null ? -1 : buttons.IndexOf(button);
    }

#region Backend
    private readonly List<Button> buttons = [];
    private Block? currentBlock;
    private readonly Panel panel = new((0, 0))
    {
        IsResizable = false,
        IsMovable = false,
        IsRestricted = false,
        isTextReadonly = true,
        hasParent = true
    };

    private void UpdateBlockPosition()
    {
        if (currentBlock == null)
            return;

        var lines = Text.Replace("\r", "").Split("\n").Length;

        currentBlock.AlignInside((0.5f, 0.5f));
        var (x, y) = currentBlock.Position;
        currentBlock.position = (x, y + lines / 2);
    }

    internal override void OnChildrenDisplay()
    {
        if (IsHidden)
            return;

        foreach (var btn in buttons)
            OnItemDisplay?.Invoke(btn);
    }
    internal override void OnChildrenUpdate()
    {
        if (IsHidden)
            return;

        var sz = Input.TileMapSize;
        var lines = Text.Replace("\r", "").Split("\n");
        var (w, h) = (sz.width / 2, 0);
        var (x, y) = (sz.width / 4, sz.height / 2 + lines.Length / 2);

        panel.size = sz;
        panel.Update();

        if (currentBlock != null)
        {
            currentBlock.Update();

            // nasty hacks ahead, proceed with caution
            var isHoveringBtns = false;
            foreach (var btn in buttons)
                if (btn.IsHovered)
                {
                    isHoveringBtns = true;
                    break;
                }

            // this ^^^ and this VVV helps to not lose hotkey functionality on the buttons for some reason (?)
            if (isHoveringBtns == false)
                currentBlock.IsFocused = true;

            // update might call Close which invalidates the previous check
            if (currentBlock != null)
            {
                w = currentBlock.Size.width;
                h = currentBlock.Size.height;
                x = currentBlock.Position.x;
                y = currentBlock.Position.y;
            }
        }
        else
        {
            var longestLine = 0;
            foreach (var line in lines)
                if (line.Length > longestLine)
                    longestLine = line.Length;

            w = longestLine;
            x = sz.width / 2 - w / 2;
        }

        position = (x - 1, y - lines.Length - 1);
        size = (w + 2, lines.Length + h + 3);

        var btnXs = Distribute(buttons.Count, (position.Item1, position.Item1 + size.Item1));
        for (var i = 0; i < buttons.Count; i++)
        {
            var btn = buttons[i];
            btn.position = ((int)btnXs[i], y + h);
            btn.Update();
        }
    }

    private static float[] Distribute(int amount, Range range)
    {
        if (amount <= 0)
            return [];

        var result = new float[amount];
        var size = range.b - range.a;
        var spacing = size / (amount + 1);

        for (var i = 1; i <= amount; i++)
            result[i - 1] = range.a + i * spacing;

        return result;
    }
#endregion
}