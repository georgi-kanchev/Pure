using Purity.Graphics;
using Purity.Tools;
using Purity.Utilities;

namespace TestGame
{
	public struct Col
	{
		public float Red, Green, Blue, Alpha;
		public char Test;
	}
	public class Test
	{
		private string instanceName;
		public int NumberName { get; set; }
		public string StringName { get; set; }
		public char CharName { get; set; }
		public bool BoolName { get; set; }
		private bool[] BoolArrayName;
		private float[] NumberArrayName;
		public string[] StringArrayName { get; set; }
		public Col[] StructName;
	}

	public class Program
	{
		static void Main()
		{
			var window = new Window();
			var layer = new Layer((48, 27));
			var time = new Time();

			var test = new Test();
			var storage = new Storage();
			storage.Load("storage.txt");
			storage.Populate(test, "instanceName");

			while(window.IsOpen)
			{
				time.Update();

				layer.Fill(26 * 2 + 7, Color.Azure);
				for(uint i = 0; i < 26 * 19; i++)
					layer.SetCell(i, i, (byte)i);

				var cell = window.GetHoveredCell(layer.Cells);
				layer.SetTextLine((0, 0), cell.ToString(), Color.White);

				window.DrawOn();
				window.DrawLayer(layer.Cells, layer.Colors, (8, 8));
				window.DrawOff();
			}
		}
	}
}