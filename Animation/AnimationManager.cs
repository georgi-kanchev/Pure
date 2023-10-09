namespace Pure.Animation;

/// <summary>
/// Stores and updates a collection of <see cref="Animation{T}"/> objects by a key of type 
/// <typeparamref name="TKey"/>.
/// </summary>
/// <typeparam name="TKey">The type of the keys used to index the animations.</typeparam>
/// <typeparam name="T">The type of the animations managed by the manager.</typeparam>
public class AnimationManager<TKey, T>
    where TKey : notnull
    where T : Animation<T>
{
    /// <summary>
    /// Gets an array of the keys used to index the animations in the manager.
    /// </summary>
    public TKey[] Keys
    {
        get;
        private set;
    } = Array.Empty<TKey>();

    /// <summary>
    /// Gets or sets the animation at the specified index.
    /// </summary>
    /// <param name="key">The key to use to access the animation.</param>
    /// <returns>The animation associated with the 
    /// specified  key.</returns>
    public T? this[TKey key]
    {
        get => animations.ContainsKey(key) ? animations[key] : default;
        set
        {
            if (value == null)
            {
                animations.Remove(key);
                keys.Remove(key);
                Keys = keys.ToArray();
                return;
            }

            if (keys.Contains(key) == false)
                keys.Add(key);

            animations[key] = value;
            Keys = keys.ToArray();
        }
    }

    /// <summary>
    /// Updates all the animations in the manager by the specified deltaTime.
    /// </summary>
    /// <param name="deltaTime">The time elapsed since the last update.</param>
    public void Update(float deltaTime)
    {
        foreach (var kvp in animations)
            kvp.Value.Update(deltaTime);
    }

#region Backend
    private readonly Dictionary<TKey, T> animations = new();
    private readonly List<TKey> keys = new();
#endregion
}