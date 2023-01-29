using System.Numerics;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

using Color = System.Drawing.Color;
using Cursor = System.Windows.Forms.Cursor;
using TextBox = System.Windows.Forms.TextBox;
using Timer = System.Windows.Forms.Timer;

namespace TilemapEditor
{
	public partial class Window : Form
	{
		private readonly Dictionary<Vector2i, (int, SFML.Graphics.Color)> copiedTiles = new();
		private readonly Dictionary<(bool, Keys), (Action<object, EventArgs>, string)> hotkeys = new();
		private readonly RenderWindow map, set;
		private readonly Timer loop;
		private readonly VertexArray vertsPreview = new(PrimitiveType.Quads);
		private readonly Vertex[] vertsSelection = new Vertex[4];
		private VertexBuffer? vertsTileset;
		private Texture? tileset;

		private int updateCount;
		private float mapZoom = 1f, setZoom = 1f;
		private Vector2 prevFormsMousePosMap, prevFormsMousePosSet,
			selectedTile, selectedTileSquare, previewTile, previewTileSquare;
		private bool isSquaring, isCreatingLayer, isSquaringCanceled;
		private readonly Clock delta = new();
		private float fps;
		private string hotkeyDescriptions = "";

		public Window()
		{
			InitializeComponent();
			CenterToScreen();
			InitializeHotkeys();

			KeyDown += OnKeyPress;

			map = new RenderWindow(Map.Handle);
			set = new RenderWindow(Set.Handle);

			map.SetVerticalSyncEnabled(true);
			set.SetVerticalSyncEnabled(true);

			Map.MouseWheel += OnMapScroll;
			Set.MouseWheel += OnSetScroll;

			loop = new() { Interval = 1 };
			loop.Tick += OnUpdate;
			loop.Start();

			var sz = GetMapSize();
			SetViewPosition(map, sz, new(sz.X / 2, sz.Y / 2));

			TileWidth.ValueChanged += (s, e) => RecreateTilesetVerts();
			TileHeight.ValueChanged += (s, e) => RecreateTilesetVerts();
			TileOffsetWidth.ValueChanged += (s, e) => RecreateTilesetVerts();
			TileOffsetHeight.ValueChanged += (s, e) => RecreateTilesetVerts();

			map.Resized += (s, e) => UpdateZoom();
			set.Resized += (s, e) => UpdateZoom();

			void UpdateZoom()
			{
				SetViewZoom(map, ref mapZoom, mapZoom);
				SetViewZoom(set, ref setZoom, setZoom);
			}
		}

		private void InitializeHotkeys()
		{
			hotkeys.Add((false, Keys.B), (OnLayerAdd, "Brush color"));

			hotkeys.Add((false, Keys.A), (OnLayerAdd, "Tilemap add"));
			hotkeys.Add((true, Keys.L), (OnLayerLoad, "Tilemap load"));

			hotkeys.Add((false, Keys.R), (OnLayerRename, "Selected tilemap rename"));
			hotkeys.Add((false, Keys.V), (OnLayerToggle, "Selected tilemap toggle visibility"));
			hotkeys.Add((false, Keys.Delete), (OnLayerRemove, "Selected tilemap remove"));
			hotkeys.Add((true, Keys.S), (OnLayerSave, "Selected tilemap save"));

			hotkeys.Add((false, Keys.Q), (OnLayerMoveTop, "Selected tilemap move top"));
			hotkeys.Add((false, Keys.W), (OnLayerMoveUp, "Selected tilemap move up"));
			hotkeys.Add((false, Keys.S), (OnLayerMoveDown, "Selected tilemap move down"));
			hotkeys.Add((false, Keys.E), (OnLayerMoveBottom, "Selected tilemap move bottom"));

			hotkeys.Add((true, Keys.C), (CopySquaredTiles, "Squared tilemap tiles copy"));
			hotkeys.Add((true, Keys.V), (PasteSquaredTiles, "Squared tilemap tiles paste"));

			var separators = new List<(bool, Keys)>()
			{
				(false, Keys.B), (true, Keys.L), (false, Keys.E), (true, Keys.V)
			};
			var newLines = new List<(bool, Keys)>() { (true, Keys.S) };
			foreach(var kvp in hotkeys)
			{
				var ctrl = kvp.Key.Item1 ? "Ctrl + " : "";
				hotkeyDescriptions += $"{kvp.Value.Item2} [{ctrl}{kvp.Key.Item2}]\n";

				if(newLines.Contains(kvp.Key))
					hotkeyDescriptions += "\n";
				else if(separators.Contains(kvp.Key))
					hotkeyDescriptions += "- - - - - - - - - - - - - - - - - - - - - - - -\n";
			}

			hotkeyDescriptions +=
				"On Tilemap:\n" +
				"[LMB] Draw selected tile with brush color\n" +
				"[RMB] Draw selected tile square with brush color\n" +
				"On Tileset:\n" +
				"[LMB] Select tile square\n" +
				"[RMB] Focus on tile square";
		}

