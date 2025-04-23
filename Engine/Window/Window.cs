global using SFML.Graphics;
global using SFML.System;
global using SFML.Window;
global using System.Diagnostics.CodeAnalysis;
global using System.Diagnostics;
global using SFML.Graphics.Glsl;
global using System.Numerics;
global using Count = (byte width, byte height);
global using SizeU = (uint width, uint height);
global using SizeI = (int width, int height);
global using SizeF = (float width, float height);
global using VecI = (int x, int y);
global using VecF = (float x, float y);
global using AreaF = (float x, float y, float width, float height);
global using AreaI = (int x, int y, int width, int height);
global using Line = (float ax, float ay, float bx, float by);
global using Tile = (ushort id, uint tint, byte pose);
global using TileStatic = (int id, uint tint);
global using CornersP = (SFML.System.Vector2f p1, SFML.System.Vector2f p2, SFML.System.Vector2f p3, SFML.System.Vector2f p4);
global using CornersS = (SFML.System.Vector2f tl, SFML.System.Vector2f tr, SFML.System.Vector2f br, SFML.System.Vector2f bl);
using System.Runtime.InteropServices;

namespace Pure.Engine.Window;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
internal class DoNotSave : Attribute;

/// <summary>
/// Possible window modes.
/// </summary>
public enum Mode { Windowed, Borderless, Fullscreen }

public enum RenderArea { Fit, Fill }

/// <summary>
/// Provides access to an OS window and its properties.
/// </summary>
public static class Window
{
    public static Action? OnClose { get; set; }
    public static Action? OnRecreate { get; set; }

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
    public static SizeU Size
    {
        get => window != null ? (window.Size.X, window.Size.Y) : (0, 0);
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
        get => isRetro && retroShader != null && Shader.IsAvailable;
        set
        {
            if (value && retroShader == null && Shader.IsAvailable)
                retroShader = Shader.FromString(ShaderCode.VERTEX_DEFAULT, null, ShaderCode.FRAGMENT_WINDOW);

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
            RecreateRenderTextures();
            TryCreate();
            Scale(0.5f);
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
            RecreateRenderTextures();
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
    public static string Clipboard
    {
        get => clipboardCache;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            clipboardCache = value;
            SFML.Window.Clipboard.Contents = value;
        }
    }
    public static RenderArea RenderArea
    {
        get => renderArea;
        set
        {
            TryCreate();
            renderArea = value;
        }
    }

    public static bool KeepOpen()
    {
        if (isRecreating)
            Recreate();

        TryCreate();

        Keyboard.Update();
        Mouse.Update();
        FinishDraw();

        if (hasClosed)
            return false;

        window.Display();
        window.DispatchEvents();
        window.Clear(new(BackgroundColor));
        window.SetActive();

        renderResult?.Clear(new(BackgroundColor));
        return window.IsOpen;
    }
    public static void Render(this LayerTiles layerTiles)
    {
        TryCreate();
        layerTiles.DrawQueue();
    }
    public static void Render(this LayerSprites layerSprites)
    {
        TryCreate();
        layerSprites.DrawQueue();
    }
    public static void Close()
    {
        if (window == null || renderResult == null)
            return;

        if (IsRetro || isClosing)
        {
            isClosing = true;

            StartRetroAnimation();
            return;
        }

        hasClosed = true;
        OnClose?.Invoke();
        window.Close();
    }

    public static void Scale(float scale)
    {
        scale = Math.Max(scale, 0.05f);

        TryCreate();
        var (mw, mh) = Engine.Window.Monitor.Current.Size;
        window.Size = new((uint)(mw * scale), (uint)(mh * scale));
    }
    public static void Center()
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

    public static void SetIconFromTile(LayerTiles layerTiles, TileStatic tile, TileStatic tileBack = default, bool saveAsFile = false)
    {
        TryCreate();

        const uint SIZE = 64;
        var rend = new RenderTexture(SIZE, SIZE);
        var texture = LayerSprites.textures[layerTiles.AtlasPath];
        var (bx, by) = IndexToCoords(tileBack.id, layerTiles);
        var (fx, fy) = IndexToCoords(tile.id, layerTiles);
        var tsz = layerTiles.AtlasTileSize;
        tsz += layerTiles.AtlasTileGap;
        tsz += layerTiles.AtlasTileGap;
        var vertices = new Vertex[]
        {
            new(new(0, 0), new(tileBack.tint), new(tsz * bx, tsz * by)),
            new(new(SIZE, 0), new(tileBack.tint), new(tsz * (bx + 1), tsz * by)),
            new(new(SIZE, SIZE), new(tileBack.tint), new(tsz * (bx + 1), tsz * (by + 1))),
            new(new(0, SIZE), new(tileBack.tint), new(tsz * bx, tsz * (by + 1))),
            new(new(0, 0), new(tile.tint), new(tsz * fx, tsz * fy)),
            new(new(SIZE, 0), new(tile.tint), new(tsz * (fx + 1), tsz * fy)),
            new(new(SIZE, SIZE), new(tile.tint), new(tsz * (fx + 1), tsz * (fy + 1))),
            new(new(0, SIZE), new(tile.tint), new(tsz * fx, tsz * (fy + 1)))
        };
        rend.Draw(vertices, PrimitiveType.Quads, new(texture));
        rend.Display();
        var image = rend.Texture.CopyToImage();
        window.SetIcon(SIZE, SIZE, image.Pixels);

        if (saveAsFile)
            image.SaveToFile("icon.png");

        rend.Dispose();
        image.Dispose();
    }
    public static void SetIconFromTextureArea(LayerSprites layerSprites, AreaI? area = default, bool saveAsFile = false)
    {
        TryCreate();

        const uint SIZE = 64;
        var rend = new RenderTexture(SIZE, SIZE);

        if (LayerSprites.textures.TryGetValue(layerSprites.TexturePath ?? "", out var texture) == false)
            return;

        var (x, y, w, h) = area ?? (0, 0, (int)texture.Size.X, (int)texture.Size.Y);

        var vertices = new Vertex[]
        {
            new(new(0, 0), Color.White, new(x, y)),
            new(new(SIZE, 0), Color.White, new(x + w, y)),
            new(new(SIZE, SIZE), Color.White, new(x + w, y + h)),
            new(new(0, SIZE), Color.White, new(x, y + h))
        };
        rend.Draw(vertices, PrimitiveType.Quads, new(texture));
        rend.Display();
        var image = rend.Texture.CopyToImage();
        window.SetIcon(SIZE, SIZE, image.Pixels);

        if (saveAsFile)
            image.SaveToFile("icon.png");

        rend.Dispose();
        image.Dispose();
    }

#region Backend
    private const float RETRO_TURNOFF_TIME = 0.5f;

    [DoNotSave]
    internal static RenderWindow? window;
    [DoNotSave]
    internal static RenderTexture? renderResult;
    [DoNotSave]
    internal static (int w, int h) rendTexViewSz;

    [DoNotSave]
    private static Shader? retroShader;
    [DoNotSave]
    private static readonly Random retroRand = new();
    [DoNotSave]
    internal static readonly Clock time = new();
    [DoNotSave]
    private static System.Timers.Timer? retroTurnoff;
    [DoNotSave]
    private static Clock? retroTurnoffTime;
    [DoNotSave]
    internal static readonly Vertex[] verts = new Vertex[4];
    [DoNotSave]
    internal static Texture? white;

    private static bool isRetro, isClosing, hasClosed, isVerticallySynced = true, isRecreating, shouldGetClipboard;
    private static string title = "Game", clipboardCache = "";
    private static uint backgroundColor, monitor, maximumFrameRate = 60;
    private static Mode mode;
    private static float pixelScale = 5f;
    private static RenderArea renderArea;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int XInitThreadsDelegate();

    // this below and here ^^^ is a weird issue related to how SFML waits to get the clipboard from X11 on linux
    // so we spin a thread to not freeze the main one
    // this issue only happens if something that is non-text occuppies the clipboard (image for example)
    static Window()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && NativeLibrary.TryLoad("libX11.so.6", out var libX11))
        {
            var funcPtr = NativeLibrary.GetExport(libX11, "XInitThreads");
            var func = Marshal.GetDelegateForFunctionPointer<XInitThreadsDelegate>(funcPtr);
            var result = func.Invoke(); // 1 should be ok, 0 fail
        }

