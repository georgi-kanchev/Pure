using Pure.Utilities;

namespace Pure.EditorUserInterface;

using Tilemap;
using UserInterface;
using Window;

public static class Program
{
    public enum Layer
    {
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
    public static (float, float) InputPosition { get; set; }
    public static (int x, int y) CameraPosition { get; set; }
    public static (int w, int h) CameraSize { get; private set; }

    private static string infoText = "";
    private static int scroll = 3;
    private const int MAX_SCALE_ASPECT = 10;
    private static float infoTextTimer;
    static Program()
    {
        Window.Create(MAX_SCALE_ASPECT);

        var (width, height) = Window.MonitorAspectRatio;
        tilemaps = new((int)Layer.Count, (width * MAX_SCALE_ASPECT, height * MAX_SCALE_ASPECT));
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

            Mouse.CursorGraphics = (Mouse.Cursor)Element.MouseCursorResult;
            for (var i = 0; i < tilemaps.Count; i++)
            {
                var tmap = tilemaps[i];
                var cam = tmap.CameraUpdate();
                InputPosition = cam.PointFrom(Mouse.CursorPosition, Window.Size);
                Window.DrawTiles(cam.ToBundle());
            }

            Window.Activate(false);
        }
    }

    public static void SetInfoText(string text)
    {
        Console.WriteLine(text);
        infoText = text;
        infoTextTimer = 1f;
    }

    private static void Update()
    {
        if (Mouse.ScrollDelta != 0)
        {
            scroll -= Mouse.ScrollDelta;
            scroll = Math.Clamp(scroll, 1, 10);
            UpdateCamera();
            SetInfoText($"{CameraSize.w}x{CameraSize.h}");
        }

        Element.ApplyInput(
            Mouse.IsButtonPressed(Mouse.Button.Left),
            InputPosition,
            Mouse.ScrollDelta,
            Keyboard.KeyIDsPressed,
            Keyboard.KeyTyped,
            tilemaps.Size);

        ui.Update();
        editUI.Update();
        editPanel.Update();

        foreach (var kvp in menus)
            kvp.Value.Update();

        UpdateInfoText();

        var editPanelIsHidden = editPanel.Position == (int.MaxValue, int.MaxValue);
        var onLmbRelease = (Mouse.IsButtonPressed(Mouse.Button.Left) == false).Once("on-lmb-deselect");
        if (onLmbRelease && GetHovered() == null && editPanelIsHidden)
            Selected = null;
    }
    private static void UpdateInfoText()
    {
        infoTextTimer -= Time.Delta;

        if (infoTextTimer > 0 == false)
            return;

        var x = CameraPosition.x + CameraSize.w / 2 - infoText.Length / 2;
        var y = CameraPosition.y;
        tilemaps[(int)Layer.EditFront].SetTextLine((x, y), infoText);
    }

    private static void UpdateCamera()
    {
        var (width, height) = Window.MonitorAspectRatio;

        for (var i = 0; i < tilemaps.Count; i++)
        {
            var tmap = tilemaps[i];
            var (x, y, _, _) = tmap.Camera;
            CameraSize = (width * scroll, height * scroll);
            tmap.Camera = (x, y, CameraSize.w, CameraSize.h);
        }
    }

    private static Element? GetHovered()
    {
        for (var i = editUI.Count - 1; i >= 0; i--)
        {
            if (editUI[i].IsOverlapping(InputPosition))
                return editUI[i];
        }

        return null;
    }
}