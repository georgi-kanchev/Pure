using System.Diagnostics.CodeAnalysis;

namespace Pure.UserInterface;

public class Palette : Element
{
    public Slider Opacity
    {
        get;
        private set;
    }
    public Pages Brightness
    {
        get;
        private set;
    }
    public Button Pick
    {
        get;
        private set;
    }

    public uint SelectedColor
    {
        get => selectedColor;
        set
        {
            var prev = selectedColor;
            selectedColor = value;

            if (prev != selectedColor)
                SimulateInteraction(Interaction.Select);
        }
    }

    public Palette((int x, int y) position = default, int brightnessLevels = 30) : base(position)
    {
        Size = (13, 3);

        Init(brightnessLevels, brightnessLevels / 2, 1f);
    }
    public Palette(byte[] bytes) : base(bytes)
    {
        SelectedColor = GrabUInt(bytes);
        var opacity = GrabFloat(bytes);
        var count = GrabInt(bytes);
        var page = GrabInt(bytes);
        Init(count, page, opacity);
    }

    public override byte[] ToBytes()
    {
        var result = base.ToBytes().ToList();
        PutUInt(result, SelectedColor);
        PutFloat(result, Opacity.Progress);
        PutInt(result, Brightness.Count);
        PutInt(result, Brightness.Current);
        return result.ToArray();
    }

    public void OnColorSampleDisplay(Action<Button, uint> method)
    {
        displaySample += method;
    }
    public void OnColorPick(Func<(float x, float y), uint> method)
    {
        pick = method;
    }

    protected override void OnInput()
    {
        if (isPicking)
            Input.MouseCursorResult = MouseCursor.Crosshair;

        if (Input.IsJustPressed && isPicking && IsHovered == false)
        {
            isPicking = false;
            SelectedColor = pick?.Invoke(Input.Position) ?? default;
        }

        SelectedColor = ToOpacity(SelectedColor, Opacity.Progress);
    }

#region Backend
    private readonly List<Button> colorButtons = new();
    private readonly uint[] palette =
    {
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
        0x_FF_00_7F_FF, // Red Magenta
    };
    private bool isPicking;
    private uint selectedColor = uint.MaxValue;

    internal Action<Button, uint>? displaySample;
    internal Func<(float x, float y), uint>? pick;

    [MemberNotNull(nameof(Opacity), nameof(Brightness), nameof(Pick))]
    private void Init(int brightnessPageCount, int brightnessCurrentPage, float opacityProgress)
    {
        var (x, y) = Position;
        var (w, h) = Size;
        brightnessPageCount = Math.Clamp(brightnessPageCount, 1, 99);

        Opacity = new((x, y)) { Progress = opacityProgress, hasParent = true };
        Brightness = new((x, y + 2), brightnessPageCount)
        {
            size = (w - 1, 1),
            Current = brightnessCurrentPage,
            ItemWidth = $"{brightnessPageCount}".Length,
            hasParent = true,
        };
        Pick = new((x + w - 1, y + h - 1)) { hasParent = true };

        Pick.OnInteraction(Interaction.Trigger, () => isPicking = true);

        for (var i = 0; i < palette.Length; i++)
        {
            var btn = new Button((x + i, y + 1)) { size = (1, 1), hasParent = true };
            var index = i;
            btn.OnInteraction(Interaction.Trigger, () => SelectedColor = GetColor(index));
            colorButtons.Add(btn);
        }
    }

    internal override void OnUpdate()
    {
        sizeMinimum = (13, 3);
        LimitSizeMin((13, 3));
    }
    internal override void OnChildrenDisplay()
    {
        for (var i = 0; i < colorButtons.Count; i++)
            displaySample?.Invoke(colorButtons[i], GetColor(i));
    }
    internal override void OnChildrenUpdate()
    {
        var (x, y) = Position;
        var (w, h) = Size;

        Opacity.position = (x, y);
        Opacity.size = (w, h - 2);
        Pick.position = (x + w - 1, y + h - 1);
        Pick.size = (1, 1);
        Brightness.position = (x, y + h - 1);
        Brightness.size = (w - 1, 1);

        Opacity.InheritParent(this);
        Pick.InheritParent(this);
        Brightness.InheritParent(this);

        Opacity.Update();
        Pick.Update();
        Brightness.Update();

        var xs = Distribute(colorButtons.Count, (x, x + w));
        for (var i = 0; i < colorButtons.Count; i++)
        {
            var btn = colorButtons[i];
            btn.position = ((int)xs[i], y + h - 2);
            btn.size = (1, 1);
            btn.InheritParent(this);
            btn.Update();
        }
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
    private uint GetColor(int index)
    {
        var color = ToOpacity(palette[index], Opacity.Progress);
        var value = Map(Brightness.Current, 1, Brightness.Count, 0, 1);
        color = ToBrightness(color, value);
        return color;
    }

    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
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