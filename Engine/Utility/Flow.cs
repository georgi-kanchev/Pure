using System.Collections;

namespace Pure.Engine.Utility;

public static class Flow
{
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
            time += Time.Delta;
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

    #region Backend
    private static readonly List<IEnumerator> flows = [];
    private static readonly Dictionary<string, Action> resumptions = new();

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

    internal static void Update()
    {
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
    }
    #endregion
}