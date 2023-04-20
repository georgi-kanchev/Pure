using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Pure.Tilemap;

public class Tilemap
{
	public enum Alignment
	{
		TopLeft, TopUp, TopRight,
		Left, Center, Right,
		BottomLeft, Bottom, BottomRight
	};

	public (int width, int height) Size => (data.GetLength(0), data.GetLength(1));

	public (int x, int y) CameraPosition { get; set; }
	public (int width, int height) CameraSize { get; set; }

	public int[,] IDs
	{
		get
		{
			var result = new int[data.GetLength(0), data.GetLength(1)];
			for (int j = 0; j < data.GetLength(1); j++)
				for (int i = 0; i < data.GetLength(0); i++)
					result[i, j] = data[i, j].ID;

			return result;
		}
	}

	public Tilemap(string path)
	{
		try
		{
			var bytes = Decompress(File.ReadAllBytes(path));
			var bWidth = new byte[4];
			var bHeight = new byte[4];

			Array.Copy(bytes, 0, bWidth, 0, bWidth.Length);
			Array.Copy(bytes, bWidth.Length, bHeight, 0, bHeight.Length);
			var w = BitConverter.ToInt32(bWidth);
			var h = BitConverter.ToInt32(bHeight);

			data = new Tile[w, h];
			var byteSize = w * h * Marshal.SizeOf(typeof(Tile));
			var bData = new byte[byteSize];

			Array.Copy(bData, bWidth.Length + bHeight.Length, bytes, 0, bData.Length);

			FromBytes(data, bData);
		}
		catch (Exception)
		{
			throw new Exception($"Could not load {nameof(Tilemap)} from '{path}'.");
		}
	}
	public Tilemap((int tilesH, int tilesV) tileCount)
	{
		var (w, h) = tileCount;

		if (w < 1)
			w = 1;
		if (h < 1)
			h = 1;

		data = new Tile[w, h];
		CameraSize = tileCount;
	}
	public Tilemap(Tile[,] data)
	{
		if (data == null)
			throw new ArgumentNullException(nameof(data));

		var w = data.GetLength(0);
		var h = data.GetLength(1);

		this.data = Copy(data);
		CameraSize = Size;
	}

	public void Save(string path)
	{
		var (w, h) = Size;
		var bWidth = BitConverter.GetBytes(w);
		var bHeight = BitConverter.GetBytes(h);
		var bytes = ToBytes(data);
		var result = new byte[bWidth.Length + bHeight.Length + bytes.Length];
		var offset = 0;

		Add(bWidth);
		Add(bHeight);
		Add(data);

		File.WriteAllBytes(path, Compress(result));

		void Add(Array array)
		{
			Array.Copy(array, 0, result, offset, array.Length);
			offset += array.Length;
		}
	}

	public Tilemap CameraUpdate()
	{
		var (w, h) = CameraSize;
		var (cx, cy) = CameraPosition;
		var data = new Tile[Math.Abs(w), Math.Abs(h)];
		var xStep = w < 0 ? -1 : 1;
		var yStep = h < 0 ? -1 : 1;
		var i = 0;
		for (int x = cx; x != cx + w; x += xStep)
		{
			var j = 0;
			for (int y = cy; y != cy + h; y += yStep)
			{
				data[i, j] = TileAt((x, y));
				j++;
			}
			i++;
		}
		return new(data);
	}

	public Tile TileAt((int x, int y) cell)
	{
		return IndicesAreValid(cell) ? data[cell.x, cell.y] : default;
	}

	public void Fill(Tile withTile = default)
	{
		for (int y = 0; y < Size.Item2; y++)
			for (int x = 0; x < Size.Item1; x++)
				SetTile((x, y), withTile);
	}

	public void SetTile((int x, int y) cell, Tile tile)
	{
		if (IndicesAreValid(cell) == false)
			return;

		data[cell.x, cell.y] = tile;
	}
	public void SetSquare((int x, int y) cell, (int width, int height) size, Tile tile)
	{
		var xStep = size.Item1 < 0 ? -1 : 1;
		var yStep = size.Item2 < 0 ? -1 : 1;
		var i = 0;
		for (int x = cell.Item1; x != cell.Item1 + size.Item1; x += xStep)
			for (int y = cell.Item2; y != cell.Item2 + size.Item2; y += yStep)
			{
				if (i > Math.Abs(size.Item1 * size.Item2))
					return;

				SetTile((x, y), tile);
				i++;
			}
	}