        var thread = new Thread(() =>
        {
            while (hasClosed == false)
            {
                if (shouldGetClipboard)
                {
                    shouldGetClipboard = false;
                    Clipboard = SFML.Window.Clipboard.Contents;
                }

                Thread.Sleep(100);
            }
        });
        thread.Start();
    }

    [MemberNotNull(nameof(window))]
    internal static void TryCreate()
    {
        if (window != null)
            return;

        if (renderResult == null)
            RecreateRenderTextures();

        Recreate();
    }
    [MemberNotNull(nameof(window))]
    private static void Recreate()
    {
        var wasRecreating = isRecreating;
        isRecreating = false;

        var prevSize = new Vector2u(960, 540);
        var prevPos = new Vector2i();
        if (window != null)
        {
            prevPos = window.Position;
            prevSize = window.Size;
            window.Dispose();
            window = null;
        }

        if (white == null)
        {
            var image = new Image(new[,] { { Color.White } });
            white = new(image) { Repeated = true };
            image.Dispose();
        }

        var style = Styles.Default;
        style = mode == Mode.Fullscreen ? Styles.Fullscreen : style;
        style = mode == Mode.Borderless ? Styles.None : style;

        window = new(new(prevSize.X, prevSize.Y), title, style) { Position = prevPos };
        window.SetKeyRepeatEnabled(false);
        window.Closed += (_, _) => Close();
        window.Resized += (_, e) =>
        {
            var view = window.GetView();
            view.Center = new(e.Width / 2f, e.Height / 2f);
            view.Size = new(e.Width, e.Height);
            window.SetView(view);

            var mousePos = SFML.Window.Mouse.GetPosition(window);
            Mouse.OnMove(null, new(new() { X = mousePos.X, Y = mousePos.Y }));
        };
        window.KeyPressed += Keyboard.OnPress;
        window.KeyReleased += Keyboard.OnRelease;
        window.TextEntered += Keyboard.OnType;
        window.MouseButtonPressed += Mouse.OnButtonPressed;
        window.MouseButtonReleased += Mouse.OnButtonReleased;
        window.MouseWheelScrolled += Mouse.OnWheelScrolled;
        window.MouseMoved += Mouse.OnMove;
        window.MouseEntered += Mouse.OnEnter;
        window.MouseLeft += Mouse.OnLeft;
        window.GainedFocus += (_, _) => shouldGetClipboard = true;
        window.LostFocus += (_, _) =>
        {
            Mouse.CancelInput();
            Keyboard.CancelInput();
        };

        window.DispatchEvents();
        window.Clear();
        window.Display();

        if (wasRecreating == false)
            SetIconFromTile(new(), (394, 16711935)); // green joystick

        // set values to the new window
        Title = title;
        IsVerticallySynced = isVerticallySynced;
        MaximumFrameRate = maximumFrameRate;

        Clipboard = SFML.Window.Clipboard.Contents;

        Mouse.CursorCurrent = Mouse.CursorCurrent;
        Mouse.IsCursorBounded = Mouse.IsCursorBounded;
        Mouse.IsCursorVisible = Mouse.IsCursorVisible;
        Mouse.TryUpdateSystemCursor();

        if (Mode == Mode.Windowed)
            Scale(0.5f);

        if (Mode != Mode.Fullscreen)
            Center();

        OnRecreate?.Invoke();
    }
    [MemberNotNull(nameof(renderResult))]
    private static void RecreateRenderTextures()
    {
        renderResult?.Dispose();
        renderResult = null;

        var currentMonitor = Engine.Window.Monitor.Monitors[Monitor];
        var (w, h) = currentMonitor.Size;
        renderResult = new((uint)(w / pixelScale), (uint)(h / pixelScale));
        var view = renderResult.GetView();
        view.Center = new();
        rendTexViewSz = ((int)view.Size.X, (int)view.Size.Y);
        renderResult.SetView(view);
    }

    private static void StartRetroAnimation()
    {
        retroTurnoffTime = new();
        retroTurnoff = new(RETRO_TURNOFF_TIME * 1000);
        retroTurnoff.Start();
        retroTurnoff.Elapsed += (_, _) =>
        {
            hasClosed = true;
            OnClose?.Invoke();
            window?.Close();
        };
    }
    private static void FinishDraw()
    {
        if (renderResult == null || hasClosed)
            return;

        TryCreate();
        renderResult.Display();

        var (rx, ry, rw, rh) = GetRenderArea();
        var (tw, th) = (renderResult.Size.X, renderResult.Size.Y);
        var shader = IsRetro ? retroShader : null;
        var rend = new RenderStates(BlendMode.Alpha, Transform.Identity, renderResult.Texture, shader);
        verts[0] = new(new(rx, ry), Color.White, new(0, 0));
        verts[1] = new(new(rw + rx, ry), Color.White, new(tw, 0));
        verts[2] = new(new(rw + rx, rh + ry), Color.White, new(tw, th));
        verts[3] = new(new(rx, rh + ry), Color.White, new(0, th));

        if (IsRetro)
        {
            var randVec = new Vector2f(retroRand.Next(0, 10) / 10f, retroRand.Next(0, 10) / 10f);
            shader?.SetUniform("time", time.ElapsedTime.AsSeconds());
            shader?.SetUniform("randomVec", randVec);
            shader?.SetUniform("viewSize", new Vector2f(rw, rh));
            shader?.SetUniform("offScreen", new Vector2f(rx, ry));

            if (isClosing && retroTurnoffTime != null)
            {
                var timing = retroTurnoffTime.ElapsedTime.AsSeconds() / RETRO_TURNOFF_TIME;
                shader?.SetUniform("turnoffAnimation", timing);
            }
        }

        window.Draw(verts, PrimitiveType.Quads, rend);
    }

    private static (int, int) IndexToCoords(int index, LayerTiles layerTiles)
    {
        var (tw, th) = layerTiles.AtlasTileCount;
        index = index < 0 ? 0 : index;
        index = index > tw * th - 1 ? tw * th - 1 : index;

        return (index % tw, index / tw);
    }

    internal static AreaF GetRenderArea()
    {
        TryCreate();

        var (aw, ah) = Engine.Window.Monitor.Current.AspectRatio;
        var (ww, wh) = (window.Size.X, window.Size.Y);
        var ratio = aw / (float)ah;

        if (RenderArea == RenderArea.Fit)
        {
            wh = ww / (float)wh < ratio ? (uint)(ww / ratio) : wh;
            ww = ww / (float)wh < ratio ? ww : (uint)(wh * ratio);
        }
        else if (RenderArea == RenderArea.Fill)
        {
            var rw = (uint)(ww / ratio);
            var rh = (uint)(wh * ratio);
            wh = rw >= wh ? rw : wh;
            ww = rw >= wh ? ww : rh;
        }

        return (((float)window.Size.X - ww) / 2f, ((float)window.Size.Y - wh) / 2f, ww, wh);
    }
    internal static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
#endregion
}