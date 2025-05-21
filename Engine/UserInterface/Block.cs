global using Area = (int x, int y, int width, int height);
global using PointI = (int x, int y);
global using PointF = (float x, float y);
global using Size = (int width, int height);
global using Range = (float a, float b);
using static Pure.Engine.UserInterface.Pivot;

namespace Pure.Engine.UserInterface;

public enum Pivot { TopLeft, Top, TopRight, Left, Center, Right, BottomLeft, Bottom, BottomRight }

/// <summary>
/// Represents a user interface block that the user can interact with and receive some
/// results back.
/// </summary>
public class Block
{
	public Action? OnDisplay { get; set; }
	public Action? OnUpdate { get; set; }
	public Action<(int deltaX, int deltaY)>? OnDrag { get; set; }

	public Area Area
	{
		get => (X, Y, Width, Height);
		set
		{
			Position = (value.x, value.y);
			Size = (value.width, value.height);
		}
	}
	public Area Mask
	{
		get => mask;
		set
		{
			if (hasParent)
				return;

			wasMaskSet = true;
			mask = value;
		}
	}

	public int X
	{
		get => Position.x;
		set => Position = (value, Position.y);
	}
	public int Y
	{
		get => Position.y;
		set => Position = (Position.x, value);
	}
	public int Width
	{
		get => Size.width;
		set => Size = (value, Size.height);
	}
	public int Height
	{
		get => Size.height;
		set => Size = (Size.width, value);
	}

