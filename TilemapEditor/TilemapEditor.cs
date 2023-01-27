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
		private readonly RenderWindow map, set;
		private readonly Timer loop;
		private readonly VertexArray vertsPreview = new(PrimitiveType.Quads);
		private readonly Vertex[] versSelection = new Vertex[4];
		private VertexBuffer? vertsTileset;
		private Texture? tileset;

		private int updateCount;
		private float mapZoom = 1f, setZoom = 1f;
		private Vector2 prevFormsMousePosMap, prevFormsMousePosSet;
		private Vector2 selectedTile, selectedTileSquare, previewTile, previewTileSquare;
		private bool isSquaring, isCreatingLayer;
		private readonly Clock delta = new();
		private float fps;

		public Window()
		{
			InitializeComponent();
			CenterToScreen();

			Layers.KeyDown += OnKeyPress; ;

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

		#region Actions
		private void PaintTiles()
		{
			if(Layers.Items.Count == 0)
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
						isSquaring ? random.Next(0, (int)MathF.Abs(selectedSz.X) + 1) : x - p.X;
					var offY = isSingleTile ? 0 :
						isSquaring ? random.Next(0, (int)MathF.Abs(selectedSz.Y) + 1) : y - p.Y;
					var tex = selectedTile + new Vector2(offX, offY);
					var tile = tex.X * tileCount.X + tex.Y;

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
			return new((int)coords.X, (int)coords.Y);
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
		private static bool IsHoveringArea(Vector2 pos, Vector2 size)
		{
			return pos.X > 0 && pos.X < size.X &&
				pos.Y > 0 && pos.Y < size.Y;
		}

		private static void TryScrollView(RenderWindow window, ref float zoom, float delta,
			Vector2 limit)
		{
			delta = delta > 0 ? -0.05f : 0.05f;
			var pos = ToSystemVector(window.GetView().Center);
			var mousePos = ToSystemVector(GetMousePosition(window));

			if(delta < 0)
			{
				pos = Vector2.Lerp(pos, mousePos, 0.05f);
				SetViewPosition(window, limit, pos);
			}
			SetViewZoom(window, ref zoom, zoom + delta);
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
			scale = Math.Clamp(scale, 0.1f, 3f);
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

			versSelection[0] = new(tl, c);
			versSelection[1] = new(tr, c);
			versSelection[2] = new(br, c);
			versSelection[3] = new(bl, c);

			set.Draw(versSelection, PrimitiveType.Quads);
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
			var c = ToSFMLColor(ColorBrush.BackColor);
			var mapCount = new Vector2i((int)MapWidth.Value, (int)MapHeight.Value);
			var random = new Random((int)(p.X * p.Y * MathF.PI));
			var isSingleTile = selectedSz.X + 1 == 1 && selectedSz.Y + 1 == 1;

			sz += new Vector2(stepX, stepY);
			c.A = 150;

			for(float x = p.X; x != p.X + sz.X; x += stepX)
				for(float y = p.Y; y != p.Y + sz.Y; y += stepY)
				{
					if(x < 0 || x >= mapCount.X ||
						y < 0 || y >= mapCount.Y)
						continue;

					var offX = isSingleTile ? 0 :
						isSquaring ? random.Next(0, (int)MathF.Abs(selectedSz.X) + 1) : x - p.X;
					var offY = isSingleTile ? 0 :
						isSquaring ? random.Next(0, (int)MathF.Abs(selectedSz.Y) + 1) : y - p.Y;
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
			var h = new Vector2(float.NaN, float.NaN);
			var hoveredTile = "";
			var hoveredPos = "";
			var tileCount = GetTilesetTileCount();

			if(IsHoveringMap())
			{
				h = GetHoveredCoords(map);

				if(float.IsNaN(h.X) == false && float.IsNaN(h.Y) == false)
					hoveredPos = $"X[{h.X:F1}] Y[{h.Y:F1}] ";
			}
			else if(IsHoveringSet() && tileset != null)
			{
				h = GetHoveredCoords(set);

				if(float.IsNaN(h.X) == false && float.IsNaN(h.Y) == false)
					hoveredTile = $"Tile[{CoordsToIndex((int)h.Y, (int)h.X, tileCount.X)}] ";
			}

			TileHovered.Text =
				$"{hoveredTile}" +
				$"{hoveredPos}" +
				$"FPS[{fps:F1}]";
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
			if(e.KeyCode == Keys.Delete)
				OnLayerRemove(this, new());
		}

		private void OnNumericValueChange(object sender, EventArgs e)
		{
			LimitView(map, GetMapSize());
			LimitView(set, GetTilesetSize());
		}

		private void OnMapMouseMove(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Middle)
				TryMoveView(map, GetMapSize(), prevFormsMousePosMap);

			prevFormsMousePosMap = GetFormsMousePos(map);

			if(e.Button == MouseButtons.Right)
				previewTileSquare = GetHoveredCoordsRounded(map);
			else
				previewTile = GetHoveredCoordsRounded(map);

			if(e.Button == MouseButtons.Left)
				PaintTiles();
		}
		private void OnMapScroll(object? sender, MouseEventArgs e)
		{
			TryScrollView(map, ref mapZoom, e.Delta, GetMapSize());
		}
		private void OnMapPress(object sender, MouseEventArgs e)
		{
			if(Layers.Items.Count == 0)
				return;

			if(e.Button == MouseButtons.Right)
			{
				previewTileSquare = previewTile;
				isSquaring = true;
			}
			else if(e.Button == MouseButtons.Left)
				PaintTiles();
		}
		private void OnMapRelease(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Right)
			{
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
			RecreateTilesetVerts();
		}
		private void OnSetPress(object sender, MouseEventArgs e)
		{
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

		private void OnLayerPress(object sender, MouseEventArgs e)
		{
			var clickedIndex = Layers.IndexFromPoint(e.Location);
			if(clickedIndex != ListBox.NoMatches)
				Layers.SelectedIndex = clickedIndex;

			var move = (ToolStripMenuItem)LayerMenu.Items[3];
			var isFirst = Layers.SelectedIndex == 0;
			var isLast = Layers.SelectedIndex == Layers.Items.Count - 1;

			move.Enabled = Layers.Items.Count > 1;
			LayerMenu.Items[4].Enabled = Layers.Items.Count > 1;
			move.DropDownItems[0].Enabled = isFirst == false;
			move.DropDownItems[1].Enabled = isFirst == false;
			move.DropDownItems[2].Enabled = isLast == false;
			move.DropDownItems[3].Enabled = isLast == false;
		}
		private void OnLayerAdd(object sender, EventArgs e)
		{
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

					// update
					var check = Layers.GetItemChecked(0);
					Layers.SetItemChecked(0, true);
					Layers.SetItemChecked(0, false);
					Layers.SetItemChecked(0, check);
				}
			};

			form.Controls.Add(input);
			form.ShowDialog();
		}
		private void OnLayerRemove(object sender, EventArgs e)
		{
			if(Layers.Items.Count == 1)
				return;

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