		#region Actions
		private void CopySquaredTiles(object s, EventArgs e)
		{
			if(Layers.Items.Count == 0 || tileset == null)
				return;

			copiedTiles.Clear();

			var layer = (Layer)Layers.SelectedItem;
			var p = previewTile;
			var selectedSz = selectedTileSquare - selectedTile;
			var sz = selectedSz;
			if(isSquaring)
				sz = previewTileSquare - previewTile;
			var isSingleTile = selectedSz.X + 1 == 1 && selectedSz.Y + 1 == 1;
			var stepX = sz.X < 0 ? -1 : 1;
			var stepY = sz.Y < 0 ? -1 : 1;
			var (mapW, mapH) = ((int)MapWidth.Value, (int)MapHeight.Value);

			sz += new Vector2(stepX, stepY);
			isSquaring = false;
			isSquaringCanceled = true;

			var i = 0;
			for(float x = p.X; x != p.X + sz.X; x += stepX)
				for(float y = p.Y; y != p.Y + sz.Y; y += stepY)
				{
					if(i > Math.Abs(mapW * mapH))
						return;

					var pos = new Vector2i((int)x, (int)y);
					var tile = layer.GetTile(pos);
					var color = layer.GetColor(pos);

					pos.X -= (int)p.X;
					pos.Y -= (int)p.Y;
					copiedTiles.Add(pos, (tile, color));
					i++;
				}
		}
		private void PasteSquaredTiles(object s, EventArgs e)
		{
			if(Layers.Items.Count == 0 || tileset == null)
				return;

			var layer = (Layer)Layers.SelectedItem;
			var pos = new Vector2i((int)previewTile.X, (int)previewTile.Y);
			foreach(var kvp in copiedTiles)
			{
				var p = new Vector2i(pos.X + kvp.Key.X, pos.Y + kvp.Key.Y);
				layer.SetTile(p, kvp.Value.Item1, kvp.Value.Item2);
			}
		}

		private void PaintTiles()
		{
			if(Layers.Items.Count == 0 || tileset == null)
				return;

			var layer = (Layer)Layers.SelectedItem;
			var p = previewTile;
			var col = ToSFMLColor(ColorBrush.BackColor);
			var selectedSz = selectedTileSquare - selectedTile;
			var sz = selectedSz;
			if(isSquaring)
				sz = previewTileSquare - previewTile;
			var stepX = sz.X < 0 ? -1 : 1;
			var stepY = sz.Y < 0 ? -1 : 1;
			var tStepX = selectedSz.X < 0 ? -1 : 1;
			var tStepY = selectedSz.Y < 0 ? -1 : 1;
			var random = new Random((int)(p.X * p.Y * MathF.PI));
			var isSingleTile = selectedSz.X + 1 == 1 && selectedSz.Y + 1 == 1;
			var tileCount = GetTilesetTileCount();
			var (mapW, mapH) = ((int)MapWidth.Value, (int)MapHeight.Value);

			sz += new Vector2(stepX, stepY);

			var i = 0;
			for(float x = p.X; x != p.X + sz.X; x += stepX)
				for(float y = p.Y; y != p.Y + sz.Y; y += stepY)
				{
					if(i > Math.Abs(mapW * mapH))
						return;

					var offX = isSingleTile ? 0 :
						isSquaring ? random.Next(0, (int)MathF.Abs(selectedSz.X) + 1) * tStepX : x - p.X;
					var offY = isSingleTile ? 0 :
						isSquaring ? random.Next(0, (int)MathF.Abs(selectedSz.Y) + 1) * tStepY : y - p.Y;
					var tex = selectedTile + new Vector2(offX, offY);
					var tile = tex.Y * tileCount.X + tex.X;

					layer.SetTile(new((int)x, (int)y), (int)tile, col);
					i++;
				}
		}
		private void EnableTilesetOptions(bool enabled)
		{
			MapWidth.Enabled = enabled;
			MapHeight.Enabled = enabled;
			TileWidth.Enabled = enabled;
			TileHeight.Enabled = enabled;
			TileOffsetWidth.Enabled = enabled;
			TileOffsetHeight.Enabled = enabled;
		}
		private void MoveLayerToIndex(int index)
		{
			if(index < 0 || index >= Layers.Items.Count)
				return;

			var item = Layers.SelectedItem;
			var isChecked = Layers.GetItemChecked(Layers.SelectedIndex);
			Layers.Items.RemoveAt(Layers.SelectedIndex);
			Layers.Items.Insert(index, item);
			Layers.SetItemChecked(index, isChecked);
			Layers.SelectedIndex = index;
		}

