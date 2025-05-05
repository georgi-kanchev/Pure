using System.Collections;
using Pure.Engine.Execution;
using Pure.Engine.Hardware;
using Pure.Engine.Utility;
using Pure.Engine.Window;

namespace Pure.Examples.Systems;

public static class Flows
{
	public static void Run()
	{
		var window = new Window { Title = "Pure - Flows Example" };
		var hardware = new Hardware(window.Handle);
		while (window.KeepOpen())
		{
			Time.Update();
			Flow.Update(Time.Delta);

			if (Flow.TrueEvery(1f))
				Console.WriteLine(Time.RuntimeClock);
			if (hardware.Keyboard.IsJustPressed(Keyboard.Key.A))
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