using System.Collections;
using Pure.Engine.Execution;
using Pure.Engine.Utility;
using Pure.Engine.Window;

namespace Pure.Examples.Systems;

public static class Flows
{
	public static void Run()
	{
		while (Window.KeepOpen())
		{
			Time.Update();
			Flow.Update(Time.Delta);

			if (Flow.TrueEvery(1f))
				Console.WriteLine(Time.RuntimeClock);
			if (Keyboard.Key.A.IsJustPressed())
			{
				Flow.Start(A());
				Flow.Start(B());
			}
		}
	}

	private static IEnumerator A()
	{
		Console.WriteLine("A: A started!");
		yield return Flow.WaitForDelay(3f);
		Console.WriteLine("A: A finished!");
	}
	private static IEnumerator B()
	{
		Console.WriteLine("B: B started!");
		yield return Flow.WaitForMethod(A());
		Console.WriteLine("B: B finished!");
	}
}