		private Vector2 GetHoveredCoords(RenderWindow window)
		{
			var pos = GetMousePosition(window);
			var tileWidth = (float)TileWidth.Value;
			var tileHeight = (float)TileHeight.Value;
			return new(pos.X / tileWidth, pos.Y / tileHeight);
		}
		private Vector2 GetHoveredCoordsRounded(RenderWindow window)
		{
			var coords = GetHoveredCoords(window);
			return new(MathF.Floor(coords.X), MathF.Floor(coords.Y));
		}
		private bool IsHoveringSet()
		{
			var pos = GetMousePosition(set);
			return IsHoveringArea(new(pos.X, pos.Y), GetTilesetSize());
		}
		private bool IsHoveringMap()
		{
			var pos = GetMousePosition(map);
			return IsHoveringArea(new(pos.X, pos.Y), GetMapSize());
		}
		private static bool IsHovering(Control control)
		{
			return control != null &&
				control.ClientRectangle.Contains(control.PointToClient(Cursor.Position));
		}
		private static bool IsHoveringArea(Vector2 pos, Vector2 size)
		{
			return pos.X > 0 && pos.X < size.X &&
				pos.Y > 0 && pos.Y < size.Y;
		}

		private static void TryScrollView(RenderWindow window, ref float zoom, float delta, Vector2 limit)
		{
			var prevDelta = delta;
			var pos = ToSystemVector(window.GetView().Center);
			var mousePos = ToSystemVector(GetMousePosition(window));
			var newPos = Vector2.Lerp(pos, mousePos, 0.05f);

			delta = delta > 0 ? 0.95f : 1.05f;
			SetViewZoom(window, ref zoom, zoom * delta);

			if(prevDelta > 0)
				SetViewPosition(window, limit, newPos);
		}
		private static void TryMoveView(RenderWindow window, Vector2 limit, Vector2 prevMousePos)
		{
			var pos = ToSystemVector(window.GetView().Center);

			SetViewPosition(window, limit, Drag(window, pos, prevMousePos));
			Cursor.Current = Cursors.SizeAll;
		}

		private static void SetViewPosition(RenderWindow window, Vector2 limit, Vector2 pos)
		{
			var view = window.GetView();
			view.Center = new(pos.X, pos.Y);
			window.SetView(view);
			LimitView(window, limit);
		}
		private static void SetViewZoom(RenderWindow window, ref float zoom, float scale = 1f)
		{
			scale = MathF.Max(scale, 0.02f);
			zoom = scale;
			var view = window.GetView();
			var w = RoundToMultipleOfTwo((int)(window.Size.X * scale));
			var h = RoundToMultipleOfTwo((int)(window.Size.Y * scale));
			view.Size = new(w, h);
			window.SetView(view);
		}
		private static Vector2 Drag(RenderWindow window, Vector2 mousePos, Vector2 prevMousePos)
		{
			var pos = GetFormsMousePos(window);
			var dir = prevMousePos - pos;
			return mousePos + dir;
		}