	/// <summary>
	/// Gets or sets the position of the user interface block.
	/// </summary>
	public PointI Position
	{
		get => position;
		set => position = value;
	}
	/// <summary>
	/// Gets or sets the size of the user interface block.
	/// </summary>
	public Size Size
	{
		get => size;
		set => size = value;
	}
	/// <summary>
	/// Gets or sets the minimum size that this block can have.
	/// </summary>
	public Size SizeMinimum
	{
		get => sizeMin;
		set
		{
			if (hasParent)
				return;

			value.width = Math.Clamp(value.width, 1, SizeMaximum.width);
			value.height = Math.Clamp(value.height, 1, SizeMaximum.height);

			sizeMin = value;
			Size = size; // reclamp
		}
	}
	/// <summary>
	/// Gets or sets the maximum size that this block can have.
	/// </summary>
	public Size SizeMaximum
	{
		get => sizeMax;
		set
		{
			if (hasParent)
				return;

			value.width = Math.Max(value.width, 1);
			value.height = Math.Max(value.height, 1);

			value.width = value.width < SizeMinimum.width ? SizeMinimum.width : value.width;
			value.height = value.height < SizeMinimum.height ? SizeMinimum.height : value.height;

			sizeMax = value;
			Size = size; // reclamp
		}
	}
	/// <summary>
	/// Gets or sets the text displayed (if any) by the user interface block.
	/// </summary>
	public string Text
	{
		get => text ?? string.Empty;
		set
		{
			if (isTextReadonly == false)
				text = value ?? string.Empty;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the user interface block is hidden.
	/// </summary>
	public bool IsHidden { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether the user interface block is disabled.
	/// </summary>
	public bool IsDisabled { get; set; }
	/// <summary>
	/// Gets a value indicating whether the user interface block is currently focused by
	/// the user input.
	/// </summary>
	public bool IsFocused
	{
		get => Input.Focused == this;
		set => Input.Focused = value ? this : default;
	}
	/// <summary>
	/// Gets a value indicating whether the input position is currently hovering 
	/// over the user interface block.
	/// </summary>
	[DoNotSave]
	public bool IsHovered { get; internal set; }
	/// <summary>
	/// Gets a value indicating whether the user interface block is currently held by the input,
	/// regardless of being hovered or not.
	/// </summary>
	[DoNotSave]
	public bool IsPressedAndHeld { get; private set; }
	/// <summary>
	/// Gets a value indicating whether this block belongs to another user interface block.
	/// </summary>
	public bool IsChild
	{
		get => hasParent;
	}
	/// <summary>
	/// Gets a value indicating whether the user interface block is currently scrollable.
	/// </summary>
	internal bool IsScrollable
	{
		get => IsFocused && Input.FocusedPrevious == this && IsHovered;
	}

	/// <summary>
	/// Initializes a new user interface block instance class.
	/// </summary>
	public Block() : this((0, 0))
	{
	}
	/// <summary>
	/// Initializes a new user interface block instance class with the specified 
	/// position.
	/// </summary>
	/// <param name="position">The position of the user interface block.</param>
	public Block(PointI position)
	{
		Size = (1, 1);
		Position = position;
		Text = GetType().Name;
	}

	/// <summary>
	/// Overrides the ToString method to provide a custom string representation of the object.
	/// </summary>
	/// <returns>A string representation of the object.</returns>
	public override string ToString()
	{
		return $"{GetType().Name} \"{Text}\"";
	}

	/// <summary>
	/// Updates the block's state based on user input and other conditions.
	/// </summary>
	public void Update()
	{
		LimitSizeMin((1, 1));

		justInteracted.Clear();
		var (ix, iy) = Input.Position;

		mask = wasMaskSet ? mask : Input.Mask;
		var wasHovered = IsHovered;
		var (mx, my, mw, mh) = mask;
		var isInputInsideMask = ix >= mx && iy >= my && ix < mx + mw && iy < my + mh;
		IsHovered = IsOverlapping(Input.Position) && isInputInsideMask;

		if (IsDisabled)
		{
			if (IsHovered && IsHidden == false)
				Input.CursorResult = MouseCursor.Disable;

			OnUpdate?.Invoke();
			TryDisplaySelfAndProcessChildren();
			return;
		}

		if (IsHovered)
			Input.CursorResult = MouseCursor.Arrow;

		var isJustPressed = Input.IsButtonJustPressed() && IsPressed();
		var isJustScrolled = Input.ScrollDelta != 0 && IsHovered;

		if (isJustPressed || isJustScrolled)
			IsFocused = true;

		if (Input.IsKeyJustPressed(Key.Escape))
			IsFocused = false;

		TryTrigger();

		if (IsFocused && Input.FocusedPrevious != this)
			Interact(Interaction.Focus);
		if (IsFocused == false && Input.FocusedPrevious == this)
			Interact(Interaction.Unfocus);
		if (IsHovered && wasHovered == false)
			Interact(Interaction.Hover);
		if (IsHovered == false && wasHovered)
			Interact(Interaction.Unhover);
		if (IsPressed() && Input.IsButtonJustPressed())
			Interact(Interaction.Press);
		if (IsHovered && IsPressed() == false && Input.IsButtonJustReleased())
			Interact(Interaction.Release);
		if (IsPressedAndHeld && Input.IsJustHeld)
			Interact(Interaction.PressAndHold);

		const MouseButton RMB = MouseButton.Right;
		if (IsPressed(RMB) && Input.IsButtonJustPressed(RMB))
			Interact(Interaction.PressRight);
		if (IsHovered && IsPressed(RMB) == false && Input.IsButtonJustReleased(RMB))
			Interact(Interaction.ReleaseRight);

		const MouseButton MMB = MouseButton.Middle;
		if (IsPressed(MMB) && Input.IsButtonJustPressed(MMB))
			Interact(Interaction.PressMiddle);
		if (IsHovered && IsPressed(MMB) == false && Input.IsButtonJustReleased(MMB))
			Interact(Interaction.ReleaseMiddle);

		var p = Input.Position;
		var pp = Input.PositionPrevious;
		var px = (int)Math.Floor(p.x);
		var py = (int)Math.Floor(p.y);
		var ppx = (int)Math.Floor(pp.x);
		var ppy = (int)Math.Floor(pp.y);

		if ((px != ppx || py != ppy) && IsPressedAndHeld && IsFocused && Input.FocusedPrevious == this)
		{
			var delta = (px - ppx, py - ppy);
			OnDrag?.Invoke(delta);
		}

		OnUpdate?.Invoke();

		if (isInputInsideMask)
			OnInput();

		TryDisplaySelfAndProcessChildren();

		if (Input.ScrollDelta == 0 || IsScrollable == false)
			return; // scroll even when hovering children

		Interact(Interaction.Scroll);
		ApplyScroll();

		void TryDisplaySelfAndProcessChildren()
		{
			if (IsHidden)
				return;

			OnDisplay?.Invoke();

			OnChildrenUpdate();
			// parents call OnDisplay on children and themselves to ensure order if needed
			OnChildrenDisplay();
		}
	}

	/// <summary>
	/// Checks if the mouse button is pressed on the block.
	/// </summary>
	/// <param name="button">The mouse button to check.</param>
	/// <returns>True if the mouse button is pressed on the block, false otherwise.</returns>
	public bool IsPressed(MouseButton button = default)
	{
		return IsHovered && Input.IsButtonPressed(button);
	}
	public bool IsJustInteracted(Interaction interaction)
	{
		return justInteracted.Contains(interaction);
	}

	/// <summary>
	/// Checks if the block is overlapping with a specified point.
	/// </summary>
	/// <param name="point">The point to check for overlap.</param>
	/// <returns>True if the block is overlapping with the point, false otherwise.</returns>
	public bool IsOverlapping(PointF point)
	{
		if (point.x < 0 || point.x >= Input.Bounds.width || point.y < 0 || point.y >= Input.Bounds.height)
			return false;

		return point.x >= Position.x &&
		       point.x < Position.x + Size.width &&
		       point.y >= Position.y &&
		       point.y < Position.y + Size.height;
	}
	public bool IsOverlapping(Area area)
	{
		var (x, y, w, h) = Area;
		var (rx, ry, rw, rh) = area;
		return (x + w <= rx || x >= rx + rw || y + h <= ry || y >= ry + rh) == false;
	}
	public bool IsContaining(PointF point)
	{
		if (point.x < 0 || point.x >= Input.Bounds.width || point.y < 0 || point.y >= Input.Bounds.height)
			return false;

		return point.x >= Position.x &&
		       point.x < Position.x + Size.width &&
		       point.y >= Position.y &&
		       point.y < Position.y + Size.height;
	}
	public bool IsContaining(Area area)
	{
		var (x, y, w, h) = Area;
		var (rx, ry, rw, rh) = area;
		return rx >= x && ry >= y && rx + rw <= x + w && ry + rh <= y + h;
	}

	public void AlignInside(PointF alignment, Area? area = null)
	{
		var (ax, ay, aw, ah) = area ?? (0, 0, Input.Bounds.width, Input.Bounds.height);
		var (x, y, w, h) = Area;
		var newX = Map(alignment.x, (0, 1), (ax, aw - w));
		var newY = Map(alignment.y, (0, 1), (ay, ah - h));

		Position = (float.IsNaN(alignment.x) ? x : (int)newX, float.IsNaN(alignment.y) ? y : (int)newY);
	}
	public void AlignInside(Area? area = null, Pivot pivot = Center, PointI offset = default)
	{
		var (ax, ay, aw, ah) = area ?? (0, 0, Input.Bounds.width, Input.Bounds.height);
		var (x, y, w, h) = Area;

		if (pivot == TopLeft) (x, y) = (ax, ay);
		else if (pivot == Top) (x, y) = (ax + aw / 2 - w / 2, ay);
		else if (pivot == TopRight) (x, y) = (ax + aw - w, ay);
		else if (pivot == Left) (x, y) = (ax, ay + ah / 2 - h / 2);
		else if (pivot == Center) (x, y) = (ax + aw / 2 - w / 2, ay + ah / 2 - h / 2);
		else if (pivot == Right) (x, y) = (ax + aw - w, ay + ah / 2 - h / 2);
		else if (pivot == BottomLeft) (x, y) = (ax, ay + ah - h);
		else if (pivot == Bottom) (x, y) = (ax + aw / 2 - w / 2, ay + ah - h);
		else if (pivot == BottomRight) (x, y) = (ax + aw - w, ay + ah - h);

		Position = (x + offset.x, y + offset.y);
	}
	public void AlignOutside(Area? area = null, Pivot pivot = Center, PointI offset = default)
	{
		var (ax, ay, aw, ah) = area ?? (0, 0, Input.Bounds.width, Input.Bounds.height);
		var (x, y, w, h) = Area;
		var (cx, cy) = (ax + aw / 2 - w / 2, ay + ah / 2 - h / 2);

		if (pivot == TopLeft) (x, y) = (ax - w, ay - h);
		else if (pivot == Top) (x, y) = (cx, ay - h);
		else if (pivot == TopRight) (x, y) = (ax + aw, ay - h);
		else if (pivot == Left) (x, y) = (ax - w, cy);
		else if (pivot == Center)
		{
			// if axis is inside, align on the closest edge
			if (x > ax && x < ax + aw)
				x = x < cx ? ax - w : ax + aw;
			if (y > ay && y < ay + ah)
				y = y < cy ? ay - h : ay + ah;
		}
		else if (pivot == Right) (x, y) = (ax + aw, cy);
		else if (pivot == BottomLeft) (x, y) = (ax - w, ay + ah);
		else if (pivot == Bottom) (x, y) = (cx, ay + ah);
		else if (pivot == BottomRight) (x, y) = (ax + aw, ay + ah);

		Position = (x + offset.x, y + offset.y);
	}
	public void AlignX((Pivot self, Pivot target) pivots, Area? area = null, int offset = default)
	{
		var (ax, _, aw, _) = area ?? (0, 0, Input.Bounds.width, Input.Bounds.height);
		var (x, w) = (X, Width);

		if (pivots.target is TopLeft or Left or BottomLeft) x = ax;
		if (pivots.target is Top or Center or Bottom) x = ax + aw / 2;
		if (pivots.target is TopRight or Right or BottomRight) x = ax + aw;

		// if (pivots.self is Pivot.TopLeft or Pivot.Left or Pivot.BottomLeft) x -= 0;
		if (pivots.self is Top or Center or Bottom) x -= w / 2;
		if (pivots.self is TopRight or Right or BottomRight) x -= w;

		X = x + offset;
	}
	public void AlignY((Pivot self, Pivot target) pivots, Area? area = null, int offset = default)
	{
		var (_, ay, _, ah) = area ?? (0, 0, Input.Bounds.width, Input.Bounds.height);
		var (y, h) = (Y, Height);

		if (pivots.target is TopLeft or Top or TopRight) y = ay;
		if (pivots.target is Left or Center or Right) y = ay + ah / 2;
		if (pivots.target is BottomLeft or Bottom or BottomRight) y = ay + ah;

		// if (pivots.self is Pivot.TopLeft or Pivot.Top or Pivot.TopRight) y -= 0;
		if (pivots.self is Left or Center or Right) y -= h / 2;
		if (pivots.self is BottomLeft or Bottom or BottomRight) y -= h;

		Y = y + offset;
	}
	public void Fit(Area? area = null)
	{
		var (w, h) = Size;
		var (ax, ay, aw, ah) = area ?? (0, 0, Input.Bounds.width, Input.Bounds.height);
		var newX = aw - w < ax ? ax : Math.Clamp(Position.x, ax, aw - w);
		var newY = ah - h < ay ? ay : Math.Clamp(Position.y, ay, ah - h);
		var hasOverflown = false;

		if (aw - w < 0)
		{
			AlignInside((0.5f, float.NaN), area);
			hasOverflown = true;
		}

		if (ah - h < 0)
		{
			AlignInside((float.NaN, 0.5f), area);
			hasOverflown = true;
		}

		if (hasOverflown == false)
			Position = (newX, newY);
	}

	/// <summary>
	/// Interacts with the block based on the specified interaction.
	/// </summary>
	/// <param name="interaction">The interaction type.</param>
	public void Interact(Interaction interaction)
	{
		if (justInteracted.Contains(interaction) == false)
			justInteracted.Add(interaction);

		if (interactions.TryGetValue(interaction, out var act))
			act.Invoke();
	}

	/// <summary>
	/// Adds a method to be called when a specific interaction occurs.
	/// </summary>
	/// <param name="interaction">The interaction that triggers the method.</param>
	/// <param name="method">The method to be called.</param>
	public void OnInteraction(Interaction interaction, Action method)
	{
		if (interactions.TryAdd(interaction, method) == false)
			interactions[interaction] += method;
	}

	/// <summary>
	/// Called when input is received.
	/// </summary>
	protected virtual void OnInput()
	{
	}

	/// <summary>
	/// Converts the block to a tuple of integers representing the position and size.
	/// </summary>
	/// <param name="block">The block to convert.</param>
	/// <returns>A tuple of integers representing the position and size of the Block.</returns>
	public static implicit operator Area(Block block)
	{
		return (block.Position.x, block.Position.y, block.Size.width, block.Size.height);
	}

	public static void SortRow(Block[]? blocks, Area? area = null, Pivot pivot = Center, PointI gap = default, bool wrap = true)
	{
		if (blocks == null || blocks.Length == 0)
			return;

		area ??= (0, 0, Input.Bounds.width, Input.Bounds.height);
		var originalArea = area;
		var accumulatedWidth = 0;
		var height = 0;
		var totalHeight = 0;

		lines.Clear();
		lines.Add((0, 0, []));

		for (var i = 0; i < blocks.Length; i++)
		{
			var h = blocks[i].Height + gap.y;
			accumulatedWidth += (i == 0 ? 0 : gap.x) + blocks[i].Width;
			height = height < h ? h : height;

			if (wrap && accumulatedWidth > originalArea.Value.width)
			{
				accumulatedWidth = (i == 0 ? 0 : gap.x) + blocks[i].Width;
				lines.Add((0, 0, []));
				height = h;
			}

			lines[^1] = (accumulatedWidth, height, lines[^1].blocks);
			lines[^1].blocks.Add(blocks[i]);
		}

		foreach (var (_, h, _) in lines)
			totalHeight += h;

		//====================================================

		var y = originalArea.Value.y;
		for (var i = 0; i < lines.Count; i++)
		{
			var (w, h, bs) = lines[i];

			bs[0].Position = (originalArea.Value.x, y + (int)MathF.Round((h - bs[0].Height) / 2f));

			if (pivot is Top or Center or Bottom)
				bs[0].X += (originalArea.Value.width - w) / 2;
			else if (pivot is TopRight or Right or BottomRight)
				bs[0].X += originalArea.Value.width - w;

			if (pivot is Left or Center or Right)
				bs[0].Y += (originalArea.Value.height - totalHeight) / 2;
			else if (pivot is BottomLeft or Bottom or BottomRight)
				bs[0].Y += originalArea.Value.height - totalHeight;

			area = bs[0].Area;

			foreach (var b in bs.Skip(1))
			{
				b.AlignX((Left, Right), area, gap.x);
				b.AlignY((Left, Right), area);
				area = b.Area;
			}

			y += h;
		}
	}
	public static void SortColumn(Block[]? blocks, Area? area = null, Pivot pivot = Center, PointI gap = default, bool wrap = true)
	{
		if (blocks == null || blocks.Length == 0)
			return;

		area ??= (0, 0, Input.Bounds.width, Input.Bounds.height);
		var originalArea = area;
		var columns = new List<(int width, int height, List<Block> blocks)> { (0, 0, []) };
		var accumulatedHeight = 0;
		var width = 0;
		var totalWidth = 0;

		for (var i = 0; i < blocks.Length; i++)
		{
			var w = blocks[i].Width + gap.x;
			accumulatedHeight += (i == 0 ? 0 : gap.y) + blocks[i].Height;
			width = width < w ? w : width;

			if (wrap && accumulatedHeight > originalArea.Value.height)
			{
				accumulatedHeight = (i == 0 ? 0 : gap.y) + blocks[i].Height;
				columns.Add((0, 0, []));
				width = w;
			}

			columns[^1] = (width, accumulatedHeight, columns[^1].blocks);
			columns[^1].blocks.Add(blocks[i]);
		}

		foreach (var (w, _, _) in columns)
			totalWidth += w;

		//====================================================

		var x = originalArea.Value.x;
		for (var i = 0; i < columns.Count; i++)
		{
			var (w, h, bs) = columns[i];

			bs[0].Position = (x + (int)MathF.Round((w - bs[0].Width) / 2f), originalArea.Value.y);

			if (pivot is Top or Center or Bottom)
				bs[0].X += (originalArea.Value.width - totalWidth) / 2;
			else if (pivot is TopRight or Right or BottomRight)
				bs[0].X += originalArea.Value.width - totalWidth;

			if (pivot is Left or Center or Right)
				bs[0].Y += (originalArea.Value.height - h) / 2;
			else if (pivot is BottomLeft or Bottom or BottomRight)
				bs[0].Y += originalArea.Value.height - h;

			area = bs[0].Area;

			foreach (var b in bs.Skip(1))
			{
				b.AlignX((Top, Bottom), area);
				b.AlignY((Top, Bottom), area, gap.y);
				area = b.Area;
			}

			x += w;
		}
	}

	public static Area GetBounds(Block[]? blocks)
	{
		if (blocks == null || blocks.Length == 0)
			return default;

		var (minX, minY) = (int.MaxValue, int.MaxValue);
		var (maxX, maxY) = (int.MinValue, int.MinValue);
		foreach (var block in blocks)
		{
			var (x, y, w, h) = block.Area;
			minX = x < minX ? x : minX;
			minY = y < minY ? y : minY;
			maxX = x + w > maxX ? x + w : maxX;
			maxY = y + h > maxY ? y + h : maxY;
		}

		return (minX, minY, maxX - minX, maxY - minY);
	}

#region Backend
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
	internal class DoNotSave : Attribute;

	internal PointI position;
	internal Size size, sizeMin = (1, 1), sizeMax = (int.MaxValue, int.MaxValue);
	internal bool hasParent, isTextReadonly;
	internal string text = string.Empty;
	internal Area mask;

	[DoNotSave]
	private static readonly List<(int width, int height, List<Block> blocks)> lines = [];

	[DoNotSave]
	internal (int, int) listSizeTrimOffset;
	[DoNotSave]
	internal bool wasMaskSet;
	[DoNotSave]
	private bool isReadyForDoubleClick;
	[DoNotSave]
	private readonly List<Interaction> justInteracted = [];
	[DoNotSave]
	private readonly Dictionary<Interaction, Action> interactions = new();

	internal void LimitSizeMin(Size minimumSize)
	{
		if (Size.width < minimumSize.width)
			size = (minimumSize.width, Size.height);
		if (Size.height < minimumSize.height)
			size = (Size.width, minimumSize.height);
	}
	internal void LimitSizeMax(Size maximumSize)
	{
		if (Size.width > maximumSize.width)
			size = (maximumSize.width, Size.height);
		if (Size.height > maximumSize.height)
			size = (Size.width, maximumSize.height);
	}

	internal virtual void OnChildrenUpdate()
	{
	}
	internal virtual void OnChildrenDisplay()
	{
	}
	internal virtual void ApplyScroll()
	{
	}

	private void TryTrigger()
	{
		var isAllowed = Input.DOUBLE_CLICK_DELAY > Input.doubleClick.Elapsed.TotalSeconds;
		if ((isAllowed == false && isReadyForDoubleClick) || IsHovered == false)
			isReadyForDoubleClick = false;

		if (IsFocused == false || IsDisabled)
		{
			IsPressedAndHeld = false;
			return;
		}

		if (IsHovered && Input.IsButtonJustReleased() && IsPressedAndHeld)
		{
			IsPressedAndHeld = false;
			Interact(Interaction.Trigger);

			if (isReadyForDoubleClick == false)
			{
				Input.doubleClick.Restart();
				isReadyForDoubleClick = true;
				return;
			}

			if (isAllowed)
				Interact(Interaction.DoubleTrigger);

			isReadyForDoubleClick = false;
		}

		if (IsHovered && Input.IsButtonJustPressed())
			IsPressedAndHeld = true;

		if (IsHovered == false && Input.IsButtonJustReleased())
			IsPressedAndHeld = false;
	}

	private static float Map(float number, Range range, Range targetRange)
	{
		var value = (number - range.a) / (range.b - range.a) * (targetRange.b - targetRange.a) + targetRange.a;
		return float.IsNaN(value) || float.IsInfinity(value) ? targetRange.a : value;
	}
#endregion
}