namespace Pure.UserInterface;

public class Palette : Element
{
	public Slider Opacity { get; }
	public Pages Brightness { get; }
	public Button Pick { get; }

	public uint SelectedColor { get; set; } = uint.MaxValue;

	public Palette((int x, int y) position, int brightnessLevels) : base(position)
	{
		Size = (13, 3);
		var (x, y) = Position;
		var (w, h) = Size;

		Opacity = new((x, y), w) { Progress = 1f, hasParent = true };
		Brightness = new ChildPagination((x, y + 2), brightnessLevels, this)
		{
			Size = (w - 1, 1),
			CurrentPage = brightnessLevels / 2,
			hasParent = true
		};
		Pick = new((x + w - 1, y + h - 1)) { hasParent = true };

		Pick.SubscribeToUserEvent(UserEvent.Trigger, () => isPicking = true);

		for (int i = 0; i < 13; i++)
		{
			var btn = new Button((x + i, y + 1)) { Size = (1, 1), hasParent = true };
			colorButtons.Add(btn);
		}
	}

	protected override void OnUpdate()
	{
		Size = (13, 3);

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
			SelectedColor = OnPick(Input.Current.Position);
		}

		SelectedColor = ToOpacity(SelectedColor, Opacity.Progress);

		for (int i = 0; i < colorButtons.Count; i++)
		{
			if (colorButtons[i].IsPressed)
				SelectedColor = GetColor(colorButtons.IndexOf(colorButtons[i]));

			OnSampleUpdate(colorButtons[i], GetColor(i));
			colorButtons[i].Update();
		}
	}
	protected virtual void OnSampleUpdate(Button sample, uint color) { }
	protected virtual void OnPageUpdate(Button page) { }
	protected virtual uint OnPick((float x, float y) position) => default;

	#region Backend
	private class ChildPagination : Pages
	{
		// custom class for receiving events
		private Palette parent;

		public ChildPagination((int x, int y) position, int count, Palette parent) :
			base(position, count) => this.parent = parent;
		protected override void OnPageUpdate(Button page) => this.parent.OnPageUpdate(page);
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

	private static uint ToOpacity(uint color, float unit)
	{
		var (r, g, b, a) = GetColor(color);
		a = (byte)Map(unit, 0, 1, 0, 255);
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
		var value = Map((float)Brightness.CurrentPage, 1, (float)Brightness.Count, 0, 1);
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