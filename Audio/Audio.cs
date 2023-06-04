namespace Pure.Audio;

using SFML.Audio;

public class Audio
{
	public static float GlobalVolume
	{
		get => Listener.GlobalVolume / 100f;
		set => Listener.GlobalVolume = value * 100f;
	}
	public static (float x, float y) ListenerPosition
	{
		get => (Listener.Position.X, Listener.Position.Y);
		set => Listener.Position = new(value.x, value.y, 0);
	}
	public static float ListenerAngle
	{
		get => MathF.Atan2(Listener.UpVector.Y, Listener.UpVector.X) * (180f / MathF.PI);
		set
		{
			var rad = MathF.PI / 180 * value;
			Listener.UpVector = new(MathF.Cos(rad), MathF.Sin(rad), 0);
		}
	}

	public (float x, float y) Position
	{
		get => Get().pos;
		set { var g = Get(); Set(value, g.vol, g.pch, g.att, g.minDist, g.loop, g.gl); }
	}
	public float Volume
	{
		get => Get().vol;
		set { var g = Get(); Set(g.pos, value, g.pch, g.att, g.minDist, g.loop, g.gl); }
	}
	public float Pitch
	{
		get => Get().pch;
		set { var g = Get(); Set(g.pos, g.vol, value, g.att, g.minDist, g.loop, g.gl); }
	}
	public float Attenuation
	{
		get => Get().att;
		set { var g = Get(); Set(g.pos, g.vol, g.pch, value, g.minDist, g.loop, g.gl); }
	}
	public float MinimumDistance
	{
		get => Get().minDist;
		set { var g = Get(); Set(g.pos, g.vol, g.pch, g.att, value, g.loop, g.gl); }
	}
	public float Progress
	{
		get => Get().pr;
		set
		{
			var seconds = value * Duration;
			if (sound != null)
				sound.PlayingOffset = SFML.System.Time.FromSeconds(seconds);
			else if (music != null)
				music.PlayingOffset = SFML.System.Time.FromSeconds(seconds);
		}
	}
	public float Duration
	{
		get => Get().dur;
	}

	public bool IsLooping
	{
		get => Get().loop;
		set { var g = Get(); Set(g.pos, g.vol, g.pch, g.att, g.minDist, value, g.gl); }
	}
	public bool IsGlobal
	{
		get => Get().gl;
		set { var g = Get(); Set(g.pos, g.vol, g.pch, g.att, g.minDist, g.loop, value); }
	}

	public bool IsPlaying => Get().pl;

	public Audio(string path, bool isStreaming)
	{
		if (isStreaming)
			music = new(path);
		else
			sound = new(new SoundBuffer(path));
	}

	public void Play()
	{
		music?.Play();
		sound?.Play();
	}
	public void Pause()
	{
		music?.Pause();
		sound?.Pause();
	}
	public void Stop()
	{
		music?.Stop();
		sound?.Stop();
	}

	#region Backend
	private readonly Music? music;
	private readonly Sound? sound;

	private void Set((float x, float y) pos, float vol, float pch, float att, float minDist, bool loop, bool gl)
	{
		if (music != null)
		{
			music.Position = new(pos.x, pos.y, 0);
			music.Volume = vol * 100f;
			music.Pitch = pch;
			music.Attenuation = att * 100f;
			music.MinDistance = minDist;
			music.Loop = loop;
			music.RelativeToListener = gl;
		}
		else if (sound != null)
		{
			sound.Position = new(pos.x, pos.y, 0);
			sound.Volume = vol * 100f;
			sound.Pitch = pch;
			sound.Attenuation = att * 100f;
			sound.MinDistance = minDist;
			sound.Loop = loop;
			sound.RelativeToListener = gl;
		}
	}
	private ((float x, float y) pos, float vol, float pch, float att, float minDist, bool loop, bool gl, bool pl, float dur, float pr) Get()
	{
		((float x, float y) pos, float vol, float pch, float att, float minDist, bool loop, bool gl, bool pl, float dur, float pr) result = default;

		if (music != null)
		{
			result.pos = (music.Position.X, music.Position.Y);
			result.vol = music.Volume / 100f;
			result.pch = music.Pitch;
			result.att = music.Attenuation / 100f;
			result.minDist = music.MinDistance;
			result.loop = music.Loop;
			result.gl = music.RelativeToListener;
			result.pl = music.Status == SoundStatus.Playing;
			result.dur = music.Duration.AsSeconds();
			result.pr = music.PlayingOffset.AsSeconds() / result.dur;
		}
		else if (sound != null)
		{
			result.pos = (sound.Position.X, sound.Position.Y);
			result.vol = sound.Volume / 100f;
			result.pch = sound.Pitch;
			result.att = sound.Attenuation / 100f;
			result.minDist = sound.MinDistance;
			result.loop = sound.Loop;
			result.gl = sound.RelativeToListener;
			result.pl = sound.Status == SoundStatus.Playing;
			result.dur = sound.SoundBuffer.Duration.AsSeconds();
			result.pr = sound.PlayingOffset.AsSeconds() / result.dur;
		}
		return result;
	}
	#endregion
}
