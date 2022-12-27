using System.Xml;

namespace Purity.Collision
{
	/// <summary>
	/// (Inherits <see cref="Hitbox"/>)<br></br><br></br>
	/// An additional grid collection of <see cref="Rectangle"/>s on top of the
	/// <see cref="Hitbox"/> one.<br></br><br></br>
	/// 
	/// Each <see cref="Grid"/> cell has its own <see cref="Rectangle"/> collection and the
	/// <see cref="Grid"/> combines them. Unlike <see cref="Hitbox"/>, this collection is optimized
	/// to do checks in chunks by picking only neighbouring cells rather than checking each cell's
	/// collection.
	/// </summary>
	public class Grid : Hitbox
	{
		/// <summary>
		/// The amount of <see cref="Rectangle"/>s in the <see cref="Rectangle"/> collection and
		/// cell <see cref="Rectangle"/> collection, combined.
		/// </summary>
		public override int RectangleCount => rectangles.Count + cellRects.Count;

		/// <summary>
		/// Get: returns the <see cref="Rectangle"/> at <paramref name="index"/>.
		/// </summary>
		public override Rectangle this[int index]
		{
			get
			{
				var r = rectangles;
				return index < r.Count ? r[index] : cellRects[index - r.Count];
			}
		}

		/// <summary>
		/// Creates the <see cref="Grid"/> with a certain <paramref name="cellSize"/>,
		/// <paramref name="position"/> and <paramref name="scale"/>.
		/// </summary>
		public Grid(int cellSize, (float, float) position = default, float scale = 1)
			: base(position, scale)
		{
			if(cellSize < 1)
				throw new ArgumentException("Value cannot be < 1.", nameof(cellSize));

			this.cellSize = cellSize;
		}
		/// <summary>
		/// Creates the <see cref="Grid"/> from <paramref name="tiles"/> and a
		/// <see langword="Tiled Tileset"/> export file at <paramref name="tsxPath"/>, with a certain
		/// <paramref name="position"/> and <paramref name="scale"/>.
		/// </summary>
		public Grid(string tsxPath, int[,] tiles, (float, float) position = default, float scale = 1)
			: base(position, scale)
		{
			if(tiles == null)
				throw new ArgumentNullException(nameof(tiles));

			if(tsxPath == null)
				throw new ArgumentNullException(nameof(tsxPath));

			if(File.Exists(tsxPath) == false)
				throw new ArgumentException($"No tsx file was found at '{tsxPath}'.");

			var xml = new XmlDocument();
			xml.Load(tsxPath);

			var tilesets = xml.GetElementsByTagName("tileset");
			if(tilesets == null || tilesets.Count == 0)
			{
				Error();
				return;
			}

			var tileset = tilesets[0];
			var attributes = tileset?.Attributes;

			if(tileset == null || attributes == null)
			{
				Error();
				return;
			}

			_ = int.TryParse(attributes["tilewidth"]?.InnerText, out var tileWidth);
			//_ = int.TryParse(attributes["tileheight"]?.InnerText, out var h);

			cellSize = tileWidth;

			foreach(XmlNode child in tileset)
			{
				var rects = child.ChildNodes?[0]?.ChildNodes;

				if(child.Name != "tile" || rects == null || rects.Count == 0)
					continue;

				_ = int.TryParse(child.Attributes?["id"]?.InnerText, out var id);

				var currRectList = new List<Rectangle>();
				foreach(XmlNode rect in rects)
				{
					var att = rect.Attributes;
					var nodeX = att?["x"];
					var nodeY = att?["y"];
					var nodeW = att?["width"];
					var nodeH = att?["height"];

					if(nodeX == null || nodeY == null || nodeW == null || nodeH == null)
						continue;

					_ = int.TryParse(nodeX?.InnerText, out var x);
					_ = int.TryParse(nodeY?.InnerText, out var y);
					_ = int.TryParse(nodeW?.InnerText, out var w);
					_ = int.TryParse(nodeH?.InnerText, out var h);

					var localRect = new Rectangle((w, h), (x, y));
					currRectList.Add(localRect);
				}
				cellRectsMap[id] = currRectList;
			}

			UpdatePositions(tiles);

			void Error() => throw new Exception($"Could not parse file at '{tsxPath}'.");
		}

		/// <summary>
		/// Expands the <see cref="Rectangle"/> collection with a new <paramref name="rectangle"/>
		/// for each <paramref name="tile"/> in <paramref name="tiles"/>.
		/// </summary>
		public void AddRectangleToTile(Rectangle rectangle, int tile, int[,] tiles)
		{
			if(tiles == null)
				throw new ArgumentNullException(nameof(tiles));

			if(cellRectsMap.ContainsKey(tile))
			{
				cellRectsMap[tile].Add(rectangle);
				return;
			}

			cellRectsMap[tile] = new List<Rectangle>() { rectangle };
			UpdatePositions(tiles);
		}
		/// <summary>
		/// Retrieves the <see cref="Rectangle"/> collection at a certain <paramref name="cell"/> and
		/// returns it if the provided <paramref name="cell"/> is present.
		/// Returns an empty <see cref="Array"/> of <see cref="Rectangle"/>s otherwise. 
		/// </summary>
		public Rectangle[] GetRectanglesAt((int, int) cell)
		{
			if(tileIndices.ContainsKey(cell) == false)
				return Array.Empty<Rectangle>();

			var id = tileIndices[cell];
			var rects = cellRectsMap[id];
			var result = new List<Rectangle>();
			var (x, y) = cell;

			for(int r = 0; r < rects.Count; r++)
				result.Add(LocalToGlobalRectangle(rects[r], (x, y)));

			return result.ToArray();
		}

