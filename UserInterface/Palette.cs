using System.Diagnostics.CodeAnalysis;

namespace Pure.UserInterface;

public class Palette : Element
{
    public Slider Opacity { get; private set; }
    public Pages Brightness { get; private set; }
    public Button Pick { get; private set; }

    public uint SelectedColor
    {
        get => selectedColor;
        set
        {
            var prev = selectedColor;
            selectedColor = value;

            if (prev != selectedColor)
                TriggerUserAction(UserAction.Select);
        }
    }

    public Palette((int x, int y) position, int brightnessLevels = 30) : base(position)
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
        PutInt(result, Brightness.CurrentPage);
        return result.ToArray();
    }

    protected override void OnUpdate()
    {
        sizeMinimum = (13, 3);
        sizeMaximum = (13, 3);

        if (IsDisabled)
            return;

        Opacity.Update();
        Brightness.Update();
        Pick.Update();

        if (isPicking)
            MouseCursorResult = MouseCursor.Crosshair;

        if (Input.Current.IsJustPressed && isPicking && IsHovered == false)
        {
            isPicking = false;

            // ui callback first, then child callback (child is with priority
            // - will overwrite the value if possible)
            var uiColor = pickCallback?.Invoke(Input.Current.Position);
            var childColor = OnPick(Input.Current.Position);
            SelectedColor = childColor == default && uiColor != null ? (uint)uiColor : childColor;
        }

        SelectedColor = ToOpacity(SelectedColor, Opacity.Progress);

        for (var i = 0; i < colorButtons.Count; i++)
        {
            var btn = colorButtons[i];

            if (Input.Current.IsJustReleased && btn is { IsPressedAndHeld: true, IsHovered: true })
                SelectedColor = GetColor(colorButtons.IndexOf(btn));

            OnSampleUpdate(btn, GetColor(i));
            sampleUpdateCallback?.Invoke(btn, GetColor(i));
            btn.Update();
        }

        UpdateParts();
    }
    protected virtual void OnSampleUpdate(Button sample, uint color) { }
    protected virtual void OnPageUpdate(Button page) { }
    protected virtual uint OnPick((float x, float y) position) => default;

    #region Backend
    private class ChildPagination : Pages
    {
        // custom class for receiving events
        private readonly Palette parent;

        public ChildPagination((int x, int y) position, int count, Palette parent) :
            base(position, count) =>
            this.parent = parent;
        protected override void OnPageUpdate(Button page)
        {
            parent.OnPageUpdate(page);
            parent.pageUpdateCallback?.Invoke(page);
        }
    }

    private readonly List<Button> colorButtons = new();
    private readonly uint[] palette = new uint[]
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

    // used in the UI class to receive callbacks
    internal Action<Button>? pageUpdateCallback;
    internal Action<Button, uint>? sampleUpdateCallback;
    internal Func<(float, float), uint>? pickCallback;

    [MemberNotNull(nameof(Opacity))]
    [MemberNotNull(nameof(Brightness))]
    [MemberNotNull(nameof(Pick))]
    private void Init(int brightnessPageCount, int brightnessCurrentPage, float opacityProgress)
    {
        var (x, y) = Position;
        var (w, h) = Size;
        brightnessPageCount = Math.Clamp(brightnessPageCount, 1, 99);

        Opacity = new((x, y), w) { Progress = opacityProgress, hasParent = true };
        Brightness = new ChildPagination((x, y + 2), brightnessPageCount, this)
        {
            size = (w - 1, 1),
            CurrentPage = brightnessCurrentPage,
            hasParent = true
        };
        Pick = new((x + w - 1, y + h - 1)) { hasParent = true };

        Pick.SubscribeToUserAction(UserAction.Trigger, () => isPicking = true);

        for (var i = 0; i < 13; i++)
            colorButtons.Add(new((x + i, y + 1)) { size = (1, 1), hasParent = true });
    }
    private void UpdateParts()
    {
        var (x, y) = Position;
        var (w, h) = Size;

        Opacity.position = (x, y);
        Opacity.size = (w, 1);
        Pick.position = (x + w - 1, y + h - 1);
        Pick.size = (1, 1);
        Brightness.position = (x, y + 2);
        Brightness.size = (w - 1, 1);

        for (var i = 0; i < colorButtons.Count; i++)
        {
            var btn = colorButtons[i];
            btn.position = (x + i, y + 1);
            btn.size = (1, 1);
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
        var value = Map(Brightness.CurrentPage, 1, Brightness.Count, 0, 1);
        color = ToBrightness(color, value);
        return color;
    }

    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
    #endregion
}