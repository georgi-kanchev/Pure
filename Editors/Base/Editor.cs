global using System.Diagnostics.CodeAnalysis;
global using Pure.Engine.Tilemap;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Utilities;
global using Pure.Engine.Window;
global using Pure.Tools.Tilemap;
global using Monitor = Pure.Engine.Window.Monitor;
global using Color = Pure.Engine.Utilities.Color;
global using System.Text;

namespace Pure.Editors.Base;

public class Editor
{
    public enum LayerMapsEditor
    {
        Back, Middle, Front, Count
    }

    public enum LayerMapsUi
    {
        Back, Middle, Front, PromptFade, PromptBack, PromptMiddle, PromptFront, Count
    }

    public Layer LayerGrid { get; }
    public Layer LayerMap { get; }
    public Layer LayerUi { get; }

    public Tilemap MapGrid { get; private set; }
    public TilemapPack MapsEditor { get; private set; }
    public TilemapPack MapsUi { get; }

    public BlockPack Ui { get; }
    public Prompt Prompt { get; }
    public InputBox PromptInput { get; }
    public FileViewer PromptFileViewer { get; set; }

    public Panel MapPanel { get; }

    public (float x, float y) MousePositionUi { get; private set; }
    public (float x, float y) MousePositionWorld { get; private set; }
    public (float x, float y) MousePositionRaw { get; private set; }

    public bool IsDisabledViewZoom { get; set; }
    public bool IsDisabledViewMove { get; set; }
    public bool IsDisabledViewInteraction { get; set; }

    public float ViewZoom
    {
        get => viewZoom;
        set => viewZoom = Math.Clamp(value, ZOOM_MIN, ZOOM_MAX);
    }
    public (float x, float y) ViewPosition
    {
        get => viewPosition;
        set
        {
            var (w, h) = MapsEditor.View.Size;
            var (cw, ch) = (w * 4f * ViewZoom, h * 4f * ViewZoom);
            viewPosition = (Math.Clamp(value.x, -cw, cw), Math.Clamp(value.y, -ch, ch));
        }
    }

    public Action? OnSetGrid { get; set; }
    public Action? OnUpdateEditor { get; set; }
    public Action? OnUpdateUi { get; set; }
    public Action? OnUpdateLate { get; set; }

    public Action<Button>? OnPromptItemDisplay { get; set; }

    public List<bool> MapsEditorVisible { get; private set; }

    public Editor(string title)
    {
        Window.PixelScale = PIXEL_SCALE;
        Window.Title = title;
        Mouse.IsCursorVisible = false;

        var (width, height) = Monitor.Current.AspectRatio;
        ChangeMapSize((50, 50));
        MapsEditorVisible = [true, true, true];
        MapsEditor.View = new(MapsEditor.View.Position, (50, 50));
        MapsUi = new((int)LayerMapsUi.Count, (width * 5, height * 5));

        MapPanel = new() { IsResizable = false, IsMovable = false };

        Ui = new();
        Prompt = new();
        Prompt.OnDisplay += () => MapsUi.SetPrompt(Prompt, (int)LayerMapsUi.PromptFade);
        OnPromptItemDisplay = item => MapsUi.SetPromptItem(Prompt, item, (int)LayerMapsUi.PromptMiddle);
        Prompt.OnItemDisplay += item => OnPromptItemDisplay?.Invoke(item);
        promptSize = new()
        {
            Value = string.Empty,
            Size = (25, 1),
            SymbolGroup = SymbolGroup.Decimals | SymbolGroup.Space
        };
        promptSize.OnDisplay += () => MapsUi.SetInputBox(promptSize, (int)LayerMapsUi.PromptBack);

        LayerGrid = new(MapGrid.Size);
        LayerMap = new(MapsEditor.Size);
        LayerUi = new(MapsUi.Size) { Zoom = 3f };
        Input.TilemapSize = MapsUi.View.Size;

        for (var i = 0; i < 5; i++)
            CreateViewButton(i);

        CreateResizeButtons();

        const int BACK = (int)LayerMapsUi.PromptBack;
        const int MIDDLE = (int)LayerMapsUi.PromptMiddle;
        PromptFileViewer = new()
        {
            FilesAndFolders = { IsSingleSelecting = true },
            Size = (21, 16)
        };
        PromptFileViewer.OnDisplay += () => MapsUi.SetFileViewer(PromptFileViewer, BACK);
        PromptFileViewer.FilesAndFolders.OnItemDisplay += btn =>
            MapsUi.SetFileViewerItem(PromptFileViewer, btn, MIDDLE);
        PromptFileViewer.HardDrives.OnItemDisplay += btn =>
            MapsUi.SetFileViewerItem(PromptFileViewer, btn, MIDDLE);
        PromptFileViewer.FilesAndFolders.OnItemInteraction(Interaction.DoubleTrigger, item =>
        {
            if (PromptFileViewer.IsFolder(item) == false)
                Prompt.TriggerButton(0);
        });

        PromptInput = new() { Size = (30, 1), Value = string.Empty };
        PromptInput.OnDisplay += () => MapsUi.SetInputBox(PromptInput, BACK);

        tilesetPrompt = new(this);
    }

