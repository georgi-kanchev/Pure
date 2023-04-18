namespace Pure.Window;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Provides a simple way to create and interact with an OS window.
/// </summary>
public static class Window
{
	/// <summary>
	/// The state of the OS window upon calling <see cref="Create"/>.
	/// </summary>
	public enum State
	{
		Windowed, Borderless, Fullscreen
	}

	/// <summary>
	/// Whether the OS window exists. This is <see langword="true"/> even when it
	/// is minimized or <see cref="IsHidden"/>.
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
	/// The title on the title bar of the OS window.
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
	/// The size of the OS window.
	/// </summary>
	public static (uint width, uint height) Size
	{
		get
		{
			TryNoWindowException();
			return (window.Size.X, window.Size.Y);
		}
		set
		{
			TryNoWindowException();
			window.Size = new(value.Item1, value.Item2);
		}
	}
	/// <summary>
	/// Returns whether the OS window is currently focused.
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
	/// Whether the OS window has a retro TV/arcade screen effect over it.
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
	/// Returns the aspect ratio of the desktop monitor that the OS window was created on.
	/// This is useful for keeping the size of the window contents consistent throughout
	/// different resolutions with the same aspect ratio.
	/// </summary>
	public static (int width, int height) MonitorAspectRatio
	{
		get { TryNoWindowException(); return aspectRatio; }
	}

	/// <summary>
	/// The <see cref="State"/> used to create the OS window with.
	/// </summary>
	public static State InitialState { get; private set; }

	/// <summary>
	/// Creates an OS window with <paramref name="state"/> on a desktop <paramref name="monitor"/>.
	/// </summary>
	[MemberNotNull(nameof(window))]
	public static void Create(State state = State.Windowed, uint monitor = 0)
	{
		if (window != null)
			return;

		InitialState = state;

		Monitor.Initialize();

		if (monitor >= Monitor.posSizes.Count)
			monitor = (uint)Monitor.posSizes.Count - 1;

		var style = Styles.Default;
		var (x, y, w, h) = Monitor.posSizes[(int)monitor];
		aspectRatio = Monitor.GetAspectRatio(w, h);

		if (state == State.Fullscreen) style = Styles.Fullscreen;
		else if (state == State.Borderless) style = Styles.None;
		else if (state == State.Windowed)
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
	/// Determines whether the <see cref="Window"/> <paramref name="isActive"/>.
	/// An application loop should ideally activate it at the very start and
	/// deactivate it at the very end.
	/// </summary>
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

		window.SetMouseCursorVisible(Mouse.IsCursorHoveringWindow == false);
		Mouse.Update();
		Vertices.DrawQueue();
		window.Display();
	}
	/// <summary>
	/// Terminates the OS window and closes the application.
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
	/// Prepares a <paramref name="layer"/> for drawing. The <paramref name="layer"/> has a size of
	/// <paramref name="cellCount"/> tiles. Its graphics are loaded and cached from a
	/// <paramref name="graphicsPath"/> containing tiles of <paramref name="tileSize"/> with a
	/// <paramref name="tileGap"/> in between. Default values result in default graphics:<br></br>
	/// <paramref name="layer"/>= 0<br></br>
	/// <paramref name="graphicsPath"/>= "default"/null<br></br>
	/// <paramref name="cellCount"/>= (48, 27)<br></br>
	/// </summary>
	public static void SetLayer((uint cellsHorizontal, uint cellsVertical) cellCount = default, (uint tileWidth, uint tileHeight) tileSize = default, (uint tileGapX, uint tileGapY) tileGap = default, string? graphicsPath = null, int layer = 0)
	{
		TryNoWindowException();

		cellCount = cellCount == default ? (48, 27) : cellCount;
		tileSize = tileSize == default ? (8, 8) : tileSize;
		graphicsPath ??= "default";

		TryLoadGraphics(graphicsPath);

		Vertices.layer = layer;
		Vertices.graphicsPath = graphicsPath;
		Vertices.tileSize = tileSize;
		Vertices.tileGap = tileGap;
		Vertices.mapCellCount = ((uint)cellCount.Item1, (uint)cellCount.Item2);

		Vertices.TryInitQueue();
	}

