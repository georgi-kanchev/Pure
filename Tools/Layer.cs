namespace Purity.Tools
{
	public class Layer
	{
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

		public uint GetCell((uint, uint) indices)
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
		public void SetCell((uint, uint) indices, uint cell, byte color)
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
		public void SetSquare((uint, uint) startIndices, (uint, uint) endIndices,
			uint cell, byte color)
		{
			for(uint y = startIndices.Item2; y < endIndices.Item2 + 1; y++)
				for(uint x = startIndices.Item1; x < endIndices.Item1 + 1; x++)
					SetCell((x, y), cell, color);
		}
		public void SetTextLine((uint, uint) indices, string text, byte color)
		{
			for(uint i = 0; i < text.Length; i++)
			{
				var symbol = text[(int)i];
				var index = GetCell(symbol);

				if(index == default && symbol != ' ')
					return;

				SetCell((indices.Item1 + i, indices.Item2), index, color);
			}
		}

		#region Backend
		private readonly Dictionary<char, uint> map = new()
		{
			{ '░', 1 }, { '▒', 4 }, { '▓', 8 }, { '█', 11 },

			{ '⅛', 140 }, { '⅐', 141 }, { '⅙', 142 }, { '⅕', 143 }, { '¼', 144 },
			{ '⅓', 145 }, { '⅜', 146 }, { '⅖', 147 }, { '½', 148 }, { '⅗', 149 },
			{ '⅝', 150 }, { '⅔', 151 }, { '¾', 152 },  { '⅘', 153 },  { '⅚', 154 },  { '⅞', 155 },

			{ '⁰', 156 }, { '¹', 157 }, { '²', 158 }, { '³', 159 }, { '⁴', 160 },
			{ '⁵', 161 }, { '⁶', 162 }, { '⁷', 163 }, { '⁸', 164 }, { '⁹', 165 },

			{ '₀', 169 }, { '₁', 170 }, { '₂', 171 }, { '₃', 172 }, { '₄', 173 },
			{ '₅', 174 }, { '₆', 175 }, { '₇', 176 }, { '₈', 177 }, { '₉', 178 },

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

			{ '●', 476 }, { '○', 479 }, { '■', 470 }, { '□', 473 }, { '▲', 483 }, { '△', 481 },

			{ '♟', 456 }, { '♜', 457 }, { '♞', 458 }, { '♝', 459 }, { '♛', 460 }, { '♚', 461 },
			{ '♙', 462 }, { '♖', 463 }, { '♘', 464 }, { '♗', 465 }, { '♕', 466 }, { '♔', 467 },
			{ '♠', 448 }, { '♥', 449 }, { '♣', 450 }, { '♦', 451 },
			{ '♤', 452 }, { '♡', 453 }, { '♧', 454 }, { '♢', 455 },
		};

		private (uint, uint) IndexToCoords(uint index)
		{
			var width = CellCount.Item1;
			var height = CellCount.Item2;
			index = index < 0 ? 0 : index;
			index = index > width * height - 1 ? width * height - 1 : index;

			return (index % width, index / width);
		}
		private bool IndicesAreValid((uint, uint) indices)
		{
			return indices.Item1 < Cells.GetLength(0) && indices.Item2 < Cells.GetLength(1);
		}
		#endregion
	}
}
