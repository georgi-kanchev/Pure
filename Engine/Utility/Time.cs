using System.Diagnostics;

namespace Pure.Engine.Utility;

/// <summary>
/// Provides various time-related functionalities, such as time conversion and clock-related calculations.
/// </summary>
public static class Time
{
    /// <summary>
    /// Defines the different types of time conversions used by <see cref="Time.ToTime"/>.
    /// </summary>
    public enum Conversion
    {
        MillisecondsToSeconds, MillisecondsToMinutes,
        SecondsToMilliseconds, SecondsToMinutes, SecondsToHours,
        MinutesToMilliseconds, MinutesToSeconds, MinutesToHours, MinutesToDays,
        HoursToSeconds, HoursToMinutes, HoursToDays, HoursToWeeks,
        DaysToMinutes, DaysToHours, DaysToWeeks,
        WeeksToHours, WeeksToDays
    }

    /// <summary>
    /// Defines the different units of time that can be used in calculations and display by
    /// <see cref="Time.ToClock"/>.
    /// </summary>
    [Flags]
    public enum Unit
    {
        Day = 1 << 0,
        Hour = 1 << 1,
        Minute = 1 << 2,
        Second = 1 << 3,
        Millisecond = 1 << 4,
        AmPm = 1 << 5,
        DisplayAmPm = 1 << 6
    }

    /// <summary>
    /// Gets the real time clock taken from <see cref="DateTime.Now"/> in seconds (ranged
    /// 0 to 86399 or in clock hours ranged 12 AM/00:00/24:00 to 11:59:59 AM/23:59:59).
    /// </summary>
    public static float Clock { get; private set; }

    /// <summary>
    /// Gets or sets the maximum delta time between updates.
    /// </summary>
    public static float DeltaMax { get; set; } = 0.1f;
    /// <summary>
    /// Gets the delta time since the last update capped at <see cref="DeltaMax"/>.
    /// </summary>
    public static float Delta { get; private set; }
    /// <summary>
    /// Gets the raw delta time since the last update.
    /// </summary>
    public static float DeltaRaw { get; private set; }
    /// <summary>
    /// Gets the number of updates per second.
    /// </summary>
    public static float UpdatesPerSecond { get; private set; }
    /// <summary>
    /// Gets the average number of updates per second.
    /// </summary>
    public static float UpdatesPerSecondAverage { get; private set; }
    /// <summary>
    /// Gets the runtime clock in seconds.
    /// </summary>
    public static float RuntimeClock { get; private set; }
    /// <summary>
    /// Gets the number of updates that have occurred.
    /// </summary>
    public static uint UpdateCount { get; private set; }

    /// <summary>
    /// Updates the clock and timers. Should be called once per frame.
    /// </summary>
    public static void Update()
    {
        var delta = (float)dt.Elapsed.TotalSeconds;
        DeltaRaw = delta;
        Delta = Math.Clamp(delta, 0, MathF.Max(0, DeltaMax));
        dt.Restart();

        Clock = (float)DateTime.Now.TimeOfDay.TotalSeconds;
        RuntimeClock += Delta;
        UpdatesPerSecond = 1f / Delta;
        UpdatesPerSecondAverage = UpdateCount / RuntimeClock;
        UpdateCount++;

        UpdateTimers();
        Collections.TryRemoveAnimations();
        Particles.Update();
    }
    /// <summary>
    /// Converts a duration in seconds to a formatted clock string.
    /// </summary>
    /// <param name="seconds">The duration in seconds.</param>
    /// <param name="separator">The separator string to use between clock elements.</param>
    /// <param name="units">The units to include in the formatted string.</param>
    /// <returns>A formatted clock string.</returns>
    public static string ToClock(this float seconds, string separator = ":", Unit units = Unit.Hour | Unit.Minute | Unit.Second)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        var result = string.Empty;
        var counter = 0;

        if (units.HasFlag(Unit.Day))
        {
            var val = (int)ts.TotalDays;
            result += $"{val:D2}";
            counter++;
        }

        if (units.HasFlag(Unit.Hour))
        {
            var sep = counter > 0 ? separator : string.Empty;
            var val = (int)ts.TotalHours;

            if (counter != 0)
                val = units.HasFlag(Unit.AmPm) ? (int)Wrap(ts.Hours, 12) : ts.Hours;

            result += $"{sep}{val:D2}";
            counter++;
        }

        if (units.HasFlag(Unit.Minute))
        {
            var sep = counter > 0 ? separator : string.Empty;
            var val = counter == 0 ? (int)ts.TotalMinutes : ts.Minutes;
            result += $"{sep}{val:D2}";
            counter++;
        }

