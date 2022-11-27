using SFML.Graphics; // textures and vertices etc.
using SFML.System; // vectors etc.
using SFML.Window; // window etc.

namespace Purity.Graphics
{
	public class Window
	{
		public bool IsOpen
		{
			get => window != null && window.IsOpen;
			set
			{
				if(value == false)
					window.Close();
			}
		}
		public string Title
		{
			get => title;
			set { title = value; window.SetTitle(title); }
		}

		public Window(string title = "Purity")
		{
			var desktopW = VideoMode.DesktopMode.Width;
			var desktopH = VideoMode.DesktopMode.Height;
			this.title = title;

			window = new(new VideoMode((uint)(desktopW * 0.65f), (uint)(desktopH * 0.65f)), title);
			window.Closed += (s, e) => window.Close();
			window.Resized += (s, e) =>
			{
				var view = window.GetView();
				view.Size = new(e.Width, e.Height);
				view.Center = new(e.Width / 2f, e.Height / 2f);
				window.SetView(view);
			};
			window.DispatchEvents();
			window.Clear();
			window.Display();
		}

		public void DrawOn()
		{
			window.DispatchEvents();
			window.Clear();
		}
		public void DrawLayer(uint[,] cells, byte[,] colors, (uint, uint) tileSize,
			(uint, uint) tileMargin, string path = "graphics.png")
		{
			if(path == null || cells == null || colors == null || cells.Length != colors.Length)
				return;

			TryLoadGraphics(path);
			var verts = GetLayerVertices(cells, colors, tileSize, tileMargin, path);
			window.Draw(verts, PrimitiveType.Quads, new(graphics[path]));
		}
		public void DrawSprite((float, float) position, uint cell, byte color)
		{
			if(prevDrawLayerGfxPath == null)
				return;

			var verts = GetSpriteVertices(position, cell, color);
			window.Draw(verts, PrimitiveType.Quads, new(graphics[prevDrawLayerGfxPath]));
		}
		public void DrawParticles(byte color, params (float, float)[] positions)
		{
			if(positions == null || positions.Length == 0)
				return;

			var verts = GetParticlesVertices(color, positions);
			window.Draw(verts, PrimitiveType.Quads);
		}
		public void DrawOff()
		{
			window.Display();
		}

		public (float, float) GetMousePosition((uint, uint) layerCellCount)
		{
			var pos = Mouse.GetPosition(window);
			var x = Map(pos.X, 0, window.Size.X, 0, layerCellCount.Item1);
			var y = Map(pos.Y, 0, window.Size.Y, 0, layerCellCount.Item2);
			return (x, y);
		}
		public (int, int) GetHoveredIndicies((uint, uint) layerCellCount)
		{
			var mousePos = GetMousePosition(layerCellCount);
			var x = MathF.Floor(mousePos.Item1);
			var y = MathF.Floor(mousePos.Item2);
			return ((int)x, (int)y);
		}
		public uint GetHoveredCell(uint[,] cells)
		{
			var w = (uint)cells.GetLength(0);
			var h = (uint)cells.GetLength(1);
			var indices = GetHoveredIndicies((w, h));
			var x = indices.Item1;
			var y = indices.Item2;

			return x < 0 || y < 0 || x >= w || y >= h ? default : cells[x, y];
		}

		#region Backend
		private string title;
		private string? prevDrawLayerGfxPath;
		private (uint, uint) prevDrawLayerTileSz;
		private (float, float) prevDrawLayerCellSz;
		private (uint, uint) prevDrawLayerCellCount;

		private readonly Dictionary<string, Texture> graphics = new();
		private readonly RenderWindow window;

