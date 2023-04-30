namespace Pure.Audio;

using SFML.Audio;

/// <summary>
/// Provides methods for loading, playing, pausing, and stopping audio files by a key of type
/// <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type of the audio file identifier.</typeparam>
/// <remarks>
/// This class uses the SFML.NET library for audio playback and caching.
/// </remarks>
public static class Audio<T> where T : notnull
{
	/// <summary>
	/// Loads an audio file from the specified path and assigns it 
	/// the specified identifier.
	/// </summary>
	/// <param name="path">The path to the audio file.</param>
	/// <param name="id">The identifier to assign to the audio file.</param>
	/// <param name="isStreaming">Whether to stream the audio file or load it into memory.</param>
	/// <exception cref="ArgumentNullException">Thrown if either path or 
	/// id is null.</exception>
	/// <exception cref="ArgumentException">Thrown if the file at the specified 
	/// path does not exist.</exception>
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

	/// <param name="id">
	/// The identifier to check.</param>
	/// <returns>True if the specified audio file identifier is currently loaded and 
	/// available for playback, false otherwise.</returns>
	public static bool HasID(T id)
	{
		return i.cachedSounds.ContainsKey(id) || cachedMusic.ContainsKey(id);
	}

	/// <summary>
	/// Plays the audio file with the specified identifier at the specified volume 
	/// and with looping enabled or disabled. Those settings remain.
	/// </summary>
	/// <param name="id">The identifier of the audio file to play.</param>
	/// <param name="volume">The volume at which to play the audio file (ranged 0 to 1).</param>
	/// <param name="isLooping">Whether to loop the audio file or play it once.</param>
	/// <exception cref="ArgumentException">Thrown if the specified audio file at that identifier 
	/// is not currently loaded.</exception>
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
	/// <summary>
	/// Plays the audio file with the specified identifier without changing any settings.
	/// </summary>
	/// <param name="id">The identifier of the audio file to play.</param>
	/// <exception cref="ArgumentException">Thrown if the specified audio 
	/// file identifier is not currently loaded.</exception>
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
	/// <summary>
	/// Pauses playback of the audio file with the specified identifier.
	/// </summary>
	/// <param name="id">The identifier of the audio file to pause.</param>
	public static void Pause(T id)
	{
		i.Pause(id);
		cachedMusic[id]?.Pause();
	}
	/// <summary>
	/// Stops playback of the audio file with the specified identifier.
	/// </summary>
	/// <param name="id">The identifier of the audio file to stop.</param>
	public static void Stop(T id)
	{
		i.Stop(id);
		cachedMusic[id]?.Stop();
	}
	/// <summary>
	/// Stops playback of all loaded audio files.
	/// </summary>
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
