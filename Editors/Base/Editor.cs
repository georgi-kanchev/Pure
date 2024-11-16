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

            // 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

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