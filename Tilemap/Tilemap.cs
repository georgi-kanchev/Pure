namespace Pure.Tilemap
{
	public class Tilemap
	{
		public enum Alignment
		{
			TopLeft, TopUp, TopRight,
			Left, Center, Right,
			BottomLeft, Bottom, BottomRight
		};

		public (int, int) Size => (tiles.GetLength(0), tiles.GetLength(1));

		public (int, int) CameraPosition { get; set; }
		public (int, int) CameraSize { get; set; }

		public Tilemap((uint, uint) tileCount)
		{
			var (w, h) = tileCount;
			tiles = new int[w, h];
			colors = new byte[w, h];
			CameraSize = ((int)w, (int)h);
		}
		public Tilemap(int[,] tiles, byte[,] colors)
		{
			if(tiles == null)
				throw new ArgumentNullException(nameof(tiles));
			if(colors == null)
				throw new ArgumentNullException(nameof(colors));

			if(tiles.Length != colors.Length)
				throw new ArgumentException($"The sizes of the {nameof(tiles)} and {nameof(colors)} cannot be different.");

			this.tiles = Copy(tiles);
			this.colors = Copy(colors);
			CameraSize = (tiles.GetLength(0), tiles.GetLength(1));
		}

		public Tilemap UpdateCamera()
		{
			var (w, h) = CameraSize;
			var (cx, cy) = CameraPosition;
			var tiles = new int[Math.Abs(w), Math.Abs(h)];
			var colors = new byte[Math.Abs(w), Math.Abs(h)];
			var xStep = w < 0 ? -1 : 1;
			var yStep = h < 0 ? -1 : 1;
			var i = 0;
			for(int x = cx; x != cx + w; x += xStep)
			{
				var j = 0;
				for(int y = cy; y != cy + h; y += yStep)
				{
					tiles[i, j] = TileAt((x, y));
					colors[i, j] = ColorAt((x, y));
					j++;
				}
				i++;
			}
			return new(tiles, colors);
		}

		public int TileAt((int, int) position)
		{
			return IndicesAreValid(position) ? tiles[position.Item1, position.Item2] : default;
		}
		public byte ColorAt((int, int) position)
		{
			return IndicesAreValid(position) ? colors[position.Item1, position.Item2] : default;
		}

		public void Fill(int tile = 0, byte color = 0)
		{
			for(uint y = 0; y < Size.Item2; y++)
				for(uint x = 0; x < Size.Item1; x++)
				{
					tiles[x, y] = tile;
					colors[x, y] = color;
				}
		}
		public void SetTile((int, int) position, int tile, byte color = 255)
		{
			if(IndicesAreValid(position) == false)
				return;

			var x = position.Item1;
			var y = position.Item2;
			tiles[x, y] = tile;
			colors[x, y] = color;
		}
		public void SetSquare((int, int) position, (int, int) size, int tile, byte color = 255)
		{
			var xStep = size.Item1 < 0 ? -1 : 1;
			var yStep = size.Item2 < 0 ? -1 : 1;
			for(int x = position.Item1; x != position.Item1 + size.Item1; x += xStep)
				for(int y = position.Item2; y != position.Item2 + size.Item2; y += yStep)
					SetTile((x, y), tile, color);
		}
		public void SetTextLine((int, int) position, string text, byte color = 255)
		{
			var errorOffset = 0;
			for(int i = 0; i < text?.Length; i++)
			{
				var symbol = text[i];
				var index = TileFrom(symbol);

				if(index == default && symbol != ' ')
				{
					errorOffset++;
					continue;
				}

				if(symbol == ' ')
					continue;

				SetTile((position.Item1 + i - errorOffset, position.Item2), index, color);
			}
		}
		public void SetTextBox((int, int) position, (int, int) size, string text, byte color = 255,
			bool isWordWrapping = true, Alignment alignment = Alignment.TopLeft, float scrollProgress = 0)
		{
			if(text == null || text.Length == 0 ||
				size.Item1 <= 0 || size.Item2 <= 0)
				return;

			var x = position.Item1;
			var y = position.Item2;
			var lineList = text.Split("\n", StringSplitOptions.RemoveEmptyEntries).ToList();

			if(lineList == null || lineList.Count == 0)
				return;

			for(int i = 0; i < lineList.Count; i++)
			{
				var line = lineList[i];

				if(line.Length <= size.Item1) // line is valid length
					continue;

				var lastLineIndex = size.Item1 - 1;
				var newLineIndex = isWordWrapping ?
					GetSafeNewLineIndex(line, (uint)lastLineIndex) : lastLineIndex;

				// end of line? can't word wrap, proceed to symbol wrap
				if(newLineIndex == 0)
				{
					lineList[i] = line[0..size.Item1];
					lineList.Insert(i + 1, line[size.Item1..line.Length]);
					continue;
				}

				// otherwise wordwrap
				lineList[i] = line[0..newLineIndex];
				lineList.Insert(i + 1, line[(newLineIndex + 1)..line.Length]);
			}
			var yDiff = size.Item2 - lineList.Count;

			if(alignment == Alignment.Left ||
				alignment == Alignment.Center ||
				alignment == Alignment.Right)
			{
				for(int i = 0; i < yDiff / 2; i++)
					lineList.Insert(0, "");
			}
			else if(alignment == Alignment.BottomLeft ||
				alignment == Alignment.Bottom ||
				alignment == Alignment.BottomRight)
			{
				for(int i = 0; i < yDiff; i++)
					lineList.Insert(0, "");
			}
			// new lineList.Count
			yDiff = size.Item2 - lineList.Count;

			var startIndex = 0;
			var end = startIndex + size.Item2;
			var scrollValue = (int)Math.Round(scrollProgress * (lineList.Count - size.Item2));

			if(yDiff < 0)
			{
				startIndex += scrollValue;
				end += scrollValue;
			}

			var e = lineList.Count - size.Item2;
			startIndex = Math.Clamp(startIndex, 0, Math.Max(e, 0));
			end = Math.Clamp(end, 0, lineList.Count);

			for(int i = startIndex; i < end; i++)
			{
				var line = lineList[i].Replace('\n', ' ');

				if(isWordWrapping == false && i > size.Item1)
					NewLine();

				if(alignment == Alignment.TopRight ||
					alignment == Alignment.Right ||
					alignment == Alignment.BottomRight)
					line = line.PadLeft(size.Item1);
				else if(alignment == Alignment.TopUp ||
					alignment == Alignment.Center ||
					alignment == Alignment.Bottom)
					line = PadLeftAndRight(line, size.Item1);

				SetTextLine((x, y), line, color);
				NewLine();
			}

			void NewLine()
			{
				x = position.Item1;
				y++;
			}
			int GetSafeNewLineIndex(string line, uint endLineIndex)
			{
				for(int i = (int)endLineIndex; i >= 0; i--)
					if(line[i] == ' ' && i <= size.Item1)
						return i;

				return default;
			}
		}
		public void SetBorder((int, int) position, (int, int) size, int tile, byte color = 255)
		{
			var (x, y) = position;
			var (w, h) = size;

			SetTile(position, tile, color);
			SetSquare((x + 1, y), (w - 2, 1), tile + 1, color);
			SetTile((x + w - 1, y), tile + 2, color);

			SetSquare((x, y + 1), (1, h - 2), tile + 3, color);
			SetSquare((x + 1, y + 1), (w - 2, h - 2), 0, 0);
			SetSquare((x + w - 1, y + 1), (1, h - 2), tile + 3, color);

			SetTile((x, y + h - 1), tile + 4, color);
			SetSquare((x + 1, y + h - 1), (w - 2, 1), tile + 1, color);
			SetTile((x + w - 1, y + h - 1), tile + 5, color);
		}
		public void SetBar((int, int) position, int tile, byte color = 255, int size = 5,
			bool isVertical = false)
		{
			var (x, y) = position;
			SetTile(position, tile, color);
			if(isVertical)
			{
				SetSquare((x, y + 1), (1, size - 2), tile + 1, color);
				SetTile((x, y + size - 1), tile + 2, color);
				return;
			}

			SetSquare((x + 1, y), (size - 2, 1), tile + 1, color);
			SetTile((x + size - 1, y), tile + 2, color);
		}

		public void SetInputLineCursor((int, int) position, bool isFocused, int cursorIndex,
			byte cursorColor)
		{
			if(isFocused == false)
				return;

			var cursorPos = (position.Item1 + cursorIndex, position.Item2);
			SetTile((cursorPos.Item1, cursorPos.Item2), Tile.SHAPE_LINE_LEFT, cursorColor);
		}
		public void SetInputLineSelection((int, int) position, int cursorIndex,
			int selectionIndex, byte selectionColor)
		{
			var cursorPos = (position.Item1 + cursorIndex, position.Item2);
			var selectedPos = (position.Item1 + selectionIndex, position.Item2);
			var size = cursorPos.Item1 - selectedPos.Item1;

			if(size < 0)
				selectedPos.Item1--;

			SetSquare(selectedPos, (size, 1), Tile.SHADE_OPAQUE, selectionColor);
		}

		public (float, float) PositionFrom((int, int) screenPixel, (uint, uint) windowSize,
			bool isAccountingForCamera = true)
		{
			var x = Map(screenPixel.Item1, 0, windowSize.Item1, 0, Size.Item1);
			var y = Map(screenPixel.Item2, 0, windowSize.Item2, 0, Size.Item2);

			if(isAccountingForCamera)
			{
				x += CameraPosition.Item1;
				y += CameraPosition.Item2;
			}

			return (x, y);
		}

		public static int TileFrom(char symbol)
		{
			var index = default(int);
			if(symbol >= 'A' && symbol <= 'Z')
				index = symbol - 'A' + 78;
			else if(symbol >= 'a' && symbol <= 'z')
				index = symbol - 'a' + 104;
			else if(symbol >= '0' && symbol <= '9')
				index = symbol - '0' + 130;
			else if(map.ContainsKey(symbol))
				index = map[symbol];

			return index;
		}

		public static implicit operator Tilemap(int[,] tiles)
			=> new(tiles, new byte[tiles.GetLength(0), tiles.GetLength(1)]);
		public static implicit operator int[,](Tilemap tilemap)
			=> Copy(tilemap.tiles);
		public static implicit operator Tilemap(byte[,] colors)
			=> new(new int[colors.GetLength(0), colors.GetLength(1)], colors);
		public static implicit operator byte[,](Tilemap tilemap)
			=> Copy(tilemap.colors);

		#region Backend
		private static readonly Dictionary<char, int> map = new()
		{
			{ '░', 1 }, { '▒', 4 }, { '▓', 8 }, { '█', 11 },

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
		};

		private readonly int[,] tiles;
		private readonly byte[,] colors;

		private bool IndicesAreValid((int, int) indices)
		{
			return indices.Item1 >= 0 && indices.Item2 >= 0 &&
				indices.Item1 < tiles.GetLength(0) && indices.Item2 < tiles.GetLength(1);
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
		#endregion
	}
}
