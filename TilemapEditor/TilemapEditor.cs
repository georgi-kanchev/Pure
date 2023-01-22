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
		private readonly RenderWindow map, set;
		private readonly Timer loop;
		private float mapZoom = 1f, setZoom = 1f;
		private Vector2 prevFormsMousePosMap, prevFormsMousePosSet;
		//private Vector2f prevMousePosMap, prevMousePosSet;

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

			SetViewPosition(map);
			SetViewPosition(set);
		}

		#region Actions
		private static void TryScrollView(RenderWindow window, ref float zoom, float delta)
		{
			delta = delta > 0 ? -0.05f : 0.05f;
			var pos = ToSystemVector(window.GetView().Center);
			var mousePos = ToSystemVector(GetMousePosition(window));

			if(delta < 0)
			{
				pos = Vector2.Lerp(pos, mousePos, 0.05f);
				SetViewPosition(window, pos);
			}
			SetViewZoom(window, ref zoom, zoom + delta);
		}
		private static void TryMoveView(RenderWindow window, Vector2 prevMousePos)
		{
			var pos = ToSystemVector(window.GetView().Center);

			SetViewPosition(window, Drag(window, pos, prevMousePos));
			Cursor.Current = Cursors.SizeAll;
		}

		private static void SetViewPosition(RenderWindow window, Vector2 pos = default)
		{
			var view = window.GetView();
			view.Center = new(pos.X, pos.Y);
			window.SetView(view);
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

		private void DrawGrid(RenderWindow window, float zoom)
		{
			var cellVerts = new VertexArray(PrimitiveType.Lines);
			var sz = new Vector2f(window.Size.X, window.Size.Y) * zoom;
			var spacing = new Vector2f((float)TileWidth.Value, (float)TileHeight.Value);
			var viewPos = window.GetView().Center;
			var gridColor = ToSFMLColor(GridColor.BackColor);

			for(float i = 0; i <= sz.X * 2; i += spacing.X)
			{
				var x = viewPos.X - sz.X + i;
				var y = viewPos.Y;
				var top = ToGrid(new Vector2f(x, y - sz.Y), spacing);
				var bot = ToGrid(new Vector2f(x, y + sz.Y), spacing);
				var col = GetColor(top.X);

				cellVerts.Append(new(top, col));
				cellVerts.Append(new(bot, col));
			}
			for(float i = 0; i <= sz.Y * 2; i += spacing.Y)
			{
				var x = viewPos.X;
				var y = viewPos.Y - sz.Y + i;
				var left = ToGrid(new Vector2f(x - sz.X, y), spacing);
				var right = ToGrid(new Vector2f(x + sz.X, y), spacing);
				var col = GetColor(left.Y);

				cellVerts.Append(new(left, col));
				cellVerts.Append(new(right, col));
			}

			window.Draw(cellVerts);

			SFML.Graphics.Color GetColor(float coordinate)
			{
				return coordinate == 0 ? SFML.Graphics.Color.White : gridColor;
			}
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

		#endregion
		#region Events
		private void OnMouseMoveMap(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Middle)
				TryMoveView(map, prevFormsMousePosMap);

			prevFormsMousePosMap = GetFormsMousePos(map);
			//prevMousePosMap = GetMousePosition(map);

		}
		private void OnMouseMoveSet(object sender, MouseEventArgs e)
		{
			if(e.Button == MouseButtons.Middle)
				TryMoveView(set, prevFormsMousePosSet);

			prevFormsMousePosSet = GetFormsMousePos(set);
			//prevMousePosSet = GetMousePosition(set);
		}

		private void OnMapScroll(object? sender, MouseEventArgs e)
		{
			TryScrollView(map, ref mapZoom, e.Delta);
		}
		private void OnSetScroll(object? sender, MouseEventArgs e)
		{
			TryScrollView(set, ref setZoom, e.Delta);
		}

		private void OnUpdate(object? sender, EventArgs e)
		{
			map.Size = new((uint)Map.Width, (uint)Map.Height);
			map.DispatchEvents();
			map.Clear(ToSFMLColor(MapBgColor.BackColor));
			DrawGrid(map, mapZoom);
			map.Display();

			set.Size = new((uint)Set.Width, (uint)Set.Height);
			set.DispatchEvents();
			set.Clear(ToSFMLColor(SetBgColor.BackColor));
			DrawGrid(set, setZoom);
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
		private static Vector2f ToGrid(Vector2f point, Vector2f gridSize)
		{
			if(gridSize == default)
				return point;

			// this prevents -0 cells
			var x = point.X - (point.X < 0 ? gridSize.X : 0);
			var y = point.Y - (point.Y < 0 ? gridSize.Y : 0);

			x -= point.X % gridSize.X;
			y -= point.Y % gridSize.Y;
			return new(x, y);
		}
		#endregion
	}
}