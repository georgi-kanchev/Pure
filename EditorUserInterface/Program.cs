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

    public static Element? Selected;

    public static readonly Dictionary<MenuType, Menu> menus = new();
    public static readonly TilemapManager tilemaps;
    public static readonly RendererUI ui;
    public static readonly RendererEdit editUI;
    public static readonly EditPanel editPanel;
    public static (float, float) InputPosition => tilemaps.PointFrom(Mouse.CursorPosition, Window.Size);

    static Program()
    {
        Window.Create(3);

        var (width, height) = Window.MonitorAspectRatio;
        tilemaps = new((int)Layer.Count, (width * 3, height * 3));
        ui = new();
        editUI = new();
        editPanel = new((int.MaxValue, int.MaxValue));

        menus[MenuType.Add] = new MenuAdd();
        menus[MenuType.Main] = new MenuMain();
    }

    private static void Main()
    {
        while (Window.IsOpen)
        {
            Window.Activate(true);
            tilemaps.Fill();

            Update();

            Mouse.CursorGraphics = (Mouse.Cursor)Element.MouseCursorResult;
            for (var i = 0; i < tilemaps.Count; i++)
                Window.DrawTiles(tilemaps[i].ToBundle());
            Window.Activate(false);
        }
    }

    private static void Update()
    {
        var mousePos = tilemaps.PointFrom(Mouse.CursorPosition, Window.Size);
        Element.ApplyInput(
            Mouse.IsButtonPressed(Mouse.Button.Left),
            mousePos,
            Mouse.ScrollDelta,
            Keyboard.KeyIDsPressed,
            Keyboard.KeyTyped,
            tilemaps.Size);

        ui.Update();
        editUI.Update();
        editPanel.Update();

        foreach (var kvp in menus)
            kvp.Value.Update();

        var editPanelIsHidden = editPanel.Position == (int.MaxValue, int.MaxValue);
        var onLmbRelease = (Mouse.IsButtonPressed(Mouse.Button.Left) == false).Once("on-lmb-deselect");
        if (onLmbRelease && GetHovered() == null && editPanelIsHidden)
            Selected = null;
    }

    private static Element? GetHovered()
    {
        var inputPos = tilemaps.PointFrom(Mouse.CursorPosition, Window.Size);

        for (var i = editUI.Count - 1; i >= 0; i--)
        {
            if (editUI[i].IsOverlapping(inputPos))
                return editUI[i];
        }

        return null;
    }
}