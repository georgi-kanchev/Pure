namespace Pure.EditorUserInterface;

using System.Diagnostics.CodeAnalysis;
using Pure.Tilemap;
using Pure.Tracker;
using Pure.UserInterface;
using Pure.Utilities;
using Pure.Window;

public partial class Program
{
	private enum Layer
	{
		UI_Back, UI_Middle, UI_Front,
		Edit_Back, Edit_Middle, Edit_Front,
		Count
	}
	private static RightClickMenu? rightClickMenu;
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
			for (int i = 0; i < tilemaps.Count; i++)
				Window.DrawTiles(tilemaps[i].ToBundle());
			Window.Activate(false);
		}
	}

	[MemberNotNull(nameof(tilemaps), nameof(rightClickMenu))]
	private static void Init()
	{
		Window.Create(3, Window.Mode.Windowed);

		var (width, height) = Window.MonitorAspectRatio;
		tilemaps = new TilemapManager((int)Layer.Count, (width * 3, height * 3));
		ui = new(tilemaps);
		edit = new(tilemaps, ui);

		rightClickMenu = new RightClickMenu(
			tilemaps[(int)Layer.Edit_Back], tilemaps[(int)Layer.Edit_Middle], ui, edit);
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
		rightClickMenu?.Update();
	}
}