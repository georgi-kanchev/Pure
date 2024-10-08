﻿namespace Pure.Engine.Animation;

/// <summary>
/// Represents an animation that can iterate over a sequence of values of type 
/// <typeparamref name="T"/> over time.
/// </summary>
/// <typeparam name="T">The type of the values in the animation.</typeparam>
public class Animation<T> where T : notnull
{
    public List<T> Values { get; } = [];

    /// <summary>
    /// Gets the current value of the animation.
    /// </summary>
    public T CurrentValue
    {
        get => Values[CurrentIndex];
    }
    /// <summary>
    /// Gets the current index of the animation.
    /// </summary>
    public int CurrentIndex
    {
        get => (int)MathF.Round(RawIndex);
    }
    /// <summary>
    /// Gets or sets the current progress of the animation as a value between 0 and 1.
    /// </summary>
    public float CurrentProgress
    {
        get => Map(rawIndex, LOWER_BOUND, Values.Count, 0, 1);
        set => rawIndex = Map(value, 0, 1, LOWER_BOUND, Values.Count);
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
        get => Duration / Values.Count;
        set => Duration = value * Values.Count;
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
    /// Initializes a new instance of the animation with the specified duration, 
    /// repetition, and values.
    /// </summary>
    /// <param name="duration">The duration of the animation in seconds.</param>
    /// <param name="loop">A value indicating whether the animation should repeat from the beginning
    /// after it has finished playing through all the values.</param>
    /// <param name="values">The values of the animation.</param>
    public Animation(float duration, bool loop, params T[]? values)
    {
        if (values is { Length: > 0 })
            Values.AddRange(values);

        rawIndex = 0;
        Duration = duration;
        IsLooping = loop;
        RawIndex = LOWER_BOUND;
    }
    /// <summary>
    /// Initializes a new instance of the animation with the specified values, 
    /// repeating and speed properties set.
    /// </summary>
    /// <param name="loop">A value indicating whether the animation should repeat 
    /// from the beginning after it has finished playing through all the values.</param>
    /// <param name="speed">The speed at which the animation should play, as values
    /// per second.</param>
    /// <param name="values">The values to be animated.</param>
    public Animation(bool loop, float speed, params T[]? values) : this(0f, loop, values)
    {
        Speed = speed;
    }
    /// <summary>
    /// Initializes a new instance of the animation with the specified values 
    /// and default properties of <code>Duration = 1f</code> and <code>IsRepeating = false</code>
    /// </summary>
    /// <param name="values">The values to be animated.</param>
    public Animation(params T[]? values) : this(1f, false, values)
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

        if ((int)MathF.Round(RawIndex) < Values.Count)
            return;

        RawIndex = IsLooping ? LOWER_BOUND : Values.Count - 1;

        if (IsLooping)
            onLoop?.Invoke();
        else
            onEnd?.Invoke();
    }

    public void OnEnd(Action method)
    {
        onEnd += method;
    }
    public void OnLoop(Action method)
    {
        onLoop += method;
    }

    public static implicit operator Animation<T>(List<T> values)
    {
        return new(values.ToArray());
    }
    public static implicit operator Animation<T>(T[] values)
    {
        return new(values);
    }
    public static implicit operator T[](Animation<T> animation)
    {
        return animation.Values.ToArray();
    }
    public static implicit operator List<T>(Animation<T> animation)
    {
        return animation.Values.ToList();
    }

#region Backend
    private Action? onEnd, onLoop;

    private float rawIndex;
    private const float LOWER_BOUND = -0.499f;

    private float RawIndex
    {
        get => rawIndex;
        set => rawIndex = Math.Clamp(value, LOWER_BOUND, Values.Count);
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