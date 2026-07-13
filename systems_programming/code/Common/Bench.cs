using System.Diagnostics;

namespace SystemsProgramming.Common;

/// <summary>Tiny timing helper for eyeballing throughput/latency differences between approaches.</summary>
public static class Bench
{
    /// <summary>Time <paramref name="action"/>, print the elapsed milliseconds, and return the duration.</summary>
    public static TimeSpan Time(string label, Action action)
    {
        var sw = Stopwatch.StartNew();
        action();
        sw.Stop();
        Console.WriteLine($"[{label}] {sw.Elapsed.TotalMilliseconds:F2} ms");
        return sw.Elapsed;
    }
}
