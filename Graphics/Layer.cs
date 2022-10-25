namespace Purity.Graphics
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

		public void Fill(uint cell, byte color)
		{
			for(uint y = 0; y < CellCount.Item2; y++)
				for(uint x = 0; x < CellCount.Item1; x++)
				{
					Cells[x, y] = cell;
					Colors[x, y] = color;
				}
		}
		public void SetCell(uint x, uint y, uint cell, byte color)
		{
			x = Math.Clamp(x, 0, CellCount.Item1 - 1);
			y = Math.Clamp(y, 0, CellCount.Item2 - 1);

			Cells[x, y] = cell;
			Colors[x, y] = color;
		}
		public void SetCell(uint index, uint cell, byte color)
		{
			var i = Math.Clamp(index, 0, CellTotalCount);
			var coords = IndexToCoords(i);

			Cells[coords.Item1, coords.Item2] = cell;
			Colors[coords.Item1, coords.Item2] = color;
		}
		/*
		public void SetAtIndex(int index, int cell, byte color)
		{
			if(cells == null)
				return;

			var coords = IndexToCoords(index, size.Item1, size.Item2);
			cells[coords.Item1, coords.Item2] = new() { ID = cell, Color = color };
		}
		public void DisplayText(string text, int x, int y)
		{
			for(int i = 0; i < text.Length; i++)
			{
				var symbol = text[i];
			}


		}
		public void SetSquare(int cell, int startX, int startY, int endX, int endY)
		{
			if(cells == null)
				return;

			startX = Limit(startX, 0, size.Item1 - 1);
			startY = Limit(startY, 0, size.Item2 - 1);
			endX = Limit(endX, 0, size.Item1 - 1);
			endY = Limit(endY, 0, size.Item2 - 1);

			for(int y = startY; y < endY + 1; y++)
				for(int x = startX; x < endX + 1; x++)
					cells[x, y].ID = cell;
		}
		*/

		#region Backend
		private static int Limit(int number, int rangeA, int rangeB)
		{
			if(number < rangeA)
				return rangeA;
			else if(number > rangeB)
				return rangeB;
			return number;
		}
		private (uint, uint) IndexToCoords(uint index)
		{
			var width = CellCount.Item1;
			var height = CellCount.Item2;
			index = index < 0 ? 0 : index;
			index = index > width * height - 1 ? width * height - 1 : index;

			return (index % width, index / width);
		}
		private uint GetIndex(uint x, uint y)
		{
			return y * CellCount.Item1 + x;
		}
		#endregion
	}
}
