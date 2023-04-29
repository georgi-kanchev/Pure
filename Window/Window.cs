namespace Pure.Window;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Provides access to an OS window and its properties.
/// </summary>
public static class Window
{
	/// <summary>
	/// Possible window modes.
	/// </summary>
	public enum Mode
	{
		Windowed, Borderless, Fullscreen
	}

	/// <summary>
	/// Gets or sets a value indicating whether the window is open.
	/// </summary>
	public static bool IsOpen
	{
		get
		{
			TryNoWindowException();
			return window.IsOpen;
		}
		set
		{
			if (value == false)
				Close();
		}
	}
	/// <summary>
	/// Gets or sets the title of the window.
	/// </summary>
	public static string Title
	{
		get
		{
			TryNoWindowException();
			return title;
		}
		set
		{
			TryNoWindowException();

			if (string.IsNullOrWhiteSpace(value))
				value = "Game";

			title = value;
			window.SetTitle(title);
		}
	}
	/// <summary>
	/// Gets the size of the window.
	/// </summary>
	public static (int width, int height) Size
	{
		get
		{
			TryNoWindowException();
			return ((int)window.Size.X, (int)window.Size.Y);
		}
	}
	/// <summary>
	/// Gets a value indicating whether the window is focused.
	/// </summary>
	public static bool IsFocused
	{
		get
		{
			TryNoWindowException();
			return window.HasFocus();
		}
	}
	/// <summary>
	/// Gets or sets a value indicating whether the window should use retro TV graphics.
	/// </summary>
	public static bool IsRetro
	{
		get
		{
			TryNoWindowException();
			return isRetro;
		}
		set
		{
			TryNoWindowException();

			if (value && Vertices.retroScreen == null && Shader.IsAvailable)
				Vertices.retroScreen = RetroShader.Create();

			isRetro = value;
		}
	}
	/// <summary>
	/// Gets the aspect ratio of the monitor the window was created on.
	/// </summary>
	public static (int width, int height) MonitorAspectRatio
	{
		get { TryNoWindowException(); return aspectRatio; }
	}
	/// <summary>
	/// Gets the mode that the window was created with.
	/// </summary>
	public static Mode InitialMode { get; private set; }

	/// <summary>
	/// Creates the window with the specified <paramref name="mode"/>
	/// on the specified <paramref name="monitor"/>.
	/// </summary>
	/// <param name="mode">The mode to create the window in.</param>
	/// <param name="monitor">The index of the user monitor to display the window on.</param>
	[MemberNotNull(nameof(window))]
	public static void Create(Mode mode = Mode.Windowed, uint monitor = 0)
	{
		if (window != null)
			return;

		InitialMode = mode;

		Monitor.Initialize();

		if (monitor >= Monitor.posSizes.Count)
			monitor = (uint)Monitor.posSizes.Count - 1;

		var style = Styles.Default;
		var (x, y, w, h) = Monitor.posSizes[(int)monitor];
		aspectRatio = Monitor.GetAspectRatio(w, h);

		if (mode == Mode.Fullscreen) style = Styles.Fullscreen;
		else if (mode == Mode.Borderless) style = Styles.None;
		else if (mode == Mode.Windowed)
		{
			w /= 2;
			h /= 2;
			x += w / 2;
			y += h / 2;

		}

		window = new(new VideoMode((uint)w, (uint)h), title, style);
		window.Position = new(x, y);
		window.Closed += (s, e) => Close();
		window.Resized += (s, e) => UpdateWindowAndView();
		window.LostFocus += (s, e) =>
		{
			Mouse.CancelInput();
			Keyboard.CancelInput();
		};

		window.KeyPressed += Keyboard.OnKeyPressed;
		window.KeyReleased += Keyboard.OnKeyReleased;
		window.MouseButtonPressed += Mouse.OnButtonPressed;
		window.MouseButtonReleased += Mouse.OnButtonReleased;
		window.MouseWheelScrolled += Mouse.OnWheelScrolled;

		window.DispatchEvents();
		window.Clear();
		window.Display();

		UpdateWindowAndView();

		//var str = DefaultGraphics.PNGToBase64String(
		//	"/home/gojur/code/Pure/Examples/bin/Debug/net6.0/graphics.png");

		graphics["default"] = DefaultGraphics.CreateTexture();
	}
	/// <summary>
	/// Activates or deactivates the window for updates and drawing. Ideally, an application
	/// loop would start with activating and end with deactivating the window.
	/// </summary>
	/// <param name="isActive">Whether the window should be activated for updates and drawing.</param>
	public static void Activate(bool isActive)
	{
		TryNoWindowException();

		if (isActive)
		{
			window.DispatchEvents();
			window.Clear();
			window.SetActive();
			Vertices.ClearQueue();
			return;
		}

		if (Mouse.CursorGraphics < Mouse.Cursor.TILE_WAIT)
			window.SetMouseCursorVisible(Mouse.IsCursorHoveringWindow == false);
		Mouse.Update();
		Vertices.DrawQueue();
		window.Display();
	}
	/// <summary>
	/// Sets the properties of the layer that the window should draw on.
	/// </summary>
	/// <param name="cellCount">The number of cells in the layer.</param>
	/// <param name="tileSize">The size of each tile in the graphics.</param>
	/// <param name="tileGap">The gap between each tile in the graphics.</param>
	/// <param name="graphicsPath">The path to the graphics file to use for the layer.</param>
	/// <param name="drawOrder">The order in which to draw the layer alongside layers.</param>
	public static void SetLayer((int horizontal, int vertical) cellCount = default, (int width, int height) tileSize = default, (int x, int y) tileGap = default, string? graphicsPath = null, int drawOrder = 0)
	{
		TryNoWindowException();

		cellCount = cellCount == default ? (48, 27) : cellCount;
		tileSize = tileSize == default ? (8, 8) : tileSize;
		graphicsPath ??= "default";

		TryLoadGraphics(graphicsPath);

		Vertices.layer = drawOrder;
		Vertices.graphicsPath = graphicsPath;
		Vertices.tileSize = tileSize;
		Vertices.tileGap = tileGap;
		Vertices.mapCellCount = ((uint)cellCount.Item1, (uint)cellCount.Item2);

		Vertices.TryInitQueue();
	}
	/// <summary>
	/// Closes the window.
	/// </summary>
	public static void Close()
	{
		TryNoWindowException();

		if (IsRetro || isClosing)
		{
			isClosing = true;

			Vertices.StartRetroAnimation();
			return;
		}

		window.Close();
	}

