using Pure.Engine.Collision;

namespace Pure.Editors.Map;

internal class TilePalette
{
	public bool justPickedTile;
	public TileMap map;
	public LayerTiles layerTiles;
	public (int x, int y) mousePos;
	public (int x, int y) start, end;

	public TilePalette(Editor editor)
	{
		this.editor = editor;
		Create((26, 26));

		Mouse.Button.Left.OnPress(() =>
		{
			if (Program.menu.IsHidden == false ||
			    editor.Prompt.IsHidden == false ||
			    inspector is { IsHovered: false } ||
			    layerTiles.IsHovered == false)
				return;

			var pos = layerTiles.PositionFromPixel(Mouse.CursorPosition);
			var (vx, vy) = map.View.Position;
			selectedPos = ((int)pos.x + vx, (int)pos.y + vy);
			selectedSz = (1, 1);
		});
		Mouse.Button.Left.OnPress(OnMousePressed);
		Mouse.Button.Left.OnRelease(OnMouseRelease);

		Keyboard.Key.C.OnPress(() =>
		{
			var ctrl = Keyboard.Key.ControlLeft.IsPressed() || Keyboard.Key.ControlRight.IsPressed();

			if (ctrl == false || inspector?.tools.Current != 14) // select
				return;

			var (szw, szh) = (end.x - start.x, end.y - start.y);
			var rect = new Solid(start.x, start.y, szw, szh);
			var tilemap = inspector?.GetSelectedTilemap();

			copyTiles = tilemap?.TilesIn(rect.ToBundle());
		});
		Keyboard.Key.V.OnPress(() =>
		{
			var ctrl = Keyboard.Key.ControlLeft.IsPressed() || Keyboard.Key.ControlRight.IsPressed();

			if (ctrl == false || inspector?.tools.Current != 14) // select
				return;

			var tilemap = inspector?.GetSelectedTilemap();
			tilemap?.SetTiles(start, copyTiles ?? new Tile[0, 0]);
		});
		Keyboard.Key.X.OnPress(() =>
		{
			var ctrl = Keyboard.Key.ControlLeft.IsPressed() || Keyboard.Key.ControlRight.IsPressed();

			if (ctrl == false || inspector?.tools.Current != 14) // select
				return;

			var (szw, szh) = (end.x - start.x, end.y - start.y);
			var rect = new Solid(start.x, start.y, szw, szh);
			var tilemap = inspector?.GetSelectedTilemap();

			copyTiles = tilemap?.TilesIn(rect.ToBundle());
			tilemap?.SetArea(rect.ToBundle(), [Tile.EMPTY]);
		});
		Keyboard.Key.Delete.OnPress(() =>
		{
			if (inspector?.tools.Current != 14) // select
				return;

			var tilemap = inspector?.GetSelectedTilemap();
			var (szw, szh) = (end.x - start.x, end.y - start.y);
			var rect = new Solid(start.x, start.y, szw, szh);

			tilemap?.SetArea(rect.ToBundle(), [Tile.EMPTY]);
		});
	}
	[MemberNotNull(nameof(map), nameof(layerTiles))]
	public void Create((int width, int height) size)
	{
		map = new(size) { View = (0, 0, 10, 10) };
		layerTiles = new(map.View.Size) { Zoom = 7.6f, PixelOffset = (198, 88) };
	}

