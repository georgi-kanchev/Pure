namespace Pure.Act;

public static class Act<T> where T : notnull
{
	public static void While(T uniqueID, Action method)
	{
		Subscribe(uniqueID, method, true);
	}
	public static void When(T uniqueID, Action method)
	{
		Subscribe(uniqueID, method, false);
	}

	public static void Update(T uniqueID, bool condition)
	{
		if (actInstances.ContainsKey(uniqueID) == false)
			return;

		var act = actInstances[uniqueID];
		var prev = act.condition;
		act.condition = condition;

		if (act.isContinuous && condition)
			act.Trigger();

		else if (act.isContinuous == false && condition && prev == false)
			act.Trigger();
	}

	#region Backend
	private class ActInstance
	{
		public readonly List<Action> methods = new();
		public bool condition, isContinuous;

		public void Trigger()
		{
			for (int i = 0; i < methods.Count; i++)
				methods[i].Invoke();
		}
	}

	private static readonly Dictionary<T, ActInstance> actInstances = new();

	private static void Subscribe(T uniqueID, Action method, bool isContinuous)
	{
		if (actInstances.ContainsKey(uniqueID) == false)
			actInstances[uniqueID] = new();

		actInstances[uniqueID].methods.Add(method);
		actInstances[uniqueID].isContinuous = isContinuous;
	}
	#endregion
}