    public void Run()
    {
        SetGrid();

        Input.OnTextCopy(() => Window.Clipboard = Input.Clipboard);
        while (Window.KeepOpen())
        {
            Time.Update();

            MapsUi.Flush();

            LayerGrid.Size = MapGrid.View.Size;
            LayerMap.Size = MapsEditor.View.Size;
            LayerUi.Size = MapsUi.View.Size;

            Input.Update(
                Mouse.ButtonIdsPressed,
                Mouse.ScrollDelta,
                Keyboard.KeyIdsPressed,
                Keyboard.KeyTyped,
                Window.Clipboard);

            prevRaw = MousePositionRaw;
            MousePositionRaw = Mouse.CursorPosition;

            //========

            MapGrid.View = MapsEditor.View;
            var gridView = MapGrid.UpdateView();
            LayerGrid.Offset = ViewPosition;
            LayerGrid.Zoom = ViewZoom;
            LayerGrid.DrawTilemap(gridView.ToBundle());

            //========

            var (x, y) = LayerMap.PixelToPosition(Mouse.CursorPosition);
            var (vw, vy) = MapsEditor.View.Position;
            prevWorld = MousePositionWorld;
            MousePositionWorld = (x + vw, y + vy);
            Input.PositionPrevious = prevWorld;
            Input.Position = MousePositionWorld;
            Input.TilemapSize = MapsEditor.View.Size;

            TryViewInteract();
            OnUpdateEditor?.Invoke();

            //========
            var view = MapsEditor.ViewUpdate();
            for (var i = 0; i < view.Length; i++)
            {
                if (i < MapsEditorVisible.Count && MapsEditorVisible[i] == false)
                    continue;

                LayerMap.DrawTilemap(view[i].ToBundle());
            }

            //========

            prevUi = MousePositionUi;
            MousePositionUi = LayerUi.PixelToPosition(Mouse.CursorPosition);
            Input.Position = MousePositionUi;
            Input.PositionPrevious = prevUi;
            Input.TilemapSize = MapsUi.View.Size;

            UpdateHud();
            Ui.Update();
            OnUpdateUi?.Invoke();

            if (Prompt.IsHidden == false)
                Prompt.Update();

            Mouse.CursorCurrent = (Mouse.Cursor)Input.CursorResult;

            foreach (var map in MapsUi.Tilemaps)
                LayerUi.DrawTilemap(map.ToBundle());

            LayerUi.DrawCursor();

            //========

            LayerGrid.Draw();
            LayerMap.Draw();
            LayerUi.Draw();

            OnUpdateLate?.Invoke();
        }
    }

    [MemberNotNull(nameof(MapGrid), nameof(MapsEditor))]
    public void ChangeMapSize((int width, int height) newMapSize)
    {
        MapGrid = new(newMapSize);
        MapsEditor = new((int)LayerMapsEditor.Count, newMapSize);
    }
    public void Log(string text)
    {
        // var infoTextSplit = infoText.Replace("\r", "").Split("\n", StringSplitOptions.RemoveEmptyEntries);
        // if (infoTextSplit.Length > 0 && infoTextSplit[^1].Contains(text))
        // {
        //     var lastLine = infoTextSplit[^1];
        //     if (lastLine.Contains(')'))
        //     {
        //         var split = lastLine.Split("(")[1].Replace(")", string.Empty);
        //         var number = int.Parse(split) + 1;
        //         text += $" ({number})";
        //     }
        //     else
        //         text += " (2)";
        // }
        //
        // infoText += text + "\n";
        // infoTextTimer = 2f;
    }

