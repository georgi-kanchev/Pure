global using VecF = (float x, float y);

namespace Pure.Engine.Audio;

using SFML.Audio;
using SFML.System;

public class Audio
{
    public static VecF ListenerPosition
    {
        get => (Listener.Position.X, Listener.Position.Y);
        set => Listener.Position = new(value.x, value.y, 0f);
    }
    public static float GlobalVolume
    {
        get => Listener.GlobalVolume / 100f;
        set => Listener.GlobalVolume = value * 100f;
    }

    public string? FilePath
    {
        get => filePath;
        set
        {
            Destroy();
            filePath = value;
        }
    }
    public VecF Position
    {
        get => Get().pos;
        set
        {
            settings.pos = value;
            Set();
        }
    }

    public float Volume
    {
        get => Get().vol;
        set
        {
            settings.vol = value;
            Set();
        }
    }
    public float Pitch
    {
        get => Get().pch;
        set
        {
            settings.pch = value;
            Set();
        }
    }
    public float Attenuation
    {
        get => Get().att;
        set
        {
            settings.att = value;
            Set();
        }
    }
    public float MinimumDistance
    {
        get => Get().minDist;
        set
        {
            settings.minDist = value;
            Set();
        }
    }
    public float Progress
    {
        get => Get().pr;
        set
        {
            var seconds = value * Duration;
            if (sound != null)
                sound.PlayingOffset = Time.FromSeconds(seconds);
            else if (music != null)
                music.PlayingOffset = Time.FromSeconds(seconds);
        }
    }
    public float Duration
    {
        get => Get().dur / Get().pch;
    }

    public bool IsLooping
    {
        get => Get().loop;
        set
        {
            settings.loop = value;
            Set();
        }
    }
    public bool IsGlobal
    {
        get => Get().gl;
        set
        {
            settings.gl = value;
            Set();
        }
    }
    public bool IsPlaying
    {
        get => Get().st == SoundStatus.Playing;
    }
    public bool IsPaused
    {
        get => Get().st == SoundStatus.Paused;
    }
    public bool IsStopped
    {
        get => Get().st == SoundStatus.Stopped;
    }
    public bool IsStreamed
    {
        get => isStreamed;
        set
        {
            Destroy();
            isStreamed = value;
        }
    }

    public Audio(string filePath, bool stream)
    {
        FilePath = filePath;
        IsStreamed = stream;
    }

    public void Play()
    {
        TryLoad();
        music?.Play();
        sound?.Play();
    }
    public void Pause()
    {
        TryLoad();
        music?.Pause();
        sound?.Pause();
    }
    public void Stop()
    {
        TryLoad();
        music?.Stop();
        sound?.Stop();
    }

#region Backend
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
    internal class DoNotSave : Attribute;

    private sealed class Settings
    {
        public SoundStatus st;
        public bool loop, gl;
        public VecF pos;
        public float vol = 0.5f, pch = 1f, att, minDist, dur, pr;
    }

    [DoNotSave]
    private Music? music;
    [DoNotSave]
    internal Sound? sound;

    private Settings settings = new();
    private string? filePath;
    private bool isStreamed;

    static Audio()
    {
        GlobalVolume = 0.5f;
    }
    internal Audio()
    {
    }

    private void Destroy()
    {
        sound?.Dispose();
        music?.Dispose();
        sound = null;
        music = null;
    }
    private void TryLoad()
    {
        if (music != null || sound != null)
            return;

        if (IsStreamed)
            music = new(FilePath);
        else
            sound = new(new SoundBuffer(FilePath));
    }

    private void Set()
    {
        if (music != null)
        {
            music.Position = new(settings.pos.x, settings.pos.y, 0);
            music.Volume = settings.vol * 100f;
            music.Pitch = settings.pch;
            music.Attenuation = settings.att * 100f;
            music.MinDistance = settings.minDist;
            music.Loop = settings.loop;
            music.RelativeToListener = settings.gl;
        }
        else if (sound != null)
        {
            sound.Position = new(settings.pos.x, settings.pos.y, 0);
            sound.Volume = settings.vol * 100f;
            sound.Pitch = settings.pch;
            sound.Attenuation = settings.att * 100f;
            sound.MinDistance = settings.minDist;
            sound.Loop = settings.loop;
            sound.RelativeToListener = settings.gl;
        }
    }
    private Settings Get()
    {
        var result = new Settings();

        if (music != null)
        {
            result.pos = (music.Position.X, music.Position.Y);
            result.vol = music.Volume / 100f;
            result.pch = music.Pitch;
            result.att = music.Attenuation / 100f;
            result.minDist = music.MinDistance;
            result.loop = music.Loop;
            result.gl = music.RelativeToListener;
            result.st = music.Status;
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
            result.st = sound.Status;
            result.dur = sound.SoundBuffer.Duration.AsSeconds();
            result.pr = sound.PlayingOffset.AsSeconds() / result.dur;
        }

        settings = result;
        return result;
    }
#endregion
}