		/// <summary>
		/// Checks whether a <paramref name="line"/> overlaps at least one of the
		/// <see cref="Rectangle"/>s in the collection and returns the result.
		/// </summary>
		public override bool IsOverlapping(Line line)
		{
			return line.CrossPoints(this).Length > 0;
		}
		/// <summary>
		/// Checks whether a <paramref name="rectangle"/> overlaps at least one of the
		/// <see cref="Rectangle"/>s in the collection and returns the result.
		/// </summary>
		public override bool IsOverlapping(Rectangle rectangle)
		{
			var point = rectangle.Position;
			var chunkSize = GetChunkSizeForRect(rectangle);
			var neighborRects = GetNeighborRects(point, chunkSize);
			for(int i = 0; i < neighborRects.Count; i++)
				if(neighborRects[i].IsOverlapping(rectangle))
					return true;

			return false;
		}
		/// <summary>
		/// Checks whether at least one of the <see cref="Rectangle"/>s in the collection contains
		/// a <paramref name="point"/> and returns the result.
		/// </summary>
		public override bool IsOverlapping((float, float) point)
		{
			var neighborRects = GetNeighborRects(point, 1);
			for(int i = 0; i < neighborRects.Count; i++)
				if(neighborRects[i].IsOverlapping(point))
					return true;

			return false;
		}

		#region Backend
		internal readonly int cellSize;

		private readonly List<Rectangle> cellRects = new();
		private readonly Dictionary<int, List<Rectangle>> cellRectsMap = new();
		private readonly Dictionary<(int, int), int> tileIndices = new();

		private void UpdatePositions(int[,] tiles)
		{
			for(int y = 0; y < tiles.GetLength(0); y++)
				for(int x = 0; x < tiles.GetLength(1); x++)
				{
					var tileID = tiles[x, y];
					if(cellRectsMap.ContainsKey(tileID))
						tileIndices[(x, y)] = tileID;
					else
						tileIndices.Remove((x, y));
				}

			cellRects.Clear();
			foreach(var kvp in tileIndices)
			{
				var pos = kvp.Key;
				var id = kvp.Value;
				var localRects = cellRectsMap[id];
				for(int i = 0; i < localRects.Count; i++)
					cellRects.Add(LocalToGlobalRectangle(localRects[i], pos));
			}
		}

		private Rectangle LocalToGlobalRectangle(Rectangle localRect, (float, float) tilePos)
		{
			var sc = Scale;
			var sz = cellSize;
			var (x, y) = tilePos;
			var (rx, ry) = localRect.Position;
			var (rw, rh) = localRect.Size;
			var (offX, offY) = Position;

			x *= sz * sc;
			y *= sz * sc;

			rx *= sc;
			ry *= sc;
			rw *= sc;
			rh *= sc;

			rx += offX * sc;
			ry += offY * sc;

			return new((rw, rh), (x + rx, y + ry));
		}
		private Rectangle GlobalToLocal(Rectangle globalRect)
		{
			var (w, h) = globalRect.Size;
			var (x, y) = globalRect.Position;
			var sc = Scale;
			var sz = cellSize;
			var (offX, offY) = Position;

			x -= offX * sc;
			y -= offY * sc;

			x /= sc;
			y /= sc;
			w /= sc;
			h /= sc;

			w /= sz;
			h /= sz;
			x /= sz;
			y /= sz;

			return new(((int)w, (int)h), ((int)x, (int)y));
		}
		internal List<Rectangle> GetNeighborRects((float, float) point, int chunkSize)
		{
			var result = new List<Rectangle>(cellRects);
			var localRect = GlobalToLocal(new(point, (0, 0)));
			var (x, y) = localRect.Position;
			var (w, h) = localRect.Size;
			var xStep = w < 0 ? -1 : 1;
			var yStep = h < 0 ? -1 : 1;
			var ch = chunkSize;

			for(int j = (int)y - yStep * (ch - 1); j != y + h + yStep * ch; j += yStep)
				for(int i = (int)x - xStep * (ch - 1); i != x + w + xStep * ch; i += xStep)
				{
					var pos = (i, j);

					if(tileIndices.ContainsKey(pos) == false)
						continue;

					var id = tileIndices[(i, j)];
					var rects = cellRectsMap[id];
					for(int r = 0; r < rects.Count; r++)
					{
						var rect = LocalToGlobalRectangle(rects[r], (i, j));
						result.Add(rect);
					}
				}
			return result;
		}
		private int GetChunkSizeForRect(Rectangle globalRect)
		{
			var (w, h) = globalRect.Size;
			var size = w > h ? w : h;
			size /= cellSize * Scale;
			return (int)size * 2;
		}
		#endregion
	}
}
