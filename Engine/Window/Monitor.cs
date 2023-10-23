namespace Pure.Engine.Window;

using Raylib_cs;

internal static class Monitor
{
    internal static int current;
    internal static readonly List<(int x, int y, int w, int h)> posSizes = new();

    internal static void Initialize()
    {
        Raylib.SetTraceLogLevel(TraceLogLevel.LOG_NONE);
        Raylib.InitWindow(1, 1, "");
        Raylib.SetWindowState(ConfigFlags.FLAG_WINDOW_HIDDEN);
        Raylib.SetWindowPosition(-1000, -1000);

        var monitorCount = Raylib.GetMonitorCount();
        for (var i = 0; i < monitorCount; i++)
        {
            var p = Raylib.GetMonitorPosition(i);
            var w = Raylib.GetMonitorWidth(i);
            var h = Raylib.GetMonitorHeight(i);
            posSizes.Add(((int)p.X, (int)p.Y, w, h));
        }

        Raylib.CloseWindow();
    }

    internal static (int width, int height) GetAspectRatio(int width, int height)
    {
        var gcd = height == 0 ? width : GetGreatestCommonDivisor(height, width % height);

        return (width / gcd, height / gcd);

        int GetGreatestCommonDivisor(int a, int b)
        {
            return b == 0 ? a : GetGreatestCommonDivisor(b, a % b);
        }
    }
}