using System.Numerics;

using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace AudioEditor
{
	public partial class Window : Form
	{
		private float zoom = 0.8f, x;

		private int step;
		private const int CELL_SIZE = 10;
		private readonly RenderWindow window;
		private readonly System.Windows.Forms.Timer clock;
		private readonly SFML.Graphics.View view;
		private readonly List<Vector2f> points = new();
		private readonly List<short> samples = new();
		private Point mousePosPrev;
		private Sound? sound;

		public Window()
		{
			InitializeComponent();
			CenterToScreen();

			window = new(Screen.Handle) { Size = new((uint)Size.Width, (uint)Size.Height) };
			view = window.GetView();

			Resize += (s, e) => UpdateView();
			MouseWheel += (s, e) =>
			{
				if(e.Delta < 0)
					zoom *= 1.05f;
				else
					zoom *= 0.95f;

				zoom = Math.Clamp(zoom, 0.1f, 10f);
				UpdateView();
			};

			clock = new() { Interval = 20 };
			clock.Tick += (s, e) =>
			{
				if(Width == 0 || Height == 0)
					return;

				window.Size = new((uint)Screen.Width, (uint)Screen.Height);
				window.DispatchEvents();

				if(Mouse.IsButtonPressed(Mouse.Button.Left))
					TryMoveSamplePoint();

				RecalculatePoints();

				window.Clear();

				//DrawGrid();
				DrawAudioWave();

				window.Display();
			};
			clock.Start();

			//samples.AddRange(Audio.GetSamplesFromChords(new() { "A4", "A4" }, 120));

			for(int i = 0; i < 11025; i++)
			{
				var f = 440f / 11025f;
				var r = 2f * f * i % (1f / f) - 1f;
				var sin = (short)(short.MaxValue * r);
				samples.Add((short)sin);
			}

			UpdateView();
		}

		private static Vector2f ToGrid(Vector2f point, float gridSize)
		{
			// this prevents -0 cells
			var x = point.X - (point.X < 0 ? gridSize : 0);
			var y = point.Y - (point.Y < 0 ? gridSize : 0);

			x -= point.X % gridSize;
			y -= point.Y % gridSize;
			return new(x, y);
		}
		private static float Limit(float number, float rangeA, float rangeB, bool isOverflowing = false)
		{
			if(rangeA > rangeB)
				(rangeA, rangeB) = (rangeB, rangeA);

			if(isOverflowing)
			{
				var d = rangeB - rangeA;
				return ((number - rangeA) % d + d) % d + rangeA;
			}
			else
			{
				if(number < rangeA)
					return rangeA;
				else if(number > rangeB)
					return rangeB;
				return number;
			}
		}
		private static float Map(float number, float a1, float a2, float b1, float b2)
		{
			var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
			return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
		}
		private Vector2f GetCurvePoint(float index)
		{
			if(points.Count == 0)
				return new Vector2f(float.NaN, float.NaN);

			int p0, p1, p2, p3;

			index = Limit(index, 0, points.Count);
			index = Map(index, 0, points.Count, 0, points.Count - 3);

			p1 = (int)index + 1;
			p2 = p1 + 1;
			p3 = p2 + 1;
			p0 = p1 - 1;

			index -= (int)index;

			var tt = index * index;
			var ttt = tt * index;

			var q1 = -ttt + 2.0f * tt - index;
			var q2 = 3.0f * ttt - 5.0f * tt + 2.0f;
			var q3 = -3.0f * ttt + 4.0f * tt + index;
			var q4 = ttt - tt;

			if(p3 == points.Count)
				return points[^2];

			var tx = 0.5f * (points[p0].X * q1 + points[p1].X * q2 + points[p2].X * q3 + points[p3].X * q4);
			var ty = 0.5f * (points[p0].Y * q1 + points[p1].Y * q2 + points[p2].Y * q3 + points[p3].Y * q4);

			return new(tx, ty);
		}

		private void TryMoveSamplePoint()
		{
			var mousePos = window.MapPixelToCoords(Mouse.GetPosition(window), view);
			var cell = ToGrid(mousePos, CELL_SIZE);
			var x = (int)MathF.Round(cell.X);

			if(x >= 0 && x < samples.Count)
			{
				for(int i = x - 25; i < x + 25; i++)
					if(i > 0 && i < samples.Count)
						samples[i] = (short)(cell.Y * 20);
			}
		}
		private void RecalculatePoints()
		{
			points.Clear();

			for(int i = 0; i < samples.Count; i++)
				points.Add(new(i, (short)(samples[i] / 20)));
		}
		private void UpdateView()
		{
			view.Size = new(window.Size.X * zoom, window.Size.Y * zoom);
			view.Center = new(x, 0);
			window.SetView(view);
		}
		private void DrawGrid()
		{
			var verts = new VertexArray(PrimitiveType.Lines);
			var sz = new Vector2(Screen.Width, Screen.Height) * zoom;
			var viewPos = window.GetView().Center;
			var col = new SFML.Graphics.Color(100, 100, 100);

			for(float i = 0; i <= sz.X * 4; i += CELL_SIZE)
			{
				var x = viewPos.X - sz.X * 2 + i;
				var y = viewPos.Y;
				var top = ToGrid(new Vector2f(x, y - sz.Y * 2), CELL_SIZE);
				var bot = ToGrid(new Vector2f(x, y + sz.Y * 2), CELL_SIZE);

				verts.Append(new(top, col));
				verts.Append(new(bot, col));
			}
			for(float i = 0; i <= sz.Y * 4; i += CELL_SIZE)
			{
				var x = viewPos.X;
				var y = viewPos.Y - sz.Y * 2 + i;
				var left = ToGrid(new Vector2f(x - sz.X * 2, y), CELL_SIZE);
				var right = ToGrid(new Vector2f(x + sz.X * 2, y), CELL_SIZE);
				var c = col;

				if(left.Y == 0)
					c = SFML.Graphics.Color.White;

				verts.Append(new(left, c));
				verts.Append(new(right, c));
			}

			window.Draw(verts);
		}
		private void DrawAudioWave()
		{
			var verts = new VertexArray(PrimitiveType.Lines);
			var c = SFML.Graphics.Color.White;

			for(int i = 0; i < points.Count; i++)
			{
				if(i == 0)
					continue; // don't draw loop (0 - step = last index)

				verts.Append(new(points[i], c));
				verts.Append(new(points[i - 1], c));
			}
			window.Draw(verts);
		}

		private void OnClose(object sender, FormClosedEventArgs e)
		{
			window.Close();
		}

		private void OnMenuPlay(object sender, EventArgs e)
		{
			sound?.Stop();
			sound?.Dispose();
			sound = new(new SoundBuffer(samples.ToArray(), 1, 11025));
			sound.Play();
			//sound.Pitch = 1f / 30f;
		}

		private void OnMouseMove(object sender, MouseEventArgs e)
		{
			var mousePosRaw = MousePosition;

			if(Mouse.IsButtonPressed(Mouse.Button.Left))
				TryMoveSamplePoint();
			else if(Mouse.IsButtonPressed(Mouse.Button.Middle))
			{
				var delta = mousePosPrev.X - mousePosRaw.X;
				x += delta * zoom;
				x = Math.Clamp(x, 0, points[^1].X);
				UpdateView();
			}

			mousePosPrev = mousePosRaw;
		}
	}
}