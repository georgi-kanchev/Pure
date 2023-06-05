using SFML.Audio;

namespace Pure.Audio;

public class Playlist
{
	public (float x, float y) ListenerPosition
	{
		get => (Listener.Position.X, Listener.Position.Y);
		set => Listener.Position = new(value.x, value.y, 0);
	}
	public float ListenerAngle
	{
		get => MathF.Atan2(Listener.UpVector.Y, Listener.UpVector.X) * (180f / MathF.PI);
		set
		{
			var rad = MathF.PI / 180 * value;
			Listener.UpVector = new(MathF.Cos(rad), MathF.Sin(rad), 0);
		}
	}

	public float Volume { get; set; } // to do
	public bool IsGlobal { get; set; } // to do
	public bool IsLooping { get; set; } // to do

	public Audio this[int index] => audios[index];
	public Audio this[string tag]
	{
		get
		{
			var collection = audios;
			if (tag != null)
				collection = tags[tag];

			return collection[new Random().Next(0, collection.Count)];
		}
	}

	public void Add(Audio audio, string? tag = null)
	{
		audios.Add(audio);

		if (tag == null)
			return;

		if (tags.ContainsKey(tag) == false)
			tags[tag] = new();

		tags[tag].Add(audio);
	}
	public void Shuffle(string? tag = null)
	{
		if (tag != null)
		{
			Shuffle(tags[tag]);
			return;
		}

		Shuffle(audios);
	}

	public void Play()
	{
		isPlaying = true;

		PlayCurrent();
	}
	public void Restart()
	{
		currentIndex = 0;
		time = 0;

		Play();
	}
	public void Skip()
	{
		currentIndex++;
		time = 0;

		PlayCurrent();
	}
	public void Pause()
	{
		isPlaying = false;
	}
	public void Stop()
	{
		time = 0;
		currentIndex = 0;
		isPlaying = false;
	}

	public void Update(float deltaTime)
	{
		if (audios.Count == 0 || isPlaying == false)
			return;

		time += deltaTime;

		var audio = audios[currentIndex];
		if (time >= audio.Duration)
		{
			var tag = default(string);
			foreach (var kvp in tags)
				if (kvp.Value.Contains(audio))
				{
					tag = kvp.Key;
					break;
				}

			time = 0;
			OnAudioEnd(currentIndex, tag);
			PlayCurrent();
		}

		if (currentIndex == audios.Count)
			OnListEnd();
	}

	public virtual void OnAudioEnd(int index, string? tag) { }
	public virtual void OnListEnd() { }

	#region Backend
	private bool isPlaying;
	private float time;
	private int currentIndex;

	private bool IsInvalid => audios.Count == 0 || currentIndex < 0 || currentIndex >= audios.Count;

	private readonly Dictionary<string, List<Audio>> tags = new();
	private readonly List<Audio> audios = new();

	private void PlayCurrent()
	{
		if (IsInvalid)
			return;

		audios[currentIndex].Play();
	}
	private void PauseCurrent()
	{
		if (IsInvalid)
			return;

		audios[currentIndex].Pause();
	}
	private void StopCurrent()
	{
		if (IsInvalid)
			return;

		audios[currentIndex].Stop();
	}

	private static void Shuffle<T>(IList<T> collection)
	{
		var rand = new Random();
		for (int i = collection.Count - 1; i > 0; i--)
		{
			int j = rand.Next(i + 1);
			var temp = collection[i];
			collection[i] = collection[j];
			collection[j] = temp;
		}
	}
	#endregion
}