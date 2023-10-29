﻿global using SFML.Graphics;
global using SFML.Window;
global using SFML.System;

namespace Pure.Engine.Window;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Possible window modes.
/// </summary>
public enum Mode
{
    Windowed,
    Borderless,
    Fullscreen
}

/// <summary>
/// Provides access to an OS window and its properties.
/// </summary>
public static class Window
{
    /// <summary>
    /// Gets the mode that the window was created with.
    /// </summary>
    public static Mode Mode
    {
        get;
        private set;
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
            return size;
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

            if (value && retroScreen == null && Shader.IsAvailable)
                retroScreen = RetroShader.Create();

            isRetro = value;
        }
    }
    public static uint BackgroundColor
    {
        get;
        set;
    }

    [MemberNotNull(nameof(window))]
    public static void Create(float pixelScale = 5f, Mode mode = Mode.Windowed, uint monitor = 0)
    {
        if (window != null)
            return;

        Mode = mode;

        Monitor.current = (int)monitor;

        var style = Styles.Default;
        var (x, y) = Monitor.Position;
        var (w, h) = Monitor.Size;
        renderTexture = new((uint)(w / pixelScale), (uint)(h / pixelScale));
        var view = renderTexture.GetView();
        view.Center = new();
        renderTextureViewSize = ((int)view.Size.X, (int)view.Size.Y);
        renderTexture.SetView(view);

        if (mode == Mode.Fullscreen) style = Styles.Fullscreen;
        else if (mode == Mode.Borderless) style = Styles.None;
        else if (mode == Mode.Windowed)
        {
            w /= 2;
            h /= 2;
            x += w / 2;
            y += h / 2;
        }

        window = new(new((uint)w, (uint)h), title, style) { Position = new(x, y) };

        window.Closed += (_, _) => Close();
        window.Resized += (_, _) => Resize();
        window.LostFocus += (_, _) =>
        {
            Mouse.CancelInput();
            Keyboard.CancelInput();
        };
        window.KeyPressed += Keyboard.OnKeyPress;
        window.KeyReleased += Keyboard.OnKeyRelease;
        window.MouseButtonPressed += Mouse.OnButtonPressed;
        window.MouseButtonReleased += Mouse.OnButtonReleased;
        window.MouseWheelScrolled += Mouse.OnWheelScrolled;
        window.MouseMoved += Mouse.OnMove;

        window.DispatchEvents();
        window.Clear();
        window.Display();
        window.SetVerticalSyncEnabled(true);

        Resize();
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
            return;
        }

        Mouse.Update();
        FinishDraw();
        window.Display();
    }
    public static void DrawLayer(Layer layer)
    {
        TryNoWindowException();
        renderTexture.Clear(new(BackgroundColor));

        var tex = Layer.tilesets[layer.TilesetPath];
        var centerX = layer.TilemapPixelSize.w / 2f * layer.Zoom;
        var centerY = layer.TilemapPixelSize.h / 2f * layer.Zoom;
        var r = new RenderStates(BlendMode.Alpha, Transform.Identity, tex, null);

        r.Transform.Translate(layer.Offset.x - centerX, layer.Offset.y - centerY);
        r.Transform.Scale(layer.Zoom, layer.Zoom);

        renderTexture.Draw(layer.verts, r);
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

            StartRetroAnimation();
            return;
        }

        window.Close();
    }

#region Backend
    internal static RenderWindow? window;
    internal static RenderTexture? renderTexture;
    internal static (int w, int h) renderTextureViewSize;

    private static Shader? retroScreen;
    private static readonly Random retroRand = new();
    private static readonly Clock retroScreenTimer = new();
    private static System.Timers.Timer? retroTurnoff;
    private static Clock? retroTurnoffTime;
    private const float RETRO_TURNOFF_TIME = 0.5f;

    private static bool isRetro, isClosing;
    private static string title = "Game";
    private static (int w, int h) size;

    private static void StartRetroAnimation()
    {
        retroTurnoffTime = new();
        retroTurnoff = new(RETRO_TURNOFF_TIME * 1000);
        retroTurnoff.Start();
        retroTurnoff.Elapsed += (_, _) => window?.Close();
    }
    private static void FinishDraw()
    {
        TryNoWindowException();
        renderTexture.Display();

        var sz = window.GetView().Size;
        var (tw, th) = (renderTexture.Size.X, renderTexture.Size.Y);
        var shader = IsRetro ? retroScreen : null;
        var rend = new RenderStates(BlendMode.Alpha, Transform.Identity, renderTexture.Texture, shader);
        var verts = new Vertex[]
        {
            new(new(0, 0), Color.White, new(0, 0)),
            new(new(sz.X, 0), Color.White, new(tw, 0)),
            new(new(sz.X, sz.Y), Color.White, new(tw, th)),
            new(new(0, sz.Y), Color.White, new(0, th)),
        };

        if (IsRetro)
        {
            var randVec = new Vector2f(retroRand.Next(0, 10) / 10f, retroRand.Next(0, 10) / 10f);
            shader?.SetUniform("time", retroScreenTimer.ElapsedTime.AsSeconds());
            shader?.SetUniform("randomVec", randVec);
            shader?.SetUniform("viewSize", new Vector2f(window.Size.X, window.Size.Y));

            if (isClosing && retroTurnoffTime != null)
            {
                var timing = retroTurnoffTime.ElapsedTime.AsSeconds() / RETRO_TURNOFF_TIME;
                shader?.SetUniform("turnoffAnimation", timing);
            }
        }

        window.Draw(verts, PrimitiveType.Quads, rend);
    }

    private static void Resize()
    {
        TryNoWindowException();
        size = ((int)window.Size.X, (int)window.Size.Y);
    }

    [MemberNotNull(nameof(window))]
    [MemberNotNull(nameof(renderTexture))]
    internal static void TryNoWindowException()
    {
        if (window == null || renderTexture == null)
            throw new MemberAccessException(
                $"{nameof(Window)} is not created. Use {nameof(Create)}(...).");
    }
#endregion
}