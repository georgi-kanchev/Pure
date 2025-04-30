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
		var layer = new LayerTiles((w * 4, h * 4));
		var layoutGUI = new Layout(@"
Container: top			Area: left, top, width, 1						Pivot: Left
	Button: steam		Text: ~Steam	Size: text, 1
	Button: view		Text: View		Size: text, 1
	Button: friends		Text: Friends	Size: text, 1
	Button: games		Text: Games		Size: text, 1
	Button: help		Text: Help		Size: text, 1
Container: top-right	Parent: top		Area: left, top, width, 1		Pivot: Right
	Button: speaker		Text: >			Size: 1, 1
	Button: bell		Text: *			Size: 1, 1
	Dropdown: profile	Items: @nickname438, Profile, Account, Preferences, Wallet, Change Account, Sign Out		Size: 14, items		ItemSize: width, 1
	Button: screen		Text: #			Size: 1, 1");

		var profile = layoutGUI.GetBlock<List>("profile");

		while (Window.KeepOpen())
		{
			if ((layoutGUI.TileMaps.Count > 0).Once("text-config"))
				foreach (var map in layoutGUI.TileMaps)
				{
					map.ConfigureText("~", Tile.ICON_ZAP);
					map.ConfigureText(">", Tile.AUDIO_VOLUME_HIGH);
					map.ConfigureText("*", Tile.ICON_BELL);
					map.ConfigureText("@", Tile.ICON_PERSON);
					map.ConfigureText("#", Tile.ICON_SCREEN);
					map.ConfigureText("_", Tile.PUNCTUATION_UNDERSCORE);
					map.ConfigureText("+", Tile.SHAPE_SQUARE_HOLLOW);
					map.ConfigureText("-", Tile.ICON_X);
				}

			if (profile!.IsCollapsed.Once("fff"))
				profile.Select(profile.Items[0]);

			layoutGUI.DrawGUI(layer);
			layer.Render();
		}
	}
}