global using Pure.Engine.Utilities;
global using Pure.Engine.Tilemap;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Window;
global using static Pure.Default.RendererUserInterface.Default;

namespace Pure.Editors.EditorBase;

public class Editor
{
    public enum LayerMaps
    {
        Back,
        Middle,
        Front,
        Count
    }

    public enum LayerUi
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
    }
    public TilemapPack Maps
    {
        get;
    }
    public TilemapPack Ui
    {
        get;
    }

    public (float x, float y) MousePositionWorld
    {
        get;
        private set;
    }
    public (float x, float y) MousePosition
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
            var (cw, ch) = (Maps.View.width * 4f * ViewZoom, Maps.View.height * 4f * ViewZoom);
            viewPosition = (Math.Clamp(value.x, -cw, cw), Math.Clamp(value.y, -ch, ch));
        }
    }

    public Editor(string title, (int width, int height) mapSize)
    {
        Window.Create(PIXEL_SCALE);
        Window.Title = title;

        var (width, height) = Window.MonitorAspectRatio;
        Grid = new(mapSize);
        Maps = new((int)LayerMaps.Count, mapSize) { View = (0, 0, 50, 50) };
        Ui = new((int)LayerUi.Count, (width * 5, height * 5));

        Window.LayerAdd(id: 1);
        Window.LayerAdd(id: 2);
        Window.LayerUpdate(id: 2, zoom: 3f);

        Input.TilemapSize = (Ui.View.width, Ui.View.height);

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
            Ui.Clear();

            Input.Update(
                Mouse.IsButtonPressed(Mouse.Button.Left),
                MousePositionWorld,
                Mouse.ScrollDelta,
                Keyboard.KeyIDsPressed,
                Keyboard.KeyTyped);

            ui.Update();

            TryViewInteract();
            UpdateHud();
            OnUpdate();

            Mouse.CursorGraphics = (Mouse.Cursor)Input.MouseCursorResult;
            
            Window.LayerCurrent = 0;
            Grid.View = Maps.View;
            var gridView = Grid.ViewUpdate();
            Window.LayerUpdate(0, ViewPosition, ViewZoom);
            Window.DrawTiles(gridView.ToBundle());
            
            prevMousePos = MousePosition;
            MousePositionWorld = Mouse.PixelToWorld(Mouse.CursorPosition);
            MousePosition = Mouse.CursorPosition;

            Window.LayerCurrent = 1;
            var view = Maps.ViewUpdate();
            foreach (var t in view)
                Window.DrawTiles(t.ToBundle());

            Window.LayerCurrent = 2;
            for (var i = 0; i < Ui.Count; i++)
                Window.DrawTiles(Ui[i].ToBundle());

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

    protected virtual void OnUpdate()
    {
    }

#region Backend
    private const float PIXEL_SCALE = 1f, ZOOM_MIN = 0.1f, ZOOM_MAX = 20f;
    private const int GRID_GAP = 10;

    private string infoText = "";
    private float infoTextTimer;
    private int fps;
    private (float x, float y) prevMousePos;
    private float viewZoom = 2f;
    private (float x, float y) viewPosition;

    private readonly BlockPack ui = new();

    private void SetGrid()
    {
        var size = Maps.Size;
        var color = Color.Gray.ToDark(0.66f);
        var (x, y) = (0, 0);

        for (var i = 0; i < size.width + GRID_GAP; i += GRID_GAP)
        {
            var newX = x - (x + i) % GRID_GAP;
            Grid.SetLine(
                pointA: (newX + i, y),
                pointB: (newX + i, y + size.height),
                tile: new(Tile.SHADE_OPAQUE, color));
        }

        for (var i = 0; i < size.height + GRID_GAP; i += GRID_GAP)
        {
            var newY = y - (y + i) % GRID_GAP;
            Grid.SetLine(
                pointA: (x, newY + i),
                pointB: (x + size.width, newY + i),
                tile: new(Tile.SHADE_OPAQUE, color));
        }

        for (var i = 0; i < size.height; i += 20)
            for (var j = 0; j < size.width; j += 20)
                Grid.SetTextLine(
                    position: (j + 1, i + 1),
                    text: $"{j}, {i}",
                    color);
    }
    private void UpdateHud()
    {
        infoTextTimer -= Time.Delta;

        const int TEXT_WIDTH = 32;
        const int TEXT_HEIGHT = 2;
        var x = Ui.Size.width / 2 - TEXT_WIDTH / 2;
        var bottomY = Ui.Size.height - 1;
        var (mx, my) = MousePositionWorld;
        var (w, h) = Maps.Size;

        if (Time.UpdateCount % 60 == 0)
            fps = (int)Time.UpdatesPerSecond;

        Ui[(int)LayerUi.Front].SetTextLine((0, 0), $"FPS:{fps}");
        Ui[(int)LayerUi.Front].SetTextLine((0, bottomY), $"MAP VIEW ({w} x {h})");

        Ui[(int)LayerUi.Front].SetTextRectangle(
            position: (x, bottomY),
            size: (TEXT_WIDTH, 1),
            text: $"Cursor {(int)mx}, {(int)my}",
            alignment: Alignment.Center);

        if (infoTextTimer <= 0)
        {
            infoText = "";
            return;
        }

        Ui[(int)LayerUi.Front].SetTextRectangle(
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
        var (mx, my) = MousePosition;
        var (px, py) = prevMousePos;

        ViewPosition = (
            ViewPosition.x + (mx - px) / PIXEL_SCALE * aw,
            ViewPosition.y + (my - py) / PIXEL_SCALE * ah);
    }
    private void TryViewZoom()
    {
        if (Mouse.ScrollDelta == 0 || IsDisabledViewZoom)
            return;

        var mousePos = (Point)MousePosition;
        var viewPos = (Point)ViewPosition;
        var (ww, wh) = Window.Size;
        var pos = (Point)(ww / 2f, wh / 2f);
        var zoomInDist = mousePos.Distance(pos);
        var zoomInDir = (Direction)mousePos.Direction(pos);
        var zoomInPos = viewPos.MoveIn(zoomInDir, zoomInDist / 5f * ViewZoom);
        var zoomOutDist = viewPos.Distance(new());
        var zoomOutPos = viewPos.MoveTo((0, 0), (30f + zoomOutDist / 50f) * ViewZoom);

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
            var color = GetInteractionColor(btn, Color.Gray);
            var arrow = new Tile(Tile.ARROW_NO_TAIL, color, (sbyte)rotations);
            var center = new Tile(Tile.SHAPE_CIRCLE, color);
            Ui[(int)LayerUi.Back].SetTile(
                btn.Position,
                tile: rotations == 4 ? center : arrow);
        });

        ui.Add(btn);

        void Trigger()
        {
            var (x, y, w, h) = Maps.View;
            Maps.View = (x + offX * 10, y + offY * 10, w, h);

            if (offX == 0 && offY == 0)
            {
                Maps.View = (0, 0, w, h);
                DisplayInfoText("View Reset");
            }

            if (x != Maps.View.x || y != Maps.View.y)
                DisplayInfoText($"View {Maps.View.x}, {Maps.View.y}");
        }
    }
#endregion
}