    public void PromptFileSave(byte[] bytes)
    {
        PromptFileViewer.IsSelectingFolders = true;
        Prompt.Text = "Select a Directory:";
        Prompt.Open(PromptFileViewer, onButtonTrigger: i =>
        {
            if (i != 0)
                return;

            Prompt.Text = "Provide a File Name:";
            PromptInput.SymbolGroup = SymbolGroup.All;
            PromptInput.Value = "name.map";
            Prompt.Open(PromptInput, onButtonTrigger: j =>
            {
                if (j != 0)
                    return;

                try
                {
                    var paths = PromptFileViewer.SelectedPaths;
                    var directory = paths.Length == 1 ? paths[0] : PromptFileViewer.CurrentDirectory;
                    var path = Path.Combine($"{directory}", PromptInput.Value);
                    File.WriteAllBytes(path, bytes);
                }
                catch (Exception)
                {
                    PromptMessage("Saving failed!");
                }
            });
        });
    }
    public void PromptFileLoad(Action<byte[]> onLoad)
    {
        PromptFileViewer.IsSelectingFolders = false;
        Prompt.Text = "Select a File:";
        Prompt.Open(PromptFileViewer, onButtonTrigger: i =>
        {
            if (i != 0)
                return;

            try
            {
                var bytes = File.ReadAllBytes(PromptFileViewer.SelectedPaths[0]);
                onLoad.Invoke(bytes);
            }
            catch (Exception)
            {
                PromptMessage("Loading failed!");
            }
        });
    }
    public void PromptMessage(string message)
    {
        Prompt.Text = message;
        Prompt.Open(btnCount: 1);
    }
    public void PromptYesNo(string message, Action? onAccept)
    {
        Prompt.Text = message;
        Prompt.Open(onButtonTrigger: i =>
        {
            if (i == 0)
                onAccept?.Invoke();
        });
    }
    public void PromptTileset(Action<Layer, Tilemap>? onSuccess, Action? onFail)
    {
        tilesetPrompt.OnSuccess = onSuccess;
        tilesetPrompt.OnFail = onFail;
        tilesetPrompt.Open();
    }
    public void PromptConfirm(Action? onConfirm)
    {
        PromptYesNo($"Any unsaved changes will be lost.\nConfirm?", onConfirm);
    }
    public void PromptBase64(Action? onAccept)
    {
        Prompt.Text = $"Paste Base64";
        Prompt.Open(PromptInput, onButtonTrigger: i =>
        {
            if (i == 0)
                onAccept?.Invoke();
        });
    }

    public void SetGrid()
    {
        var size = MapsEditor.Size;
        var color = Color.Gray.ToDark(0.6f);
        var (x, y) = (0, 0);

        MapGrid.Fill(null, new Tile(LayerMap.AtlasTileIdFull, Color.Gray.ToDark(0.7f)));

        for (var i = 0; i < size.width + GRID_GAP; i += GRID_GAP)
        {
            var newX = x - (x + i) % GRID_GAP;
            MapGrid.SetLine((newX + i, y), (newX + i, y + size.height), null,
                new Tile(LayerMap.AtlasTileIdFull, color));
        }

        for (var i = 0; i < size.height + GRID_GAP; i += GRID_GAP)
        {
            var newY = y - (y + i) % GRID_GAP;
            MapGrid.SetLine((x, newY + i), (x + size.width, newY + i), null,
                new Tile(LayerMap.AtlasTileIdFull, color));
        }

        OnSetGrid?.Invoke();
    }

    public void PromptLoadMap(Action<string[], MapGenerator?> onLoad)
    {
        Prompt.Text = "Select a File:";
        PromptFileViewer.IsSelectingFolders = false;
        Prompt.Open(PromptFileViewer, onButtonTrigger: btnIndex =>
        {
            if (btnIndex != 0)
                return;

            var selectedPaths = PromptFileViewer.SelectedPaths;
            var file = selectedPaths.Length == 1 ? selectedPaths[0] : PromptFileViewer.CurrentDirectory;
            var resultLayers = Array.Empty<string>();

            try
            {
                var gen = default(MapGenerator);
                var maps = default(TilemapPack);
                if (Path.GetExtension(file) == ".tmx" && file != null)
                {
                    var (layers, tilemapPack) = TiledLoader.Load(file);
                    maps = tilemapPack;
                    resultLayers = layers;
                }
                else
                {
                    var bytes = File.ReadAllBytes($"{file}");
                    maps = LoadMap(bytes, ref resultLayers, ref gen);
                }

                MapsEditor.Tilemaps.Clear();
                MapsEditor.Tilemaps.AddRange(maps.Tilemaps);

                onLoad.Invoke(resultLayers, gen);
            }
            catch (Exception)
            {
                PromptMessage("Could not load map!");
            }
        });
    }
    public void PromptLoadMapBase64(Action<string[], MapGenerator?> onLoad)
    {
        PromptBase64(() =>
        {
            var bytes = Convert.FromBase64String(PromptInput.Value);
            var layers = Array.Empty<string>();
            var gen = default(MapGenerator);
            var maps = LoadMap(bytes, ref layers, ref gen);
            MapsEditor.Tilemaps.Clear();
            MapsEditor.Tilemaps.AddRange(maps.Tilemaps);

            onLoad.Invoke(layers, gen);
        });
    }

#region Backend
    private const float PIXEL_SCALE = 1f, ZOOM_MIN = 0.1f, ZOOM_MAX = 20f;
    private const int GRID_GAP = 10;
    private readonly InputBox promptSize;
    private readonly TilesetPrompt tilesetPrompt;

