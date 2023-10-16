namespace Pure.Engine.Audio;

public class Playlist
{
    public float TrackDelay
    {
        get;
        set;
    } = 0.5f;
    public bool IsLooping
    {
        get;
        set;
    }

    public Audio this[int index]
    {
        get => audios[index];
    }
    public Audio this[string? tag]
    {
        get
        {
            var collection = audios;
            if (tag != null)
                collection = tags[tag];

            return collection[new Random().Next(0, collection.Count)];
        }
    }

    public Playlist(float trackDelay = 0.5f, bool isLooping = false)
    {
        TrackDelay = trackDelay;
        IsLooping = isLooping;
    }

    public void AddTrack(string? tag, params Audio[] tracks)
    {
        foreach (var track in tracks)
        {
            audios.Add(track);

            if (tag == null)
                continue;

            if (tags.ContainsKey(tag) == false)
                tags[tag] = new();

            tags[tag].Add(track);
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
        time = 0;
        currentIndex = 0;
        Play();
    }
    public void Skip()
    {
        time = 0;
        currentIndex++;
        PlayCurrent();
    }
    public void Pause()
    {
        isPlaying = false;
        PauseCurrent();
    }
    public void Stop()
    {
        time = 0;
        currentIndex = 0;
        isPlaying = false;
        StopCurrent();
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

        if (currentIndex != audios.Count)
            return;

        OnListEnd();

        if (IsLooping)
            Restart();
        else
            Stop();
    }

    protected virtual void OnAudioEnd(int index, string? tag)
    {
    }
    protected virtual void OnListEnd()
    {
    }

#region Backend
    private bool isPlaying;
    private float time;
    private int currentIndex;

    private bool IsInvalid
    {
        get => audios.Count == 0 || currentIndex < 0 || currentIndex >= audios.Count;
    }

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
        for (var i = collection.Count - 1; i > 0; i--)
        {
            var j = rand.Next(i + 1);
            (collection[j], collection[i]) = (collection[i], collection[j]);
        }
    }
#endregion
}