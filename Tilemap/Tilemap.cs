using System.IO.Compression;
using System.Runtime.InteropServices;

namespace Pure.Tilemap;

/// <summary>
/// Represents a tilemap consisting of a grid of tiles.
/// </summary>
public class Tilemap
{
	/// <summary>
	/// Specifies the alignment of the text in <see cref="SetTextSquare"/>.
	/// </summary>
	public enum Alignment
	{
		TopLeft, TopUp, TopRight,
		Left, Center, Right,
		BottomLeft, Bottom, BottomRight
	};

	/// <summary>
	/// Gets the size of the tilemap in tiles.
	/// </summary>
	public (int width, int height) Size => (data.GetLength(0), data.GetLength(1));

	/// <summary>
	/// Gets or sets the position of the camera.
	/// </summary>
	public (int x, int y) CameraPosition { get; set; }
	/// <summary>
	/// Gets or sets the size of the camera viewport.
	/// </summary>
	public (int width, int height) CameraSize { get; set; }

	/// <summary>
	/// Gets the identifiers of the tiles in the tilemap.
	/// </summary>
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

	/// <summary>
	/// Initializes a new tilemap instance and loads tile data into it from the specified file.
	/// </summary>
	/// <param name="path">The path to the tilemap file.</param>
	/// <exception cref="ArgumentException">Thrown if the tilemap could not be 
	/// loaded from the specified file or the file was not found.</exception>
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
		catch (ArgumentException)
		{
			throw new ArgumentException($"Could not load {nameof(Tilemap)} from '{path}'.");
		}
	}
	/// <summary>
	/// Initializes a new tilemap instance with the specified <paramref name="size"/>.
	/// </summary>
	/// <param name="size">The size of the tilemap in tiles.</param>
	public Tilemap((int width, int height) size)
	{
		var (w, h) = size;

		if (w < 1)
			w = 1;
		if (h < 1)
			h = 1;

		data = new Tile[w, h];
		CameraSize = size;
	}
	/// <summary>
	/// Initializes a new tilemap instance with the specified <paramref name="tileData"/>.
	/// </summary>
	/// <param name="tileData">The tile data to use for the tilemap.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="tileData"/> is null.</exception>
	public Tilemap(Tile[,] tileData)
	{
		if (tileData == null)
			throw new ArgumentNullException(nameof(tileData));

		var w = tileData.GetLength(0);
		var h = tileData.GetLength(1);

		this.data = Copy(tileData);
		CameraSize = Size;
	}

	/// <summary>
	/// Saves the tilemap to the specified file.
	/// </summary>
	/// <param name="path">The path to the file to save the tilemap to.</param>
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

	/// <summary>
	/// Updates the camera's view of the tilemap.
	/// </summary>
	/// <returns>The updated tilemap view.</returns>
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

	/// <summary>
	/// Gets the tile at the specified <paramref name="position"/>.
	/// </summary>
	/// <param name="position">The position to get the tile from.</param>
	/// <returns>The tile at the specified <paramref name="position"/>, 
	/// or the default tile value if the <paramref name="position"/> is out of bounds.</returns>
	public Tile TileAt((int x, int y) position)
	{
		return IndicesAreValid(position) ? data[position.x, position.y] : default;
	}

	/// <summary>
	/// Fills the entire tilemap with the specified tile.
	/// </summary>
	/// <param name="withTile">The tile to fill the tilemap with.</param>
	public void Fill(Tile withTile = default)
	{
		for (int y = 0; y < Size.Item2; y++)
			for (int x = 0; x < Size.Item1; x++)
				SetTile((x, y), withTile);
	}

	/// <summary>
	/// Sets the tile at the specified <paramref name="position"/> 
	/// to the specified <paramref name="tile"/>.
	/// </summary>
	/// <param name="position">The position to set the tile at.</param>
	/// <param name="tile">The tile to set.</param>
	public void SetTile((int x, int y) position, Tile tile)
	{
		if (IndicesAreValid(position) == false)
			return;

		data[position.x, position.y] = tile;
	}
	/// <summary>
	/// Sets a square region of tiles starting at the specified <paramref name="position"/> with the 
	/// specified <paramref name="size"/> to the specified <paramref name="tile"/>.
	/// </summary>
	/// <param name="position">The position to start setting tiles from.</param>
	/// <param name="size">The size of the square region to set tiles for.</param>
	/// <param name="tile">The tile to set the square region to.</param>
	public void SetSquare((int x, int y) position, (int width, int height) size, Tile tile)
	{
		var xStep = size.Item1 < 0 ? -1 : 1;
		var yStep = size.Item2 < 0 ? -1 : 1;
		var i = 0;
		for (int x = position.Item1; x != position.Item1 + size.Item1; x += xStep)
			for (int y = position.Item2; y != position.Item2 + size.Item2; y += yStep)
			{
				if (i > Math.Abs(size.Item1 * size.Item2))
					return;

				SetTile((x, y), tile);
				i++;
			}
	}
	/// <summary>
	/// Sets a group of tiles starting at the specified <paramref name="position"/> to the 
	/// specified 2D tile array.
	/// </summary>
	/// <param name="position">The position to start setting tiles from.</param>
	/// <param name="tiles">The 2D array of tiles to set.</param>
	public void SetGroup((int x, int y) position, Tile[,] tiles)
	{
		if (tiles == null || tiles.Length == 0)
			return;

		for (int i = 0; i < tiles.GetLength(1); i++)
			for (int j = 0; j < tiles.GetLength(0); j++)
				SetTile((position.x + i, position.y + j), tiles[j, i]);
	}

	/// <summary>
	/// Sets a single line of <paramref name="text"/> starting from a <paramref name="position"/>
	/// with optional <paramref name="tint"/>.
	/// </summary>
	/// <param name="position">The starting position to place the text.</param>
	/// <param name="text">The text to display.</param>
	/// <param name="tint">Optional tint color value (defaults to white).</param>
	public void SetTextLine((int x, int y) position, string text, uint tint = uint.MaxValue)
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

			SetTile((position.Item1 + i - errorOffset, position.Item2), new(index, tint));
		}
	}
	/// <summary>
	/// Sets a square of <paramref name="text"/> with optional 
	/// <paramref name="alignment"/>, scrolling, and word wrapping.
	/// </summary>
	/// <param name="position">The starting position to place the <paramref name="text"/>.</param>
	/// <param name="size">The width and height of the square.</param>
	/// <param name="text">The <paramref name="text"/> to display.</param>
	/// <param name="tint">Optional tint color value (defaults to white).</param>
	/// <param name="isWordWrapping">Optional flag for enabling word wrapping.</param>
	/// <param name="alignment">Optional <paramref name="text"/> alignment.</param>
	/// <param name="scrollProgress">Optional scrolling value (between 0 and 1).</param>
	public void SetTextSquare((int x, int y) position, (int width, int height) size, string text, uint tint = uint.MaxValue, bool isWordWrapping = true, Alignment alignment = Alignment.TopLeft, float scrollProgress = 0)
	{
		if (text == null || text.Length == 0 ||
			size.Item1 <= 0 || size.Item2 <= 0)
			return;

		var x = position.Item1;
		var y = position.Item2;
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
			x = position.Item1;
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
	/// <summary>
	/// Sets the <paramref name="tint"/> of the tiles in a rectangular area of the tilemap to 
	/// highlight a specific <paramref name="text"/> (if found).
	/// </summary>
	/// <param name="position">The position of the top-left corner of the rectangular 
	/// area to search for the <paramref name="text"/>.</param>
	/// <param name="size">The size of the rectangular area to search for the <paramref name="text"/>.</param>
	/// <param name="text">The <paramref name="text"/> to search for and highlight.</param>
	/// <param name="tint">The color to tint the matching tiles.</param>
	/// <param name="isMatchingWord">Whether to only match the <paramref name="text"/> 
	/// as a whole word or any symbols.</param>
	public void SetTextSquareTint((int x, int y) position, (int width, int height) size, string text, uint tint = uint.MaxValue, bool isMatchingWord = false)
	{
		if (string.IsNullOrWhiteSpace(text))
			return;

		var xStep = size.Item1 < 0 ? -1 : 1;
		var yStep = size.Item2 < 0 ? -1 : 1;
		var tileList = TilesFrom(text).ToList();

		for (int x = position.Item1; x != position.Item1 + size.Item1; x += xStep)
			for (int y = position.Item2; y != position.Item2 + size.Item2; y += yStep)
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
						curX = position.Item1;
						curY++;
					}
				}

				var endPos = (curX, curY);
				var left = TileAt(startPos).ID == 0 || curX == position.Item1;
				var right = TileAt(endPos).ID == 0 || curX == position.Item1 + size.Item1;
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
						curX = position.Item1;
						curY++;
					}

					SetTile((curX, curY), new(TileAt((curX, curY)).ID, tint));
					curX++;
				}
			}
	}

	/// <summary>
	/// Sets the tiles in a rectangular area of the tilemap to create a border.
	/// </summary>
	/// <param name="position">The position of the top-left corner of the rectangular 
	/// area to create the border.</param>
	/// <param name="size">The size of the rectangular area to create the border.</param>
	/// <param name="tileIdCorner">The identifier of the tile to use for the corners of the border.</param>
	/// <param name="tileIdStraight">The identifier of the tile to use for the 
	/// straight edges of the border.</param>
	/// <param name="tint">The color to tint the border tiles.</param>
	public void SetBorder((int x, int y) position, (int width, int height) size, int tileIdCorner, int tileIdStraight, uint tint = uint.MaxValue)
	{
		var (x, y) = position;
		var (w, h) = size;

		SetTile(position, new(tileIdCorner, tint, 0));
		SetSquare((x + 1, y), (w - 2, 1), new(tileIdStraight, tint, 0));
		SetTile((x + w - 1, y), new(tileIdCorner, tint, 1));

		SetSquare((x, y + 1), (1, h - 2), new(tileIdStraight, tint, 3));
		SetSquare((x + 1, y + 1), (w - 2, h - 2), new(0, 0));
		SetSquare((x + w - 1, y + 1), (1, h - 2), new(tileIdStraight, tint, 1));

		SetTile((x, y + h - 1), new(tileIdCorner, tint, 3));
		SetSquare((x + 1, y + h - 1), (w - 2, 1), new(tileIdStraight, tint, 2));
		SetTile((x + w - 1, y + h - 1), new(tileIdCorner, tint, 2));
	}
	/// <summary>
	/// Sets the tiles in a rectangular area of the tilemap to create a vertical or horizontal bar.
	/// </summary>
	/// <param name="position">The position of the top-left corner of the rectangular area 
	/// to create the bar.</param>
	/// <param name="tileIdEdge">The identifier of the tile to use for the edges of the bar.</param>
	/// <param name="tileIdStraight">The identifier of the tile to use for the 
	/// straight part of the bar.</param>
	/// <param name="tint">The color to tint the bar tiles.</param>
	/// <param name="size">The length of the bar in tiles.</param>
	/// <param name="isVertical">Whether the bar should be vertical or horizontal.</param>
	public void SetBar((int x, int y) position, int tileIdEdge, int tileIdStraight, uint tint = uint.MaxValue, int size = 5, bool isVertical = false)
	{
		var (x, y) = position;
		if (isVertical)
		{
			SetTile(position, new(tileIdEdge, tint, 1));
			SetSquare((x, y + 1), (1, size - 2), new(tileIdStraight, tint, 1));
			SetTile((x, y + size - 1), new(tileIdEdge, tint, 3));
			return;
		}

		SetTile(position, new(tileIdEdge, tint));
		SetSquare((x + 1, y), (size - 2, 1), new(tileIdStraight, tint));
		SetTile((x + size - 1, y), new(tileIdEdge, tint, 2));
	}

	/// <summary>
	/// Converts a screen <paramref name="pixelPosition"/> to a world point on the tilemap.
	/// </summary>
	/// <param name="pixelPosition">The screen pixel position to convert.</param>
	/// <param name="windowSize">The size of the application window.</param>
	/// <param name="isAccountingForCamera">Whether or not to account for the camera's position.</param>
	/// <returns>The world point corresponding to the given screen 
	/// <paramref name="pixelPosition"/>.</returns>
	public (float x, float y) PointFrom((int x, int y) pixelPosition, (int width, int height) windowSize, bool isAccountingForCamera = true)
	{
		var x = Map(pixelPosition.x, 0, windowSize.width, 0, Size.width);
		var y = Map(pixelPosition.y, 0, windowSize.height, 0, Size.height);

		if (isAccountingForCamera)
		{
			x += CameraPosition.x;
			y += CameraPosition.y;
		}

		return (x, y);
	}

	/// <summary>
	/// Converts a <paramref name="symbol"/> to its corresponding tile identifier.
	/// </summary>
	/// <param name="symbol">The symbol to convert.</param>
	/// <returns>The tile identifier corresponding to the given symbol.</returns>
	public static int TileFrom(char symbol)
	{
		var id = default(int);
		if (symbol >= 'A' && symbol <= 'Z')
			id = symbol - 'A' + 78;
		else if (symbol >= 'a' && symbol <= 'z')
			id = symbol - 'a' + 104;
		else if (symbol >= '0' && symbol <= '9')
			id = symbol - '0' + 130;
		else if (symbolMap.ContainsKey(symbol))
			id = symbolMap[symbol];

		return id;
	}
	/// <summary>
	/// Converts a <paramref name="text"/> to an array of tile identifiers.
	/// </summary>
	/// <param name="text">The <paramref name="text"/> to convert.</param>
	/// <returns>An array of tile identifiers corresponding to the given symbols.</returns>
	public static int[] TilesFrom(string text)
	{
		if (text == null || text.Length == 0)
			return Array.Empty<int>();

		var result = new int[text.Length];
		for (int i = 0; i < text.Length; i++)
			result[i] = TileFrom(text[i]);

		return result;
	}

	/// <summary>
	/// Implicitly converts a 2D array of tiles to a tilemap object.
	/// </summary>
	/// <param name="data">The 2D array of tiles to convert.</param>
	/// <returns>A new tilemap object containing the given tiles.</returns>
	public static implicit operator Tilemap(Tile[,] data) => new(data);
	/// <summary>
	/// Implicitly converts a tilemap object to a 2D array of tiles.
	/// </summary>
	/// <param name="tilemap">The tilemap object to convert.</param>
	/// <returns>A new 2D array of tiles containing the tiles from the tilemap object.</returns>
	public static implicit operator Tile[,](Tilemap tilemap) => Copy(tilemap.data);

	/// <returns>
	/// A 2D array of the bundle tuples of the tiles in the tilemap.</returns>
	public (int tile, uint tint, sbyte angle, (bool isHorizontal, bool isVertical) flips)[,] ToBundle()
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

	public static (int, int) FromIndex(int index, (int width, int height) size)
	{
		index = index < 0 ? 0 : index;
		index = index > size.Item1 * size.Item2 - 1 ? size.Item1 * size.Item2 - 1 : index;

		return (index % size.Item1, index / size.Item1);
	}
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
