global using SizeI = (int width, int height);
global using VecI = (int x, int y);
global using AreaI = (int x, int y, int width, int height);
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Pure.Engine.Hardware;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class)]
internal class DoNotSave : Attribute;

[DoNotSave]
public class Hardware
{
	public Monitor[] Monitors { get; set; } = [];
	public Mouse Mouse { get; set; }
	public Keyboard Keyboard { get; set; }

	public Hardware(IntPtr windowHandle)
	{
		Mouse = new(windowHandle);
		Keyboard = new(windowHandle);

		if (OperatingSystem.IsWindows())
			InitWindows();
		else if (OperatingSystem.IsLinux())
			InitLinux();
	}

	public void Update()
	{
		Mouse.Update();
		Keyboard.Update();
	}

#region Backend
	private void InitLinux()
	{
		var process = new Process();
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.FileName = "xrandr";
		process.StartInfo.Arguments = "--prop";
		process.Start();
		var result = process.StandardOutput.ReadToEnd();
		process.WaitForExit();

		var monitorList = new List<Monitor>();
		var data = result.Replace("\r", "").Split("\n", StringSplitOptions.RemoveEmptyEntries);
		var currentMonitor = default(Monitor);
		foreach (var line in data)
			if (line.StartsWith('\t') == false &&
			    line.StartsWith(' ') == false &&
			    line.Contains("disconnected") == false &&
			    line.Contains("Screen") == false)
			{
				if (currentMonitor != null)
					monitorList.Add(currentMonitor);

				var match = Regex.Match(line, @"\b(\d+)x(\d+)\+(\d+)\+(\d+)\b");
				if (match.Success == false)
					continue;

				var width = int.Parse(match.Groups[1].Value);
				var height = int.Parse(match.Groups[2].Value);
				var x = int.Parse(match.Groups[3].Value);
				var y = int.Parse(match.Groups[4].Value);
				var isPrimary = line.Contains("primary");

				currentMonitor = new()
				{
					DesktopArea = (x, y, width, height),
					Name = $"Monitor {monitorList.Count}{(isPrimary ? " (Primary)" : string.Empty)}",
					IsPrimary = isPrimary
				};
			}

		if (currentMonitor != null)
			monitorList.Add(currentMonitor);

		Monitors = monitorList.ToArray();
	}
	private void InitWindows()
	{
		var data = new MonitorDetails.Reader().GetMonitorDetails();
		var monitorList = new List<Monitor>();
		foreach (var monitor in data)
		{
			var m = new Monitor
			{
				DesktopArea = (monitor.Resolution.X, monitor.Resolution.Y,
					monitor.Resolution.Width, monitor.Resolution.Height)
			};
			m.Name = $"{monitor.Description} ({m.DesktopArea.width}x{m.DesktopArea.height} | " +
			         $"{m.AspectRatio.width}:{m.AspectRatio.height} | {monitor.Frequency}Hz)";

			monitorList.Add(m);
		}

		Monitors = monitorList.ToArray();
	}
#endregion
}