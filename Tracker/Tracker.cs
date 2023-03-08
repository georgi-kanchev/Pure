namespace Pure.Tracker;

public static class Tracker<T> where T : notnull
{
	public static void While(T uniqueID, Action method)
	{
		Subscribe(uniqueID, method, continuous);
	}
	public static void When(T uniqueID, Action method)
	{
		Subscribe(uniqueID, method, once);
	}

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