	public void Update(Inspector inspector, TerrainPanel terrainPanel)
	{
		prevMousePosWorld = editor.MousePositionWorld;
		justPickedTile = false;

		var tool = inspector.tools.Current;

		inspector.paletteScrollH.IsHidden = tool > 8;
		inspector.paletteScrollV.IsHidden = tool > 8;
		inspector.paletteColor.IsHidden = tool > 8 && tool != 12;

		if (editor.Prompt.IsHidden == false || tool > 8)
			return;

		var (tw, th) = layerTiles.AtlasTileCount;
		map.View = new(map.View.Position, (Math.Min(10, (int)tw), Math.Min(10, (int)th)));
		layerTiles.Size = map.View.Size;
		var (mw, mh) = map.Size;
		var (vw, vh) = map.View.Size;

		for (var i = 0; i < mh; i++)
			for (var j = 0; j < mw; j++)
			{
				var id = (ushort)(i, j).ToIndex((mw, mh));
				var tile = new Tile(id, Color.White);
				map.SetTile((j, i), tile);
			}

		this.inspector = inspector;
		this.terrainPanel = terrainPanel;
		inspector.paletteScrollH.Ratio = 1f / (mw - vw);
		inspector.paletteScrollV.Ratio = 1f / (mh - vh);
		var w = (int)MathF.Round(inspector.paletteScrollH.Slider.Progress * (mw - vw));
		var h = (int)MathF.Round(inspector.paletteScrollV.Slider.Progress * (mh - vh));
		map.View = new((w, h), map.View.Size);

		var (mx, my) = layerTiles.PositionFromPixel(Mouse.CursorPosition);
		prevMousePos = mousePos;
		mousePos = ((int)mx + w, (int)my + h);

		var view = map.UpdateView();
		layerTiles.DrawTileMap(view);

		if (layerTiles.IsHovered)
			layerTiles.DrawTiles(((int)mx, (int)my),
				new Tile(layerTiles.AtlasTileIdFull, new Color(50, 100, 255, 150)));

		UpdateSelected();
		var s = selected.ToBundle();
		layerTiles.DrawRectangles([(s.x, s.y, s.width, s.height)], new Color(50, 255, 100, 150));

		layerTiles.Render();
	}

	public void TryDraw()
	{
		var (mx, my) = ((int)editor.MousePositionWorld.x, (int)editor.MousePositionWorld.y);
		var tool = inspector?.tools.Current ?? -1;
		var selectedColor = inspector?.paletteColor.SelectedColor ?? uint.MaxValue;
		var color = new Color(selectedColor) { A = 200 };
		var seed = mx.ToSeed([my]) + clickSeed;
		var tiles = GetSelectedTiles();

		for (var i = 0; i < tiles.GetLength(1); i++)
			for (var j = 0; j < tiles.GetLength(0); j++)
				tiles[j, i].Tint = color;

		var preview = new TileMap(tiles);
		var randomTile = tiles.Flatten().ChooseOne(seed);
		var tilemap = inspector?.GetSelectedTilemap();
		var (szw, szh) = (end.x - start.x, end.y - start.y);

		preview.View = new((-mx, -my), tilemap?.Size ?? (1, 1));

		if (tool is 1) // group of tiles
			editor.LayerTilesMap.DrawTileMap(preview.UpdateView());
		else if (tool is 2 or 7 or 8) // single random tile of tiles/replace/fill
			editor.LayerTilesMap.DrawTiles((mx, my), randomTile);
		else if (rectangleTools.Contains(tool)) // rectangle/ellipse of random tiles
			editor.LayerTilesMap.DrawRectangles([(start.x, start.y, szw, szh)], color);
		else if (tool == 4 && start != end) // line of random tiles
			editor.LayerTilesMap.DrawLines(
			[
				(start.x, start.y, end.x - 1, end.y - 1),
				(start.x + 1, start.y, end.x, end.y - 1),
				(start.x + 1, start.y + 1, end.x, end.y),
				(start.x, start.y + 1, end.x - 1, end.y)
			], color);

		if (IsPaintAllowed() && Mouse.Button.Left.IsPressed())
			OnMouseHold(randomTile, tilemap);
	}
	public Tile[,] GetSelectedTiles()
	{
		var (sx, sy) = selected.Position;
		sx += map.View.X;
		sy += map.View.Y;
		var pose = inspector?.pose.Items.IndexOf(inspector.pose.SelectedItems[0]) ?? 0;
		var (sw, sh) = selected.Size;
		var tiles = map.TilesIn(((int)sx, (int)sy, (int)sw, (int)sh));
		for (var i = 0; i < tiles.GetLength(1); i++)
			for (var j = 0; j < tiles.GetLength(0); j++)
			{
				tiles[j, i].Tint = inspector?.paletteColor.SelectedColor ?? uint.MaxValue;
				tiles[j, i].Pose = (Pose)pose;
			}

		if (pose > 3)
			tiles.Flip((false, true));

		tiles = tiles.Rotate(-pose);

		return tiles;
	}

#region Backend
	private readonly List<int> rectangleTools = [3, 5, 6, 9, 10, 11, 12, 13, 14];
	private Inspector? inspector;
	private TerrainPanel? terrainPanel;
	private (int x, int y) prevMousePos;
	private readonly Editor editor;
	private (float x, float y) selectedPos, prevMousePosWorld;
	private (float w, float h) selectedSz = (1, 1);
	private static int clickSeed;
	private static bool isDrawingSelection, isDraggingTiles, isDraggingSelection;
	private Tile[,]? draggingTiles, copyTiles;
	private Solid selected;