	public void SetTextLine((int x, int y) cell, string text, uint tint = uint.MaxValue)
	{
		var errorOffset = 0;
		for (int i = 0; i < text?.Length; i++)
		{
			var symbol = text[i];
			var index = TileFrom(symbol);

			if (index == default && symbol != ' ')
			{
				errorOffset++;
				continue;
			}

			if (symbol == ' ')
				continue;

			SetTile((cell.Item1 + i - errorOffset, cell.Item2), new(index, tint));
		}
	}
	public void SetTextSquare((int x, int y) cell, (int width, int height) size, string text, uint tint = uint.MaxValue, bool isWordWrapping = true, Alignment alignment = Alignment.TopLeft, float scrollProgress = 0)
	{
		if (text == null || text.Length == 0 ||
			size.Item1 <= 0 || size.Item2 <= 0)
			return;

		var x = cell.Item1;
		var y = cell.Item2;
		var lineList = text.Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();

		if (lineList == null || lineList.Count == 0)
			return;

		for (int i = 0; i < lineList.Count; i++)
		{
			var line = lineList[i];

			if (line.Length <= size.Item1) // line is valid length
				continue;

			var lastLineIndex = size.Item1 - 1;
			var newLineIndex = isWordWrapping ?
				GetSafeNewLineIndex(line, (uint)lastLineIndex) : lastLineIndex;

			// end of line? can't word wrap, proceed to symbol wrap
			if (newLineIndex == 0)
			{
				lineList[i] = line[0..size.Item1];
				lineList.Insert(i + 1, line[size.Item1..line.Length]);
				continue;
			}

			// otherwise wordwrap
			var endIndex = newLineIndex + (isWordWrapping ? 0 : 1);
			lineList[i] = line[0..endIndex];
			lineList.Insert(i + 1, line[(newLineIndex + 1)..line.Length]);
		}
		var yDiff = size.Item2 - lineList.Count;

		if (alignment == Alignment.Left ||
			alignment == Alignment.Center ||
			alignment == Alignment.Right)
		{
			for (int i = 0; i < yDiff / 2; i++)
				lineList.Insert(0, "");
		}
		else if (alignment == Alignment.BottomLeft ||
			alignment == Alignment.Bottom ||
			alignment == Alignment.BottomRight)
		{
			for (int i = 0; i < yDiff; i++)
				lineList.Insert(0, "");
		}
		// new lineList.Count
		yDiff = size.Item2 - lineList.Count;

		var startIndex = 0;
		var end = startIndex + size.Item2;
		var scrollValue = (int)Math.Round(scrollProgress * (lineList.Count - size.Item2));

		if (yDiff < 0)
		{
			startIndex += scrollValue;
			end += scrollValue;
		}

		var e = lineList.Count - size.Item2;
		startIndex = Math.Clamp(startIndex, 0, Math.Max(e, 0));
		end = Math.Clamp(end, 0, lineList.Count);

		for (int i = startIndex; i < end; i++)
		{
			var line = lineList[i].Replace('\n', ' ');

			if (isWordWrapping == false && i > size.Item1)
				NewLine();

			if (alignment == Alignment.TopRight ||
				alignment == Alignment.Right ||
				alignment == Alignment.BottomRight)
				line = line.PadLeft(size.Item1);
			else if (alignment == Alignment.TopUp ||
				alignment == Alignment.Center ||
				alignment == Alignment.Bottom)
				line = PadLeftAndRight(line, size.Item1);

			SetTextLine((x, y), line, tint);
			NewLine();
		}

		void NewLine()
		{
			x = cell.Item1;
			y++;
		}
		int GetSafeNewLineIndex(string line, uint endLineIndex)
		{
			for (int i = (int)endLineIndex; i >= 0; i--)
				if (line[i] == ' ' && i <= size.Item1)
					return i;

			return default;
		}
	}
	public void SetTextSquareTint((int x, int y) cell, (int width, int height) size, string text, uint tint = uint.MaxValue, bool isMatchingWord = false)
	{
		if (string.IsNullOrWhiteSpace(text))
			return;

		var xStep = size.Item1 < 0 ? -1 : 1;
		var yStep = size.Item2 < 0 ? -1 : 1;
		var tileList = TilesFrom(text).ToList();

		for (int x = cell.Item1; x != cell.Item1 + size.Item1; x += xStep)
			for (int y = cell.Item2; y != cell.Item2 + size.Item2; y += yStep)
			{
				if (tileList[0] != TileAt((x, y)).ID)
					continue;

				var correctSymbCount = 0;
				var curX = x;
				var curY = y;
				var startPos = (x - 1, y);

				for (int i = 0; i < text.Length; i++)
				{
					if (tileList[i] != TileAt((curX, curY)).ID)
						break;

					correctSymbCount++;
					curX++;

					if (curX > x + size.Item1) // try new line
					{
						curX = cell.Item1;
						curY++;
					}
				}

				var endPos = (curX, curY);
				var left = TileAt(startPos).ID == 0 || curX == cell.Item1;
				var right = TileAt(endPos).ID == 0 || curX == cell.Item1 + size.Item1;
				var isWord = left && right;

				if (isWord ^ isMatchingWord)
					continue;

				if (text.Length != correctSymbCount)
					continue;

				curX = x;
				curY = y;
				for (int i = 0; i < text.Length; i++)
				{
					if (curX > x + size.Item1) // try new line
					{
						curX = cell.Item1;
						curY++;
					}

					SetTile((curX, curY), new(TileAt((curX, curY)).ID, tint));
					curX++;
				}
			}
	}

