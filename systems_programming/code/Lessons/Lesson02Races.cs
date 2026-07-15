using SystemsProgramming.Common;

namespace SystemsProgramming.Lessons;

/// <summary>
/// Lesson 02 — Shared state &amp; race conditions.
/// Makes the LOST UPDATE visible:
///   1) many threads do a plain read-modify-write (count = count + 1) on a SHARED counter;
///      the final total comes up SHORT on many runs, by a different amount each time
///      (non-determinism you can see) — that's the lost update from study/02, Part 2b.
///   2) the SAME workload, but the increment is done as a single ATOMIC operation, is
///      exact on every run — a preview of the "make the critical section atomic" fix.
/// The only difference between the two is whether the critical section is indivisible.
/// Pairs with study/02-shared-state-races.md, Part 7.
/// </summary>
public sealed class Lesson02Races : IDemo
{
    public string Name => "02-races";
    public string Description => "Watch a shared counter LOSE updates, then fix it with an atomic op.";

    public void Run(string[] args)
    {
        const int workers = 4;
        const int incrementsPerWorker = 100_000;
        const int trials = 20;
        int expected = workers * incrementsPerWorker;

        Console.WriteLine($"{workers} threads each increment a SHARED counter {incrementsPerWorker:N0} times.");
        Console.WriteLine($"Correct final total (by construction) = {expected:N0}.\n");

        // ---- 1) UNSYNCHRONISED: plain read-modify-write. Races expected. ----
        Console.WriteLine("(1) Unsynchronised  count = count + 1  (a 3-step read-modify-write):");
        Concurrency.SurfaceRaces("racy", trials, () =>
        {
            int count = 0; // shared across the worker threads below (captured closure field)
            Concurrency.RunThreads(workers, _ =>
            {
                for (int i = 0; i < incrementsPerWorker; i++)
                    count = count + 1; // NOT atomic: LOAD, ADD, STORE — preemptible between each
            });
            return count == expected; // true = correct; false = a lost update occurred
        });
        Console.WriteLine("    → final totals come up SHORT (updates lost), and vary run to run.\n");

        // ---- 2) ATOMIC: the identical workload, increment made indivisible. No races. ----
        Console.WriteLine("(2) Atomic increment (the whole read-modify-write is one indivisible step):");
        Concurrency.SurfaceRaces("atomic", trials, () =>
        {
            int count = 0;
            Concurrency.RunThreads(workers, _ =>
            {
                for (int i = 0; i < incrementsPerWorker; i++)
                    Interlocked.Increment(ref count); // one atomic op — cannot be split
            });
            return count == expected;
        });
        Console.WriteLine("    → exact every run. Same threads, same interleaving pressure;");
        Console.WriteLine("      the ONLY change is that the critical section became atomic.");
    }
}
