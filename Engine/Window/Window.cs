global using SFML.Graphics;
global using SFML.System;
global using SFML.Window;
global using System.Diagnostics.CodeAnalysis;
global using System.Diagnostics;
global using System.Text;

namespace Pure.Engine.Window;

/// <summary>
/// Possible window modes.
/// </summary>
public enum Mode { Windowed, Borderless, Fullscreen }

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
    public static string? Clipboard
    {
        get => clipboardCache;
        set
        {
            clipboardCache = value;
            SFML.Window.Clipboard.Contents = value;
        }
    }

    public static void FromBytes(byte[] bytes)
    {
        var b = Decompress(bytes);
        var offset = 0;

        mode = (Mode)GetBytesFrom(b, 1, ref offset)[0];
        var bTitleLength = GetInt();
        title = Encoding.UTF8.GetString(GetBytesFrom(b, bTitleLength, ref offset));
        isRetro = GetBool();
        backgroundColor = GetUInt();
        monitor = GetUInt();
        pixelScale = GetFloat();
        isVerticallySynced = GetBool();
        maximumFrameRate = GetUInt();
        var (x, y, w, h) = (GetInt(), GetInt(), GetUInt(), GetUInt());
        TryCreate();
        window.Position = new(x, y);
        window.Size = new(w, h);

        float GetFloat()
        {
            return BitConverter.ToSingle(GetBytesFrom(b, 4, ref offset));
        }

        int GetInt()
        {
            return BitConverter.ToInt32(GetBytesFrom(b, 4, ref offset));
        }

        uint GetUInt()
        {
            return BitConverter.ToUInt32(GetBytesFrom(b, 4, ref offset));
        }

        bool GetBool()
        {
            return BitConverter.ToBoolean(GetBytesFrom(b, 1, ref offset));
        }
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
        result.AddRange(BitConverter.GetBytes(IsRetro));
        result.AddRange(BitConverter.GetBytes(BackgroundColor));
        result.AddRange(BitConverter.GetBytes(Monitor));
        result.AddRange(BitConverter.GetBytes(PixelScale));
        result.AddRange(BitConverter.GetBytes(IsVerticallySynced));
        result.AddRange(BitConverter.GetBytes(MaximumFrameRate));
        result.AddRange(BitConverter.GetBytes(window.Position.X));
        result.AddRange(BitConverter.GetBytes(window.Position.Y));
        result.AddRange(BitConverter.GetBytes(window.Size.X));
        result.AddRange(BitConverter.GetBytes(window.Size.Y));
        return Compress(result.ToArray());
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
        tr.Translate(layer.Offset.x, layer.Offset.y);
        tr.Scale(layer.Zoom, layer.Zoom, -layer.Offset.x, -layer.Offset.y);

        var (w, h) = (layer.result?.Texture.Size.X ?? 0, layer.result?.Texture.Size.Y ?? 0);
        vertsWindow[0] = new(new(-w / 2f, -h / 2f), Color.White, new(0, 0));
        vertsWindow[1] = new(new(w / 2f, -h / 2f), Color.White, new(w, 0));
        vertsWindow[2] = new(new(w / 2f, h / 2f), Color.White, new(w, h));
        vertsWindow[3] = new(new(-w / 2f, h / 2f), Color.White, new(0, h));

        allLayers?.Draw(vertsWindow, PrimitiveType.Quads, new(BlendMode.Alpha, tr, layer.result?.Texture, null));
        //allLayers?.Draw(vertsWindow, PrimitiveType.Quads, new(BlendMode.Alpha, tr, layer.data?.Texture, null));
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
        close?.Invoke();
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
        var texture = Layer.tilesets[layer.AtlasPath];
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

    public static void OnClose(Action method)
    {
        close += method;
    }
    public static void OnRecreate(Action method)
    {
        recreate += method;
    }

#region Backend
    internal static RenderWindow? window;
    private static RenderTexture? allLayers;
    internal static (int w, int h) rendTexViewSz;

    private static Action? close, recreate;
    private static Shader? retroShader;
    private static readonly Random retroRand = new();
    internal static readonly Clock time = new();
    private static System.Timers.Timer? retroTurnoff;
    private static Clock? retroTurnoffTime;
    private const float RETRO_TURNOFF_TIME = 0.5f;
    internal static readonly Vertex[] vertsWindow = new Vertex[4];

    private static bool isRetro, isClosing, hasClosed, isVerticallySynced, isRecreating;
    private static string title = "Game";
    private static string? clipboardCache;
    private static uint backgroundColor, monitor, maximumFrameRate;
    private static Mode mode;
    private static float pixelScale = 5f;

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
        window.GainedFocus += (_, _) => Clipboard = SFML.Window.Clipboard.Contents;
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

        if (wasRecreating)
            recreate?.Invoke();
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
            close?.Invoke();
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
    internal static byte[] GetBytesFrom(byte[] fromBytes, int amount, ref int offset)
    {
        var result = fromBytes[offset..(offset + amount)];
        offset += amount;
        return result;
    }

    private class Node
    {
        public byte value;
        public int freq;
        public Node? left, right;
    }

    private static Dictionary<byte, string> GetTable(Node root)
    {
        var codeTable = new Dictionary<byte, string>();
        BuildCode(root, "", codeTable);
        return codeTable;

        void BuildCode(Node node, string code, Dictionary<byte, string> table)
        {
            if (node.left == null && node.right == null)
                table[node.value] = code;

            if (node.left != null) BuildCode(node.left, code + "0", table);
            if (node.right != null) BuildCode(node.right, code + "1", table);
        }
    }
    private static Node GetTree(Dictionary<byte, int> frequencies)
    {
        var nodes = new List<Node>(frequencies.Select(f => new Node { value = f.Key, freq = f.Value }));
        while (nodes.Count > 1)
        {
            nodes = nodes.OrderBy(n => n.freq).ToList();
            var left = nodes[0];
            var right = nodes[1];
            var parent = new Node { left = left, right = right, freq = left.freq + right.freq };
            nodes.RemoveRange(0, 2);
            nodes.Add(parent);
        }

        return nodes[0];
    }
    internal static byte[] Compress(byte[] data)
    {
        var compressed = new List<byte>();
        for (var i = 0; i < data.Length; i++)
        {
            var count = (byte)1;
            while (i + 1 < data.Length && data[i] == data[i + 1] && count < 255)
            {
                count++;
                i++;
            }

            compressed.Add(count);
            compressed.Add(data[i]);
        }

        var frequencies = compressed.GroupBy(b => b).ToDictionary(g => g.Key, g => g.Count());
        var root = GetTree(frequencies);
        var codeTable = GetTable(root);
        var header = new List<byte> { (byte)frequencies.Count };
        foreach (var kvp in frequencies)
        {
            header.Add(kvp.Key);
            header.AddRange(BitConverter.GetBytes(kvp.Value));
        }

        var bitString = string.Join("", compressed.Select(b => codeTable[b]));
        var byteList = new List<byte>(header);

        for (var i = 0; i < bitString.Length; i += 8)
        {
            var byteStr = bitString.Substring(i, Math.Min(8, bitString.Length - i));
            byteList.Add(Convert.ToByte(byteStr, 2));
        }

        return byteList.ToArray();
    }
    internal static byte[] Decompress(byte[] compressedData)
    {
        var index = 0;
        var tableSize = (int)compressedData[index++];

        var frequencies = new Dictionary<byte, int>();
        for (var i = 0; i < tableSize; i++)
        {
            var key = compressedData[index++];
            var frequency = BitConverter.ToInt32(compressedData, index);
            index += 4;
            frequencies[key] = frequency;
        }

        var root = GetTree(frequencies);
        var decompressed = new List<byte>();

        var node = root;
        for (var i = index; i < compressedData.Length; i++)
        {
            var bits = Convert.ToString(compressedData[i], 2).PadLeft(8, '0');
            foreach (var bit in bits)
            {
                node = bit == '0' ? node.left : node.right;
                if ((node!.left == null && node.right == null) == false)
                    continue;

                decompressed.Add(node.value);
                node = root;
            }
        }

        var result = new List<byte>();
        for (var i = 0; i < decompressed.Count; i += 2)
            for (var j = 0; j < decompressed[i]; j++)
                result.Add(decompressed[i + 1]);

        return result.ToArray();
    }
#endregion
}