	public void SetBorder((int x, int y) cell, (int width, int height) size, int tileCorner, int tileStraight, uint tint = uint.MaxValue)
	{
		var (x, y) = cell;
		var (w, h) = size;

		SetTile(cell, new(tileCorner, tint, 0));
		SetSquare((x + 1, y), (w - 2, 1), new(tileStraight, tint, 0));
		SetTile((x + w - 1, y), new(tileCorner, tint, 1));

		SetSquare((x, y + 1), (1, h - 2), new(tileStraight, tint, 3));
		SetSquare((x + 1, y + 1), (w - 2, h - 2), new(0, 0));
		SetSquare((x + w - 1, y + 1), (1, h - 2), new(tileStraight, tint, 1));

		SetTile((x, y + h - 1), new(tileCorner, tint, 3));
		SetSquare((x + 1, y + h - 1), (w - 2, 1), new(tileStraight, tint, 2));
		SetTile((x + w - 1, y + h - 1), new(tileCorner, tint, 2));
	}
	public void SetBar((int x, int y) cell, int tileEdge, int tileStraight, uint tint = uint.MaxValue, int size = 5, bool isVertical = false)
	{
		var (x, y) = cell;
		if (isVertical)
		{
			SetTile(cell, new(tileEdge, tint, 1));
			SetSquare((x, y + 1), (1, size - 2), new(tileStraight, tint, 1));
			SetTile((x, y + size - 1), new(tileEdge, tint, 3));
			return;
		}

		SetTile(cell, new(tileEdge, tint));
		SetSquare((x + 1, y), (size - 2, 1), new(tileStraight, tint));
		SetTile((x + size - 1, y), new(tileEdge, tint, 2));
	}

	public (float x, float y) PointFrom((int x, int y) screenPixel, (int width, int height) windowSize, bool isAccountingForCamera = true)
	{
		var x = Map(screenPixel.x, 0, windowSize.width, 0, Size.width);
		var y = Map(screenPixel.y, 0, windowSize.height, 0, Size.height);

		if (isAccountingForCamera)
		{
			x += CameraPosition.x;
			y += CameraPosition.y;
		}

		return (x, y);
	}

	public static int TileFrom(char symbol)
	{
		var index = default(int);
		if (symbol >= 'A' && symbol <= 'Z')
			index = symbol - 'A' + 78;
		else if (symbol >= 'a' && symbol <= 'z')
			index = symbol - 'a' + 104;
		else if (symbol >= '0' && symbol <= '9')
			index = symbol - '0' + 130;
		else if (symbolMap.ContainsKey(symbol))
			index = symbolMap[symbol];

		return index;
	}
	public static int[] TilesFrom(string text)
	{
		if (text == null || text.Length == 0)
			return Array.Empty<int>();

		var result = new int[text.Length];
		for (int i = 0; i < text.Length; i++)
			result[i] = TileFrom(text[i]);

		return result;
	}

