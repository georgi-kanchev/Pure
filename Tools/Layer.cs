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
		public void SetSquare((uint, uint) startIndices, (uint, uint) endIndices, uint cell, byte color)
		{
			for(uint y = startIndices.Item2; y < endIndices.Item2 + 1; y++)
				for(uint x = startIndices.Item1; x < endIndices.Item1 + 1; x++)
					SetCell((x, y), cell, color);
		}
		public void SetText(string text, int x, int y)
		{
			for(int i = 0; i < text.Length; i++)
			{
				var symbol = text[i];
			}

		}

		#region Backend
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
