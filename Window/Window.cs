namespace Pure.Window;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

using System.Diagnostics.CodeAnalysis;

public static class Window
{
	public enum State
	{
		Windowed, Borderless, Fullscreen
	}


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

	public static (int width, int height) Size
	{
		get
		{
			TryNoWindowException();
			return ((int)window.Size.X, (int)window.Size.Y);
		}
	}

	public static bool IsFocused
	{
		get
		{
			TryNoWindowException();
			return window.HasFocus();
		}
	}

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

	public static (int width, int height) MonitorAspectRatio
	{
		get { TryNoWindowException(); return aspectRatio; }
	}

	public static State InitialState { get; private set; }


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

	public static void SetLayer((int cellCountH, int cellCountV) cellCount = default, (int tileWidth, int tileHeight) tileSize = default, (int tileGapX, int tileGapY) tileGap = default, string? graphicsPath = null, int layer = 0)
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


	public static void DrawBasicPoint((float x, float y) position, uint color)
	{
		TryNoWindowException();
		Vertices.QueuePoint(position, color);
	}

	public static void DrawBasicRectangle((float x, float y) position, (float width, float height) size, uint color = uint.MaxValue)
	{
		TryNoWindowException();
		Vertices.QueueRectangle(position, size, color);
	}

	public static void DrawBasicLine((float x, float y) start, (float x, float y) end, uint color)
	{
		TryNoWindowException();
		Vertices.QueueLine(start, end, color);
	}

	public static void DrawBasicSprite((float x, float y) position, int tile, uint tint = uint.MaxValue, sbyte angle = 0, (int width, int height) size = default)
	{
		TryNoWindowException();

		Vertices.QueueTile(position, tile, tint, angle, size);
	}

	public static void DrawBundleTilemap((int tile, uint tint, sbyte angle, (bool isFlippedH, bool isFlippedV) flips)[,] tiles)
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