	public static implicit operator Tilemap(Tile[,] data) => new(data);
	public static implicit operator Tile[,](Tilemap tilemap) => Copy(tilemap.data);

	public (int tile, uint tint, sbyte angle, (bool isFlippedH, bool isFlippedV) flips)[,] ToBundle()
	{
		var result = new (int, uint, sbyte, (bool, bool))[data.GetLength(0), data.GetLength(1)];
		for (int j = 0; j < data.GetLength(1); j++)
			for (int i = 0; i < data.GetLength(0); i++)
				result[i, j] = data[i, j];

		return result;
	}

	#region Backend
	// save format
	// [amount of bytes]		- data
	// --------------------------------
	// [4]						- width
	// [4]						- height
	// [width * height * 4]		- tiles
	// [width * height * 4]		- tints
	// [width * height]			- angles
	// [width * height]			- flips

	private static readonly Dictionary<char, int> symbolMap = new()
		{
			{ '░', 2 }, { '▒', 5 }, { '▓', 7 }, { '█', 10 },

			{ '⅛', 140 }, { '⅐', 141 }, { '⅙', 142 }, { '⅕', 143 }, { '¼', 144 },
			{ '⅓', 145 }, { '⅜', 146 }, { '⅖', 147 }, { '½', 148 }, { '⅗', 149 },
			{ '⅝', 150 }, { '⅔', 151 }, { '¾', 152 },  { '⅘', 153 },  { '⅚', 154 },  { '⅞', 155 },

			{ '₀', 156 }, { '₁', 157 }, { '₂', 158 }, { '₃', 159 }, { '₄', 160 },
			{ '₅', 161 }, { '₆', 162 }, { '₇', 163 }, { '₈', 164 }, { '₉', 165 },

			{ '⁰', 169 }, { '¹', 170 }, { '²', 171 }, { '³', 172 }, { '⁴', 173 },
			{ '⁵', 174 }, { '⁶', 175 }, { '⁷', 176 }, { '⁸', 177 }, { '⁹', 178 },

			{ '+', 182 }, { '-', 183 }, { '×', 184 }, { '―', 185 }, { '÷', 186 }, { '%', 187 },
			{ '=', 188 }, { '≠', 189 }, { '≈', 190 }, { '√', 191 }, { '∫', 193 }, { 'Σ', 194 },
			{ 'ε', 195 }, { 'γ', 196 }, { 'ϕ', 197 }, { 'π', 198 }, { 'δ', 199 }, { '∞', 200 },
			{ '≪', 204 }, { '≫', 205 }, { '≤', 206 }, { '≥', 207 }, { '<', 208 }, { '>', 209 },
			{ '(', 210 }, { ')', 211 }, { '[', 212 }, { ']', 213 }, { '{', 214 }, { '}', 215 },
			{ '⊥', 216 }, { '∥', 217 }, { '∠', 218 }, { '∟', 219 }, { '~', 220 }, { '°', 221 },
			{ '℃', 222 }, { '℉', 223 }, { '*', 224 }, { '^', 225 }, { '#', 226 }, { '№', 227 },
			{ '$', 228 }, { '€', 229 }, { '£', 230 }, { '¥', 231 }, { '¢', 232 }, { '¤', 233 },

			{ '!', 234 }, { '?', 235 }, { '.', 236 }, { ',', 237 }, { '…', 238 },
			{ ':', 239 }, { ';', 240 }, { '"', 241 }, { '\'', 242 }, { '`', 243 }, { '–', 244 },
			{ '_', 245 }, { '|', 246 }, { '/', 247 }, { '\\', 248 }, { '@', 249 }, { '&', 250 },
			{ '®', 251 }, { '℗', 252 }, { '©', 253 }, { '™', 254 },

			{ '→', 282 }, { '↓', 283 }, { '←', 284 }, { '↑', 285 },
			{ '⇨', 330 }, { '⇩', 331 }, { '⇦', 332 }, { '⇧', 333 },
			{ '➡', 334 }, { '⬇', 335 }, { '⬅', 336 }, { '⬆', 337 },
			{ '⭆', 356 }, { '⤋', 357 }, { '⭅', 358 }, { '⤊', 359 },
			{ '⇻', 360 }, { '⇟', 361 }, { '⇺', 362 }, { '⇞', 363 },

			{ '│', 260 }, { '─', 261 }, { '┌', 262 }, { '┐', 263 }, { '┘', 264 }, { '└', 265 },
			{ '├', 266 }, { '┤', 267 }, { '┴', 268 }, { '┬', 269 }, { '┼', 270 },
			{ '║', 286 }, { '═', 287 }, { '╔', 288 }, { '╗', 289 }, { '╝', 290 }, { '╚', 291 },
			{ '╠', 292 }, { '╣', 293 }, { '╩', 294 }, { '╦', 295 }, { '╬', 296 },

			{ '♩', 409 }, { '♪', 410 }, { '♫', 411 }, { '♬', 412 }, { '♭', 413 }, { '♮', 414 },
			{ '♯', 415 },

			{ '★', 385 }, { '☆', 386 }, { '✓', 390 }, { '⏎', 391 },

			{ '●', 475 }, { '○', 478 }, { '■', 469 }, { '□', 472 }, { '▲', 480 }, { '△', 482 },

			{ '♟', 456 }, { '♜', 457 }, { '♞', 458 }, { '♝', 459 }, { '♛', 460 }, { '♚', 461 },
			{ '♙', 462 }, { '♖', 463 }, { '♘', 464 }, { '♗', 465 }, { '♕', 466 }, { '♔', 467 },
			{ '♠', 448 }, { '♥', 449 }, { '♣', 450 }, { '♦', 451 },
			{ '♤', 452 }, { '♡', 453 }, { '♧', 454 }, { '♢', 455 },

			{ '▕', 484 }, { '▁', 485 }, { '▏', 486 }, { '▔', 487 },
		};

