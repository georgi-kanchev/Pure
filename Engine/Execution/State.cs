using System.Text;

namespace Pure.Engine.Execution;

using System.Text.RegularExpressions;

[Script.DoNotSave]
public class State
{
    public string Name { get; }
    public bool IsDisabled
    {
        get => isDisabled || (parent?.IsDisabled ?? false);
    }
    public bool IsRunning { get; internal set; }
    public bool IsParent
    {
        get => children.Count > 0;
    }
    public bool IsRoot
    {
        get => root == this;
    }

    public void Disable()
    {
        isDisabled = true;
    }
    public void Enable()
    {
        isDisabled = false;
    }

    public static implicit operator State(Action method)
    {
        return new(method);
    }
    public static implicit operator Action(State state)
    {
        return state.method;
    }

#region Backend
    internal readonly Action method;
    internal readonly List<State> children = [];
    private bool isDisabled;
    internal State? parent, root;

    internal State(Action method)
    {
        this.method = method;

        var regex = new Regex(@"(?:.*g__([^\|]*)\|.*|(.+))");
        var match = regex.Match(method.Method.Name);
        if (match.Success)
            Name = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
        else
            Name = method.Method.Name;

        Name = $"{method.Method.DeclaringType?.Name}.{Name}";
    }

    public override string ToString()
    {
        return $"{nameof(State)} '{Name}'";
    }

    internal State? FindState(Action method)
    {
        if (this.method == method)
            return this;

        foreach (var child in children)
        {
            if (child.method == method)
                return child;

            var result = child.FindState(method);
            if (result != null)
                return result;
        }

        return null;
    }
    internal string ToTree(string indent = "", bool isLast = true)
    {
        var result = new StringBuilder(indent);
        var disabled = root == this ? "║ " : " ";
        var enabled = isLast ? "╚═" : "╠═";

        result.Append(IsDisabled ? disabled : enabled);
        indent += isLast ? "  " : "║ ";
        result.AppendLine($"{Name}{(IsRunning ? "◄" : "")}");

        for (var i = 0; i < children.Count; i++)
            result.Append(children[i].ToTree(indent, i == children.Count - 1));

        return result.ToString();
    }
#endregion
}