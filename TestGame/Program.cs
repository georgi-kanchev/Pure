using Pure.Audio;
using Pure.Tilemap;
using Pure.Utilities;
using Pure.Window;

namespace TestGame
{
	public class Program
	{
		// https://chillmindscapes.itch.io/
		// https://gigi.nullneuron.net/gigilabs/a-pathfinding-example-in-c/
		// inputline double click selection and ctrl+z/y
		// tilemap editor ctrl+z/y and collisions
		//https://babylonjs.medium.com/retro-crt-shader-a-post-processing-effect-study-1cb3f783afbcy

		static void Main()
		{
			var bg = new Tilemap("bg.map");
			var layer = new Tilemap("overworld.map");

			var track1 = "";
			var track2 = "";
			for(int i = 0; i < 5; i++)
			{
				track1 += T(2, 3) + T(3, 3);
				track2 += T(2, 3) + T(2, 3);
			}

			//track1 = "G3. E2 F2 B3. D3. E3. G3. B3 C2. G3. E3. A3. F3. E4. E3 A3. F3. E3. F4. B3. A2 A3. B3~4 C3 G3 C3. B2. B3 A3. B2. E3. C3. B2. D3~2 A3. G2. A3. B3. G2. E2. A4. F3. B3. F3. E3. F3. D4. B3 G3. B3. B3. E3. F3. C2. D3. D3. G3. B3. G2. B2. D2. B3 D2. G3 C3. B3 A2. E3. C2. E2 B3. B4. E3. G3 D3 F3. E3. F3. A4 D3. F2. A3 F3. A3 A2 G3 D3. E2. D2. C2. F2 B2. F2. D3. F3. B2 G2 C3. C3 F3. D4. D4. G4. D3. D4. B3. A4. D3. D4. B3. B2. C3 A2. F3 C3. E2. G2 G3. C3. E2. C2. E2. B2. B2. E2. A2 E2. E3. D3. B3. C4. G3. G3. D3. C3. A4. C4 G4. G3. E4. E2. F2. B3. G3 F3. B3. B2. D2 F3. E3";
			//track1 = "F2. E3. F2. A2. D3 D2 C2. B3. G2 F2. D3. A4. F3. E3. B3. D4. B3 F3. D3. C4. A3 A2. B3. G2. B3. G2 A2. E2. F2. F3. C3. F2. B3 G3 F3. G3 E2. C3. C3 C3. D4. A4. C4. A3. A4 G4 C3 D4. E3. E4. G2. D3. E2. C3. B3. F2 F2. D3. F3. G3. A3. A2. E3 C3. F3. G3 A3. B2. E2. A2. G3~3 B4 B3 A3. F4. F3. A3. F4 C3. G3. A2 C2. D3. G3. B2. F3. B3 F3. D2. D2. A3 A2 E2. A3. E3. E2. E3. C2. E2. E2. F4. B3. C3. C4. G3. B3. D3. G3. F4 C3 B2. C2. F2. B2. B2 D3. C3. B3. A3 D2 F2. A3. B2. C3. G3 C3 F2. E3. C2. G3 F3. B4. D3. C3. B3 B3. A3 D3. A4. B4. A2. A2. G3~4 A3. D2. E3. E3. G2 A3. B3 ";

			Notes<int>.Generate(0, track1, 60 * 6, Wave.Triangle);
			//Notes<int>.Generate(1, track2, 60 * 6, Wave.Sawtooth);
			Notes<int>.Play(0, 0f, true);
			//Notes<int>.Play(1, 0.1f, true);

			while(Window.IsExisting)
			{
				Window.Activate(true);

				Window.DrawTilemap(bg, bg, (12, 12), (1, 1), "urizen.png");
				Window.DrawTilemap(layer, layer, (12, 12), (1, 1), "urizen.png");

				Window.Activate(false);
			}
		}
		static string T(int octaveLower, int octaveHigher)
		{
			var track = "";
			var notes = new string[] { "A" };
			for(int i = 0; i < 12; i++)
			{
				track += notes.ChooseOne();
				track += octaveLower.Random(octaveHigher).ToString();

				if(80.HasChance())
					track += ".";
				else if(10.HasChance())
					track += "~" + 2.Random(4);
				track += " ";
			}
			return track;
		}
	}
}