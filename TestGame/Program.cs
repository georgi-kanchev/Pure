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
				Start();

				MyCoolButton(layer, btn);

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
		static void MyCoolButton(Tilemap layer, Slider btn)
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