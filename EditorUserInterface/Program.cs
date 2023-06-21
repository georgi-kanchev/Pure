using Pure.Utilities;

namespace Pure.EditorUserInterface;

using Tilemap;
using UserInterface;
using Window;

public static class Program
{
    public enum Layer
    {
        Grid,
        UiBack,
        UiMiddle,
        UiFront,
        EditBack,
        EditMiddle,
        EditFront,
        Count
    }

    public enum MenuType
    {
        Main,
        Add,
    }

    public static Element? Selected { get; set; }

    public static readonly Dictionary<MenuType, Menu> menus = new();
    public static readonly TilemapManager tilemaps;
    public static readonly RendererUI ui;
    public static readonly RendererEdit editUI;
    public static readonly EditPanel editPanel;
    public static (float x, float y) MousePosition { get; set; }
    public static (int x, int y) CameraPosition { get; set; }
    public static (int w, int h) CameraSize { get; private set; }

    private static string infoText = "";
    private static int zoom = 4;
    private const int SCALE_ASPECT_MAX = 10, SCALE_ASPECT_MIN = 2;
    private static float infoTextTimer;
    private static (float x, float y) prevMousePos;

    static Program()
    {
        Window.Create(SCALE_ASPECT_MAX);

        var (width, height) = Window.MonitorAspectRatio;
        tilemaps = new((int)Layer.Count, (width * SCALE_ASPECT_MAX, height * SCALE_ASPECT_MAX));
        ui = new();
        editUI = new();
        editPanel = new((int.MaxValue, int.MaxValue));

        menus[MenuType.Add] = new MenuAdd();
        menus[MenuType.Main] = new MenuMain();

        UpdateCamera();
    }

    private static void Main()
    {
        while (Window.IsOpen)
        {
            Window.Activate(true);
            Time.Update();
            tilemaps.Fill();

            Update();
            RendererEdit.DrawGrid();

            Mouse.CursorGraphics = (Mouse.Cursor)Element.MouseCursorResult;
            for (var i = 0; i < tilemaps.Count; i++)
            {
                var tmap = tilemaps[i];
                var cam = tmap.CameraUpdate();
                var (cx, cy, _, _) = tmap.Camera;
                MousePosition = cam.PointFrom(Mouse.CursorPosition, Window.Size);
                MousePosition = (MousePosition.x + cx, MousePosition.y + cy);
                Window.DrawTiles(cam.ToBundle());
            }

            Window.Activate(false);
        }
    }

    public static void DisplayInfoText(string text)
    {
        infoText += text + Environment.NewLine;
        infoTextTimer = 2f;
    }

    private static void Update()
    {
        Element.ApplyInput(
            Mouse.IsButtonPressed(Mouse.Button.Left),
            MousePosition,
            Mouse.ScrollDelta,
            Keyboard.KeyIDsPressed,
            Keyboard.KeyTyped,
            tilemaps.Size);

        TryControlCamera();

        ui.Update();
        editUI.Update();
        editPanel.Update();

        foreach (var kvp in menus)
            kvp.Value.Update();

        UpdateInfoText();

        var onLmbRelease = (Mouse.IsButtonPressed(Mouse.Button.Left) == false).Once("on-lmb-deselect");
        if (onLmbRelease && GetHovered() == null && editPanel.IsHidden)
            Selected = null;
    }
    private static void UpdateInfoText()
    {
        infoTextTimer -= Time.Delta;

        const int TEXT_WIDTH = 20;
        const int TEXT_HEIGHT = 2;
        var x = CameraPosition.x + CameraSize.w / 2 - TEXT_WIDTH / 2;
        var topY = CameraPosition.y;
        var bottomY = topY + CameraSize.h;
        var (mx, my) = MousePosition;

        tilemaps[(int)Layer.EditFront]
            .SetTextRectangle((x, bottomY - 1), (TEXT_WIDTH, 1), $"Cursor {(int)mx}, {(int)my}",
                alignment: Tilemap.Alignment.Center);

        if (infoTextTimer <= 0)
        {
            infoText = "";
            return;
        }

        tilemaps[(int)Layer.EditFront]
            .SetTextRectangle((x, topY), (TEXT_WIDTH, TEXT_HEIGHT), infoText,
                alignment: Tilemap.Alignment.Top, scrollProgress: 1f);
    }

    private static void TryControlCamera()
    {
        var prevSz = CameraSize;
        var prevPos = CameraPosition;

        if (Mouse.ScrollDelta != 0)
        {
            var prevZoom = zoom;
            zoom -= Mouse.ScrollDelta;
            zoom = Math.Clamp(zoom, SCALE_ASPECT_MIN, SCALE_ASPECT_MAX);

            var m = new Point(MousePosition);
            var c = new Point(CameraPosition);
            var cx = (float)Mouse.CursorPosition.x;
            var cy = (float)Mouse.CursorPosition.y;
            cx = cx.Map((0, Window.Size.width), (0, 1)) * 16f;
            cy = cy.Map((0, Window.Size.height), (0, 1)) * 9.5f;
            var newX = CameraPosition.x + cx;
            var newY = CameraPosition.y + cy;

            if (prevZoom != zoom)
                CameraPosition = Mouse.ScrollDelta > 0
                    ? ((int)newX, (int)newY)
                    : (CameraPosition.x - 8, CameraPosition.y - 5);
        }

        var mousePos = tilemaps[0].PointFrom(Mouse.CursorPosition, Window.Size, false);
        var tmapCameraAspectX = (float)tilemaps.Size.width / CameraSize.w;
        var tmapCameraAspectY = (float)tilemaps.Size.height / CameraSize.h;
        mousePos.x /= tmapCameraAspectX;
        mousePos.y /= tmapCameraAspectY;
        var (mx, my) = ((int)mousePos.x, (int)mousePos.y);
        var (px, py) = ((int)prevMousePos.x, (int)prevMousePos.y);
        var mmb = Mouse.IsButtonPressed(Mouse.Button.Middle);

        if (mmb && (px != mx || py != my))
        {
            var (deltaX, deltaY) = (mx - px, my - py);
            CameraPosition = (CameraPosition.x - deltaX, CameraPosition.y - deltaY);
            UpdateCamera();
        }

        var (w, h) = CameraSize;
        if (prevPos != CameraPosition)
        {
            DisplayInfoText($"Camera {CameraPosition.x + w / 2}, {CameraPosition.y + h / 2}");
            UpdateCamera();
        }

        if (prevSz != CameraSize)
        {
            DisplayInfoText($"Camera {CameraSize.w}x{CameraSize.h}");
            UpdateCamera();
        }

        prevMousePos = (mx, my);
    }
    private static void UpdateCamera()
    {
        var (width, height) = Window.MonitorAspectRatio;

        for (var i = 0; i < tilemaps.Count; i++)
        {
            var tmap = tilemaps[i];
            CameraSize = (width * zoom, height * zoom);
            var x = Math.Clamp(CameraPosition.x, 0, tilemaps.Size.width - CameraSize.w);
            var y = Math.Clamp(CameraPosition.y, 0, tilemaps.Size.height - CameraSize.h);
            CameraPosition = (x, y);

            tmap.Camera = (CameraPosition.x, CameraPosition.y, CameraSize.w, CameraSize.h);
        }
    }

    private static Element? GetHovered()
    {
        for (var i = editUI.Count - 1; i >= 0; i--)
        {
            if (editUI[i].IsOverlapping(MousePosition))
                return editUI[i];
        }

        return null;
    }
}