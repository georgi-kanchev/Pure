using Pure.Input;
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
			var inputBox = new InputLine((10, 15), 16, "Test");
			var container = new Container((10, 2), (13, 5)) { Text = "Title" };
			var container2 = new Container((15, 10), (10, 5)) { Text = "Title2" };
			container.AdditionalMaxSize = (5, 1);

			while(Window.IsExisting)
			{
				var mousePos = Window.MousePosition;
				var hov = layer.PositionFrom(mousePos, Window.Size);

				Mouse.Update();
				Keyboard.Update();
				Time.Update();
				bg.Fill(0, 0);
				layer.Fill(0, 0);
				over.Fill(0, 0);

				var input = new Input()
				{
					Position = hov,
					IsPressed = Mouse.IsPressed(Pure.Input.Button.LEFT),
					TypedSymbols = Keyboard.TypedSymbols,
					PressedKeys = Keyboard.Pressed,
				};
				Window.DrawEnable(true);

				UserInterface.ApplyInput(input, layer.Size);
				MyCoolContainer(bg, layer, container);
				var (cx, cy) = container.Position;
				inputBox.Position = (cx + 1, cy + 1);
				MyCoolInputBoxUpdate(bg, layer, over, inputBox);

				MyCoolContainer(bg, layer, container2);

				var camBg = bg.UpdateCamera();
				var camLayer = layer.UpdateCamera();
				var camOver = over.UpdateCamera();

				Window.DrawTilemap(camBg, camBg, (8, 8), (0, 0));
				Window.DrawTilemap(camLayer, camLayer, (8, 8), (0, 0));
				Window.DrawTilemap(camOver, camOver, (8, 8), (0, 0));

				Window.MouseCursor = UserInterface.MouseCursorTile;

				Window.DrawEnable(false);
			}
		}
		static void MyCoolInputBoxUpdate(Tilemap bg, Tilemap layer, Tilemap over, InputLine inputBox)
		{
			var b = inputBox;
			bg.SetSquare(b.Position, b.Size, 10, Color.Gray);
			bg.SetInputLineSelection(b.Position, b.IndexCursor, b.IndexSelection, Color.Blue);
			layer.SetTextLine(b.Position, b.Text, Color.Red);
			over.SetInputLineCursor(b.Position, b.IsFocused, b.IndexCursor, Color.White);
			b.Update();
		}
		static void MyCoolContainer(Tilemap bg, Tilemap layer, Container container)
		{
			var c = container;
			bg.SetSquare(c.Position, c.Size, Tile.PATTERN_24, Color.Gray);
			layer.SetNinePatch(c.Position, c.Size, Tile.BORDER_GRID_TOP_LEFT, Color.Blue);
			layer.SetTextLine((c.Position.Item1 + 1, c.Position.Item2), c.Text, Color.White);
			c.Update();
		}
	}
}