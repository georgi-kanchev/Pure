namespace Pure.Audio;

using SFML.Audio;

using static System.MathF;

/// <summary>
/// Represents the type of waveform used in generating audio from notes.
/// </summary>
public enum Wave { Sine, Square, Triangle, Sawtooth, Noise }

/// <summary>
/// Provides functionality for generating, playing and saving/loading audio from a string of notes.
/// </summary>
/// <typeparam name="T">The type of identifier used to reference cached sounds.</typeparam>
public static class Notes<T> where T : notnull
{
	/// <summary>
	/// The volume of generated sounds, between 0 and 1.
	/// </summary>
	public static float Volume
	{
		get => i.Volume;
		set => i.Volume = value;
	}

	/// <param name="id">
	/// The identifier of the sound to check for.</param>
	/// <returns>True if a sound with the given identifier exists in the cache, otherwise false.</returns>
	public static bool Hasidentifier(T id)
	{
		return i.cachedSounds.ContainsKey(id);
	}

	/// <summary>
	/// Generates an audio from a string of <paramref name="notes"/> and caches it with 
	/// the given identifier.
	/// </summary>
	/// <param name="id">The identifier to use for the cached sound.</param>
	/// <param name="notes">The string of notes to generate audio from.</param>
	/// <param name="tempoBPM">The tempo of the generated audio in beats per minute.</param>
	/// <param name="wave">The wave shape of the generated audio.</param>
	public static void Generate(T id, string notes, int tempoBPM = 120, Wave wave = Wave.Square)
	{
		if (id == null)
			throw new ArgumentNullException(nameof(id));

		var validChords = GetValidNotes(notes);
		if (validChords.Count == 0)
			return;

		var samples = GetSamplesFromNotes(validChords, tempoBPM, wave);

		if (i.cachedSounds.ContainsKey(id))
		{
			i.cachedSounds[id].Stop();
			i.cachedSounds[id].Dispose();
		}
		i.cachedSounds[id] = new(new SoundBuffer(samples, 1, SAMPLE_RATE));
	}
	/// <summary>
	/// Saves the audio associated with the given identifier as an audio file to the 
	/// specified <paramref name="path"/>.
	/// </summary>
	/// <param name="id">The identifier of the audio to save.</param>
	/// <param name="path">The file path to save the audio to.</param>
	/// <exception cref="ArgumentException">Thrown if the audio cannot 
	/// be saved to the specified path.</exception>
	public static void Save(T id, string path)
	{
		i.TryError(id);

		var sound = i.cachedSounds[id];
		var success = sound.SoundBuffer.SaveToFile(path);

		if (success == false)
			throw new ArgumentException($"Cannot save {nameof(Notes<T>)} to the provided {nameof(path)}.");
	}
	/// <summary>
	/// Generates an audio from the contents of the specified file path (full of notes) and 
	/// caches it with the given identifier.
	/// </summary>
	/// <param name="notesPath">The file path containing the string of notes to generate audio from.</param>
	/// <param name="id">The identifier to use for the cached sound.</param>
	/// <param name="tempoBPM">The tempo of the generated audio in beats per minute.</param>
	/// <param name="wave">The wave shape of the generated audio.</param>
	public static void Load(string notesPath, T id, int tempoBPM = 120, Wave wave = Wave.Square)
	{
		if (id == null)
			throw new ArgumentNullException(nameof(id));

		if (notesPath == null)
			throw new ArgumentNullException(nameof(notesPath));

		if (File.Exists(notesPath) == false)
			throw new ArgumentException($"No file exists at the provided {nameof(notesPath)}.");

		Generate(id, File.ReadAllText(notesPath), tempoBPM, wave);
	}

	/// <summary>
	/// Plays the audio associated with the given identifier at the specified 
	/// <paramref name="volume"/> and loop settings. Those settings remain.
	/// </summary>
	/// <param name="id">The identifier of the audio to play.</param>
	/// <param name="volume">The volume to play the audio at, between 0 and 1.</param>
	/// <param name="isLooping">True if the audio should loop, otherwise false.</param>
	public static void Play(T id, float volume, bool isLooping) => i.Play(id, volume, isLooping);
	/// <summary>
	/// Plays the audio associated with the given identifier without changing its settings.
	/// </summary>
	/// <param name="id">The identifier of the audio to play.</param>
	public static void Play(T id) => i.Play(id);
	/// <summary>
	/// Pauses playback of the audio associated with the given identifier.
	/// </summary>
	/// <param name="id">The identifier of the audio to pause.</param>
	public static void Pause(T id) => i.Pause(id);
	/// <summary>
	/// Stops playback of the audio associated with the given identifier.
	/// </summary>
	/// <param name="id">The identifier of the audio to stop.</param>
	public static void Stop(T id) => i.Stop(id);
	/// <summary>
	/// Stops playback of all generated sounds.
	/// </summary>
	public static void Stop() => i.StopAll();

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
		chord = chord.Trim();

		if ((chord.Length > 0 && chord[0] == '.') || // pause
			(chord.Length != 2 && chord.Length != 3)) // invalid
			return 0; // pause

		var notes = new List<string> { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };

		chord = chord == "E#" ? "F" : chord;
		chord = chord == "B#" ? "C" : chord;

		var hasOctave = int.TryParse((chord.Length == 3 ? chord[2] : chord[1]).ToString(), out var octave);
		var keyNumber = notes.IndexOf(chord[0..^1]);

		if (hasOctave == false || keyNumber == -1)
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
		for (int i = 0; i < notes.Count; i++)
		{
			var frequency = GetFrequency(notes[i]);
			for (int j = 0; j < time; j++)
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

		for (int i = 0; i < chordsSplit?.Length; i++)
		{
			var note = chordsSplit[i];
			var prolongs = note.Split(REPEAT);
			var prolongCount = 1;

			if (prolongs.Length == 2)
				_ = int.TryParse(prolongs[1], out prolongCount);

			note = prolongs[0];

			var frequency = GetFrequency(note);

			if (float.IsNaN(frequency))
				continue;
			else if (frequency == 0) // pause
			{
				var pauses = note.Split(PAUSE);
				if (note != PAUSE.ToString() && pauses.Length == 2)
					_ = int.TryParse(pauses[1], out prolongCount);
				note = PAUSE.ToString();
			}

			for (int j = 0; j < prolongCount; j++)
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