    private string infoText = string.Empty;
    private float infoTextTimer;
    private int fps = 60;
    private (float x, float y) prevRaw, prevWorld, prevUi;
    private float viewZoom = 2f;
    private (float x, float y) viewPosition;

    private void CreateResizeButtons()
    {
        var btnMapSize = new Button { Text = "Resize Map", Size = (10, 1) };
        var btnViewSize = new Button { Text = "Resize View", Size = (11, 1) };

        btnMapSize.AlignInside((0f, 0.96f));
        btnMapSize.OnInteraction(Interaction.Trigger, () =>
        {
            Prompt.Text = $"Enter Map Size,\n" +
                          $"example: '100 100'.";
            Prompt.Open(promptSize, onButtonTrigger: ResizePressMap);
        });
        btnMapSize.OnDisplay += () => MapsUi.SetButton(btnMapSize);

        btnViewSize.AlignInside((0f, 1f));
        btnViewSize.OnInteraction(Interaction.Trigger, () =>
        {
            Prompt.Text = $"Enter View Size,\n" +
                          $"example: '100 100'.";
            Prompt.Open(promptSize, onButtonTrigger: ResizePressView);
        });
        btnViewSize.OnDisplay += () => MapsUi.SetButton(btnViewSize);

        Ui.Blocks.AddRange([btnMapSize, btnViewSize]);
    }
    private void ResizePressMap(int i)
    {
        var text = promptSize.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        if (i != 0 || text.Length != 2)
            return;

        var (w, h) = ((int)text[0].ToNumber(), (int)text[1].ToNumber());

        ChangeMapSize((w, h));

        var (vw, vh) = MapsEditor.View.Size;
        var packCopy = MapsEditor.ToBytes().ToObject<TilemapPack>();
        if (packCopy == null)
            return;

        for (var j = 0; j < packCopy.Tilemaps.Count; j++)
            MapsEditor.Tilemaps[j].SetGroup((0, 0), packCopy.Tilemaps[j]!);
        MapsEditor.View = new(MapsEditor.View.Position, (vw, vh));

        SetGrid();
        ViewMove(); // reclamp view position
    }
    private void ResizePressView(int i)
    {
        var text = promptSize.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        if (i != 0 || text.Length != 2)
            return;

        var (w, h) = ((int)text[0].ToNumber(), (int)text[1].ToNumber());
        MapsEditor.View = new(MapsEditor.View.Position, (w, h));
        ViewMove(); // reclamp view position
        Log($"View {w}x{h}");
    }
    private void CreateViewButton(int rotations)
    {
        var offsets = new (int x, int y)[] { (1, 0), (0, 1), (-1, 0), (0, -1), (0, 0) };
        var btn = new Button { Size = (1, 1) };
        var (offX, offY) = offsets[rotations];
        btn.AlignInside((0.26f, 0.98f));
        btn.Position = (btn.Position.x + offX, btn.Position.y + offY);

        if (offX != 0 && offY != 0)
            btn.OnInteraction(Interaction.PressAndHold, Trigger);

        btn.OnInteraction(Interaction.Trigger, Trigger);
        btn.OnInteraction(Interaction.PressAndHold, Trigger);
        btn.OnDisplay += () =>
        {
            var color = btn.GetInteractionColor(Color.Gray.ToBright());
            var arrow = new Tile(Tile.ARROW_TAILLESS_ROUND, color, (sbyte)rotations);
            var center = new Tile(Tile.SHAPE_CIRCLE, color);
            MapsUi.Tilemaps[(int)LayerMapsUi.Front].SetTile(btn.Position, rotations == 4 ? center : arrow);
        };

        Ui.Blocks.Add(btn);

        void Trigger()
        {
            var (x, y) = MapsEditor.View.Position;
            MapsEditor.View = new((x + offX * 5, y + offY * 5), MapsEditor.View.Size);

            if (offX == 0 && offY == 0)
            {
                MapsEditor.View = new(default, MapsEditor.View.Size);
                Log("View Reset");
            }

            if (x != MapsEditor.View.X || y != MapsEditor.View.Y)
                Log($"View {MapsEditor.View.X}, {MapsEditor.View.Y}");
        }
    }

