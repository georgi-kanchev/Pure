﻿namespace Pure.Engine.Utilities;

using System.Diagnostics;

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
#pragma warning disable CS1591
        MillisecondsToSeconds,
        MillisecondsToMinutes,
        SecondsToMilliseconds,
        SecondsToMinutes,
        SecondsToHours,
        MinutesToMilliseconds,
        MinutesToSeconds,
        MinutesToHours,
        MinutesToDays,
        HoursToSeconds,
        HoursToMinutes,
        HoursToDays,
        HoursToWeeks,
        DaysToMinutes,
        DaysToHours,
        DaysToWeeks,
        WeeksToHours,
        WeeksToDays
#pragma warning restore CS1591
    }

    /// <summary>
    /// Defines the different units of time that can be used in calculations and display by
    /// <see cref="Time.ToClock"/>.
    /// </summary>
    [Flags]
    public enum Unit
    {
#pragma warning disable CS1591
        Day = 1,
        Hour = 2,
        Minute = 4,
        Second = 8,
        Millisecond = 16,
        AmPm = 32,
        DisplayAmPm = 64,
#pragma warning restore CS1591
    }

    /// <summary>
    /// Gets the real time clock taken from <see cref="DateTime.Now"/> in seconds ranged
    /// 0 to 86399)<br></br>or in clock hours ranged 12 AM/00:00/24:00 to 11:59:59 AM/23:59:59.
    /// </summary>
    public static float Clock
    {
        get;
        private set;
    }

    /// <summary>
    /// Gets or sets the maximum delta time between updates.
    /// </summary>
    public static float DeltaMax
    {
        get;
        set;
    } = 0.1f;
    /// <summary>
    /// Gets the delta time since the last update capped at <see cref="DeltaMax"/>.
    /// </summary>
    public static float Delta
    {
        get;
        private set;
    }
    /// <summary>
    /// Gets the raw delta time since the last update.
    /// </summary>
    public static float DeltaRaw
    {
        get;
        private set;
    }
    /// <summary>
    /// Gets the number of updates per second.
    /// </summary>
    public static float UpdatesPerSecond
    {
        get;
        private set;
    }
    /// <summary>
    /// Gets the average number of updates per second.
    /// </summary>
    public static float UpdatesPerSecondAverage
    {
        get;
        private set;
    }
    /// <summary>
    /// Gets the runtime clock in seconds.
    /// </summary>
    public static float RuntimeClock
    {
        get;
        private set;
    }
    /// <summary>
    /// Gets the number of updates that have occurred.
    /// </summary>
    public static uint UpdateCount
    {
        get;
        private set;
    }

    /// <summary>
    /// Updates the clock and timers. Should be called once per frame.
    /// </summary>
    public static void Update()
    {
        var delta = (float)dt.Elapsed.TotalSeconds;
        DeltaRaw = delta;
        Delta = Math.Clamp(delta, 0, MathF.Max(0, DeltaMax));
        dt.Restart();

        Clock = Now;
        RuntimeClock += Delta;
        UpdatesPerSecond = 1f / Delta;
        UpdatesPerSecondAverage = UpdateCount / RuntimeClock;
        UpdateCount++;

        var toBeRemoved = new List<Timer>();
        foreach (var timer in timers)
        {
            timer.TryTrigger(delta);

            if (timer.IsDisposed)
                toBeRemoved.Add(timer);
        }

        foreach (var timer in toBeRemoved)
            timers.Remove(timer);
    }
    /// <summary>
    /// Converts a duration in seconds to a formatted clock string.
    /// </summary>
    /// <param name="seconds">The duration in seconds.</param>
    /// <param name="separator">The separator string to use between clock elements.</param>
    /// <param name="units">The units to include in the formatted string.</param>
    /// <returns>A formatted clock string.</returns>
    public static string ToClock(
        this float seconds,
        string separator = ":",
        Unit units = Unit.Hour | Unit.Minute | Unit.Second)
    {
        var ts = TimeSpan.FromSeconds(seconds);
        var result = "";
        var counter = 0;

        if (units.HasFlag(Unit.Day))
        {
            var val = (int)ts.TotalDays;
            result += $"{val:D2}";
            counter++;
        }

        if (units.HasFlag(Unit.Hour))
        {
            var sep = counter > 0 ? separator : "";
            var val = counter == 0 ?
                (int)ts.TotalHours :
                (units.HasFlag(Unit.AmPm) ? (int)Wrap(ts.Hours, 12) : ts.Hours);
            //val = val == 0 ? 12 : val;
            result += $"{sep}{val:D2}";
            counter++;
        }

        if (units.HasFlag(Unit.Minute))
        {
            var sep = counter > 0 ? separator : "";
            var val = counter == 0 ? (int)ts.TotalMinutes : ts.Minutes;
            result += $"{sep}{val:D2}";
            counter++;
        }

        if (units.HasFlag(Unit.Second))
        {
            var sep = counter > 0 ? separator : "";
            var val = counter == 0 ? (int)ts.TotalSeconds : ts.Seconds;
            result += $"{sep}{val:D2}";
            counter++;
        }

        if (units.HasFlag(Unit.Millisecond))
        {
            var val = counter == 0 ? (int)ts.TotalMilliseconds : ts.Milliseconds;
            var dot = units.HasFlag(Unit.Second) ? "." : "";
            var sep = dot == "" && counter > 0 ? separator : "";
            result += $"{sep}{dot}{val:D3}";
            counter++;
        }

        if (units.HasFlag(Unit.DisplayAmPm))
        {
            var sep = counter > 0 ? " " : "";
            var str = ts.Hours >= 12 ? "PM" : "AM";
            result += $"{sep}{str}";
        }

        return result;

        float Wrap(float number, float range)
        {
            return ((number % range) + range) % range;
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
            _ => 0,
        };
    }

    /// <summary>
    /// Calls a method after a specified number of seconds.
    /// </summary>
    /// <param name="seconds">The number of seconds to wait before calling the method.</param>
    /// <param name="method">The method to call.</param>
    /// <param name="isRepeating">Whether to repeat the call at the specified interval.</param>
    public static void CallAfter(float seconds, Action method, bool isRepeating = false)
    {
        timers.Add(new Timer(seconds, isRepeating, method));
    }
    /// <summary>
    /// Cancels a scheduled method call.
    /// </summary>
    /// <param name="method">The method to cancel.</param>
    public static void CancelCall(Action method)
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
    public static void OffsetCall(float seconds, Action method)
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
    private static readonly Stopwatch dt = new();

    private static float Now
    {
        get => (float)DateTime.Now.TimeOfDay.TotalSeconds;
    }
#endregion
}