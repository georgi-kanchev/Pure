namespace Pure.Engine.Window;

using Raylib_cs;

public static class Monitor
{
    public static int Count
    {
        get;
    }
    public static string Name
    {
        get;
    }
    public static int RefreshRate
    {
        get;
    }
    public static (int width, int height) Size
    {
        get;
    }
    public static (int x, int y) Position
    {
        get;
    }
    public static (int width, int height) AspectRatio
    {
        get => GetAspectRatio(Size.width, Size.height);
    }

#region Backend
    internal static int current;

    static Monitor()
    {
        Raylib.SetTraceLogLevel(TraceLogLevel.LOG_NONE);
        Raylib.InitWindow(1, 1, "");
        Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_HIDDEN);
        Raylib.SetWindowPosition(-1000, -1000);

        Count = Raylib.GetMonitorCount();
        current = Math.Min(current, Count - 1);

        var p = Raylib.GetMonitorPosition(current);
        Position = ((int)p.X, (int)p.Y);
        Size = (Raylib.GetMonitorWidth(current), Raylib.GetMonitorHeight(current));
        RefreshRate = Raylib.GetMonitorRefreshRate(current);
        Name = Raylib.GetMonitorName_(current);

        Raylib.CloseWindow();
    }

    internal static (int width, int height) GetAspectRatio(int width, int height)
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
    internal static (float width, float height) WindowToMonitorRatio
    {
        get
        {
            var (w, h) = Size;
            return ((float)w / Window.Size.width, (float)h / Window.Size.height);
        }
    }
#endregion
}