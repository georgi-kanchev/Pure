using Pure.Engine.Execution;
using Pure.Engine.Hardware;
using Pure.Engine.Tiles;
using Pure.Engine.Utility;
using Pure.Engine.Window;

namespace Pure.Examples.Systems;

public static class AutoTiling
{
	public static void Run()
	{
		var window = new Window { Title = "Pure - Auto Tiling Example" };
		var hardware = new Hardware(window.Handle);
		var (w, h) = hardware.Monitors[0].AspectRatio;
		var layer = new LayerTiles((w * 3, h * 3));
		var original = new TileMap(layer.Size);
		var firstPass = new TileMap(layer.Size);
		var secondPass = new TileMap(layer.Size);
		var flag = false;
		var boxRules = new TileMapperRules();
		var pipeRules = new TileMapperRules();

		boxRules.Add(Tile.FULL, [
			Tile.FULL, Tile.FULL, Tile.FULL,
			Tile.FULL, Tile.FULL, Tile.FULL,
			Tile.FULL, Tile.FULL, Tile.FULL
		]);
		boxRules.Add(new Tile(Tile.BOX_OUTLINE_CORNER).Rotate(2), [
			null /**/, Tile.FULL, Tile.FULL,
			Tile.FULL, Tile.FULL, Tile.FULL,
			Tile.FULL, Tile.FULL, Tile.FULL
		]);
		boxRules.Add(Tile.BOX_OUTLINE_EDGE, [
			null /**/, null /**/, null /**/,
			Tile.FULL, Tile.FULL, Tile.FULL,
			Tile.FULL, Tile.FULL, Tile.FULL
		]);
		boxRules.Add(Tile.BOX_OUTLINE_CORNER, [
			null, null /**/, null /**/,
			null, Tile.FULL, Tile.FULL,
			null, Tile.FULL, Tile.FULL
		]);

		pipeRules.Add(Tile.PIPE_BIG_CROSS, [
			null /*   */, Tile.SHADE_9, null /*   */,
			Tile.SHADE_9, Tile.SHADE_9, Tile.SHADE_9,
			null /*   */, Tile.SHADE_9, null /*   */
		]);
		pipeRules.Add(Tile.PIPE_BIG_T_SHAPED, [
			null, Tile.SHADE_9, null /*   */,
			null, Tile.SHADE_9, Tile.SHADE_9,
			null, Tile.SHADE_9, null /*   */
		]);
		pipeRules.Add(Tile.PIPE_BIG_CORNER, [
			null, null /*   */, null /*   */,
			null, Tile.SHADE_9, Tile.SHADE_9,
			null, Tile.SHADE_9, null /*   */
		]);
		pipeRules.Add(Tile.PIPE_BIG_STRAIGHT, [
			null /*   */, null /*   */, null,
			Tile.SHADE_9, Tile.SHADE_9, null,
			null /*   */, null /*   */, null
		]);

		hardware.Mouse.OnPress(Mouse.Button.Left, () => flag = !flag);

		var rule = boxRules.Get(2, 1);

		while (window.KeepOpen())
		{
			Time.Update();
			Flow.Update(Time.Delta);

			original.Flush();
			firstPass.Flush();
			secondPass.Flush();

			original.SetArea((10, 10, 15, 10), [Tile.FULL]);
			original.SetArea((18, 5, 10, 10), [Tile.FULL]);
			original.SetLine((20, 7), (25, 9), [Tile.SHADE_1]);
			original.SetCircle((13, 17), 1, true, [Tile.SHADE_1]);
			original.SetBox((30, 5, 12, 12), Tile.EMPTY, Tile.SHADE_9, Tile.SHADE_9);
			original.SetLine((37, 5), (37, 15), [Tile.SHADE_9]);
			original.SetLine((30, 8), (40, 8), [Tile.SHADE_9]);

			var mouseCell = layer.PositionFromPixel(window, hardware.Mouse.CursorPosition);
			original.SetTile(((int)mouseCell.x, (int)mouseCell.y), Tile.SHADE_1);

			firstPass.SetTiles((0, 0), original);
			firstPass.SetText((0, 0), "<LMB> to remove rules");
			original.SetText((0, 0), "<LMB> to apply rules");

			boxRules.Apply(firstPass);
			pipeRules.Apply(firstPass);

			secondPass.SetTiles((0, 0), firstPass);
			boxRules.Apply(secondPass);
			secondPass.Replace(Tile.FULL, [Tile.EMPTY]);

			layer.DrawTileMap(flag ? original : secondPass);
			layer.DrawMouseCursor(window, hardware.Mouse.CursorPosition, (int)hardware.Mouse.CursorCurrent);
			layer.Render(window);
		}
	}
}