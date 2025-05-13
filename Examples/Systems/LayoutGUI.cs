using Pure.Engine.Tiles;
using Pure.Engine.UserInterface;
using Pure.Engine.Window;
using Pure.Tools.UserInterface;
using Monitor = Pure.Engine.Window.Monitor;

namespace Pure.Examples.Systems;

public static class LayoutGUI
{
	public static void Run()
	{
		Window.Title = "Pure - Layout Graphical User Interface Example";
		Window.PixelScale = 1;

		var (w, h) = Monitor.Current.AspectRatio;
		var layer = new LayerTiles((w * 6, h * 6));
		var layoutGUI = new Layout(@"
//Container: top			Area: left, top, width, 1		Pivot: Left		Tile: 10
//	Button: steam		Icon: 391			Text:  Steam		Size: text, 1
//	Button: view		Text: View			Size: text, 1
//	Button: friends		Text: Friends		Size: text, 1
//	Button: games		Text: Games			Size: text, 1
//	Button: help		Text: Help			Size: text, 1
//Container: top-second	Area: left, top + 1, width, 3	Pivot: Left		Gap: 0, 0	Tile: 10	Color: 45, 45, 45, 255
//	Button: back		Text: <				Size: 3, 3
//	Button: forward		Text: >				Size: 3, 3
//	Button: space1		Text: .				Size: 2, 3	IsHidden	IsDisabled
//	Button: store		Text: STORE			Size: text + 2, 3	IsToggle
//	Button: library		Text: LIBRARY		Size: text + 2, 3	IsSelected
//	Button: community	Text: COMMUNITY		Size: text + 2, 3	IsToggle
//	Button: nickname	Text: NICKNAME438	Size: text + 2, 3	IsToggle
//Container: top-right	Pivot: TopRight		Area: left + width / 2, top, width / 2, 8
//	Button: speaker		Icon: 427			Size: 1, 1
//	Button: bell		Icon: 353			Size: 1, 1
//	Menu: profile		Text: @nickname438	Items: Profile, Account, Preferences, Wallet, Change Account, Sign Out		Size: 14, items + 1		ItemSize: 14, 1		IsSingleSelecting
//	Button: screen		Icon: 395			Size: 1, 1
Container: left-games	Area: left, top + 5, width * 0.4, height - 6	Pivot: TopRight		Gap: 0, 0	Tile: 10	Wrap: SingleRow
	Button: home		Text: Home			Size: 17, 7			IsSelected		PadLeftRight: 0, width - 2
	Button: collections	Icon: 369			Size: 3, 3			Toggle
	Button: test		Text: Test			Size: text, 1
	Button: long		Text: very much looooonger		Size: text + 2, 5
	Button: smol		Text: smol						Size: text + 2, 3
	Button: l			Text: testttttttt					Size: text, 1
	Button: longer		Text: another looong button		Size: text + 2, 3");
		// var profile = layoutGUI.GetBlock<List>("profile");
		// profile!.OnFoldChange += () => profile.Deselect();

		while (Window.KeepOpen())
		{
			foreach (var map in layoutGUI.TileMaps)
				map.ConfigureText("@", Tile.ICON_PERSON);

			layoutGUI.DrawGUI(layer);
			layer.Render();
		}
	}
}