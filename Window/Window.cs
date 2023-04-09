﻿namespace Pure.Window;

using Raylib_cs;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

using Shader = SFML.Graphics.Shader;
using BlendMode = SFML.Graphics.BlendMode;
using Transform = SFML.Graphics.Transform;
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
	public static (uint, uint) Size
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

			if (value && retroScreen == null && Shader.IsAvailable)
				retroScreen = RetroShader.Create();

			isRetro = value;
		}
	}

	/// <summary>
	/// The <see cref="State"/> used to create the OS window with.
	/// </summary>
	public static State InitialState { get; private set; }

	/// <summary>
	/// Creates an OS window with <paramref name="state"/>.
	/// </summary>
	[MemberNotNull(nameof(window))]
	public static void Create(State state = State.Windowed)
	{
		if (window != null)
			return;

		StoreMonitorData();

		InitialState = state;

		var style = Styles.Default;
		var (x, y, w, h) = monitorPosSize[0];

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
		Draw();
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

			retroTurnoffTime = new();
			retroTurnoff = new(RETRO_TURNOFF_TIME * 1000);
			retroTurnoff.Start();
			retroTurnoff.Elapsed += (s, e) => window.Close();
			return;
		}

		window.Close();
	}

	/// <summary>
	/// Draws a tilemap onto the OS window. Its graphics image is loaded from a
	/// <paramref name="path"/> (default graphics if <see langword="null"/>) using a
	/// <paramref name="tileSize"/> and <paramref name="tileGaps"/>, then it is cached
	/// for future draws. The tilemap's contents are decided by <paramref name="tiles"/>,
	/// <paramref name="tints"/>, <paramref name="angles"/> and <paramref name="flips"/>
	/// (flip first, rotation second - order matters).
	/// </summary>
	public static void DrawTilemap(int[,] tiles, uint[,] tints, sbyte[,] angles, (bool, bool)[,] flips, (uint, uint) tileSize, (uint, uint) tileGaps = default, string? path = default)
	{
		TryNoWindowException();

		if (tiles == null || tints == null || tiles.Length != tints.Length)
			return;

		path ??= "default";

		TryLoadGraphics(path);
		Vertices.QueueTilemap(tiles, tints, angles, flips, tileSize, tileGaps, path);
	}
	/// <summary>
	/// Draws a <paramref name="tilemap"/> onto the OS window. Its graphics image is loaded from a
	///  (default graphics if <see langword="null"/>) using a
	/// <paramref name="tileSize"/> and <paramref name="tileGaps"/>, then it is cached
	/// for future draws.
	/// </summary>
	public static void DrawTilemap((int[,], uint[,], sbyte[,], (bool, bool)[,]) tilemap, (uint, uint) tileSize, (uint, uint) tileGaps = default, string? path = default)
	{
		var (tiles, tints, angles, flips) = tilemap;
		DrawTilemap(tiles, tints, angles, flips, tileSize, tileGaps, path);
	}
	/// <summary>
	/// Draws a sprite onto the OS window. Its graphics are decided by a <paramref name="tile"/>
	/// from the last <see cref="DrawTilemap"/> call, a <paramref name="tint"/>, an
	/// <paramref name="angle"/> and a <paramref name="flip"/>
	/// (flip first, rotation second - order matters).
	/// The sprite's <paramref name="position"/> is also relative to the previously drawn tilemap.
	/// </summary>
	public static void DrawSprite((float, float) position, int tile, uint tint = uint.MaxValue, sbyte angle = 0, (bool, bool) flip = default, (uint, uint) size = default)
	{
		TryNoWindowException();

		if (Vertices.prevDrawTilesetGfxPath == null)
			return;

		Vertices.QueueSprite(position, tile, tint, angle, size, flip);
	}
	/// <summary>
	/// Draws single pixel points with <paramref name="tint"/> onto the OS window.
	/// Their <paramref name="positions"/> are relative to the previously drawn tilemap.
	/// </summary>
	public static void DrawPoints(uint tint, params (float, float)[] positions)
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
	public static void DrawRectangle((float, float) position, (float, float) size, uint tint = uint.MaxValue)
	{
		TryNoWindowException();
		Vertices.QueueRectangle(position, size, tint);
	}
	/// <summary>
	/// Draws a line between <paramref name="pointA"/> and <paramref name="pointB"/> with
	/// <paramref name="tint"/> onto the OS window.
	/// Its points are relative to the previously drawn tilemap.
	/// </summary>
	public static void DrawLine((float, float) pointA, (float, float) pointB, uint tint = uint.MaxValue)
	{
		TryNoWindowException();
		Vertices.QueueLine(pointA, pointB, tint);
	}

	#region Backend
	private static bool isRetro, isClosing;
	private static string title = "Game";

	private static Shader? retroScreen;
	private static RenderStates Rend => IsRetro ? new(retroScreen) : default;
	private static readonly SFML.System.Clock retroScreenTimer = new();
	private static Random retroRand = new();
	private static System.Timers.Timer? retroTurnoff;
	private static Clock? retroTurnoffTime;
	private const float RETRO_TURNOFF_TIME = 0.5f;
	private static List<(int, int, int, int)> monitorPosSize = new();

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
	private static void StoreMonitorData()
	{
		Raylib.SetTraceLogLevel(TraceLogLevel.LOG_NONE);
		Raylib.InitWindow(1, 1, "");
		Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_HIDDEN);
		Raylib.SetWindowPosition(-1000, -1000);

		var monitorCount = Raylib.GetMonitorCount();
		for (int i = 0; i < monitorCount; i++)
		{
			var p = Raylib.GetMonitorPosition(i);
			var w = Raylib.GetMonitorWidth(i);
			var h = Raylib.GetMonitorHeight(i);
			monitorPosSize.Add(((int)p.X, (int)p.Y, w, h));
		}
		Raylib.CloseWindow();
	}
	private static void Draw()
	{
		if (window == null)
			return;

		foreach (var kvp in graphics)
		{
			var tex = graphics[kvp.Key];
			var shader = IsRetro ? retroScreen : null;
			var rend = new RenderStates(BlendMode.Alpha, Transform.Identity, tex, shader);
			var randVec = new Vector2f(retroRand.Next(0, 10) / 10f, retroRand.Next(0, 10) / 10f);

			if (IsRetro)
			{
				shader?.SetUniform("time", retroScreenTimer.ElapsedTime.AsSeconds());
				shader?.SetUniform("randomVec", randVec);
				shader?.SetUniform("viewSize", window.GetView().Size);

				if (isClosing && retroTurnoffTime != null)
				{
					var timing = retroTurnoffTime.ElapsedTime.AsSeconds() / RETRO_TURNOFF_TIME;
					shader?.SetUniform("turnoffAnimation", timing);
				}
			}
			window.Draw(Vertices.GetFromQueue(kvp.Key), rend);
		}
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
		var x = Map(screenPixel.Item1, 0, Size.Item1, 0, Vertices.prevDrawTilemapCellCount.Item1);
		var y = Map(screenPixel.Item2, 0, Size.Item2, 0, Vertices.prevDrawTilemapCellCount.Item2);

		return (x, y);
	}
	private static float Map(float number, float a1, float a2, float b1, float b2)
	{
		var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
		return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
	}
	private static (uint, uint) GetAspectRatio(uint width, uint height)
	{
		var gcd = height == 0 ? width : GetGreatestCommonDivisor(height, width % height);

		return (width / gcd, height / gcd);

		uint GetGreatestCommonDivisor(uint a, uint b)
		{
			return b == 0 ? a : GetGreatestCommonDivisor(b, a % b);
		}
	}

	[MemberNotNull(nameof(window))]
	internal static void TryNoWindowException()
	{
		if (window == null)
			throw new MemberAccessException($"{nameof(Window)} is not created. Use {nameof(Create)}(...).");
	}

	#endregion
}
