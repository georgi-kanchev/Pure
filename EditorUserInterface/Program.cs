using System.Dynamic;
using Pure.Utilities;

namespace Pure.EditorUserInterface;

using System.Diagnostics.CodeAnalysis;
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

    static Program()
    {
        Window.Create(3);

        var (width, height) = Window.MonitorAspectRatio;
        tilemaps = new((int)Layer.Count, (width * 3, height * 3));
        ui = new(tilemaps);
        editUI = new();
        editPanel = new((int.MaxValue, int.MaxValue), tilemaps);

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

        ui?.Update();
        editUI?.Update();
        editPanel.Update();

        foreach (var kvp in menus)
            kvp.Value.Update();

        var onLmb = Mouse.IsButtonPressed(Mouse.Button.Left).Once("on-lmb-deselect");
        if (onLmb && GetHovered() == null)
            Selected = null;
    }

    private static Element? GetHovered()
    {
        var keys = editUI.Keys;
        var inputPos = tilemaps.PointFrom(Mouse.CursorPosition, Window.Size);

        for (var i = keys.Length - 1; i >= 0; i--)
        {
            if (editUI[keys[i]].IsOverlapping(inputPos))
                return editUI[keys[i]];
        }

        return null;
    }
}