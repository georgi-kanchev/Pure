namespace Pure.Tracker;

/// <summary>
/// Tracks conditions and triggers methods associated with a unique identifier at certain times.
/// </summary>
/// <typeparam name="T">The type of the unique identifier.</typeparam>
public static class Tracker<T> where T : notnull
{
	/// <summary>
	/// Subscribes a <paramref name="method"/> to continuously get called
	/// when triggered by the unique identifier.
	/// </summary>
	/// <param name="uniqueID">The unique identifier associated with the <paramref name="method"/>.</param>
	/// <param name="method">The method to subscribe.</param>
	public static void While(T uniqueID, Action method)
	{
		Subscribe(uniqueID, method, continuous);
	}
	/// <summary>
	/// Subscribes a <paramref name="method"/> to get called
	/// once when triggered by the unique identifier.
	/// </summary>
	/// <param name="uniqueID">The unique identifier associated with the <paramref name="method"/>.</param>
	/// <param name="method">The method to subscribe.</param>
	public static void When(T uniqueID, Action method)
	{
		Subscribe(uniqueID, method, once);
	}

	/// <summary>
	/// Tracks the <paramref name="method"/> of the unique identifier and 
	/// triggers any subscribed methods accordingly.
	/// </summary>
	/// <param name="uniqueID">The unique identifier to track.</param>
	/// <param name="condition">The condition to track.</param>
	public static void Track(T uniqueID, bool condition)
	{
		var isContinuous = continuous.ContainsKey(uniqueID);
		var isOnce = once.ContainsKey(uniqueID);

		if (isContinuous == false && isOnce == false)
			return;

		if (isContinuous && condition)
			continuous[uniqueID].Trigger();

		var instance = once[uniqueID];
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
			for (int i = 0; i < methods.Count; i++)
				methods[i].Invoke();
		}
	}

	private static readonly Dictionary<T, Instance> continuous = new(), once = new();

	private static void Subscribe(T uniqueID, Action method, Dictionary<T, Instance> collection)
	{
		if (collection.ContainsKey(uniqueID) == false)
			collection[uniqueID] = new();

		collection[uniqueID].methods.Add(method);
	}
	#endregion
}