		private void DrawSelectedTile()
		{
			var tileSz = new Vector2f((float)TileWidth.Value, (float)TileHeight.Value);
			var s = selectedTile;
			var sq = selectedTileSquare;

			if(sq.X < s.X)
				(sq.X, s.X) = (s.X, sq.X);
			if(sq.Y < s.Y)
				(sq.Y, s.Y) = (s.Y, sq.Y);

			var tl = new Vector2f((int)s.X * tileSz.X, (int)s.Y * tileSz.Y);
			var br = new Vector2f(sq.X * tileSz.X + tileSz.X, sq.Y * tileSz.Y + tileSz.Y);
			var tr = new Vector2f(br.X, tl.Y);
			var bl = new Vector2f(tl.X, br.Y);
			var c = ToSFMLColor(ColorSelection.BackColor);
			c.A = 150;

			vertsSelection[0] = new(tl, c);
			vertsSelection[1] = new(tr, c);
			vertsSelection[2] = new(br, c);
			vertsSelection[3] = new(bl, c);

			set.Draw(vertsSelection, PrimitiveType.Quads);
		}
		private void DrawGrid(RenderWindow window, int width, int height)
		{
			var cellVerts = new VertexArray(PrimitiveType.Lines);
			var grid1Color = ToSFMLColor(ColorGrid1.BackColor);
			var grid5Color = ToSFMLColor(ColorGrid5.BackColor);
			var tileSize = new Vector2i((int)TileWidth.Value, (int)TileHeight.Value);
			var view = window.GetView();
			var viewTopLeft = view.Center - view.Size / 2f;
			var viewBottomRight = view.Center + view.Size / 2f;

			for(int i = 0; i <= width; i++)
			{
				var top = new Vector2f(i * tileSize.X, 0);
				if(top.X < viewTopLeft.X || top.X > viewBottomRight.X)
					continue;

				var bot = new Vector2f(top.X, tileSize.Y * height);
				var col = GetColor(top.X);

				cellVerts.Append(new(top, col));
				cellVerts.Append(new(bot, col));
			}
			for(int i = 0; i <= height; i++)
			{
				var left = new Vector2f(0, i * tileSize.Y);
				if(left.Y < viewTopLeft.Y || left.Y > viewBottomRight.Y)
					continue;

				var right = new Vector2f(tileSize.X * width, left.Y);
				var col = GetColor(left.Y);

				cellVerts.Append(new(left, col));
				cellVerts.Append(new(right, col));
			}

			window.Draw(cellVerts);

			SFML.Graphics.Color GetColor(float coordinate)
			{
				return coordinate % 5 == 0 ? grid5Color : grid1Color;
			}
		}
		private void DrawPreview()
		{
			vertsPreview.Clear();
			var selectedSz = selectedTileSquare - selectedTile;
			var sz = selectedSz;

			if(isSquaring)
				sz = previewTileSquare - previewTile;

			var p = previewTile;
			var tileSz = new Vector2((float)TileWidth.Value, (float)TileHeight.Value);
			var tileOff = new Vector2((float)TileOffsetWidth.Value, (float)TileOffsetHeight.Value);
			var stepX = sz.X < 0 ? -1 : 1;
			var stepY = sz.Y < 0 ? -1 : 1;
			var tStepX = selectedSz.X < 0 ? -1 : 1;
			var tStepY = selectedSz.Y < 0 ? -1 : 1;
			var c = ToSFMLColor(ColorBrush.BackColor);
			var mapCount = new Vector2i((int)MapWidth.Value, (int)MapHeight.Value);
			var random = new Random((int)(p.X * p.Y * MathF.PI));
			var isSingleTile = selectedSz.X + 1 == 1 && selectedSz.Y + 1 == 1;
			var multipleTiles = isSquaring || isSingleTile == false;
			var off = new Vector2f(
				sz.X >= 0 && multipleTiles ? 0 : 1,
				sz.Y >= 0 && multipleTiles ? 0 : 1);
			var selTl = new Vector2f(
				(multipleTiles ? (p.X + off.X) : p.X) * tileSz.X,
				(multipleTiles ? (p.Y + off.Y) : p.Y) * tileSz.Y);
			var selBr = new Vector2f(
				((multipleTiles ? p.X + sz.X - off.X : p.X) + 1) * tileSz.X,
				((multipleTiles ? p.Y + sz.Y - off.Y : p.Y) + 1) * tileSz.Y);
			var selTr = new Vector2f(selBr.X, selTl.Y);
			var selBl = new Vector2f(selTl.X, selBr.Y);
			var selCol = ToSFMLColor(ColorSelection.BackColor);

			sz += new Vector2(stepX, stepY);
			selCol.A = 150;

			vertsSelection[0] = new(selTl, selCol);
			vertsSelection[1] = new(selTr, selCol);
			vertsSelection[2] = new(selBr, selCol);
			vertsSelection[3] = new(selBl, selCol);

			for(float x = p.X; x != p.X + sz.X; x += stepX)
				for(float y = p.Y; y != p.Y + sz.Y; y += stepY)
				{
					if(x < 0 || x >= mapCount.X ||
						y < 0 || y >= mapCount.Y)
						continue;

					var offX = isSingleTile ? 0 :
						isSquaring ? random.Next(0, (int)MathF.Abs(selectedSz.X) + 1) * tStepX : x - p.X;
					var offY = isSingleTile ? 0 :
						isSquaring ? random.Next(0, (int)MathF.Abs(selectedSz.Y) + 1) * tStepY : y - p.Y;

					var tex = (selectedTile + new Vector2(offX, offY)) * (tileSz + tileOff);
					var texTl = new Vector2f(tex.X, tex.Y);
					var texBr = new Vector2f(texTl.X + tileSz.X, texTl.Y + tileSz.Y);
					var texTr = new Vector2f(texBr.X, texTl.Y);
					var texBl = new Vector2f(texTl.X, texBr.Y);

					var tl = new Vector2f(x * tileSz.X, y * tileSz.Y);
					var br = new Vector2f(tl.X + tileSz.X, tl.Y + tileSz.Y);
					var tr = new Vector2f(br.X, tl.Y);
					var bl = new Vector2f(tl.X, br.Y);

					vertsPreview.Append(new(tl, c, texTl));
					vertsPreview.Append(new(tr, c, texTr));
					vertsPreview.Append(new(br, c, texBr));
					vertsPreview.Append(new(bl, c, texBl));
				}

			map.Draw(vertsSelection, PrimitiveType.Quads);
			map.Draw(vertsPreview, new(tileset));
		}

