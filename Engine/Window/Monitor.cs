using System.Text.RegularExpressions;

namespace Pure.Engine.Window;

public class Monitor
{
    public static Monitor[] Monitors
    {
        get => monitors.ToArray();
    }
    public static Monitor Current
    {
        get => Monitors[Window.Monitor];
    }

    public string Name { get; private set; } = string.Empty;
    public (int width, int height) AspectRatio { get; private set; }
    public (int width, int height) Size { get; private set; }
    public bool IsPrimary { get; private set; }

    public override string ToString()
    {
        return Name;
    }

    #region Backend
    internal (int x, int y) position;
    private static readonly Monitor[] monitors;

    private Monitor()
    {
    }

    static Monitor()
    {
        monitors = [];

        if (OperatingSystem.IsWindows())
        {
            var data = new MonitorDetails.Reader().GetMonitorDetails();
            var monitors = new List<Monitor>();
            foreach (var monitor in data)
            {
                var m = new Monitor
                {
                    Size = (monitor.Resolution.Width, monitor.Resolution.Height),
                    position = (monitor.Resolution.X, monitor.Resolution.Y)
                };
                m.AspectRatio = GetAspectRatio(m.Size.width, m.Size.height);
                m.Name = $"{monitor.Description} ({m.Size.width}x{m.Size.height} | " +
                         $"{m.AspectRatio.width}:{m.AspectRatio.height} | {monitor.Frequency}Hz)";

                monitors.Add(m);
            }

            Monitor.monitors = monitors.ToArray();
        }
        else if (OperatingSystem.IsLinux())
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
            var data = result.Replace("\r", "").Split("\n", StringSplitOptions.RemoveEmptyEntries);
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
                        position = (x, y),
                        AspectRatio = GetAspectRatio(width, height),
                        Name = $"Monitor {monitors.Count}{(isPrimary ? " (Primary)" : string.Empty)}",
                        IsPrimary = isPrimary
                    };
                }

            if (currentMonitor != null)
                monitors.Add(currentMonitor);

            Monitor.monitors = monitors.ToArray();
        }
    }

    private static (int width, int height) GetAspectRatio(int width, int height)
    {
        var gcd = height == 0 ? width : GetGreatestCommonDivisor(height, width % height);
        return (width / gcd, height / gcd);
    }
    internal static int GetGreatestCommonDivisor(int a, int b)
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
    #endregion
}