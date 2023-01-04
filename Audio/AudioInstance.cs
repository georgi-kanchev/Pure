using SFML.Audio;

namespace Audio
{
	internal class AudioInstance<T> where T : notnull
	{
		public readonly Dictionary<T, Sound> cachedSounds = new();

		public float Volume
		{
			get => volume;
			set => volume = Math.Clamp(volume, 0f, 1f);
		}

		public void TryError(T id)
		{
			if(id == null)
				throw new ArgumentNullException(nameof(id));

			if(cachedSounds.ContainsKey(id) == false)
				ThrowMissingID();
		}

		public void Play(T id, float volume, bool isLooping)
		{
			TryError(id);

			var sound = cachedSounds[id];
			sound.Loop = isLooping;
			sound.Volume = Volume * volume * 100f;
			sound.Play();
		}
		public void Play(T id)
		{
			TryError(id);
			cachedSounds[id].Play();
		}
		public void Pause(T id)
		{
			TryError(id);
			cachedSounds[id]?.Pause();
		}
		public void Stop(T id)
		{
			TryError(id);
			cachedSounds[id]?.Stop();
		}
		public void StopAll()
		{
			foreach(var kvp in cachedSounds)
				kvp.Value.Stop();
		}

		public void ThrowMissingID()
		{
			throw new ArgumentException($"The provided id does not exist.");
		}

		#region Backend
		private float volume = 0.5f;
		#endregion
	}
}