	static TilePalette()
	{
		Mouse.Button.Left.OnRelease(() => clickSeed = (-10000, 10000).Random());
	}
	private void UpdateSelected()
	{
		if (layerTiles.IsHovered &&
		    Mouse.Button.Left.IsPressed() &&
		    prevMousePos != mousePos &&
		    isDrawingSelection == false)
		{
			var (szx, szy) = (mousePos.x - selectedPos.x, mousePos.y - selectedPos.y);
			selectedSz = (szx + (szx < 0 ? -1 : 1), szy + (szy < 0 ? -1 : 1));
		}

		var (sx, sy) = selectedPos;
		var (sw, sh) = selectedSz;
		var (ox, oy) = (sw < 0 ? 1 : 0, sh < 0 ? 1 : 0);
		var (vx, vy) = map.View.Position;
		selected = new(sx + ox - vx, sy + oy - vy, sw, sh);
	}

	[MemberNotNullWhen(true, nameof(inspector))]
	private bool IsPaintAllowed()
	{
		return inspector is { IsHovered: false } &&
		       terrainPanel is { IsHovered: false } &&
		       editor.Prompt.IsHidden &&
		       Program.menu.IsHidden &&
		       editor.MapPanel.IsHovered == false;
	}

	private void OnMousePressed()
	{
		if (IsPaintAllowed() == false)
			return;

		var tilemap = inspector.GetSelectedTilemap();

		if (inspector.tools.Current == 14) // select
		{
			isDrawingSelection = false;
			isDraggingSelection = false;
			isDraggingTiles = false;

			var (szw, szh) = (end.x - start.x, end.y - start.y);
			var rect = new Solid(start.x, start.y, szw, szh);
			var ctrl = Keyboard.Key.ControlLeft.IsPressed() || Keyboard.Key.ControlRight.IsPressed();
			if (rect.IsContaining(editor.MousePositionWorld))
			{
				isDraggingSelection = true;

				if (ctrl == false)
					return;

				isDraggingTiles = true;
				draggingTiles = tilemap?.TilesIn(rect.ToBundle());
				tilemap?.SetArea(rect.ToBundle(), [Tile.EMPTY]);

				return;
			}

			isDrawingSelection = true;
		}

		start = ((int)editor.MousePositionWorld.x, (int)editor.MousePositionWorld.y);
		end = (start.x + 1, start.y + 1);

		if (inspector.layers.SelectedItems.Count != 1)
		{
			editor.PromptMessage("Select a single layer to draw on.");
			return;
		}

		tilemap?.ApplySeed(clickSeed);
	}
	private void OnMouseRelease()
	{
		if (inspector == null)
			return;

		var tilemap = inspector.GetSelectedTilemap();

		if (IsPaintAllowed() == false || tilemap == null)
			return;

		var (mx, my) = ((int)editor.MousePositionWorld.x, (int)editor.MousePositionWorld.y);
		var tool = inspector.tools.Current;
		var (szw, szh) = (end.x - start.x, end.y - start.y);
		var tiles = GetSelectedTiles().Flatten();
		var (tw, th) = tilemap.Size;

		if (rectangleTools.Contains(tool))
		{
			if (start.x > end.x)
				(start.x, end.x) = (end.x, start.x);
			if (start.y > end.y)
				(start.y, end.y) = (end.y, start.y);
		}

		if (tool == 3) // rectangle of random tiles
			tilemap.SetArea((start.x, start.y, Math.Abs(szw), Math.Abs(szh)), tiles);
		else if (tool == 4) // line of random tiles
			tilemap.SetLine(start, (end.x - 1, end.y - 1), tiles);
		else if (tool is 5 or 6) // ellipse of random tiles
		{
			var center = ((Point)start).PercentTo(50f, (end.x - 1, end.y - 1));
			var radius = ((int)((end.x - start.x - 1) / 2f), (int)((end.y - start.y - 1) / 2f));

			tilemap.SetEllipse(center, radius, tool == 5, tiles);
		}
		else if (tool == 7) // replace
			tilemap.Replace((0, 0, tw, th), tilemap.TileAt(start), tiles);
		else if (tool == 8) // fill
			tilemap.Flood((mx, my), true, tiles);
		else if (tool == 9) // rotate
			ProcessRegion(tile => tile.Rotate(1));
		else if (tool == 10) // mirror
			ProcessRegion(tile => tile.FlipHorizontally());
		else if (tool == 11) // flip
			ProcessRegion(tile => tile.FlipVertically());
		else if (tool == 12) // color
			ProcessRegion(tile => new(tile.Id, inspector.paletteColor.SelectedColor, tile.Pose));
		else if (tool == 13) // pick
		{
			var tile = tilemap.TileAt((mx, my));
			var coords = ((int)tile.Id).ToIndexes(layerTiles.AtlasTileCount);
			inspector.paletteColor.SelectedColor = tile.Tint;
			inspector.pickedTile = tile;
			justPickedTile = true;
			// inspector.mirror.IsSelected = tile.IsMirrored;
			// inspector.flip.IsSelected = tile.IsFlipped;
			// inspector.tileTurns = tile.Turns;
			selectedPos = coords;
			selectedSz = (1, 1);
		}
		else if (tool == 14 && isDraggingTiles && draggingTiles != null) // select
			tilemap.SetTiles(start, draggingTiles);

		if (tool != 14)
			start = end;

		void ProcessRegion(Func<Tile, Tile> editTile)
		{
			if (inspector == null)
				return;

			var region = tilemap.TilesIn((start.x, start.y, Math.Abs(szw), Math.Abs(szh)));
			for (var i = 0; i < region.GetLength(1); i++)
				for (var j = 0; j < region.GetLength(0); j++)
					region[j, i] = editTile.Invoke(region[j, i]);

			tilemap.SetTiles(start, region);
		}
	}
	private void OnMouseHold(Tile randomTile, TileMap? tilemap)
	{
		if (IsPaintAllowed() == false)
			return;

		var (mx, my) = ((int)editor.MousePositionWorld.x, (int)editor.MousePositionWorld.y);
		var pos = (mx, my);
		var tool = inspector.tools.Current;
		var lmb = Mouse.Button.Left.IsPressed();
		var (szw, szh) = (end.x - start.x, end.y - start.y);
		var (offX, offY) = (1, 1);
		var pose = inspector.pose.Items.IndexOf(inspector.pose.SelectedItems[0]);
		randomTile.Tint = inspector.paletteColor.SelectedColor;
		randomTile.Pose = (Pose)pose;

		if (inspector.tools.Current == 14) // select
		{
			if (isDrawingSelection == false && isDraggingSelection == false)
				return;

			if (isDraggingSelection)
			{
				var (dx, dy) = ((int)prevMousePosWorld.x - mx, (int)prevMousePosWorld.y - my);
				start = (start.x - dx, start.y - dy);
				end = (end.x - dx, end.y - dy);
				return;
			}
		}

		if (rectangleTools.Contains(inspector.tools.Current))
		{
			offX = szw > 0 ? offX : 0;
			offY = szh > 0 ? offY : 0;
		}

		end = (mx + offX, my + offY);
		if (end.x - start.x == 0)
			end = (end.x + 1, end.y);
		if (end.y - start.y == 0)
			end = (end.x, end.y + 1);

		if (tool == 13) // pick
		{
			start = (mx, my);
			end = (mx + 1, my + 1);
		}

		if (lmb && tool == 1) // group of tiles
			tilemap?.SetTiles(pos, GetSelectedTiles());
		else if (lmb && tool == 2) // single random tile of tiles
			tilemap?.SetTile(pos, randomTile);
	}
#endregion
}