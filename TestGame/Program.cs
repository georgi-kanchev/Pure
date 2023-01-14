using Pure.Tilemap;
using Pure.UserInterface;
using Pure.Utilities;
using Pure.Window;

namespace TestGame
{
	public class Program
	{
		static void Main()
		{
			var bg = new Tilemap((48, 27));
			var layer = new Tilemap((48, 27));
			var over = new Tilemap((48, 27));
			var scroll = new Scroll((0, 0), 6, true);
			var inputLine = new InputLine((0, 0), 11);
			var panel = new Panel((4, 4), (15, 7))
			{
				AdditionalMinSize = ("Cool Chat".Length, 4)
			};
			var clear = new Button((0, 0), (1, 1));
			var chat = "";

			clear.InCaseOf(UserInterface.When.Trigger, () => chat = "");
			KeyboardKey.OnPressed(KeyboardKey.ENTER, () =>
			{
				if(inputLine.IsFocused == false)
					return;

				chat += "\n" + inputLine.Text;
				inputLine.Text = "";
				scroll.Progress = 1f;
			});

			while(Window.IsExisting)
			{
				Start();

				MyCoolPanel(bg, layer, panel);
				MyCoolChat(layer, panel, scroll, chat);
				MyCoolSlider(bg, layer, scroll, panel);
				MyCoolClearButton(layer, panel, clear);
				MyCoolInputLine(bg, layer, over, inputLine, panel);

				End();

				void Start()
				{
					Window.Activate(true);
					Time.Update();

					UserInterface.ApplyInput(
						MouseButton.IsPressed(MouseButton.LEFT),
						layer.PositionFrom(MouseCursor.Position, Window.Size),
						MouseButton.ScrollDelta,
						KeyboardKey.Pressed,
						KeyboardKey.Typed,
						layer.Size);

					bg.Fill(0, 0);
					layer.Fill(0, 0);
					over.Fill(0, 0);
				}
				void End()
				{
					Window.DrawTilemap(bg, bg, (8, 8), (0, 0));
					Window.DrawTilemap(layer, layer, (8, 8), (0, 0));
					Window.DrawTilemap(over, over, (8, 8), (0, 0));

					MouseCursor.Type = UserInterface.MouseCursorTile;
					Window.Activate(false);
				}
			}
		}
		static void MyCoolClearButton(Tilemap layer, Panel panel, Button button)
		{
			var (px, py) = panel.Position;
			var (pw, ph) = panel.Size;
			var color = Color.Red;

			if(button.IsHovered) color = Color.Orange;
			if(button.IsClicked) color = Color.Brown;

			button.Position = (px + 1, py + ph - 2);
			layer.SetTile(button.Position, Tile.ICON_DELETE, color);
			button.Update();
		}
		static void MyCoolChat(Tilemap layer, Panel panel, Scroll scroll, string chat)
		{
			var (px, py) = panel.Position;
			var (pw, ph) = panel.Size;

			layer.SetTextBox(
				position: (px + 2, py + 1),
				size: (pw - 3, ph - 3),
				text: chat,
				alignment: Tilemap.Alignment.BottomRight,
				scrollProgress: scroll.Progress);
		}
		static void MyCoolPanel(Tilemap bg, Tilemap layer, Panel panel)
		{
			var (x, y) = panel.Position;
			bg.SetSquare(panel.Position, panel.Size, Tile.PATTERN_3, Color.Gray);
			bg.SetSquare(panel.Position, (panel.Size.Item1, 1), Tile.SHADE_OPAQUE, Color.Brown);
			layer.SetBorder(panel.Position, panel.Size, Tile.BORDER, Color.Orange);
			layer.SetTextBox((x, y), (panel.Size.Item1, 1), "Cool Chat", Color.White,
				alignment: Tilemap.Alignment.Center);
			panel.Update();
		}
		static void MyCoolSlider(Tilemap bg, Tilemap layer, Scroll scroll, Panel panel)
		{
			var (px, py) = panel.Position;
			var (pw, ph) = panel.Size;

			scroll.Position = (px + 1, py + 1);
			scroll.Size = (1, ph - 3);

			var color = Color.Orange;
			var v = scroll.IsVertical;
			var (w, h) = scroll.Size;

			if(scroll.IsHovered) color = Color.White;
			if(scroll.IsClicked) color = Color.Brown;

			bg.SetBar(scroll.Position, Tile.BAR_BIG_VERTICAL_HOLLOW, color, v ? h : w, v);
			layer.SetSquare(scroll.Handle.Position, scroll.Handle.Size, Tile.SHAPE_CIRCLE_SMALL);
			scroll.Update();
		}
		static void MyCoolInputLine(Tilemap bg, Tilemap layer, Tilemap over, InputLine inputLine,
			Panel panel)
		{
			var (px, py) = panel.Position;
			var (pw, ph) = panel.Size;
			var bgCol = Color.Brown;
			var col = Color.Gray;
			inputLine.Position = (px + 2, py + ph - 2);
			inputLine.Size = (pw - 3, 1);

			if(inputLine.IsHovered)
			{
				bgCol = Color.Orange;
				col = Color.Brown;
			}
			if(inputLine.IsFocused)
			{
				bgCol = Color.Purple;
				col = Color.White;
			}

			bg.SetSquare(inputLine.Position, inputLine.Size, Tile.SHADE_OPAQUE, bgCol);
			bg.SetInputLineSelection(inputLine.Position, inputLine.IndexCursor, inputLine.IndexSelection,
				Color.Blue);
			layer.SetTextLine(inputLine.Position, inputLine.Text, col);
			over.SetInputLineCursor(inputLine.Position, inputLine.IsFocused, inputLine.IndexCursor,
				Color.Green);

			inputLine.Update();
		}
	}
}