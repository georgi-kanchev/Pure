namespace Pure.Tracker;

/// <summary>
/// Tracks conditions and triggers methods associated with a unique identifier of type
/// <typeparamref name="T"/> at certain times according to the provided conditions.
/// </summary>
/// <typeparam name="T">The type of the unique identifier.</typeparam>
public static class Tracker<T> where T : notnull
{
    /// <summary>
    /// Subscribes a method to continuously get called
    /// when triggered by the unique identifier.
    /// </summary>
    /// <param name="uniqueId">The unique identifier associated with the method.</param>
    /// <param name="method">The method to subscribe.</param>
    public static void While(T uniqueId, Action method)
    {
        Subscribe(uniqueId, method, continuous);
    }
    /// <summary>
    /// Subscribes a method to get called
    /// once when triggered by the unique identifier.
    /// </summary>
    /// <param name="uniqueId">The unique identifier associated with the method.</param>
    /// <param name="method">The method to subscribe.</param>
    public static void When(T uniqueId, Action method)
    {
        Subscribe(uniqueId, method, once);
    }

    /// <summary>
    /// Associates a condition to a unique identifier and triggers any subscribed methods accordingly.
    /// </summary>
    /// <param name="uniqueId">The unique identifier of the methods.</param>
    /// <param name="condition">The condition to track.</param>
    public static void Track(T uniqueId, bool condition)
    {
        var isContinuous = continuous.ContainsKey(uniqueId);
        var isOnce = once.ContainsKey(uniqueId);

        if (isContinuous == false && isOnce == false)
            return;

        if (isContinuous && condition)
            continuous[uniqueId].Trigger();

        var instance = once[uniqueId];
        var prev = instance.condition;
        instance.condition = condition;

        if (isOnce && condition && prev == false)
            instance.Trigger();
    }

#region Backend
    private class Instance
    {
        public readonly List<Action> methods = new();
        public bool condition;

        public void Trigger()
        {
            foreach (var t in methods)
                t.Invoke();
        }
    }

    private static readonly Dictionary<T, Instance> continuous = new(), once = new();

    private static void Subscribe(T uniqueID, Action method, Dictionary<T, Instance> collection)
    {
        collection.TryAdd(uniqueID, new());

        collection[uniqueID].methods.Add(method);
    }
#endregion
}