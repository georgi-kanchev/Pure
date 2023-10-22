global using Pure.Engine.Utilities;
global using Pure.Engine.Tilemap;
global using Pure.Engine.UserInterface;
global using Pure.Engine.Window;
global using static Pure.Default.RendererUserInterface.Default;

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

    public Editor(string title, int scaleUi, (int width, int height) mapSize)
    {
        Window.Create();
        Window.Title = title;

        scaleUi = Math.Clamp(scaleUi, 4, 10);
        var (width, height) = Window.MonitorAspectRatio;
        UserMaps = new((int)UserLayer.Count, mapSize);
        UiMaps = new((int)UiLayer.Count, (width * scaleUi, height * scaleUi));

        ClampView();
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
                MousePosition,
                Mouse.ScrollDelta,
                Keyboard.KeyIDsPressed,
                Keyboard.KeyTyped);

            TryViewInteract();
            UpdateInfoText();
            DrawGrid();
            OnUpdate();

            Mouse.CursorGraphics = (Mouse.Cursor)Input.MouseCursorResult;

            prevMousePos = MousePosition;
            MousePosition = UserMaps.PointFrom(Mouse.CursorPosition, Window.Size);

            var maps = UserMaps.ViewUpdate();
            foreach (var t in maps)
                Window.DrawTiles(t.ToBundle());

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
    private const int GRID_GAP = 10;

    private string infoText = "";
    private float infoTextTimer;
    private float zoom = 1f;
    private (float x, float y) prevMousePos;

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
                new(Tile.SHADE_2, color));
        }

        for (var i = 0; i < size.height + GRID_GAP; i += GRID_GAP)
        {
            var newY = y - (y + i) % GRID_GAP;
            UserMaps[LAYER].SetLine((x, newY + i), (x + size.width, newY + i), new(Tile.SHADE_2, color));
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
        var (mx, my) = MousePosition;

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

        var prevSz = (UserMaps.View.width, UserMaps.View.height);
        var prevPos = (UserMaps.View.x, UserMaps.View.y);

        TryViewZoom();
        TryViewMove();
        ClampView();

        var (w, h) = (UserMaps.View.width, UserMaps.View.height);
        if (prevPos != (UserMaps.View.x, UserMaps.View.y))
            DisplayInfoText($"View {UserMaps.View.x + w / 2}, {UserMaps.View.y + h / 2}");

        if (prevSz != (UserMaps.View.width, UserMaps.View.height))
            DisplayInfoText($"View {UserMaps.View.width}x{UserMaps.View.height}");
    }
    private void TryViewMove()
    {
        if (IsDisabledViewMove)
            return;

        var mousePos = UserMaps.PointFrom(Mouse.CursorPosition, Window.Size, false);
        var aspectX = (float)UserMaps.Size.width / UserMaps.View.width;
        var aspectY = (float)UserMaps.Size.height / UserMaps.View.height;
        var (mx, my) = ((int)(mousePos.x / aspectX), (int)(mousePos.y / aspectY));
        var (px, py) = ((int)prevMousePos.x, (int)prevMousePos.y);
        var mmb = Mouse.IsButtonPressed(Mouse.Button.Middle);

        mx += UserMaps.View.x;
        my += UserMaps.View.y;

        if (mmb == false || (px == mx && py == my))
            return;

        var (deltaX, deltaY) = (mx - px, my - py);

        UserMaps.View = (
            UserMaps.View.x - deltaX,
            UserMaps.View.y - deltaY,
            UserMaps.View.width,
            UserMaps.View.height);
    }
    private void TryViewZoom()
    {
        if (Mouse.ScrollDelta == 0 || IsDisabledViewZoom)
            return;

        var prevZoom = zoom;
        zoom -= Mouse.ScrollDelta / 10f;
        zoom = Math.Clamp(zoom, 0.1f, 1f);

        var cx = (float)Mouse.CursorPosition.x;
        var cy = (float)Mouse.CursorPosition.y;
        cx = cx.Map((0, Window.Size.width), (0, 1)) * 16f;
        cy = cy.Map((0, Window.Size.height), (0, 1)) * 9.5f;
        var newX = UserMaps.View.x + cx;
        var newY = UserMaps.View.y + cy;

        if (Math.Abs(prevZoom - zoom) < 0.01f)
            return;

        var (x, y) = Mouse.ScrollDelta > 0 ?
            ((int)newX, (int)newY) :
            (UserMaps.View.x - 8, UserMaps.View.y - 5);
        var (w, h) = (UserMaps.Size.width * zoom, UserMaps.Size.height * zoom);

        UserMaps.View = (x, y, (int)w, (int)h);
    }
    private void ClampView()
    {
        var (w, h) = (UserMaps.View.width, UserMaps.View.height);
        var x = Math.Clamp(UserMaps.View.x, -w / 2, UserMaps.Size.width - w + w / 2);
        var y = Math.Clamp(UserMaps.View.y, -h / 2, UserMaps.Size.height - h + h / 2);
        UserMaps.View = (x, y, UserMaps.View.width, UserMaps.View.height);
    }

    public static float Map(float number, (float a, float b) range, (float a, float b) targetRange)
    {
        var value = (number - range.a) / (range.b - range.a) * (targetRange.b - targetRange.a) +
                    targetRange.a;
        return float.IsNaN(value) || float.IsInfinity(value) ? targetRange.a : value;
    }
#endregion
}