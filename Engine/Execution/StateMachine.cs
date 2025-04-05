namespace Pure.Engine.Execution;

[Script.DoNotSave]
public class StateMachine
{
    public enum Path { Running, RunningPrevious, Root, Parent, Child, First, Last, Next, Previous }

    public State? Current { get; private set; }

    public State? this[Action? method]
    {
        get
        {
            if (method == null)
                return null;

            foreach (var (_, state) in roots)
            {
                var result = state.FindState(method);
                if (result != null)
                    return result;
            }

            return null;
        }
    }

    public State? Get(Path[] path)
    {
        var result = Current;
        foreach (var target in path)
        {
            if (result == null)
                return Reset();

            var siblings = (result.IsRoot ? roots.Values.ToList() : result.parent?.children) ?? [];
            var index = siblings.IndexOf(result);

            switch (target)
            {
                case Path.Root:
                    result = result.root;
                    break;
                case Path.Parent:
                    result = result.parent;
                    break;
                case Path.RunningPrevious:
                    result = previous;
                    break;
                case Path.Next:
                {
                    if (index >= 0 && index < siblings.Count - 1)
                        result = siblings[index + 1];
                    break;
                }
                case Path.Previous:
                {
                    if (index >= 1 && index < siblings.Count)
                        result = siblings[index - 1];
                    break;
                }
                case Path.First:
                {
                    result = siblings[0];
                    break;
                }
                case Path.Last:
                {
                    result = siblings[^1];
                    break;
                }
                case Path.Child:
                {
                    if (result.children.Count > 0)
                        result = result.children[0];
                    break;
                }
                case Path.Running:
                default:
                    break;
            }
        }

        return result;
    }

    public void Add(Action? parentState, Action[]? states)
    {
        if (states == null)
            return;

        var match = this[parentState];
        if (match == null)
        {
            foreach (var state in states)
            {
                var st = new State(state);
                st.root = st;
                roots[$"{st}"] = st;
            }

            return;
        }

        foreach (var state in states)
            match.children.Add(new(state) { parent = match, root = match.root });
    }
    public void GoTo(Path[] path)
    {
        Current = Get(path);
    }
    public void GoTo(Action? state)
    {
        if (state == null)
        {
            Reset();
            return;
        }

        previous = Current;
        Current = this[state];

        if (previous != null)
            previous.IsRunning = false;

        if (Current != null)
            Current.IsRunning = true;
    }
    public void Update()
    {
        if (roots.Count == 0)
            return;

        if (Current == null)
            Reset();

        var disabled = Current?.IsDisabled ?? false;
        if (disabled == false)
            Current?.method.Invoke();
    }

    public string ToTree()
    {
        var result = $"{nameof(StateMachine)}\n";
        foreach (var (_, state) in roots)
            result += state.ToTree(isLast: state == roots.Last().Value);
        return result[..^1];
    }

#region Backend
    private readonly Dictionary<string, State> roots = new();
    private State? previous;

    private State Reset()
    {
        Current = roots.First().Value;
        return Current;
    }
#endregion
}