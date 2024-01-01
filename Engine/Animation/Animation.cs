namespace Pure.Engine.Animation;

/// <summary>
/// Represents an animation that can iterate over a sequence of values of type 
/// <typeparamref name="T"/> over time.
/// </summary>
/// <typeparam name="T">The type of the values in the animation.</typeparam>
public class Animation<T>
    where T : notnull
{
    /// <summary>
    /// Gets the current value of the animation.
    /// </summary>
    public T CurrentValue
    {
        get => values[CurrentIndex];
    }
    /// <summary>
    /// Gets the current index of the animation.
    /// </summary>
    public int CurrentIndex
    {
        get => (int)MathF.Round(RawIndex);
    }
    /// <summary>
    /// Gets or sets the duration of the animation in seconds.
    /// </summary>
    public float Duration { get; set; }
    /// <summary>
    /// Gets or sets the speed of the animation.
    /// </summary>
    public float Speed
    {
        get => Duration / values.Length;
        set => Duration = value * values.Length;
    }
    /// <summary>
    /// Gets or sets a value indicating whether the animation should repeat.
    /// </summary>
    public bool IsLooping { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether the animation is paused.
    /// </summary>
    public bool IsPaused { get; set; }
    /// <summary>
    /// Gets or sets the current progress of the animation as a value between 0 and 1.
    /// </summary>
    public float CurrentProgress
    {
        get => Map(rawIndex, LOWER_BOUND, values.Length, 0, 1);
        set => rawIndex = Map(value, 0, 1, LOWER_BOUND, values.Length);
    }

    /// <summary>
    /// Gets or sets the value at the specified index.
    /// </summary>
    /// <param name="index">The index of the value to get or set.</param>
    /// <returns>The value at the specified index.</returns>
    public T this[int index]
    {
        get => values[index];
        set => values[index] = value;
    }

    /// <summary>
    /// Initializes a new instance of the animation with the specified duration, 
    /// repetition, and values.
    /// </summary>
    /// <param name="duration">The duration of the animation in seconds.</param>
    /// <param name="isLooping">A value indicating whether the animation should repeat from the beginning
    /// after it has finished playing through all the values.</param>
    /// <param name="values">The values of the animation.</param>
    public Animation(float duration, bool isLooping, params T[] values)
    {
        if (values == null)
            throw new ArgumentNullException(nameof(values));

        this.values = Copy(values);
        rawIndex = 0;
        Duration = duration;
        IsLooping = isLooping;
        RawIndex = LOWER_BOUND;
    }
    /// <summary>
    /// Initializes a new instance of the animation with the specified values, 
    /// repeating and speed properties set.
    /// </summary>
    /// <param name="isLooping">A value indicating whether the animation should repeat 
    /// from the beginning after it has finished playing through all the values.</param>
    /// <param name="speed">The speed at which the animation should play, as values
    /// per second.</param>
    /// <param name="values">The values to be animated.</param>
    public Animation(bool isLooping, float speed, params T[] values)
        : this(0f, isLooping, values)
    {
        Speed = speed;
    }
    /// <summary>
    /// Initializes a new instance of the animation with the specified values 
    /// and default properties of <code>Duration = 1f</code> and <code>IsRepeating = false</code>
    /// </summary>
    /// <param name="values">The values to be animated.</param>
    public Animation(params T[] values)
        : this(1f, false, values)
    {
    }

    /// <summary>
    /// Updates the animation progress based on the specified delta time.
    /// </summary>
    /// <param name="deltaTime">The amount of time that has passed since the last update.</param>
    public void Update(float deltaTime)
    {
        if (IsPaused)
            return;

        RawIndex += deltaTime / Speed;

        if ((int)MathF.Round(RawIndex) < values.Length)
            return;

        RawIndex = IsLooping ? LOWER_BOUND : values.Length - 1;

        if (IsLooping)
            onLoop?.Invoke();
        else
            onEnd?.Invoke();
    }

    /// <summary>
    /// Implicitly converts an array of values to an Animation object.
    /// </summary>
    /// <param name="values">The values to be animated.</param>
    public static implicit operator Animation<T>(T[] values)
    {
        return new(values);
    }
    /// <summary>
    /// Implicitly converts an Animation object to an array of values.
    /// </summary>
    /// <param name="animation">The Animation object to convert.</param>
    public static implicit operator T[](Animation<T> animation)
    {
        return Copy(animation.values);
    }

    /// <returns>
    /// An array copy containing the values in the animation sequence.</returns>
    public T[] ToArray()
    {
        return this;
    }

    public void OnEnd(Action method)
    {
        onEnd += method;
    }
    public void OnLoop(Action method)
    {
        onLoop += method;
    }

    #region Backend
    private Action? onEnd, onLoop;
    private readonly T[] values;

    private float rawIndex;
    private const float LOWER_BOUND = -0.499f;

    private float RawIndex
    {
        get => rawIndex;
        set => rawIndex = Math.Clamp(value, LOWER_BOUND, values.Length);
    }

    private static float Map(float number, float a1, float a2, float b1, float b2)
    {
        var value = (number - a1) / (a2 - a1) * (b2 - b1) + b1;
        return float.IsNaN(value) || float.IsInfinity(value) ? b1 : value;
    }
    private static T[] Copy(T[] array)
    {
        var copy = new T[array.Length];
        Array.Copy(array, copy, array.Length);
        return copy;
    }
    #endregion
}