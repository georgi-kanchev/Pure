using Audio;

using SFML.Audio;

using static System.MathF;

namespace Pure.Audio
{
	public enum Wave { Sine, Square, Triangle, Sawtooth, Noise }

	public static class Notes<T> where T : notnull
	{
		public static float Volume
		{
			get => i.Volume;
			set => i.Volume = value;
		}

		public static void Generate(T id, string notes, int tempoBPM = 120, Wave wave = Wave.Square)
		{
			var validChords = GetValidNotes(notes);
			var samples = GetSamplesFromNotes(validChords, tempoBPM, wave);

			if(i.cachedSounds.ContainsKey(id))
			{
				i.cachedSounds[id].Stop();
				i.cachedSounds[id].Dispose();
			}
			i.cachedSounds[id] = new(new SoundBuffer(samples, 1, SAMPLE_RATE));
		}
		public static void Save(T id, string path)
		{
			i.TryError(nameof(Notes<T>), id);

			var sound = i.cachedSounds[id];
			var success = sound.SoundBuffer.SaveToFile(path);

			if(success == false)
				throw new ArgumentException($"Cannot save {nameof(Notes<T>)} to the provided {nameof(path)}.");
		}

		public static void Play(T id, float volume, bool isLooping) => i.Play(id, volume, isLooping);
		public static void Play(T id) => i.Play(id);
		public static void Pause(T id) => i.Pause(id);
		public static void Stop(T id) => i.Stop(id);

		#region Backend
		private static readonly AudioInstance<T> i = new();
		private static readonly Random rand = new();

		// this avoids switch or if chain to determine the wave, results in generating the sounds a bit faster
		private static readonly Dictionary<Wave, Func<int, float, float>> waveFuncs = new()
		{
			{ Wave.Sine, (i, f) => Sin(A(f) * i) },
			{ Wave.Square, (i, f) => (Sin(A(f) * i) > 0 ? 1f : -1f) * 0.5f },
			{ Wave.Triangle, (i, f) => Asin(Sin(A(f) * i)) * 2f / PI },
			{ Wave.Sawtooth, (i, f) => 2f * f * i % (1f / f) - 1f },
			{ Wave.Noise, (i, f) => 2f * (rand.Next(1000) / 1000f) - 1f },
		};

		private const uint SAMPLE_RATE = 11025;
		private const float AMPLITUDE = 1f * short.MaxValue;
		private const char SEPARATOR = ' ', PAUSE = '.', REPEAT = '~';

		private static float GetFrequency(string chord)
		{
			if(chord.Length > 0 && chord[0] == '.')
				return 0; // pause

			var notes = new List<string> { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };

			var hasOctave = int.TryParse((chord.Length == 3 ? chord[2] : chord[1]).ToString(), out var octave);
			var keyNumber = notes.IndexOf(chord[0..^1]);

			if(hasOctave == false || keyNumber == -1)
				return float.NaN;

			keyNumber = keyNumber < 3 ?
				keyNumber + 12 + ((octave - 1) * 12) + 1 :
				keyNumber + ((octave - 1) * 12) + 1;
			var result = 440f * Pow(2f, (float)(keyNumber - 49f) / 12f);

			return result;
		}
		private static short GetWaveSample(int i, float frequency, Wave wave)
		{
			return (short)(AMPLITUDE * waveFuncs[wave].Invoke(i, frequency / SAMPLE_RATE));
		}
		private static short[] GetSamplesFromNotes(List<string> notes, int tempoBPM, Wave wave)
		{
			var duration = 60f / tempoBPM;
			var time = Ceiling(SAMPLE_RATE * duration);
			var samples = new short[(int)(time * notes.Count)];
			var sampleIndex = 0;
			for(int i = 0; i < notes.Count; i++)
			{
				var frequency = GetFrequency(notes[i]);
				for(int j = 0; j < time; j++)
				{
					samples[sampleIndex] = GetWaveSample(sampleIndex, frequency, wave);
					sampleIndex++;
				}
			}
			return samples;
		}
		private static List<string> GetValidNotes(string notes)
		{
			var chordsSplit = notes.Split(SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
			var validChords = new List<string>();

			for(int i = 0; i < chordsSplit?.Length; i++)
			{
				var note = chordsSplit[i];
				var prolongs = note.Split(REPEAT);
				var prolongCount = 1;

				if(prolongs.Length == 2)
					_ = int.TryParse(prolongs[1], out prolongCount);

				note = prolongs[0];

				var frequency = GetFrequency(note);

				if(float.IsNaN(frequency))
					continue;
				else if(frequency == 0) // pause
				{
					var pauses = note.Split(PAUSE);
					if(note != PAUSE.ToString() && pauses.Length == 2)
						_ = int.TryParse(pauses[1], out prolongCount);
					note = PAUSE.ToString();
				}

				for(int j = 0; j < prolongCount; j++)
					validChords.Add(note);
			}
			return validChords;
		}
		private static float A(float freq)
		{
			return 2f * PI * freq;
		}
		#endregion
	}
}
