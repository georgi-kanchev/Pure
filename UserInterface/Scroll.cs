using System.Diagnostics.CodeAnalysis;

namespace Pure.UserInterface;

public class Scroll : Element
{
	/// <summary>
	/// Gets the slider part of the scroll.
	/// </summary>
	public Slider Slider { get; private set; }
	/// <summary>
	/// Gets the button used to scroll up.
	/// </summary>
	public Button Up { get; private set; }
	/// <summary>
	/// Gets the button used to scroll down.
	/// </summary>
	public Button Down { get; private set; }

	public bool IsVertical
	{
		get => isVertical;
		set { if (hasParent == false) isVertical = value; }
	}

	public Scroll((int x, int y) position, int size = 5, bool isVertical = true) : base(position)
	{
		IsVertical = isVertical;
		Size = IsVertical ? (1, size) : (size, 1);

		Init();
	}
	public Scroll(byte[] bytes) : base(bytes)
	{
		IsVertical = GrabBool(bytes);
		Size = IsVertical ? (1, Size.height) : (Size.width, 1);

		Init();
		Slider.progress = GrabFloat(bytes);
		Slider.index = GrabInt(bytes);
	}

	public override byte[] ToBytes()
	{
		var result = base.ToBytes().ToList();
		PutBool(result, IsVertical);
		PutFloat(result, Slider.Progress);
		PutInt(result, Slider.index);
		return result.ToArray();
	}

	protected override void OnUpdate()
	{
		if (IsDisabled)
			return;

		Size = (IsVertical ? 1 : Size.width, IsVertical ? Size.height : 1);
		Slider.isVertical = IsVertical;

		var (x, y) = Position;
		var (w, h) = Size;

		if (IsVertical)
		{
			Up.position = (x, y);
			Down.position = (x, y + h - 1);
			Slider.position = (x, y + 1);
			Slider.size = (w, h - 2);
		}
		else
		{
			Up.position = (x + w - 1, y);
			Down.position = (x, y);
			Slider.position = (x + 1, y);
			Slider.size = (w - 2, h);
		}

		Slider.Update();
		Up.Update();
		Down.Update();
	}

	#region Backend
	internal bool isVertical;

	[MemberNotNull(nameof(Slider))]
	[MemberNotNull(nameof(Up))]
	[MemberNotNull(nameof(Down))]
	private void Init()
	{
		Slider = new((0, 0), IsVertical ? Size.height : Size.width, IsVertical) { hasParent = true };
		Up = new((0, 0)) { Size = (1, 1), hasParent = true };
		Down = new((0, 0)) { Size = (1, 1), hasParent = true };
		var dir = IsVertical ? 1 : -1;

		Up.SubscribeToUserAction(UserAction.Trigger, () => Slider.Move(1 * dir));
		Up.SubscribeToUserAction(UserAction.PressAndHold, () => Slider.Move(1 * dir));
		Down.SubscribeToUserAction(UserAction.Trigger, () => Slider.Move(-1 * dir));
		Down.SubscribeToUserAction(UserAction.PressAndHold, () => Slider.Move(-1 * dir));
	}
	#endregion
}