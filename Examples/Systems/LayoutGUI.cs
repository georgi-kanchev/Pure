using Pure.Engine.Execution;
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
Container: top			Area: left, top, width, 1		Pivot: Left
	Button: steam		Icon: 391			Text: Steam		Size: text + 1, 1		PadLeft: width
	Button: view		Text: View			Size: text, 1
	Button: friends		Text: Friends		Size: text, 1
	Button: games		Text: Games			Size: text, 1
	Button: help		Text: Help			Size: text, 1
Container: top-right	Pivot: Right		Area: left, top, width, 1	Color: 60, 60, 60, 255
	Button: speaker		Icon: 427			Size: 1, 1
	Button: bell		Icon: 353			Size: 1, 1
	Menu: profile		Text: @nickname438	Items: Profile, Account, Preferences, Wallet, Change Account, Sign Out		Size: 14, items		ItemSize: width, 1		IsSingleSelecting
	Button: screen		Icon: 395			Size: 1, 1
Container: top-second	Area: left, top + 1, width, 3	Pivot: Left		Gap: 0, 0
	Button: back		Text: <				Size: 3, 3
	Button: forward		Text: >				Size: 3, 3
	Button: space1		Text: .				Size: text + 2, 3	IsHidden	IsDisabled
	Button: store		Text: STORE			Size: text + 2, 3	IsToggle
	Button: library		Text: LIBRARY		Size: text + 2, 3	IsSelected
	Button: community	Text: COMMUNITY		Size: text + 2, 3	IsToggle
	Button: nickname	Text: NICKNAME438	Size: text + 2, 3	IsToggle
Container: left-games	Area: left, top + 5, 20, height - 6		Pivot: TopLeft	Gap: 0, 0
	Button: home		Text: Home			Size: width - 3, 3	IsSelected		PadRight: width - 2
	Button: collections	Icon: 369			Size: 3, 3			Toggle");

		var profile = layoutGUI.GetBlock<List>("profile");

		while (Window.KeepOpen())
		{
			foreach (var map in layoutGUI.TileMaps)
				map.ConfigureText("@", Tile.ICON_PERSON);

			if (profile!.IsFolded.Once("fff"))
				profile.Deselect();

			layoutGUI.UpdateAndDraw(layer);
			layer.Render();
		}
	}
}