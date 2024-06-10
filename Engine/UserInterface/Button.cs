namespace Pure.Engine.UserInterface;

using System.Diagnostics;

/// <summary>
/// Represents a user interface button.
/// </summary>
public class Button : Block
{
    /// <summary>
    /// Gets or sets a value indicating whether the button is selected.
    /// </summary>
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            if (hasParent == false)
                isSelected = value;
        }
    }
    public (int id, bool isHoldable) Hotkey { get; set; }

    /// <summary>
    /// Initializes a new button instance with the specified position and default size of (10, 1).
    /// </summary>
    /// <param name="position">The position of the button.</param>
    public Button((int x, int y) position = default) : base(position)
    {
        Init();
        Size = (10, 1);
        Hotkey = (-1, false);
    }
    public Button(byte[] bytes) : base(bytes)
    {
        Init();
        var b = Decompress(bytes);
        IsSelected = GrabBool(b);
        Hotkey = (GrabInt(b), GrabBool(b));
    }
    public Button(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public override string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public override byte[] ToBytes()
    {
        var result = Decompress(base.ToBytes()).ToList();
        PutBool(result, IsSelected);
        PutInt(result, Hotkey.id);
        PutBool(result, Hotkey.isHoldable);
        return Compress(result.ToArray());
    }

    public Button Duplicate()
    {
        return new(ToBytes());
    }

    public static implicit operator byte[](Button button)
    {
        return button.ToBytes();
    }
    public static implicit operator Button(byte[] bytes)
    {
        return new(bytes);
    }

#region Backend
    internal bool isSelected;
    private static readonly Stopwatch hold = new(), holdTrigger = new();

    private void Init()
    {
        hold.Start();
        holdTrigger.Start();

        OnInteraction(Interaction.Trigger, () =>
        {
            // not using property since the user click can access it despite of parent
            isSelected = isSelected == false;

            if (IsSelected)
                Interact(Interaction.Select);
        });
    }
    protected override void OnInput()
    {
        if (IsHovered)
            Input.CursorResult = MouseCursor.Hand;

        if (Hotkey.id == -1 || Input.PressedKeys?.Length != 1)
            return; // disallow multiple key presses when triggering hotkey, should be only hotkey

        if (Input.IsKeyJustPressed((Key)Hotkey.id))
        {
            Interact(Interaction.Trigger);
            hold.Restart();
        }

        if (Hotkey.isHoldable == false)
            return;

        var isJustHeld = false;
        if (hold.Elapsed.TotalSeconds > Input.HOLD_DELAY &&
            holdTrigger.Elapsed.TotalSeconds > Input.HOLD_INTERVAL)
        {
            holdTrigger.Restart();
            isJustHeld = true;
        }

        if (Input.IsKeyPressed((Key)Hotkey.id) && isJustHeld)
            Interact(Interaction.Trigger);
    }
#endregion
}