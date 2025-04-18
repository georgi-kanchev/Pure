namespace Pure.Engine.UserInterface;

public class Palette : Block
{
    [DoNotSave]
    public Func<PointF, uint>? OnPick { get; set; }

    [DoNotSave]
    public Slider Opacity { get; }
    [DoNotSave]
    public Slider Brightness { get; }
    [DoNotSave]
    public Slider Samples { get; }
    [DoNotSave]
    public Button Pick { get; }

    public uint SelectedColor
    {
        get => selectedColor;
        set
        {
            var prev = selectedColor;
            selectedColor = value;

            if (prev != selectedColor)
                Interact(Interaction.Select);
        }
    }

    public Palette() : this((0, 0))
    {
    }
    public Palette(PointI position) : base(position)
    {
        Size = (13, 3);

        OnUpdate += OnRefresh;

        Opacity = new(Position) { Progress = 1f, hasParent = true, wasMaskSet = true };
        Brightness = new((X, Y + 2))
        {
            Progress = 0.5f, size = (Width - 1, 1), hasParent = true, wasMaskSet = true
        };
        Pick = new((X + Width - 1, Y + Height - 1)) { hasParent = true, wasMaskSet = true };

        Pick.OnInteraction(Interaction.Trigger, () => isPicking = true);

        Samples = new((X, Y + 1)) { hasParent = true, wasMaskSet = true };
        Samples.Handle.OnInteraction(Interaction.Trigger, () =>
        {
            SelectedColor = GetSample((int)(Samples.Progress * Width));
        });
        Samples.OnInteraction(Interaction.Select, () =>
        {
            SelectedColor = GetSample((int)(Samples.Progress * Width));
        });
    }

    public uint GetSample(int index)
    {
        index = index < 0 ? 0 : index;
        index = index >= palette.Length ? palette.Length - 1 : index;

        // var color = ToOpacity(palette[index], Opacity.Progress);
        var color = ToBrightness(palette[index], Brightness.Progress);
        return color;
    }

#region Backend
    private static readonly uint[] palette =
    [
        0x_7F_7F_7F_FF, // Gray
        0x_FF_00_00_FF, // Red
        0x_FF_7F_00_FF, // Orange
        0x_FF_FF_00_FF, // Yellow
        0x_7F_FF_00_FF, // Green Yellow
        0x_00_FF_00_FF, // Green
        0x_00_FF_7F_FF, // Green Cyan
        0x_00_FF_FF_FF, // Cyan
        0x_00_7F_FF_FF, // Blue Cyan
        0x_00_00_FF_FF, // Blue
        0x_7F_00_FF_FF, // Blue Magenta
        0x_FF_00_FF_FF, // Magenta
        0x_FF_00_7F_FF // Red Magenta
    ];
    [DoNotSave]
    private bool isPicking;
    private uint selectedColor = uint.MaxValue;

    internal void OnRefresh()
    {
        sizeMin = (13, 3);
        sizeMax = sizeMin;
        LimitSizeMin(sizeMin);
        LimitSizeMax(sizeMin);
    }
    protected override void OnInput()
    {
        if (isPicking)
            Input.CursorResult = MouseCursor.Crosshair;

        if (Input.IsButtonJustPressed() && isPicking && IsHovered == false)
        {
            isPicking = false;
            SelectedColor = OnPick?.Invoke(Input.Position) ?? default;
        }

        SelectedColor = ToOpacity(SelectedColor, Opacity.Progress);
    }
    internal override void OnChildrenUpdate()
    {
        var (x, y) = Position;
        var (w, h) = Size;

        if (IsDisabled)
        {
            Opacity.IsDisabled = true;
            Pick.IsDisabled = true;
            Brightness.IsDisabled = true;
            Samples.IsDisabled = true;
        }

        Opacity.position = (x, y);
        Opacity.size = (w - (Pick.IsHidden ? 0 : 1), h - 2);
        Opacity.mask = mask;
        Opacity.Update();

        Pick.position = (x + w - 1, y);
        Pick.size = (1, 1);
        Pick.mask = Pick.IsHidden ? Input.Mask : mask;
        Pick.Update();

        Brightness.position = (x, y + h - 1);
        Brightness.size = (w, 1);
        Brightness.mask = mask;
        Brightness.Update();

        Samples.position = (x, y + 1);
        Samples.size = (w, 1);
        Samples.mask = mask;
        Samples.Update();
    }

    private static uint ToOpacity(uint color, float unit)
    {
        var (r, g, b, _) = GetColor(color);
        var a = (byte)Map(unit, 0, 1, 0, 255);
        return GetColor(r, g, b, a);
    }
    private static uint ToBrightness(uint color, float unit)
    {
        var (r, g, b, a) = GetColor(color);

        if (unit < 0.5f)
        {
            r = (byte)Map(unit, 0, 0.5f, 0, r);
            g = (byte)Map(unit, 0, 0.5f, 0, g);
            b = (byte)Map(unit, 0, 0.5f, 0, b);
        }
        else
        {
            r = (byte)Map(unit, 0.5f, 1, r, 255);
            g = (byte)Map(unit, 0.5f, 1, g, 255);
            b = (byte)Map(unit, 0.5f, 1, b, 255);
        }

        return GetColor(r, g, b, a);
    }
    private static (byte r, byte g, byte b, byte a) GetColor(uint color)
    {
        var r = (byte)((color >> 24) & 255);
        var g = (byte)((color >> 16) & 255);
        var b = (byte)((color >> 8) & 255);
        var a = (byte)((color >> 0) & 255);
        return (r, g, b, a);
    }
    private static uint GetColor(byte r, byte g, byte b, byte a = 255)
    {
        return (uint)((r << 24) + (g << 16) + (b << 8) + a);
    }

    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
#endregion
}