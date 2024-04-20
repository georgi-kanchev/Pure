namespace Pure.Engine.Audio;

public class AudioPack
{
    public float TrackDelay { get; set; }
    public bool IsLooping { get; set; }

    public Audio this[int index]
    {
        get => audios[index];
    }

    public AudioPack(float trackDelay = 0.5f, bool isLooping = false)
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

    public Audio Pick()
    {
        return audios[new Random().Next(0, audios.Count)];
    }
    public Audio Pick(string? tag)
    {
        return tag == null ? Pick() : tags[tag][new Random().Next(0, tags[tag].Count)];
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

            if (onAudioEndIndex.TryGetValue(currentIndex, out var valueInd))
                valueInd.Invoke((currentIndex, tag));
            if (tag != null && onAudioEndTag.TryGetValue(tag, out var valueTag))
                valueTag.Invoke((currentIndex, tag));
            onAudioEndAny?.Invoke((currentIndex, tag));

            Skip();
        }

        if (currentIndex != audios.Count)
            return;

        if (IsLooping)
        {
            Restart();
            onLoop?.Invoke();
        }
        else
        {
            Stop();
            onEnd?.Invoke();
        }
    }

    public void OnEnd(Action method)
    {
        onEnd += method;
    }
    public void OnLoop(Action method)
    {
        onLoop += method;
    }
    public void OnEndAudioAny(Action<(int index, string? tag)> method)
    {
        onAudioEndAny += method;
    }
    public void OnEndAudioTag(string tag, Action<(int index, string? tag)> method)
    {
        if (onAudioEndTag.TryAdd(tag, method) == false)
            onAudioEndTag[tag] += method;
    }
    public void OnEndAudioIndex(int index, Action<(int index, string? tag)> method)
    {
        if (onAudioEndIndex.TryAdd(index, method) == false)
            onAudioEndIndex[index] += method;
    }

    #region Backend
    private Action? onLoop, onEnd;
    private Action<(int index, string? tag)>? onAudioEndAny;
    private readonly Dictionary<string, Action<(int index, string? tag)>> onAudioEndTag = new();
    private readonly Dictionary<int, Action<(int index, string? tag)>> onAudioEndIndex = new();

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