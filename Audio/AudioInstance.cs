using Pure.Audio;

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

		public void TryError(string className, T id)
		{
			if(id == null)
				throw new ArgumentNullException(nameof(id));

			if(cachedSounds.ContainsKey(id) == false)
				ThrowMissingID(className);
		}

		public void Play(T id, float volume, bool isLooping)
		{
			TryError(nameof(Notes<T>), id);

			var sound = cachedSounds[id];
			sound.Loop = isLooping;
			sound.Volume = Volume * volume * 100f;
			sound.Play();
		}
		public void Play(T id)
		{
			TryError(nameof(Notes<T>), id);
			cachedSounds[id].Play();
		}
		public void Pause(T id) => cachedSounds[id]?.Pause();
		public void Stop(T id) => cachedSounds[id]?.Stop();

		public void ThrowMissingID(string className)
		{
			throw new ArgumentException($"No {className} exists with the provided id.");
		}

		#region Backend
		private float volume = 0.5f;
		#endregion
	}
}