	/// <summary>
	/// Draws a tilemap onto the OS window. Its graphics image is loaded from a
	/// <paramref name="path"/> (default graphics if <see langword="null"/>) using a
	/// <paramref name="tileSize"/>, <paramref name="tileGaps"/> and then it is cached
	/// for future draws. The tilemap's contents are decided by <paramref name="tiles"/>,
	/// <paramref name="tints"/>, <paramref name="angles"/>, <paramref name="flips"/>
	/// (flip first, rotation second - order matters) and their Z order by a <paramref name="layer"/>.
	/// </summary>
	public static void DrawTilemap(int[,] tiles, uint[,] tints, sbyte[,] angles, (bool isFlippedHorizontally, bool isFlippedVertically)[,] flips)
	{
		TryNoWindowException();

		if (tiles == null || tints == null || angles == null || flips == null ||
			tiles.Length != tints.Length || tiles.Length != angles.Length || tiles.Length != flips.Length)
			throw new ArgumentException("All the provided arrays should be non-null and with equal sizes.");

		Vertices.QueueTilemap(tiles, tints, angles, flips);
	}
	/// <summary>
	/// Draws a <paramref name="tilemap"/> onto the OS window. Its graphics image is loaded from a
	/// <paramref name="path"/> (default graphics if <see langword="null"/>) using a
	/// <paramref name="tileSize"/> and <paramref name="tileGaps"/>, then it is cached
	/// for future draws. The <paramref name="tilemap"/>'s Z order is decided by a <paramref name="layer"/>.
	/// </summary>
	public static void DrawTilemap((int[,] tiles, uint[,] tints, sbyte[,] angles, (bool isFlippedHorizontally, bool isFlippedVertically)[,] flips) tilemap)
	{
		var (tiles, tints, angles, flips) = tilemap;
		DrawTilemap(tiles, tints, angles, flips);
	}
	/// <summary>
	/// Draws a sprite onto the OS window. Its graphics are decided by a <paramref name="tile"/>
	/// from the last <see cref="DrawTilemap"/> call, a <paramref name="tint"/>, an
	/// <paramref name="angle"/>, and a <paramref name="size"/> (negative values flip the sprite).
	/// Order matters - flips first, rotates second.
	/// The sprite's <paramref name="position"/> is also relative to the previously drawn tilemap.
	/// </summary>
	public static void DrawSprite((float x, float y) position, int tile, uint tint = uint.MaxValue, sbyte angle = 0, (int width, int height) size = default)
	{
		TryNoWindowException();

		Vertices.QueueSprite(position, tile, tint, angle, size);
	}

	/// <summary>
	/// Draws single pixel points with <paramref name="tint"/> onto the OS window.
	/// Their <paramref name="positions"/> are relative to the previously drawn tilemap.
	/// </summary>
	public static void DrawPoints(uint tint, params (float x, float y)[] positions)
	{
		TryNoWindowException();

		if (positions == null || positions.Length == 0)
			return;

		Vertices.QueuePoints(tint, positions);
	}
	/// <summary>
	/// Draws a rectangle with <paramref name="tint"/> onto the OS window.
	/// Its <paramref name="position"/> and <paramref name="size"/> are relative
	/// to the previously drawn tilemap.
	/// </summary>
	public static void DrawRectangle((float x, float y) position, (float width, float height) size, uint tint = uint.MaxValue)
	{
		TryNoWindowException();
		Vertices.QueueRectangle(position, size, tint);
	}
	/// <summary>
	/// Draws a line between <paramref name="pointStart"/> and <paramref name="pointEnd"/> with
	/// <paramref name="tint"/> onto the OS window.
	/// Its points are relative to the previously drawn tilemap.
	/// </summary>
	public static void DrawLine((float x, float y) pointStart, (float x, float y) pointEnd, uint tint = uint.MaxValue)
	{
		TryNoWindowException();
		Vertices.QueueLine(pointStart, pointEnd, tint);
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

	#endregion
}
