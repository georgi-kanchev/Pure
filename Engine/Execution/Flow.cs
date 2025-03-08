using System.Collections;
using System.Diagnostics;

namespace Pure.Engine.Execution;

public static class Flow
{
    public static void Update(float deltaTime)
    {
        dt = deltaTime;
        runtimeClock += deltaTime;

        for (var i = 0; i < timers.Count; i++)
        {
            timers[i].TryTrigger();

            if (timers[i].method != null || timers[i].methodF != null)
                continue;

            timers.Remove(timers[i]);
            i--;
        }

        for (var i = 0; i < flows.Count; i++)
        {
            var flow = flows[i];

            if (flow.Current == null)
            {
                flow.MoveNext();
                continue; // we are at the very start of the method or told to keep going
            }

            if (flow.Current is Rule rule)
            {
                if (rule.destroy)
                {
                    Remove(); // we are ending the method abruptly
                    continue;
                }

                if (rule.MoveNext() == false && flow.MoveNext() == false)
                    Remove(); // no more interruptions & the method finished
            }

            void Remove()
            {
                Signal(flow.GetType().FullName); // notify WaitForFlow waiters

                flows.Remove(flows[i]);
                i--;
            }
        }

        foreach (var (start, curr) in trueEvery)
        {
            // clamp should be before to let it go 1 time below 0 for detection
            trueEvery[start] = trueEvery[start] < 0f ? start : trueEvery[start];
            trueEvery[start] -= dt;
        }
    }

    public static void Start(IEnumerator flow)
    {
        flows.Add(flow);
    }
    public static bool IsWaiting(string? key)
    {
        return key != null && resumptions.ContainsKey(key);
    }

    public static IEnumerator WaitForFlow(IEnumerator flow)
    {
        return WaitForSignal(flow.GetType().FullName);
    }
    public static IEnumerator WaitForDelay(float seconds, string? key = null)
    {
        var time = 0f;

        if (key != null && resumptions.TryAdd(key, () => time = seconds) == false)
            resumptions[key] += () => time = seconds;

        return new Rule(() =>
        {
            time += dt;
            return time <= seconds;
        });
    }
    public static IEnumerator WaitForSignal(string? key)
    {
        var keepWaiting = true;
        if (key != null && resumptions.TryAdd(key, () => keepWaiting = false) == false)
            resumptions[key] += () => keepWaiting = false;
        return new Rule(() => keepWaiting);
    }

    public static void Signal(string? key)
    {
        if (key == null || resumptions.TryGetValue(key, out var cb) == false)
            return;

        cb?.Invoke();
        resumptions.Remove(key);
    }
    public static IEnumerator End()
    {
        return new Rule(() => false) { destroy = true };
    }
    public static void End(IEnumerator flow)
    {
        for (var i = 0; i < flows.Count; i++)
            if (flows[i].GetType().FullName == flow.GetType().FullName)
            {
                flows.Remove(flows[i]);
                i--;
            }
    }

    public static void CallAfter(float seconds, Action? method)
    {
        if (method == null)
            return;

        timers.Add(new(method, null, seconds, false));
    }
    public static void CallEvery(float seconds, Action? method)
    {
        if (method == null)
            return;

        timers.Add(new(method, null, seconds, true));
    }
    public static void CallFor(float seconds, Action<float>? method)
    {
        if (method == null)
            return;

        timers.Add(new(null, method, seconds, false));
    }
    /// <summary>
    /// Cancels a scheduled method call.
    /// </summary>
    /// <param name="method">The method to cancel.</param>
    public static void CancelCall(Action? method)
    {
        if (method == null)
            return;

        foreach (var t in timers)
            if (t.method == method)
                t.method = null;
    }
    public static void CancelCall(Action<float>? method)
    {
        if (method == null)
            return;

        foreach (var t in timers)
            if (t.methodF == method)
                t.methodF = null;
    }
    /// <summary>
    /// Offsets the scheduled call time of a method.
    /// </summary>
    /// <param name="method">The method to offset the call time for.</param>
    /// <param name="seconds">The number of seconds to offset the call time by.</param>
    public static void DelayCall(float seconds, Action? method)
    {
        if (method == null)
            return;

        foreach (var t in timers)
            if (t.method == method)
                t.startTime += seconds;
    }
    public static void ExtendCall(float seconds, Action<float>? method)
    {
        if (method == null)
            return;

        foreach (var t in timers)
            if (t.methodF == method)
                t.startTime += seconds;
    }

