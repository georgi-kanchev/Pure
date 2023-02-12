namespace Pure.Audio;

using SFML.Audio;

public static class Audio<T> where T : notnull
{
	public static void Load(string path, T id, bool isStreaming = false)
	{
		if (id == null)
			throw new ArgumentNullException(nameof(id));

		if (path == null)
			throw new ArgumentNullException(nameof(path));

		if (File.Exists(path) == false)
			throw new ArgumentException($"No file exists at the provided {nameof(path)}.");

		if (isStreaming)
		{
			if (cachedMusic.ContainsKey(id))
			{
				cachedMusic[id]?.Stop();
				cachedMusic[id]?.Dispose();
			}

			cachedMusic[id] = new Music(path);
			return;
		}

		if (i.cachedSounds.ContainsKey(id))
		{
			i.cachedSounds[id]?.Stop();
			i.cachedSounds[id]?.Dispose();
		}

		i.cachedSounds[id] = new(new SoundBuffer(path));
	}

	public static bool HasID(T id)
	{
		return i.cachedSounds.ContainsKey(id) || cachedMusic.ContainsKey(id);
	}

	public static void Play(T id, float volume, bool isLooping)
	{
		i.TryError(id);

		if (cachedMusic.ContainsKey(id))
		{
			var music = cachedMusic[id];
			music.Loop = isLooping;
			music.Volume = volume * 100f;
			music.Play();
			return;
		}
		else if (i.cachedSounds.ContainsKey(id))
		{
			i.Play(id, volume, isLooping);
			return;
		}
		else
			i.ThrowMissingID();
	}
	public static void Play(T id)
	{
		i.TryError(id);

		if (cachedMusic.ContainsKey(id))
		{
			cachedMusic[id].Play();
			return;
		}
		else if (i.cachedSounds.ContainsKey(id))
		{
			i.Play(id);
			return;
		}
		else
			i.ThrowMissingID();
	}
	public static void Pause(T id)
	{
		i.Pause(id);
		cachedMusic[id]?.Pause();
	}
	public static void Stop(T id)
	{
		i.Stop(id);
		cachedMusic[id]?.Stop();
	}
	public static void Stop()
	{
		i.StopAll();
		foreach (var kvp in cachedMusic)
			kvp.Value.Stop();
	}

	#region Backend
	private static readonly AudioInstance<T> i = new();
	private static readonly Dictionary<T, Music> cachedMusic = new();
	#endregion
}
