global using System.Diagnostics.CodeAnalysis;
global using Pure.Engine.Tilemap;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Utilities;
global using Pure.Engine.Window;
global using Pure.Tools.TiledLoader;
global using static Pure.Tools.Tilemapper.TilemapperUserInterface;
global using Monitor = Pure.Engine.Window.Monitor;
global using Color = Pure.Engine.Utilities.Color;
using System.Text;

namespace Pure.Editors.EditorBase;

public class Editor
{
    public enum LayerMapsEditor
    {
        Back,
        Middle,
        Front,
        Count
    }

    public enum LayerMapsUi
    {
        Back,
        Middle,
        Front,
        PromptFade,
        PromptBack,
        PromptMiddle,
        PromptFront,
        Count
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
            var (w, h) = MapsEditor.ViewSize;
            var (cw, ch) = (w * 4f * ViewZoom, h * 4f * ViewZoom);
            viewPosition = (Math.Clamp(value.x, -cw, cw), Math.Clamp(value.y, -ch, ch));
        }
    }

    public Action? OnSetGrid { get; set; }
    public Action? OnUpdateEditor { get; set; }
    public Action? OnUpdateUi { get; set; }
    public Action? OnUpdateLate { get; set; }

    public Action<Button>? OnPromptItemDisplay { get; set; }

    public Editor(string title)
    {
        Window.Create(PIXEL_SCALE);
        Window.Title = title;
        Mouse.IsCursorVisible = false;

        var (width, height) = Monitor.Current.AspectRatio;
        ChangeMapSize((50, 50));
        MapsEditor.ViewSize = (50, 50);
        MapsUi = new((int)LayerMapsUi.Count, (width * 5, height * 5));

        MapPanel = new() { IsResizable = false, IsMovable = false };

        Ui = new();
        Prompt = new();
        Prompt.OnDisplay(() => MapsUi.SetPrompt(Prompt, zOrder: (int)LayerMapsUi.PromptFade));
        OnPromptItemDisplay = item => MapsUi.SetPromptItem(Prompt, item, (int)LayerMapsUi.PromptMiddle);
        Prompt.OnItemDisplay(item => OnPromptItemDisplay?.Invoke(item));
        promptSize = new()
        {
            Value = "",
            Size = (25, 1),
            SymbolGroup = SymbolGroup.Digits | SymbolGroup.Space,
            IsSingleLine = true
        };
        promptSize.OnDisplay(() => MapsUi.SetInputBox(promptSize, (int)LayerMapsUi.PromptBack));

        LayerGrid = new(MapGrid.Size);
        LayerMap = new(MapsEditor.Size);
        LayerUi = new(MapsUi.Size) { Zoom = 3f };
        Input.TilemapSize = MapsUi.ViewSize;

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
        PromptFileViewer.OnDisplay(() => MapsUi.SetFileViewer(PromptFileViewer, BACK));
        PromptFileViewer.FilesAndFolders.OnItemDisplay(btn =>
            MapsUi.SetFileViewerItem(PromptFileViewer, btn, MIDDLE));
        PromptFileViewer.HardDrives.OnItemDisplay(btn =>
            MapsUi.SetFileViewerItem(PromptFileViewer, btn, MIDDLE));
        PromptFileViewer.FilesAndFolders.OnItemInteraction(Interaction.DoubleTrigger, item =>
        {
            if (PromptFileViewer.IsFolder(item) == false)
                Prompt.TriggerButton(0);
        });

        PromptInput = new()
        {
            Size = (30, 1),
            Value = "",
            IsSingleLine = true
        };
        PromptInput.OnDisplay(() => MapsUi.SetInputBox(PromptInput, BACK));

        tilesetPrompt = new(this);
    }

    public void Run()
    {
        SetGrid();

        while (Window.KeepOpen())
        {
            Time.Update();
            MapsUi.Flush();

            LayerGrid.TilemapSize = MapGrid.ViewSize;
            LayerMap.TilemapSize = MapsEditor.ViewSize;
            LayerUi.TilemapSize = MapsUi.ViewSize;

            Input.Update(
                Mouse.Button.Left.IsPressed(),
                Mouse.ScrollDelta,
                Keyboard.KeyIDsPressed,
                Keyboard.KeyTyped);

            prevRaw = MousePositionRaw;
            MousePositionRaw = Mouse.CursorPosition;

            //========

            MapGrid.ViewPosition = MapsEditor.ViewPosition;
            MapGrid.ViewSize = MapsEditor.ViewSize;
            var gridView = MapGrid.ViewUpdate();
            LayerGrid.Offset = ViewPosition;
            LayerGrid.Zoom = ViewZoom;
            LayerGrid.DrawTilemap(gridView.ToBundle());

            //========

            var (x, y) = LayerMap.PixelToWorld(Mouse.CursorPosition);
            var (vw, vy) = MapsEditor.ViewPosition;
            prevWorld = MousePositionWorld;
            MousePositionWorld = (x + vw, y + vy);
            Input.PositionPrevious = prevWorld;
            Input.Position = MousePositionWorld;
            Input.TilemapSize = MapsEditor.ViewSize;

            TryViewInteract();
            OnUpdateEditor?.Invoke();

            //========
            var view = MapsEditor.ViewUpdate();
            foreach (var t in view)
                LayerMap.DrawTilemap(t.ToBundle());

            //========

            prevUi = MousePositionUi;
            MousePositionUi = LayerUi.PixelToWorld(Mouse.CursorPosition);
            Input.Position = MousePositionUi;
            Input.PositionPrevious = prevUi;
            Input.TilemapSize = MapsUi.ViewSize;

            UpdateHud();
            Ui.Update();
            OnUpdateUi?.Invoke();

            if (Prompt.IsHidden == false)
                Prompt.Update();

            Mouse.CursorCurrent = (Mouse.Cursor)Input.CursorResult;

            for (var i = 0; i < MapsUi.Count; i++)
                LayerUi.DrawTilemap(MapsUi[i].ToBundle());

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
        var infoTextSplit = infoText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        if (infoTextSplit.Length > 0 && infoTextSplit[^1].Contains(text))
        {
            var lastLine = infoTextSplit[^1];
            if (lastLine.Contains(')'))
            {
                var split = lastLine.Split("(")[1].Replace(")", "");
                var number = int.Parse(split) + 1;
                text += $" ({number})";
            }
            else
                text += " (2)";
        }

        infoText += text + Environment.NewLine;
        infoTextTimer = 2f;
    }

    public void PromptFileSave(byte[] bytes)
    {
        PromptFileViewer.IsSelectingFolders = true;
        Prompt.Text = "Select a Directory:";
        Prompt.Open(PromptFileViewer, onButtonTrigger: i =>
        {
            if (i != 0)
                return;

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
                    PromptMessage("Failed to save file!");
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
                onLoad?.Invoke(bytes);
            }
            catch (Exception)
            {
                PromptMessage("Failed to load file!");
            }
        });
    }
    public void PromptMessage(string message)
    {
        Prompt.Text = message;
        Prompt.Open(buttonCount: 1);
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

    public void SetGrid()
    {
        var size = MapsEditor.Size;
        var color = Color.Gray.ToDark(0.66f);
        var (x, y) = (0, 0);

        MapGrid.Fill(new Tile(LayerMap.TileIdFull, Color.Brown.ToDark(0.8f)));

        for (var i = 0; i < size.width + GRID_GAP; i += GRID_GAP)
        {
            var newX = x - (x + i) % GRID_GAP;
            MapGrid.SetLine(
                pointA: (newX + i, y),
                pointB: (newX + i, y + size.height),
                tiles: new Tile(LayerMap.TileIdFull, color));
        }

        for (var i = 0; i < size.height + GRID_GAP; i += GRID_GAP)
        {
            var newY = y - (y + i) % GRID_GAP;
            MapGrid.SetLine(
                pointA: (x, newY + i),
                pointB: (x + size.width, newY + i),
                tiles: new Tile(LayerMap.TileIdFull, color));
        }

        OnSetGrid?.Invoke();
    }

    public void PromptLoadMap(Action<string[]> onLoad)
    {
        Prompt.Text = "Select Map File:";
        PromptFileViewer.IsSelectingFolders = false;
        Prompt.Open(PromptFileViewer, onButtonTrigger: btnIndex =>
        {
            if (btnIndex != 0)
                return;

            try
            {
                var selectedPaths = PromptFileViewer.SelectedPaths;
                var file = selectedPaths.Length == 1 ?
                    selectedPaths[0] :
                    PromptFileViewer.CurrentDirectory;
                var maps = default(TilemapPack);
                var result = Array.Empty<string>();

                if (Path.GetExtension(file) == ".tmx" && file != null)
                {
                    var (layers, tilemapPack) = TiledLoader.Load(file);
                    maps = tilemapPack;
                    result = layers;
                }
                else
                {
                    var bytes = File.ReadAllBytes($"{file}");
                    var byteOffset = 0;
                    var layerCount = GrabInt(bytes, ref byteOffset);

                    result = new string[layerCount];
                    for (var i = 0; i < layerCount; i++)
                        result[i] = GrabString(bytes, ref byteOffset);

                    maps = new(bytes[byteOffset..]);
                }

                MapsEditor.Clear();
                for (var i = 0; i < maps.Count; i++)
                    MapsEditor.Add(maps[i]);

                onLoad?.Invoke(result);
            }
            catch (Exception)
            {
                PromptMessage("Could not load map!");
            }
        });
    }

#region Backend
    private const float PIXEL_SCALE = 1f, ZOOM_MIN = 0.1f, ZOOM_MAX = 20f;
    private const int GRID_GAP = 10;
    private readonly InputBox promptSize;
    private readonly TilesetPrompt tilesetPrompt;

    private string infoText = "";
    private float infoTextTimer;
    private int fps = 60;
    private (float x, float y) prevRaw, prevWorld, prevUi;
    private float viewZoom = 2f;
    private (float x, float y) viewPosition;

    private void CreateResizeButtons()
    {
        var btnMapSize = new Button { Text = "Resize Map", Size = (10, 1) };
        var btnViewSize = new Button { Text = "Resize View", Size = (11, 1) };

        btnMapSize.Align((0f, 0.96f));
        btnMapSize.OnInteraction(Interaction.Trigger, () =>
        {
            Prompt.Text = $"Enter Map Size,{Environment.NewLine}" +
                          $"example: '100 100'.";
            Prompt.Open(promptSize, onButtonTrigger: ResizePressMap);
        });
        btnMapSize.OnDisplay(() => MapsUi.SetButton(btnMapSize));

        btnViewSize.Align((0f, 1f));
        btnViewSize.OnInteraction(Interaction.Trigger, () =>
        {
            Prompt.Text = $"Enter View Size,{Environment.NewLine}" +
                          $"example: '100 100'.";
            Prompt.Open(promptSize, onButtonTrigger: ResizePressView);
        });
        btnViewSize.OnDisplay(() => MapsUi.SetButton(btnViewSize));

        Ui.Add(btnMapSize, btnViewSize);
    }
    private void ResizePressMap(int i)
    {
        var text = promptSize.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        if (i != 0 || text.Length != 2)
            return;

        var (vw, vh) = MapsEditor.ViewSize;
        var (w, h) = ((int)text[0].ToNumber(), (int)text[1].ToNumber());
        var packCopy = MapsEditor.Copy();

        ChangeMapSize((w, h));

        for (var j = 0; j < packCopy.Count; j++)
            MapsEditor[j].SetGroup((0, 0), packCopy[j]);
        MapsEditor.ViewSize = (vw, vh);

        SetGrid();
        ViewMove(); // reclamp view position
        Log($"Map {w}x{h}");
    }
    private void ResizePressView(int i)
    {
        var text = promptSize.Value.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        if (i != 0 || text.Length != 2)
            return;

        var (w, h) = ((int)text[0].ToNumber(), (int)text[1].ToNumber());
        MapsEditor.ViewSize = (w, h);
        ViewMove(); // reclamp view position
        Log($"View {w}x{h}");
    }
    private void CreateViewButton(int rotations)
    {
        var offsets = new (int x, int y)[] { (1, 0), (0, 1), (-1, 0), (0, -1), (0, 0) };
        var btn = new Button { Size = (1, 1) };
        var (offX, offY) = offsets[rotations];
        btn.Align((0.26f, 0.98f));
        btn.Position = (btn.Position.x + offX, btn.Position.y + offY);

        if (offX != 0 && offY != 0)
            btn.OnInteraction(Interaction.PressAndHold, Trigger);

        btn.OnInteraction(Interaction.Trigger, Trigger);
        btn.OnInteraction(Interaction.PressAndHold, Trigger);
        btn.OnDisplay(() =>
        {
            var color = btn.GetInteractionColor(Color.Gray.ToBright());
            var arrow = new Tile(Tile.ARROW_TAILLESS_ROUND, color, (sbyte)rotations);
            var center = new Tile(Tile.SHAPE_CIRCLE, color);
            MapsUi[(int)LayerMapsUi.Front].SetTile(btn.Position, tile: rotations == 4 ? center : arrow);
        });

        Ui.Add(btn);

        void Trigger()
        {
            var (x, y) = MapsEditor.ViewPosition;
            MapsEditor.ViewPosition = (x + offX * 5, y + offY * 5);

            if (offX == 0 && offY == 0)
            {
                MapsEditor.ViewPosition = default;
                Log("View Reset");
            }

            if (x != MapsEditor.ViewPosition.x || y != MapsEditor.ViewPosition.y)
                Log($"View {MapsEditor.ViewPosition.x}, {MapsEditor.ViewPosition.y}");
        }
    }

    private void UpdateHud()
    {
        infoTextTimer -= Time.Delta;

        const int TEXT_WIDTH = 32;
        const int TEXT_HEIGHT = 2;
        var x = MapsUi.Size.width / 2 - TEXT_WIDTH / 2;
        var bottomY = MapsUi.Size.height - 1;
        var (mx, my) = MousePositionWorld;
        var (mw, mh) = MapsEditor.Size;
        var (vw, vh) = MapsEditor.ViewSize;
        const int FRONT = (int)LayerMapsUi.Front;

        if (Time.UpdateCount % 60 == 0)
            fps = (int)Time.UpdatesPerSecond;

        MapPanel.Position = (0, bottomY - 2);
        MapPanel.Size = (22, 3);
        MapPanel.Update();

        MapsUi[(int)LayerMapsUi.Back].SetBox(
            position: MapPanel.Position,
            size: MapPanel.Size,
            tileFill: new(Tile.SHADE_OPAQUE, Color.Gray.ToDark()),
            cornerTileId: Tile.BOX_CORNER_ROUND,
            borderTileId: Tile.SHADE_OPAQUE,
            borderTint: Color.Gray.ToDark());

        MapsUi[FRONT].SetTextLine((0, 0), $"FPS:{fps}");
        MapsUi[FRONT].SetTextLine((11, bottomY - 2), $"{mw} x {mh}");
        MapsUi[FRONT].SetTextLine((12, bottomY), $"{vw} x {vh}");

        MapsUi[FRONT].SetTextRectangle(
            position: (x, bottomY),
            size: (TEXT_WIDTH, 1),
            text: $"Cursor {(int)mx}, {(int)my}",
            alignment: Alignment.Center);

        if (infoTextTimer <= 0)
        {
            infoText = "";
            return;
        }

        MapsUi[FRONT].SetTextRectangle(
            position: (x, 0),
            size: (TEXT_WIDTH, TEXT_HEIGHT),
            text: infoText,
            alignment: Alignment.Top,
            scrollProgress: 1f);
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
        if (Mouse.ScrollDelta == 0 || IsDisabledViewZoom)
            return;

        var mousePos = (Point)MousePositionRaw;
        var viewPos = (Point)ViewPosition;
        var (ww, wh) = Window.Size;
        var pos = (Point)(ww / 2f, wh / 2f);
        var zoomInDist = mousePos.Distance(pos);
        var zoomInDir = (Direction)mousePos.Direction(pos);
        var zoomInPos = viewPos.MoveIn(zoomInDir, zoomInDist / 10f * ViewZoom);
        var zoomOutDist = viewPos.Distance(new());
        var zoomOutPos = viewPos.MoveTo((0, 0), (5f + zoomOutDist / 50f) * ViewZoom);

        ViewZoom *= Mouse.ScrollDelta > 0 ? 1.1f : 0.9f;

        if (Math.Abs(ViewZoom - ZOOM_MIN) > 0.01f &&
            Math.Abs(ViewZoom - ZOOM_MAX) > 0.01f)
            ViewPosition = Mouse.ScrollDelta > 0 ? zoomInPos : zoomOutPos;
    }
    private void ViewMove()
    {
        var (mw, mh) = Monitor.Current.Size;
        var (ww, wh) = Window.Size;
        var (aw, ah) = (mw / ww, mh / wh);
        var (mx, my) = MousePositionRaw;
        var (px, py) = prevRaw;

        ViewPosition = (
            ViewPosition.x + (mx - px) / PIXEL_SCALE * aw,
            ViewPosition.y + (my - py) / PIXEL_SCALE * ah);
    }

    private static string GrabString(byte[] fromBytes, ref int byteOffset)
    {
        var textBytesLength = GrabInt(fromBytes, ref byteOffset);
        var bText = GetBytes(fromBytes, textBytesLength, ref byteOffset);
        return Encoding.UTF8.GetString(bText);
    }
    private static int GrabInt(byte[] fromBytes, ref int byteOffset)
    {
        return BitConverter.ToInt32(GetBytes(fromBytes, 4, ref byteOffset));
    }
    private static byte[] GetBytes(byte[] fromBytes, int amount, ref int byteOffset)
    {
        var result = fromBytes[byteOffset..(byteOffset + amount)];
        byteOffset += amount;
        return result;
    }
#endregion
}