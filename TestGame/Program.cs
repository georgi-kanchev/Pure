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
			var btn = new Slider((5, 5), 5, true);

			while(Window.IsExisting)
			{
				var mousePos = Window.MousePosition;
				var hov = layer.PositionFrom(mousePos, Window.Size);

				Time.Update();
				var input = new Input()
				{
					Position = hov,
					IsPressed = Mouse.IsPressed(Pure.Input.Button.LEFT),
					TypedSymbols = Keyboard.TypedSymbols,
					PressedKeys = Keyboard.Pressed,
				};
				UserInterface.ApplyInput(input, layer.Size);

				bg.Fill(0, 0);
				layer.Fill(0, 0);
				over.Fill(0, 0);

				Window.DrawEnable(true);

				layer.SetBar((5, 10), Tile.BAR_BIG_VERTICAL_HOLLOW, size: 8, isVertical: true);
				MyCoolButton(bg, layer, btn);

				Window.DrawTilemap(bg, bg, (8, 8), (0, 0));
				Window.DrawTilemap(layer, layer, (8, 8), (0, 0));
				Window.DrawTilemap(over, over, (8, 8), (0, 0));

				Window.MouseCursor = UserInterface.MouseCursorTile;

				Window.DrawEnable(false);
			}
		}
		static void MyCoolButton(Tilemap bg, Tilemap layer, Slider btn)
		{
			var color = Color.Orange;
			var v = btn.IsVertical;

			if(btn.IsHovered) color = Color.White;
			if(btn.IsClicked) color = Color.Brown;

			layer.SetBar(btn.Position, Tile.BAR_BIG_VERTICAL_HOLLOW, color, v ? btn.Size.Item2 : btn.Size.Item1, v);
			layer.SetSquare(btn.Handle.Position, btn.Handle.Size, Tile.PATTERN_33);
			btn.Update();
		}
	}
}