        if (units.HasFlag(Unit.Second))
        {
            var sep = counter > 0 ? separator : string.Empty;
            var val = counter == 0 ? (int)ts.TotalSeconds : ts.Seconds;
            result += $"{sep}{val:D2}";
            counter++;
        }

        if (units.HasFlag(Unit.Millisecond))
        {
            var val = counter == 0 ? (int)ts.TotalMilliseconds : ts.Milliseconds;
            var dot = units.HasFlag(Unit.Second) ? "." : string.Empty;
            var sep = dot == string.Empty && counter > 0 ? separator : string.Empty;
            result += $"{sep}{dot}{val:D3}";
            counter++;
        }

        if (units.HasFlag(Unit.DisplayAmPm))
        {
            var sep = counter > 0 ? " " : string.Empty;
            var str = ts.Hours >= 12 ? "PM" : "AM";
            result += $"{sep}{str}";
        }

        return result;

        float Wrap(float number, float range)
        {
            return (number % range + range) % range;
        }
    }
    /// <summary>
    /// Converts a duration in time units to a different time unit.
    /// </summary>
    /// <param name="time">The duration to convert.</param>
    /// <param name="convertType">The type of conversion to perform.</param>
    /// <returns>The converted duration.</returns>
    public static float ToTime(this float time, Conversion convertType)
    {
        return convertType switch
        {
            Conversion.MillisecondsToSeconds => time / 1000,
            Conversion.MillisecondsToMinutes => time / 1000 / 60,
            Conversion.SecondsToMilliseconds => time * 1000,
            Conversion.SecondsToMinutes => time / 60,
            Conversion.SecondsToHours => time / 3600,
            Conversion.MinutesToMilliseconds => time * 60000,
            Conversion.MinutesToSeconds => time * 60,
            Conversion.MinutesToHours => time / 60,
            Conversion.MinutesToDays => time / 1440,
            Conversion.HoursToSeconds => time * 3600,
            Conversion.HoursToMinutes => time * 60,
            Conversion.HoursToDays => time / 24,
            Conversion.HoursToWeeks => time / 168,
            Conversion.DaysToMinutes => time * 1440,
            Conversion.DaysToHours => time * 24,
            Conversion.DaysToWeeks => time / 7,
            Conversion.WeeksToHours => time * 168,
            Conversion.WeeksToDays => time * 7,
            _ => 0
        };
    }

    public static void CallAfter(float seconds, Action method, bool repeat = false)
    {
        timers.Add(new(method, null, seconds, repeat));
    }
    public static void CallFor(float seconds, Action<float> method, bool repeat = false)
    {
        timers.Add(new(null, method, seconds, repeat));
    }
    /// <summary>
    /// Cancels a scheduled method call.
    /// </summary>
    /// <param name="method">The method to cancel.</param>
    public static void CancelCall(Action method)
    {
        foreach (var t in timers)
            if (t.method == method)
                t.method = null;
    }
    public static void CancelCall(Action<float> method)
    {
        foreach (var t in timers)
            if (t.methodF == method)
                t.methodF = null;
    }
    /// <summary>
    /// Offsets the scheduled call time of a method.
    /// </summary>
    /// <param name="method">The method to offset the call time for.</param>
    /// <param name="seconds">The number of seconds to offset the call time by.</param>
    public static void DelayCall(float seconds, Action method)
    {
        foreach (var t in timers)
            if (t.method == method)
                t.startTime += seconds;
    }
    public static void ExtendCall(float seconds, Action<float> method)
    {
        foreach (var t in timers)
            if (t.methodF == method)
                t.startTime += seconds;
    }

#region Backend
    private static readonly Stopwatch dt = new();

    private sealed class Timer(Action? method, Action<float>? methodF, float delay, bool loop)
    {
        public Action? method = method;
        public Action<float>? methodF = methodF;
        public float startTime = RuntimeClock;

        public void TryTrigger()
        {
            var progress = delay <= 0.001f ? 1f : RuntimeClock.Map((startTime, startTime + delay), (0, 1));
            if (RuntimeClock < startTime + delay)
            {
                methodF?.Invoke(Math.Clamp(progress, 0f, 1f));
                return;
            }

            methodF?.Invoke(Math.Clamp(progress, 0f, 1f));
            method?.Invoke();
            startTime = RuntimeClock;

            if (loop)
                return;

            methodF = null;
            method = null;
        }
    }

    private static readonly List<Timer> timers = [];

    private static void UpdateTimers()
    {
        var toBeRemoved = new List<Timer>();
        foreach (var timer in timers)
        {
            timer.TryTrigger();

            if (timer.method == null && timer.methodF == null)
                toBeRemoved.Add(timer);
        }

        foreach (var timer in toBeRemoved)
            timers.Remove(timer);
    }
#endregion
}