using Pure.Tilemap;

using SFML.Graphics;
using SFML.System;

using Color = SFML.Graphics.Color;

namespace TilemapEditor
{
	internal class Layer
	{
		private readonly Vertex[] vertsTile = new Vertex[4];
		private readonly int[,] tiles;
		private readonly byte[,] colors;
		private readonly VertexBuffer verts;
		private Vector2i tilesetCount, tileSize, tileOffset;

		public string name = "Tilemap";

		public Layer(Vector2i size, Vector2i tilesetCount, Vector2i tileSize, Vector2i tileOffset)
		{
			this.tilesetCount = tilesetCount;
			this.tileSize = tileSize;
			this.tileOffset = tileOffset;

			tiles = new int[size.X, size.Y];
			colors = new byte[size.X, size.Y];
			verts = new(
				(uint)(size.X * size.Y * 4),
				PrimitiveType.Quads,
				VertexBuffer.UsageSpecifier.Dynamic);

			for(int y = 0; y < size.Y; y++)
				for(int x = 0; x < size.X; x++)
				{
					SetTile(new(x, y), 1, Color.White);
					SetTile(new(x, y), 0, Color.White);
				}
		}
		public Layer(string path, Vector2i tilesetCount, Vector2i tileSize, Vector2i tileOffset)
		{
			this.tilesetCount = tilesetCount;
			this.tileSize = tileSize;
			this.tileOffset = tileOffset;

			var tilemap = new Tilemap(path);

			tiles = tilemap;
			var (w, h) = (tiles.GetLength(0), tiles.GetLength(1));

			colors = tilemap;
			verts = new(
				(uint)(w * h * 4),
				PrimitiveType.Quads,
				VertexBuffer.UsageSpecifier.Dynamic);

			for(int y = 0; y < h; y++)
				for(int x = 0; x < w; x++)
				{
					var p = new Vector2i(x, y);
					var value = colors[x, y];
					var r = (byte)((value >> 5) * 255 / 7);
					var g = (byte)(((value >> 2) & 0x07) * 255 / 7);
					var b = (byte)((value & 0x03) * 255 / 3);
					var tile = GetTile(p);
					SetTile(p, tile + 1, Color.White);
					SetTile(p, tile - 1, Color.White);
					SetTile(p, tile, new(r, g, b));
				}
		}
		~Layer() => verts?.Dispose();

		public void SetTile(Vector2i pos, int tile, Color color)
		{
			if(tiles == null || colors == null)
				return;

			var w = tiles.GetLength(0);
			var h = tiles.GetLength(1);

			if(pos.X < 0 || pos.X >= w ||
				pos.Y < 0 || pos.Y >= h ||
				tiles[pos.X, pos.Y] == tile)
				return;

			var c = color;
			var count = tilesetCount;
			var tilePos = new Vector2f(
				count.X == 0 ? 0 : tile % count.X,
				count.X == 0 ? 0 : tile / count.X);
			var sz = tileSize;
			var off = tileOffset;
			var tx = new Vector2f(tilePos.X * (sz.X + off.X), tilePos.Y * (sz.Y + off.Y));
			var index = pos.X * h + pos.Y;
			var byteCol = (byte)(((c.R * 7 / 255) << 5) + ((c.G * 7 / 255) << 2) + (c.B * 3 / 255));
			var p = new Vector2f(pos.X * sz.X, pos.Y * sz.Y);

			vertsTile[0] = new(new(p.X, p.Y), c, new(tx.X, tx.Y));
			vertsTile[1] = new(new(p.X + sz.X, p.Y), c, new(tx.X + sz.X, tx.Y));
			vertsTile[2] = new(new(p.X + sz.X, p.Y + sz.Y), c, new(tx.X + sz.X, tx.Y + sz.Y));
			vertsTile[3] = new(new(p.X, p.Y + sz.Y), c, new(tx.X, tx.Y + sz.Y));

			tiles[pos.X, pos.Y] = tile;
			colors[pos.X, pos.Y] = byteCol;
			verts.Update(vertsTile, (uint)(index * 4));
		}
		public int GetTile(Vector2i pos)
		{
			var w = tiles.GetLength(0);
			var h = tiles.GetLength(1);
			var x = pos.X;
			var y = pos.Y;
			var isOutside = x < 0 || x >= w || y < 0 || y >= h;

			return isOutside ? -1 : tiles[x, y];
		}
		public Color GetColor(Vector2i pos)
		{
			var w = colors.GetLength(0);
			var h = colors.GetLength(1);
			var x = pos.X;
			var y = pos.Y;
			var isOutside = x < 0 || x >= w || y < 0 || y >= h;
			var value = isOutside ? default : colors[x, y];
			var r = (byte)((value >> 5) * 255 / 7);
			var g = (byte)(((value >> 2) & 0x07) * 255 / 7);
			var b = (byte)((value & 0x03) * 255 / 3);

			return new(r, g, b);
		}
		public void Save(string path)
		{
			var tilemap = new Tilemap(tiles, colors);
			tilemap.Save(path);
		}
		public void Draw(RenderWindow map, Texture texture)
		{
			if(verts == null)
				return;

			map.Draw(verts, new(texture));
		}

		public override string ToString()
		{
			return name;
		}
	}
}