    private void UpdateHud()
    {
        infoTextTimer -= Time.Delta;

        const int TEXT_WIDTH = 40;
        const int TEXT_HEIGHT = 4;
        var x = MapsUi.Size.width / 2 - TEXT_WIDTH / 2;
        var bottomY = MapsUi.Size.height - 1;
        var (mx, my) = MousePositionWorld;
        var (mw, mh) = MapsEditor.Size;
        var (vw, vh) = MapsEditor.View.Size;
        const int FRONT = (int)LayerMapsUi.Front;

        if (Time.UpdateCount % 60 == 0)
            fps = (int)Time.UpdatesPerSecond;

        MapPanel.Position = (0, bottomY - 2);
        MapPanel.Size = (22, 3);
        MapPanel.Update();

        var gray = Color.Gray.ToDark();
        MapsUi.Tilemaps[(int)LayerMapsUi.Back].SetBox(
            MapPanel.Area, new(Tile.FULL, gray), new(Tile.BOX_CORNER_ROUND, gray), new(Tile.FULL, gray));

        MapsUi.Tilemaps[FRONT].SetText((0, 0), $"FPS:{fps}");
        MapsUi.Tilemaps[FRONT].SetText((11, bottomY - 2), $"{mw} x {mh}");
        MapsUi.Tilemaps[FRONT].SetText((12, bottomY), $"{vw} x {vh}");

        MapsUi.Tilemaps[FRONT].SetText((x, bottomY),
            $"Cursor {(int)mx}, {(int)my}".Constrain((TEXT_WIDTH, 1), alignment: Alignment.Center));

        if (infoTextTimer <= 0)
        {
            infoText = string.Empty;
            return;
        }

        var text = infoText.Constrain((TEXT_WIDTH, TEXT_HEIGHT), alignment: Alignment.Top, scrollProgress: 1f);
        MapsUi.Tilemaps[FRONT].SetText((x, 0), text);
    }

    private void TryViewInteract()
    {
        if (IsDisabledViewInteraction || Prompt.IsHidden == false)
            return;

        var prevZoom = ViewZoom;

        TryViewZoom();
        TryViewMove();

        LayerMap.Offset = ViewPosition;
        LayerMap.Zoom = ViewZoom;

        if (Math.Abs(prevZoom - ViewZoom) > 0.01f)
            Log($"Zoom {ViewZoom * 100:F0}%");
    }
    private void TryViewMove()
    {
        var mmb = Mouse.Button.Middle.IsPressed();

        if (IsDisabledViewMove || mmb == false)
            return;

        ViewMove();
    }
    private void TryViewZoom()
    {
        if (Mouse.ScrollDelta != 0 && IsDisabledViewZoom == false)
            ViewZoom *= Mouse.ScrollDelta > 0 ? 1.05f : 0.95f;
    }
    private void ViewMove()
    {
        var (mx, my) = MousePositionRaw;
        var (px, py) = prevRaw;
        var (aw, ah) = Monitor.Current.AspectRatio;
        var (ww, wh) = Window.Size;

        ViewPosition = (
            ViewPosition.x + (mx - px) * ((float)aw / ww) / LayerMap.Zoom * 122.5f,
            ViewPosition.y + (my - py) * ((float)ah / wh) / LayerMap.Zoom * 122.5f);
    }

