﻿namespace Pure.Engine.UserInterface;

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
            var prev = isSelected;

            if (hasParent == false)
                isSelected = value;

            if (prev != isSelected)
                Interact(Interaction.Select);
        }
    }
    public (int id, bool holdable) Hotkey { get; set; }

    public Button() : this((0, 0))
    {
    }
    /// <summary>
    /// Initializes a new button instance with the specified position and default size of (10, 1).
    /// </summary>
    /// <param name="position">The position of the button.</param>
    public Button((int x, int y) position) : base(position)
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
        Put(result, IsSelected);
        Put(result, Hotkey.id);
        Put(result, Hotkey.holdable);
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
            var prev = isSelected;
            // not using property since the user click can access it despite of parent
            isSelected = isSelected == false;

            if (prev != isSelected)
                Interact(Interaction.Select);
        });
    }
    protected override void OnInput()
    {
        if (IsHovered)
            Input.CursorResult = MouseCursor.Hand;

        if (Hotkey.id == -1 ||
            Input.PressedKeys?.Length != 1 ||
            Input.IsTyping)
            return; // ensure single hotkey press & not typing

        if (Input.IsKeyJustPressed((Key)Hotkey.id))
        {
            Interact(Interaction.Trigger);
            hold.Restart();
        }

        if (Hotkey.holdable == false)
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