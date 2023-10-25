global using Pure.Engine.Utilities;
global using Pure.Engine.Tilemap;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Window;
global using static Pure.Default.RendererUserInterface.Default;
using System.Diagnostics.CodeAnalysis;

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

    public Tilemap Grid
    {
        get;
        private set;
    }
    public TilemapPack MapsEditor
    {
        get;
        private set;
    }
    public TilemapPack MapsUi
    {
        get;
    }

    public BlockPack Ui
    {
        get;
    }
    public Prompt Prompt
    {
        get;
    }

    public (float x, float y) MousePositionUi
    {
        get;
        private set;
    }
    public (float x, float y) MousePositionWorld
    {
        get;
        private set;
    }
    public (float x, float y) MousePositionRaw
    {
        get;
        private set;
    }

    public bool IsDisabledViewZoom
    {
        get;
        set;
    }
    public bool IsDisabledViewMove
    {
        get;
        set;
    }
    public bool IsDisabledViewInteraction
    {
        get;
        set;
    }

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
            var (cw, ch) = (MapsEditor.View.width * 4f * ViewZoom,
                MapsEditor.View.height * 4f * ViewZoom);
            viewPosition = (Math.Clamp(value.x, -cw, cw), Math.Clamp(value.y, -ch, ch));
        }
    }

    public Action? OnSetGrid
    {
        get;
        set;
    }
    public Action? OnUpdateEditor
    {
        get;
        set;
    }
    public Action? OnUpdateUi
    {
        get;
        set;
    }

    public Editor(string title, (int width, int height) mapSize, (int width, int height) viewSize)
    {
        Window.Create(PIXEL_SCALE);
        Window.Title = title;

        var (width, height) = Window.MonitorAspectRatio;
        ChangeMapSize(mapSize);
        MapsEditor.View = (0, 0, viewSize.width, viewSize.height);
        MapsUi = new((int)LayerMapsUi.Count, (width * 5, height * 5));

        Ui = new();
        Prompt = new() { ButtonCount = 2 };
        Prompt.OnDisplay(() => MapsUi.SetPrompt(Prompt, zOrder: (int)LayerMapsUi.PromptFade));
        Prompt.OnItemDisplay(item => MapsUi.SetPromptItem(Prompt, item, (int)LayerMapsUi.PromptMiddle));
        promptSize = new()
        {
            Value = "",
            Size = (25, 1),
            SymbolGroup = SymbolGroup.Digits | SymbolGroup.Space,
            IsSingleLine = true
        };
        promptSize.OnDisplay(() => MapsUi.SetInputBox(promptSize, (int)LayerMapsUi.PromptBack));

        Window.LayerAdd(id: 1);
        Window.LayerAdd(id: 2);
        Window.LayerUpdate(id: 2, zoom: 3f);
        Input.TilemapSize = (MapsUi.View.width, MapsUi.View.height);

        for (var i = 0; i < 5; i++)
            CreateViewButton(i);

        CreateResizeButtons();
    }

    public void Run()
    {
        SetGrid();

        while (Window.IsOpen)
        {
            Window.Activate(true);
            Time.Update();
            MapsUi.Flush();

            Input.Update(
                Mouse.IsButtonPressed(Mouse.Button.Left),
                Mouse.ScrollDelta,
                Keyboard.KeyIDsPressed,
                Keyboard.KeyTyped);

            prevRaw = MousePositionRaw;
            MousePositionRaw = Mouse.CursorPosition;

            //========

            Window.LayerCurrent = 0;
            var (x, y) = Mouse.PixelToWorld(Mouse.CursorPosition);
            var (wx, wy, ww, wh) = MapsEditor.View;
            prevWorld = MousePositionWorld;
            MousePositionWorld = (x + wx, y + wy);
            Input.PositionPrevious = prevWorld;
            Input.Position = MousePositionWorld;
            Input.TilemapSize = (ww, wh);

            Grid.View = MapsEditor.View;
            var gridView = Grid.ViewUpdate();
            Window.LayerUpdate(0, ViewPosition, ViewZoom);
            Window.DrawTiles(gridView.ToBundle());

            //========

            TryViewInteract();
            OnUpdateEditor?.Invoke();

            //========

            Window.LayerCurrent = 1;
            var view = MapsEditor.ViewUpdate();
            foreach (var t in view)
                Window.DrawTiles(t.ToBundle());

            //========

            Window.LayerCurrent = 2;
            var (_, _, uw, uh) = MapsUi.View;
            prevUi = MousePositionUi;
            MousePositionUi = Mouse.PixelToWorld(Mouse.CursorPosition);
            Input.Position = MousePositionUi;
            Input.PositionPrevious = prevUi;
            Input.TilemapSize = (uw, uh);

            UpdateHud();
            Ui.Update();
            OnUpdateUi?.Invoke();

            if (Prompt.IsHidden == false)
                Prompt.Update();

            for (var i = 0; i < MapsUi.Count; i++)
                Window.DrawTiles(MapsUi[i].ToBundle());

            //========

            Mouse.CursorGraphics = (Mouse.Cursor)Input.CursorResult;
            Window.Activate(false);
        }
    }

    [MemberNotNull(nameof(Grid), nameof(MapsEditor))]
    public void ChangeMapSize((int width, int height) newMapSize)
    {
        Grid = new(newMapSize);
        MapsEditor = new((int)LayerMapsEditor.Count, newMapSize);
    }
    public void DisplayInfoText(string text)
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