	/// <summary>
	/// Draws a set of <paramref name="points"/> to the window.
	/// </summary>
	/// <param name="points">An array of bundle tuples representing the position and color 
	/// of each point.</param>
	public static void DrawPoints(params (float x, float y, uint color)[] points)
	{
		TryNoWindowException();

		for (int i = 0; i < points?.Length; i++)
			Vertices.QueuePoint((points[i].x, points[i].y), points[i].color);
	}
	/// <summary>
	/// Draws a set of <paramref name="rectangles"/> to the window.
	/// </summary>
	/// <param name="rectangles">An array of bundle tuples representing the 
	/// position, size, and color of each rectangle.</param>
	public static void DrawRectangles(params (float x, float y, float width, float height, uint color)[] rectangles)
	{
		TryNoWindowException();

		for (int i = 0; i < rectangles?.Length; i++)
		{
			var r = rectangles[i];
			Vertices.QueueRectangle((r.x, r.y), (r.width, r.height), r.color);
		}
	}
	/// <summary>
	/// Draws a set of <paramref name="lines"/> to the window.
	/// </summary>
	/// <param name="lines">An array of bundle tuples representing the 
	/// start position, end position, and color of each line.</param>
	public static void DrawLines(params (float ax, float ay, float bx, float by, uint color)[] lines)
	{
		TryNoWindowException();

		for (int i = 0; i < lines?.Length; i++)
			Vertices.QueueLine((lines[i].ax, lines[i].ay), (lines[i].bx, lines[i].by), lines[i].color);
	}
	/// <summary>
	/// Draws a single <paramref name="tile"/> to the window.
	/// </summary>
	/// <param name="position">The position at which to draw the <paramref name="tile"/>.</param>
	/// <param name="tile">A bundle tuple representing the identifier, 
	/// tint, angle, and flip status of the tile.</param>
	/// <param name="size">The size of the tile, in tiles. Defaults to (1, 1) if not specified,
	/// (0, 0) or negative.</param>
	public static void DrawTile((float x, float y) position, (int id, uint tint, sbyte angle, bool isFlippedHorizontally, bool isFlippedVertically) tile, (int width, int height) size = default)
	{
		TryNoWindowException();
		var (id, tint, angle, flipH, flipV) = tile;
		Vertices.QueueTile(position, id, tint, angle, size, (flipH, flipV));
	}
	/// <summary>
	/// Draws a set of <paramref name="tiles"/> to the window.
	/// </summary>
	/// <param name="tiles">A 2D array of bundle tuples representing the identifier, 
	/// tint, angle, and flip status of each tile.</param>
	public static void DrawTiles((int id, uint tint, sbyte angle, bool isFlippedHorizontally, bool isFlippedVertically)[,] tiles)
	{
		TryNoWindowException();

		Vertices.QueueTilemap(tiles);
	}

	#region Backend
	internal static bool isRetro, isClosing;
	private static string title = "Game";
	private static (int, int) aspectRatio;

	internal static readonly Dictionary<string, Texture> graphics = new();
	internal static RenderWindow? window;

	private static void TryLoadGraphics(string path)
	{
		if (graphics.ContainsKey(path))
			return;

		graphics[path] = new(path);
	}
	private static void UpdateWindowAndView()
	{
		if (window == null)
			return;

		var view = window.GetView();
		var (w, h) = (RoundToMultipleOfTwo((int)Size.Item1), RoundToMultipleOfTwo((int)Size.Item2));
		view.Size = new(w, h);
		view.Center = new(RoundToMultipleOfTwo((int)(Size.Item1 / 2f)), RoundToMultipleOfTwo((int)(Size.Item2 / 2f)));
		window.SetView(view);
		window.Size = new((uint)w, (uint)h);
	}

	private static int RoundToMultipleOfTwo(int n)
	{
		var rem = n % 2;
		var result = n - rem;
		if (rem >= 1)
			result += 2;
		return result;
	}
	internal static (float, float) PositionFrom((int, int) screenPixel)
	{
		var x = Map(screenPixel.Item1, 0, Size.Item1, 0, Vertices.mapCellCount.Item1);
		var y = Map(screenPixel.Item2, 0, Size.Item2, 0, Vertices.mapCellCount.Item2);

		return (x, y);
	}
	private static float Map(float number, float a1, float a2, float b1, float b2)
	{
		var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
		return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
	}

	[MemberNotNull(nameof(window))]
	internal static void TryNoWindowException()
	{
		if (window == null)
			throw new MemberAccessException($"{nameof(Window)} is not created. Use {nameof(Create)}(...).");
	}
	internal static void TryArrayMismatchException(params Array[] arrays)
	{
		var length = 0;
		for (int i = 0; i < arrays.Length; i++)
		{
			length = i == 0 ? arrays[i].Length : length;

			if (arrays[i] == null || arrays.Length != length)
				throw new ArgumentException("All the provided arrays should be non-null and with equal sizes.");
		}
	}

	#endregion
}
