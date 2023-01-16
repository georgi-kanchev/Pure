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

			var list = new List((5, 5), (10, 5));

			while(Window.IsExisting)
			{
				Start();

				bg.SetSquare(list.Position, list.Size, Tile.PATTERN_22, Color.Gray);
				layer.SetBar(list.Scroll.Position, Tile.BAR_BIG_VERTICAL, Color.White,
					list.Scroll.Size.Item2, true);
				over.SetTile(list.Scroll.Handle.Position, Tile.SHAPE_CIRCLE_HOLLOW, Color.Red);
				list.Update();
				list.Scroll.Update();

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
	}
}