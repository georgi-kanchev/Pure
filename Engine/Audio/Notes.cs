namespace Pure.Engine.Audio;

using SFML.Audio;
using static System.MathF;

/// <summary>
/// Represents the type of waveform used in generating audio from notes.
/// </summary>
public enum Wave { Sine, Square, Triangle, Sawtooth, Noise }

public class Notes : Audio
{
    public static (char separator, char pause, char repeat) Symbols
    {
        get;
        set;
    } = ('_', '.', '~');

    public Notes(
        string notes,
        float duration = 0.2f,
        Wave wave = Wave.Square,
        (float start, float end) fade = default)
    {
        var validChords = GetValidNotes(notes);
        if (validChords.Count == 0)
            return;

        fade.start = Math.Clamp(fade.start, 0, 1);
        fade.end = Math.Clamp(fade.end, 0, 1);

        this.fade = fade;

        var samples = GetSamplesFromNotes(validChords, duration, wave);
        var sound = new Sound(new SoundBuffer(samples, 1, SAMPLE_RATE));
        Initialize(sound);
        buffer = sound.SoundBuffer;
    }

    public void ToFile(string path)
    {
        buffer?.SaveToFile(path);
    }

#region Backend
    private readonly SoundBuffer? buffer;
    private readonly (float start, float end) fade;
    private static readonly Random rand = new();

    // this avoids switch or if chain to determine the wave, results in generating the sounds a bit faster
    private static readonly Dictionary<Wave, Func<int, float, float>> waveFuncs = new()
    {
        { Wave.Sine, (i, f) => Sin(A(f) * i) },
        { Wave.Square, (i, f) => (Sin(A(f) * i) > 0 ? 1f : -1f) * 0.5f },
        { Wave.Triangle, (i, f) => Asin(Sin(A(f) * i)) * 2f / PI },
        { Wave.Sawtooth, (i, f) => 2f * f * i % (1f / f) - 1f },
        { Wave.Noise, (_, _) => 2f * (rand.Next(1000) / 1000f) - 1f },
    };

    private const uint SAMPLE_RATE = 11025;
    private const float AMPLITUDE = 1f * short.MaxValue;

    private static float GetFrequency(string chord)
    {
        chord = chord.Trim();

        if ((chord.Length > 0 && chord[0] == Symbols.pause) || // pause
            (chord.Length != 2 && chord.Length != 3)) // invalid
            return 0; // pause

        var notes = new List<string> { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };

        chord = chord == "E#" ? "F" : chord;
        chord = chord == "B#" ? "C" : chord;

        var hasOctave =
            int.TryParse((chord.Length == 3 ? chord[2] : chord[1]).ToString(), out var octave);
        var keyNumber = notes.IndexOf(chord[0..^1]);

        if (hasOctave == false || keyNumber == -1)
            return float.NaN;

        keyNumber = keyNumber < 3 ?
            keyNumber + 12 + ((octave - 1) * 12) + 1 :
            keyNumber + ((octave - 1) * 12) + 1;
        var result = 440f * Pow(2f, (keyNumber - 49f) / 12f);

        return result;
    }
    private static short GetWaveSample(int i, float frequency, Wave wave)
    {
        return (short)(AMPLITUDE * waveFuncs[wave].Invoke(i, frequency / SAMPLE_RATE));
    }
    private short[] GetSamplesFromNotes(List<string> notes, float noteDuration, Wave wave)
    {
        var time = Ceiling(SAMPLE_RATE * noteDuration);
        var samples = new short[(int)(time * notes.Count)];
        var sampleIndex = 0;
        var noteA = fade.start / 2f;
        var noteB = 1f - fade.end / 2f;

        for (var i = 0; i < notes.Count; i++)
        {
            var frequency = GetFrequency(notes[i]);
            var previousNote = i > 0 ? notes[i - 1] : "";
            var nextNote = i < notes.Count - 1 ? notes[i + 1] : "";
            var isStarting = notes[i] != previousNote;
            var isStopping = notes[i] != nextNote;

            for (var j = 0; j < time; j++)
            {
                var noteProgress = Map(j, (0f, time), (0f, 1f));
                var fadeValue = 1f;

                fadeValue *= isStarting && noteProgress <= noteA ?
                    Map(noteProgress, (0f, noteA), (0f, 1f)) :
                    1f;
                fadeValue *= isStopping && noteProgress > noteB ?
                    Map(noteProgress, (noteB, 1f), (1f, 0f)) :
                    1f;

                samples[sampleIndex] = (short)(frequency == 0 ?
                    0 :
                    GetWaveSample(sampleIndex, frequency, wave) * fadeValue);
                sampleIndex++;
            }
        }

        return samples;
    }
    private static List<string> GetValidNotes(string notes)
    {
        var chordsSplit = notes.Split(Symbols.separator, StringSplitOptions.RemoveEmptyEntries);
        var validChords = new List<string>();

        foreach (var t in chordsSplit)
        {
            var note = t;
            var prolongs = note.Split(Symbols.repeat);
            var prolongCount = 1;

            if (prolongs.Length == 2)
                _ = int.TryParse(prolongs[1], out prolongCount);

            note = prolongs[0];

            var frequency = GetFrequency(note);

            if (float.IsNaN(frequency))
                continue;
            else if (frequency == 0) // pause
            {
                var pauses = note.Split(Symbols.pause);
                if (note != Symbols.pause.ToString() && pauses.Length == 2)
                    _ = int.TryParse(pauses[1], out prolongCount);
                note = Symbols.pause.ToString();
            }

            for (var j = 0; j < prolongCount; j++)
                validChords.Add(note);
        }

        return validChords;
    }
    private static float A(float freq)
    {
        return 2f * PI * freq;
    }
    private static float Map(float number, (float a, float b) range, (float a, float b) targetRange)
    {
        var value = (number - range.a) / (range.b - range.a) * (targetRange.b - targetRange.a) +
                    targetRange.a;
        return float.IsNaN(value) || float.IsInfinity(value) ? targetRange.a : value;
    }
#endregion
}