    private TilemapPack LoadMap(byte[] bytes, ref string[] layerNames, ref MapGenerator? gen)
    {
        try
        {
            var (a, b) = (bytes.Length - 4, bytes.Length);
            var generatorByteLength = BitConverter.ToInt32(bytes.AsSpan()[a..b]);
            a -= 4;
            b -= 4;
            var layersByteLength = BitConverter.ToInt32(bytes.AsSpan()[a..b]);
            a -= generatorByteLength;
            b -= 4;
            var generatorBytes = bytes[a..b];
            a -= layersByteLength;
            b -= generatorByteLength;
            var layerBytes = bytes[a..b];
            a = 0;
            b -= layersByteLength;
            var mapsBytes = bytes[a..b].Decompress();

            // H4sIAAAAAAAAA+2dZ7BdVRXHbwqgjh8iGcs4jlIEYp/JJ0hQ/OgnhjEvgZAqBqQkwRnsbRQ1ZWwphF5S4AMIIQQbitiVGRVsiN3YsSsW7Prueus4mcMt+567z7pr7/17M4+dP/f37j13r7PW/+x9ztnn4dPmdPZP/94y/dv9ecr079zOzM/h7b1Hdzrz9N8LD2sX9OEX1tr/Tv9020tVn67t+dO/b3hMpzN7ut1+Qvf/zNbfHaKOULVT1FGqtoh6nKoVx3bVu1WdIeo9qpaLeq+qpaLep+pu2YJtqpbIa9tVrRS1Q9UyUTtVfVz+7jJVZ8pru1StEnW5qilRV6i6R/7uSlVnyWtXqTpb1NWqVou6RtVbj+mqa1W9TdR1qi4Vdb2qt4u6QdU7RO1W9U5Re1RtErVX1WZR+6reFXWjqq2iblL1SlE3q7pLvtEtqg6Ker+qO0XdqupV8ne3qXq1qP2qXiPqdlWvFXVA1etE3aHq9aIOqnqOfMKHVD1P1IdVPSjqI6qeKH93l6pH5LWPqjpB1MdUvVDU3arWHjdX4j2jXiLqHlVPFfWJaltEfUrVvjld9WlVHxT1mapfRH1W1QOiPqfqk6I+r+oDor6g6unyCfequlDUl1RdJOrLqtaJuk/VuaLuV3WqfL+vqHqRqK+qWiDqa6qeJerrqlaI+oaqVaIeUPVGUd9U9WZRD6q6ST79W1Vfi/q2qmNEfUfVM0R9V9Vb5F2+p+qAqO+rmiPZ8QNVjxX1Q1XPFvKQqtWifqTqeFE/VvUk+bufqHqmvPZTVbfK/vKzKpry2s+rbyTqF6r2inpI1Y2ifqnqWFG/UnWaqF+rer6o36h6rqjfqjpRPv13qhbJa79XtVjUH1QdJ+qPqk4S9bCqtaL+pGq+vOefVa2R1/6i6k2i/qrqBaIeUbVS1N9UvVjU36vtFPUPVftF/VPVk6V3/6XqZnnt39VWy2v/UdWRWj531ox6/EzVV/U0UUeqOnXGA1SdIWqeqilRT1C1VtTRqtaLmq/qghOlLqm6UNRtqi4Stb/6O1G3q9og6oCqjaLuUHWxqIOqjhB1vqojRV2gav5JXfUuVfcdL36k6n7ZzpWzZ9QXRa1RtVvUMlV7RC1XdVDUOaruFLVO1Rr59CtVrRR1larVoq5WtUrUNapWiLpW1dmirlO1XNT1qpaJukHVWaJ2qzpT1B5VS0XtVTUlap+qtaK2qTpX1HZVLxe1Q9V5onaqWifqMlUvE7VL1StEXa7qHFFXqNolvXTe9D8PXTKrs/6obi71PpLp/sz6/3+goKCgoKCgmlL4LRQUFBQUVPsUfgsFBQUFBdU+hd9CQUFBQUG1T+G3UFBQUFBQ7VP4LRQUFBQUVPsUfpsfdbK21Z1xXqiXBlHrg6hNQVRYf0FBQUFZUPhtfu53ilMKh4dKheKY1XuE0qTw27Zz0t7XFjmlvHo3NSwPKu1jVry7BAq/bTsn7X1tsVPK3rupO94prx6Z9tEox6xeqdL81t5Jvbrf0iDq4iBqq/nW48reqbQ9Mu2jUfvsIIdK89u0ndTe/cJ61SsVs4ZRK7xnWgn56NXhceXS/Nark+KR3qmwOFIFRqM4ZvUeoTDKqyvnnGkp+C1OCtUmRRUYjcJJS6Jw5dL8FieFapPKOb8nV4HJtJIozu/k47ccKUO1SeWc31BQfqiY53fSzNp8/BYnhWpGpZm5UFB5UjnPOqfgt1BQzSivObk5iLLvLyioyVM5zzrjt1ApUl6zLcxJtwRR9v2V80weVCpUzncV4LdQKVJesy3MScPOV3hde4k7MqEmT6W55+C3UKVnSMyRa0wn9br2Eqtl492Tp9KMNn4Lle++73UOOO21l7yulp3zdTZQdSrNOOK3UCnu1WEeGdNJY45cS1gxwqt3M1bOg0ozQvhtfpTXPdF+tBnTSTs92kdTYWMsrx4Z9h3tqZjezSg4DyrNCOG3Piive0/ao82YLmM/csVv26BYeTAPKs2+x2/zi7hXj7QfbcakYo5c8VvvY+Wc7wHNg0qz7/HblGKZ9oxs2jWfkWtJVM73gOZBpdn3+G1KsUx7RtYrFRZtRq5QdcprnbDvCXsqzb4vzW+9Rsl+NYWw/iqB4honqGaU12pi3xP2VJp9j9+mFEsqcBsUM8VQzSivdcK+J+ypNPu+NL/lusOSqJgzxThpSZTXCsCTLrxHaPDWl+a33BNfEsVMMVQzymtu2z/pwivlNUKDt740v7W/Jz7N/cI7xd2wUM0or/kYc33umP1lT6UdocHfsTS/DaNY1dV7tnE3LFSd8ppDjFzziGOMCOG3zSuw/YrsXp9iav/kOEauJVFeKzAj1zziaBch/LZtV47p3V6fYmr/5DhGrnlQaVdg+ydLpU15jbZdhPBbH5T9iDqmw3t9chxOSm2tU15XXwvr1bQpr/uEXYTw25Qor08xxSNLorxWTVZf80553XPsegK/hRpE4ZElUV7rIauv5UGFzZYtCaI2BFHe7lfGb6GgoHzXQ644yoMKmy2bCqI2BlHezp3jt1D5Ul7Ha/Y9EUZ5rYfeqiZUM4rZMvwWKl8Kv82jHnqrmlBQzSj8FipfCr+FgoLyQ+G31Ol8KeIIBQXlh8Jve1Ferxux74m0KfwWCgrKD4Xf9qK8Xjdi3xNpU/gtFBSUHwq/7UV5vW7EvifSppingIKC8kPht1D5UsxTQEFB+aHwW6h8KeYpoKCg/FD4LRQUFBQUVPvUoUtm4blQUFBQUFAGFH4LBQUFBQXVPoXfQkFBQUFBtU/ht1BQUFBQUO1T+C0UFBQUFFT7FH4LBQUFBQXVPoXf5keFrWRoT7F2IhQUVMkUfpuf+4WtZGhP4fBQqVAcs3qPUJoUftt2Ttr7WthKhvaUV++mhuVBpX3MineXQOG3beekva+FrWRoT9l7N3XHO+XVI9M+GuWY1StVmt/aO6lX97NfOdjeu3HlPDLN3iPTPhq1zw5yqDS/TdtJWTd/NCpmDaNWeM+0EvLRq8PjyqX5rVcnxSO9U2FxpAqMRnHM6j1CYZRXV84501LwW5wUqk2KKjAahZOWROHKpfktTgrVJpVzfk+uApNpJVGc38nHbzlShmqTyjm/oaD8UDHP76SZtfn4LU4K1YxKM3OhoPKkcp51TsFvoaCaUV5zcnMQZd9fUFCTp3KedcZvoVKkvGZbmJNuCaLs+yvnmTyoVKic7yrAb6FSpLxmW5iThp2v8Lr2EndkQk2eSnPPwW+hSs+QmCPXmE7qde0lVsvGuydPpRlt/BYq333f6xxw2msveV0tO+frbKDqVJpxxG+hUtyrwzwyppPGHLmWsGKEV+9mrJwHlWaE8Nv8KK97ov1oM6aTdnq0j6bCxlhePTLsO9pTMb2bUXAeVJoRwm99UF73nrRHmzFdxn7kit+2QbHyYB5Umn2P3+YXca8eaT/ajEnFHLnit97HyjnfA5oHlWbf47cpxTLtGdm0az4j15KonO8BzYNKs+/x25RimfaMrFcqLNqMXKHqlNc6Yd8T9lSafV+a33qNkv1qCmH9VQLFNU5QzSiv1cS+J+ypNPsev00pllTgNihmiqGaUV7rhH1P2FNp9n1pfst1hyVRMWeKcdKSKK8VgCddeI/Q4K0vzW+5J74kipliqGaU19y2f9KFV8prhAZvfWl+a39PfJr7hXeKu2GhmlFe8zHm+twx+8ueSjtCg79jaX4bRrGqq/ds425YqDrlNYcYueYRxxgRwm+bV2D7Fdm9PsXU/slxjFxLorxWYEauecTRLkL4bduuHNO7vT7F1P7JcYxc86DSrsD2T5ZKm/IabbsI4bc+KPsRdUyH9/rkOJyU2lqnvK6+FtaraVNe9wm7COG3KVFen2KKR5ZEea2arL7mnfK659j1BH4LNYjCI0uivNZDVl/LgwqbLVsSRG0Iorzdr4zfQkFB+a6HXHGUBxU2WzYVRG0MorydO8dvofKlvI7X7HsijPJaD71VTahmFLNl+C1UvhR+m0c99FY1oaCaUfgtVL4UfgsFBeWHwm+p0/lSxBEKCsoPhd/2orxeN2LfE2lT+C0UFJQfCr/tRXm9bsS+J9Km8FsoKCg/FH7bi/J63Yh9T6RNMU8BBQXlh8JvofKlmKeAgoLyQ+G3UPlSzFNAQUH5ofBbKCgoKCio9qmHTu90ts3qT9JOpiUm/lpi4q8lJv5aYuKvJSb+WquYnDykra4Xrq4I3jTk/XJuY8ekX5+fMqT1FqtJ7kOxYjKs7xcNaYfFqO3YhO5DFjEaNyb179CvzxcPafvFqK2YDItB6L7Txj7UNCb9YjGs76vzgtWZv619uLZiExqD0H2n33aOs72jxiQ0Fv36PvRz6n0wbkyG5XPTfWfYPtRku0NjMiwW48agX1u9/7gxsd6Hxsnv0JhYx6LejhuTtmJRb2PEJjQmk4pF1Y4bE+vtHqf2hsak7f1rWDvp8UrTtl57Q77HqDGxjkXVphqTqh2lhnmf7xr2HTYHvs+k21FqmNeYhMZiy5C/9zJXM8rxY6oxqWKxtcaPOjdiHaOUYjJsW+v5UY9F07mRSc2reY7JqLGo16px50b6xait2HiOSahf1GNRz49x50bqMWo7bzzFpGk+1GNRvV+1X497jF6PkdV89CRiMmoM+uVDvW/75UescVPb5wosYzJuDPrlQ72t50fsmMSejx61n7o/sc5pjVuTQvswtfnoeushJqE1KfRzrOfdUopJaH6E1qRhbSxPj/0923i/cWMy7DNi9VWq522avF/TmFiPsbzFouncZ5sxsRpjea1Z/eY+x33f7k/TmPQbY8W+vsS6ZoXmR9NjlJC8G/e4K/a8UX1O0eo4a9Rjltjvf/j7jhuT0Gvm+uVRvzl2q/xoOh8d6/17vW+scfyw65365VG/Ofa286PpHGjsz+n1vm3Nd4VeN9j02rem2zXqGHfc8VWTsULb88LDzmN48YtY8w2jfm6vv5vU+RMvx1Gx5xvqbVUPqvWBqxWAB41vJn2ese12WJ809e7QtqoH1RqS1SqRg/zJe0xiXZPar0+aendo26Qe5B6TYX1idc3gKG3uMUmxbfv8ybjbSExGb5scVxCTwW2s+a5RjiuIyeA21nxXW+OMtvPQY+vd49vOQ4+t95hM+r6XSbTeY1JiO0//vfCwdkEffmGt7a5X/T+7rIBN9j8DAAoAAAAGAAAATGF5ZXIxOAAAAAQAAAAEAAAABAAAAAAAIEEEAAAAAAAAABAAAAAEAAAAAAAAAAQAAAAAAAAABAAAAAAAAAAAAAAADgAAADwAAAA=

            gen = generatorBytes.ToObject<MapGenerator>();
            layerNames = layerBytes.ToObject<string[]>()!;
            var maps = mapsBytes.ToObject<TilemapPack>()!;
            return maps;
        }
        catch (Exception)
        {
            var maps = bytes.ToObject<TilemapPack>()!;
            return maps;
        }
    }
#endregion
}