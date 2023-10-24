global using Pure.Engine.Utilities;
global using Pure.Engine.Tilemap;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Window;
global using static Pure.Default.RendererUserInterface.Default;

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

    public TilemapPack MapsEditor
    {
        get;
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

    public Editor(string title, (int width, int height) mapSize)
    {
        Window.Create(PIXEL_SCALE);
        Window.Title = title;

        var (width, height) = Window.MonitorAspectRatio;
        grid = new(mapSize);
        MapsEditor = new((int)LayerMapsEditor.Count, mapSize) { View = (0, 0, 50, 50) };
        MapsUi = new((int)LayerMapsUi.Count, (width * 5, height * 5));

        Ui = new();
        Prompt = new() { ButtonCount = 2 };
        Prompt.OnDisplay(() => MapsUi.SetPrompt(Prompt, zOrder: (int)LayerMapsUi.PromptFade));
        Prompt.OnItemDisplay(item =>
            MapsUi.SetPromptItem(Prompt, item, zOrder: (int)LayerMapsUi.PromptMiddle));

        Window.LayerAdd(id: 1);
        Window.LayerAdd(id: 2);
        Window.LayerUpdate(id: 2, zoom: 3f);
        Input.TilemapSize = (MapsUi.View.width, MapsUi.View.height);

        for (var i = 0; i < 5; i++)
            CreateViewButton(i);

        SetGrid();
    }

    public void Run()
    {
        while (Window.IsOpen)
        {
            Window.Activate(true);
            Time.Update();
            MapsUi.Clear();

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

            grid.View = MapsEditor.View;
            var gridView = grid.ViewUpdate();
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

    private readonly Tilemap grid;

    private string infoText = "";
    private float infoTextTimer;
    private int fps;
    private (float x, float y) prevRaw, prevWorld, prevUi;
    private float viewZoom = 2f;
    private (float x, float y) viewPosition;

    private void SetGrid()
    {
        var size = MapsEditor.Size;
        var color = Color.Gray.ToDark(0.66f);
        var (x, y) = (0, 0);

        for (var i = 0; i < size.width + GRID_GAP; i += GRID_GAP)
        {
            var newX = x - (x + i) % GRID_GAP;
            grid.SetLine(
                pointA: (newX + i, y),
                pointB: (newX + i, y + size.height),
                tile: new(Tile.SHADE_OPAQUE, color));
        }

        for (var i = 0; i < size.height + GRID_GAP; i += GRID_GAP)
        {
            var newY = y - (y + i) % GRID_GAP;
            grid.SetLine(
                pointA: (x, newY + i),
                pointB: (x + size.width, newY + i),
                tile: new(Tile.SHADE_OPAQUE, color));
        }

        for (var i = 0; i < size.height; i += 20)
            for (var j = 0; j < size.width; j += 20)
                grid.SetTextLine(
                    position: (j + 1, i + 1),
                    text: $"{j}, {i}",
                    color);
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
        MapsUi[(int)LayerMapsUi.Front].SetTextLine((0, bottomY - 1), $"MAP  {mw} x {mh}");
        MapsUi[(int)LayerMapsUi.Front].SetTextLine((0, bottomY), $"VIEW {vw} x {vh}");

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

    private void CreateViewButton(int rotations)
    {
        var offsets = new (int x, int y)[] { (1, 0), (0, 1), (-1, 0), (0, -1), (0, 0) };
        var btn = new Button { Size = (1, 1) };
        var (offX, offY) = offsets[rotations];
        btn.Align((0.052f, 0.95f));
        btn.Position = (btn.Position.x + offX, btn.Position.y + offY);

        btn.OnInteraction(Interaction.Trigger, Trigger);
        btn.OnInteraction(Interaction.PressAndHold, Trigger);
        btn.OnDisplay(() =>
        {
            var (_, _, w, h) = MapsEditor.View;
            btn.IsHidden = MapsEditor.Size == (w, h);
            btn.IsDisabled = btn.IsHidden;

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
#endregion
}