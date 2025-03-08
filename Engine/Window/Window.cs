﻿global using SFML.Graphics;
global using SFML.System;
global using SFML.Window;
global using System.Diagnostics.CodeAnalysis;
global using System.Diagnostics;
global using SFML.Graphics.Glsl;
global using System.Numerics;
using System.Runtime.InteropServices;

namespace Pure.Engine.Window;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
internal class DoNotSave : Attribute;

/// <summary>
/// Possible window modes.
/// </summary>
public enum Mode { Windowed, Borderless, Fullscreen }

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
    public static (uint width, uint height) Size
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
                retroShader = new EffectWindow().Shader;

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

        allLayers?.Clear(new(BackgroundColor));
        return window.IsOpen;
    }
    public static void Draw(this Layer layer)
    {
        TryCreate();

        layer.DrawQueue();

        var tr = Transform.Identity;
        tr.Translate(layer.PixelOffset.x, layer.PixelOffset.y);
        tr.Scale(layer.Zoom, layer.Zoom, -layer.PixelOffset.x, -layer.PixelOffset.y);

        var (w, h) = (layer.result?.Texture.Size.X ?? 0, layer.result?.Texture.Size.Y ?? 0);
        vertsWindow[0] = new(new(-w / 2f, -h / 2f), Color.White, new(0, 0));
        vertsWindow[1] = new(new(w / 2f, -h / 2f), Color.White, new(w, 0));
        vertsWindow[2] = new(new(w / 2f, h / 2f), Color.White, new(w, h));
        vertsWindow[3] = new(new(-w / 2f, h / 2f), Color.White, new(0, h));

        allLayers?.Draw(vertsWindow, PrimitiveType.Quads, new(BlendMode.Alpha, tr, layer.result?.Texture, null));
        // allLayers?.Draw(vertsWindow, PrimitiveType.Quads, new(BlendMode.Alpha, tr, layer.data?.Texture, null));
    }
    public static void Close()
    {
        if (window == null || allLayers == null)
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

    public static void SetIconFromTile(Layer layer, (int id, uint tint) tile, (int id, uint tint) tileBack = default, bool saveAsFile = false)
    {
        TryCreate();

        const uint SIZE = 64;
        var rend = new RenderTexture(SIZE, SIZE);
        var texture = Layer.atlases[layer.AtlasPath];
        var (bx, by) = IndexToCoords(tileBack.id, layer);
        var (fx, fy) = IndexToCoords(tile.id, layer);
        var tsz = layer.AtlasTileSize;
        tsz += layer.AtlasTileGap;
        tsz += layer.AtlasTileGap;
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

#region Backend
    private const float RETRO_TURNOFF_TIME = 0.5f;

    [DoNotSave]
    internal static RenderWindow? window;
    [DoNotSave]
    private static RenderTexture? allLayers;
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
    internal static readonly Vertex[] vertsWindow = new Vertex[4];

    private static bool isRetro, isClosing, hasClosed, isVerticallySynced = true, isRecreating, shouldGetClipboard;
    private static string title = "Game";
    private static string clipboardCache = "";
    private static uint backgroundColor, monitor, maximumFrameRate = 60;
    private static Mode mode;
    private static float pixelScale = 5f;

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

        if (allLayers == null)
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
    [MemberNotNull(nameof(allLayers))]
    private static void RecreateRenderTextures()
    {
        allLayers?.Dispose();
        allLayers = null;

        var currentMonitor = Engine.Window.Monitor.Monitors[Monitor];
        var (w, h) = currentMonitor.Size;
        allLayers = new((uint)(w / pixelScale), (uint)(h / pixelScale));
        var view = allLayers.GetView();
        view.Center = new();
        rendTexViewSz = ((int)view.Size.X, (int)view.Size.Y);
        allLayers.SetView(view);
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
        if (allLayers == null || hasClosed)
            return;

        TryCreate();
        allLayers.Display();

        var (ww, wh, ow, oh) = GetRenderOffset();
        var (tw, th) = (allLayers.Size.X, allLayers.Size.Y);
        var shader = IsRetro ? retroShader : null;
        var rend = new RenderStates(BlendMode.Alpha, Transform.Identity, allLayers.Texture, shader);
        vertsWindow[0] = new(new(ow, oh), Color.White, new(0, 0));
        vertsWindow[1] = new(new(ww + ow, oh), Color.White, new(tw, 0));
        vertsWindow[2] = new(new(ww + ow, wh + oh), Color.White, new(tw, th));
        vertsWindow[3] = new(new(ow, wh + oh), Color.White, new(0, th));

        if (IsRetro)
        {
            var randVec = new Vector2f(retroRand.Next(0, 10) / 10f, retroRand.Next(0, 10) / 10f);
            shader?.SetUniform("time", time.ElapsedTime.AsSeconds());
            shader?.SetUniform("randomVec", randVec);
            shader?.SetUniform("viewSize", new Vector2f(ww, wh));
            shader?.SetUniform("offScreen", new Vector2f(ow, oh));

            if (isClosing && retroTurnoffTime != null)
            {
                var timing = retroTurnoffTime.ElapsedTime.AsSeconds() / RETRO_TURNOFF_TIME;
                shader?.SetUniform("turnoffAnimation", timing);
            }
        }

        window.Draw(vertsWindow, PrimitiveType.Quads, rend);
    }

    private static (int, int) IndexToCoords(int index, Layer layer)
    {
        var (tw, th) = layer.AtlasTileCount;
        index = index < 0 ? 0 : index;
        index = index > tw * th - 1 ? tw * th - 1 : index;

        return (index % tw, index / tw);
    }

    internal static (float winW, float winH, float offW, float offH) GetRenderOffset()
    {
        TryCreate();

        var (rw, rh) = Engine.Window.Monitor.Current.AspectRatio;
        var ratio = rw / (float)rh;
        var (ww, wh) = (window.Size.X, window.Size.Y);

        if (ww / (float)wh < ratio)
            wh = (uint)(ww / ratio);
        else
            ww = (uint)(wh * ratio);

        return (ww, wh, (window.Size.X - ww) / 2f, (window.Size.Y - wh) / 2f);
    }
#endregion
}