	private readonly Tile[,] data;
	//private readonly int[,] tiles;
	//private readonly uint[,] tints;
	//private readonly sbyte[,] angles;
	//private readonly (bool, bool)[,] flips;

	private bool IndicesAreValid((int, int) indices)
	{
		return indices.Item1 >= 0 && indices.Item2 >= 0 &&
			indices.Item1 < data.GetLength(0) && indices.Item2 < data.GetLength(1);
	}
	private static string PadLeftAndRight(string text, int length)
	{
		var spaces = length - text.Length;
		var padLeft = spaces / 2 + text.Length;
		return text.PadLeft(padLeft).PadRight(length);

	}
	private static float Map(float number, float a1, float a2, float b1, float b2)
	{
		var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
		return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
	}
	private static T[,] Copy<T>(T[,] array)
	{
		var copy = new T[array.GetLength(0), array.GetLength(1)];
		Array.Copy(array, copy, array.Length);
		return copy;
	}

	private static byte[] ToBytes<T>(T[,] array) where T : struct
	{
		var size = array.GetLength(0) * array.GetLength(1) * Marshal.SizeOf(typeof(T));
		var buffer = new byte[size];
		Buffer.BlockCopy(array, 0, buffer, 0, buffer.Length);
		return buffer;
	}
	private static void FromBytes<T>(T[,] array, byte[] buffer) where T : struct
	{
		var size = array.GetLength(0) * array.GetLength(1);
		var len = Math.Min(size * Marshal.SizeOf(typeof(T)), buffer.Length);
		Buffer.BlockCopy(buffer, 0, array, 0, len);
	}

	private static byte[] Compress(byte[] data)
	{
		var output = new MemoryStream();
		using (var stream = new DeflateStream(output, CompressionLevel.Optimal))
			stream.Write(data, 0, data.Length);

		return output.ToArray();
	}
	private static byte[] Decompress(byte[] data)
	{
		var input = new MemoryStream(data);
		var output = new MemoryStream();
		using (var stream = new DeflateStream(input, CompressionMode.Decompress))
			stream.CopyTo(output);

		return output.ToArray();
	}
	#endregion
}