		private void RecreateTilesetVerts()
		{
			vertsTileset?.Dispose();
			selectedTile = new();
			selectedTileSquare = new();

			var count = GetTilesetTileCount();
			var tileSz = new Vector2f((float)TileWidth.Value, (float)TileHeight.Value);
			var tileOff = new Vector2f((float)TileOffsetWidth.Value, (float)TileOffsetHeight.Value);
			var vertCount = (uint)(count.X * count.Y * 4);
			vertsTileset = new(vertCount, PrimitiveType.Quads, VertexBuffer.UsageSpecifier.Static);

			var verts = new Vertex[vertCount];
			var index = 0;
			var c = SFML.Graphics.Color.White;
			for(int j = 0; j < count.Y; j++)
				for(int i = 0; i < count.X; i++)
				{
					var texTl = new Vector2f(i * (tileSz.X + tileOff.X), j * (tileSz.Y + tileOff.Y));
					var texBr = new Vector2f(texTl.X + tileSz.X, texTl.Y + tileSz.Y);
					var texTr = new Vector2f(texBr.X, texTl.Y);
					var texBl = new Vector2f(texTl.X, texBr.Y);

					var tl = new Vector2f(i * tileSz.X, j * tileSz.Y);
					var br = new Vector2f(tl.X + tileSz.X, tl.Y + tileSz.Y);
					var tr = new Vector2f(br.X, tl.Y);
					var bl = new Vector2f(tl.X, br.Y);

					verts[index + 0] = new(tl, c, texTl);
					verts[index + 1] = new(tr, c, texTr);
					verts[index + 2] = new(br, c, texBr);
					verts[index + 3] = new(bl, c, texBl);

					index += 4;
				}
			vertsTileset.Update(verts);
		}
		private void UpdateStats()
		{
			var tileCount = GetTilesetTileCount();
			var isHoveringSet = IsHoveringSet() && IsHovering(Set);
			var isHoveringMap = IsHoveringMap() && IsHovering(Map);

			if(tileset == null)
			{
				Stats.Text = "Adjust the parameters and load a tileset.";
				return;
			}

			// assume user is hovering map
			var layer = (Layer)Layers.SelectedItem;
			var pos = GetHoveredCoords(map);
			var tile = layer?.GetTile(new((int)pos.X, (int)pos.Y));

			// unless
			if(isHoveringSet)
			{
				pos = GetHoveredCoords(set);
				tile = CoordsToIndex((int)pos.Y, (int)pos.X, tileCount.X);
			}
			var stats = $"Tile[{tile}] X[{pos.X:F1}] Y[{pos.Y:F1}] ";

			// not hovering any
			if(isHoveringSet == false && isHoveringMap == false)
				stats = "";

			Stats.Text = $"{stats}FPS[{fps:F1}]";
		}
		private static Vector2f GetMousePosition(RenderWindow window)
		{
			var p = Mouse.GetPosition(window);
			var result = window.MapPixelToCoords(new(p.X, p.Y), window.GetView());
			return result;
		}
		private static Vector2 GetFormsMousePos(RenderWindow window)
		{
			var view = window.GetView();
			var sz = window.Size;
			var scale = ToSystemVector(view.Size) / new Vector2(sz.X, sz.Y);
			return new Vector2(MousePosition.X, MousePosition.Y) * scale;
		}

		private static void LimitView(RenderWindow window, Vector2 limit)
		{
			var view = window.GetView();
			var pos = view.Center;
			pos.X = Math.Clamp(pos.X, 0, limit.X);
			pos.Y = Math.Clamp(pos.Y, 0, limit.Y);
			view.Center = pos;
			window.SetView(view);
		}
		#endregion
		#region Utilities
		private static Vector2 ToSystemVector(Vector2f vec)
		{
			return new(vec.X, vec.Y);
		}

		private static SFML.Graphics.Color ToSFMLColor(System.Drawing.Color color)
		{
			return new(color.R, color.G, color.B);
		}

		private Vector2 GetMapSize()
		{
			var w = MapWidth.Value * TileWidth.Value;
			var h = MapHeight.Value * TileHeight.Value;
			return new((float)w, (float)h);
		}
		private Vector2i GetTilesetTileCount()
		{
			if(tileset == null)
				return default;

			var width = tileset.Size.X / (uint)(TileWidth.Value + TileOffsetWidth.Value);
			var height = tileset.Size.Y / (uint)(TileHeight.Value + TileOffsetHeight.Value);
			return new((int)width, (int)height);
		}
		private Vector2 GetTilesetSize()
		{
			var count = GetTilesetTileCount();
			return new(count.X * (float)TileWidth.Value, count.Y * (float)TileHeight.Value);
		}

