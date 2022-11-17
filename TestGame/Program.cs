using Purity.Graphics;
using Purity.Tools;
using Purity.Utilities;

namespace TestGame
{
	public struct Pair<TKey, TValue>
	{
		public TKey Key;
		public TValue Value;
	}
	public class Test
	{
		public Pair<string, float> pair = new() { Key = "key", Value = 12345.67f };
		public Pair<string, float>[] pairs = new Pair<string, float>[]
		{
			new() { Key = "hmm", Value = 167.123f },
			new() { Key = "oh no", Value = 11f },
		};
		public string StringName { get; set; } = "hello, world!";
		public int NumberName { get; set; } = 5;
		public string[] StringArrayName { get; set; } = new string[] { "f", "qwe" };
		public bool BoolName { get; set; } = true;
		private bool[] BoolArrayName = new bool[] { false, true, true, false };
		public char CharName { get; set; } = 'f';
		private float[] NumberArrayName = new float[] { 2.3f, 0.5f, 3.8f, 1f };
		private string instanceName = "test";

		//public Pair<string, float> pair;
		//public Pair<string, float>[] pairs;
		//public string StringName { get; set; }
		//public int NumberName { get; set; }
		//public string[] StringArrayName { get; set; }
		//public bool BoolName { get; set; }
		//private bool[] BoolArrayName;
		//public char CharName { get; set; }
		//private float[] NumberArrayName;
		//private string instanceName;
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
			//storage.Load("storage-save-test.purst");
			//storage.Populate("instanceName", test);
			storage.Store("instanceName", test);
			storage.Save("storage-save-test.purst");

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