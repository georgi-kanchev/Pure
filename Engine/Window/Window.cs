global using SFML.Graphics;
global using SFML.System;
global using SFML.Window;
global using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Text;

namespace Pure.Engine.Window;

/// <summary>
/// Possible window modes.
/// </summary>
public enum Mode
{
    Windowed, Borderless, Fullscreen
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
            if (mode == Mode.Fullscreen)
                monitor = 0;

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
            var (w, h) = Engine.Window.Monitor.Current.Size;
            return ((uint)(w * Scale), (uint)(h * Scale));
        }
    }
    public static float Scale
    {
        get => scale;
        set
        {
            TryCreate();

            if (mode != Mode.Windowed)
                return;

            SetInternalScale(value);
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
            if (mode == Mode.Fullscreen)
                return;

            monitor = (uint)Math.Min(value, Engine.Window.Monitor.Monitors.Length - 1);
            TryCreate();

            var (w, h) = Engine.Window.Monitor.Current.Size;
            if (mode == Mode.Windowed)
                while (window.Size.X > w || window.Size.Y > h)
                    Scale -= 0.05f;

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

    public static void FromBytes(byte[] bytes)
    {
        var b = Decompress(bytes);
        var offset = 0;

        mode = (Mode)GetBytesFrom(b, 1, ref offset)[0];
        var bTitleLength = BitConverter.ToInt32(GetBytesFrom(b, 4, ref offset));
        title = Encoding.UTF8.GetString(GetBytesFrom(b, bTitleLength, ref offset));
        scale = BitConverter.ToSingle(GetBytesFrom(b, 4, ref offset));
        isRetro = BitConverter.ToBoolean(GetBytesFrom(b, 1, ref offset));
        backgroundColor = BitConverter.ToUInt32(GetBytesFrom(b, 4, ref offset));
        monitor = BitConverter.ToUInt32(GetBytesFrom(b, 4, ref offset));
        pixelScale = BitConverter.ToSingle(GetBytesFrom(b, 4, ref offset));
        isVerticallySynced = BitConverter.ToBoolean(GetBytesFrom(b, 1, ref offset));
        maximumFrameRate = BitConverter.ToUInt32(GetBytesFrom(b, 4, ref offset));
        var x = BitConverter.ToInt32(GetBytesFrom(b, 4, ref offset));
        var y = BitConverter.ToInt32(GetBytesFrom(b, 4, ref offset));
        TryCreate();
        window.Position = new(x, y);
    }
    public static void FromBase64(string base64)
    {
        FromBytes(Convert.FromBase64String(base64));
    }
    public static string ToBase64()
    {
        return Convert.ToBase64String(ToBytes());
    }
    public static byte[] ToBytes()
    {
        TryCreate();

        var result = new List<byte>();
        var bTitle = Encoding.UTF8.GetBytes(Title);
        result.Add((byte)Mode);
        result.AddRange(BitConverter.GetBytes(bTitle.Length));
        result.AddRange(bTitle);
        result.AddRange(BitConverter.GetBytes(Scale));
        result.AddRange(BitConverter.GetBytes(IsRetro));
        result.AddRange(BitConverter.GetBytes(BackgroundColor));
        result.AddRange(BitConverter.GetBytes(Monitor));
        result.AddRange(BitConverter.GetBytes(PixelScale));
        result.AddRange(BitConverter.GetBytes(IsVerticallySynced));
        result.AddRange(BitConverter.GetBytes(MaximumFrameRate));
        result.AddRange(BitConverter.GetBytes(window.Position.X));
        result.AddRange(BitConverter.GetBytes(window.Position.Y));
        return Compress(result.ToArray());
    }

    public static bool KeepOpen()
    {
        if (isRecreating)
            Recreate();

        TryCreate();
        ForceAspectRatio();

        Mouse.Update();
        FinishDraw();

        if (hasClosed)
            return false;

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

        hasClosed = true;
        close?.Invoke();
        window.Close();
    }

    public static void OnClose(Action method)
    {
        close += method;
    }

#region Backend
    internal static RenderWindow? window;
    internal static RenderTexture? renderTexture;
    internal static (int w, int h) renderTextureViewSize;

    private static Action? close;
    private static Shader? retroScreen;
    private static readonly Random retroRand = new();
    private static readonly Clock retroScreenTimer = new();
    private static System.Timers.Timer? retroTurnoff;
    private static Clock? retroTurnoffTime;
    private const float RETRO_TURNOFF_TIME = 0.5f;

    private static bool isRetro, isClosing, hasClosed, isVerticallySynced, isRecreating;
    private static string title = "Game";
    private static uint backgroundColor, monitor;
    private static Mode mode;
    private static float pixelScale = 5f;
    private static uint maximumFrameRate;
    private static float scale = 0.75f;

    [MemberNotNull(nameof(window))]
    private static void TryCreate()
    {
        if (window != null)
            return;

        if (renderTexture == null)
            RecreateRenderTexture();

        Recreate();
    }
    [MemberNotNull(nameof(window))]
    private static void Recreate()
    {
        isRecreating = false;

        var prevSize = new Vector2u(1280, 720);
        var prevPos = new Vector2i();
        if (window != null)
        {
            SetInternalScale(mode == Mode.Windowed ? 0.75f : 1f);

            prevPos = window.Position;
            prevSize = window.Size;
            window.Dispose();
            window = null;
        }

        var style = Styles.Default;
        style = mode == Mode.Fullscreen ? Styles.Fullscreen : style;
        style = mode == Mode.Borderless ? Styles.None : style;

        window = new(new(prevSize.X, prevSize.Y), title, style) { Position = prevPos };
        window.SetKeyRepeatEnabled(false);
        window.Closed += (_, _) => Close();
        window.Resized += (_, e) =>
        {
            if (e.Width < 50 || e.Height < 50)
                Scale = 0.1f;
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

        // set values to the new window
        Scale = scale;
        Title = title;
        IsVerticallySynced = isVerticallySynced;
        MaximumFrameRate = maximumFrameRate;
        Mouse.CursorCurrent = Mouse.CursorCurrent;
        Mouse.IsCursorBounded = Mouse.IsCursorBounded;
        Mouse.IsCursorVisible = Mouse.IsCursorVisible;
        Mouse.TryUpdateSystemCursor();

        Center();
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

    private static void SetInternalScale(float value)
    {
        TryCreate();
        scale = Math.Max(value, 0.1f);
        var monitorSize = Engine.Window.Monitor.Current.Size;
        var (w, h) = (monitorSize.width * value, monitorSize.height * value);
        window.Size = new((uint)w, (uint)h);
    }
    private static void StartRetroAnimation()
    {
        retroTurnoffTime = new();
        retroTurnoff = new(RETRO_TURNOFF_TIME * 1000);
        retroTurnoff.Start();
        retroTurnoff.Elapsed += (_, _) =>
        {
            hasClosed = true;
            close?.Invoke();
            window?.Close();
        };
    }
    private static void FinishDraw()
    {
        if (renderTexture == null || hasClosed)
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

        var current = Engine.Window.Monitor.Current;
        var (x, y) = current.position;
        var (w, h) = current.Size;
        var (ww, wh) = Size;

        x += w / 2 - (int)ww / 2;
        y += h / 2 - (int)wh / 2;

        window.Position = new(x, y);
    }
    private static void ForceAspectRatio()
    {
        TryCreate();

        var (w, h) = ((int)window.Size.X, (int)window.Size.Y);
        var curr = Engine.Window.Monitor.Current;
        var (mw, mh) = curr.Size;
        var (aw, ah) = curr.AspectRatio;
        var (dw, dh) = (w % aw, h % ah);

        if (dw != 0)
        {
            w -= dw;
            Scale = w / (float)mw;
        }

        if (dh != 0)
        {
            h -= dh;
            Scale = h / (float)mh;
        }

        Scale = scale;
    }

    private static byte[] Compress(byte[] data)
    {
        var output = new MemoryStream();
        using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
            stream.Write(data, 0, data.Length);

        return output.ToArray();
    }
    private static byte[] Decompress(byte[] data)
    {
        var input = new MemoryStream(data);
        var output = new MemoryStream();
        using (var stream = new DeflateStream(input, CompressionMode.Decompress))
            stream.CopyTo(output);

        return output.ToArray();
    }
    private static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }
#endregion
}