		private static int CoordsToIndex(int x, int y, int width)
		{
			return x * width + y;
		}
		private static int RoundToMultipleOfTwo(int n)
		{
			var rem = n % 2;
			var result = n - rem;
			if(rem >= 1)
				result += 2;
			return result;
		}
		#endregion
		#region Events
		private void OnUpdate(object? sender, EventArgs e)
		{
			map.Size = new((uint)Map.Width, (uint)Map.Height);
			map.DispatchEvents();
			map.Clear(ToSFMLColor(ColorBackground.BackColor));

			if(tileset != null)
				for(int i = 0; i < Layers.Items.Count; i++)
				{
					var item = (Layer)Layers.Items[i];
					if(Layers.GetItemChecked(i) == false)
						continue;

					item.Draw(map, tileset);
				}
			if(mapZoom < 5 || tileset == null)
				DrawGrid(map, (int)MapWidth.Value, (int)MapHeight.Value);
			if(tileset != null && (IsHoveringMap() || isSquaring))
				DrawPreview();

			map.Display();

			set.Size = new((uint)Set.Width, (uint)Set.Height);
			set.DispatchEvents();
			set.Clear(ToSFMLColor(ColorBackground.BackColor));

			if(tileset != null)
			{
				var sz = GetTilesetTileCount();
				set.Draw(vertsTileset, new(tileset));
				if(setZoom < 5 || tileset == null)
					DrawGrid(set, sz.X, sz.Y);
				DrawSelectedTile();
			}

			set.Display();

			if(updateCount % 10 == 0)
				fps = delta.ElapsedTime.AsSeconds() * 60f * 60f;

			UpdateStats();

			delta.Restart();
			updateCount++;
		}
		private void OnKeyPress(object? sender, System.Windows.Forms.KeyEventArgs e)
		{
			foreach(var kvp in hotkeys)
			{
				var key = kvp.Key.Item2;
				var ctrl = e.Control && kvp.Key.Item1 && e.KeyCode == key;
				var noCtrl = e.Control == false && kvp.Key.Item1 == false && e.KeyCode == key;

				if(noCtrl || ctrl)
					kvp.Value.Item1.Invoke(this, new());
			}
		}

		private void OnNumericValueChange(object sender, EventArgs e)
		{
			LimitView(map, GetMapSize());
			LimitView(set, GetTilesetSize());
		}
		private void OnHotkeysClick(object sender, EventArgs e)
		{
			MessageBox.Show(
				hotkeyDescriptions,
				"Hotkeys",
				MessageBoxButtons.OK,
				MessageBoxIcon.Information);
		}

		private void OnMapMouseMove(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Middle)
				TryMoveView(map, GetMapSize(), prevFormsMousePosMap);

			prevFormsMousePosMap = GetFormsMousePos(map);

			var right = e.Button == MouseButtons.Right;
			if(right)
				previewTileSquare = GetHoveredCoordsRounded(map);

			if(right == false || isSquaringCanceled)
				previewTile = GetHoveredCoordsRounded(map);

