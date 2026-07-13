namespace SystemsProgramming.Common;

/// <summary>Reusable helpers for spinning up threads and surfacing concurrency bugs.</summary>
public static class Concurrency
{
    /// <summary>
    /// Run <paramref name="body"/> on <paramref name="threadCount"/> real OS threads
    /// (passing the worker index 0..n-1) and block until all have finished.
    /// </summary>
    public static void RunThreads(int threadCount, Action<int> body)
    {
        var threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int id = i; // capture a per-iteration copy, not the loop variable
            threads[i] = new Thread(() => body(id)) { Name = $"worker-{id}", IsBackground = false };
        }
        foreach (var t in threads) t.Start();
        foreach (var t in threads) t.Join();
    }

    /// <summary>
    /// Run a trial repeatedly to surface non-deterministic bugs.
    /// <paramref name="trial"/> returns <c>true</c> when the run was CORRECT and
    /// <c>false</c> when it exhibited the bug (e.g. a lost update). Reports how often the bug appeared.
    /// </summary>
    public static void SurfaceRaces(string label, int trials, Func<bool> trial)
    {
        int failures = 0;
        for (int i = 0; i < trials; i++)
            if (!trial()) failures++;

        int correct = trials - failures;
        Console.WriteLine(
            $"[{label}] {correct}/{trials} runs correct, "
          + $"{failures}/{trials} exhibited the race ({100.0 * failures / trials:F1}%).");
    }
}
