global using SFML.Graphics;
global using SFML.System;
global using SFML.Window;
global using System.Diagnostics.CodeAnalysis;

namespace Pure.Engine.Window;

/// <summary>
/// Possible window modes.
/// </summary>
[Flags]
public enum Mode
{
    None = 0,
    Titlebar = 1 << 0,
    Resize = 1 << 1,
    Close = 1 << 2,
    Fullscreen = 1 << 3,
    Default = Titlebar | Resize | Close
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
        get => mode;
        set
        {
            if (mode != value && window != null)
                isRecreating = true;

            mode = value;

            TryCreate();
        }
    }
    /// <summary>
    /// Gets or sets the title of the window.
    /// </summary>
    public static string Title
    {
        get => title;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                value = "Game";

            title = value;
            TryCreate();
            window.SetTitle(title);
        }
    }
    /// <summary>
    /// Gets the size of the window.
    /// </summary>
    public static (uint width, uint height) Size
    {
        get
        {
            var ratio = Engine.Window.Monitor.Current.AspectRatio;
            var (w, h) = (ratio.width * scale, ratio.height * scale);
            return ((uint)w, (uint)h);
        }
    }
    public static float Scale
    {
        get => scale;
        set
        {
            scale = value;
            var ratio = Engine.Window.Monitor.Current.AspectRatio;
            var size = (ratio.width * value, ratio.height * value);
        }
    }
    /// <summary>
    /// Gets a value indicating whether the window is focused.
    /// </summary>
    public static bool IsFocused
    {
        get => window != null && window.HasFocus();
    }
    /// <summary>
    /// Gets or sets a value indicating whether the window should use retro TV graphics.
    /// </summary>
    public static bool IsRetro
    {
        get => isRetro;
        set
        {
            if (value && retroScreen == null && Shader.IsAvailable)
                retroScreen = RetroShader.Create();

            isRetro = value;
            TryCreate();
        }
    }
    public static uint BackgroundColor
    {
        get => backgroundColor;
        set
        {
            backgroundColor = value;
            TryCreate();
        }
    }
    public static uint Monitor
    {
        get => monitor;
        set
        {
            monitor = (uint)Math.Min(value, Engine.Window.Monitor.Monitors.Length - 1);
            TryCreate();
            Center();
        }
    }
    public static float PixelScale
    {
        get => pixelScale;
        set
        {
            pixelScale = value;
            TryCreate();
            RecreateRenderTexture();
        }
    }
    public static bool IsVerticallySynced
    {
        get => isVerticallySynced;
        set
        {
            isVerticallySynced = value;
            TryCreate();
            window.SetVerticalSyncEnabled(value);
        }
    }
    public static uint MaximumFrameRate
    {
        get => maximumFrameRate;
        set
        {
            maximumFrameRate = value;
            TryCreate();
            window.SetFramerateLimit(value);
        }
    }
    public static string? Clipboard
    {
        get => SFML.Window.Clipboard.Contents;
        set => SFML.Window.Clipboard.Contents = value;
    }

    public static bool KeepOpen()
    {
        if (isRecreating)
            Recreate();

        TryCreate();

        Mouse.Update();
        FinishDraw();

        window.Display();
        window.DispatchEvents();
        window.Clear();
        window.SetActive();

        renderTexture?.Clear(new(BackgroundColor));
        return window.IsOpen;
    }
    public static void Draw(this Layer layer)
    {
        TryCreate();

        var tex = Layer.tilesets[layer.TilesetPath];
        var centerX = layer.TilemapPixelSize.w / 2f * layer.Zoom;
        var centerY = layer.TilemapPixelSize.h / 2f * layer.Zoom;
        var r = new RenderStates(BlendMode.Alpha, Transform.Identity, tex, null);

        r.Transform.Translate(layer.Offset.x - centerX, layer.Offset.y - centerY);
        r.Transform.Scale(layer.Zoom, layer.Zoom);

        renderTexture?.Draw(layer.verts, r);
        layer.verts.Clear();
    }
    /// <summary>
    /// Closes the window.
    /// </summary>
    public static void Close()
    {
        if (window == null || renderTexture == null)
            return;

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

    private static bool isRetro, isClosing, isVerticallySynced, isRecreating;
    private static string title = "Game";
    private static (uint w, uint h) size;
    private static uint backgroundColor, monitor;
    private static Mode mode;
    private static float pixelScale = 5f;
    private static uint maximumFrameRate;
    private static float scale;

    [MemberNotNull(nameof(window))]
    private static void TryCreate()
    {
        if (window != null)
            return;

        if (renderTexture == null)
            RecreateRenderTexture();

        mode = Mode.Default;
        Recreate();
    }
    [MemberNotNull(nameof(window))]
    private static void Recreate()
    {
        isRecreating = false;

        var shouldCenter = true;
        var prevSize = new Vector2u(1280, 720);
        var prevPos = new Vector2i();
        if (window != null)
        {
            shouldCenter = false;
            prevPos = window.Position;
            prevSize = window.Size;
            window.Dispose();
            window = null;
        }

        window = new(new(prevSize.X, prevSize.Y), title, (Styles)mode) { Position = prevPos };
        window.Closed += (_, _) => Close();
        window.Resized += (_, _) =>
        {
        };
        window.KeyPressed += Keyboard.OnPress;
        window.KeyReleased += Keyboard.OnRelease;
        window.MouseButtonPressed += Mouse.OnButtonPressed;
        window.MouseButtonReleased += Mouse.OnButtonReleased;
        window.MouseWheelScrolled += Mouse.OnWheelScrolled;
        window.MouseMoved += Mouse.OnMove;
        window.MouseEntered += Mouse.OnEnter;
        window.MouseLeft += Mouse.OnLeft;
        window.LostFocus += (_, _) =>
        {
            Mouse.CancelInput();
            Keyboard.CancelInput();
        };

        window.DispatchEvents();
        window.Clear();
        window.Display();

        if (shouldCenter)
            Center();

        // set values to the new window
        Title = title;
        IsVerticallySynced = isVerticallySynced;
        MaximumFrameRate = maximumFrameRate;
        Mouse.CursorCurrent = Mouse.CursorCurrent;
        Mouse.IsCursorBounded = Mouse.IsCursorBounded;
        Mouse.IsCursorVisible = Mouse.IsCursorVisible;
        Mouse.TryUpdateSystemCursor();
    }

    [MemberNotNull(nameof(renderTexture))]
    private static void RecreateRenderTexture()
    {
        var currentMonitor = Engine.Window.Monitor.Monitors[Monitor];
        var (w, h) = currentMonitor.Size;
        renderTexture = new((uint)(w / pixelScale), (uint)(h / pixelScale));
        var view = renderTexture.GetView();
        view.Center = new();
        renderTextureViewSize = ((int)view.Size.X, (int)view.Size.Y);
        renderTexture.SetView(view);
    }

    private static void StartRetroAnimation()
    {
        retroTurnoffTime = new();
        retroTurnoff = new(RETRO_TURNOFF_TIME * 1000);
        retroTurnoff.Start();
        retroTurnoff.Elapsed += (_, _) => window?.Close();
    }
    private static void FinishDraw()
    {
        if (renderTexture == null)
            return;

        TryCreate();
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
            new(new(0, sz.Y), Color.White, new(0, th))
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

    private static void Center()
    {
        TryCreate();

        var currentMonitor = Engine.Window.Monitor.Monitors[Monitor];
        var (x, y) = currentMonitor.position;
        var (w, h) = currentMonitor.Size;
        var (ww, wh) = Size;

        x += w / 2 - (int)ww / 2;
        y += h / 2 - (int)wh / 2;

        window.Position = new((int)x, (int)y);
    }
#endregion
}