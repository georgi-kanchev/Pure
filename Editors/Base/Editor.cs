global using System.Diagnostics.CodeAnalysis;
global using Pure.Engine.Tiles;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Utility;
global using Pure.Engine.Window;
global using Pure.Tools.Tiles;
global using Monitor = Pure.Engine.Window.Monitor;
global using Color = Pure.Engine.Utility.Color;

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

    public TileMap MapGrid { get; private set; }
    public List<TileMap> MapsEditor { get; private set; }
    public List<TileMap> MapsUi { get; }

    public List<Block> Ui { get; }
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
            var (w, h) = MapsEditor[0].View.Size;
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
        MapsEditor.ForEach(map => map.View = new(MapsEditor[0].View.Position, (50, 50)));
        MapsUi = [];

        for (var i = 0; i < (int)LayerMapsUi.Count; i++)
            MapsUi.Add(new((width * 5, height * 5)));

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
        LayerMap = new(MapsEditor[0].Size);
        LayerUi = new(MapsUi[0].Size) { Zoom = 3f };
        Input.TilemapSize = MapsUi[0].View.Size;

        for (var i = 0; i < 5; i++)
            CreateViewButton((byte)i);

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

        Input.OnTextCopy(() => Window.Clipboard = Input.Clipboard ?? "");
        while (Window.KeepOpen())
        {
            Time.Update();

            MapsUi.ForEach(map => map.Flush());

            LayerGrid.Size = MapGrid.View.Size;
            LayerMap.Size = MapsEditor[0].View.Size;
            LayerUi.Size = MapsUi[0].View.Size;

            Input.Update(
                Mouse.ButtonIdsPressed,
                Mouse.ScrollDelta,
                Keyboard.KeyIdsPressed,
                Keyboard.KeyTyped,
                Window.Clipboard);

            prevRaw = MousePositionRaw;
            MousePositionRaw = Mouse.CursorPosition;

            //========

            MapGrid.View = MapsEditor[0].View;
            var gridView = MapGrid.UpdateView();
            LayerGrid.PixelOffset = ViewPosition;
            LayerGrid.Zoom = ViewZoom;
            LayerGrid.DrawTileMap(gridView.ToBundle());

            //========

            var (x, y) = LayerMap.PixelToPosition(Mouse.CursorPosition);
            var (vw, vy) = MapsEditor[0].View.Position;
            prevWorld = MousePositionWorld;
            MousePositionWorld = (x + vw, y + vy);
            Input.PositionPrevious = prevWorld;
            Input.Position = MousePositionWorld;
            Input.TilemapSize = MapsEditor[0].View.Size;

            TryViewInteract();
            OnUpdateEditor?.Invoke();

            //========
            var views = MapsEditor.ForEachGet(map => map.UpdateView());
            for (var i = 0; i < views.Length; i++)
            {
                if (i < MapsEditorVisible.Count && MapsEditorVisible[i] == false)
                    continue;

                LayerMap.DrawTileMap(views[i].ToBundle());
            }

            //========

            prevUi = MousePositionUi;
            MousePositionUi = LayerUi.PixelToPosition(Mouse.CursorPosition);
            Input.Position = MousePositionUi;
            Input.PositionPrevious = prevUi;
            Input.TilemapSize = MapsUi[0].View.Size;

            UpdateHud();
            Ui.ForEach(block => block.Update());
            OnUpdateUi?.Invoke();

            if (Prompt.IsHidden == false)
                Prompt.Update();

            Mouse.CursorCurrent = (Mouse.Cursor)Input.CursorResult;

            MapsUi.ForEach(map => LayerUi.DrawTileMap(map));
            LayerUi.DrawMouseCursor();

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
        MapsEditor = [];

        for (var i = 0; i < (int)LayerMapsEditor.Count; i++)
            MapsEditor.Add(new(newMapSize));
    }
    public void Log(string text)
    {
        // TODO make editor log be hover info

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
    public void PromptTileset(Action<Layer, TileMap>? onSuccess, Action? onFail)
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
        var size = MapsEditor[0].Size;
        var color = Color.Gray.ToDark(0.6f);
        var (x, y) = (0, 0);

        MapGrid.Fill(new Tile(LayerMap.AtlasTileIdFull, Color.Gray.ToDark(0.7f)));

        for (var i = 0; i < size.width + GRID_GAP; i += GRID_GAP)
        {
            var newX = x - (x + i) % GRID_GAP;
            MapGrid.SetLine((newX + i, y), (newX + i, y + size.height), new Tile(LayerMap.AtlasTileIdFull, color));
        }

        for (var i = 0; i < size.height + GRID_GAP; i += GRID_GAP)
        {
            var newY = y - (y + i) % GRID_GAP;
            MapGrid.SetLine((x, newY + i), (x + size.width, newY + i), new Tile(LayerMap.AtlasTileIdFull, color));
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
            var resultLayers = default(string[]);

            try
            {
                var gen = default(MapGenerator);
                var maps = new List<TileMap>();
                if (Path.GetExtension(file) == ".tmx" && file != null)
                {
                    var result = TiledLoader.Load(file).ToList();
                    var layers = new List<string>();
                    foreach (var item in result)
                    {
                        maps.Add(item.map);
                        layers.Add(item.layerName);
                    }

                    resultLayers = layers.ToArray();
                }
                else
                {
                    var bytes = File.ReadAllBytes($"{file}");
                    maps = LoadMap(bytes, ref resultLayers, ref gen);
                }

                MapsEditor.Clear();
                MapsEditor.AddRange(maps);

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
            var bytes = Convert.FromBase64String(PromptInput.Value ?? "");
            var layers = Array.Empty<string>();
            var gen = default(MapGenerator);
            var maps = LoadMap(bytes, ref layers, ref gen);
            MapsEditor.Clear();
            MapsEditor.AddRange(maps);

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

        Ui.AddRange([btnMapSize, btnViewSize]);
    }
    private void ResizePressMap(int i)
    {
        var text = promptSize.Value?.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        if (i != 0 || text?.Length != 2)
            return;

        var (w, h) = ((int)text[0].ToNumber(), (int)text[1].ToNumber());

        ChangeMapSize((w, h));

        var (vw, vh) = MapsEditor[0].View.Size;
        var packCopy = MapsEditor.ToBytes().ToObject<List<TileMap>>();
        if (packCopy == null)
            return;

        for (var j = 0; j < packCopy.Count; j++)
            MapsEditor[j].SetTiles((0, 0), packCopy[j]!);

        MapsEditor.ForEach(map => map.View = new(MapsEditor[0].View.Position, (vw, vh)));

        SetGrid();
        ViewMove(); // reclamp view position
    }
    private void ResizePressView(int i)
    {
        var text = promptSize.Value?.Split(" ", StringSplitOptions.RemoveEmptyEntries);

        if (i != 0 || text?.Length != 2)
            return;

        var (w, h) = ((int)text[0].ToNumber(), (int)text[1].ToNumber());
        MapsEditor.ForEach(map => map.View = new(MapsEditor[0].View.Position, (w, h)));
        ViewMove(); // reclamp view position
        Log($"View {w}x{h}");
    }
    private void CreateViewButton(byte rotations)
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
            var arrow = new Tile(Tile.ARROW_TAILLESS_ROUND, color, (Pose)rotations);
            var center = new Tile(Tile.SHAPE_CIRCLE, color);
            MapsUi[(int)LayerMapsUi.Front].SetTile(btn.Position, rotations == 4 ? center : arrow);
        };

        Ui.Add(btn);

        void Trigger()
        {
            var (x, y) = MapsEditor[0].View.Position;
            MapsEditor.ForEach(map => map.View = new((x + offX * 5, y + offY * 5), MapsEditor[0].View.Size));
            ;

            if (offX == 0 && offY == 0)
            {
                MapsEditor.ForEach(map => map.View = new(default, MapsEditor[0].View.Size));
                Log("View Reset");
            }

            if (x != MapsEditor[0].View.X || y != MapsEditor[0].View.Y)
                Log($"View {MapsEditor[0].View.X}, {MapsEditor[0].View.Y}");
        }
    }

    private void UpdateHud()
    {
        infoTextTimer -= Time.Delta;

        const int TEXT_WIDTH = 40;
        const int TEXT_HEIGHT = 4;
        var x = MapsUi[0].Size.width / 2 - TEXT_WIDTH / 2;
        var bottomY = MapsUi[0].Size.height - 1;
        var (mx, my) = MousePositionWorld;
        var (mw, mh) = MapsEditor[0].Size;
        var (vw, vh) = MapsEditor[0].View.Size;
        const int FRONT = (int)LayerMapsUi.Front;

        if (Time.UpdateCount % 60 == 0)
            fps = (int)Time.UpdatesPerSecond;

        MapPanel.Position = (0, bottomY - 2);
        MapPanel.Size = (22, 3);
        MapPanel.Update();

        var gray = Color.Gray.ToDark();
        MapsUi[(int)LayerMapsUi.Back].SetBox(
            MapPanel.Area, new(Tile.FULL, gray), new(Tile.BOX_CORNER, gray), new(Tile.FULL, gray));

        MapsUi[FRONT].SetText((0, 0), $"FPS:{fps}");
        MapsUi[FRONT].SetText((11, bottomY - 2), $"{mw} x {mh}");
        MapsUi[FRONT].SetText((12, bottomY), $"{vw} x {vh}");

        MapsUi[FRONT].SetText((x, bottomY),
            $"Cursor {(int)mx}, {(int)my}".Constrain((TEXT_WIDTH, 1), alignment: Alignment.Center));

        if (infoTextTimer <= 0)
        {
            infoText = string.Empty;
            return;
        }

        var text = infoText.Constrain((TEXT_WIDTH, TEXT_HEIGHT), alignment: Alignment.Top, scrollProgress: 1f);
        MapsUi[FRONT].SetText((x, 0), text);
    }

    private void TryViewInteract()
    {
        if (IsDisabledViewInteraction || Prompt.IsHidden == false)
            return;

        var prevZoom = ViewZoom;

        TryViewZoom();
        TryViewMove();

        LayerMap.PixelOffset = ViewPosition;
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

    private static List<TileMap> LoadMap(byte[] bytes, ref string[] layerNames, ref MapGenerator? gen)
    {
        // if the file was exported with this editor, there should be some extra editor data at the end
        // however, if it was exported purely by tilemapPack.ToBytes().Compress(),
        // it should still be possible to decompress it directly, so this tries both cases

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

            gen = generatorBytes.ToObject<MapGenerator>();
            layerNames = layerBytes.ToObject<string[]>()!;
            var maps = mapsBytes.ToObject<List<TileMap>>()!;
            return maps;
        }
        catch (Exception)
        {
            var maps = bytes.Decompress().ToObject<List<TileMap>>();

            if (maps == null)
                throw new();

            return maps;
        }
    }
#endregion
}