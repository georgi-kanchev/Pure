namespace Pure.Engine.Window;

public class Monitor
{
    public static Monitor[] Monitors
    {
        get;
    }
    public static Monitor Current
    {
        get => Monitors[current];
    }
    
    public string Name
    {
        get;
        private set;
    }
    public (int width, int height) AspectRatio
    {
        get;
        private set;
    }
    public (int width, int height) Size
    {
        get;
        private init;
    }

    public override string ToString()
    {
        return Name;
    }

    #region Backend
    internal static int current;
    
    internal (int x, int y) Position
    {
        get;
        private set;
    }
    
    static Monitor()
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