			if(e.Button == MouseButtons.Left && IsHoveringMap())
				PaintTiles();
		}
		private void OnMapScroll(object? sender, MouseEventArgs e)
		{
			previewTile = GetHoveredCoordsRounded(map);
			isSquaring = false;
			isSquaringCanceled = true;

			TryScrollView(map, ref mapZoom, e.Delta, GetMapSize());
		}
		private void OnMapPress(object sender, MouseEventArgs e)
		{
			if(Layers.Items.Count == 0)
				return;

			if(e.Button == MouseButtons.Right)
			{
				previewTileSquare = previewTile;
				isSquaringCanceled = false;
				isSquaring = true;
			}
			else if(e.Button == MouseButtons.Left)
			{
				previewTile = GetHoveredCoordsRounded(map);

				// user is dragging a square with RMB and pressed LMB so cancel drag
				if(isSquaring)
				{
					isSquaringCanceled = true;
					isSquaring = false;
					return;
				}

				if(IsHoveringMap())
					PaintTiles();
			}
		}
		private void OnMapRelease(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Right)
			{
				// user pressed LMB while dragging - skip draw
				if(isSquaringCanceled)
				{
					isSquaringCanceled = false;
					return;
				}

				PaintTiles();
				isSquaring = false;
			}
		}

		private void OnSetMouseMove(object sender, MouseEventArgs e)
		{
			if(tileset == null)
				return;

			if(e.Button == MouseButtons.Left && IsHoveringSet())
				selectedTileSquare = GetHoveredCoordsRounded(set);
			else if(e.Button == MouseButtons.Middle)
				TryMoveView(set, GetTilesetSize(), prevFormsMousePosSet);

			prevFormsMousePosSet = GetFormsMousePos(set);
		}
		private void OnSetScroll(object? sender, MouseEventArgs e)
		{
			TryScrollView(set, ref setZoom, e.Delta, GetTilesetSize());
		}
		private void OnSetLoadClick(object sender, EventArgs e)
		{
			if(tileset != null)
			{
				var msg = MessageBox.Show(
					"Do you want to unload the tileset and choose another one?\n" +
					"Keep in mind this would unload all tilemaps as well.\n" +
					"Any unsaved changes will be lost.",
					"Unload Tilemap",
					MessageBoxButtons.YesNo);

				if(msg == DialogResult.No)
					return;

				Layers.Items.Clear();
				tileset.Dispose();
				tileset = null;

				EnableTilesetOptions(true);

				return;
			}

			if(LoadTileset.ShowDialog() != DialogResult.OK)
				return;

			try { tileset = new(LoadTileset.FileName); }
			catch(Exception)
			{
				MessageBox.Show(
					text: "Could not load the selected tileset.",
					caption: "Load Tileset",
					buttons: MessageBoxButtons.OK,
					icon: MessageBoxIcon.Error);

				return;
			}

			var sz = GetTilesetSize();

			OnLayerAdd(this, new());
			EnableTilesetOptions(false);
			SetViewPosition(set, sz, new(sz.X / 2, sz.Y / 2));
			SetViewZoom(set, ref setZoom);
			RecreateTilesetVerts();
		}
		private void OnSetPress(object sender, MouseEventArgs e)
		{
			Focus();

			if(e.Button == MouseButtons.Left && IsHoveringSet())
			{
				selectedTile = GetHoveredCoordsRounded(set);
				selectedTileSquare = selectedTile;
			}
			else if(e.Button == MouseButtons.Right)
			{
				var sz = selectedTileSquare - selectedTile;
				var tileSz = new Vector2((float)TileWidth.Value, (float)TileHeight.Value);
				sz += new Vector2(1, 1);
				var center = (selectedTile + sz / 2) * tileSz;
				var sc = new Vector2(MathF.Abs(sz.X - 1), MathF.Abs(sz.Y - 1)) / tileSz;

				SetViewPosition(set, GetTilesetSize(), center);
				SetViewZoom(set, ref setZoom, sc.X > sc.Y ? sc.X : sc.Y);
			}
		}

		private void OnLayerToggle(object s, EventArgs e)
		{
			if(Layers.SelectedItem == null)
				return;

			var index = Layers.SelectedIndex;
			Layers.SetItemChecked(index, Layers.GetItemChecked(index) == false);
		}
		private void OnLayerPress(object sender, MouseEventArgs e)
		{
			var clickedIndex = Layers.IndexFromPoint(e.Location);
			if(clickedIndex != ListBox.NoMatches)
				Layers.SelectedIndex = clickedIndex;

			var isNotFirst = Layers.SelectedIndex > 0;
			var isNotLast = Layers.SelectedIndex < Layers.Items.Count - 1;
			var hasTileset = tileset != null;
			var hasAnyLayers = Layers.Items.Count > 0;

			LayerMenuAdd.Enabled = hasTileset;
			LayerMenuLoad.Enabled = hasTileset;

			LayerMenuRename.Enabled = hasAnyLayers;
			LayerMenuSave.Enabled = hasAnyLayers;
			LayerMenuRemove.Enabled = hasAnyLayers;
			LayerMenuMove.Enabled = hasAnyLayers;
			LayerMenuMoveTop.Enabled = isNotFirst;
			LayerMenuMoveUp.Enabled = isNotFirst;
			LayerMenuMoveDown.Enabled = isNotLast;
			LayerMenuMoveBottom.Enabled = isNotLast;
		}
		private void OnLayerRelease(object sender, MouseEventArgs e)
		{
			if(tileset == null)
				LayerMenu.Hide();
		}
		private void OnLayerSave(object sender, EventArgs e)
		{
			var layer = (Layer)Layers.SelectedItem;
			if(layer == null)
				return;

			if(SaveTilemap.ShowDialog() != DialogResult.OK)
				return;

			layer.Save(SaveTilemap.FileName);
		}
		private void OnLayerLoad(object sender, EventArgs e)
		{
			if(tileset == null || LoadTilemap.ShowDialog() != DialogResult.OK)
				return;

			try
			{
				var tileSz = new Vector2i((int)TileWidth.Value, (int)TileHeight.Value);
				var tileOff = new Vector2i((int)TileOffsetWidth.Value, (int)TileOffsetHeight.Value);
				var layer = new Layer(LoadTilemap.FileName, GetTilesetTileCount(), tileSz, tileOff)
				{ name = Path.GetFileNameWithoutExtension(LoadTilemap.FileName) };

				Layers.Items.Add(layer);
				Layers.SelectedIndex = Layers.Items.Count - 1;
				Layers.SetItemChecked(Layers.Items.Count - 1, true);
			}
			catch(Exception)
			{
				MessageBox.Show(
					"Could not load tilemap.",
					"Load Tilemap",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		}
		private void OnLayerCollisionsSave(object sender, EventArgs e)
		{

		}
		private void OnLayerCollisionsLoad(object sender, EventArgs e)
		{

		}
		private void OnLayerAdd(object sender, EventArgs e)
		{
			if(tileset == null)
				return;

			var isFirst = Layers.Items.Count == 0;
			var tileSz = new Vector2i((int)TileWidth.Value, (int)TileHeight.Value);
			var tileOff = new Vector2i((int)TileOffsetWidth.Value, (int)TileOffsetHeight.Value);
			var sz = new Vector2i((int)MapWidth.Value, (int)MapHeight.Value);

			Layers.Items.Add(new Layer(sz, GetTilesetTileCount(), tileSz, tileOff));
			Layers.SelectedIndex = Layers.Items.Count - 1;
			Layers.SetItemChecked(Layers.Items.Count - 1, true);

			if(isFirst == false)
			{
				isCreatingLayer = true;
				OnLayerRename(this, new());
			}
		}
		private void OnLayerRename(object sender, EventArgs e)
		{
			var selected = (Layer)Layers.SelectedItem;
			if(selected == null)
				return;

			var rect = Layers.GetItemRectangle(Layers.SelectedIndex);
			var rSz = rect.Size;
			var rPos = rect.Location;
			var input = new TextBox()
			{
				Width = Layers.Width - 4,
				Text = selected.ToString(),
				Multiline = false,
				AcceptsReturn = false,
				AcceptsTab = false,
				BackColor = Color.Black,
				ForeColor = Color.Wheat
			};
			var form = new Form()
			{
				ShowInTaskbar = false,
				AutoScaleMode = AutoScaleMode.None,
				FormBorderStyle = FormBorderStyle.None,
				StartPosition = FormStartPosition.Manual,
				Location = Layers.PointToScreen(new(rPos.X + rSz.Width - input.Width, rPos.Y))
			};
			form.Load += (s, e) => form.ClientSize = new(input.Width, input.Height);

			input.KeyDown += (s, e) =>
			{
				if(e.KeyCode == Keys.Escape)
				{
					form.Close();

					if(isCreatingLayer)
						OnLayerRemove(this, new());

					isCreatingLayer = false;
					return;
				}

				var value = input.Text.Trim();

				if(e.KeyCode == Keys.Return && string.IsNullOrWhiteSpace(value) == false)
				{
					selected.name = value;
					form.Close();
					isCreatingLayer = false;

					Layers.Refresh();
				}
			};

			form.Controls.Add(input);
			form.ShowDialog();
		}
		private void OnLayerRemove(object sender, EventArgs e)
		{
			Layers.Focus();

			var prev = Layers.SelectedIndex;
			Layers.Items.Remove(Layers.SelectedItem);
			Layers.SelectedIndex = prev >= Layers.Items.Count ? Layers.Items.Count - 1 : prev;
		}
		private void OnLayerMoveTop(object sender, EventArgs e)
		{
			MoveLayerToIndex(0);
		}

		private void OnLayerMoveBottom(object sender, EventArgs e)
		{
			MoveLayerToIndex(Layers.Items.Count - 1);
		}
		private void OnLayerMoveUp(object sender, EventArgs e)
		{
			MoveLayerToIndex(Layers.SelectedIndex - 1);
		}
		private void OnLayerMoveDown(object sender, EventArgs e)
		{
			MoveLayerToIndex(Layers.SelectedIndex + 1);
		}

		private void OnColorBrushClick(object sender, EventArgs e)
		{
			Colors.Color = ColorBrush.BackColor;
			if(Colors.ShowDialog() == DialogResult.OK)
				ColorBrush.BackColor = Colors.Color;
		}
		private void OnColorSelectionClick(object sender, EventArgs e)
		{
			Colors.Color = ColorSelection.BackColor;
			if(Colors.ShowDialog() == DialogResult.OK)
				ColorSelection.BackColor = Colors.Color;
		}
		private void OnColorGrid1Click(object sender, EventArgs e)
		{
			Colors.Color = ColorGrid1.BackColor;
			if(Colors.ShowDialog() == DialogResult.OK)
				ColorGrid1.BackColor = Colors.Color;
		}
		private void OnColorGrid5Click(object sender, EventArgs e)
		{
			Colors.Color = ColorGrid5.BackColor;
			if(Colors.ShowDialog() == DialogResult.OK)
				ColorGrid5.BackColor = Colors.Color;
		}
		private void OnColorBackgroundClick(object sender, EventArgs e)
		{
			Colors.Color = ColorBackground.BackColor;
			if(Colors.ShowDialog() == DialogResult.OK)
				ColorBackground.BackColor = Colors.Color;
		}
		#endregion
	}
}