		private void TryLoadGraphics(string path)
		{
			if(graphics.ContainsKey(path))
				return;

			graphics[path] = new(path);
		}
		private Vertex[] GetSpriteVertices((float, float) position, uint cell, byte color)
		{
			if(prevDrawLayerGfxPath == null)
				return Array.Empty<Vertex>();

			var verts = new Vertex[4];
			var cellWidth = prevDrawLayerCellSz.Item1;
			var cellHeight = prevDrawLayerCellSz.Item2;
			var tileSz = prevDrawLayerTileSz;
			var texture = graphics[prevDrawLayerGfxPath];

			var tileCount = (texture.Size.X / tileSz.Item1, texture.Size.Y / tileSz.Item2);
			var texCoords = IndexToCoords(cell, tileCount);
			var tx = new Vector2f(texCoords.Item1 * tileSz.Item1, texCoords.Item2 * tileSz.Item2);
			var cellCount = prevDrawLayerCellCount;
			var x = Map(position.Item1, 0, cellCount.Item1, 0, window.Size.X);
			var y = Map(position.Item2, 0, cellCount.Item2, 0, window.Size.Y);
			var c = ByteToColor(color);
			var grid = ToGrid((x, y), (cellWidth / tileSz.Item1, cellHeight / tileSz.Item2));
			var tl = new Vector2f(grid.Item1, grid.Item2);
			var br = new Vector2f(grid.Item1 + cellWidth, grid.Item2 + cellHeight);

			verts[0] = new(new(tl.X, tl.Y), c, tx);
			verts[1] = new(new(br.X, tl.Y), c, tx + new Vector2f(tileSz.Item1, 0));
			verts[2] = new(new(br.X, br.Y), c, tx + new Vector2f(tileSz.Item1, tileSz.Item2));
			verts[3] = new(new(tl.X, br.Y), c, tx + new Vector2f(0, tileSz.Item2));
			return verts;
		}
		private Vertex[] GetLayerVertices(uint[,] cells, byte[,] colors,
			(uint, uint) tileSz, (uint, uint) tileOff, string path)
		{
			if(cells == null || window == null)
				return Array.Empty<Vertex>();

			var cellWidth = (float)window.Size.X / cells.GetLength(0);
			var cellHeight = (float)window.Size.Y / cells.GetLength(1);
			var texture = graphics[path];
			var tileCount = (texture.Size.X / tileSz.Item1, texture.Size.Y / tileSz.Item2);
			var verts = new Vertex[cells.Length * 4];

			// this cache is used for a potential sprite draw
			prevDrawLayerGfxPath = path;
			prevDrawLayerCellSz = (cellWidth, cellHeight);
			prevDrawLayerTileSz = tileSz;
			prevDrawLayerCellCount = ((uint)cells.GetLength(0), (uint)cells.GetLength(1));

			for(uint y = 0; y < cells.GetLength(1); y++)
				for(uint x = 0; x < cells.GetLength(0); x++)
				{
					var cell = cells[x, y];
					var color = ByteToColor(colors[x, y]);
					var i = GetIndex(x, y, (uint)cells.GetLength(0)) * 4;
					var tl = new Vector2f(x * cellWidth, y * cellHeight);
					var tr = new Vector2f((x + 1) * cellWidth, y * cellHeight);
					var br = new Vector2f((x + 1) * cellWidth, (y + 1) * cellHeight);
					var bl = new Vector2f(x * cellWidth, (y + 1) * cellHeight);

					var w = tileSz.Item1;
					var h = tileSz.Item2;
					var texCoords = IndexToCoords(cell, tileCount);
					var tx = new Vector2f(
						texCoords.Item1 * (w + tileOff.Item1),
						texCoords.Item2 * (h + tileOff.Item2));
					var texTr = new Vector2f(tx.X + w, tx.Y);
					var texBr = new Vector2f(tx.X + w, tx.Y + h);
					var texBl = new Vector2f(tx.X, tx.Y + h);

					verts[i + 0] = new(tl, color, tx);
					verts[i + 1] = new(tr, color, texTr);
					verts[i + 2] = new(br, color, texBr);
					verts[i + 3] = new(bl, color, texBl);
				}
			return verts;
		}
		private Vertex[] GetParticlesVertices(byte color, (float, float)[] positions)
		{
			var verts = new Vertex[positions.Length * 4];
			var tileSz = prevDrawLayerTileSz;
			var cellWidth = prevDrawLayerCellSz.Item1 / tileSz.Item1;
			var cellHeight = prevDrawLayerCellSz.Item2 / tileSz.Item2;
			var cellCount = prevDrawLayerCellCount;

			for(int i = 0; i < positions.Length; i++)
			{
				var x = Map(positions[i].Item1, 0, cellCount.Item1, 0, window.Size.X);
				var y = Map(positions[i].Item2, 0, cellCount.Item2, 0, window.Size.Y);
				var c = ByteToColor(color);
				var grid = ToGrid((x, y), (cellWidth, cellHeight));
				var tl = new Vector2f(grid.Item1, grid.Item2);
				var br = new Vector2f(grid.Item1 + cellWidth, grid.Item2 + cellHeight);

				var index = i * 4;
				verts[index + 0] = new(new(tl.X, tl.Y), c);
				verts[index + 1] = new(new(br.X, tl.Y), c);
				verts[index + 2] = new(new(br.X, br.Y), c);
				verts[index + 3] = new(new(tl.X, br.Y), c);
			}
			return verts;
		}
		private static (uint, uint) IndexToCoords(uint index, (uint, uint) fieldSize)
		{
			index = index < 0 ? 0 : index;
			index = index > fieldSize.Item1 * fieldSize.Item2 - 1 ? fieldSize.Item1 * fieldSize.Item2 - 1 : index;

			return (index % fieldSize.Item1, index / fieldSize.Item1);
		}
		private static uint GetIndex(uint x, uint y, uint width)
		{
			return y * width + x;
		}
		private static Color ByteToColor(byte color)
		{
			var binary = Convert.ToString(color, 2).PadLeft(8, '0');
			var r = binary[0..3];
			var g = binary[3..6];
			var b = binary[6..8];
			var red = (byte)(Convert.ToByte(r, 2) * byte.MaxValue / 7);
			var green = (byte)(Convert.ToByte(g, 2) * byte.MaxValue / 7);
			var blue = (byte)(Convert.ToByte(b, 2) * byte.MaxValue / 3);
			return new(red, green, blue);
		}
		static float Map(float number, float a1, float a2, float b1, float b2)
		{
			var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
			return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
		}
		private static (float, float) ToGrid((float, float) pos, (float, float) gridSize)
		{
			if(gridSize == default)
				return pos;

			var X = pos.Item1;
			var Y = pos.Item2;

			// this prevents -0 cells
			var x = X - (X < 0 ? gridSize.Item1 : 0);
			var y = Y - (Y < 0 ? gridSize.Item2 : 0);

			x -= X % gridSize.Item1;
			y -= Y % gridSize.Item2;
			return new(x, y);
		}
		#endregion
	}
}
