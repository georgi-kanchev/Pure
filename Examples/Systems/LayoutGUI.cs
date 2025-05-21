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
Container: top			Area: ui_left, ui_top, ui_width, 1		Pivot: TopLeft		Tile: 10
	Button: steam		Icon: 391			Text:  Steam		Size: self_text, 1
	Button: view		Text: View			Size: self_text, 1
	Button: friends		Text: Friends		Size: self_text, 1
	Button: games		Text: Games			Size: self_text, 1
	Button: help		Text: Help			Size: self_text, 1
Container: top-second	Area: ui_left, ui_top + 1, ui_width, 3	Pivot: Left		Tile: 10	Color: 55, 55, 55, 255	Gap: 0, 0
	Button: back		Text: <				Size: 3, 3
	Button: forward		Text: >				Size: 3, 3
	Button: space1		Text: .				Size: 2, 3	IsHidden	IsDisabled
	Button: store		Text: STORE			Size: self_text + 2, 3	IsToggle
	Button: library		Text: LIBRARY		Size: self_text + 2, 3	IsSelected
	Button: community	Text: COMMUNITY		Size: self_text + 2, 3	IsToggle
	Button: nickname	Text: NICKNAME438	Size: self_text + 2, 3	IsToggle
Container: top-right	Pivot: TopRight		Area: ui_left, ui_top, ui_width, 1	Tile: 10
	Button: speaker		Icon: 427			Size: 1, 1
	Button: bell		Icon: 353			Size: 1, 1
	Menu: profile		Text: @nickname438	Items: Profile, Account, Preferences, Wallet, Change Account, Sign Out		Size: 14, self_items + 1		ItemSize: self_width, 1		IsSingleSelecting
	Button: screen		Icon: 395			Size: 1, 1
Container: left-top		Area: ui_left, ui_top + 4, ui_width * 0.2, 3		Pivot: TopLeft		Tile: 10	Gap: 0, 0
	Button: home		Text: Home			Size: owner_width - 3, 3			IsSelected		PadLeftRight: 0, self_width - 2
	Button: collections	Icon: 369			Size: 3, 3			IsToggle
Container: left-filter	Parent: left-top	Area: parent_left, parent_bottom, parent_width, 1	Pivot: TopLeft		Tile: 10	Color: 55, 55, 55, 255
	Dropdown: filter	Items: Games, Software, Tools	Size: owner_width - 6, 1	ItemSize: self_width, 1		SelectedItems: Games
	Button: linux		Icon: 449			Size: 1, 1		IsToggle
	Button: time		Icon: 575			Size: 1, 1		IsToggle
	Button: play		Icon: 532			Size: 1, 1		IsToggle");
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