    /// <summary>
    /// Returns true only the first time a condition is true.
    /// This is reset whenever the condition becomes false.
    /// A uniqueID needs to be provided that describes each type of
    /// condition in order to separate/identify them.
    /// Useful for triggering continuous checks only once, rather than every update.
    /// </summary>
    /// <param name="condition">The bool value to check for.</param>
    /// <param name="uniqueId">The uniqueID to associate the check with.</param>
    /// <returns>True if the condition is true and the uniqueID has not been checked before,
    /// or if the condition is true and the number of entries is less than the maximum allowed. False otherwise.</returns>
    public static bool Once(this bool condition, string uniqueId)
    {
        if (gates.ContainsKey(uniqueId) == false && condition == false)
            return false;
        if (gates.ContainsKey(uniqueId) == false && condition)
        {
            gates[uniqueId] = true;
            return true;
        }

        if (gates[uniqueId] && condition)
            return false;
        if (gates[uniqueId] == false && condition)
        {
            gates[uniqueId] = true;
            return true;
        }

        gates[uniqueId] = false;

        return false;
    }
    public static void Once(this bool condition, Action? method)
    {
        if (method == null)
            return;

        if (condition.Once(method.GetHashCode().ToString()))
            method.Invoke();
    }
    /// <summary>
    /// Returns true the first time a condition is true.
    /// Also returns true after a delay in seconds every frequency seconds.
    /// Returns false otherwise.
    /// A uniqueID needs to be provided that describes each type of condition in order to separate/identify them.
    /// Useful for turning a continuous input condition into the familiar "press and hold" key trigger.
    /// </summary>
    /// <param name="condition">The bool value to check for.</param>
    /// <param name="uniqueId">The unique ID to associate the check with.</param>
    /// <param name="delay">The delay in seconds before the condition is considered held.</param>
    /// <param name="frequency">The frequency in seconds at which the result is true while held.</param>
    public static bool PressAndHold(this bool condition, string uniqueId, float delay = 0.5f, float frequency = 0.06f)
    {
        if (condition.Once(uniqueId))
        {
            holdDelay.Restart();
            return true;
        }

        if (condition == false ||
            holdDelay.Elapsed.TotalSeconds <= delay ||
            holdFrequency.Elapsed.TotalSeconds <= frequency)
            return false;
        holdFrequency.Restart();
        return true;
    }
    public static void PressAndHold(this bool condition, Action? method, float delay = 0.5f, float frequency = 0.06f)
    {
        if (method == null)
            return;

        if (PressAndHold(condition, method.GetHashCode().ToString(), delay, frequency))
            method.Invoke();
    }

    public static bool TrueEvery(float seconds)
    {
        trueEvery.TryAdd(seconds, seconds);
        return trueEvery[seconds] < 0f;
    }

#region Backend
    private class Rule(Func<bool> keepWaitingCondition) : IEnumerator
    {
        public bool destroy;

        public object? Current
        {
            get => null;
        }
        public void Reset()
        {
        }
        public bool MoveNext()
        {
            return keepWaitingCondition.Invoke();
        }
    }

    private sealed class Timer(Action? method, Action<float>? methodF, float delay, bool loop)
    {
        public Action? method = method;
        public Action<float>? methodF = methodF;
        public float startTime = runtimeClock;

        public void TryTrigger()
        {
            var progress = delay <= 0.001f ? 1f : runtimeClock.Map((startTime, startTime + delay), (0, 1));
            if (runtimeClock < startTime + delay)
            {
                methodF?.Invoke(Math.Clamp(progress, 0f, 1f));
                return;
            }

            methodF?.Invoke(Math.Clamp(progress, 0f, 1f));
            method?.Invoke();
            startTime = runtimeClock;

            if (loop)
                return;

            methodF = null;
            method = null;
        }
    }

    private static float runtimeClock, dt;
    private static readonly List<IEnumerator> flows = [];
    private static readonly Dictionary<string, Action> resumptions = new();
    private static readonly List<Timer> timers = [];
    private static readonly Stopwatch holdFrequency = new(), holdDelay = new();
    private static readonly Dictionary<string, bool> gates = new();
    private static readonly Dictionary<float, float> trueEvery = new();

    static Flow()
    {
        holdFrequency.Start();
        holdDelay.Start();
    }

    private static float Map(this float number, (float a, float b) rangeIn, (float a, float b) rangeOut)
    {
        if (Math.Abs(rangeIn.a - rangeIn.b) < 0.001f)
            return (rangeOut.a + rangeOut.b) / 2f;
        var target = rangeOut;
        var value = (number - rangeIn.a) / (rangeIn.b - rangeIn.a) * (target.b - target.a) + target.a;
        return float.IsNaN(value) || float.IsInfinity(value) ? rangeOut.a : value;
    }
#endregion
}