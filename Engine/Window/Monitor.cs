using System.Drawing;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Pure.Engine.Window;

public class Monitor
{
    public static Monitor[] Monitors { get; }
    public static Monitor Current
    {
        get => Monitors[current];
    }

    public string Name { get; private set; } = "";
    public (int width, int height) AspectRatio { get; private set; }
    public (int width, int height) Size { get; private set; }
    public bool IsPrimary { get; private set; }

    public override string ToString()
    {
        return Name;
    }

#region Backend
    internal static int current;

    internal (int x, int y) Position { get; private set; }

    static Monitor()
    {
        Monitors = Array.Empty<Monitor>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var data = new MonitorDetails.Reader().GetMonitorDetails();
            var monitors = new List<Monitor>();
            foreach (var monitor in data)
            {
                var m = new Monitor
                {
                    Size = (monitor.Resolution.Width, monitor.Resolution.Height),
                    Position = (monitor.Resolution.X, monitor.Resolution.Y),
                };
                m.AspectRatio = GetAspectRatio(m.Size.width, m.Size.height);
                m.Name = $"{monitor.Description} ({m.Size.width}x{m.Size.height} | " +
                         $"{m.AspectRatio.width}:{m.AspectRatio.height} | {monitor.Frequency}Hz)";

                monitors.Add(m);
            }

            current = Math.Min(current, monitors.Count - 1);
            Monitors = monitors.ToArray();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.FileName = "xrandr";
            process.StartInfo.Arguments = "--prop";
            process.Start();
            var result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var monitors = new List<Monitor>();
            var data = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            var currentMonitor = default(Monitor);
            foreach (var line in data)
                if (line.StartsWith('\t') == false &&
                    line.StartsWith(' ') == false &&
                    line.Contains("disconnected") == false &&
                    line.Contains("Screen") == false)
                {
                    if (currentMonitor != null)
                        monitors.Add(currentMonitor);

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
                        Size = (width, height),
                        Position = (x, y),
                        AspectRatio = GetAspectRatio(width, height),
                        Name = $"Monitor {monitors.Count}{(isPrimary ? " (Primary)" : "")}",
                        IsPrimary = isPrimary
                    };
                }

            if (currentMonitor != null)
                monitors.Add(currentMonitor);

            Monitors = monitors.ToArray();
        }
    }

    private static (int width, int height) GetAspectRatio(int width, int height)
    {
        var gcd = height == 0 ? width : GetGreatestCommonDivisor(height, width % height);

        return (width / gcd, height / gcd);

        int GetGreatestCommonDivisor(int a, int b)
        {
            while (true)
            {
                if (b == 0)
                    return a;
                var a1 = a;
                a = b;
                b = a1 % b;
            }
        }
    }
#endregion
}