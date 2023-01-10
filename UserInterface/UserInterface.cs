namespace Pure.UserInterface
{
	/// <summary>
	/// The OS and tile mouse cursors that result from interacting with the <see cref="UserInterface"/>.
	/// </summary>
	public enum CursorResult
	{
		TileArrow, TileArrowNoTail, TileHand, TileText, TileCrosshair, TileNo, TileResizeHorizontal, TileResizeVertical,
		TileResizeDiagonal1, TileResizeDiagonal2, TileMove, TileWait1, TileWait2, TileWait3,

		SystemArrow, SystemArrowWait, SystemWait, SystemText, SystemHand, SystemResizeHorinzontal, SystemResizeVertical,
		SystemResizeDiagonal2, SystemResizeDiagonal1, SystemMove, SystemCrosshair, SystemHelp, SystemNo,

		None
	}

	public abstract class UserInterface
	{
		public (int, int) Position { get; set; }
		public (int, int) Size
		{
			get => size;
			set
			{
				var (x, y) = value;
				size = (Math.Max(x, 1), Math.Max(y, 1));
			}
		}
		public string Text { get; set; } = "";

		public bool IsFocused
		{
			get => focusedObject == this;
			private set => focusedObject = value ? this : null;
		}
		public bool IsHovered
		{
			get
			{
				var (ix, iy) = Input.Position;
				var (x, y) = Position;
				var (w, h) = Size;
				var isHoveredX = ix >= x && ix < x + w;
				var isHoveredY = iy >= y && iy < y + h;
				if(w < 0)
					isHoveredX = ix > x + w && ix <= x;
				if(h < 0)
					isHoveredY = iy > y + h && iy <= y;

				return isHoveredX && isHoveredY;
			}
		}
		public bool IsPressed => IsHovered && Input.IsPressed && IsClicked;
		public static CursorResult MouseCursorTile { get; internal set; }
		public static CursorResult MouseCursorSystem { get; internal set; }

		public UserInterface((int, int) position, (int, int) size)
		{
			Position = position;
			Size = size;
		}

		public void Update()
		{
			if(Input.WasPressed == false && Input.IsPressed && IsHovered)
				IsFocused = true;

			TryTrigger();
			OnUpdate();
		}

		public static void ApplyInput(Input input, (int, int) tilemapSize)
		{
			IsInputCanceled = false;

			MouseCursorTile = CursorResult.TileArrow;
			MouseCursorSystem = CursorResult.SystemArrow;
			TilemapSize = (Math.Abs(tilemapSize.Item1), Math.Abs(tilemapSize.Item2));

			input.WasPressed = Input.IsPressed;
			input.WasPressedAlt = Input.IsPressedAlt;
			input.WasPressedBackspace = Input.IsPressedBackspace;
			input.WasPressedControl = Input.IsPressedControl;
			input.WasPressedEnter = Input.IsPressedEnter;
			input.WasPressedEscape = Input.IsPressedEscape;
			input.WasPressedShift = Input.IsPressedShift;
			input.WasPressedTab = Input.IsPressedTab;

			input.WasPressedLeft = Input.IsPressedLeft;
			input.WasPressedRight = Input.IsPressedRight;
			input.WasPressedUp = Input.IsPressedUp;
			input.WasPressedDown = Input.IsPressedDown;

			input.PrevTypedSymbols = Input.TypedSymbols;
			input.PrevPosition = Input.Position;

			Input = input;

			if(Input.WasPressed == false && Input.IsPressed)
				focusedObject = null;
		}

		#region Backend
		private (int, int) size;

		protected bool IsClicked { get; private set; }
		protected bool IsTriggered { get; private set; }
		protected static bool IsInputCanceled { get; private set; }
		protected static (int, int) TilemapSize { get; private set; }

		protected static Input Input { get; set; }
		private static UserInterface? focusedObject;

		protected abstract void OnUpdate();
		protected void TryTrigger()
		{
			IsTriggered = false;

			if(IsFocused == false)
			{
				IsClicked = false;
				return;
			}

			if(IsHovered && Input.IsReleased && IsClicked)
			{
				IsClicked = false;
				IsTriggered = true;
			}

			if(IsHovered && Input.IsPressed && Input.WasPressed == false)
				IsClicked = true;

			if(Input.IsReleased)
				IsClicked = false;
		}

		protected static void SetTileAndSystemCursor(CursorResult tileCursor)
		{
			MouseCursorTile = tileCursor;
			MouseCursorSystem = (CursorResult)((int)tileCursor + (int)CursorResult.SystemArrow);
		}
		#endregion
	}
}
