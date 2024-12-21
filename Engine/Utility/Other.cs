using System.Diagnostics;

namespace Pure.Engine.Utility;

public static class Other
{
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
        else if (gates.ContainsKey(uniqueId) == false && condition)
        {
            gates[uniqueId] = true;
            return true;
        }
        else
        {
            if (gates[uniqueId] && condition)
                return false;
            else if (gates[uniqueId] == false && condition)
            {
                gates[uniqueId] = true;
                return true;
            }
            else
                gates[uniqueId] = false;
        }

        return false;
    }
    public static void Once(this bool condition, Action method)
    {
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
    public static void PressAndHold(this bool condition, Action method, float delay = 0.5f, float frequency = 0.06f)
    {
        if (PressAndHold(condition, method.GetHashCode().ToString(), delay, frequency))
            method.Invoke();
    }

    public static bool IsOneOf<T>(this T value, params T[] values)
    {
        return value.IsAnyOf(values);
    }
    public static bool IsAnyOf<T>(this T value, params T[] values)
    {
        for (var i = 0; i < values?.Length; i++)
            if (EqualityComparer<T>.Default.Equals(value, values[i]))
                return true;

        return false;
    }
    public static bool IsNoneOf<T>(this T value, params T[] values)
    {
        return value.IsAnyOf(values) == false;
    }
    public static bool IsAllOf<T>(this T value, params T[] values)
    {
        for (var i = 0; i < values?.Length; i++)
            if (EqualityComparer<T>.Default.Equals(value, values[i]) == false)
                return false;

        return true;
    }

#region Backend
    private static readonly Stopwatch holdFrequency = new(), holdDelay = new();
    private static readonly Dictionary<string, bool> gates = new();

    static Other()
    {
        holdFrequency.Start();
        holdDelay.Start();
    }
#endregion
}