#region Backend
    private const float PIXEL_SCALE = 1f, ZOOM_MIN = 0.1f, ZOOM_MAX = 20f;
    private const int GRID_GAP = 10;
    private readonly InputBox promptSize;

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

        btnMapSize.Align((0f, 0.98f));
        btnMapSize.OnInteraction(Interaction.Trigger, () =>
        {
            Prompt.Text = $"Enter the new map size,{Environment.NewLine}" +
                          $"example: '100 100'.";
            Prompt.Open(promptSize, ResizePressMap);
        });
        btnMapSize.OnDisplay(() => MapsUi.SetButton(btnMapSize));

        btnViewSize.Align((0f, 1f));
        btnViewSize.OnInteraction(Interaction.Trigger, () =>
        {
            Prompt.Text = $"Enter the new view size,{Environment.NewLine}" +
                          $"example: '100 100'.";
            Prompt.Open(promptSize, ResizePressView);
        });
        btnViewSize.OnDisplay(() => MapsUi.SetButton(btnViewSize));

        Ui.Add(btnMapSize, btnViewSize);

        Keyboard.OnKeyPress(Keyboard.Key.Enter, asText =>
        {
            if (Prompt.IsHidden)
                return;

            if (Prompt.Text.Contains("view"))
                ResizePressView(0);
            else
                ResizePressMap(0);
        });
    }
    private void ResizePressMap(int i)
    {
        Prompt.Close();
        var text = promptSize.Value.Split();

        if (i != 0 || text.Length != 2)
            return;

        var (_, _, vw, vh) = MapsEditor.View;
        var (w, h) = ((int)text[0].ToNumber(), (int)text[1].ToNumber());
        var packCopy = MapsEditor.Copy();

        ChangeMapSize((w, h));

        for (var j = 0; j < packCopy.Count; j++)
            MapsEditor[j].SetGroup((0, 0), packCopy[j]);
        MapsEditor.View = (0, 0, vw, vh);

        SetGrid();
        DisplayInfoText($"Map {w}x{h}");
    }
    private void ResizePressView(int i)
    {
        Prompt.Close();
        var text = promptSize.Value.Split();

        if (i != 0 || text.Length != 2)
            return;

        var (x, y, _, _) = MapsEditor.View;
        var (w, h) = ((int)text[0].ToNumber(), (int)text[1].ToNumber());
        MapsEditor.View = (x, y, w, h);
        DisplayInfoText($"View {w}x{h}");
    }
    private void CreateViewButton(int rotations)
    {
        var offsets = new (int x, int y)[] { (1, 0), (0, 1), (-1, 0), (0, -1), (0, 0) };
        var btn = new Button { Size = (1, 1) };
        var (offX, offY) = offsets[rotations];
        btn.Align((0.03f, 0.94f));
        btn.Position = (btn.Position.x + offX, btn.Position.y + offY);

        if (offX != 0 && offY != 0)
            btn.OnInteraction(Interaction.PressAndHold, Trigger);

        btn.OnInteraction(Interaction.Trigger, Trigger);
        btn.OnUpdate(() =>
        {
            var (_, _, w, h) = MapsEditor.View;
            btn.IsHidden = MapsEditor.Size == (w, h);
            btn.IsDisabled = btn.IsHidden;
        });
        btn.OnDisplay(() =>
        {
            var color = GetInteractionColor(btn, Color.Gray);
            var arrow = new Tile(Tile.ARROW_NO_TAIL, color, (sbyte)rotations);
            var center = new Tile(Tile.SHAPE_CIRCLE, color);
            MapsUi[(int)LayerMapsUi.Back].SetTile(
                btn.Position,
                tile: rotations == 4 ? center : arrow);
        });

        Ui.Add(btn);

        void Trigger()
        {
            var (x, y, w, h) = MapsEditor.View;
            MapsEditor.View = (x + offX * 10, y + offY * 10, w, h);

            if (offX == 0 && offY == 0)
            {
                MapsEditor.View = (0, 0, w, h);
                DisplayInfoText("View Reset");
            }

            if (x != MapsEditor.View.x || y != MapsEditor.View.y)
                DisplayInfoText($"View {MapsEditor.View.x}, {MapsEditor.View.y}");
        }
    }

    private void SetGrid()
    {
        var size = MapsEditor.Size;
        var color = Color.Gray.ToDark(0.66f);
        var (x, y) = (0, 0);

        Grid.Fill(new(Tile.SHADE_OPAQUE, Color.Brown.ToDark(0.8f)));

        for (var i = 0; i < size.width + GRID_GAP; i += GRID_GAP)
        {
            var newX = x - (x + i) % GRID_GAP;
            Grid.SetLine(
                pointA: (newX + i, y),
                pointB: (newX + i, y + size.height),
                tile: new(Tile.SHADE_2, color));
        }

        for (var i = 0; i < size.height + GRID_GAP; i += GRID_GAP)
        {
            var newY = y - (y + i) % GRID_GAP;
            Grid.SetLine(
                pointA: (x, newY + i),
                pointB: (x + size.width, newY + i),
                tile: new(Tile.SHADE_2, color));
        }

        for (var i = 0; i < size.height; i += 20)
            for (var j = 0; j < size.width; j += 20)
                Grid.SetTextLine(
                    position: (j + 1, i + 1),
                    text: $"{j}, {i}",
                    color);

        OnSetGrid?.Invoke();
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
        var (_, _, vw, vh) = MapsEditor.View;

        if (Time.UpdateCount % 60 == 0)
            fps = (int)Time.UpdatesPerSecond;

        MapsUi[(int)LayerMapsUi.Front].SetTextLine((0, 0), $"FPS:{fps}");
        MapsUi[(int)LayerMapsUi.Front].SetTextLine((11, bottomY - 1), $"{mw} x {mh}");
        MapsUi[(int)LayerMapsUi.Front].SetTextLine((12, bottomY), $"{vw} x {vh}");

        MapsUi[(int)LayerMapsUi.Front].SetTextRectangle(
            position: (x, bottomY),
            size: (TEXT_WIDTH, 1),
            text: $"Cursor {(int)mx}, {(int)my}",
            alignment: Alignment.Center);

        if (infoTextTimer <= 0)
        {
            infoText = "";
            return;
        }

        MapsUi[(int)LayerMapsUi.Front].SetTextRectangle(
            position: (x, 0),
            size: (TEXT_WIDTH, TEXT_HEIGHT),
            text: infoText,
            alignment: Alignment.Top,
            scrollProgress: 1f);
    }

    private void TryViewInteract()
    {
        if (IsDisabledViewInteraction)
            return;

        var prevZoom = ViewZoom;

        TryViewZoom();
        TryViewMove();

        Window.LayerUpdate(1, ViewPosition, ViewZoom);

        if (Math.Abs(prevZoom - ViewZoom) > 0.01f)
            DisplayInfoText($"Zoom {ViewZoom * 100:F0}%");
    }
    private void TryViewMove()
    {
        var mmb = Mouse.IsButtonPressed(Mouse.Button.Middle);

        if (IsDisabledViewMove || mmb == false)
            return;

        var (mw, mh) = Window.MonitorSize;
        var (ww, wh) = Window.Size;
        var (aw, ah) = (mw / ww, mh / wh);
        var (mx, my) = MousePositionRaw;
        var (px, py) = prevRaw;

        ViewPosition = (
            ViewPosition.x + (mx - px) / PIXEL_SCALE * aw,
            ViewPosition.y + (my - py) / PIXEL_SCALE * ah);
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
#endregion
}