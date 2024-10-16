namespace Pure.Engine.UserInterface;

using System.Diagnostics.CodeAnalysis;

public class Palette : Block
{
    public Slider Opacity { get; private set; }
    public Slider Brightness { get; private set; }
    public Slider Samples { get; private set; }
    public Button Pick { get; private set; }

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

    public Palette((int x, int y) position = default) : base(position)
    {
        Size = (13, 3);

        Init(0.5f, 1f);
    }
    public Palette(byte[] bytes) : base(bytes)
    {
        var b = Decompress(bytes);
        SelectedColor = GrabUInt(b);
        var opProgress = GrabFloat(b);
        var opIndex = GrabInt(b);
        var brProgress = GrabFloat(b);
        var brIndex = GrabInt(b);
        Init(brProgress, opProgress);
        Opacity.progress = opProgress;
        Opacity.index = opIndex;
        Brightness.progress = brProgress;
        Brightness.index = brIndex;
    }
    public Palette(string base64) : this(Convert.FromBase64String(base64))
    {
    }

    public override string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public override byte[] ToBytes()
    {
        var result = Decompress(base.ToBytes()).ToList();
        Put(result, SelectedColor);
        Put(result, Opacity.Progress);
        Put(result, Opacity.index);
        Put(result, Brightness.Progress);
        Put(result, Brightness.index);
        return Compress(result.ToArray());
    }

    public uint GetSample(int index)
    {
        index = index < 0 ? 0 : index;
        index = index >= palette.Length ? palette.Length - 1 : index;

        // var color = ToOpacity(palette[index], Opacity.Progress);
        var color = ToBrightness(palette[index], Brightness.Progress);
        return color;
    }

    public void OnPick(Func<(float x, float y), uint> method)
    {
        pick = method;
    }

    protected override void OnInput()
    {
        if (isPicking)
            Input.CursorResult = MouseCursor.Crosshair;

        if (Input.IsButtonJustPressed() && isPicking && IsHovered == false)
        {
            isPicking = false;
            SelectedColor = pick?.Invoke(Input.Position) ?? default;
        }

        SelectedColor = ToOpacity(SelectedColor, Opacity.Progress);
    }

    public Palette Duplicate()
    {
        return new(ToBytes());
    }

    public static implicit operator byte[](Palette palette)
    {
        return palette.ToBytes();
    }
    public static implicit operator Palette(byte[] bytes)
    {
        return new(bytes);
    }

#region Backend
    private readonly uint[] palette =
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
    private bool isPicking;
    private uint selectedColor = uint.MaxValue;

    internal Func<(float x, float y), uint>? pick;

    [MemberNotNull(nameof(Opacity), nameof(Brightness), nameof(Pick), nameof(Samples))]
    private void Init(float brightnessProgress, float opacityProgress)
    {
        OnUpdate(OnUpdate);

        Opacity = new(Position) { Progress = opacityProgress, hasParent = true, wasMaskSet = true };
        Brightness = new((X, Y + 2))
        {
            Progress = brightnessProgress, size = (Width - 1, 1), hasParent = true, wasMaskSet = true
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

    internal void OnUpdate()
    {
        sizeMinimum = (13, 3);
        sizeMaximum = sizeMinimum;
        LimitSizeMin(sizeMinimum);
        LimitSizeMax(sizeMinimum);
    }
    internal override void OnChildrenUpdate()
    {
        var (x, y) = Position;
        var (w, h) = Size;

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
    private static float[] Distribute(int amount, (float a, float b) range)
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