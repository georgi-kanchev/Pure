namespace Pure.Engine.Flow;

public static class Delay
{
    public static void Update(float deltaTime)
    {
        var toBeRemoved = new List<Timer>();
        foreach (var timer in timers)
        {
            timer.TryTrigger(deltaTime);

            if (timer.IsDisposed)
                toBeRemoved.Add(timer);
        }

        foreach (var timer in toBeRemoved)
            timers.Remove(timer);
    }

    /// <summary>
    /// Calls a method after a specified number of seconds.
    /// </summary>
    /// <param name="seconds">The number of seconds to wait before calling the method.</param>
    /// <param name="method">The method to call.</param>
    /// <param name="repeat">Whether to repeat the call at the specified interval.</param>
    public static void Wait(float seconds, Action method, bool repeat = false)
    {
        timers.Add(new(seconds, repeat, method));
    }
    /// <summary>
    /// Cancels a scheduled method call.
    /// </summary>
    /// <param name="method">The method to cancel.</param>
    public static void Cancel(Action method)
    {
        var timersToRemove = new List<Timer>();
        foreach (var t in timers)
            if (t.method == method)
                timersToRemove.Add(t);

        foreach (var t in timersToRemove)
            timers.Remove(t);
    }
    /// <summary>
    /// Offsets the scheduled call time of a method.
    /// </summary>
    /// <param name="seconds">The number of seconds to offset the call time by.</param>
    /// <param name="method">The method to offset the call time for.</param>
    public static void Offset(float seconds, Action method)
    {
        foreach (var t in timers)
            if (t.method == method)
                t.delay += seconds;
    }

#region Backend
    private class Timer
    {
        private float time;
        private readonly bool isLooping;

        public Action? method;
        public float delay;
        public bool IsDisposed
        {
            get => method == null;
        }

        public Timer(float seconds, bool isLooping, Action method)
        {
            delay = seconds;
            this.isLooping = isLooping;
            this.method = method;
        }

        public void TryTrigger(float delta)
        {
            time += delta;

            if (time < delay)
                return;

            Trigger(true);

            if (isLooping == false)
                Dispose();
        }
        private void Restart()
        {
            time = 0;
        }
        private void Trigger(bool reset)
        {
            method?.Invoke();

            if (reset)
                Restart();
        }

        private void Dispose()
        {
            method = null;
        }
    }

    private static readonly List<Timer> timers = new();
#endregion
}