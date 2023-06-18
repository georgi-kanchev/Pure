namespace Pure.EditorUserInterface;

using System.Diagnostics.CodeAnalysis;
using Tilemap;
using UserInterface;
using Window;

public static class Program
{
    private enum Layer
    {
        UiBack,
        UiMiddle,
        UiFront,
        EditBack,
        EditMiddle,
        EditFront,
        Count
    }

    private enum Menus
    {
        Main,
        Add,
    }

    private static readonly Dictionary<Menus, Menu> menus = new();
    private static TilemapManager? tilemaps;
    private static RendererUI? ui;
    private static RendererEdit? edit;

    private static void Main()
    {
        Init();

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

    [MemberNotNull(nameof(tilemaps))]
    private static void Init()
    {
        Window.Create(3);

        var (width, height) = Window.MonitorAspectRatio;
        tilemaps = new((int)Layer.Count, (width * 3, height * 3));
        ui = new(tilemaps);
        edit = new(tilemaps, ui);

        var back = tilemaps[(int)Layer.EditBack];
        var middle = tilemaps[(int)Layer.EditMiddle];
        menus[Menus.Add] = new MenuAdd(back, middle, edit);
        menus[Menus.Main] = new MenuMain(back, middle, (MenuAdd)menus[Menus.Add], ui);
    }
    private static void Update()
    {
        if (tilemaps == null)
            return;

        var mousePos = tilemaps.PointFrom(Mouse.CursorPosition, Window.Size);
        Element.ApplyInput(
            Mouse.IsButtonPressed(Mouse.Button.Left),
            mousePos,
            Mouse.ScrollDelta,
            Keyboard.KeyIDsPressed,
            Keyboard.KeyTyped,
            tilemaps.Size);

        ui?.Update();
        edit?.Update();

        foreach (var kvp in menus)
            kvp.Value.Update();
    }
}