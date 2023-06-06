using SFML.Audio;

namespace Pure.Audio;

public class Playlist
{
	public bool IsLooping { get; set; }
	public float TrackDelay { get; set; } = 0.5f;

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

	public void AddTrack(string? tag, params Audio[] tracks)
	{
		for (int i = 0; i < tracks?.Length; i++)
		{
			audios.Add(tracks[i]);

			if (tag == null)
				return;

			if (tags.ContainsKey(tag) == false)
				tags[tag] = new();

			tags[tag].Add(tracks[i]);
		}
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
		var delay = currentIndex == audios.Count - 1 ? 0 : TrackDelay; // no delay on last track
		if (time >= audio.Duration + delay)
		{
			var tag = default(string);
			foreach (var kvp in tags)
				if (kvp.Value.Contains(audio))
				{
					tag = kvp.Key;
					break;
				}

			OnAudioEnd(currentIndex, tag);
			Skip();
		}
		if (currentIndex == audios.Count)
		{
			OnListEnd();

			if (IsLooping)
				Restart();
			else
				Stop();
		}
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