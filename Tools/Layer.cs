namespace Purity.Tools
{
	public class Layer
	{
		public enum Alignment
		{
			UpLeft, Up, UpRight,
			Left, Center, Right,
			DownLeft, Down, DownRight
		};

		public uint[,] Cells { get; }
		public byte[,] Colors { get; }

		public uint CellTotalCount => CellCount.Item1 * CellCount.Item2;
		public (uint, uint) CellCount { get; }

		public Layer((uint, uint) cellCount)
		{
			CellCount = cellCount;
			Cells = new uint[cellCount.Item1, cellCount.Item2];
			Colors = new byte[cellCount.Item1, cellCount.Item2];
		}

		public uint GetCell((int, int) indices)
		{
			return IndicesAreValid(indices) ? Cells[indices.Item1, indices.Item2] : default;
		}
		public uint GetCell(char symbol)
		{
			var index = default(uint);
			if(symbol >= 'A' && symbol <= 'Z')
				index = (uint)(symbol - 'A' + 78);
			else if(symbol >= 'a' && symbol <= 'z')
				index = (uint)(symbol - 'a' + 104);
			else if(symbol >= '0' && symbol <= '9')
				index = (uint)(symbol - '0' + 130);
			else if(map.ContainsKey(symbol))
				index = map[symbol];

			return index;
		}

		public void Fill(uint cell, byte color)
		{
			for(uint y = 0; y < CellCount.Item2; y++)
				for(uint x = 0; x < CellCount.Item1; x++)
				{
					Cells[x, y] = cell;
					Colors[x, y] = color;
				}
		}
		public void SetCell((int, int) indices, uint cell, byte color)
		{
			if(IndicesAreValid(indices) == false)
				return;

			var x = indices.Item1;
			var y = indices.Item2;
			Cells[x, y] = cell;
			Colors[x, y] = color;
		}
		public void SetCell(uint index, uint cell, byte color)
		{
			var i = Math.Clamp(index, 0, CellTotalCount);
			var coords = IndexToCoords(i);
			if(IndicesAreValid(coords) == false)
				return;

			Cells[coords.Item1, coords.Item2] = cell;
			Colors[coords.Item1, coords.Item2] = color;
		}
		public void SetSquare((int, int) indices, (int, int) size, uint cell, byte color)
		{
			var xStep = size.Item1 < 0 ? -1 : 1;
			var yStep = size.Item2 < 0 ? -1 : 1;
			for(int x = indices.Item1; x != indices.Item1 + size.Item1; x += xStep)
				for(int y = indices.Item2; y != indices.Item2 + size.Item2; y += yStep)
				{
					if(x < 0 || y < 0)
						continue;

					SetCell((x, y), cell, color);
				}
		}
		public void SetTextLine((int, int) indices, string text, byte color)
		{
			var errorOffset = 0;
			for(int i = 0; i < text?.Length; i++)
			{
				var symbol = text[i];
				var index = GetCell(symbol);

				if(index == default && symbol != ' ')
				{
					errorOffset++;
					continue;
				}

				SetCell((indices.Item1 + i - errorOffset, indices.Item2), index, color);
			}
		}
		public void SetTextBox((int, int) indices, (int, int) size, byte color,
			bool isWordWrapping, Alignment alignment, params string[] lines)
		{
			if(lines == null || lines.Length == 0 ||
				size.Item1 <= 0 || size.Item2 <= 0)
				return;

			var x = indices.Item1;
			var y = indices.Item2;


			var lineList = new List<string>(lines);

			if(lineList == null || lineList.Count == 0)
				return;

			for(int i = 0; i < lineList.Count - 1; i++)
				lineList[i] = lineList[i] + '\n';

			for(int i = 0; i < lineList.Count; i++)
			{
				var line = lineList[i];

				for(int j = 0; j < line.Length; j++)
				{
					var isSymbolNewLine = line[j] == '\n' && j != line.Length - 1 && j > size.Item1;
					var isEndOfLine = j > size.Item1;

					if(j == line.Length - 1 && line[j] == '\n')
					{
						j--;
						line = line[0..^1];
						lineList[i] = line;
					}

					if(isEndOfLine ^ isSymbolNewLine)
					{
						var newLineIndex = isWordWrapping && isSymbolNewLine == false ?
							GetSafeNewLineIndex(line, (uint)j) : j;

						// end of line? can't word wrap, proceed to symbol wrap
						if(newLineIndex == 0)
						{
							lineList[i] = line[0..size.Item1];
							var newLineSymbol = line[size.Item1..line.Length];
							if(i == lineList.Count - 1)
							{
								lineList.Add(newLineSymbol);
								break;
							}
							lineList[i + 1] = $"{newLineSymbol} {lineList[i + 1]}";
							break;
						}

						lineList[i] = line[0..newLineIndex];

						var newLine = isWordWrapping ?
							line[(newLineIndex + 1)..^0] : line[j..^0];

						if(i == lineList.Count - 1)
						{
							lineList.Add(newLine);
							break;
						}

						var space = newLine.EndsWith('\n') ? "" : " ";
						lineList[i + 1] = $"{newLine}{space}{lineList[i + 1]}";
						break;
					}
				}
				if(i > size.Item2)
					break;
			}

			var yDiff = size.Item2 - lineList.Count;

			if(yDiff > 1)
			{
				if(alignment == Alignment.Left ||
					alignment == Alignment.Center ||
					alignment == Alignment.Right)
					for(int i = 0; i < yDiff / 2; i++)
						lineList.Insert(0, "");

				else if(alignment == Alignment.DownLeft ||
					alignment == Alignment.Down ||
					alignment == Alignment.DownRight)
					for(int i = 0; i < yDiff; i++)
						lineList.Insert(0, "");
			}

			for(int i = 0; i < lineList.Count; i++)
			{
				if(i >= size.Item2)
					return;

				var line = lineList[i].Replace('\n', ' ');

				if(isWordWrapping == false && i > size.Item1)
					NewLine();

				if(alignment == Alignment.UpRight ||
					alignment == Alignment.Right ||
					alignment == Alignment.DownRight)
					line = line.PadLeft(size.Item1);
				else if(alignment == Alignment.Up ||
					alignment == Alignment.Center ||
					alignment == Alignment.Down)
					line = PadLeftAndRight(line, size.Item1);

				SetTextLine((x, y), line, color);
				NewLine();
			}

			void NewLine()
			{
				x = indices.Item1;
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

		#region Backend
		private readonly Dictionary<char, uint> map = new()
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

		private (int, int) IndexToCoords(uint index)
		{
			var i = (int)index;
			var width = (int)CellCount.Item1;
			var height = (int)CellCount.Item2;
			i = i < 0 ? 0 : i;
			i = i > width * height - 1 ? width * height - 1 : i;

			return (i % width, i / width);
		}
		private bool IndicesAreValid((int, int) indices)
		{
			return indices.Item1 < Cells.GetLength(0) && indices.Item2 < Cells.GetLength(1);
		}
		private static string PadLeftAndRight(string text, int length)
		{
			var spaces = length - text.Length;
			var padLeft = spaces / 2 + text.Length;
			return text.PadLeft(padLeft).PadRight(length);

		}
		#endregion
	}
}
