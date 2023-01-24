using System.Numerics;

using SFML.Graphics;
using SFML.System;
using SFML.Window;

using Cursor = System.Windows.Forms.Cursor;
using Timer = System.Windows.Forms.Timer;

namespace ImageEditor
{
	public partial class Window : Form
	{
		private Texture? tileset;
		private readonly RenderWindow map, set;
		private readonly Timer loop;
		private float mapZoom = 1f, setZoom = 1f;
		private Vector2 prevFormsMousePosMap, prevFormsMousePosSet;
		private VertexBuffer? vertsTileset, vertsMap;
		private Vector2 selectedTile, selectedTileSquare;

		public Window()
		{
			InitializeComponent();
			CenterToScreen();

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

			ResizeEnd += (s, e) =>
			{
				SetViewZoom(map, ref mapZoom, mapZoom);
				SetViewZoom(set, ref setZoom, setZoom);
			};
		}

		#region Actions
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
			view.Size = new(window.Size.X * scale, window.Size.Y * scale);
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
			var c = SFML.Graphics.Color.White;

			var verts = new Vertex[4]
			{
				new(tl, c),
				new(tr, c),
				new(br, c),
				new(bl, c),
			};
			set.Draw(verts, PrimitiveType.Quads);
		}
		private void DrawGrid(RenderWindow window, int width, int height)
		{
			var cellVerts = new VertexArray(PrimitiveType.Lines);
			var gridColor = ToSFMLColor(GridColor.BackColor);
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
				return coordinate % 10 == 0 ? SFML.Graphics.Color.White : gridColor;
			}
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
					var texTl = new Vector2f(i * tileSz.X + tileOff.X, j * tileSz.Y + tileOff.Y);
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
		private void UpdateHoveredTile()
		{
			var hoveredTilePos = new Vector2(float.NaN, float.NaN);
			var hoveredTile = "";
			var tileCount = GetTilesetTileCount();

			if(IsHoveringMap())
				hoveredTilePos = GetHoveredCoords(map);
			else if(IsHoveringSet() && tileset != null)
			{
				hoveredTilePos = GetHoveredCoords(set);
				var h = hoveredTilePos;
				hoveredTile = $"Tile[{CoordsToIndex((int)h.Y, (int)h.X, tileCount.X)}] ";
			}

			if(float.IsNaN(hoveredTilePos.X) || float.IsNaN(hoveredTilePos.Y))
			{
				TileHovered.Text = "";
				return;
			}

			TileHovered.Text = $"{hoveredTile}X[{hoveredTilePos.X:F1}] Y[{hoveredTilePos.Y:F1}]";
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
		#endregion
		#region Events
		private void OnMouseMoveMap(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Middle)
				TryMoveView(map, GetMapSize(), prevFormsMousePosMap);

			prevFormsMousePosMap = GetFormsMousePos(map);

		}
		private void OnMouseMoveSet(object sender, MouseEventArgs e)
		{
			if(tileset == null)
				return;

			if(e.Button == MouseButtons.Left && IsHoveringSet())
				selectedTileSquare = GetHoveredCoordsRounded(set);
			else if(e.Button == MouseButtons.Middle)
				TryMoveView(set, GetTilesetSize(), prevFormsMousePosSet);

			prevFormsMousePosSet = GetFormsMousePos(set);
		}

		private void OnMapScroll(object? sender, MouseEventArgs e)
		{
			TryScrollView(map, ref mapZoom, e.Delta, GetMapSize());
		}
		private void OnSetScroll(object? sender, MouseEventArgs e)
		{
			TryScrollView(set, ref setZoom, e.Delta, GetTilesetSize());
		}

		private void OnUpdate(object? sender, EventArgs e)
		{
			map.Size = new((uint)Map.Width, (uint)Map.Height);
			map.DispatchEvents();
			map.Clear(ToSFMLColor(MapBgColor.BackColor));
			DrawGrid(map, (int)MapWidth.Value, (int)MapHeight.Value);
			map.Display();

			set.Size = new((uint)Set.Width, (uint)Set.Height);
			set.DispatchEvents();
			set.Clear(ToSFMLColor(SetBgColor.BackColor));

			if(tileset != null)
			{
				var sz = GetTilesetTileCount();
				set.Draw(vertsTileset, new(tileset));
				DrawGrid(set, sz.X, sz.Y);
				DrawSelectedTile();
			}
			UpdateHoveredTile();

			set.Display();
		}

		private void OnBrushColorClick(object sender, EventArgs e)
		{
			Colors.Color = BrushColor.BackColor;
			if(Colors.ShowDialog() == DialogResult.OK)
				BrushColor.BackColor = Colors.Color;
		}
		private void OnGridColorClick(object sender, EventArgs e)
		{
			Colors.Color = GridColor.BackColor;
			if(Colors.ShowDialog() == DialogResult.OK)
				GridColor.BackColor = Colors.Color;
		}
		private void OnSetBackgroundColorClick(object sender, EventArgs e)
		{
			Colors.Color = SetBgColor.BackColor;
			if(Colors.ShowDialog() == DialogResult.OK)
				SetBgColor.BackColor = Colors.Color;
		}

		private void OnMapBackgroundColorClick(object sender, EventArgs e)
		{
			Colors.Color = MapBgColor.BackColor;
			if(Colors.ShowDialog() == DialogResult.OK)
				MapBgColor.BackColor = Colors.Color;
		}

		private void OnTilesetLoadClick(object sender, EventArgs e)
		{
			if(LoadTileset.ShowDialog() != DialogResult.OK)
				return;

			tileset?.Dispose();

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
			SetViewPosition(set, sz, new(sz.X / 2, sz.Y / 2));
			RecreateTilesetVerts();
		}

		private void OnNumericValueChange(object sender, EventArgs e)
		{
			LimitView(map, GetMapSize());
			LimitView(set, GetTilesetSize());
		}

		private void OnSetPress(object sender, MouseEventArgs e)
		{
			if(e.Button != MouseButtons.Left || IsHoveringSet() == false)
				return;

			selectedTile = GetHoveredCoordsRounded(set);
			selectedTileSquare = selectedTile;
		}
		#endregion
	}
}