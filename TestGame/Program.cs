using Purity.Tools;
using Purity.Utilities;

using SFML.Window;

namespace TestGame
{
	public class Program
	{
		static void Main()
		{
			var window = new Purity.Graphics.Window();
			var bg = new Layer((48, 27));
			var layer = new Layer((48, 27));
			var time = new Time();

			while(window.IsOpen)
			{
				time.Update();

				bg.Fill(26 * 2 + 7, Color.Brown);
				layer.Fill(0, 0);
				/*
				for(uint i = 0; i < 26 * 19; i++)
					layer.SetCell(i, i, (byte)i);
				var hoveredCell = window.GetHoveredCell(layer.Cells);
				layer.SetTextLine((0, 0), hoveredCell.ToString(), Color.White);
				*/
				window.DrawOn();
				var h = window.GetHoveredIndicies(layer.CellCount);
				var hoveredIndices = ((uint)h.Item1, (uint)h.Item2);

				var align = 0;
				if(Keyboard.IsKeyPressed(Keyboard.Key.Num2)) align = 1;
				if(Keyboard.IsKeyPressed(Keyboard.Key.Num3)) align = 2;
				if(Keyboard.IsKeyPressed(Keyboard.Key.Num4)) align = 3;
				if(Keyboard.IsKeyPressed(Keyboard.Key.Num5)) align = 4;
				if(Keyboard.IsKeyPressed(Keyboard.Key.Num6)) align = 5;
				if(Keyboard.IsKeyPressed(Keyboard.Key.Num7)) align = 6;
				if(Keyboard.IsKeyPressed(Keyboard.Key.Num8)) align = 7;
				if(Keyboard.IsKeyPressed(Keyboard.Key.Num9)) align = 8;

				bg.SetSquare((3, 3), (h.Item1 - 3, h.Item2 - 3), 10, Color.Gray);
				layer.SetTextBox((3, 3), (h.Item1 - 3, h.Item2 - 3), Color.White, true, (Layer.Alignment)align,
					$"It is a long established fact",
					"that a reader will be distracted",
					"by the readable content of a page",
					"when looking at its layout. The",
					"point of using Lorem Ipsum is",
					"that it has a more-or-less normal",
					"distribution of letters, as opposed",
					"to using 'Content here, content",
					"here', making it look like readable",
					"English. Many desktop publishing",
					"packages and web page editors now",
					"use Lorem Ipsum as their default",
					"model text, and a search for 'lorem",
					"ipsum' will uncover many web sites",
					"still in their infancy.");

				layer.SetCell(hoveredIndices, 470, Color.Red);

				window.DrawLayer(bg.Cells, bg.Colors, (8, 8));
				window.DrawLayer(layer.Cells, layer.Colors, (8, 8));
				window.DrawOff();
			}
		}
	}
}

//"It is a long established fact",
//"that a reader will be distracted",
//"by the readable content of a page",
//"when looking at its layout. The",
//"point of using Lorem Ipsum is",
//"that it has a more-or-less normal",
//"distribution of letters, as opposed",
//"to using 'Content here, content",
//"here', making it look like readable",
//"English. Many desktop publishing",
//"packages and web page editors now",
//"use Lorem Ipsum as their default",
//"model text, and a search for 'lorem",
//"ipsum' will uncover many web sites",
//"still in their infancy. Various",
//"versions have evolved over the",
//"years, sometimes by accident,",
//"sometimes on purpose (injected",
//"humour and the like).");