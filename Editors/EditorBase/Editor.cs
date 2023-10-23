global using Pure.Engine.Utilities;
global using Pure.Engine.Tilemap;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Window;
using Pure.Engine.Collision;

namespace Pure.Editors.EditorBase;

public class Editor
{
    public enum UserLayer
    {
        Grid,
        Back,
        Middle,
        Front,
        Count
    }

    public enum UiLayer
    {
        EditBack,
        EditMiddle,
        EditFront,
        PromptFade,
        PromptBack,
        PromptMiddle,
        PromptFront,
        Count
    }

    public TilemapPack UserMaps
    {
        get;
    }
    public TilemapPack UiMaps
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
            var (cw, ch) = (UserMaps.Size.width * 4f * ViewZoom, UserMaps.Size.height * 4f * ViewZoom);
            viewPosition = (Math.Clamp(value.x, -cw, cw), Math.Clamp(value.y, -ch, ch));
        }
    }

    public Editor(string title, int scaleUi, (int width, int height) mapSize)
    {
        Window.Create(PIXEL_SCALE);
        Window.Title = title;

        scaleUi = Math.Clamp(scaleUi, 4, 10);
        var (width, height) = Window.MonitorAspectRatio;
        UserMaps = new((int)UserLayer.Count, mapSize);
        UiMaps = new((int)UiLayer.Count, (width * scaleUi, height * scaleUi));
    }

    public void Run()
    {
        while (Window.IsOpen)
        {
            Window.Activate(true);
            Time.Update();
            UserMaps.Clear();
            UiMaps.Clear();

            Input.TilemapSize = (UiMaps.View.width, UiMaps.View.height);
            Input.Update(
                Mouse.IsButtonPressed(Mouse.Button.Left),
                MousePositionWorld,
                Mouse.ScrollDelta,
                Keyboard.KeyIDsPressed,
                Keyboard.KeyTyped);

            TryViewInteract();
            UpdateInfoText();
            DrawGrid();
            OnUpdate();

            Mouse.CursorGraphics = (Mouse.Cursor)Input.MouseCursorResult;

            prevMousePosWorld = MousePositionWorld;
            prevMousePos = MousePosition;
            MousePositionWorld = Mouse.PixelToWorld(Mouse.CursorPosition);
            MousePosition = Mouse.CursorPosition;

            Window.SetDrawLayer(offset: ViewPosition, zoom: ViewZoom);
            for (var i = 0; i < UserMaps.Count; i++)
                Window.DrawTiles(UserMaps[i].ToBundle());

            Window.SetDrawLayer(offset: (0, 0), zoom: 1f);
            for (var i = 0; i < UiMaps.Count; i++)
                Window.DrawTiles(UiMaps[i].ToBundle());

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
    private const float PIXEL_SCALE = 1f, ZOOM_MIN = 0.3f, ZOOM_MAX = 6f;
    private const int GRID_GAP = 9;

    private string infoText = "";
    private float infoTextTimer;
    private (float x, float y) prevMousePosWorld, prevMousePos;
    private float viewZoom = 1f;
    private (float x, float y) viewPosition;

    private void DrawGrid()
    {
        const int LAYER = (int)UserLayer.Grid;
        var size = (UserMaps.View.width, UserMaps.View.height);
        var color = Color.Gray.ToDark(0.66f);
        var (x, y) = (UserMaps.View.x, UserMaps.View.y);

        for (var i = 0; i < size.width + GRID_GAP; i += GRID_GAP)
        {
            var newX = x - (x + i) % GRID_GAP;
            UserMaps[LAYER].SetLine((newX + i, y), (newX + i, y + size.height),
                new(Tile.SHADE_OPAQUE, color));
        }

        for (var i = 0; i < size.height + GRID_GAP; i += GRID_GAP)
        {
            var newY = y - (y + i) % GRID_GAP;
            UserMaps[LAYER].SetLine((x, newY + i), (x + size.width, newY + i),
                new(Tile.SHADE_OPAQUE, color));
        }

        //for (var i = 0; i < size.height; i += 20)
        //    for (var j = 0; j < size.width; j += 20)
        //    {
        //        Maps[LAYER].SetTile((j, i), new(Tile.SHADE_OPAQUE, color));
        //        Maps[LAYER].SetTextLine((j + 1, i + 1), $"{j}, {i}", color);
        //    }
    }
    private void UpdateInfoText()
    {
        infoTextTimer -= Time.Delta;

        const int TEXT_WIDTH = 32;
        const int TEXT_HEIGHT = 2;
        var x = UiMaps.View.x + UiMaps.View.width / 2 - TEXT_WIDTH / 2;
        var topY = UiMaps.View.y;
        var bottomY = topY + UiMaps.View.height;
        var (mx, my) = MousePositionWorld;

        UiMaps[(int)UiLayer.EditFront]
            .SetTextRectangle((x, bottomY - 1), (TEXT_WIDTH, 1), $"Cursor {(int)mx}, {(int)my}",
                alignment: Alignment.Center);

        if (infoTextTimer <= 0)
        {
            infoText = "";
            return;
        }

        UiMaps[(int)UiLayer.EditFront]
            .SetTextRectangle((x, topY), (TEXT_WIDTH, TEXT_HEIGHT), infoText,
                alignment: Alignment.Top, scrollProgress: 1f);
    }

    private void TryViewInteract()
    {
        if (IsDisabledViewInteraction)
            return;

        var prevZoom = ViewZoom;
        var prevPos = ViewPosition;

        TryViewZoom();
        TryViewMove();

        var (w, h) = ViewPosition;
        if (prevPos != ViewPosition)
            DisplayInfoText($"View Position {w}, {h}");

        if (Math.Abs(prevZoom - ViewZoom) > 0.01f)
            DisplayInfoText($"View Zoom {ViewZoom * 100}%");
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

        if (Math.Abs(ViewZoom - ZOOM_MIN) > 0.01f &&
            Math.Abs(ViewZoom - ZOOM_MAX) > 0.01f)
            ViewPosition = Mouse.ScrollDelta > 0 ? zoomInPos : zoomOutPos;

        ViewZoom *= Mouse.ScrollDelta > 0 ? 1.1f : 0.9f;
    }
#endregion
}